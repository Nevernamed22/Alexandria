using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    public class CustomThrowableEffectDoer : MonoBehaviour
    {
        private void Start()
        {
            throwable = base.GetComponent<CustomThrowableObject>();
            throwable.OnEffectTriggered += OnEffect;
        }
        public virtual void OnEffect(GameObject obj)
        {

        }
        private CustomThrowableObject throwable;
    }
    public class CustomThrowableObject : SpawnObjectItem
    {
        private void Start()
        {
            if (!string.IsNullOrEmpty(thrownSoundEffect)) AkSoundEngine.PostEvent(thrownSoundEffect, base.gameObject);
            DebrisObject component = base.gameObject.GetOrAddComponent<DebrisObject>();
            if (component)
            {
                component.killTranslationOnBounce = false;
                component.OnBounced += OnBounced;
                component.OnGrounded += OnHitGround;
            }
            if (!string.IsNullOrEmpty(OnThrownAnimation))
            {
                base.GetComponent<tk2dSpriteAnimator>().Play(OnThrownAnimation);
                tk2dSpriteAnimator spriteAnimator = base.GetComponent<tk2dSpriteAnimator>();
                spriteAnimator.AnimationCompleted += TransitionToIdle;
            }
            if (doEffectAfterTime)
            {
                Invoke("DoThing", timeTillEffect);
            }
        }
        private void TransitionToIdle(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
        {
            if (DefaultAnim != null && !animator.IsPlaying(OnTimedEffectAnim) && !animator.IsPlaying(OnHitGroundAnimation))
            {
                animator.Play(DefaultAnim);
            }
            animator.AnimationCompleted -= TransitionToIdle;
        }
        private void OnBounced(DebrisObject obj)
        {

        }
        private void OnHitGround(DebrisObject obj)
        {
            this.OnBounced(obj);
            if (doEffectOnHitGround) DoThing();
            if (!string.IsNullOrEmpty(landedSoundEffect)) AkSoundEngine.PostEvent(landedSoundEffect, base.gameObject);
            if (!string.IsNullOrEmpty(OnHitGroundAnimation))
            {
                if (destroyOnHitGround)
                {
                    base.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject(OnHitGroundAnimation, null);
                }
                else
                {
                    base.GetComponent<tk2dSpriteAnimator>().Play(OnHitGroundAnimation);
                }
            }
            else
            {
               if (destroyOnHitGround) UnityEngine.Object.Destroy(base.gameObject);
            }
        }
        private void DoThing()
        {
            if (!string.IsNullOrEmpty(OnEffectAnim))
            {
                base.GetComponent<tk2dSpriteAnimator>().Play(OnEffectAnim);
            }
            if (!string.IsNullOrEmpty(effectSoundEffect)) AkSoundEngine.PostEvent(effectSoundEffect, base.gameObject);
            if (this.OnEffectTriggered != null)
            {
                //ETGModConsole.Log("On effect in the component triggered");
                this.OnEffectTriggered(this.gameObject);
            }
            else { //ETGModConsole.Log("No on-effect to trigger");
                   }
        }
        public Action<GameObject> OnEffectTriggered;
        //Sounds
        public string thrownSoundEffect;
        public string landedSoundEffect;
        public string effectSoundEffect;
        //Animations
        public string DefaultAnim;
        public string OnTimedEffectAnim;
        public string OnThrownAnimation;
        public string OnHitGroundAnimation;
        public string OnEffectAnim;
        //Conditions
        public bool destroyOnHitGround;
        public bool doEffectOnHitGround;
        public bool doEffectAfterTime;
        public float timeTillEffect;
    }

}
