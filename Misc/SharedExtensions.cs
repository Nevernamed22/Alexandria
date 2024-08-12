using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI; // SpriteBuilder

namespace Alexandria.Misc
{
    internal static class Shared
    {
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
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
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
            tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleLeft)
        {
            foreach (var frame in beamAnimation.GetClipByName(animationName).frames)
            {
                tk2dSpriteDefinition frameDef = data.spriteDefinitions[frame.spriteId];
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
    }
}
