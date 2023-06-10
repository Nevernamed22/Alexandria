using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.EnemyAPI;

namespace Alexandria.Misc
{
    //Runtime Roomhandler Utility is for methods and extensions related to rooms which may be useful at runtime, but which ARE NOT related to the actual setup of the room
   public static class RuntimeRoomhandlerUtility
    {
        /// <summary>
        /// Returns a specified number of unique enemies in the target room. Returns a list of AIActors.
        /// </summary>
        /// <param name="room">The target room.</param>
        /// <param name="numOfEnemiesToReturn">How many enemies the returned list should contain. May return fewer than the specified amount if there are not enough valid enemies in the room.</param>
        /// <param name="reqForRoomClear">If true, will only return enemies which are required for room clear.</param>
        /// <param name="canReturnBosses">If false, returned list of AIActors cannot contain bosses.</param>
        public static List<AIActor> GetXEnemiesInRoom(this RoomHandler room, int numOfEnemiesToReturn, bool reqForRoomClear = true, bool canReturnBosses = true)
        {
            if (numOfEnemiesToReturn <= 0) return null;
            RoomHandler.ActiveEnemyType type = RoomHandler.ActiveEnemyType.All;
            if (reqForRoomClear) type = RoomHandler.ActiveEnemyType.RoomClear;
            List<AIActor> activeEnemies = room.GetActiveEnemies(type);
            if (activeEnemies != null)
            {
                if (!canReturnBosses)
                {
                    for (int i = 0; i < activeEnemies.Count; i++)
                    {
                        if (activeEnemies[i] && activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss) activeEnemies.RemoveAt(i);
                    }
                }
                if (activeEnemies.Count > numOfEnemiesToReturn)
                {
                    List<AIActor> pickedEnemies = new List<AIActor>();
                    for (int i = 0; i < numOfEnemiesToReturn; i++)
                    {
                        AIActor actor = BraveUtility.RandomElement(activeEnemies);
                        pickedEnemies.Add(actor);
                        activeEnemies.Remove(actor);
                    }
                    return pickedEnemies;
                }
                else
                {
                    return activeEnemies;
                }
            }
            else return null;
        }

        /// <summary>
        /// Returns true if the target room contains the Mine Flayer boss, or another AIActor (such as a bell) who is 'secretly' the Mine Flayer.
        /// </summary>
        /// <param name="room">The target room.</param>
        public static bool RoomContainsMineFlayer(this RoomHandler room)
        {
            bool foundFlayer = false;
            if (room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) > 0)
            {
                foreach (AIActor enemy in room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All))
                {
                    if (enemy)
                    {
                        if (enemy.EnemyGuid == "8b0dd96e2fe74ec7bebc1bc689c0008a" || enemy.IsSecretlyTheMineFlayer()) foundFlayer = true;
                    }
                }
            }
            return foundFlayer;
        }
    }
}
