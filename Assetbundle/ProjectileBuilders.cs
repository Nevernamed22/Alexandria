using Alexandria.ItemAPI;
using Alexandria.PrefabAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Assetbundle
{
    public static class ProjectileBuilders
    {
        public static List<T> ConstructListOfSameValues<T>(T value, int length)
        {
            return Enumerable.Repeat<T>(value, length).ToList();
        }

        public static tk2dSpriteDefinition SetProjectileCollisionRight(this Projectile proj, string name, tk2dSpriteCollectionData data, int pixelWidth, int pixelHeight, bool lightened = true, tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.LowerLeft, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, bool anchorChangesCollider = true, bool fixesScale = false, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            try
            {
                proj.sprite.Collection = data;
                proj.GetAnySprite().spriteId = data.GetSpriteIdByName(name);
                tk2dSpriteDefinition def = SetupDefinitionForProjectileSprite(name, proj.GetAnySprite().spriteId, data, pixelWidth, pixelHeight, lightened,
                    overrideColliderPixelWidth, overrideColliderPixelHeight, overrideColliderOffsetX, overrideColliderOffsetY, overrideProjectileToCopyFrom);

                def.ConstructOffsetsFromAnchor(anchor, def.position3, fixesScale, anchorChangesCollider);
                proj.GetAnySprite().scale = Vector3.one;
                proj.transform.localScale = Vector3.one;
                proj.GetAnySprite().transform.localScale = Vector3.one;
                proj.AdditionalScaleMultiplier = 1f;
                return def;
            }
            catch (Exception ex)
            {
                ETGModConsole.Log("Ooops! Seems like something got very, Very, VERY wrong. Here's the exception:");
                ETGModConsole.Log(ex.ToString());
                return null;
            }
        }

        private static tk2dSpriteDefinition SetupDefinitionForProjectileSprite(string name, int id, tk2dSpriteCollectionData data, int pixelWidth, int pixelHeight, bool lightened = true, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
        {
            overrideColliderPixelWidth ??= pixelWidth;
            overrideColliderPixelHeight ??= pixelHeight;
            overrideColliderOffsetX ??= 0;
            overrideColliderOffsetY ??= 0;

            float trueWidth = 0.0625f * pixelWidth;
            float trueHeight = 0.0625f * pixelHeight;
            float colliderWidth = 0.0625f * overrideColliderPixelWidth.Value;
            float colliderHeight = 0.0625f * overrideColliderPixelHeight.Value;
            float colliderOffsetX = 0.0625f * overrideColliderOffsetX.Value;
            float colliderOffsetY = 0.0625f * overrideColliderOffsetY.Value;
            tk2dSpriteDefinition def = ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[(overrideProjectileToCopyFrom ??
                    (PickupObjectDatabase.GetById(lightened ? 47 : 12) as Gun).DefaultModule.projectiles[0]).GetAnySprite().spriteId].CopyDefinitionFrom();
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.texelSize = new Vector2(1 / 16f, 1 / 16f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);

            def.materialInst.mainTexture = data.spriteDefinitions[id].materialInst.mainTexture;
            def.uvs = data.spriteDefinitions[id].uvs.ToArray();

            def.colliderVertices = new Vector3[2];
            def.colliderVertices[0] = new Vector3(colliderOffsetX, colliderOffsetY, 0f);
            def.colliderVertices[1] = new Vector3(colliderWidth / 2, colliderHeight / 2);
            def.name = name;
            data.spriteDefinitions[id] = def;
            return def;
        }

        public static void AnimateProjectileBundle(this Projectile proj, string defaultClipName, tk2dSpriteCollectionData data, tk2dSpriteAnimation animation, string animationName, List<IntVector2> pixelSizes, List<bool> lighteneds, List<tk2dBaseSprite.Anchor> anchors, List<bool> anchorsChangeColliders, List<bool> fixesScales, List<Vector3?> manualOffsets, List<IntVector2?> overrideColliderPixelSizes, List<IntVector2?> overrideColliderOffsets, List<Projectile> overrideProjectilesToCopyFrom)
        {
            if (proj.sprite.spriteAnimator == null)
                proj.sprite.spriteAnimator = proj.sprite.gameObject.AddComponent<tk2dSpriteAnimator>();
            proj.sprite.spriteAnimator.Library = animation;
            proj.sprite.spriteAnimator.playAutomatically = true;
            if (defaultClipName != null)
                proj.sprite.spriteAnimator.DefaultClipId = animation.GetClipIdByName(defaultClipName);

            var frames = animation.GetClipByName(animationName).frames;
            if (frames == null || frames.Length == 0)
                return;

            proj.GetAnySprite().SetSprite(data, frames[0].spriteId);
            for (int i = 0; i < frames.Length; i++)
            {
                tk2dSpriteDefinition def = SetupDefinitionForProjectileSprite(animationName, frames[i].spriteId, data, pixelSizes[i].x, pixelSizes[i].y, lighteneds[i],
                    overrideColliderPixelSizes[i]?.x, overrideColliderPixelSizes[i]?.y, overrideColliderOffsets[i]?.x, overrideColliderOffsets[i]?.y,
                    overrideProjectilesToCopyFrom[i]);
                def.ConstructOffsetsFromAnchor(anchors[i], def.position3, fixesScales[i], anchorsChangeColliders[i]);
                if (manualOffsets[i] is Vector3 manualOffset)
                {
                    def.position0 += manualOffset;
                    def.position1 += manualOffset;
                    def.position2 += manualOffset;
                    def.position3 += manualOffset;
                }
            }
        }

        public static GameObject AddTrailToProjectileBundle(this Projectile target, tk2dSpriteCollectionData tk2DSpriteCollectionData, string spriteName, tk2dSpriteAnimation animationLibrary, string defaultAnimation, Vector2 colliderDimensions, Vector2 colliderOffsets, bool destroyOnEmpty = false, string startAnimationName = null,
            float timeTillAnimStart = 0f, float cascadeTimer = -1, float softMaxLength = -1)
        {
            try
            {
                GameObject newTrailObject = PrefabBuilder.BuildObject("trailObject");
                newTrailObject.transform.parent = target.transform;
                newTrailObject.name = "trailObject";

                tk2dTiledSprite tiledSprite = newTrailObject.GetOrAddComponent<tk2dTiledSprite>();

                tiledSprite.SetSprite(tk2DSpriteCollectionData, tk2DSpriteCollectionData.GetSpriteIdByName(spriteName));
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[]{ 0.0625f * colliderOffsets, 0.0625f * colliderDimensions };
                def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.LowerLeft); //NOTE: this doesn't seem right, but maybe it is?
                tk2dSpriteAnimator animator = newTrailObject.GetOrAddComponent<tk2dSpriteAnimator>();
                animator.playAutomatically = true;
                animator.defaultClipId = animationLibrary.GetClipIdByName(defaultAnimation);
                animator.Library = animationLibrary;

                TrailController trail = newTrailObject.AddComponent<TrailController>();
                trail.usesAnimation = defaultAnimation != null;
                if (trail.usesAnimation)
                {
                    SetupBeamPart(animationLibrary, defaultAnimation, null, null, def.colliderVertices);
                    trail.animation = defaultAnimation;
                }

                trail.usesStartAnimation = startAnimationName != null;
                if (trail.usesStartAnimation)
                {
                    SetupBeamPart(animationLibrary, startAnimationName, null, null, def.colliderVertices);
                    trail.startAnimation = startAnimationName;
                }

                //Trail Variables
                trail.usesSoftMaxLength = (softMaxLength > 0);
                trail.softMaxLength = softMaxLength;
                trail.usesCascadeTimer = (cascadeTimer > 0);
                trail.cascadeTimer = cascadeTimer;
                trail.usesGlobalTimer = true;
                trail.globalTimer = timeTillAnimStart;
                trail.destroyOnEmpty = destroyOnEmpty;
                return newTrailObject;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
                return null;
            }
        }
        private static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, string animationName, Vector2? colliderDimensions = null, Vector2? colliderOffsets = null, Vector3[] overrideVertices = null)
        {
            if (beamAnimation.GetClipByName(animationName) is not tk2dSpriteAnimationClip clip)
                return;
            if (clip.frames == null || clip.frames.Length == 0)
                return;
            Shared.SetupBeamPart(beamAnimation, clip.frames[0].spriteCollection, animationName, colliderDimensions, colliderOffsets, overrideVertices,
                tk2dSpriteAnimationClip.WrapMode.Once, anchor: tk2dBaseSprite.Anchor.MiddleLeft); //NOTE: a third different offset
        }
    }

}
