using System;
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
        }
        private void Start() { AllMagicCircles.Add(this); }
        private void OnDestroy() { AllMagicCircles.Remove(this); }
        public void Enable()
        {
            if (!enabled)
            {
                if (m_radialIndicator != null) { m_radialIndicator.EndEffect(); }
                m_radialIndicator = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.gameObject.transform.position, Quaternion.identity)).GetComponent<HeatIndicatorController>();
                m_radialIndicator.CurrentColor = colour;
                m_radialIndicator.IsFire = emitsParticles;
                m_radialIndicator.CurrentRadius = radius;
                m_radialIndicator.transform.parent = this.transform;
                OnEnabled();
                enabled = true;
            }
            else { Debug.LogWarning("Alexandria (MagicCircleDoer): Cannot enable a magic circle which is already enabled."); }
        }
        public void Disable()
        {
            if (m_radialIndicator != null) { m_radialIndicator.EndEffect(); m_radialIndicator = null; }
            enabled = false;
        }

        public void UpdateRadius(float newRadius)
        {
            radius = newRadius;
            m_radialIndicator.CurrentRadius = radius;
            m_radialIndicator.m_materialInst.SetFloat(m_radialIndicator.m_radiusID, radius);
        }

        public float radius;
        public Color colour;
        public bool emitsParticles;
        public virtual void OnEnabled() { }
        public virtual void TickOnEnemy(AIActor enemy) { }
        public virtual void EnemyEnteredCircle(AIActor enemy) { }
        public virtual void EnemyLeftCircle(AIActor enemy) { }

        private void Update()
        {
            if (enabled && !GameManager.Instance.IsLoadingLevel && GameManager.Instance.Dungeon != null)
            {
                ETGModConsole.Log("tick");
                for (int i = StaticReferenceManager.AllEnemies.Count - 1; i >= 0; i--)
                {
                    ETGModConsole.Log("actor check");
                    if (StaticReferenceManager.AllEnemies[i] != null)
                    {
                        ETGModConsole.Log("not null");
                        if (Vector2.Distance(StaticReferenceManager.AllEnemies[i].Position, base.gameObject.transform.position) <= radius)
                        {
                            ETGModConsole.Log("within radius");
                            if (!actorsInCircle.Contains(StaticReferenceManager.AllEnemies[i]))
                            {
                                ETGModConsole.Log("not recognised");
                                EnemyEnteredCircle(StaticReferenceManager.AllEnemies[i]);
                                actorsInCircle.Add(StaticReferenceManager.AllEnemies[i]);
                            }
                            TickOnEnemy(StaticReferenceManager.AllEnemies[i]);
                            ETGModConsole.Log("ticked");
                        }
                        else if (actorsInCircle.Contains(StaticReferenceManager.AllEnemies[i]))
                        {
                            ETGModConsole.Log("out of radius but recognised");
                            EnemyLeftCircle(StaticReferenceManager.AllEnemies[i]);
                            actorsInCircle.Remove(StaticReferenceManager.AllEnemies[i]);
                        }
                    }
                }
                for (int i = actorsInCircle.Count - 1; i >= 0; i--)
                {
                    ETGModConsole.Log("cleaning register");
                    if (actorsInCircle[i] == null) actorsInCircle.RemoveAt(i);
                }
            }
        }
        private bool enabled;
        private List<AIActor> actorsInCircle = new List<AIActor>();
        private HeatIndicatorController m_radialIndicator;


    }
}
