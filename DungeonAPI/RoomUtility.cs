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
                        if (activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss) activeEnemies.RemoveAt(i);
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
        public static void RegenerateMapTilemap(this Minimap self)
        {
            GameManager.Instance.StartCoroutine(DoLateRegeneration(self));
        }
        private static IEnumerator DoLateRegeneration(Minimap instance)
        {
            yield return null;
            FieldInfo field = typeof(Minimap).GetField("m_shouldBuildTilemap", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(instance, true);
            yield break;
        }
    }
}
