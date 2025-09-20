using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace Alexandria.EnemyAPI
{
    [HarmonyPatch]
    public static class Hooks
    {
        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void Init() { }

        [HarmonyPatch(typeof(AIActor), nameof(AIActor.Awake))]
        [HarmonyPostfix]
        private static void AIActorAwakePatch(AIActor __instance)
        {
            if (EnemyTools.overrideBehaviors == null)
                return;
            try
            {
                var obehaviors = EnemyTools.overrideBehaviors.Where(ob => ob.OverrideAIActorGUID == __instance.EnemyGuid);
                foreach (var obehavior in obehaviors)
                {
                    obehavior.SetupOB(__instance);
                    if (obehavior.ShouldOverride())
                    {
                        obehavior.DoOverride();
                    }
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
        }
    }
}
