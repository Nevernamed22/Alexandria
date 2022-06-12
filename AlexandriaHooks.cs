using Alexandria.ItemAPI;
using Alexandria.Misc;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria
{
    public static class AlexandriaHooks
    {
        public static void Init()
        {
            var ChangeToRandomGunHook = new Hook(
                    typeof(PlayerController).GetMethod("ChangeToRandomGun", BindingFlags.Instance | BindingFlags.Public),
                    typeof(AlexandriaHooks).GetMethod("ChangeToRandomGunHook", BindingFlags.Static | BindingFlags.Public));

        }

        public static void ChangeToRandomGunHook(Action<PlayerController> orig, PlayerController self)
        {
            orig(self);
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
            {
                return;
            }

            var currentGun = self.CurrentGun;

            if (currentGun && currentGun.HasTag("exclude_blessed"))
            {
                self.ChangeToRandomGun();
            }
        }
    }
}