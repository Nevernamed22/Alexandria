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
        public static Vector2 GetCenteredLookingPosForObj(this Vector2 originalValue, SpeculativeRigidbody rigidBody, bool centerX = true, bool centerY = false)
        {
            float UnitX = rigidBody.UnitDimensions.x;
            float ReturnX = originalValue.x;
            if (centerX) ReturnX -= (UnitX * 0.5f);

            float UnitY = rigidBody.UnitDimensions.y;
            float ReturnY = originalValue.y;
            if (centerY) ReturnY -= (UnitY * 0.5f);

            return new Vector2(ReturnX, ReturnY);
        }
        public static bool isEven(this float number)
        {
            if (number % 2 == 0) return true;
            else return false;
        }
        public static bool isEven(this int number)
        {
            if (number % 2 == 0) return true;
            else return false;
        }
        public static bool IsBetweenRange(this float numberToCheck, float bottom, float top)
        {
            return (numberToCheck >= bottom && numberToCheck <= top);
        }
        public static T KeyByValue<T, W>(this Dictionary<T, W> dict, W val)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dict)
            {
                if (EqualityComparer<W>.Default.Equals(pair.Value, val))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }
        public static class RandomEnum<T>
        {
            static T[] m_Values;
            static RandomEnum()
            {
                var values = System.Enum.GetValues(typeof(T));
                m_Values = new T[values.Length];
                for (int i = 0; i < m_Values.Length; i++)
                    m_Values[i] = (T)values.GetValue(i);
            }
            public static T Get()
            {
                return m_Values[UnityEngine.Random.Range(0, m_Values.Length)];
            }
        }
        public static bool PositionBetweenRelativeValidAngles(this Vector2 positionToCheck, Vector2 startPosition, float centerAngle, float range, float validAngleVariation)
        {
            if (Vector2.Distance(positionToCheck, startPosition) < range)
            {
                float num7 = BraveMathCollege.Atan2Degrees(positionToCheck - startPosition);
                float minRawAngle = Math.Min(validAngleVariation, -validAngleVariation);
                float maxRawAngle = Math.Max(validAngleVariation, -validAngleVariation);
                bool isInRange = false;
                float actualMaxAngle = centerAngle + maxRawAngle;
                float actualMinAngle = centerAngle + minRawAngle;

                if (num7.IsBetweenRange(actualMinAngle, actualMaxAngle)) isInRange = true;
                if (actualMaxAngle > 180)
                {
                    float Overflow = actualMaxAngle - 180;
                    if (num7.IsBetweenRange(-180, (-180 + Overflow))) isInRange = true;
                }
                if (actualMinAngle < -180)
                {
                    float Underflow = actualMinAngle + 180;
                    if (num7.IsBetweenRange((180 + Underflow), 180)) isInRange = true;
                }
                return isInRange;
            }
            return false;
        }
    }
}
