using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
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
        public static AIActor GetNearestEnemyToPosition(this Vector2 position, bool checkIsWorthShootingAt = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, Func<AIActor, bool> overrideValidityCheck = null)
        {
            Func<AIActor, bool> isValid = (AIActor a) => a && a.HasBeenEngaged && a.healthHaver && a.healthHaver.IsVulnerable && a.healthHaver.IsAlive && ((checkIsWorthShootingAt && a.IsWorthShootingAt) || !checkIsWorthShootingAt);
            if (overrideValidityCheck != null) isValid = overrideValidityCheck;
            AIActor closestToPosition = BraveUtility.GetClosestToPosition<AIActor>(GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(position.ToIntVector2()).GetActiveEnemies(type), position, isValid, new AIActor[] { });
            if (closestToPosition) return closestToPosition;
            else return null;
        }
        public static AIActor GetNearestEnemyToPosition(this IntVector2 position, bool checkIsWorthShootingAt = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, Func<AIActor, bool> overrideValidityCheck = null)
        {
            Func<AIActor, bool> isValid = (AIActor a) => a && a.HasBeenEngaged && a.healthHaver && a.healthHaver.IsVulnerable && a.healthHaver.IsAlive && ((checkIsWorthShootingAt && a.IsWorthShootingAt) || !checkIsWorthShootingAt);
            if (overrideValidityCheck != null) isValid = overrideValidityCheck;
            AIActor closestToPosition = BraveUtility.GetClosestToPosition<AIActor>(GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(position).GetActiveEnemies(type), (Vector2)position, isValid, new AIActor[] { });
            if (closestToPosition) return closestToPosition;
            else return null;
        }
        
    }
}
