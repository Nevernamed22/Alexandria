using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class GoopUtility
    {
        private static GoopDefinition LoadIndividualGoop(string text)
        {
            GoopDefinition goopDefinition;
            try
            {
                GameObject gameObject = LoadHelper.LoadAssetFromAnywhere(text) as GameObject;
                goopDefinition = gameObject.GetComponent<GoopDefinition>();
            }
            catch { goopDefinition = LoadHelper.LoadAssetFromAnywhere(text) as GoopDefinition; }
            goopDefinition.name = text.Replace("assets/data/goops/", "").Replace(".asset", "");
            return goopDefinition;
        }
        public static void Init()
        {
            FireDef = LoadIndividualGoop("assets/data/goops/napalmgoopthatworks.asset");
            OilDef = LoadIndividualGoop("assets/data/goops/oil goop.asset");
            PoisonDef = LoadIndividualGoop("assets/data/goops/poison goop.asset");
            BlobulonGoopDef = LoadIndividualGoop("assets/data/goops/blobulongoop.asset");
            WebDef = LoadIndividualGoop("assets/data/goops/phasewebgoop.asset");
            WaterDef = LoadIndividualGoop("assets/data/goops/water goop.asset");
            EternalFireDef = LoadIndividualGoop("assets/data/goops/eternal fire.asset");
            NapalmDef = LoadIndividualGoop("assets/data/goops/napalm goop.asset");
            QuickIgniteNapalmDef = LoadIndividualGoop("assets/data/goops/napalmgoopquickignite.asset");

            new Hook(typeof(DeadlyDeadlyGoopManager).GetMethod("DoGoopEffect", BindingFlags.Instance | BindingFlags.Public), typeof(GoopUtility).GetMethod("DoGoopEffectUpdateHook", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(DeadlyDeadlyGoopManager).GetMethod("InitialGoopEffect", BindingFlags.Instance | BindingFlags.Public), typeof(GoopUtility).GetMethod("ActorEnteredGoopHook", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(DeadlyDeadlyGoopManager).GetMethod("EndGoopEffect", BindingFlags.Instance | BindingFlags.Public), typeof(GoopUtility).GetMethod("ActorLeftGoopHook", BindingFlags.Static | BindingFlags.NonPublic));
            new Hook(typeof(DeadlyDeadlyGoopManager).GetMethod("GetGoopManagerForGoopType", BindingFlags.Static | BindingFlags.Public), typeof(GoopUtility).GetMethod("GoopManagerForTypeHook", BindingFlags.Static | BindingFlags.NonPublic));

            specialGoopComps = new Dictionary<string, Type>();
        }
        public static GoopDefinition Clone(this GoopDefinition toClone)
        {
            GoopDefinition newGoo = ScriptableObject.CreateInstance<GoopDefinition>();

            //Status Effects
            newGoo.CanBeIgnited = toClone.CanBeIgnited;
            newGoo.igniteSpreadTime = toClone.igniteSpreadTime;
            newGoo.SelfIgnites = toClone.SelfIgnites;
            newGoo.selfIgniteDelay = toClone.selfIgniteDelay;
            newGoo.ignitionChangesLifetime = toClone.ignitionChangesLifetime;
            newGoo.ignitedLifetime = toClone.ignitedLifetime;
            newGoo.CanBeElectrified = toClone.CanBeElectrified;
            newGoo.electrifiedTime = toClone.electrifiedTime;
            newGoo.fireDamageToPlayer = toClone.fireDamageToPlayer;
            newGoo.fireDamagePerSecondToEnemies = toClone.fireDamagePerSecondToEnemies;
            newGoo.fireBurnsEnemies = toClone.fireBurnsEnemies;
            newGoo.fireEffect = toClone.fireEffect;
            newGoo.UsesGreenFire = toClone.UsesGreenFire;
            //Misc
            newGoo.AppliesCharm = toClone.AppliesCharm;
            newGoo.CharmModifierEffect = toClone.CharmModifierEffect;
            newGoo.AppliesCheese = toClone.AppliesCheese;
            newGoo.CheeseModifierEffect = toClone.CheeseModifierEffect;


            //Direct Damage
            newGoo.damagesEnemies = toClone.damagesEnemies;
            newGoo.damagesPlayers = toClone.damagesPlayers;
            newGoo.damagePerSecondtoEnemies = toClone.damagePerSecondtoEnemies;
            newGoo.electrifiedDamageToPlayer = toClone.electrifiedDamageToPlayer;
            newGoo.electrifiedDamagePerSecondToEnemies = toClone.electrifiedDamagePerSecondToEnemies;
            newGoo.delayBeforeDamageToPlayers = toClone.delayBeforeDamageToPlayers;
            newGoo.damageTypes = toClone.damageTypes;

            //Appearance
            newGoo.baseColor32 = toClone.baseColor32;
            newGoo.goopTexture = toClone.goopTexture;
            newGoo.fadeColor32 = toClone.fadeColor32;
            newGoo.worldTexture = toClone.worldTexture;
            newGoo.usesWorldTextureByDefault = toClone.usesWorldTextureByDefault;
            newGoo.usesOverrideOpaqueness = toClone.usesOverrideOpaqueness;
            newGoo.overrideOpaqueness = toClone.overrideOpaqueness;
            newGoo.ambientGoopFX = toClone.ambientGoopFX;
            newGoo.ambientGoopFXChance = toClone.ambientGoopFXChance;
            newGoo.usesAmbientGoopFX = toClone.usesAmbientGoopFX;
            newGoo.isOily = toClone.isOily;
            newGoo.usesAcidAudio = toClone.usesAcidAudio;
            newGoo.usesWaterVfx = toClone.usesWaterVfx;
            newGoo.igniteColor32 = toClone.igniteColor32;
            newGoo.fireColor32 = toClone.fireColor32;

            //Freeze
            newGoo.CanBeFrozen = toClone.CanBeFrozen;
            newGoo.freezeLifespan = toClone.freezeLifespan;
            newGoo.freezeSpreadTime = toClone.freezeSpreadTime;
            newGoo.prefreezeColor32 = toClone.prefreezeColor32;
            newGoo.frozenColor32 = toClone.frozenColor32;

            //Speed
            newGoo.AppliesSpeedModifier = toClone.AppliesSpeedModifier;
            newGoo.AppliesSpeedModifierContinuously = toClone.AppliesSpeedModifierContinuously;
            newGoo.SpeedModifierEffect = toClone.SpeedModifierEffect;

            //Poison
            newGoo.AppliesDamageOverTime = toClone.AppliesDamageOverTime;
            newGoo.HealthModifierEffect = toClone.HealthModifierEffect;


            //Properties
            newGoo.lifespan = toClone.lifespan;
            newGoo.usesLifespan = toClone.usesLifespan;
            newGoo.fadePeriod = toClone.fadePeriod;
            newGoo.goopDamageTypeInteractions = toClone.goopDamageTypeInteractions;
            newGoo.lifespanRadialReduction = toClone.lifespanRadialReduction;
            newGoo.eternal = toClone.eternal;
            newGoo.playerStepsChangeLifetime = toClone.playerStepsChangeLifetime;
            newGoo.playerStepsLifetime = toClone.playerStepsLifetime;
            newGoo.DrainsAmmo = toClone.DrainsAmmo;
            newGoo.PercentAmmoDrainPerSecond = toClone.PercentAmmoDrainPerSecond;

            return newGoo;
        }
        public static void RegisterComponentToGoopDefinition(GoopDefinition def, Type comp)
        {
            if (specialGoopComps == null) specialGoopComps = new Dictionary<string, Type>();
            specialGoopComps.Add(def.name, comp);
        }
        private static DeadlyDeadlyGoopManager GoopManagerForTypeHook(Func<GoopDefinition, DeadlyDeadlyGoopManager> orig, GoopDefinition goop)
        {
            DeadlyDeadlyGoopManager newThing = orig( goop);
            string repairedGoopName = goop.name.Replace("(clone)", "");
            if (specialGoopComps.ContainsKey(repairedGoopName))
            {
                Type comp = specialGoopComps[repairedGoopName];            
                newThing.gameObject.AddComponent(comp);
            }
            return newThing;
        }
        private static void DoGoopEffectUpdateHook(Action<DeadlyDeadlyGoopManager, GameActor, IntVector2> orig, DeadlyDeadlyGoopManager self, GameActor actor, IntVector2 goopPosition)
        {
            orig(self, actor, goopPosition);
            if (self.gameObject.GetComponent<SpecialGoopBehaviourDoer>() != null) self.gameObject.GetComponent<SpecialGoopBehaviourDoer>().DoGoopEffectUpdate(self, actor, goopPosition);
        }
        private static void ActorEnteredGoopHook(Action<DeadlyDeadlyGoopManager, GameActor> orig, DeadlyDeadlyGoopManager self, GameActor actor)
        {
            orig(self, actor);
            if (self.gameObject.GetComponent<SpecialGoopBehaviourDoer>() != null) self.gameObject.GetComponent<SpecialGoopBehaviourDoer>().ActorEnteredGoop(self, actor);
        }
        private static void ActorLeftGoopHook(Action<DeadlyDeadlyGoopManager, GameActor> orig, DeadlyDeadlyGoopManager self, GameActor actor)
        {
            orig(self, actor);
            if (self.gameObject.GetComponent<SpecialGoopBehaviourDoer>() != null) self.gameObject.GetComponent<SpecialGoopBehaviourDoer>().ActorLeftGoop(self, actor);
        }

        public static GoopDefinition EternalFireDef;
        public static GoopDefinition NapalmDef;
        public static GoopDefinition QuickIgniteNapalmDef;
        public static GoopDefinition FireDef;
        public static GoopDefinition OilDef;
        public static GoopDefinition PoisonDef;
        public static GoopDefinition BlobulonGoopDef;
        public static GoopDefinition WebDef;
        public static GoopDefinition WaterDef;
        public static GoopDefinition CharmGoopDef = PickupObjectDatabase.GetById(310)?.GetComponent<WingsItem>()?.RollGoop;
        public static GoopDefinition GreenFireDef = (PickupObjectDatabase.GetById(698) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
        public static GoopDefinition CheeseDef = (PickupObjectDatabase.GetById(808) as Gun).DefaultModule.projectiles[0].GetComponent<GoopModifier>().goopDefinition;
        public static GoopDefinition BloodDef = PickupObjectDatabase.GetById(272)?.GetComponent<IronCoinItem>()?.BloodDefinition;
        public static GoopDefinition MimicSpitDef = EnemyDatabase.GetOrLoadByGuid("479556d05c7c44f3b6abb3b2067fc778").GetComponent<GoopDoer>().goopDefinition;
        public static GoopDefinition WineDef = EnemyDatabase.GetOrLoadByGuid("ffca09398635467da3b1f4a54bcfda80").bulletBank.GetBullet("goblet").BulletObject.GetComponent<GoopDoer>().goopDefinition;
        private static Dictionary<string, Type> specialGoopComps;
    }
    public class SpecialGoopBehaviourDoer : MonoBehaviour
    {
        public virtual void DoGoopEffectUpdate(DeadlyDeadlyGoopManager goop, GameActor actor, IntVector2 position)
        {

        }
        public virtual void ActorEnteredGoop(DeadlyDeadlyGoopManager goop, GameActor actor)
        {

        }
        public virtual void ActorLeftGoop(DeadlyDeadlyGoopManager goop, GameActor actor)
        {

        }
    }
}
