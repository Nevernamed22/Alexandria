using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using UnityEngine;
using Brave.BulletScript;
using DirectionType = DirectionalAnimation.DirectionType;
using FlipType = DirectionalAnimation.FlipType;
using HarmonyLib;

namespace Alexandria.EnemyAPI
{
    [HarmonyPatch]
    public static class BossBuilder
    {
        private static GameObject behaviorSpeculatorPrefab;
        public static Dictionary<string, GameObject> Dictionary = new Dictionary<string, GameObject>();

        public static void Init()
        {
            var actor = EnemyDatabase.GetOrLoadByGuid("f905765488874846b7ff257ff81d6d0c");
            actor.gameObject.SetActive(false); // temporarily deactivate so instantiating doesn't call Awake() on everything
            behaviorSpeculatorPrefab = GameObject.Instantiate(actor.gameObject);
            actor.gameObject.SetActive(true);

            foreach (Transform child in behaviorSpeculatorPrefab.transform)
            {
                if (child != behaviorSpeculatorPrefab.transform && child.name != "GunAttachPoint")
                    GameObject.DestroyImmediate(child.gameObject);
            }

            foreach (var comp in behaviorSpeculatorPrefab.GetComponents<Component>())
            {
                if (comp.GetType() != typeof(BehaviorSpeculator) && comp.GetType() != typeof(Transform) && comp.GetType() != typeof(SpeculativeRigidbody) && comp.GetType() != typeof(MeshFilter) && comp.GetType() != typeof(MeshRenderer))
                {
                    GameObject.DestroyImmediate(comp);
                }
            }

            GameObject.DontDestroyOnLoad(behaviorSpeculatorPrefab);
            FakePrefab.MarkAsFakePrefab(behaviorSpeculatorPrefab);
            behaviorSpeculatorPrefab.SetActive(false);
        }

        [HarmonyPatch(typeof(EnemyDatabase), nameof(EnemyDatabase.GetOrLoadByGuid))]
        [HarmonyPostfix]
        private static void EnemyDatabaseGetOrLoadByGuidPatch(EnemyDatabase __instance, string guid, ref AIActor __result)
        {
            if (Dictionary.TryGetValue(guid, out GameObject companion))
                __result = companion.GetComponent<AIActor>();
        }

        public static GameObject BuildPrefab(string name, string guid, string defaultSpritePath, IntVector2 hitboxOffset, IntVector2 hitBoxSize, bool HasAiShooter, bool UsesAttackGroup = false)
        {
            if (BossBuilder.Dictionary.ContainsKey(guid))
            {
                ETGModConsole.Log("BossBuilder: Yea something went wrong. Complain to Neighborino about it.");
                return null;
            }
            var prefab = GameObject.Instantiate(behaviorSpeculatorPrefab);
            prefab.name = name;

            //setup misc components
            var sprite = SpriteBuilder.SpriteFromResource(defaultSpritePath, prefab, Assembly.GetCallingAssembly()).GetComponent<tk2dSprite>();

            sprite.SetUpSpeculativeRigidbody(hitboxOffset, hitBoxSize).CollideWithOthers = true;
            prefab.AddComponent<tk2dSpriteAnimator>();
            prefab.AddComponent<AIAnimator>();

            //setup knockback
            var knockback = prefab.AddComponent<KnockbackDoer>();
            knockback.weight = 1;

            //setup health haver
            var healthHaver = prefab.AddComponent<HealthHaver>();
            healthHaver.RegisterBodySprite(sprite);
            healthHaver.PreventAllDamage = false;
            healthHaver.SetHealthMaximum(15000);
            healthHaver.FullHeal();

            //setup AI Actor
            var aiActor = prefab.AddComponent<AIActor>();
            aiActor.State = AIActor.ActorState.Normal;
            aiActor.EnemyGuid = guid;
            aiActor.HasShadow = false;
            //setup behavior speculator
            var bs = prefab.GetComponent<BehaviorSpeculator>();

            bs.MovementBehaviors = new List<MovementBehaviorBase>();
            bs.TargetBehaviors = new List<TargetBehaviorBase>();
            bs.OverrideBehaviors = new List<OverrideBehaviorBase>();
            bs.OtherBehaviors = new List<BehaviorBase>();
            bs.AttackBehaviorGroup.AttackBehaviors = new List<AttackBehaviorGroup.AttackGroupItem>();
            if (HasAiShooter)
            {

                var actor = EnemyDatabase.GetOrLoadByGuid("01972dee89fc4404a5c408d50007dad5");
                behaviorSpeculatorPrefab = GameObject.Instantiate(actor.gameObject);
                foreach (Transform child in behaviorSpeculatorPrefab.transform)
                {
                    if (child != behaviorSpeculatorPrefab.transform && child.name != "GunAttachPoint")
                        GameObject.DestroyImmediate(child.gameObject);
                }

                foreach (var comp in behaviorSpeculatorPrefab.GetComponents<Component>())
                {
                    if (comp.GetType() != typeof(BehaviorSpeculator) && comp.GetType() != typeof(Transform) && comp.GetType() != typeof(SpeculativeRigidbody) && comp.GetType() != typeof(MeshFilter) && comp.GetType() != typeof(MeshRenderer))
                    {
                        GameObject.DestroyImmediate(comp);
                    }
                }

                GameObject.DontDestroyOnLoad(behaviorSpeculatorPrefab);
                FakePrefab.MarkAsFakePrefab(behaviorSpeculatorPrefab);
                behaviorSpeculatorPrefab.SetActive(false);

            }
            else
            {
                AIBulletBank aibulletBank = prefab.AddComponent<AIBulletBank>();
                aibulletBank.Bullets = new List<AIBulletBank.Entry>();
            }

            //Add to enemy database
            EnemyDatabaseEntry enemyDatabaseEntry = new EnemyDatabaseEntry()
            {
                myGuid = guid,
                placeableWidth = 2,
                placeableHeight = 2,
                isNormalEnemy = true
            };
            EnemyDatabase.Instance.Entries.Add(enemyDatabaseEntry);
            BossBuilder.Dictionary.Add(guid, prefab);


            //finalize
            GameObject.DontDestroyOnLoad(prefab);
            FakePrefab.MarkAsFakePrefab(prefab);
            prefab.SetActive(false);

            StaticReferenceManager.AllHealthHavers.Remove(healthHaver);

            return prefab;
        }

        public enum AnimationType { Move, Idle, Fidget, Flight, Hit, Talk, Other }
    }
}
