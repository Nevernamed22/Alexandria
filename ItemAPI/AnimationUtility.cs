using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
   public static class AnimationUtility
    {
        public static void AddAnimationToObject(this GameObject target, tk2dSpriteCollectionData spriteCollection, string animationName, List<string> spritePaths, int fps, Vector2 colliderDimensions, Vector2 colliderOffsets, tk2dBaseSprite.Anchor anchor, tk2dSpriteAnimationClip.WrapMode wrapMode, bool isDefaultAnimation = false, Assembly assembly = null)
        {
            tk2dSpriteAnimator animator = target.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = target.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;

            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = animationName, frames = new tk2dSpriteAnimationFrame[0], fps = fps };

            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            foreach (string path in spritePaths)
            {
                tk2dSpriteCollectionData collection = spriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(path, collection, assembly);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];

                frameDef.colliderVertices = new Vector3[]{
                    new Vector3(colliderOffsets.x/16, colliderOffsets.y/16, 0f),
                    new Vector3((colliderDimensions.x / 16), (colliderDimensions.y / 16), 0f)
                };

                frameDef.ConstructOffsetsFromAnchor(anchor);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            clip.frames = frames.ToArray();
            clip.wrapMode = wrapMode;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();

            if (isDefaultAnimation)
            {
                animator.DefaultClipId = animation.GetClipIdByName(animationName);
                animator.playAutomatically = true;
            }
        }
    }
}
