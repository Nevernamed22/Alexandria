using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
        public static Vector2 RadianToVector2(this float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }
        public static Vector2 DegreeToVector2(this float degree)
        {
            return (degree * Mathf.Deg2Rad).RadianToVector2();
        }
        public static Vector2 DegreeToVector2(this int degree)
        {
            return (degree * Mathf.Deg2Rad).RadianToVector2();
        }
    }
}
