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
        public static BasicBeamController GenerateBeamPrefabBundle(this Projectile projectile, string defaultSpriteName, tk2dSpriteCollectionData data, tk2dSpriteAnimation animation, string IdleAnimationName, Vector2 colliderDimensions, Vector2 colliderOffsets, string impactVFXAnimationName = null, Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, string endAnimation = null, Vector2? endColliderDimensions = null, Vector2? endColliderOffsets = null, string muzzleAnimationName = null, Vector2? muzzleColliderDimensions = null, Vector2? muzzleColliderOffsets = null, bool glows = false,
            bool canTelegraph = false, string beamTelegraphIdleAnimationName = null, string beamStartTelegraphAnimationName = null, string beamEndTelegraphAnimationName = null, float telegraphTime = 1,
            bool canDissipate = false, string beamDissipateAnimationName = null, string beamStartDissipateAnimationName = null, string beamEndDissipateAnimationName = null, float dissipateTime = 1)
        {
            try
            {
                if (projectile.specRigidbody)
                    projectile.specRigidbody.CollideWithOthers = false;

                tk2dTiledSprite tiledSprite = projectile.gameObject.GetOrAddComponent<tk2dTiledSprite>();

                tiledSprite.Collection = data;
                tiledSprite.SetSprite(data, data.GetSpriteIdByName(defaultSpriteName));
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[]{ 0.0625f * colliderOffsets, 0.0625f * colliderDimensions };

                def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft); //NOTE: this seems right, but double check later

                tk2dSpriteAnimator animator = projectile.gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
                animator._startingSpriteCollection = data;
                animator.Library = animation;
                animator.playAutomatically = true;
                animator.defaultClipId = animation.GetClipIdByName(IdleAnimationName);

                UnityEngine.Object.Destroy(projectile.GetComponentInChildren<tk2dSprite>());
                projectile.sprite = tiledSprite;
                projectile.sprite.Collection = data;

                BasicBeamController beamController = projectile.gameObject.GetOrAddComponent<BasicBeamController>();
                beamController.sprite = tiledSprite;
                beamController.spriteAnimator = animator;
                beamController.m_beamSprite = tiledSprite;

                //---------------- Sets up the animation for the main part of the beam
                beamController.beamAnimation = IdleAnimationName;

                //------------- Sets up the animation for the part of the beam that touches the wall

                if (endAnimation != null && endColliderDimensions != null && endColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, endAnimation, (Vector2)endColliderDimensions, (Vector2)endColliderOffsets);
                    beamController.beamEndAnimation = endAnimation;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices);
                    beamController.beamEndAnimation = IdleAnimationName;
                }

                //---------------Sets up the animaton for the VFX that plays over top of the end of the beam where it hits stuff
                if (impactVFXAnimationName != null && impactVFXColliderDimensions != null && impactVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, impactVFXAnimationName, (Vector2)impactVFXColliderDimensions, (Vector2)impactVFXColliderOffsets);
                    beamController.impactAnimation = impactVFXAnimationName;
                }

                //--------------Sets up the animation for the very start of the beam
                if (muzzleAnimationName != null && muzzleColliderDimensions != null && muzzleColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, muzzleAnimationName, (Vector2)muzzleColliderDimensions, (Vector2)muzzleColliderOffsets);
                    beamController.beamStartAnimation = muzzleAnimationName;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices);
                    beamController.beamStartAnimation = IdleAnimationName;
                }

                if (canTelegraph)
                {
                    beamController.usesTelegraph = true;
                    beamController.telegraphAnimations = new BasicBeamController.TelegraphAnims();
                    if (beamStartTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamStartTelegraphAnimationName, Vector2.zero, Vector2.zero);
                        beamController.telegraphAnimations.beamStartAnimation = beamStartTelegraphAnimationName;
                    }
                    if (beamTelegraphIdleAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamTelegraphIdleAnimationName, Vector2.zero, Vector2.zero);
                        beamController.telegraphAnimations.beamAnimation = beamTelegraphIdleAnimationName;
                    }
                    if (beamEndTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndTelegraphAnimationName, Vector2.zero, Vector2.zero);
                        beamController.telegraphAnimations.beamEndAnimation = beamEndTelegraphAnimationName;
                    }
                    beamController.telegraphTime = telegraphTime;
                }

                if (canDissipate)
                {
                    beamController.endType = BasicBeamController.BeamEndType.Dissipate;
                    beamController.dissipateAnimations = new BasicBeamController.TelegraphAnims();
                    if (beamStartDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamStartDissipateAnimationName, Vector2.zero, Vector2.zero);
                        beamController.dissipateAnimations.beamStartAnimation = beamStartDissipateAnimationName;
                    }
                    if (beamDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamDissipateAnimationName, Vector2.zero, Vector2.zero);
                        beamController.dissipateAnimations.beamAnimation = beamDissipateAnimationName;
                    }
                    if (beamEndDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndDissipateAnimationName, Vector2.zero, Vector2.zero);
                        beamController.dissipateAnimations.beamEndAnimation = beamEndDissipateAnimationName;
                    }
                    beamController.dissipateTime = dissipateTime;
                }

                if (glows)
                    projectile.gameObject.GetOrAddComponent<EmmisiveBeams>();
                return beamController;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
                return null;
            }
        }

        public static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            Shared.ConstructOffsetsFromAnchor(def, anchor, scale, fixesScale, changesCollider);
        }

        public static void MakeOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            Shared.MakeOffset(def, offset, changesCollider);
        }

        private static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, tk2dSpriteCollectionData data, string animationName, Vector2? colliderDimensions = null, Vector2? colliderOffsets = null, Vector3[] overrideVertices = null, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Once)
        {
            Shared.SetupBeamPart(beamAnimation, data, animationName, colliderDimensions, colliderOffsets, overrideVertices, wrapMode, anchor: tk2dBaseSprite.Anchor.MiddleLeft);
        }
    }
}
