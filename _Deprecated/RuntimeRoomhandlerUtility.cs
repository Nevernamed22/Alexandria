using System;
using System.Collections.Generic;
using Dungeonator;

namespace Alexandria.Misc
{
    [Obsolete("This class has been obsoleted by RoomUtility; it exists for backwards compatability only.", false)]
   public static class RuntimeRoomhandlerUtility
    {
        /// <summary>
        /// Returns a specified number of unique enemies in the target room. Returns a list of AIActors.
        /// </summary>
        /// <param name="room">The target room.</param>
        /// <param name="numOfEnemiesToReturn">How many enemies the returned list should contain. May return fewer than the specified amount if there are not enough valid enemies in the room.</param>
        /// <param name="reqForRoomClear">If true, will only return enemies which are required for room clear.</param>
        /// <param name="canReturnBosses">If false, returned list of AIActors cannot contain bosses.</param>
        [Obsolete("This method has been obsoleted by the equivalent in RoomUtility; it exists for backwards compatability only.", false)]
        public static List<AIActor> GetXEnemiesInRoom(this RoomHandler room, int numOfEnemiesToReturn, bool reqForRoomClear = true, bool canReturnBosses = true)
        {
            return DungeonAPI.RoomUtility.GetXEnemiesInRoom(room, numOfEnemiesToReturn, reqForRoomClear, canReturnBosses);
        }

        /// <summary>
        /// Returns true if the target room contains the Mine Flayer boss, or another AIActor (such as a bell) who is 'secretly' the Mine Flayer.
        /// </summary>
        /// <param name="room">The target room.</param>
        [Obsolete("This method has been obsoleted by the equivalent in RoomUtility; it exists for backwards compatability only.", false)]
        public static bool RoomContainsMineFlayer(this RoomHandler room)
        {
            return DungeonAPI.RoomUtility.RoomContainsMineFlayer(room);
        }
    }
}
