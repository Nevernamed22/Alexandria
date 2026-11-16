using Dungeonator;
using HarmonyLib;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public class BreachUtility
    {
        internal static bool regenerateRooms;
        internal static Dictionary<string, BreachRoomData> RegisteredRooms = new Dictionary<string, BreachRoomData>();
        internal static Dictionary<string, RoomHandler> ActiveRooms = new Dictionary<string, RoomHandler>();
        internal static List<BreachObjectData> nonSpecialRegisteredObjects;
        internal static Dictionary<string, List<BreachObjectData>> specialRegisteredObjects;

        public static bool OUTPUT_CONSOLE_LOG = true;
        /// <summary>
        /// Registers a new room to be created in the Breach when it is loaded.
        /// </summary>
        ///  /// <param name="id">A unique string identifier for the room being added. Other mods will use this string to organsie and find your room. Add your mod prefix to make it truly unique!</param>
        ///  /// <param name="coreObject">A prefab that will be spawned at 0,0 in the new room area. Should be the base object for all default objects making up the geometry of your room.</param>
        ///  /// <param name="dimensions">The dimensions of the new room space, in tiles. Be sure this encompasses everything you plan to put in your room, but does not greatly exceed it.</param>
        public static void RegisterNewBreachRoom(string id, GameObject coreObject, IntVector2 dimensions)
        {
            RegisteredRooms.Add(id,
                new BreachRoomData()
                {
                    sourceObject = coreObject,
                    dimensions = dimensions
                });
            if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Registered new room with ID \"{id}\""); }
        }
        /// <summary>
        /// Returns the currently loaded extra Breach Room with the given ID. Returns null if no room matching the ID is found.
        /// </summary>
        ///  /// <param name="id">The unique room identification string of the room being searched for.</param>
        public static RoomHandler GetRuntimeModdedBreachRoom(string id)
        {
            if (ActiveRooms.ContainsKey(id)) { return ActiveRooms[id]; }
            else return null;
        }

        /// <summary>
        /// Registers a given object to be placed in the Breach when it is fully loaded. Can be configured to place objects in additional modded rooms added by BreachUtility.
        /// </summary>
        ///  /// <param name="prefab">The GameObject being placed into the breach.</param>
        ///  /// <param name="position">The position of the GameObject relative to the ORIGIN POINT of it's given SourceRoomID. If no SourceRoomID is set, these coordinates are instead used as the absolute world position of the object.</param>
        ///  /// <param name="sourceRoomID">The unique identifier string for the source room for the object to appoear in. Can be unset, in which case the object will be placed based on absolute world position instead. </param>
        public static void RegisterNewBreachObject(GameObject prefab, Vector2 position, string sourceRoomID = null)
        {
            if (nonSpecialRegisteredObjects == null) { nonSpecialRegisteredObjects = new List<BreachObjectData>(); }
            if (specialRegisteredObjects == null) { specialRegisteredObjects = new Dictionary<string, List<BreachObjectData>>(); }
            if (!string.IsNullOrEmpty(sourceRoomID))
            {
                if (!specialRegisteredObjects.ContainsKey(sourceRoomID)) { specialRegisteredObjects.Add(sourceRoomID, new List<BreachObjectData>()); }
                specialRegisteredObjects[sourceRoomID].Add(new BreachObjectData
                {
                    prefab = prefab,
                    position = position,
                    sourceRoomID = sourceRoomID,
                });
                if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Registered new breach object \"{prefab.name}\" for modded Breach room ID \"{sourceRoomID}\""); }
            }
            else
            {
                nonSpecialRegisteredObjects.Add(new BreachObjectData
                {
                    prefab = prefab,
                    position = position,
                    sourceRoomID = sourceRoomID,
                });
                if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Registered new breach object \"{prefab.name}\", with absolute world coordinates."); }
            }
        }
        internal struct BreachRoomData
        {
            public BreachRoomData(GameObject sourceObj, IntVector2 dimension)
            {
                sourceObject = sourceObj;
                dimensions = dimension;
            }
            public GameObject sourceObject;
            public IntVector2 dimensions;
        }
        internal struct BreachObjectData
        {
            public BreachObjectData(GameObject sourceObj, Vector2 dimension, string id)
            {
                prefab = sourceObj;
                position = dimension;
                sourceRoomID = id;
            }
            public GameObject prefab;
            public Vector2 position;
            public string sourceRoomID;
        }

        private static void GenerateRuntimeRooms()
        {
            if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Beginning runtime Breach expansion..."); }
            ActiveRooms = new Dictionary<string, RoomHandler>();
            foreach (string name in RegisteredRooms.Keys)
            {
                BreachRoomData data = RegisteredRooms[name];
                RoomHandler addedRoom = GameManager.Instance.Dungeon.AddRuntimeRoom(data.dimensions, data.sourceObject);
                Pathfinder.Instance.InitializeRegion(GameManager.Instance.Dungeon.data, addedRoom.area.basePosition, addedRoom.area.dimensions);
                ActiveRooms.Add(name, addedRoom);
                if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Successfully expanded Breach with room \"{name}\""); }
            }
            if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Finished Breach expansion."); }
        }
        private static void RuntimeAddObjects()
        {
            if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Beginning runtime Breach object placement..."); }
            //Start with world position
            if (nonSpecialRegisteredObjects != null && nonSpecialRegisteredObjects.Count > 0)
            {
                foreach (BreachObjectData data in nonSpecialRegisteredObjects)
                {
                    UnityEngine.Object.Instantiate(data.prefab, data.position, Quaternion.identity);
                    if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Successfully added object \"{data.prefab.name}\" at absolute world coordinates ({data.position.x}, {data.position.y})"); }

                }
            }
            if (specialRegisteredObjects != null && specialRegisteredObjects.Count > 0)
            {
                foreach (string targetRoom in specialRegisteredObjects.Keys)
                {
                    RoomHandler target = GetRuntimeModdedBreachRoom(targetRoom);
                    if (target != null)
                    {
                        if (specialRegisteredObjects[targetRoom] != null && specialRegisteredObjects[targetRoom].Count > 0)
                        {
                            foreach (BreachObjectData data in specialRegisteredObjects[targetRoom])
                            {
                                
                                UnityEngine.Object.Instantiate(data.prefab, target.area.basePosition.ToVector2() + data.position, Quaternion.identity);
                                if (OUTPUT_CONSOLE_LOG) { Debug.Log($"ALEXANDRIA - Breach Utility - Successfully added object \"{data.prefab.name}\" at local coordinates ({data.position.x}, {data.position.y}) in room \"{targetRoom}\""); }
                            }
                        }
                    }
                    else
                    {
                        if (OUTPUT_CONSOLE_LOG) { Debug.LogWarning($"ALEXANDRIA - Breach Utility - Attempted to add objects to room \"{targetRoom}\", however room was not found in active rooms. Are you sure you spelled its ID correctly?"); }
                    }
                }
            }
        }

        /// <summary>Regenerates the annex rooms every time the Breach loads</summary>
        [HarmonyPatch(typeof(Foyer), nameof(Foyer.ProcessPlayerEnteredFoyer))]
        private class ProcessPlayerEnteredFoyerPatch
        {
            static void Postfix(Foyer __instance, PlayerController p)
            {
                if (!regenerateRooms)
                    return;

                GenerateRuntimeRooms();
                RuntimeAddObjects();

                regenerateRooms = false;
            }
        }

        /// <summary>Marks the annex rooms in need of regeneration every time the Breach is reloaded</summary>
        [HarmonyPatch(typeof(Foyer), nameof(Foyer.Start))]
        private class OnFoyerStartPatch
        {
            static void Postfix(Foyer __instance)
            {
                regenerateRooms = true;
            }
        }

    }
}
