using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Helpers.Misc
{
   public static class MathsAndLogicHelper
    {
        public static float GetAccuracyAngled(float startFloat, float variance, PlayerController playerToScaleAccuracyOff = null)
        {
            if (playerToScaleAccuracyOff != null) variance *= playerToScaleAccuracyOff.stats.GetStatValue(PlayerStats.StatType.Accuracy);
            float positiveVariance = variance * 0.5f;
            float negativeVariance = positiveVariance * -1f;
            float finalVariance = UnityEngine.Random.Range(negativeVariance, positiveVariance);
            return startFloat + finalVariance;
        }
    }
}
