using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class Tk2dSpriteAnimatorUtility
    {
        /// <summary>
        /// Adds an event trigger to specific frames in an animation. 
        /// </summary>
        /// <param name="animator">The target tk2dSpriteAnimator.</param>
        /// <param name="animationName">The name of the aniamtion you want to affect.</param>
        /// <param name="frameAndEventName">A dictionary containing the frames and the event name you want to trigger at that frame. Ex: { 0, "sparkle" }, will trigger the event 'sparkle' on the first frame of the animation </param>
        public static void AddEventTriggersToAnimation(tk2dSpriteAnimator animator, string animationName, Dictionary<int, string> frameAndEventName)
        {
            foreach (var value in frameAndEventName)
            {
                var clip = animator.GetClipByName(animationName);
                clip.frames[value.Key].eventInfo = value.Value;
                clip.frames[value.Key].triggerEvent = true;
            }
        }

        /// <summary>
        /// Offsets specific frames positions in an animation. Be wary to NOT use this on the same frames AND aniamtions multiple times!
        /// </summary>
        /// <param name="animator">The target tk2dSpriteAnimator.</param>
        /// <param name="animationName">The name of the aniamtion you want to affect.</param>
        /// <param name="Offset">A dictionary containing the frames and the offset you want to apply to that frame. Ex: { 0, new Vector3(0.25f, 0.25f) }, will ofsset the frame by 4 pixels up and 4 pixels right on the first frame of the animation </param>
        /// <param name="idListfallback">Can be left as null. Used to store ids between uses to make sure not to offset these frames on accident.</param>

        public static void AddOffsetToFrames(tk2dSpriteAnimator animator, string animationName, Dictionary<int, Vector3> Offset, List<int> idListfallback = null)
        {
            tk2dSpriteAnimationClip awakenClip = animator.GetClipByName(animationName);
            List<int> idsModified = idListfallback ?? new List<int>();
            foreach (var value in Offset)
            {
                int i = value.Key;
                var Value = value.Value;
                var s = awakenClip.frames[i];
                if (s != null)
                {
                    int id = s.spriteId;
                    if (!idsModified.Contains(id))
                    {
                        idsModified.Add(id);
                        awakenClip.frames[i].spriteCollection.spriteDefinitions[id].position0 += Value;
                        awakenClip.frames[i].spriteCollection.spriteDefinitions[id].position1 += Value;
                        awakenClip.frames[i].spriteCollection.spriteDefinitions[id].position2 += Value;
                        awakenClip.frames[i].spriteCollection.spriteDefinitions[id].position3 += Value;
                    }
                }
            }
        }

        /// <summary>
        /// Adds triggers to specific frames in an animation to play sounds. 
        /// </summary>
        /// <param name="animator">The target tk2dSpriteAnimator.</param>
        /// <param name="animationName">The name of the aniamtion you want to affect.</param>
        /// <param name="frameAndSoundName">A dictionary containing the frames and the sound name you want to trigger at that frame. Ex: { 0, "Play_ENM_hammer_target_01" }, will play the sound 'Play_ENM_hammer_target_01' on the first frame of the animation </param>
        public static void AddSoundsToAnimationFrame(tk2dSpriteAnimator animator, string animationName, Dictionary<int, string> frameAndSoundName)
        {
            foreach (var value in frameAndSoundName)
            {
                animator.GetClipByName(animationName).frames[value.Key].eventAudio = value.Value;
                animator.GetClipByName(animationName).frames[value.Key].triggerEvent = true;
            }
        }

        /// <summary>
        /// Marks all frames in an animation as spawning frames. Probably only used for enemies. 
        /// </summary>
        /// <param name="animator">The target tk2dSpriteAnimator.</param>
        /// <param name="animationName">The name of the aniamtion you want to affect.</param>
        public static void MarkAnimationAsSpawn(tk2dSpriteAnimator animator, string animationName)
        {
            foreach (var value in animator.GetClipByName(animationName).frames)
            {
                value.finishedSpawning = false;
            }
        }
        /// <summary>
        /// Marks specific frames in an animation to make the enemy invulnerable to damage on those frames. 
        /// </summary>
        /// <param name="animator">The target tk2dSpriteAnimator.</param>
        /// <param name="animationName">The name of the aniamtion you want to affect.</param>
        /// <param name="frameAndBool">A dictionary containing the frames and a true/false for whether it'll be invulnerable on that frame. (Default is false) Ex: { 0, true }, will make the enemy invulnerable on the first frame of the animation </param>

        public static void AddInvulnverabilityFramesToAnimation(tk2dSpriteAnimator animator, string animationName, Dictionary<int, bool> frameAndBool)
        {
            foreach (var value in frameAndBool)
            {
                animator.GetClipByName(animationName).frames[value.Key].invulnerableFrame = value.Value;
                animator.GetClipByName(animationName).frames[value.Key].triggerEvent = true;
            }
        }
    }
}
