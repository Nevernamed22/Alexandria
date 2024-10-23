using System.Collections;
using UnityEngine;

namespace Alexandria.CustomDodgeRollAPI
{
    /// Private API for CustomDodgeRoll
    public partial class CustomDodgeRoll : MonoBehaviour
    {
        /// <summary>The currently-running <see cref="ContinueDodgeRoll()"/> coroutine.</summary>
        private Coroutine _activeDodgeRoll = null;

        /// <summary>The last time a dodge roll input was buffered.</summary>
        internal float _bufferTime { get; set; }

        internal bool TryBeginDodgeRoll(Vector2 direction, bool buffered)
        {
            if (!canDodge || (_isDodging && !canMultidodge) || (!canDodgeInPlace && direction == Vector2.zero))
                return false;
            BeginDodgeRollInternal(direction, buffered);
            return true;
        }

        private void BeginDodgeRollInternal(Vector2 direction, bool buffered)
        {
            bool wasAlreadyDodging = _isDodging; // check if we are already in the middle of a dodge roll
            AbortDodgeRoll(); // clean up any extant dodge roll in case we are multidodging
            _owner.lockedDodgeRollDirection = direction; // set lockedDodgeRollDirection, even if we might not use it
            _owner.m_dodgeRollTimer = 0.0f; // clear vanilla dodge roll timer
            _owner.m_hasFiredWhileSliding = false; // clear firing status during sliding
            _owner.m_rollDamagedEnemies.Clear(); // clear the list of enemies we dodged into last dodge roll
            _owner.TablesDamagedThisSlide.Clear(); // clear list of damaged tables during slide
            _owner.m_dodgeRollState = PlayerController.DodgeRollState.None; // clear the vanilla dodge roll state
            if (_owner.m_handlingQueuedAnimation) // clear any queued animations (e.g., when dodging out of minecarts)
                _owner.QueuedAnimationComplete(_owner.spriteAnimator, _owner.spriteAnimator.currentClip);
            BeginDodgeRoll(direction, buffered, wasAlreadyDodging);
            _isDodging = true;

            // by default, we want to make sure we can put out fires at the beginning of our dodge roll
            if (fireReduction > 0.0f && _owner.CurrentFireMeterValue > 0f)
            {
                _owner.CurrentFireMeterValue = Mathf.Max(0f, _owner.CurrentFireMeterValue - fireReduction);
                if (_owner.CurrentFireMeterValue == 0f)
                    _owner.IsOnFire = false;
            }

            _activeDodgeRoll = _owner.StartCoroutine(DoDodgeRollWrapper());
        }

        private IEnumerator DoDodgeRollWrapper()
        {
            IEnumerator script = ContinueDodgeRoll();
            while(_isDodging && script.MoveNext())
                yield return script.Current;
            if (_isDodging)
            {
                FinishDodgeRoll(aborted: false);
                _isDodging = false;
            }
            yield break;
        }
    }
}
