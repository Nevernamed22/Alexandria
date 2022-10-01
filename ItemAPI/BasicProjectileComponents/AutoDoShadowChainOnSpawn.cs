using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.Misc;

namespace Alexandria.ItemAPI
{
    public class AutoDoShadowChainOnSpawn : MonoBehaviour
    {
        public AutoDoShadowChainOnSpawn()
        {
            this.NumberInChain = 1;
            this.pauseLength = 0.2f;
            this.chainScaleMult = 1;
            this.overrideProjectile = null;
            this.randomChainMin = 1;
            this.randomChainMax = 4;
            this.randomiseChainNum = false;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();

            if (randomiseChainNum)
            {
                int selectednum = UnityEngine.Random.Range(randomChainMin, randomChainMax + 1);
                if (selectednum > 0) this.m_projectile.SpawnChainedShadowBullets(selectednum, pauseLength, chainScaleMult, overrideProjectile);
            }
            else this.m_projectile.SpawnChainedShadowBullets(NumberInChain, pauseLength, chainScaleMult, overrideProjectile);
        }
        public Projectile overrideProjectile;
        public int NumberInChain;
        public float pauseLength;
        public float chainScaleMult;
        public bool randomiseChainNum;
        public int randomChainMin;
        public int randomChainMax;
        private Projectile m_projectile;
    }
}
