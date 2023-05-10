using Alexandria.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    public class ProjectileSlashingBehaviour : MonoBehaviour
    {
        public ProjectileSlashingBehaviour()
        {
            DestroyBaseAfterFirstSlash = false;
            timeBetweenSlashes = 1;
            initialDelay = 0f;
            slashParameters = ScriptableObject.CreateInstance<SlashData>();
            SlashDamageUsesBaseProjectileDamage = true;
            timeBetweenCustomSequenceSlashes = 0.15f;
            customSequence = null;
            angleVariance = 0;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();
            timer = initialDelay;
            if (this.m_projectile.Owner && this.m_projectile.Owner is PlayerController) this.owner = this.m_projectile.Owner as PlayerController;
        }
        private void Update()
        {
            if (this.m_projectile)
            {
                if (timer > 0f) { timer -= BraveTime.DeltaTime; }
                else { m_projectile.StartCoroutine(DoAttackSequence()); }
            }
        }
        private IEnumerator DoAttackSequence()
        {
            if (customSequence != null && customSequence.Count > 0)
            {
                foreach (float angle in customSequence)
                {
                    DoSlash(angle);
                    yield return new WaitForSeconds(timeBetweenCustomSequenceSlashes);
                }
            }
            else { DoSlash(0); }

            timer = timeBetweenSlashes;
            if (DestroyBaseAfterFirstSlash) StartCoroutine(Suicide());
            yield break;
        }
        private void DoSlash(float angle)
        {
            Projectile proj = this.m_projectile;
            List<GameActorEffect> effects = new List<GameActorEffect>();
            effects.AddRange(proj.GetFullListOfStatusEffects(true));

            SlashData instSlash = SlashData.CloneSlashData(slashParameters);

            if (SlashDamageUsesBaseProjectileDamage)
            {
                instSlash.damage = this.m_projectile.baseData.damage;
                instSlash.bossDamageMult = this.m_projectile.BossDamageMultiplier;
                instSlash.jammedDamageMult = this.m_projectile.BlackPhantomDamageMultiplier;
                instSlash.enemyKnockbackForce = this.m_projectile.baseData.force;
            }
            instSlash.OnHitTarget += SlashHitTarget;
            instSlash.OnHitBullet += SlashHitBullet;
            instSlash.OnHitMajorBreakable += SlashHitMajorBreakable;
            instSlash.OnHitMinorBreakable += SlashHitMinorBreakable;

            angle += UnityEngine.Random.Range(angleVariance, -angleVariance);

            SlashDoer.DoSwordSlash(this.m_projectile.specRigidbody.UnitCenter, (this.m_projectile.Direction.ToAngle() + angle), owner, instSlash);
        }
        private IEnumerator Suicide()
        {
            yield return null;
            if (DestroysOnlyComponentAfterFirstSlash == true) { Destroy(this); }
            else 
            {
                UnityEngine.Object.Destroy(this.m_projectile.gameObject);
            }
            yield break;
        }
        /// <summary>
        /// Called when the slash hits a GameActor. Can be overridden for custom effects.
        /// </summary>
        /// <param name="target">The game actor that has been hit by the slash.</param>
        /// <param name="fatal">Whether or not the slash killed the actor it hit.</param>
        public virtual void SlashHitTarget(GameActor target, bool fatal) { }
        /// <summary>
        /// Called when the slash hits a projectile.
        /// </summary>
        /// <param name="target">The projectile that has been hit by the slash.</param>
        public virtual void SlashHitBullet(Projectile target) { }
        /// <summary>
        /// Called when the slash hits a Minor breakable object
        /// </summary>
        /// <param name="target">The object that has been hit by the slash.</param>
        public virtual void SlashHitMinorBreakable(MinorBreakable target) { }
        /// <summary>
        /// Called when the slash hits a Major breakable object
        /// </summary>
        /// <param name="target">The object that has been hit by the slash.</param>
        public virtual void SlashHitMajorBreakable(MajorBreakable target) { }

        /// <summary>
        /// How long should the projectile wait after spawning before doing it's first slash. Zero by default, meaning it occurs instantly.
        /// </summary>
        public float initialDelay;
        private float timer;
        /// <summary>
        /// How long the projectile will wait between performing subsequent slashes after the first.
        /// </summary>
        public float timeBetweenSlashes;
        /// <summary>
        /// If true, the slash's damage, boss damage multiplier, jammed damage multiplier, and knockback stats will be equal to the base projectile's stats.
        /// </summary>
        public bool SlashDamageUsesBaseProjectileDamage;
        /// <summary>
        /// If true, the base projectile will be erased after performing it's first slash/sequence of slashes.
        /// </summary>
        public bool DestroyBaseAfterFirstSlash;
        /// <summary>
        /// If true, the base projectile will only erase the slashing COMPONENT after performing it's first slash/sequence of slashes.
        /// </summary>
        public bool DestroysOnlyComponentAfterFirstSlash = false;
        /// <summary>
        /// The data which defines the exact nature of the slash created.
        /// </summary>
        public SlashData slashParameters;
        private Projectile m_projectile;
        private PlayerController owner;

        /// <summary>
        /// The time between slashes in a custom sequence. Only works if customSequence is set.
        /// </summary>
        public float timeBetweenCustomSequenceSlashes;
        /// <summary>
        /// A list of angles (0-360) where 0 is the projectile's direction of travel. If set, when performing a slash the projectile will instead perform a sequence of slashes corresponding to the angles in the sequence.
        /// </summary>
        public List<float> customSequence;
        /// <summary>
        /// If set, the precise direction of the slash relative to the base projectile's direction will be able to vary by up to that number of degrees in either direction.
        /// </summary>
        public float angleVariance;
    }
}