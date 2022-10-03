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
        //Vector related methods and extensions

        /// <summary>
        /// Returns a Vector2 value corresponding to the direction between the first and second positions. Useful for trajectories.
        /// </summary>
        /// <param name="startVector">The starting position in the trajectory.</param>
        /// <param name="endVector">The ending position in the trajectory.</param>
        public static Vector2 CalculateVectorBetween(this Vector2 startVector, Vector2 endVector)
        {
            Vector2 dirVec = endVector - startVector;
            return dirVec;
        }

        /// <summary>
        /// Returns a Vector2 value corresponding to the direction between the first and second positions. Useful for trajectories.
        /// </summary>
        /// <param name="startVector">The starting position in the trajectory.</param>
        /// <param name="endVector">The ending position in the trajectory.</param>
        public static Vector2 CalculateVectorBetween(this Vector3 startVector, Vector3 endVector)
        {
            Vector2 dirVec = endVector - startVector;
            return dirVec;
        }

        /// <summary>
        /// Converts a given radian into a Vector2.
        /// </summary>
        /// <param name="radian">The radian to convert.</param>
        public static Vector2 RadianToVector2(this float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        /// <summary>
        /// Converts an angle degree into a Vector2.
        /// </summary>
        /// <param name="degree">The degree to convert.</param>
        public static Vector2 DegreeToVector2(this float degree)
        {
            return (degree * Mathf.Deg2Rad).RadianToVector2();
        }

        /// <summary>
        /// Converts an angle degree into a Vector2.
        /// </summary>
        /// <param name="degree">The degree to convert.</param>
        public static Vector2 DegreeToVector2(this int degree)
        {
            return (degree * Mathf.Deg2Rad).RadianToVector2();
        }

        //Purely numerical methods and extensions

        /// <summary>
        /// Returns true if the given float is even.
        /// </summary>
        /// <param name="number">The float to check.</param>
        public static bool isEven(this float number)
        {
            if (number % 2 == 0) return true;
            else return false;
        }

        /// <summary>
        /// Returns true if the given integer is even.
        /// </summary>
        /// <param name="number">The integer to check.</param>
        public static bool isEven(this int number)
        {
            if (number % 2 == 0) return true;
            else return false;
        }

        /// <summary>
        /// Returns true if the given float is between the provided number range.
        /// </summary>
        /// <param name="numberToCheck">The float to check.</param>
        /// <param name="bottom">The low end of the given range to check.</param>
        /// <param name="top">The high end of the given range to check.</param>
        public static bool IsBetweenRange(this float numberToCheck, float bottom, float top)
        {
            return (numberToCheck >= bottom && numberToCheck <= top);
        }

        /// <summary>
        /// Takes a float which represents an angle, and returns that same angle with a random degree of variance, similarly to gun accuracy.
        /// </summary>
        /// <param name="startFloat">The starting angle.</param>
        /// <param name="variance">The amount by which the starting angle may vary in either direction.</param>
        /// <param name="playerToScaleAccuracyOff">If set, the amount of variance will be affected by the given player's accuracy stat.</param>
        public static float GetAccuracyAngled(float startFloat, float variance, PlayerController playerToScaleAccuracyOff = null)
        {
            if (playerToScaleAccuracyOff != null) variance *= playerToScaleAccuracyOff.stats.GetStatValue(PlayerStats.StatType.Accuracy);
            float finalVariance = UnityEngine.Random.Range(variance * -1, variance);
            return startFloat + finalVariance;
        }

        //Loot / Item related methods and extensions

        /// <summary>
        /// Takes a list of item IDs. Removes 'invalid' entries based on set criteria, and returns the modified list.
        /// </summary>
        /// <param name="starterList">The list to be checked. Should only contain existing item IDs.</param>
        /// <param name="checkPlayerInventories">Removes item IDs that are currently in a player's inventory.</param>
        /// <param name="checkUnlocked">Removes item IDs which have not yet been unlocked.</param>
        public static List<int> RemoveInvalidIDListEntries(this List<int> starterList, bool checkPlayerInventories = true, bool checkUnlocked = true)
        {
            List<int> returnList = new List<int>();
            returnList.AddRange(starterList);
            for (int i = returnList.Count; i > 0; i--)
            {
                int ID = returnList[i - 1];
                if (checkPlayerInventories)
                {
                    if (GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.HasPickupID(ID))
                    {
                        returnList.RemoveAt(i - 1);
                    }
                    else if (GameManager.Instance.SecondaryPlayer && GameManager.Instance.SecondaryPlayer.HasPickupID(ID))
                    {
                        returnList.RemoveAt(i - 1);
                    }
                }
                if (checkUnlocked)
                {
                    PickupObject itemByID = PickupObjectDatabase.GetById(ID);
                    if (!itemByID.PrerequisitesMet())
                    {
                        returnList.RemoveAt(i - 1);
                    }
                }
            }
            return returnList;
        }

        /// <summary>
        /// Returns a random item quality between D and S, with decreasing probability. Configurable probability. If one tier chance is set, all must be set. D tier is not set, as it is the default if no other tiers are chosen.
        /// </summary>
        /// <param name="dat">The pickupobjectdatabase.</param>
        /// <param name="cChance">The chance for the returned item to be C Tier.</param>
        /// <param name="bChance">The chance for the returned item to be B Tier.</param>
        /// <param name="aChance">The chance for the returned item to be A Tier.</param>
        /// <param name="sChance">The chance for the returned item to be S Tier.</param>
        public static PickupObject.ItemQuality GetRandomQuality(this PickupObjectDatabase dat, float cChance = 0.32f, float bChance = 0.2f, float aChance = 0.09f, float sChance = 0.04f)
        {
            float random = UnityEngine.Random.value;
            if (random <= sChance) { return PickupObject.ItemQuality.S; }
            else if (random <= aChance) { return PickupObject.ItemQuality.A; }
            else if (random <= bChance) { return PickupObject.ItemQuality.B; }
            else if (random <= cChance) { return PickupObject.ItemQuality.C; }
            else { return PickupObject.ItemQuality.D; }
        }

        //Enemy Position Related Extensions

        /// <summary>
        /// Returns the AIActor of the nearest enemy to the Vector2 position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="checkIsWorthShootingAt">If true, will ignore enemies such as Mountain Cubes.</param>
        /// <param name="type">Controls whether or not the check should ignore enemies who are not required for room clear.</param>
        /// <param name="overrideValidityCheck">A func which allows the manual checking of custom parameters for enemy validity.</param>
        public static AIActor GetNearestEnemyToPosition(this Vector2 position, bool checkIsWorthShootingAt = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, Func<AIActor, bool> overrideValidityCheck = null)
        {
            Func<AIActor, bool> isValid = (AIActor a) => a && a.HasBeenEngaged && a.healthHaver && a.healthHaver.IsVulnerable && a.healthHaver.IsAlive && ((checkIsWorthShootingAt && a.IsWorthShootingAt) || !checkIsWorthShootingAt);
            if (overrideValidityCheck != null) isValid = overrideValidityCheck;
            AIActor closestToPosition = BraveUtility.GetClosestToPosition<AIActor>(GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(position.ToIntVector2()).GetActiveEnemies(type), position, isValid, new AIActor[] { });
            if (closestToPosition) return closestToPosition;
            else return null;
        }
      
        /// <summary>
        /// Returns the AIActor of the nearest enemy to the IntVector2 position.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <param name="checkIsWorthShootingAt">If true, will ignore enemies such as Mountain Cubes.</param>
        /// <param name="type">Controls whether or not the check should ignore enemies who are not required for room clear.</param>
        /// <param name="overrideValidityCheck">A func which allows the manual checking of custom parameters for enemy validity.</param>
        public static AIActor GetNearestEnemyToPosition(this IntVector2 position, bool checkIsWorthShootingAt = true, RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.RoomClear, Func<AIActor, bool> overrideValidityCheck = null)
        {
            Func<AIActor, bool> isValid = (AIActor a) => a && a.HasBeenEngaged && a.healthHaver && a.healthHaver.IsVulnerable && a.healthHaver.IsAlive && ((checkIsWorthShootingAt && a.IsWorthShootingAt) || !checkIsWorthShootingAt);
            if (overrideValidityCheck != null) isValid = overrideValidityCheck;
            AIActor closestToPosition = BraveUtility.GetClosestToPosition<AIActor>(GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(position).GetActiveEnemies(type), (Vector2)position, isValid, new AIActor[] { });
            if (closestToPosition) return closestToPosition;
            else return null;
        }
              
        //Misc

        /// <summary>
        /// Searches a dictionary for a specific value, and returns the corresponding key. Essentially a reverse dictionary search.
        /// </summary>
        /// <param name="dict">The dictionary to be searched.</param>
        /// <param name="val">The value being searched for.</param>
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

        /// <summary>
        /// Returns a position relative to the initial Vector2 where the provided SpeculativeRigidBody may be spawned such that it's center will be centered upon the original position.
        /// </summary>
        /// <param name="originalValue">The original position, where the given rigidbody is desired to be centered upon.</param>
        /// <param name="rigidBody">The rigidbody whose dimensions are being compensated for.</param>
        /// <param name="centerX">If true, will return a position which will center the rigidbody along the X coordinate.</param>
        /// <param name="centerY">If true, will return a position which will center the rigidbody along the Y coordinate.</param>
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
    //Enums & Enum Stuff
    public enum ThreeStateValue
    {
        FORCEYES,
        FORCENO,
        UNSPECIFIED
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

        /// <summary>
        /// Returns a random value of the Enum.
        /// </summary>
        public static T Get()
        {
            return m_Values[UnityEngine.Random.Range(0, m_Values.Length)];
        }
    }
}
