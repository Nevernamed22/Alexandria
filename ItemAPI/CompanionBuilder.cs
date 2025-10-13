using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gungeon;
using UnityEngine;
using Alexandria.Misc;
using HarmonyLib;

namespace Alexandria.ItemAPI
{
    [HarmonyPatch]
    public static class CompanionBuilder
    {
        public static void Init()
        {
            string companionGuid = Game.Items["dog"].GetComponent<CompanionItem>().CompanionGuid;
            AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(companionGuid);
            CompanionBuilder.behaviorSpeculatorPrefab = UnityEngine.Object.Instantiate<GameObject>(orLoadByGuid.gameObject);
            foreach (object obj in CompanionBuilder.behaviorSpeculatorPrefab.transform)
            {
                Transform transform = (Transform)obj;
                if (transform != CompanionBuilder.behaviorSpeculatorPrefab.transform)
                {
                    UnityEngine.Object.DestroyImmediate(transform.gameObject);
                }
            }
            foreach (Component comp in CompanionBuilder.behaviorSpeculatorPrefab.GetComponents<Component>())
            {
                if (comp.GetType() != typeof(BehaviorSpeculator) && comp.GetType() != typeof(Transform) && comp.GetType() != typeof(SpeculativeRigidbody) && comp.GetType() != typeof(MeshFilter) && comp.GetType() != typeof(MeshRenderer))
                {
                    GameObject.DestroyImmediate(comp);
                }
            }
            CompanionBuilder.behaviorSpeculatorPrefab.MakeFakePrefab();
        }

        [HarmonyPatch(typeof(EnemyDatabase), nameof(EnemyDatabase.GetOrLoadByGuid))]
        [HarmonyPrefix]
        private static bool EnemyDatabaseGetOrLoadByGuidPatch(EnemyDatabase __instance, string guid, ref AIActor __result)
        {
            if (!companionDictionary.TryGetValue(guid, out GameObject companion))
                return true;
            __result = companion.GetComponent<AIActor>();
            return false;
        }

        public static GameObject BuildPrefab(string name, string guid, string defaultSpritePath, IntVector2 hitboxOffset, IntVector2 hitBoxSize)
        {
            if (CompanionBuilder.companionDictionary.ContainsKey(guid))
            {
                ETGModConsole.Log("CompanionBuilder: Tried to create two companion prefabs with the same GUID!", false);
                return null;
            }

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(CompanionBuilder.behaviorSpeculatorPrefab);
            gameObject.name = name;
            tk2dSprite component = SpriteBuilder.SpriteFromResource(defaultSpritePath, gameObject, Assembly.GetCallingAssembly()).GetComponent<tk2dSprite>();
            component.SetUpSpeculativeRigidbody(hitboxOffset, hitBoxSize).CollideWithOthers = false;
            gameObject.AddComponent<tk2dSpriteAnimator>();
            AIAnimator aiAnimator = gameObject.AddComponent<AIAnimator>();
            aiAnimator.OtherVFX = new List<AIAnimator.NamedVFXPool>(0); // fixes a null deref on exit when destroying the fake prefab
            HealthHaver healthHaver = gameObject.AddComponent<HealthHaver>();
            healthHaver.RegisterBodySprite(component, false, 0);
            healthHaver.PreventAllDamage = true;
            healthHaver.SetHealthMaximum(15000f, null, false);
            healthHaver.FullHeal();
            AIActor aiactor = gameObject.AddComponent<AIActor>();
            aiactor.State = AIActor.ActorState.Normal;
            aiactor.EnemyGuid = guid;
            BehaviorSpeculator component2 = gameObject.GetComponent<BehaviorSpeculator>();
            component2.MovementBehaviors = new List<MovementBehaviorBase>();
            component2.AttackBehaviors = new List<AttackBehaviorBase>();
            component2.TargetBehaviors = new List<TargetBehaviorBase>();
            component2.OverrideBehaviors = new List<OverrideBehaviorBase>();
            component2.OtherBehaviors = new List<BehaviorBase>();
            EnemyDatabaseEntry item = new EnemyDatabaseEntry
            {
                myGuid = guid,
                placeableWidth = 2,
                placeableHeight = 2,
                isNormalEnemy = false
            };
            EnemyDatabase.Instance.Entries.Add(item);
            CompanionBuilder.companionDictionary.Add(guid, gameObject);
            gameObject.MakeFakePrefab();
            return gameObject;
        }

