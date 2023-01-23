using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    class MagicCircle : MonoBehaviour
    {
        public static List<MagicCircle> AllMagicCircles = new List<MagicCircle>();
        public MagicCircle()
        {
            enabled = false;
            emitsParticles = false;
            colour = Color.white;
            radius = 3f;
            destroyOnDisable = true;
            autoEnableOnStart = true;
            autoEnableAutoDisableTimer = -1f;
            preventMagicIndicator = false;
        }
        private void Start()
        {
            AllMagicCircles.Add(this);
            if (autoEnableOnStart) { Enable(autoEnableAutoDisableTimer); }
        }
        private void OnDestroy() { AllMagicCircles.Remove(this); }
        public void Enable(float disableAfterSeconds = -1)
        {
            if (!enabled)
            {
                if (!preventMagicIndicator)
                {
                    if (m_radialIndicator != null) { m_radialIndicator.EndEffect(); }
                    m_radialIndicator = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.gameObject.transform.position, Quaternion.identity)).GetComponent<HeatIndicatorController>();
                    m_radialIndicator.CurrentColor = colour;
                    m_radialIndicator.IsFire = emitsParticles;
                    m_radialIndicator.CurrentRadius = radius;
                    m_radialIndicator.transform.parent = this.transform;
                }
                OnEnabled();
                enabled = true;

                if (disableAfterSeconds > 0) { base.StartCoroutine(DisableManager(disableAfterSeconds)); }
            }
            else { Debug.LogWarning("Alexandria (MagicCircleDoer): Cannot enable a magic circle which is already enabled."); }
        }
        private IEnumerator DisableManager(float time)
        {
            yield return new WaitForSeconds(time);
            Disable();
            yield break;
        }
        public void Disable()
        {
            if (m_radialIndicator != null) { m_radialIndicator.EndEffect(); m_radialIndicator = null; }
            for (int i = actorsInCircle.Count - 1; i >= 0; i--)
            {
                if (actorsInCircle[i] != null)
                {
                    EnemyLeftCircle(actorsInCircle[i]);
                }
            }
            actorsInCircle.Clear();
            OnDisabled();
            enabled = false;
            if (destroyOnDisable) { UnityEngine.Object.Destroy(gameObject); }
        }

        public void UpdateRadius(float newRadius)
        {
            radius = newRadius;
            if (m_radialIndicator)
            {
                m_radialIndicator.CurrentRadius = radius;
                m_radialIndicator.m_materialInst.SetFloat(m_radialIndicator.m_radiusID, radius);
            }
            OnRadiusUpdated();
        }

        public float radius;
        public bool preventMagicIndicator;
        public Color colour;
        public bool destroyOnDisable;
        public bool emitsParticles;
        public bool autoEnableOnStart;
        public float autoEnableAutoDisableTimer;

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnRadiusUpdated() { }
        public virtual void TickOnEnemy(AIActor enemy) { }
        public virtual void EnemyEnteredCircle(AIActor enemy) { }
        public virtual void EnemyLeftCircle(AIActor enemy) { }

        private void Update()
        {
            if (enabled && !GameManager.Instance.IsLoadingLevel && GameManager.Instance.Dungeon != null)
            {
                for (int i = StaticReferenceManager.AllEnemies.Count - 1; i >= 0; i--)
                {
                    if (StaticReferenceManager.AllEnemies[i] != null)
                    {
                        if (Vector2.Distance(StaticReferenceManager.AllEnemies[i].Position, base.gameObject.transform.position) <= radius)
                        {
                            if (!actorsInCircle.Contains(StaticReferenceManager.AllEnemies[i]))
                            {
                                EnemyEnteredCircle(StaticReferenceManager.AllEnemies[i]);
                                actorsInCircle.Add(StaticReferenceManager.AllEnemies[i]);
                            }
                            TickOnEnemy(StaticReferenceManager.AllEnemies[i]);
                        }
                        else if (actorsInCircle.Contains(StaticReferenceManager.AllEnemies[i]))
                        {
                            EnemyLeftCircle(StaticReferenceManager.AllEnemies[i]);
                            actorsInCircle.Remove(StaticReferenceManager.AllEnemies[i]);
                        }
                    }
                }
                for (int i = actorsInCircle.Count - 1; i >= 0; i--)
                {
                    if (actorsInCircle[i] == null) actorsInCircle.RemoveAt(i);
                }
            }
        }
        private bool enabled;
        private List<AIActor> actorsInCircle = new List<AIActor>();
        private HeatIndicatorController m_radialIndicator;


    }
}
