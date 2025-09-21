using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using UnityEngine;
using Brave.BulletScript;
using DirectionType = DirectionalAnimation.DirectionType;
using FlipType = DirectionalAnimation.FlipType;
using HarmonyLib;

namespace Alexandria.EnemyAPI
{
    [HarmonyPatch]
    public static class EnemyBuilder
    {
        private static GameObject behaviorSpeculatorPrefab;
        public static Dictionary<string, GameObject> Dictionary = new Dictionary<string, GameObject>();

        public static void Init()
        {
            var actor = EnemyDatabase.GetOrLoadByGuid("6b7ef9e5d05b4f96b04f05ef4a0d1b18");
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

        [HarmonyPatch(typeof(EnemyDatabase), nameof(EnemyDatabase.GetOrLoadByGuid))]
        [HarmonyPostfix]
        private static void EnemyDatabaseGetOrLoadByGuidPatch(EnemyDatabase __instance, string guid, ref AIActor __result)
        {
            if (Dictionary.TryGetValue(guid, out GameObject companion))
                __result = companion.GetComponent<AIActor>();
        }

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

        public static GameObject BuildPrefab(string name, string guid, string defaultSpritePath, IntVector2 hitboxOffset, IntVector2 hitBoxSize, bool HasAiShooter)
        {
            if (EnemyBuilder.Dictionary.ContainsKey(guid))
            {
                ETGModConsole.Log("EnemyBuilder: Yea something went wrong. Complain to Neighborino about it.");
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

            //setup behavior speculator
            var bs = prefab.GetComponent<BehaviorSpeculator>();

            bs.MovementBehaviors = new List<MovementBehaviorBase>();
            bs.AttackBehaviors = new List<AttackBehaviorBase>();
            bs.TargetBehaviors = new List<TargetBehaviorBase>();
            bs.OverrideBehaviors = new List<OverrideBehaviorBase>();
            bs.OtherBehaviors = new List<BehaviorBase>();
            AIBulletBank aibulletBank = prefab.AddComponent<AIBulletBank>();
            aibulletBank.Bullets = new List<AIBulletBank.Entry>();
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

            //Add to enemy database
            EnemyDatabaseEntry enemyDatabaseEntry = new EnemyDatabaseEntry()
            {
                myGuid = guid,
                placeableWidth = 2,
                placeableHeight = 2,
                isNormalEnemy = true
            };
            EnemyDatabase.Instance.Entries.Add(enemyDatabaseEntry);
            EnemyBuilder.Dictionary.Add(guid, prefab);


            //finalize
            GameObject.DontDestroyOnLoad(prefab);
            FakePrefab.MarkAsFakePrefab(prefab);
            prefab.SetActive(false);

            return prefab;
        }

        public enum AnimationType { Move, Idle, Fidget, Flight, Hit, Talk, Other }
        public static tk2dSpriteAnimationClip AddAnimation(this GameObject obj, string name, string spriteDirectory, int fps,
            AnimationType type, DirectionType directionType = DirectionType.None, FlipType flipType = FlipType.None, Assembly assembly = null)
        {
            AIAnimator orAddComponent = obj.GetOrAddComponent<AIAnimator>();
            DirectionalAnimation directionalAnimation = orAddComponent.GetDirectionalAnimation(name, directionType, type);
            directionalAnimation ??= Shared.BlankDirectionalAnimation(prefix: name, direction: directionType);
            directionalAnimation.AnimNames = directionalAnimation.AnimNames.Concat(new string[] { name }).ToArray<string>();
            directionalAnimation.Flipped = directionalAnimation.Flipped.Concat(new DirectionalAnimation.FlipType[] { flipType }).ToArray<DirectionalAnimation.FlipType>();
            orAddComponent.AssignDirectionalAnimation(name, directionalAnimation, type);
            return CompanionBuilder.BuildAnimation(orAddComponent, name, spriteDirectory, fps, Assembly.GetCallingAssembly());
        }


        public static tk2dSpriteAnimationClip[] AddAnimation(this GameObject obj, string enemyName, string name, string spriteDirectory, int fps, AnimationType type, DirectionType directionType = DirectionType.None, FlipType flipType = FlipType.None,
    tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Once)
        {
            AIAnimator aiAnimator = obj.GetOrAddComponent<AIAnimator>();
            DirectionalAnimation animation = aiAnimator.GetDirectionalAnimation(name, directionType, type);
            if (animation == null)
            {
                animation = new DirectionalAnimation()
                {
                    AnimNames = new string[DirectionalAnimation.m_combined[(int)directionType].Length + 1],
                    Flipped = new FlipType[DirectionalAnimation.m_combined[(int)directionType].Length + 1],
                    Type = directionType,
                    Prefix = name
                };
            }

            Shared.Append(ref animation.AnimNames, name);
            aiAnimator.AssignDirectionalAnimation(name, animation, type);
            return BuildAnimations(aiAnimator, name, enemyName, directionType, spriteDirectory, fps, wrapMode, Assembly.GetCallingAssembly());
        }

        public static tk2dSpriteAnimationClip[] BuildAnimations(AIAnimator aiAnimator, string name, string enemyName, DirectionType directionType, string spriteDirectory, int fps, tk2dSpriteAnimationClip.WrapMode wrapMode, Assembly assembly = null)
        {
            tk2dSpriteCollectionData collection = aiAnimator.GetComponent<tk2dSpriteCollectionData>();
            if (!collection)
                collection = SpriteBuilder.ConstructCollection(aiAnimator.gameObject, $"{aiAnimator.name}_collection");

            var sortedList = Shared.SortedResourceNames(assembly ?? Assembly.GetCallingAssembly());
            var animList = new List<tk2dSpriteAnimationClip>();
            string prefix = $"{spriteDirectory.Replace('/', '.')}.{enemyName}_{name.ToLower()}";
            foreach (var a in DirectionalAnimation.m_combined[(int)directionType])
            {
                List<int> indices = new List<int>();
                for (int i = 0; i < sortedList.Count; i++)
                    if (sortedList[i].Contains($"{prefix}_{a.suffix}_0", false))
                        indices.Add(SpriteBuilder.AddSpriteToCollection(sortedList[i], collection, assembly ?? Assembly.GetCallingAssembly()));
                if (indices.Count == 0)
                    continue;

                tk2dSpriteAnimationClip clip = SpriteBuilder.AddAnimation(aiAnimator.spriteAnimator, collection, indices, $"{name.ToLower()}_{a.suffix}", wrapMode);
                clip.fps = fps;
                animList.Add(clip);
            }
            return animList.ToArray();
        }

        public static tk2dSpriteAnimationClip BuildAnimation(AIAnimator aiAnimator, string name, string spriteDirectory, int fps, Assembly assembly = null)
        {
            tk2dSpriteCollectionData collection = aiAnimator.GetComponent<tk2dSpriteCollectionData>();
            if (!collection)
                collection = SpriteBuilder.ConstructCollection(aiAnimator.gameObject, $"{aiAnimator.name}_collection");

            var sortedList = Shared.SortedResourceNames(assembly ?? Assembly.GetCallingAssembly());
            List<int> indices = new List<int>();
            string prefix = spriteDirectory.Replace('/', '.');
            for (int i = 0; i < sortedList.Count; i++)
                if (sortedList[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    indices.Add(SpriteBuilder.AddSpriteToCollection(sortedList[i], collection, assembly ?? Assembly.GetCallingAssembly()));
            tk2dSpriteAnimationClip clip = SpriteBuilder.AddAnimation(aiAnimator.spriteAnimator, collection, indices, name, tk2dSpriteAnimationClip.WrapMode.Once);
            clip.fps = fps;
            return clip;
        }

        public static DirectionalAnimation GetDirectionalAnimation(this AIAnimator aiAnimator, string name, DirectionType directionType, AnimationType type)
        {
            return type switch
            {
                AnimationType.Idle => aiAnimator.IdleAnimation,
                AnimationType.Move => aiAnimator.MoveAnimation,
                AnimationType.Flight => aiAnimator.FlightAnimation,
                AnimationType.Hit => aiAnimator.HitAnimation,
                AnimationType.Talk => aiAnimator.TalkAnimation,
                _ => null,
            };
        }

        public static void AssignDirectionalAnimation(this AIAnimator aiAnimator, string name, DirectionalAnimation animation, AnimationType type)
        {
            switch (type)
            {
                case AnimationType.Idle:
                    aiAnimator.IdleAnimation = animation;
                    break;
                case AnimationType.Move:
                    aiAnimator.MoveAnimation = animation;
                    break;
                case AnimationType.Flight:
                    aiAnimator.FlightAnimation = animation;
                    break;
                case AnimationType.Hit:
                    aiAnimator.HitAnimation = animation;
                    break;
                case AnimationType.Talk:
                    aiAnimator.TalkAnimation = animation;
                    break;
                case AnimationType.Fidget:
                    aiAnimator.IdleFidgetAnimations.Add(animation);
                    break;
                default:
                    if(aiAnimator.OtherAnimations == null)
                    {
                        aiAnimator.OtherAnimations = new List<AIAnimator.NamedDirectionalAnimation>();
                    }

                    aiAnimator.OtherAnimations.Add(new AIAnimator.NamedDirectionalAnimation()
                    {
                        anim = animation,
                        name = name
                    });
                    break;
            }
        }

        public static void DuplicateAIShooterAndAIBulletBank(GameObject targetObject, AIShooter sourceShooter, AIBulletBank sourceBulletBank, int startingGunOverrideID = 0, Transform gunAttachPointOverride = null, Transform bulletScriptAttachPointOverride = null, PlayerHandController overrideHandObject = null)
        {
            if (targetObject.GetComponent<AIShooter>() && targetObject.GetComponent<AIBulletBank>())
            {
                return;
            }
            if (!targetObject.GetComponent<AIBulletBank>())
            {
                AIBulletBank aibulletBank = targetObject.AddComponent<AIBulletBank>();
                aibulletBank.Bullets = new List<AIBulletBank.Entry>(0);
                if (sourceBulletBank.Bullets.Count > 0)
                {
                    foreach (AIBulletBank.Entry entry in sourceBulletBank.Bullets)
                    {
                        aibulletBank.Bullets.Add(new AIBulletBank.Entry
                        {
                            Name = entry.Name,
                            BulletObject = entry.BulletObject,
                            OverrideProjectile = entry.OverrideProjectile,
                            ProjectileData = new ProjectileData
                            {
                                damage = entry.ProjectileData.damage,
                                speed = entry.ProjectileData.speed,
                                range = entry.ProjectileData.range,
                                force = entry.ProjectileData.force,
                                damping = entry.ProjectileData.damping,
                                UsesCustomAccelerationCurve = entry.ProjectileData.UsesCustomAccelerationCurve,
                                AccelerationCurve = entry.ProjectileData.AccelerationCurve,
                                CustomAccelerationCurveDuration = entry.ProjectileData.CustomAccelerationCurveDuration,
                                onDestroyBulletScript = entry.ProjectileData.onDestroyBulletScript,
                                IgnoreAccelCurveTime = entry.ProjectileData.IgnoreAccelCurveTime
                            },
                            PlayAudio = entry.PlayAudio,
                            AudioSwitch = entry.AudioSwitch,
                            AudioEvent = entry.AudioEvent,
                            AudioLimitOncePerFrame = entry.AudioLimitOncePerFrame,
                            AudioLimitOncePerAttack = entry.AudioLimitOncePerAttack,
                            MuzzleFlashEffects = new VFXPool
                            {
                                effects = entry.MuzzleFlashEffects.effects,
                                type = entry.MuzzleFlashEffects.type
                            },
                            MuzzleLimitOncePerFrame = entry.MuzzleLimitOncePerFrame,
                            MuzzleInheritsTransformDirection = entry.MuzzleInheritsTransformDirection,
                            ShellTransform = entry.ShellTransform,
                            ShellPrefab = entry.ShellPrefab,
                            ShellForce = entry.ShellForce,
                            ShellForceVariance = entry.ShellForceVariance,
                            DontRotateShell = entry.DontRotateShell,
                            ShellGroundOffset = entry.ShellGroundOffset,
                            ShellsLimitOncePerFrame = entry.ShellsLimitOncePerFrame,
                            rampBullets = entry.rampBullets,
                            conditionalMinDegFromNorth = entry.conditionalMinDegFromNorth,
                            forceCanHitEnemies = entry.forceCanHitEnemies,
                            suppressHitEffectsIfOffscreen = entry.suppressHitEffectsIfOffscreen,
                            preloadCount = entry.preloadCount
                        });
                    }
                }
                aibulletBank.useDefaultBulletIfMissing = true;
                aibulletBank.transforms = new List<Transform>();
                if (sourceBulletBank.transforms != null && sourceBulletBank.transforms.Count > 0)
                {
                    foreach (Transform item in sourceBulletBank.transforms)
                    {
                        aibulletBank.transforms.Add(item);
                    }
                }
                aibulletBank.RegenerateCache();
            }
            if (!targetObject.GetComponent<AIShooter>() && sourceShooter != null)
            {
                AIShooter aishooter = targetObject.AddComponent<AIShooter>();
                aishooter.volley = sourceShooter.volley;
                if (startingGunOverrideID != 0)
                {
                    aishooter.equippedGunId = startingGunOverrideID;
                }
                else
                {
                    aishooter.equippedGunId = sourceShooter.equippedGunId;
                }
                aishooter.shouldUseGunReload = true;
                aishooter.volleyShootPosition = sourceShooter.volleyShootPosition;
                aishooter.volleyShellCasing = sourceShooter.volleyShellCasing;
                aishooter.volleyShellTransform = sourceShooter.volleyShellTransform;
                aishooter.volleyShootVfx = sourceShooter.volleyShootVfx;
                aishooter.usesOctantShootVFX = sourceShooter.usesOctantShootVFX;
                aishooter.bulletName = sourceShooter.bulletName;
                aishooter.customShootCooldownPeriod = sourceShooter.customShootCooldownPeriod;
                aishooter.doesScreenShake = sourceShooter.doesScreenShake;
                aishooter.rampBullets = sourceShooter.rampBullets;
                aishooter.rampStartHeight = sourceShooter.rampStartHeight;
                aishooter.rampTime = sourceShooter.rampTime;
                if (gunAttachPointOverride)
                {
                    aishooter.gunAttachPoint = gunAttachPointOverride;
                }
                else
                {
                    aishooter.gunAttachPoint = sourceShooter.gunAttachPoint;
                }
                if (bulletScriptAttachPointOverride)
                {
                    aishooter.bulletScriptAttachPoint = bulletScriptAttachPointOverride;
                }
                else
                {
                    aishooter.bulletScriptAttachPoint = sourceShooter.bulletScriptAttachPoint;
                }
                aishooter.overallGunAttachOffset = sourceShooter.overallGunAttachOffset;
                aishooter.flippedGunAttachOffset = sourceShooter.flippedGunAttachOffset;
                if (overrideHandObject)
                {
                    aishooter.handObject = overrideHandObject;
                }
                else
                {
                    aishooter.handObject = sourceShooter.handObject;
                }
                aishooter.AllowTwoHands = sourceShooter.AllowTwoHands;
                aishooter.ForceGunOnTop = sourceShooter.ForceGunOnTop;
                aishooter.IsReallyBigBoy = sourceShooter.IsReallyBigBoy;
                aishooter.BackupAimInMoveDirection = sourceShooter.BackupAimInMoveDirection;
                aishooter.RegenerateCache();
            }
        }

    }
}
