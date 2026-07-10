using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Alexandria.Misc
{

  /// <summary>Class for managing various OverrideableBools for use with common patches</summary>
  [HarmonyPatch]
  public static class PlayerOverrides
  {
    public static bool IsImmuneToExplosionDamage(this PlayerController player)
      => PlayerOverrideCache.Overrides(player).immuneToExplosionDamage.Value;
    public static void SetImmuneToExplosionDamage(this PlayerController player, bool value, string reason)
      => PlayerOverrideCache.Overrides(player).immuneToExplosionDamage.SetOverride(reason, value);
    public static bool IsImmuneToExplosionKnockback(this PlayerController player)
      => PlayerOverrideCache.Overrides(player).immuneToExplosionKnockback.Value;
    public static void SetImmuneToExplosionKnockback(this PlayerController player, bool value, string reason)
      => PlayerOverrideCache.Overrides(player).immuneToExplosionKnockback.SetOverride(reason, value);
    public static bool IsImmuneToContactDamage(this PlayerController player)
      => PlayerOverrideCache.Overrides(player).immuneToContactDamage.Value;
    public static void SetImmuneToContactDamage(this PlayerController player, bool value, string reason)
      => PlayerOverrideCache.Overrides(player).immuneToContactDamage.SetOverride(reason, value);
    public static bool IsInvulnerable(this PlayerController player)
      => !player.healthHaver.IsVulnerable; // NOTE: handles the override case directly in the HealthHaverIsVulnerableOverridePatch patch
    public static void SetInvulnerable(this PlayerController player, bool value, string reason)
      => PlayerOverrideCache.Overrides(player).invulnerable.SetOverride(reason, value);

    private class PlayerOverrideCache
    {
      internal OverridableBool immuneToExplosionDamage    = new(false);
      internal OverridableBool immuneToExplosionKnockback = new(false);
      internal OverridableBool immuneToContactDamage      = new(false);
      internal OverridableBool invulnerable               = new(false);

      private static PlayerController _P1 = null;
      private static PlayerController _P2 = null;

      private static PlayerOverrideCache _P1Data = null;
      private static PlayerOverrideCache _P2Data = null;

      internal static PlayerOverrideCache Overrides(PlayerController player)
      {
        if (!player)
          return null;
        if (player.PlayerIDX == 0)
        {
          if (player != _P1)
          {
            _P1Data = new(); // new player instance == new set of overrides
            _P1 = player;
          }
          return _P1Data;
        }
        if (player.PlayerIDX == 1)
        {
          if (player != _P2)
          {
            _P2Data = new(); // new player instance == new set of overrides
            _P2 = player;
          }
          return _P2Data;
        }
        return null; // can't ever happen in theory
      }
    }

    /// <summary>Patch to prevent damage / knockback from explosions.</summary>
    [HarmonyPatch(typeof(Exploder), nameof(Exploder.HandleExplosion), MethodType.Enumerator)]
    [HarmonyILManipulator]
    private static void IgnoreExplosionDamageAndKnockbackIL(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        // Ignore all damage from explosions
        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<PlayerController>("IsEthereal")))
            return;
        cursor.Emit(OpCodes.Ldloc_S, (byte)13); // V_13 == the PlayerController
        cursor.CallPrivate(typeof(PlayerOverrides), nameof(CheckImmuneToExplosionDamage));
        cursor.CallPrivate(typeof(ILTools), nameof(ILTools.Or));

        // Ignore all knockback from explosions
        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<ExplosionData>("preventPlayerForce")))
            return;
        cursor.Emit(OpCodes.Ldloc_S, (byte)13); // V_13 == the PlayerController
        cursor.CallPrivate(typeof(PlayerOverrides), nameof(CheckImmuneToExplosionKnockback));
        cursor.CallPrivate(typeof(ILTools), nameof(ILTools.Or));
    }

    private static bool CheckImmuneToExplosionDamage(PlayerController player) => player && player.IsImmuneToExplosionDamage();
    private static bool CheckImmuneToExplosionKnockback(PlayerController player) => player && player.IsImmuneToExplosionKnockback();

    /// <summary>Patch to check if a player's vulnerable state has been overridden.</summary>
    [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.IsVulnerable), MethodType.Getter)]
    [HarmonyPostfix]
    private static void HealthHaverIsVulnerableOverridePatch(HealthHaver __instance, ref bool __result)
    {
        if (!__instance.isPlayerCharacter || __instance.gameActor is not PlayerController player)
          return;
        if (PlayerOverrideCache.Overrides(player).invulnerable.Value)
          __result = false;  // if we have override invulnerability, IsVulnerable should return false
    }

    /// <summary>Patch to check if a player's contact damage immunity has been overridden.</summary>
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ReceivesTouchDamage), MethodType.Getter)]
    [HarmonyPostfix]
    private static void PlayerControllerReceivesTouchDamagePatch(PlayerController __instance, ref bool __result)
    {
        if (PlayerOverrideCache.Overrides(__instance).immuneToContactDamage.Value)
          __result = false; // change the original result`
    }
  }

}

