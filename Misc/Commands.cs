//using Alexandria.SaveAPI;
using Alexandria.DungeonAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LootEngine;
using System.Collections;

namespace Alexandria.Misc
{
    class Commands
    {
        public static void Init()
        {
            ETGModConsole.Commands.AddGroup("alexandria");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("logAllFlags", delegate (string[] args)
            {
                /*foreach (var save in SaveAPIManager.AdvancedGameSaves)
                {
                    ETGModConsole.Log($"--=== {save.Key.ToUpper()} ===---");
                    var inst = AdvancedGameStatsManager.GetInstance(save.Key);
                    foreach (var flag in inst.m_flags)
                    {
                        //flag.ToString()
                        ETGModConsole.Log($"    {flag}");
                    }
                }
                ETGModConsole.Log($"--=== END ===---");*/
            });

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("checkStaticObjects", delegate (string[] args)
            {
                foreach (var thing in StaticReferences.customObjects)
                {
                    ETGModConsole.Log($"{thing.Key} - {thing.Value.name}");
                }
                
            });

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("getRoomName", delegate (string[] args)
            {
                RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
                ETGModConsole.Log(currentRoom.GetRoomName());
            });

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("spawnAssigned", delegate (string[] args)
            {
                UnityEngine.Object.Instantiate(SetupExoticObjects.ShopLayout, GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, UnityEngine.Quaternion.identity);
            });

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("loadNPCParadise", delegate (string[] args)
            {
                GameManager.Instance.LoadCustomFlowForDebug("NPCParadise", "Base_Castle", "tt_castle");
            });
            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("debugpickupSpawn", delegate (string[] args)
            {
                RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
                GameManager.Instance.StartCoroutine(H(currentRoom));
               
            });
        }
        public static IEnumerator H(RoomHandler r)
        {
            for (int i = 0; i < 64; i++)
            {
                IntVector2 bestRewardLocation = r.GetBestRewardLocation(new IntVector2(1, 1), r.GetRandomAvailableCell().Value.ToCenterVector2(),  true);
                string path = "Ammo_Pickup_Spread";
                UnityEngine.Object.Destroy(LootEngine.SpawnItem((GameObject)BraveResources.Load(path, ".prefab"), bestRewardLocation.ToVector3(), Vector2.up, 1f, true, true, false).GetComponent<DebrisObject>());
                yield return null;
            }
            yield break;
        }
    }
}
