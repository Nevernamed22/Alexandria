using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Alexandria.Misc;
using UnityEngine;

namespace Alexandria.ItemAPI
{
   [HarmonyPatch]
   public static class GenericItemAPIHooks
    {
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void InitHooks() { }

        class PreventGunjurerGrabBehaviour : MonoBehaviour { }

        [HarmonyPatch(typeof(WizardSpinShootBehavior), nameof(WizardSpinShootBehavior.OnTriggerCollision))]
        [HarmonyPrefix]
        private static bool WizardSpinShootBehaviorOnTriggerCollisionPatch(WizardSpinShootBehavior __instance, SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
        {
            if (collisionData != null && collisionData.OtherRigidbody && collisionData.OtherRigidbody.gameObject.GetComponent<PreventGunjurerGrabBehaviour>())
                return false;
            return true;
        }

        /// <summary>Prevent victory / death screen from displaying fake items (i.e., items suppressed from inventory)</summary>
        [HarmonyPatch(typeof(AmmonomiconPageRenderer), nameof(AmmonomiconPageRenderer.InitializeDeathPageRight))]
        [HarmonyILManipulator]
        private static void VictoryScreenFakeItemIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel passiveLoopEndLabel = cursor.DefineLabel();

            // Actually do the logic for suppressing the item if necessary
            if (!cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdarg(0), // this == AmmonomiconPageRenderer instance
                instr => instr.MatchLdloc((byte)6), // V_6 == playerController
                instr => instr.MatchLdfld<PlayerController>("passiveItems")
                ))
                return;
            cursor.Emit(OpCodes.Ldloc_S, (byte)12); // V_12 == m == iterator over passive items
            cursor.CallPrivate(typeof(GenericItemAPIHooks), nameof(ShouldSuppressItemFromVictoryScreen));
            cursor.Emit(OpCodes.Brtrue, passiveLoopEndLabel);
            // if we don't branch, repopulate the stack
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_S, (byte)6);
            cursor.Emit(OpCodes.Ldfld, typeof(PlayerController).GetField("passiveItems", BindingFlags.Instance | BindingFlags.Public));

            // Mark the beginning of the loop since the compiler puts it later in the IL code
            if (!cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc((byte)12), // V_12 == m == iterator over passive items
                instr => instr.MatchLdcI4(1) // increment iterator
                ))
                return;
            cursor.MarkLabel(passiveLoopEndLabel);
        }

        // need to pass in AmmonomiconPageRenderer because it's the first instruction at the top of the loop and IL gets very messy otherwise
        private static bool ShouldSuppressItemFromVictoryScreen(AmmonomiconPageRenderer renderer, List<PassiveItem> items, int index)
        {
            return items[index].encounterTrackable && items[index].encounterTrackable.SuppressInInventory;
        }
    }
}
