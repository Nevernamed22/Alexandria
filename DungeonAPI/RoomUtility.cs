using Alexandria.EnemyAPI;
using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria.DungeonAPI
{
    public static class RoomUtility
    {
        public static bool EnableDebugLogging = false;

        /// <summary>
        /// Returns a specified number of unique enemies in the target room. Returns a list of AIActors.
        /// </summary>
        /// <param name="room">The target room.</param>
        /// <param name="numOfEnemiesToReturn">How many enemies the returned list should contain. May return fewer than the specified amount if there are not enough valid enemies in the room.</param>
        /// <param name="reqForRoomClear">If true, will only return enemies which are required for room clear.</param>
        /// <param name="canReturnBosses">If false, returned list of AIActors cannot contain bosses.</param>
        public static List<AIActor> GetXEnemiesInRoom(this RoomHandler room, int numOfEnemiesToReturn, bool reqForRoomClear = true, bool canReturnBosses = true)
        {
            if (numOfEnemiesToReturn <= 0)
                return null;

            List<AIActor> activeEnemies = room.GetActiveEnemies(reqForRoomClear ? RoomHandler.ActiveEnemyType.RoomClear : RoomHandler.ActiveEnemyType.All);
            if (activeEnemies == null)
                return null;

            if (!canReturnBosses)
            {
                for (int i = 0; i < activeEnemies.Count; i++)
                    if (activeEnemies[i] && activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
                        activeEnemies.RemoveAt(i);
            }
            if (activeEnemies.Count <= numOfEnemiesToReturn)
                return activeEnemies;

            List<AIActor> pickedEnemies = new List<AIActor>();
            for (int i = 0; i < numOfEnemiesToReturn; i++)
            {
                AIActor actor = BraveUtility.RandomElement(activeEnemies);
                pickedEnemies.Add(actor);
                activeEnemies.Remove(actor);
            }
            return pickedEnemies;
        }

        private static List<AIActor> _TempEnemies = new List<AIActor>();
        /// <summary>
        /// Returns true if the target room contains the Mine Flayer boss, or another AIActor (such as a bell) who is 'secretly' the Mine Flayer.
        /// </summary>
        /// <param name="room">The target room.</param>
        public static bool RoomContainsMineFlayer(this RoomHandler room)
        {
            room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref _TempEnemies);
            foreach (AIActor enemy in _TempEnemies)
                if (enemy && enemy.EnemyGuid == EnemyGUIDs.Mine_Flayer_GUID)
                    return true;
            return false;
        }

        public static void RegenerateMapTilemap(this Minimap self)
        {
            GameManager.Instance.StartCoroutine(DoLateRegeneration(self));
        }

        private static IEnumerator DoLateRegeneration(Minimap instance)
        {
            yield return null;
            instance.m_shouldBuildTilemap = true;
            yield break;
        }
    }
}
