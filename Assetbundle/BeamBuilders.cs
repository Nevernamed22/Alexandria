using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using Alexandria.ItemAPI;

namespace Alexandria.Assetbundle
{
    public static class BeamBuilders
    {
        public static BasicBeamController GenerateBeamPrefabBundle(this Projectile projectile, string defaultSpriteName, tk2dSpriteCollectionData data,
            tk2dSpriteAnimation animation, string IdleAnimationName, Vector2 colliderDimensions, Vector2 colliderOffsets, string impactVFXAnimationName = null,
            Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, string endAnimation = null, Vector2? endColliderDimensions = null,
            Vector2? endColliderOffsets = null, string muzzleAnimationName = null, Vector2? muzzleColliderDimensions = null, Vector2? muzzleColliderOffsets = null,
            bool glows = false, bool canTelegraph = false, string beamTelegraphIdleAnimationName = null, string beamStartTelegraphAnimationName = null,
            string beamEndTelegraphAnimationName = null, float telegraphTime = 1, bool canDissipate = false, string beamDissipateAnimationName = null,
            string beamStartDissipateAnimationName = null, string beamEndDissipateAnimationName = null, float dissipateTime = 1)
        {
            return projectile.GenerateBeamPrefabBundleInternal(
                defaultSpriteName: defaultSpriteName,
                data: data,
                animation: animation,
                IdleAnimationName: IdleAnimationName,
                colliderDimensions: colliderDimensions,
                colliderOffsets: colliderOffsets,
                impactVFXAnimationName: impactVFXAnimationName,
                impactVFXColliderDimensions: impactVFXColliderDimensions,
                impactVFXColliderOffsets: impactVFXColliderOffsets,
                endAnimation: endAnimation,
                endColliderDimensions: endColliderDimensions,
                endColliderOffsets: endColliderOffsets,
                muzzleAnimationName: muzzleAnimationName,
                muzzleColliderDimensions: muzzleColliderDimensions,
                muzzleColliderOffsets: muzzleColliderOffsets,
                glows: glows,
                canTelegraph: canTelegraph,
                beamTelegraphIdleAnimationName: beamTelegraphIdleAnimationName,
                beamStartTelegraphAnimationName: beamStartTelegraphAnimationName,
                beamEndTelegraphAnimationName: beamEndTelegraphAnimationName,
                telegraphTime: telegraphTime,
                canDissipate: canDissipate,
                beamDissipateAnimationName: beamDissipateAnimationName,
                beamStartDissipateAnimationName: beamStartDissipateAnimationName,
                beamEndDissipateAnimationName: beamEndDissipateAnimationName,
                dissipateTime: dissipateTime,
                constructOffsets: true);
        }

        /// <summary>Version of GenerateBeamPrefabBundle to use if anchors have already been set up in the asset bundle</summary>
        public static BasicBeamController GenerateAnchoredBeamPrefabBundle(this Projectile projectile, string defaultSpriteName, tk2dSpriteCollectionData data,
            tk2dSpriteAnimation animation, string IdleAnimationName, Vector2 colliderDimensions, Vector2 colliderOffsets, string impactVFXAnimationName = null,
            Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, string endAnimation = null, Vector2? endColliderDimensions = null,
            Vector2? endColliderOffsets = null, string muzzleAnimationName = null, Vector2? muzzleColliderDimensions = null, Vector2? muzzleColliderOffsets = null,
            bool glows = false, bool canTelegraph = false, string beamTelegraphIdleAnimationName = null, string beamStartTelegraphAnimationName = null,
            string beamEndTelegraphAnimationName = null, float telegraphTime = 1, bool canDissipate = false, string beamDissipateAnimationName = null,
            string beamStartDissipateAnimationName = null, string beamEndDissipateAnimationName = null, float dissipateTime = 1)
        {
            return projectile.GenerateBeamPrefabBundleInternal(
                defaultSpriteName: defaultSpriteName,
                data: data,
                animation: animation,
                IdleAnimationName: IdleAnimationName,
                colliderDimensions: colliderDimensions,
                colliderOffsets: colliderOffsets,
                impactVFXAnimationName: impactVFXAnimationName,
                impactVFXColliderDimensions: impactVFXColliderDimensions,
                impactVFXColliderOffsets: impactVFXColliderOffsets,
                endAnimation: endAnimation,
                endColliderDimensions: endColliderDimensions,
                endColliderOffsets: endColliderOffsets,
                muzzleAnimationName: muzzleAnimationName,
                muzzleColliderDimensions: muzzleColliderDimensions,
                muzzleColliderOffsets: muzzleColliderOffsets,
                glows: glows,
                canTelegraph: canTelegraph,
                beamTelegraphIdleAnimationName: beamTelegraphIdleAnimationName,
                beamStartTelegraphAnimationName: beamStartTelegraphAnimationName,
                beamEndTelegraphAnimationName: beamEndTelegraphAnimationName,
                telegraphTime: telegraphTime,
                canDissipate: canDissipate,
                beamDissipateAnimationName: beamDissipateAnimationName,
                beamStartDissipateAnimationName: beamStartDissipateAnimationName,
                beamEndDissipateAnimationName: beamEndDissipateAnimationName,
                dissipateTime: dissipateTime,
                constructOffsets: false);
        }

        public static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false,
            bool changesCollider = true)
        {
            Shared.ConstructOffsetsFromAnchor(def, anchor, scale, fixesScale, changesCollider);
        }

        public static void MakeOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            Shared.MakeOffset(def, offset, changesCollider);
        }
    }
}
