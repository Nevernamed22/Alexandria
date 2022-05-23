using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.Misc
{
    public static class AlexandriaTags
    {
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
