using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
   public static class GenericItemAPIHooks
    {
        public static void InitHooks()
        {
            GunjurerCatchHook = new Hook(
                typeof(WizardSpinShootBehavior).GetMethod("OnTriggerCollision", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(GenericItemAPIHooks).GetMethod("GunjurerPreCatch", BindingFlags.Static | BindingFlags.Public)
            );
        }
        public static void GunjurerPreCatch(Action<WizardSpinShootBehavior, SpeculativeRigidbody, SpeculativeRigidbody, CollisionData> orig, WizardSpinShootBehavior self, SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
        {
            if (!(collisionData != null && collisionData.OtherRigidbody != null && collisionData.OtherRigidbody.gameObject != null && collisionData.OtherRigidbody.gameObject.GetComponent<PreventGunjurerGrabBehaviour>() != null))
            {
                orig(self, specRigidbody, sourceSpecRigidbody, collisionData);
            }
        }
        private static Hook GunjurerCatchHook;

        /// <summary>Prevent victory / death screen from displaying fake items (i.e., items suppressed from inventory)</summary>
        [HarmonyPatch]
        private static class SuppressFakeItemOnVictoryScreenPatch
        {
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
                cursor.CallPrivate(typeof(SuppressFakeItemOnVictoryScreenPatch), nameof(ShouldSuppressItemFromVictoryScreen));
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
}
