using Alexandria.Misc;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.EnemyAPI
{
    [HarmonyPatch]
    public class HealthHaverExt : MonoBehaviour
    {
        public static MethodInfo mpd_m = AccessTools.Method(typeof(HealthHaverExt), nameof(ModifyProjectileDamage_Modify));
        public static MethodInfo odc_t = AccessTools.Method(typeof(HealthHaverExt), nameof(OnDamagedContext_Trigger));

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.HandleDamage))]
        [HarmonyILManipulator]
        public static void ModifyProjectileDamage_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchStloc(4)))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Ldarg_0);
            crs.Emit(OpCodes.Call, mpd_m);
        }

        public static float ModifyProjectileDamage_Modify(float dmg, SpeculativeRigidbody rb, Projectile proj)
        {
            if (rb == null)
                return dmg;

            var hh = rb.healthHaver;

            if (hh == null)
                return dmg;

            var projExt = proj.Ext();
            if (projExt != null && projExt.ModifyDealtDamage != null)
            {
                var args = new HealthHaver.ModifyDamageEventArgs()
                {
                    InitialDamage = dmg,
                    ModifiedDamage = dmg
                };

                projExt.ModifyDealtDamage?.Invoke(proj, hh, args);
                dmg = args.ModifiedDamage;
            }

            var hhExt = hh.Ext();
            if (hhExt != null && hhExt.ModifyProjectileDamage != null)
            {
                var args = new HealthHaver.ModifyDamageEventArgs()
                {
                    InitialDamage = dmg,
                    ModifiedDamage = dmg
                };

                hhExt.ModifyProjectileDamage?.Invoke(hh, args);
                dmg = args.ModifiedDamage;
            }

            return dmg;
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.ApplyDamageDirectional))]
        [HarmonyILManipulator]
        public static void OnDamagedContext_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpBeforeNext(x => x.MatchLdfld<HealthHaver>(nameof(HealthHaver.OnHealthChanged))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Ldarg_3);
            crs.Emit(OpCodes.Ldarg_S, (byte)4);
            crs.Emit(OpCodes.Ldarg_S, (byte)5);
            crs.Emit(OpCodes.Ldarg_2);
            crs.Emit(OpCodes.Ldarg_S, (byte)6);
            crs.Emit(OpCodes.Ldarg_S, (byte)8);

            crs.Emit(OpCodes.Call, odc_t);
        }

        public static HealthHaver OnDamagedContext_Trigger(HealthHaver hh, float damage, string source, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection, bool ignoreInvulnerabilityFrames, bool ignoreDamageCaps)
        {
            var hhExt = hh.Ext();

            if (hhExt == null)
                return hh;

            hhExt.OnDamagedContext?.Invoke(hh, damage, source, hh.currentHealth, hh.AdjustedMaxHealth, damageTypes, damageCategory, damageDirection, ignoreInvulnerabilityFrames, ignoreDamageCaps);

            return hh;
        }

        public Action<HealthHaver, HealthHaver.ModifyDamageEventArgs> ModifyProjectileDamage;
        public OnDamagedContextDelegate OnDamagedContext;

        public delegate void OnDamagedContextDelegate(HealthHaver hh, float damage, string source, float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection, bool ignoreInvulnerabilityFrames, bool ignoreDamageCaps);
    }
}