        public static tk2dSpriteAnimationClip AddAnimation(this GameObject obj, string name, string spriteDirectory, int fps, CompanionBuilder.AnimationType type, DirectionalAnimation.DirectionType directionType = DirectionalAnimation.DirectionType.None, DirectionalAnimation.FlipType flipType = DirectionalAnimation.FlipType.None)
        {
            AIAnimator orAddComponent = obj.GetOrAddComponent<AIAnimator>();
            DirectionalAnimation directionalAnimation = orAddComponent.GetDirectionalAnimation(name, directionType, type);
            directionalAnimation ??= Shared.BlankDirectionalAnimation(prefix: name, direction: directionType);
            Shared.Append(ref directionalAnimation.AnimNames, name);
            Shared.Append(ref directionalAnimation.Flipped, flipType);
            orAddComponent.AssignDirectionalAnimation(name, directionalAnimation, type);
            return CompanionBuilder.BuildAnimation(orAddComponent, name, spriteDirectory, fps, Assembly.GetCallingAssembly());
        }

        public static tk2dSpriteAnimationClip BuildAnimation(AIAnimator aiAnimator, string name, string spriteDirectory, int fps, Assembly assembly = null)
        {
            tk2dSpriteCollectionData tk2dSpriteCollectionData = aiAnimator.GetComponent<tk2dSpriteCollectionData>();
            if (!tk2dSpriteCollectionData)
            {
                tk2dSpriteCollectionData = SpriteBuilder.ConstructCollection(aiAnimator.gameObject, aiAnimator.name + "_collection");
            }
            string[] resourceNames = ResourceExtractor.GetResourceNames(assembly ?? Assembly.GetCallingAssembly());
            List<int> list = new List<int>(resourceNames.Length);
            for (int i = 0; i < resourceNames.Length; i++)
            {
                if (resourceNames[i].StartsWith(spriteDirectory.Replace('/', '.'), StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(SpriteBuilder.AddSpriteToCollection(resourceNames[i], tk2dSpriteCollectionData, assembly ?? Assembly.GetCallingAssembly()));
                }
            }
            tk2dSpriteAnimationClip tk2dSpriteAnimationClip = SpriteBuilder.AddAnimation(aiAnimator.spriteAnimator, tk2dSpriteCollectionData, list, name, tk2dSpriteAnimationClip.WrapMode.Loop);
            tk2dSpriteAnimationClip.fps = (float)fps;
            return tk2dSpriteAnimationClip;
        }

        public static DirectionalAnimation GetDirectionalAnimation(this AIAnimator aiAnimator, string name, DirectionalAnimation.DirectionType directionType, CompanionBuilder.AnimationType type)
        {
            return type switch
            {
                CompanionBuilder.AnimationType.Move => aiAnimator.MoveAnimation,
                CompanionBuilder.AnimationType.Idle => aiAnimator.IdleAnimation,
                CompanionBuilder.AnimationType.Flight => aiAnimator.FlightAnimation,
                CompanionBuilder.AnimationType.Hit => aiAnimator.HitAnimation,
                CompanionBuilder.AnimationType.Talk => aiAnimator.TalkAnimation,
                _ => null,
            };
        }

        public static void AssignDirectionalAnimation(this AIAnimator aiAnimator, string name, DirectionalAnimation animation, CompanionBuilder.AnimationType type)
        {
            switch (type)
            {
                case CompanionBuilder.AnimationType.Move:
                    aiAnimator.MoveAnimation = animation;
                    break;
                case CompanionBuilder.AnimationType.Idle:
                    aiAnimator.IdleAnimation = animation;
                    break;
                case CompanionBuilder.AnimationType.Fidget:
                    aiAnimator.IdleFidgetAnimations.Add(animation);
                    break;
                case CompanionBuilder.AnimationType.Flight:
                    aiAnimator.FlightAnimation = animation;
                    break;
                case CompanionBuilder.AnimationType.Hit:
                    aiAnimator.HitAnimation = animation;
                    break;
                case CompanionBuilder.AnimationType.Talk:
                    aiAnimator.TalkAnimation = animation;
                    break;
                default:
                    aiAnimator.OtherAnimations ??= new List<AIAnimator.NamedDirectionalAnimation>();
                    aiAnimator.OtherAnimations.Add(new AIAnimator.NamedDirectionalAnimation
                    {
                        anim = animation,
                        name = name
                    });
                    break;
            }
        }

        private static GameObject behaviorSpeculatorPrefab;

        public static Dictionary<string, GameObject> companionDictionary = new Dictionary<string, GameObject>();

        public enum AnimationType
        {
            // Token: 0x0400012F RID: 303
            Move,
            // Token: 0x04000130 RID: 304
            Idle,
            // Token: 0x04000131 RID: 305
            Fidget,
            // Token: 0x04000132 RID: 306
            Flight,
            // Token: 0x04000133 RID: 307
            Hit,
            // Token: 0x04000134 RID: 308
            Talk,
            // Token: 0x04000135 RID: 309
            Other
        }
    }
}
