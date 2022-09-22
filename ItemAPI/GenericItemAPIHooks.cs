using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria.ItemAPI
{
   public static class GenericItemAPIHooks
    {
        public static void InitHooks()
        {
            GunjurerCatchHook = new Hook(
                typeof(WizardSpinShootBehavior).GetMethod("OnTriggerCollision", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(GenericItemAPIHooks).GetMethod("GunjurerPreCatch", BindingFlags.Static | BindingFlags.NonPublic)
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
    }
}
