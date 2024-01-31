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

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("checkCustomObjects", delegate (string[] args)
            {
                ETGModConsole.Log("\nCustom Objects List Start:\n=====");
                foreach (var thing in StaticReferences.customObjects)
                {
                    ETGModConsole.Log($"{thing.Key} - {thing.Value.name}");
                }
                ETGModConsole.Log("=====\nCustom Objects List End\n=====");
            });
            ETGModConsole.CommandDescriptions.Add("alexandria checkCustomObjects", "Lists every gameObject stored in StaticReferences.customObjects.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("checkCustomPlaceables", delegate (string[] args)
            {
                ETGModConsole.Log("\nCustom Placeables List Start:\n=====");
                foreach (var thing in StaticReferences.customPlaceables)
                {
                    ETGModConsole.Log($"{thing.Key} - {thing.Value.name}");
                }
                ETGModConsole.Log("=====\nCustom Placeables List End\n=====");
            });
            ETGModConsole.CommandDescriptions.Add("alexandria checkCustomPlaceables", "Lists every DungeonPlaceable stored in StaticReferences.customPlaceables.");


            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("checkStaticObjects", delegate (string[] args)
            {
                ETGModConsole.Log("\nStatic Objects List Start:\n=====");
                foreach (var thing in SetupExoticObjects.objects)
                {
                    ETGModConsole.Log($"{thing.Key} - {thing.Value.name}");
                }
                ETGModConsole.Log("=====\nStatic Objects List End\n=====");
            });
            ETGModConsole.CommandDescriptions.Add("alexandria checkStaticObjects", "Lists every stored gameObject inSetupExoticObjects.objects.");



            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("getRoomName", delegate (string[] args)
            {
                RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
                ETGModConsole.Log(currentRoom.GetRoomName());
            });
            ETGModConsole.CommandDescriptions.Add("alexandria getRoomName", "Logs the name of the primary players current room.");

            /*
            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("spawnAssigned", delegate (string[] args)
            {
                UnityEngine.Object.Instantiate(SetupExoticObjects.ShopLayout, GameManager.Instance.PrimaryPlayer.sprite.WorldCenter, UnityEngine.Quaternion.identity);
            });
            */

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("loadNPCParadise", delegate (string[] args)
            {
                GameManager.Instance.LoadCustomFlowForDebug("NPCParadise", "Base_Castle", "tt_castle");
            });
            ETGModConsole.CommandDescriptions.Add("alexandria loadNPCParadise", "Loads a special debug flow that contains every basegame NPC and their variants.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("testpickupSpawn", delegate (string[] args)
            {
                RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
                GameManager.Instance.StartCoroutine(H(currentRoom));
               
            });
            ETGModConsole.CommandDescriptions.Add("alexandria testpickupSpawn", "Spawns 64 pickups on the floor. Used to check for valid pickup spawn positions.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("debugflow", (args) =>
            {
                DungeonHandler.debugFlow = !DungeonHandler.debugFlow;
                string status = DungeonHandler.debugFlow ? "enabled" : "disabled";
                string color = DungeonHandler.debugFlow ? "00FF00" : "FF0000";
                DebugUtility.Print($"Debug flow {status}", color, true);
            });
            ETGModConsole.CommandDescriptions.Add("alexandria debugflow", "Toggles the debugflow, which loads every custom room into one large pathway.\n--(Takes very long to generate if there are too many custom rooms / may not generate at all).");

            //This is useful for figuring out where you want your shrine to go in the breach
            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("getpos", (args) =>
            {
                ETGModConsole.Log("Player position: " + GameManager.Instance.PrimaryPlayer.transform.position);
                ETGModConsole.Log("Player center: " + GameManager.Instance.PrimaryPlayer.sprite.WorldCenter);
            });
            ETGModConsole.CommandDescriptions.Add("alexandria getpos", "Logs the primary players current position and center.");



            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("hidehitboxes", (args) => HitboxMonitor.DeleteHitboxDisplays());
            ETGModConsole.CommandDescriptions.Add("alexandria hidehitboxes", "Hides the hitboxes of nearby SpeculativeRigidbodies, if they have already been shown.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("showhitboxes", (args) =>
            {
                foreach (var obj in GameObject.FindObjectsOfType<SpeculativeRigidbody>())
                {
                    if (obj && obj.sprite && Vector2.Distance(obj.sprite.WorldCenter, GameManager.Instance.PrimaryPlayer.sprite.WorldCenter) < 20)
                    {
                        DebugUtility.Log(obj?.name);
                        HitboxMonitor.DisplayHitbox(obj);
                    }
                }
            });
            ETGModConsole.CommandDescriptions.Add("alexandria showhitboxes", "Shows the hitboxes of nearby SpeculativeRigidbodies.");


            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("spawnObject_Custom", x => { if (x.Length <= 0) { DebugUtility.Log("No Object Given!"); return; } ObjectCheck(x[0], false); }, ETGModConsole.AutocompletionFromCollectionGetter(() => DungeonAPI.StaticReferences.customObjects.Keys));
            ETGModConsole.CommandDescriptions.Add("alexandria spawnObject_Custom", "Spawns a given gameObject from the StaticReferences.customObjects list.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("spawnPlaceable_Custom", x => { if (x.Length <= 0) { DebugUtility.Log("No Placeable Given!"); return; } PlaceableCheck(x[0], false); }, ETGModConsole.AutocompletionFromCollectionGetter(() => DungeonAPI.StaticReferences.customPlaceables.Keys));
            ETGModConsole.CommandDescriptions.Add("alexandria spawnPlaceable_Custom", "Spawns a given dungeonPlaceable from the StaticReferences.customPlaceables list.");

            ETGModConsole.Commands.GetGroup("alexandria").AddUnit("spawnStored_Object", x => { if (x.Length <= 0) { DebugUtility.Log("No Stored Object Given!"); return; } StoredObject(x[0], false); }, ETGModConsole.AutocompletionFromCollectionGetter(() => DungeonAPI.SetupExoticObjects.objects.Keys));
            ETGModConsole.CommandDescriptions.Add("alexandria spawnStored_Object", "Spawns a given gameObject from the SetupExoticObjects.objects list.");

        }

        public static void StoredObject(string level, bool ignoreDictionary)
        {
            string sceneName = level;


            if (DungeonAPI.SetupExoticObjects.objects.ContainsKey(sceneName) && !ignoreDictionary)
            {
                if (GameManager.Instance.PrimaryPlayer != null)
                {
                    var objectSpawned = UnityEngine.Object.Instantiate(DungeonAPI.SetupExoticObjects.objects[sceneName], GameManager.Instance.PrimaryPlayer.transform.position, Quaternion.identity);
                    var mainBody = objectSpawned.GetComponent<SpeculativeRigidbody>();
                    if (mainBody != null)
                    {
                        mainBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                    }
                    var childBody = objectSpawned.GetComponentInChildren<SpeculativeRigidbody>();
                    if (childBody != null)
                    {
                        childBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                    }
                }
                else
                {
                    ETGModConsole.Log("Failed to place Object: " + level + " because Player 1 is NULL");
                }
            }
            else
            {
                ETGModConsole.Log("Failed to place Object: " + level);
            }
        }


        public static void ObjectCheck(string level, bool ignoreDictionary)
        {
            string sceneName = level;


            if (DungeonAPI.StaticReferences.customObjects.ContainsKey(sceneName) && !ignoreDictionary)
            {
                if (GameManager.Instance.PrimaryPlayer != null)
                {
                    var objectSpawned = UnityEngine.Object.Instantiate(DungeonAPI.StaticReferences.customObjects[sceneName], GameManager.Instance.PrimaryPlayer.transform.position, Quaternion.identity);
                    var mainBody =  objectSpawned.GetComponent<SpeculativeRigidbody>();
                    if (mainBody != null)
                    {
                        mainBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                    }
                    var childBody = objectSpawned.GetComponentInChildren<SpeculativeRigidbody>();
                    if (childBody != null)
                    {
                        childBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                    }
                }
                else
                {
                    ETGModConsole.Log("Failed to place Object: " + level + " because Player 1 is NULL");
                }
            }
            else
            {
                ETGModConsole.Log("Failed to place Object: " + level);
            }
        }


        public static void PlaceableCheck(string level, bool ignoreDictionary)
        {
            string sceneName = level;


            if (DungeonAPI.StaticReferences.customPlaceables.ContainsKey(sceneName) && !ignoreDictionary)
            {
                if (GameManager.Instance.PrimaryPlayer != null)
                {
                    if (GameManager.Instance.PrimaryPlayer.CurrentRoom == null)
                    {
                        ETGModConsole.Log("Failed to place Object: " + level + " because Player 1 is NOT in a room! DungeonPlaceables require a room.");

                    }
                    else
                    {
                        var o = GameManager.Instance.PrimaryPlayer.transform.position;
                        var objectSpawned = DungeonAPI.StaticReferences.customPlaceables[sceneName].InstantiateObject(GameManager.Instance.PrimaryPlayer.CurrentRoom, new IntVector2((int)o.x, (int)o.y) - GameManager.Instance.PrimaryPlayer.CurrentRoom.area.basePosition);
                        var mainBody = objectSpawned.GetComponent<SpeculativeRigidbody>();
                        if (mainBody != null)
                        {
                            mainBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                        }
                        var childBody = objectSpawned.GetComponentInChildren<SpeculativeRigidbody>();
                        if (childBody != null)
                        {
                            childBody.RegisterTemporaryCollisionException(GameManager.Instance.PrimaryPlayer.specRigidbody, 1, 5);
                        }
                    }

                   
                    
                }
                else
                {
                    ETGModConsole.Log("Failed to place Object: " + level + " because Player 1 is NULL");
                }
            }
            else
            {
                ETGModConsole.Log("Failed to place Object: " + level);
            }
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
