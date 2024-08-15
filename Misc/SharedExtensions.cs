using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI; // SpriteBuilder

namespace Alexandria.Misc
{
    //WARNING: The sole purpose of this class is to unify methods that are repeated throughout Alexandria that can't be removed due to backwards compatibility issues.
    //         Never make this class or any of the methods inside it public.
    internal static class Shared
    {
        private static readonly HashSet<tk2dSpriteDefinition> adjustedDefs = new();

        internal static void MakeOffset(this tk2dSpriteDefinition def, Vector3 offset, bool changesCollider = false)
        {
            def.position0 += offset;
            def.position1 += offset;
            def.position2 += offset;
            def.position3 += offset;
            def.boundsDataCenter += offset;
            def.untrimmedBoundsDataCenter += offset;
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
                def.colliderVertices[0] += offset;
        }

        internal static void ConstructOffsetsFromAnchor(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            if (adjustedDefs.Contains(def))
                return; // don't set up offsets for definitions multiple times
            adjustedDefs.Add(def);

            if (!scale.HasValue)
            {
                scale = new Vector2?(def.position3);
            }
            if (fixesScale)
            {
                Vector2 fixedScale = scale.Value - def.position0.XY();
                scale = new Vector2?(fixedScale);
            }
            float xOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
            {
                xOffset = -(scale.Value.x / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                xOffset = -scale.Value.x;
            }
            float yOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
            {
                yOffset = -(scale.Value.y / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                yOffset = -scale.Value.y;
            }
            def.MakeOffset(new Vector2(xOffset, yOffset), false);
            if (changesCollider && def.colliderVertices != null && def.colliderVertices.Length > 0)
            {
                float colliderXOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.UpperLeft)
                {
                    colliderXOffset = (scale.Value.x / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderXOffset = -(scale.Value.x / 2f);
                }
                float colliderYOffset = 0;
                if (anchor == tk2dBaseSprite.Anchor.LowerLeft || anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.LowerRight)
                {
                    colliderYOffset = (scale.Value.y / 2f);
                }
                else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
                {
                    colliderYOffset = -(scale.Value.y / 2f);
                }
                def.colliderVertices[0] += new Vector3(colliderXOffset, colliderYOffset, 0);
            }
        }

        internal static tk2dSpriteDefinition ConstructDefinition(Texture2D texture, Material overrideMat = null, bool apply = true, bool useOffset = false)
        {
            RuntimeAtlasSegment ras = ETGMod.Assets.Packer.Pack(texture, apply); //pack your resources beforehand or the outlines will turn out weird

            Material material = null;
            if (overrideMat != null)
            {
                material = overrideMat;
            }
            else
            {
                material = new Material(ShaderCache.Acquire(PlayerController.DefaultShaderName));
            }
            material.mainTexture = ras.texture;

            var width = texture.width;
            var height = texture.height;

            var x = 0f;
            var y = 0f;

            var w = width / 16f;
            var h = height / 16f;

            float posX, posY, posW, posH;
            if (useOffset) //NOTE: I don't think the original code for this functions as intended, but I also can't find indication anyone uses it...
            {
                Vector2 anchor = tk2dSpriteGeomGen.GetAnchorOffset(tk2dBaseSprite.Anchor.LowerLeft, w, h);
                posX = -anchor.x;
                posY = -height + anchor.y; //NOTE: this doesn't seem right, but that's how it was originally...
                posW = w;
                posH = height; //NOTE: this doesn't seem right, but that's how it was originally...
            }
            else
            {
                posX = x;
                posY = y;
                posW = w;
                posH = h;
            }

            var def = new tk2dSpriteDefinition
            {
                normals = new Vector3[] {
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, -1.0f),
                    new Vector3(0.0f, 0.0f, -1.0f),
                },
                tangents = new Vector4[] {
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                },
                texelSize = new Vector2(1 / 16f, 1 / 16f),
                extractRegion = false,
                regionX = 0,
                regionY = 0,
                regionW = 0,
                regionH = 0,
                flipped = tk2dSpriteDefinition.FlipMode.None,
                complexGeometry = false,
                physicsEngine = tk2dSpriteDefinition.PhysicsEngine.Physics3D,
                colliderType = tk2dSpriteDefinition.ColliderType.None,
                collisionLayer = CollisionLayer.HighObstacle,
                position0 = new Vector3(posX, posY, 0f),
                position1 = new Vector3(posX + posW, posY, 0f),
                position2 = new Vector3(posX, posY + posH, 0f),
                position3 = new Vector3(posX + posW, posY + posH, 0f),
                material = material,
                materialInst = material,
                materialId = 0,
                uvs = ras.uvs,
                boundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
                boundsDataExtents = new Vector3(w, h, 0f),
                untrimmedBoundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
                untrimmedBoundsDataExtents = new Vector3(w, h, 0f),
            };

            def.name = texture.name;
            return def;
        }

        internal static tk2dSpriteAnimationClip CreateAnimation(tk2dSpriteCollectionData collection, List<int> spriteIDs, string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15, tk2dBaseSprite.Anchor? offsetAnchor = null)
        {
            bool constructOffsets = offsetAnchor.HasValue;
            tk2dSprite.Anchor anchor = offsetAnchor ?? default;
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>(spriteIDs.Count);
            for (int i = 0; i < spriteIDs.Count; i++)
            {
                tk2dSpriteDefinition def = collection.spriteDefinitions[spriteIDs[i]];
                if (!def.Valid)
                    continue;
                frames.Add(new tk2dSpriteAnimationFrame()
                {
                    spriteCollection = collection,
                    spriteId = spriteIDs[i]
                });
                if (constructOffsets)
                    ConstructOffsetsFromAnchor(def, anchor);
            }

            return new tk2dSpriteAnimationClip()
            {
                name = clipName,
                fps = fps,
                wrapMode = wrapMode,
                frames = frames.ToArray(),
            };
        }

        internal static tk2dSpriteAnimationClip CreateAnimation(this Assembly assembly, tk2dSpriteCollectionData collection, List<string> spritePaths, string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15, tk2dBaseSprite.Anchor? offsetAnchor = null)
        {
            List<int> spriteIDs = new List<int>(spritePaths.Count);
            foreach(var path in spritePaths)
                spriteIDs.Add(SpriteBuilder.AddSpriteToCollection(path, collection, assembly));
            return CreateAnimation(collection, spriteIDs, clipName, wrapMode, fps, offsetAnchor);
        }

        internal static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, tk2dSpriteCollectionData data, string animationName, Vector2? colliderDimensions = null,
            Vector2? colliderOffsets = null, Vector3[] overrideVertices = null, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
            tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleLeft, bool constructOffsets = true)
        {
            foreach (var frame in beamAnimation.GetClipByName(animationName).frames)
            {
                tk2dSpriteDefinition frameDef = data.spriteDefinitions[frame.spriteId];
                if (constructOffsets)
                    frameDef.ConstructOffsetsFromAnchor(anchor);
                if (overrideVertices != null)
                    frameDef.colliderVertices = overrideVertices;
                else if (colliderDimensions != null && colliderOffsets != null)
                    frameDef.colliderVertices = new Vector3[]{ 0.0625f * colliderOffsets.Value, 0.0625f * colliderDimensions.Value };
                else
                    ETGModConsole.Log("<size=100><color=#ff0000ff>BEAM ERROR: colliderDimensions or colliderOffsets was null with no override vertices!</color></size>", false);
            }
        }

