using UnityEngine;
using HarmonyLib;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Alexandria.Misc
{
    public abstract class CustomAmmoDisplay : MonoBehaviour
    {
        public abstract bool DoCustomAmmoDisplay(GameUIAmmoController uic);  // must return true if doing a custom ammo display, or false to revert to vanilla behavior

        private static bool DoAmmoOverride(GameUIAmmoController uic, GunInventory guns)
        {
            if (guns == null || !guns.CurrentGun || guns.CurrentGun.GetComponent<CustomAmmoDisplay>() is not CustomAmmoDisplay ammoDisplay)
              return false; // no custom ammo override, so use the vanilla behavior

            uic.SetAmmoCountLabelColor(Color.white); // reset base color (can be changed in DoCustomAmmoDisplay() with SetAmmoCountLabelColor() or with markup)

            if (!ammoDisplay.DoCustomAmmoDisplay(uic))
              return false; // custom ammo override does not want to change vanilla behavior

            // we're definitely using the custom ammo display, so prepare it
            uic.GunAmmoCountLabel.AutoSize = true; // enable dynamic width
            uic.GunAmmoCountLabel.AutoHeight = true; // enable multiline text
            uic.GunAmmoCountLabel.ProcessMarkup = true; // enable multicolor text

            if (uic.m_cachedGun && uic.m_cachedGun.GetComponent<CustomAmmoDisplay>())
              return true; // our custom ammo overrides already account for positioning weirdness, so don't adjust if our last gun had an override
                           // NOTE: without this, ammo displays for guns that toggle infinite ammo (e.g., with magazine rack) shift slowly offscreen

            // Need to do some vanilla postprocessing to make sure label alignment doesn't get all screwed up
            Gun currentGun = guns.CurrentGun;
            if (currentGun.IsUndertaleGun)
            {
              if (!uic.IsLeftAligned && uic.m_cachedMaxAmmo == int.MaxValue)
                uic.GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
            }
            else if (currentGun.InfiniteAmmo)
            {
              if (!uic.IsLeftAligned && (!uic.m_cachedGun || !uic.m_cachedGun.InfiniteAmmo))
                uic.GunAmmoCountLabel.RelativePosition += new Vector3(-3f, 0f, 0f);
            }
            else if (currentGun.AdjustedMaxAmmo > 0)
            {
              if (!uic.IsLeftAligned && uic.m_cachedMaxAmmo == int.MaxValue)
                uic.GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
            }
            else
            {
              if (!uic.IsLeftAligned && uic.m_cachedMaxAmmo == int.MaxValue)
                uic.GunAmmoCountLabel.RelativePosition += new Vector3(3f, 0f, 0f);
            }

            return true; // ammo was overridden, so skip remaining vanilla updates
        }

        [HarmonyPatch(typeof(GameUIAmmoController), nameof(GameUIAmmoController.UpdateUIGun))]
        private class CustomAmmoDisplayPatch
        {
            [HarmonyILManipulator]
            private static void CustomAmmoCountDisplayIL(ILContext il)
            {
                ILCursor cursor = new ILCursor(il);

                if (!cursor.TryGotoNext(MoveType.Before,
                  instr => instr.MatchLdarg(0),
                  instr => instr.MatchLdloc(2),
                  instr => instr.MatchCall<GameUIAmmoController>("CleanupLists")))
                    return; // failed to find what we need

                ILLabel skipPoint = cursor.MarkLabel(); // mark our own label right before the call to CleanupLists(), which is at the end of the conditional we want to skip
                cursor.Index = 0;

                if (!cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld<GameUIAmmoController>("m_cachedTotalAmmo")))
                    return; // failed to find what we need

                cursor.Emit(OpCodes.Ldarg_1); // 1st parameter is GunInventory
                cursor.Emit(OpCodes.Call, typeof(CustomAmmoDisplay).GetMethod(
                  nameof(CustomAmmoDisplay.DoAmmoOverride), BindingFlags.Static | BindingFlags.NonPublic)); // replace with our own custom hook
                cursor.Emit(OpCodes.Brtrue, skipPoint); // skip over all the logic for doing normal updates to the ammo counter
                cursor.Emit(OpCodes.Ldarg_0); // replace 0th parameter -> GameUIAmmoController
            }
        }
    }

    public static class CustomAmmoDisplayExtensions
    {
      /// <summary>Helper function to print out vanilla ammo display for a player</summary>
      public static string VanillaAmmoDisplay(this PlayerController player)
      {
        if (player.CurrentGun is not Gun gun) return string.Empty;
        if (gun.IsUndertaleGun)               return "0/0";
        if (gun.InfiniteAmmo)                 return "[sprite \"infinite-big\"]";
        if (gun.AdjustedMaxAmmo > 0)          return gun.CurrentAmmo + "/" + gun.AdjustedMaxAmmo;
                                              return gun.CurrentAmmo.ToString();
      }
    }

}
