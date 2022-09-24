using Alexandria.ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public class ExtendedPlayerComponent : MonoBehaviour
    {
        #region InitAndHooks
        public static void Init()
        {
            playerStartHook = new Hook(
                typeof(PlayerController).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
                typeof(ExtendedPlayerComponent).GetMethod("DoSetup"));
            activeItemDropHook = new Hook(
                typeof(PlayerController).GetMethod("DropActiveItem", BindingFlags.Public | BindingFlags.Instance),
                typeof(ExtendedPlayerComponent).GetMethod("DropActiveHook", BindingFlags.Public | BindingFlags.Instance)
            );
        }
        public static void DoSetup(Action<PlayerController> action, PlayerController player)
        {
            action(player);
            if (player.GetComponent<ExtendedPlayerComponent>() == null) player.gameObject.AddComponent<ExtendedPlayerComponent>();
        }
        public DebrisObject DropActiveHook(Func<PlayerController, PlayerItem, float, bool, DebrisObject> orig, PlayerController self, PlayerItem item, float force = 4f, bool deathdrop = false)
        {
            if (OnActiveItemPreDrop != null) OnActiveItemPreDrop(self, item, deathdrop);
            return orig(self, item, force, deathdrop);
        }
        private static Hook playerStartHook;
        private static Hook activeItemDropHook;
        #endregion

        public PlayerController attachedPlayer;
        private void Start()
        {
            attachedPlayer = base.GetComponent<PlayerController>();
            if (attachedPlayer != null)
            {
                if (CustomActions.OnNewPlayercontrollerSpawned != null) CustomActions.OnNewPlayercontrollerSpawned(attachedPlayer);
            }
        }
        #region Actions
        //Slash Related
        public Action<PlayerController, Vector2, SlashData> PreProcessSlash;
        public Action<PlayerController, Vector2, SlashData> PostProcessSlash;
        public Action<PlayerController, Vector2, SlashData, AIActor> OnSlashHitEnemy;
        //Other
        public Action<PlayerController> OnBlessedGunChanged;
        public Action<PlayerController, PlayerItem, bool> OnActiveItemPreDrop;
        #endregion
        public void Enrage(float dur)
        {
            if (remainingRageTime > 0) { remainingRageTime += dur; }
            else attachedPlayer.StartCoroutine(HandleRageDur(dur));
        }
        private IEnumerator HandleRageDur(float dur)
        {
            remainingRageTime = dur;
            attachedPlayer.stats.RecalculateStats(attachedPlayer, true, false);
            this.instanceRageVFX = attachedPlayer.PlayEffectOnActor(RageVFX, new Vector3(0f, 1.375f, 0f), true, true, false);
            attachedPlayer.ownerlessStatModifiers.Add(DoubleDamageStatMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer, true, false);

            float elapsed = 0f;
            float particleCounter = 0f;
            while (elapsed < remainingRageTime)
            {
                elapsed += BraveTime.DeltaTime;
                attachedPlayer.baseFlatColorOverride = this.flatRageColourOverride.WithAlpha(Mathf.Lerp(this.flatRageColourOverride.a, 0f, Mathf.Clamp01(elapsed - (remainingRageTime - 1f))));
                if (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.LOW && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && attachedPlayer && attachedPlayer.IsVisible && !attachedPlayer.IsFalling)
                {
                    particleCounter += BraveTime.DeltaTime * 40f;
                    if (this.instanceRageVFX && elapsed > 1f)
                    {
                        this.instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
                        this.instanceRageVFX = null;
                    }
                    if (particleCounter > 1f)
                    {
                        int num = Mathf.FloorToInt(particleCounter);
                        particleCounter %= 1f;
                        GlobalSparksDoer.DoRandomParticleBurst(num, attachedPlayer.sprite.WorldBottomLeft.ToVector3ZisY(0f), attachedPlayer.sprite.WorldTopRight.ToVector3ZisY(0f), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
                    }
                }
                yield return null;
            }
            if (this.instanceRageVFX) this.instanceRageVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out", null);
            attachedPlayer.ownerlessStatModifiers.Remove(DoubleDamageStatMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer, true, false);
            remainingRageTime = 0;
        }
        private float remainingRageTime;
        private static GameObject RageVFX = PickupObjectDatabase.GetById(353).GetComponent<RagePassiveItem>().OverheadVFX.gameObject;
        private GameObject instanceRageVFX;
        private static StatModifier DoubleDamageStatMod;
        private Color flatRageColourOverride = new Color(0.5f, 0f, 0f, 0.75f);

        public void DoTimedStatModifier(PlayerStats.StatType statToBoost, float amount, float time, StatModifier.ModifyMethod modifyMethod = StatModifier.ModifyMethod.MULTIPLICATIVE)
        {
            attachedPlayer.StartCoroutine(HandleTimedStatModifier(statToBoost, amount, time, modifyMethod));
        }
        private IEnumerator HandleTimedStatModifier(PlayerStats.StatType statToBoost, float amount, float dur, StatModifier.ModifyMethod method)
        {
            StatModifier timedMod = new StatModifier()
            {
                amount = amount,
                statToBoost = statToBoost,
                modifyType = method,
            };
            attachedPlayer.ownerlessStatModifiers.Add(timedMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer);
            yield return new WaitForSeconds(dur);
            attachedPlayer.ownerlessStatModifiers.Remove(timedMod);
            attachedPlayer.stats.RecalculateStats(attachedPlayer);
            yield break;
        }
    }
}
