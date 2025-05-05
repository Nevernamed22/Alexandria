using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class LootUtility
    {
        public static GameObject SelectByWeightNoExclusions(this GenericLootTable table, bool useSeedRandom = false)
        {
            int outIndex = -1;
            List<WeightedGameObject> list = new List<WeightedGameObject>();
            float num = 0f;
            for (int i = 0; i < table.defaultItemDrops.elements.Count; i++)
            {
                WeightedGameObject weightedGameObject = table.defaultItemDrops.elements[i];
                list.Add(weightedGameObject);
                num += weightedGameObject.weight;
            }
            float num2 = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
            float num3 = 0f;
            for (int k = 0; k < list.Count; k++)
            {
                num3 += list[k].weight;
                if (num3 > num2)
                {
                    outIndex = table.defaultItemDrops.elements.IndexOf(list[k]);
                    return list[k].gameObject;
                }
            }
            outIndex = table.defaultItemDrops.elements.IndexOf(list[list.Count - 1]);
            return list[list.Count - 1].gameObject;
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
        /// Adds an item to a loot table via PickupObject (Will keep weight at 1)
        /// </summary>
        /// <param name="lootTable">The loot table you want to add to</param> 
        /// <param name="items">A list of item IDs you want to add to your loottable</param>
        /// <returns></returns>
        public static void AddItemsToPool(this GenericLootTable lootTable, params int[] items)
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
        /// Adds an item to a loot table via PickupObject (Will keep weight at 1)
        /// </summary>
        /// <param name="lootTable">The loot table you want to add to</param> 
        /// <param name="items">A dictionary of item IDs and their weights. The FIRST VALUE IS THE ID.</param>
        /// <returns></returns>
        public static void AddItemsToPool(this GenericLootTable lootTable, Dictionary<int, float> items)
        {
            foreach (var entry in items)
            {
                var po = PickupObjectDatabase.GetById(entry.Key);
                lootTable.defaultItemDrops.Add(new WeightedGameObject()
                {
                    pickupId = po.PickupObjectId,
                    weight = entry.Value,
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
        /// <summary>
        /// Removes a pickup object from the default guns and items loot table
        /// </summary>
        /// <param name="po">The pickup object you want to remove</param> 
        /// <returns></returns>
        public static void RemovePickupFromLootTables(this PickupObject po)
        {
            WeightedGameObject go1 = GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.FindWeightedGameObjectInCollection(po);
            if (go1 != null)
            {
                GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.elements.Remove(go1);
            }
            WeightedGameObject go2 = GameManager.Instance.RewardManager.ItemsLootTable.defaultItemDrops.FindWeightedGameObjectInCollection(po);
            if (go2 != null)
            {
                GameManager.Instance.RewardManager.ItemsLootTable.defaultItemDrops.elements.Remove(go2);
            }
        }

        /// <summary>
        /// Finds a weighted game object corresponding to a given pickup object in a weighted game object collection.
        /// </summary>
        /// <param name="collection">The collection you are searching</param> 
        /// <param name="po">The pickup object you want to find</param> 
        /// <returns></returns>
        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, PickupObject po)
        {
            WeightedGameObject go = collection.FindWeightedGameObjectInCollection(po.PickupObjectId);
            if (go == null)
            {
                go = collection.FindWeightedGameObjectInCollection(po.gameObject);
            }
            return go;
        }

        /// <summary>
        /// Finds a weighted game object corresponding to a given pickup object id in a weighted game object collection.
        /// </summary>
        /// <param name="collection">The collection you are searching</param> 
        /// <param name="id">The id of the pickup object you want to find</param> 
        /// <returns></returns>
        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, int id)
        {
            foreach (WeightedGameObject go in collection.elements)
            {
                if (go.pickupId == id)
                {
                    return go;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a weighted game object corresponding to a given gameobject in a weighted game object collection.
        /// </summary>
        /// <param name="collection">The collection you are searching</param> 
        /// <param name="obj">The gameobject you want to find</param> 
        /// <returns></returns>
        public static WeightedGameObject FindWeightedGameObjectInCollection(this WeightedGameObjectCollection collection, GameObject obj)
        {
            foreach (WeightedGameObject go in collection.elements)
            {
                if (go.gameObject == obj)
                {
                    return go;
                }
            }
            return null;
        }
    }
}
