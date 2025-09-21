using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
   public static class AnimationUtility
    {
        public static void AddAnimationToObject(this GameObject target, tk2dSpriteCollectionData spriteCollection, string animationName, List<string> spritePaths, int fps, Vector2 colliderDimensions, Vector2 colliderOffsets, tk2dBaseSprite.Anchor anchor, tk2dSpriteAnimationClip.WrapMode wrapMode, bool isDefaultAnimation = false, Assembly assembly = null)
        {
            tk2dSpriteAnimationFrame[] frames = new tk2dSpriteAnimationFrame[spritePaths.Count];
            for (int i = 0; i < spritePaths.Count; ++i)
            {
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], spriteCollection, assembly);
                tk2dSpriteDefinition frameDef = spriteCollection.spriteDefinitions[frameSpriteId];

                frameDef.colliderVertices = new Vector3[]{
                    new Vector3(colliderOffsets.x / 16, colliderOffsets.y / 16, 0f),
                    new Vector3((colliderDimensions.x / 16), (colliderDimensions.y / 16), 0f)
                };

                frameDef.ConstructOffsetsFromAnchor(anchor);
                frames[i] = new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = spriteCollection };
            }

            tk2dSpriteAnimator animator = target.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = target.GetOrAddComponent<tk2dSpriteAnimation>();
            animator.Library = animation;
            animation.clips = new tk2dSpriteAnimationClip[1] { new tk2dSpriteAnimationClip() { name = animationName, fps = fps, frames = frames, wrapMode = wrapMode } };

            if (isDefaultAnimation)
            {
                animator.DefaultClipId = animation.GetClipIdByName(animationName);
                animator.playAutomatically = true;
            }
        }
    }
}
