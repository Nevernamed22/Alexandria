using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
    //Includes inbuilt shadow bullet functionality.
   public class ImprovedHelixProjectile : Projectile
    {
        public ImprovedHelixProjectile()
        {
            this.helixWavelength = 3f;
            this.helixAmplitude = 1f;
            this.NumberInChain = 1;
            this.pauseLength = 0.2f;
            this.chainScaleMult = 1;
            this.overrideProjectile = null;
            this.randomChainMin = 1;
            this.randomChainMax = 4;
            this.randomiseChainNum = false;
            startInverted = false;
        }
        public void AdjustRightVector(float angleDiff)
        {
            if (!float.IsNaN(angleDiff))
            {
                this.m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * this.m_initialUpVector;
                this.m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * this.m_initialRightVector;
            }
        }
        public override void Start()
        {
            base.Start();
            if (SpawnShadowBulletsOnSpawn)
            {
                if (randomiseChainNum)
                {
                    int selectednum = UnityEngine.Random.Range(randomChainMin, randomChainMax + 1);
                    if (selectednum > 0) this.SpawnChainedShadowBullets(selectednum, pauseLength, chainScaleMult, overrideProjectile);
                }
                else this.SpawnChainedShadowBullets(NumberInChain, pauseLength, chainScaleMult, overrideProjectile);
            }

        }
        public override void Move()
        {
            if (!this.m_helixInitialized)
            {
                this.m_helixInitialized = true;
                this.m_initialRightVector = base.transform.right;
                this.m_initialUpVector = base.transform.up;
                this.m_privateLastPosition = base.sprite.WorldCenter;
                this.m_displacement = 0f;
                this.m_yDisplacement = 0f;
            }
            this.m_timeElapsed += BraveTime.DeltaTime;
            int num = (!startInverted) ? 1 : -1;
            float num2 = this.m_timeElapsed * this.baseData.speed;
            float num3 = (float)num * this.helixAmplitude * Mathf.Sin(this.m_timeElapsed * 3.1415927f * this.baseData.speed / this.helixWavelength);
            float d = num2 - this.m_displacement;
            float d2 = num3 - this.m_yDisplacement;
            Vector2 vector = this.m_privateLastPosition + this.m_initialRightVector * d + this.m_initialUpVector * d2;
            this.m_privateLastPosition = vector;
            Vector2 vector2 = (vector - base.sprite.WorldCenter) / BraveTime.DeltaTime;
            float num4 = BraveMathCollege.Atan2Degrees(vector2);
            if (this.shouldRotate && !float.IsNaN(num4))
            {
                base.transform.localRotation = Quaternion.Euler(0f, 0f, num4);
            }
            if (!float.IsNaN(num4))
            {
                this.m_currentDirection = vector2.normalized;
            }
            this.m_displacement = num2;
            this.m_yDisplacement = num3;
            base.specRigidbody.Velocity = vector2;
            base.LastVelocity = base.specRigidbody.Velocity;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public float helixWavelength;
        public float helixAmplitude;
        private bool m_helixInitialized;
        private Vector2 m_initialRightVector;
        private Vector2 m_initialUpVector;
        private Vector2 m_privateLastPosition;
        private float m_displacement;

        private float m_yDisplacement;

        public bool SpawnShadowBulletsOnSpawn;
        public Projectile overrideProjectile;
        public int NumberInChain;
        public float pauseLength;
        public float chainScaleMult;
        public bool randomiseChainNum;
        public int randomChainMin;
        public int randomChainMax;

        public bool startInverted;
    }
}
