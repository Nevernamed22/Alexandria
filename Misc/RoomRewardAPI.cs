using Alexandria.Misc;
using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Alexandria
{
    [HarmonyPatch]
    public static class RoomRewardAPI
    {
        public static bool GuaranteeRoomClearRewardDrop = false;
        public static Action<DebrisObject, RoomHandler> OnRoomClearItemDrop;
        public static Action<RoomHandler, ValidRoomRewardContents, float> OnRoomRewardDetermineContents;
        public class ValidRoomRewardContents : EventArgs
        {
            public List<Action<Vector3, RoomHandler>> overrideFunctionPool = new List<Action<Vector3, RoomHandler>>();
            public List<Tuple<float, int>> overrideItemPool = new List<Tuple<float, int>>();
            public float additionalRewardChance = 0;
        }

        //Thank you to June for submitting the original hook!
        [HarmonyPatch(typeof(RoomHandler), nameof(RoomHandler.HandleRoomClearReward))]
        [HarmonyILManipulator]
        private static void RoomHandlerHandleRoomClearRewardIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("BraveUtility", nameof(BraveUtility.Log))))
                return;

            VariableDefinition contentsSet = il.DeclareLocal<ValidRoomRewardContents>();
            ILLabel dontSkipRestOfFunctionLabel = cursor.DefineLabel();
            ILLabel overrideItemPoolLabel = cursor.DefineLabel();

            // call and handle OnRoomRewardDetermineContents if available
            cursor.Emit(OpCodes.Ldarg_0); // load the RoomHandler
            cursor.Emit(OpCodes.Ldloca, contentsSet); // load our (uninitialized) ValidRoomRewardContents
            cursor.Emit(OpCodes.Ldloca_S, (byte)4); // 4 == RNG roll at beginning of function
            cursor.CallPrivate(typeof(RoomRewardAPI), nameof(HandleValidRoomRewardContents));
            cursor.Emit(OpCodes.Brfalse, dontSkipRestOfFunctionLabel);
            cursor.Emit(OpCodes.Ret); // early return if ValidRoomRewardContents has an overrideFunctionPool
            cursor.MarkLabel(dontSkipRestOfFunctionLabel);

            // shenanigans to navigate after the branch point
            if (!cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchStloc((byte)13), // 13 == reward gameobject
                instr => instr.MatchLdloc((byte)13)))
                return;
            if (!cursor.TryGotoNext(MoveType.AfterLabel,
                instr => instr.MatchLdloc((byte)13)))
                return;
            cursor.MarkLabel(overrideItemPoolLabel);

            // handle contentsSet.overrideItemPool.Count > 0 case
            cursor.Index = 0;
            if (!cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdnull(),
                instr => instr.MatchStloc((byte)13)))
                return;
            cursor.Emit(OpCodes.Ldloca, contentsSet);
            cursor.Emit(OpCodes.Ldloca_S, (byte)13);
            cursor.CallPrivate(typeof(RoomRewardAPI), nameof(SelectOverrideContents));
            cursor.Emit(OpCodes.Brtrue, overrideItemPoolLabel);

            // call and handle OnRoomClearItemDrop
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc((byte)15)))
                return;
            cursor.Emit(OpCodes.Ldloc_S, (byte)15); // reward DebrisObject
            cursor.Emit(OpCodes.Ldarg_0); // the RoomHandler
            cursor.CallPrivate(typeof(RoomRewardAPI), nameof(HandleOnRoomClearItemDrop));

            return;
        }

        /// <summary>Returns true if and only if we should return early from HandleRoomClearReward</summary>
        private static bool HandleValidRoomRewardContents(RoomHandler room, ref ValidRoomRewardContents contentsSet, ref float rngValue)
        {
            contentsSet = new ValidRoomRewardContents(); // this is passed in uninitialized, so initialize it now
            if (OnRoomRewardDetermineContents != null)
            {
                OnRoomRewardDetermineContents(room, contentsSet, rngValue);
                if (contentsSet.overrideFunctionPool.Count > 0)
                {
                    Vector3 pos = room.GetBestRewardLocation(new IntVector2(1, 1), RoomHandler.RewardLocationStyle.CameraCenter, true).ToVector3() + new Vector3(0.25f, 0f, 0f);
                    foreach (var item in contentsSet.overrideFunctionPool)
                        item?.Invoke(pos, room);
                    return true; // should return early from HandleRoomClearReward
                }
                rngValue -= contentsSet.additionalRewardChance;
            }
            if (GuaranteeRoomClearRewardDrop)
                rngValue = -99999f; // something absurdly small
            return false;
        }

        /// <summary>Returns true if and only if we actually overrode contents and should skip over the other reward assignments</summary>
        private static bool SelectOverrideContents(ref ValidRoomRewardContents contentsSet, ref GameObject reward)
        {
            if (contentsSet.overrideItemPool.Count == 0)
                return false;
            GenericLootTable onTheFlyLootTable = LootUtility.CreateLootTable();
            foreach (Tuple<float, int> entry in contentsSet.overrideItemPool)
                onTheFlyLootTable.AddItemToPool(entry.Second, entry.First);
            reward = onTheFlyLootTable.defaultItemDrops.SelectByWeight();
            return true;
        }

        private static void HandleOnRoomClearItemDrop(DebrisObject debrisObject, RoomHandler room)
        {
            OnRoomClearItemDrop?.Invoke(debrisObject, room);
        }
    }
}
