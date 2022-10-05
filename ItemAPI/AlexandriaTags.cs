using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.ItemAPI
{
    public static class AlexandriaTags
    {
        /// <summary>
        /// The setup method which adds tags to basegame items and enemies. DO NOT CALL THIS METHOD.
        /// </summary>
        public static void InitGenericTags()
        {
            //Items
            List<int> bulletModifierIDs = new List<int>() { 579, 627, 298, 640, 111, 113, 172, 277, 278, 284, 286, 288, 323, 352, 373, 374, 375, 410, 521, 523, 528, 530, 531, 532, 533, 538, 568, 569, 630, 655, 661, 822, 298, 304, 527, 638, 636, 241, 204, 295, 241, 524, 287 };
            List<int> kalashnikovWeapons = new List<int>() { 15, 29, 95, 221, 510, 611, 726 };
            List<int> guonStone = new List<int>() { 565, 260, 262, 263, 264, 269, 270, 466 };
            List<int> tableTechs = new List<int>() { 396, 397, 398, 399, 400, 465, 633, 666 };
            List<int> ammolet = new List<int>() { 321, 322, 325, 342, 343, 344 };
            List<int> arrowbolt = new List<int>() { 8, 200, 12, 4, 126, 52, 210, 227, 381, 535, 693, 749, 482 };
            List<int> companion = new List<int>() { 491, 492, 300, 249, 301, 318, 442, 232, 451, 461, 572, 580, 664, 632, 818, 645, 607 };
            List<int> flora = new List<int>() { 197, 674, 516, 339, 478, 124, 485, 71, 438, 258, 253, 289 };
            List<int> noncompanionalive = new List<int>() { 234, 201, 338, 599, 563, 176 };

            foreach (int id in bulletModifierIDs) SetTag(id, "bullet_modifier");
            foreach (int id in kalashnikovWeapons) SetTag(id, "kalashnikov");
            foreach (int id in guonStone) SetTag(id, "guon_stone");
            foreach (int id in tableTechs) SetTag(id, "table_tech");
            foreach (int id in ammolet) SetTag(id, "ammolet");
            foreach (int id in arrowbolt) SetTag(id, "arrow_bolt_weapon");
            foreach (int id in flora) SetTag(id, "flora");
            foreach (int id in noncompanionalive) SetTag(id, "non_companion_living_item");
            foreach (int id in companion) SetTag(id, "companion");

            //Enemies
            List<string> Blobulonians = new List<string>()
            {
                "0239c0680f9f467dbe5c4aab7dd1eca6", //Blobulon
                "042edb1dfb614dc385d5ad1b010f2ee3", //Blobuloid
                "42be66373a3d4d89b91a35c9ff8adfec", //Blobulin
                "e61cab252cfb435db9172adc96ded75f", //Poisbulon
                "fe3fe59d867347839824d5d9ae87f244", //Poisbuloid
                "b8103805af174924b578c98e95313074", //Poisbulin
                "022d7c822bc146b58fe3b0287568aaa2", //Blizzbulon
                "ccf6d241dad64d989cbcaca2a8477f01", //Leadbulon
                "062b9b64371e46e195de17b6f10e47c8", //Bloodbulon
                "116d09c26e624bca8cca09fc69c714b3", //Poopulon
                "864ea5a6a9324efc95a0dd2407f42810", //Cubulon
                "0b547ac6b6fc4d68876a241a88f5ca6a", //Cubulead
                "1bc2a07ef87741be90c37096910843ab", //Chancebulon
                "1b5810fafbec445d89921a4efb4e42b7", //Blobulord
                "d1c9781fdac54d9e8498ed89210a0238", //Tiny Blobulord
            };
            List<string> TitanBullets = new List<string>()
            {
                "c4cf0620f71c4678bb8d77929fd4feff", //Titan Bullet Kin
                "1f290ea06a4c416cabc52d6b3cf47266", //Titan Bullet Kin Boss
                "df4e9fedb8764b5a876517431ca67b86", //Titaness Bullet Kin Boss
             };
            List<string> MultiPhaseEnemy = new List<string>()
            {
                "062b9b64371e46e195de17b6f10e47c8", //Bloodbulon
                "21dd14e5ca2a4a388adab5b11b69a1e1", //Shelleton
                "98ea2fe181ab4323ab6e9981955a9bca", //Shambling Round
             };
            List<string> Mimics = new List<string>()
            {
                "2ebf8ef6728648089babb507dec4edb7", //Brown Mimic
                "d8d651e3484f471ba8a2daa4bf535ce6", //Blue Mimic
                "abfb454340294a0992f4173d6e5898a8", //Green Mimic
                "d8fd592b184b4ac9a3be217bc70912a2", //Red Mimic
                "6450d20137994881aff0ddd13e3d40c8", //Black Mimic
                "ac9d345575444c9a8d11b799e8719be0", //Rat Mimic
                "796a7ed4ad804984859088fc91672c7f", //Pedestal Mimic
                "479556d05c7c44f3b6abb3b2067fc778", //Wall Mimic
                "9189f46c47564ed588b9108965f975c9" //Door Lord
             };
            List<string> ShotgunKin = new List<string>()
            {
                "128db2f0781141bcb505d8f00f9e4d47", //Red Shotgun Kin
                "b54d89f9e802455cbb2b8a96a31e8259", //Blue Shotgun Kin
                "2752019b770f473193b08b4005dc781f", //Veteran Shotgun Kin
                "7f665bd7151347e298e4d366f8818284", //Mutant shotgun kin
                "b1770e0f1c744d9d887cc16122882b4f", //Executioner
                "1bd8e49f93614e76b140077ff2e33f2b", //Ashen Shotgun Kin
                "044a9f39712f456597b9762893fbc19c", //Shotgrub
                "37340393f97f41b2822bc02d14654172", //Creech
                "ddf12a4881eb43cfba04f36dd6377abb", //Western Shotgun Kin
                "86dfc13486ee4f559189de53cfb84107", //Pirate Shotgun Kin
                "2d4f8b5404614e7d8b235006acde427a" //Shotgat
             };
            List<string> slidingCubes = new List<string>()
            {
                "f155fd2759764f4a9217db29dd21b7eb", //Mountain Cube
                "33b212b856b74ff09252bf4f2e8b8c57", //Lead Cube
                "3f2026dc3712490289c4658a2ba4a24b", //Flesh Cube
                "ba928393c8ed47819c2c5f593100a5bc" // Brick Cube
             };
            List<string> CubeBlobulonians = new List<string>()
            {
                "864ea5a6a9324efc95a0dd2407f42810", //Cubulon
                "0b547ac6b6fc4d68876a241a88f5ca6a", //Cubulead
             };
            List<string> Bullats = new List<string>()
            {
                "2feb50a6a40f4f50982e89fd276f6f15", //Bullat
                "2d4f8b5404614e7d8b235006acde427a", //Shotgat
                "b4666cb6ef4f4b038ba8924fd8adf38f", //Grenat
                "7ec3e8146f634c559a7d58b19191cd43", //Spirat
                "1a4872dafdb34fd29fe8ac90bd2cea67", //King Bullat
                "981d358ffc69419bac918ca1bdf0c7f7", //Gargoyle
             };
            List<string> SmallBullat = new List<string>()
            {
                "2feb50a6a40f4f50982e89fd276f6f15", //Bullat
                "2d4f8b5404614e7d8b235006acde427a", //Shotgat
                "b4666cb6ef4f4b038ba8924fd8adf38f", //Grenat
                "7ec3e8146f634c559a7d58b19191cd43", //Spirat
             };
            List<string> Skeletons = new List<string>()
            {
                "336190e29e8a4f75ab7486595b700d4a", //Skullet
                "95ec774b5a75467a9ab05fa230c0c143", //Skullmet
                "af84951206324e349e1f13f9b7b60c1a", //Skusket
                "1cec0cdf383e42b19920787798353e46", //Black Skusket
                "c2f902b7cbe745efb3db4399927eab34", //Skusket Head
                "21dd14e5ca2a4a388adab5b11b69a1e1", //Shelleton
                "d5a7b95774cd41f080e517bea07bf495", //Revolvenant
                "cd88c3ce60c442e9aa5b3904d31652bc", //Lich
                "68a238ed6a82467ea85474c595c49c6e", //Megalich
                "7c5d5f09911e49b78ae644d2b50ff3bf", //Infinilich
                "5e0af7f7d9de4755a68d2fd3bbc15df4", //Cannonbalrog
             };
            List<string> Gunjurers = new List<string>()
            {
                "206405acad4d4c33aac6717d184dc8d4", //Apprentice Gunjurer
                "c4fba8def15e47b297865b18e36cbef8", //Gunjurer
                "9b2cf2949a894599917d4d391a0b7394", //High Gunjurer
                "56fb939a434140308b8f257f0f447829", //Lore Gunjurer
             };
            List<string> Gunsingers = new List<string>()
            {
                "cf2b7021eac44e3f95af07db9a7c442c", //Gunsinger
                "c50a862d19fc4d30baeba54795e8cb93", //Aged Gunsinger
                "b1540990a4f1480bbcb3bea70d67f60d", //Ammomancer
                "8b4a938cdbc64e64822e841e482ba3d2", //Jammomancer
                "ba657723b2904aa79f9e51bce7d23872", //Jamerlengo
             };
            List<string> Bookllets = new List<string>()
            {
                "c0ff3744760c4a2eb0bb52ac162056e6", //Bookllet
                "6f22935656c54ccfb89fca30ad663a64", //Blue Bookllet
                "a400523e535f41ac80a43ff6b06dc0bf", //Green Bookllet
                "216fd3dfb9da439d9bd7ba53e1c76462", //Necronomicon
                "78e0951b097b46d89356f004dda27c42", //Stone Tablet
             };
            List<string> RegDetTypes = new List<string>()
            {
                "ac986dabc5a24adab11d48a4bccf4cb1", //Det
                "48d74b9c65f44b888a94f9e093554977", //X Det
                "c5a0fd2774b64287bf11127ca59dd8b4", //Diagonal X Det
                "b67ffe82c66742d1985e5888fd8e6a03", //Vertical Det
                "d9632631a18849539333a92332895ebd", //Diagonal Det
                "1898f6fe1ee0408e886aaf05c23cc216", //Horizontal Det
                "abd816b0bcbf4035b95837ca931169df", //Vertical X Det
                "07d06d2b23cc48fe9f95454c839cb361", //Horizontal X Det
             };
            List<string> Machines = new List<string>()
            {
                "ac986dabc5a24adab11d48a4bccf4cb1", //Det
                "48d74b9c65f44b888a94f9e093554977", //X Det
                "c5a0fd2774b64287bf11127ca59dd8b4", //Diagonal X Det
                "b67ffe82c66742d1985e5888fd8e6a03", //Vertical Det
                "d9632631a18849539333a92332895ebd", //Diagonal Det
                "1898f6fe1ee0408e886aaf05c23cc216", //Horizontal Det
                "abd816b0bcbf4035b95837ca931169df", //Vertical X Det
                "07d06d2b23cc48fe9f95454c839cb361", //Horizontal X Det
                "9b4fb8a2a60a457f90dcf285d34143ac", //Gat
                "d4f4405e0ff34ab483966fd177f2ece3", //Grey Cylinder
                "534f1159e7cf4f6aa00aeea92459065e", //Red Cylinder
                "2b6854c0849b4b8fb98eb15519d7db1c", //Bullet Mech
                "4538456236f64ea79f483784370bc62f", //Fusebot
                "be0683affb0e41bbb699cb7125fdded6", //Mouser
                "12a054b8a6e549dcac58a82b89e319e5", //Terminator
                "fa76c8cfdf1c4a88b55173666b4bc7fb", //Treadnaught
                "4d164ba3f62648809a4a82c90fc22cae", //Rat Mech
                "41ee1c8538e8474a82a74c4aff99c712", //Helicopter Agunim
                "8d441ad4e9924d91b6070d5b3438d066", //Dr. Wolf's Monster
                "b98b10fca77d469e80fb45f3c5badec5", //HM Absolution
                "9215d1a221904c7386b481a171e52859", //Fridge Maiden
             };
            List<string> Ghosts = new List<string>()
            {
                "4db03291a12144d69fe940d5a01de376", //Hollowpoint
                "56f5a0f2c1fc4bc78875aea617ee31ac", //Spectre
             };
            List<string> Mushrooms = new List<string>()
            {
                "f905765488874846b7ff257ff81d6d0c", //Fungun
                "eed5addcc15148179f300cc0d9ee7f94", //Spogre
             };
            List<string> MuzzleWisp = new List<string>()
            {
                "ffdc8680bdaa487f8f31995539f74265", //Muzzle Wisp
                "d8a445ea4d944cc1b55a40f22821ae69", //Muzzle Flare
             };
            List<string> SniperKin = new List<string>()
            {
                "31a3ea0c54a745e182e22ea54844a82d", //Sniper Shell
                "c5b11bfc065d417b9c4d03a5e385fe2c", //Professional
             };
            List<string> Royal = new List<string>()
            {
                "1a4872dafdb34fd29fe8ac90bd2cea67", //King Bullat
                "ffca09398635467da3b1f4a54bcfda80", //Bullet King
                "5729c8b5ffa7415bb3d01205663a33ef", //Old King
             };

            foreach (string guid in Blobulonians) SetTag(guid, "blobulon");
            foreach (string guid in TitanBullets) SetTag(guid, "titan_bullet_kin");
            foreach (string guid in MultiPhaseEnemy) SetTag(guid, "multiple_phase_enemy");
            foreach (string guid in Mimics) SetTag(guid, "mimic");
            foreach (string guid in ShotgunKin) SetTag(guid, "shotgun_kin");
            foreach (string guid in slidingCubes) SetTag(guid, "sliding_cube");
            foreach (string guid in CubeBlobulonians) SetTag(guid, "cube_blobulon");
            foreach (string guid in Bullats) SetTag(guid, "bullat");
            foreach (string guid in SmallBullat) SetTag(guid, "small_bullat");
            foreach (string guid in Skeletons) SetTag(guid, "skeleton");
            foreach (string guid in Gunjurers) SetTag(guid, "gunjurer");
            foreach (string guid in Gunsingers) SetTag(guid, "gunsinger");
            foreach (string guid in Bookllets) SetTag(guid, "bookllet");
            foreach (string guid in RegDetTypes) SetTag(guid, "regular_det");
            foreach (string guid in Machines) SetTag(guid, "robotic_mechanical");
            foreach (string guid in Ghosts) SetTag(guid, "ghost");
            foreach (string guid in Mushrooms) SetTag(guid, "mushroom");
            foreach (string guid in MuzzleWisp) SetTag(guid, "muzzle_wisp");
            foreach (string guid in SniperKin) SetTag(guid, "sniper_kin");
            foreach (string guid in Royal) SetTag(guid, "royalty");

        }

        static Dictionary<string, List<int>> itemTags = new Dictionary<string, List<int>>();
        static Dictionary<string, List<string>> aiActorTags = new Dictionary<string, List<string>>();

        //Item Based Tag Interaction Methods

        /// <summary>
        /// Adds the specified tag to the PickupObject.
        /// </summary>
        /// <param name="item">The item to which the tag should be added.</param>
        /// <param name="tag">The tag which should be added.</param>
        public static void SetTag(this PickupObject item, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                itemTags.Add(tag, new List<int>());
            }
            if (!itemTags[tag].Contains(item.PickupObjectId)) itemTags[tag].Add(item.PickupObjectId);
        }

        /// <summary>
        /// Adds the specified tag to the PickupObject corresponding to the given ID.
        /// </summary>
        /// <param name="id">The ID of the item to which the tag should be added.</param>
        /// <param name="tag">The tag which should be added.</param>
        public static void SetTag(int id, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                itemTags.Add(tag, new List<int>());
            }
            if (!itemTags[tag].Contains(id)) itemTags[tag].Add(id);
        }

        /// <summary>
        /// Returns true if the PickupObject has the specified tag.
        /// </summary>
        /// <param name="item">The item which is being checked for the tag.</param>
        /// <param name="tag">The tag which is being checked for.</param>
        public static bool HasTag(this PickupObject item, string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                return false;
            }
            return (itemTags[tag].Contains(item.PickupObjectId));
        }

        /// <summary>
        /// Returns a list of all item IDs which have the specified tag.
        /// </summary>
        /// <param name="tag">The tag to be searched for.</param>
        public static List<int> GetAllItemsIdsWithTag(string tag)
        {
            if (!itemTags.ContainsKey(tag))
            {
                return new List<int>();
            }
            return itemTags[tag];
        }

        /// <summary>
        /// Returns a list of all PickupObjects in the PickupObjectDatabase which have the specified tag.
        /// </summary>
        /// <param name="tag">The tag to be searched for.</param>
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

        //Enemy based Tag Interaction Methods

        /// <summary>
        /// Adds the specified tag to the AIActor.
        /// </summary>
        /// <param name="aiActor">The AIActor to be tagged.</param>
        /// <param name="tag">The tag to be added.</param>
        public static void SetTag(this AIActor aiActor, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                aiActorTags.Add(tag, new List<string>());
            }
            if (!aiActorTags[tag].Contains(aiActor.EnemyGuid)) aiActorTags[tag].Add(aiActor.EnemyGuid);

        }

        /// <summary>
        /// Adds the specified tag to the AIActor with the given GUID.
        /// </summary>
        /// <param name="guid">The guid corresponding to the AIActor to be tagged.</param>
        /// <param name="tag">The tag to be added.</param>
        public static void SetTag(string guid, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                aiActorTags.Add(tag, new List<string>());
            }
            if (!aiActorTags[tag].Contains(guid)) aiActorTags[tag].Add(guid);

        }

        /// <summary>
        /// Returns true if the AIActor corresponding to the given GUID has the specified tag.
        /// </summary>
        /// <param name="guid">The guid corresponding to the AIActor to be checked for the tag.</param>
        /// <param name="tag">The tag to be checked for.</param>
        public static bool HasTag(string guid, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return false;
            }
            return (aiActorTags[tag].Contains(guid));
        }

        /// <summary>
        /// Returns true if the AIActor has the specified tag.
        /// </summary>
        /// <param name="aiActor">The AIActor being checked.</param>
        /// <param name="tag">The tag to be checked for.</param>
        public static bool HasTag(this AIActor aiActor, string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return false;
            }
            return (aiActorTags[tag].Contains(aiActor.EnemyGuid));
        }

        /// <summary>
        /// Returns a list of all enemy guids of AIActors with the specified tag.
        /// </summary>
        /// <param name="tag">The tag to be searched for.</param>
        public static List<string> GetAllEnemyGuidsWithTag(string tag)
        {
            if (!aiActorTags.ContainsKey(tag))
            {
                return new List<string>();
            }
            return aiActorTags[tag];
        }

        /// <summary>
        /// Returns a list of all AIActors in the EnemyDatabase with the specified tag.
        /// </summary>
        /// <param name="tag">The tag to be searched for.</param>
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

        /// <summary>
        /// Returns true if the AIActor has any of the tags in the provided list.
        /// </summary>
        /// <param name="aiActor">The AIActor to be checked.</param>
        /// <param name="tags">The list of tags to be checked for.</param>
        /// <param name="reqAll">If true, the AIActor must have ALL specified tags to return true. If false, the AIActor must only have one.</param>
        public static bool HasTags(this AIActor aiActor, List<string> tags, bool reqAll = false)
        {
            int tagsFound = 0;
            foreach (string tag in tags)
            {
                if (aiActor.HasTag(tag)) tagsFound++;
            }
            if ((reqAll && tagsFound >= tags.Count) || (!reqAll && tagsFound > 0)) return true;
            else return false;
        }

    }
}
