using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Misc
{
    public static class LootUtility
    {

        public static Dictionary<string, List<int>> tags = new Dictionary<string, List<int>>(); 

        public static void SetTag(this PickupObject item, string tag)
        {
            if (!tags.ContainsKey(tag))
            {
                tags.Add(tag, new List<int>());
            }
            tags[tag].Add(item.PickupObjectId);
        }


        public static bool HasTag(this PickupObject item, string tag)
        {
            if (!tags.ContainsKey(tag))
            {
                return false;
            }
            return (tags[tag].Contains(item.PickupObjectId));
            
        }

        public static List<int> GetAllItemsIdsWithTag(string tag)
        {
            if (!tags.ContainsKey(tag))
            {
                return new List<int>();
            }
            return tags[tag];
        }

        public static List<PickupObject> GetAllItemsWithTag(string tag)
        {
            if (!tags.ContainsKey(tag))
            {
                return new List<PickupObject>();
            }
            List<PickupObject> pickupObjects = new List<PickupObject>();
            foreach (var id in tags[tag]) { if (PickupObjectDatabase.GetById(id) != null) pickupObjects.Add(PickupObjectDatabase.GetById(id)); }

            return pickupObjects;
        }


        /// <summary>
        /// Creates a new blank loot table
        /// </summary>
        /// <param name="includedLootTables">i think this litterally dose fuck all</param> 
        /// <param name="prerequisites">the prerequisites of the loot table... whatever the fuck that means</param>
        /// <returns></returns>
        public static GenericLootTable CreateLootTable(List<GenericLootTable> includedLootTables = null, DungeonPrerequisite[] prerequisites = null)
        {
            var lootTable = ScriptableObject.CreateInstance<GenericLootTable>();
            lootTable.defaultItemDrops = new WeightedGameObjectCollection()
            {
                elements = new List<WeightedGameObject>()
            };


            if (prerequisites != null)
            {
                lootTable.tablePrerequisites = prerequisites;
            }
            else
            {
                lootTable.tablePrerequisites = new DungeonPrerequisite[0];
            }

            if (includedLootTables != null)
            {
                lootTable.includedLootTables = includedLootTables;
            }
            else
            {
                lootTable.includedLootTables = new List<GenericLootTable>();
            }


            return lootTable;
        }

        /// <summary>
        /// Adds an item to a loot table via PickupObject
        /// </summary>
        /// <param name="lootTable">The loot table you want to add to</param> 
        /// <param name="po">The PickupObject you're adding</param>
        /// <param name="weight">The Weight of the item you're adding (default is 1)</param>
        /// <returns></returns>
        public static void AddItemToPool(this GenericLootTable lootTable, PickupObject po, float weight = 1)
        {
            lootTable.defaultItemDrops.Add(new WeightedGameObject()
            {
                pickupId = po.PickupObjectId,
                weight = weight,
                rawGameObject = po.gameObject,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
        }

        /// <summary>
        /// Adds an item to a loot table via PickupObject
        /// </summary>
        /// <param name="lootTable">The loot table you want to add to</param> 
        /// <param name="po">The PickupObject you're adding</param>
        /// <param name="weight">The Weight of the item you're adding (default is 1)</param>
        /// <returns></returns>
        public static void AddItemToPool(this GenericLootTable lootTable, params int[] items)
        {
            foreach (int id in items)
            {
                var po = PickupObjectDatabase.GetById(id);
                lootTable.defaultItemDrops.Add(new WeightedGameObject()
                {
                    pickupId = po.PickupObjectId,
                    weight = 1,
                    rawGameObject = po.gameObject,
                    forceDuplicatesPossible = false,
                    additionalPrerequisites = new DungeonPrerequisite[0]
                });
            }

        }

        /// <summary>
        /// Adds an item to a loot table via PickupObjectId
        /// </summary>
        /// <param name="lootTable">The loot table you want to add to</param> 
        /// <param name="poID">The id of the PickupObject you're adding</param>
        /// <param name="weight">The Weight of the item you're adding (default is 1)</param>
        /// <returns></returns>
        public static void AddItemToPool(this GenericLootTable lootTable, int poID, float weight = 1)
        {

            var po = PickupObjectDatabase.GetById(poID);
            lootTable.defaultItemDrops.Add(new WeightedGameObject()
            {
                pickupId = po.PickupObjectId,
                weight = weight,
                rawGameObject = po.gameObject,
                forceDuplicatesPossible = false,
                additionalPrerequisites = new DungeonPrerequisite[0]
            });
        }
    }
}