        internal static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, List<string> animSpritePaths, string animationName, int fps, Assembly assembly,
            Vector2? colliderDimensions = null, Vector2? colliderOffsets = null, Vector3[] overrideVertices = null,
            tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter) //NOTE: why is the default anchor here different...
        {
            tk2dSpriteCollectionData collection = ETGMod.Databases.Items.ProjectileCollection;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() {
                name = animationName,
                frames = new tk2dSpriteAnimationFrame[animSpritePaths.Count],
                fps = fps,
            };
            for (int i = 0; i < animSpritePaths.Count; ++i)
            {
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(animSpritePaths[i], collection, assembly);
                clip.frames[i] = new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection };
            }
            beamAnimation.clips = beamAnimation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            SetupBeamPart(beamAnimation, collection, animationName, colliderDimensions, colliderOffsets, overrideVertices,
                wrapMode: tk2dSpriteAnimationClip.WrapMode.Once, anchor: anchor);
        }

        internal static BasicBeamController GenerateBeamPrefabBundleInternal(this Projectile projectile, string defaultSpriteName, tk2dSpriteCollectionData data,
            tk2dSpriteAnimation animation, string IdleAnimationName, Vector2 colliderDimensions, Vector2 colliderOffsets, string impactVFXAnimationName = null,
            Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, string endAnimation = null, Vector2? endColliderDimensions = null,
            Vector2? endColliderOffsets = null, string muzzleAnimationName = null, Vector2? muzzleColliderDimensions = null, Vector2? muzzleColliderOffsets = null,
            bool glows = false, bool canTelegraph = false, string beamTelegraphIdleAnimationName = null, string beamStartTelegraphAnimationName = null,
            string beamEndTelegraphAnimationName = null, float telegraphTime = 1, bool canDissipate = false, string beamDissipateAnimationName = null,
            string beamStartDissipateAnimationName = null, string beamEndDissipateAnimationName = null, float dissipateTime = 1, bool constructOffsets = true)
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
                    SetupBeamPart(animation, data, endAnimation, (Vector2)endColliderDimensions, (Vector2)endColliderOffsets, constructOffsets: constructOffsets);
                    beamController.beamEndAnimation = endAnimation;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices, constructOffsets: constructOffsets);
                    beamController.beamEndAnimation = IdleAnimationName;
                }

                //---------------Sets up the animaton for the VFX that plays over top of the end of the beam where it hits stuff
                if (impactVFXAnimationName != null && impactVFXColliderDimensions != null && impactVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, impactVFXAnimationName, (Vector2)impactVFXColliderDimensions, (Vector2)impactVFXColliderOffsets, anchor: tk2dBaseSprite.Anchor.MiddleCenter, constructOffsets: constructOffsets);
                    beamController.impactAnimation = impactVFXAnimationName;
                }

                //--------------Sets up the animation for the very start of the beam
                if (muzzleAnimationName != null && muzzleColliderDimensions != null && muzzleColliderOffsets != null)
                {
                    SetupBeamPart(animation, data, muzzleAnimationName, (Vector2)muzzleColliderDimensions, (Vector2)muzzleColliderOffsets, constructOffsets: constructOffsets);
                    beamController.beamStartAnimation = muzzleAnimationName;
                }
                else
                {
                    SetupBeamPart(animation, data, IdleAnimationName, null, null, def.colliderVertices, constructOffsets: constructOffsets);
                    beamController.beamStartAnimation = IdleAnimationName;
                }

                if (canTelegraph)
                {
                    beamController.usesTelegraph = true;
                    beamController.telegraphAnimations = new BasicBeamController.TelegraphAnims();
                    if (beamStartTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamStartTelegraphAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.telegraphAnimations.beamStartAnimation = beamStartTelegraphAnimationName;
                    }
                    if (beamTelegraphIdleAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamTelegraphIdleAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.telegraphAnimations.beamAnimation = beamTelegraphIdleAnimationName;
                    }
                    if (beamEndTelegraphAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndTelegraphAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
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
                        SetupBeamPart(animation, data, beamStartDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.dissipateAnimations.beamStartAnimation = beamStartDissipateAnimationName;
                    }
                    if (beamDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
                        beamController.dissipateAnimations.beamAnimation = beamDissipateAnimationName;
                    }
                    if (beamEndDissipateAnimationName != null)
                    {
                        SetupBeamPart(animation, data, beamEndDissipateAnimationName, Vector2.zero, Vector2.zero, constructOffsets: constructOffsets);
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

        internal static tk2dSpriteDefinition SetupDefinitionForProjectileSprite(string name, int id, tk2dSpriteCollectionData data, int pixelWidth, int pixelHeight, bool lightened = true, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null, int? overrideColliderOffsetX = null, int? overrideColliderOffsetY = null, Projectile overrideProjectileToCopyFrom = null)
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

        internal static PixelCollider SetupCollider(CollisionLayer layer, IntVector2 offset = default, IntVector2 dimensions = default,
            bool enabled = true, bool isTrigger = false, PixelCollider.PixelColliderGeneration mode = PixelCollider.PixelColliderGeneration.Manual)
        {
            return new PixelCollider()
            {
                ColliderGenerationMode = mode,
                CollisionLayer = layer,
                ManualWidth = dimensions.x,
                ManualHeight = dimensions.y,
                ManualOffsetX = offset.x,
                ManualOffsetY = offset.y,
                Enabled = enabled,
                IsTrigger = isTrigger,
            };
        }

        internal static PixelCollider AddCollider(this SpeculativeRigidbody body, CollisionLayer layer, IntVector2 offset = default, IntVector2 dimensions = default,
            bool enabled = true, bool isTrigger = false, PixelCollider.PixelColliderGeneration mode = PixelCollider.PixelColliderGeneration.Manual)
        {
            if (body.PixelColliders == null)
                body.PixelColliders = new List<PixelCollider>();
            PixelCollider collider = SetupCollider(layer, offset, dimensions, enabled, isTrigger, mode);
            body.PixelColliders.Add(collider);
            return collider;
        }

        internal static PixelCollider SetupPolygonCollider(CollisionLayer layer, IntVector2 offset, bool enabled = true, bool isTrigger = false)
        {
            return new PixelCollider()
            {
                ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon,
                CollisionLayer = layer,
                ManualOffsetX = offset.x,
                ManualOffsetY = offset.y,
                Enabled = enabled,
                IsTrigger = isTrigger,
            };
        }

        internal static PixelCollider AddPolygonCollider(this SpeculativeRigidbody body, CollisionLayer layer, IntVector2 offset, bool enabled = true, bool isTrigger = false)
        {
            if (body.PixelColliders == null)
                body.PixelColliders = new List<PixelCollider>();
            PixelCollider collider = SetupPolygonCollider(layer, offset, enabled, isTrigger);
            body.PixelColliders.Add(collider);
            return collider;
        }

        internal static DirectionalAnimation BlankDirectionalAnimation(string prefix = null)
        {
            return new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = prefix ?? string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
        }

        private static readonly Dictionary<string, List<string>> _SortedResourcesByAssembly = new();
        internal static List<string> SortedResourceNames(Assembly assembly)
        {
            string assemblyName = assembly.FullName;
            if (_SortedResourcesByAssembly.TryGetValue(assemblyName, out List<string> sorted))
                return sorted;
            string[] resources = ResourceExtractor.GetResourceNames(assembly);
            return _SortedResourcesByAssembly[assemblyName] = resources.OrderBy(x => x).ToList();
        }
    }
}
