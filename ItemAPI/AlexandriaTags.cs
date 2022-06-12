using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.ItemAPI
{
    public static class AlexandriaTags
    {
        public static void InitGenericTags()
        {
            List<int> bulletModifierIDs = new List<int>() { 579, 627, 298, 640, 111, 113, 172, 277, 278, 284, 286, 288, 323, 352, 373, 374, 375, 410, 521, 523, 528, 530, 531, 532, 533, 538, 568, 569, 630, 655, 661, 822, 298, 304, 527, 638, 636, 241, 204, 295, 241, 524, 287 };
            List<int> kalashnikovWeapons = new List<int>() { 15, 29, 95, 221, 510, 611, 726 };
            List<int> guonStone = new List<int>() { 565, 260, 262, 263, 264, 269, 270 };
            List<int> tableTechs = new List<int>() { 396, 397, 398, 399, 400, 465, 633, 666 };
            List<int> ammolet = new List<int>() { 321, 322, 325, 342, 343, 344 };

            foreach (int id in bulletModifierIDs) SetTag(id, "bullet_modifier");
            foreach (int id in kalashnikovWeapons) SetTag(id, "kalashnikov");
            foreach (int id in guonStone) SetTag(id, "guon_stone");
            foreach (int id in tableTechs) SetTag(id, "table_tech");
            foreach (int id in ammolet) SetTag(id, "ammolet");

        }
        //item tagging

        static Dictionary<string, List<int>> itemTags = new Dictionary<string, List<int>>();

        public static void SetTag(this PickupObject item, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                itemTags.Add(tag, new List<int>());
            }
            if (!itemTags[tag].Contains(item.PickupObjectId)) itemTags[tag].Add(item.PickupObjectId);
        }

        public static void SetTag(int id, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                itemTags.Add(tag, new List<int>());
            }
            if (!itemTags[tag].Contains(id)) itemTags[tag].Add(id);
        }

        public static bool HasTag(this PickupObject item, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                return false;
            }
            return (itemTags[tag].Contains(item.PickupObjectId));
        }

        public static List<int> GetAllItemsIdsWithTag(string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                return new List<int>();
            }
            return itemTags[tag];
        }

        public static List<PickupObject> GetAllItemsWithTag(string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                return new List<PickupObject>();
            }
            List<PickupObject> pickupObjects = new List<PickupObject>();
            foreach (var id in itemTags[tag]) { if (PickupObjectDatabase.GetById(id) != null) pickupObjects.Add(PickupObjectDatabase.GetById(id)); }

            return pickupObjects;
        }

        //enemy tagging

        static Dictionary<string, List<string>> aiActorTags = new Dictionary<string, List<string>>();

        public static void SetTag(this AIActor aiActor, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                aiActorTags.Add(tag, new List<string>());
            }
            if (!aiActorTags[tag].Contains(aiActor.EnemyGuid)) aiActorTags[tag].Add(aiActor.EnemyGuid);

        }

        public static void SetTag(string guid, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                aiActorTags.Add(tag, new List<string>());
            }
            if (!aiActorTags[tag].Contains(guid)) aiActorTags[tag].Add(guid);

        }

        public static bool HasTag(this AIActor aiActor, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return false;
            }
            return (aiActorTags[tag].Contains(aiActor.EnemyGuid));
        }

        public static List<string> GetAllEnemiesGuidWithTag(string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return new List<string>();
            }
            return aiActorTags[tag];
        }

        public static List<AIActor> GetAllEnemiesWithTag(string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return new List<AIActor>();
            }
            List<AIActor> aiActors = new List<AIActor>();
            foreach (var id in aiActorTags[tag]) { if (EnemyDatabase.GetOrLoadByGuid(id) != null) aiActors.Add(EnemyDatabase.GetOrLoadByGuid(id)); }

            return aiActors;
        }

    }
}
