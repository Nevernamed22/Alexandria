using System.Collections;
using UnityEngine;

namespace Alexandria.CustomDodgeRollAPI
{
    /// <summary>Public API surface for <see cref="CustomDodgeRoll"/></summary>
    public partial class CustomDodgeRoll : MonoBehaviour
    {
        /// <summary>The PlayerController owner of this dodge roll.</summary>
        public PlayerController _owner  { get; internal set; }
        /// <summary>Whether <see cref="ContinueDodgeRoll()"/> is currently running.</summary>
        public bool _isDodging          { get; private set; }
        /// <summary>Whether the player has been continuously holding the dodge button since initiating the dodge roll.</summary>
        public bool _dodgeButtonHeld    { get; internal set; }
        /// <summary>Whether the player is currently pressing the dodge button at all.</summary>
        public bool _dodgeButtonPressed => (_owner && _owner.PlayerIDX >= 0)
            ? BraveInput.GetInstanceForPlayer(_owner.PlayerIDX).ActiveActions.DodgeRollAction.IsPressed
            : false;

        /// <summary>
        /// Whether the base game's <see cref="PlayerController.IsDodgeRolling"/> property returns true while this dodge roll is active.
        /// This property should generally just return "true" unless you REALLY know what you're getting into.
        /// </summary>
        public virtual bool  countsAsDodgeRolling => true;
        /// <summary>Custom logic imposing additional restrictions on whether the player can dodge roll.</summary>
        public virtual bool  canDodge             => true;
        /// <summary>Whether this dodge roll can be initiated again while it's already in progress.</summary>
        public virtual bool  canMultidodge        => false;
        /// <summary>Whether this dodge roll can be initiated while the player is not moving.</summary>
        public virtual bool  canDodgeInPlace      => false;
        /// <summary>Whether the player can attack while the dodge roll is active.</summary>
        public virtual bool  canUseWeapon         => false;
        /// <summary>Whether the dodge roll can slide over tables while active.</summary>
        public virtual bool  canSlide             => true;
        /// <summary>Whether the dodge roll counts as airborne for pitfall purposes while active.</summary>
        public virtual bool  isAirborne           => true;
        /// <summary>Whether the dodge roll goes through projectiles while active.</summary>
        public virtual bool  dodgesProjectiles    => true;
        /// <summary>Whether the player is direction-locked while the dodge roll is active.</summary>
        public virtual bool  lockedDirection      => true;
        /// <summary>Whether the player takes contact damage from enemies while the dodge roll is active.</summary>
        public virtual bool  takesContactDamage   => true;
        /// <summary>Base damage to deal when contacting an enemy while the dodge roll is active. If less than 0, defaults to player's base roll damage stat.</summary>
        public virtual float overrideRollDamage   => -1f;
        /// <summary>Percent by which the player's fire meter is reduced upon initiating the dodge roll (vanilla dodge rolls reduce it by 50%).</summary>
        public virtual float fireReduction        => 0.5f;
        /// <summary>How many seconds in advance the dodge roll can be buffered. Set to 0 to disable buffering.</summary>
        public virtual float bufferWindow         => 0.0f;


        /// <summary>Called automatically when the player successfully begins the custom dodge roll.</summary>
        /// <param name="direction">The direction the player initiated the dodge roll in.</param>
        /// <param name="buffered">Whether the dodge roll was buffered.</param>
        /// <param name="wasAlreadyDodging">
        /// Whether we were already dodging prior to beginning this custom dodge roll. Only possible if <see cref="canMultidodge"/> returns true.
        /// </param>
        protected virtual void BeginDodgeRoll(Vector2 direction, bool buffered, bool wasAlreadyDodging)
        {
            // any dodge setup code should be here
        }

        /// <summary>
        /// Called automatically after <see cref="BeginDodgeRoll"/> while the custom dodge roll is active.
        /// If this coroutine finishes naturally, <see cref="FinishDodgeRoll"/> is called with aborted == false.
        /// If this coroutine is aborted before finishing, <see cref="FinishDodgeRoll"/> is called with aborted == true.
        /// </summary>
        protected virtual IEnumerator ContinueDodgeRoll()
        {
            // code to execute while dodge rolling should be here
            yield break;
        }

        /// <summary>Called automatically when the player completes the custom dodge roll (i.e., after <see cref="ContinueDodgeRoll"/> finishes).</summary>
        /// <param name="aborted">Whether <see cref="ContinueDodgeRoll"/> was ended early for any reason (multidodge, cutscene, opening a chest, etc.).</param>
        protected virtual void FinishDodgeRoll(bool aborted)
        {
            // any succesful (or aborted) dodge cleanup code should be here
        }

        /// <summary>If the custom dodge roll is active, immediately ends <see cref="ContinueDodgeRoll"/> and calls <see cref="FinishDodgeRoll"/> with aborted == true.</summary>
        public void AbortDodgeRoll()
        {
            if (!_isDodging)
                return;
            _isDodging = false; //WARNING: this is set here because dodge rolls that call RecalculateStats() in FinishDodgeRoll() can get stuck in an infinte loop
            FinishDodgeRoll(aborted: true);
            if (_activeDodgeRoll != null)
            {
                _owner.StopCoroutine(_activeDodgeRoll);
                _activeDodgeRoll = null;
            }
        }

        /// <summary>Forcibly begin a vanilla dodge roll.</summary>
        public void ForceVanillaDodgeRoll() => _owner.ForceVanillaDodgeRollInternal();

        /// <summary>Forcibly begin a vanilla dodge roll in a specific direction.</summary>
        public void ForceVanillaDodgeRoll(Vector2 vec) => _owner.ForceVanillaDodgeRollInternal(vec);
    }
}
