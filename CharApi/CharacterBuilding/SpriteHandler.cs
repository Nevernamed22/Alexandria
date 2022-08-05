using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;


using System;
using Alexandria.ItemAPI;
using System.IO;

namespace CustomCharacters
{
    /*
     * Handles adding sprites to the appropriate collections,
     * creating animations, and loading them when necessary
     */

    public static class SpriteHandler
    {
        private static FieldInfo spriteNameLookup =
            typeof(tk2dSpriteCollectionData).GetField("spriteNameLookupDict",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo m_playerMarkers = typeof(Minimap).GetField("m_playerMarkers", BindingFlags.NonPublic | BindingFlags.Instance);
        public static dfAtlas uiAtlas;
        public static List<dfAtlas.ItemInfo> uiFaceCards = new List<dfAtlas.ItemInfo>();
        public static List<dfAtlas.ItemInfo> punchoutFaceCards = new List<dfAtlas.ItemInfo>();

        private static readonly Rect uiFacecardBounds = new Rect(0, 1235, 2048, 813);
        private static readonly Rect punchoutFacecardBounds = new Rect(128, 71, 128, 186);
        private static readonly Vector2 faceCardSizeInPixels = new Vector2(34, 34);

        private static Dictionary<string, Material[]> usedMaterialDictionary = new Dictionary<string, Material[]>();

        //sorry not sorry
        //ive decied to name them steve
        static Dictionary<string, Tuple<tk2dSpriteAnimationClip.WrapMode, int>> playerAnimInfo = new Dictionary<string, Tuple<tk2dSpriteAnimationClip.WrapMode, int>>
        {
            { "this one dose nothing im just putting it here to say im really really sorry you have to look at this code", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 0) },

            { "chest_recover", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },
            { "death", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },
            { "death_coop", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 16) },
            { "death_shot", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },

            { "dodge", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },
            { "dodge_bw", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },
            { "dodge_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },
            { "dodge_left_bw", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },

            { "doorway", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 10) },
            { "ghost_idle_back", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_idle_back_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_idle_back_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_idle_front", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_idle_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_idle_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 4) },
            { "ghost_sneeze_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },
            { "ghost_sneeze_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },

            { "idle", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_backward", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_backward_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_backward_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_bw", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            //{ "idle_bw_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_bw_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_forward", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_forward_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_forward_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "idle_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },

            { "item_get", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 9) },

            { "jetpack_down", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "jetpack_down_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "jetpack_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "jetpack_right_bw", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "jetpack_right_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },
            { "jetpack_up", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },

            { "pet", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 6) },

            { "pitfall", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 15) },
            { "pitfall_down", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 15) },
            { "pitfall_return", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 11) },

            { "run_down", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_down_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_down_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_right_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_right_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },

            { "run_right_bw", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_right_bw_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
          
            { "run_up", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_up_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },
            { "run_up_twohands", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 9) },

            { "slide_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 2) },
            { "slide_up", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 2) },
            { "slide_down", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 2) },           

            { "spinfall", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 16) },
            { "spit_out", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 12) },

            { "tablekick_down", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },
            { "tablekick_down_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },
            { "tablekick_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },
            { "tablekick_right_hand", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },
            { "tablekick_up", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Once, 8) },

            { "timefall", new Tuple<tk2dSpriteAnimationClip.WrapMode, int>( tk2dSpriteAnimationClip.WrapMode.Loop, 8) },
        };

        static Dictionary<string, Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]>> punchoutPlayerAnimInfo = new Dictionary<string, Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]>>
        {
            { "_idle", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Loop, 6, "idle", new int[] { 1, 1, 2, 4 } ) }, //1, 1, 2, 4

            { "_punch_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 12, "punch_right", new int[] { 1, 2, 3 }) }, // 1, 2, 3
            { "_punch_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 12, "punch_left", new int[] { 1, 2, 3 }) }, // 1, 2, 3

            { "_punch_left_miss", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 6, "punch_blocked_left", new int[] { 2, 3 }) }, //1, 2
            { "_punch_right_miss", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 6, "punch_blocked_right", new int[] { 2, 3 }) }, //1, 2

            { "_punch_left_miss_far", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 6, "miss_left", new int[] { 2, 3 }) }, //2, 3
            { "_punch_right_miss_far", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 6, "miss_right", new int[] { 2, 3 }) }, //2, 3

            { "_hit_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 12, "hit_right", new int[] { 1, 2, 3, 3, 3 }) }, // 1, 2, 3, 3, 3
            { "_hit_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 12, "hit_left", new int[] { 1, 2, 3, 3, 3 }) }, // 1, 2, 3, 3, 3

            { "_super", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 12, "star", new int[] { 1, 1, 2, 2, 3, 3, 4, 5, 6, 6, 7, 7, 8, 8, 9, 9 }) }, // 1, 1, 2, 2, 3, 3, 4, 5, 6, 6, 7, 7, 8, 8, 9, 9
            { "_super_final", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 8, "star", new int[] { 1, 1, 2, 2, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 6, 6, 7, 7, 8, 8, 9, 9 }) }, // 1, 1, 2, 2, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 6, 6, 7, 7, 8, 8, 9, 9

            { "_dodge_left", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 10, "dodge_left", new int[] { 1, 1, 2, 2 }) }, // 1, 1, 2, 2
            { "_dodge_right", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 10, "dodge_right", new int[] { 1, 1, 2, 2 }) }, // 1, 1, 2, 2

            { "_block", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 10, "block", new int[] { 1, 2, 2, 2 , 2, 2 }) }, // 1, 2, 2, 2 , 2, 2

            { "_block_hit", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 5, "block_hit", new int[] { 1, 1 }) }, // 1, 1

            { "_duck", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Once, 10,"duck", new int[] { 1, 1, 2, 2, 2, 2, 2 }) }, // 2, 2, 3, 3, 3, 3, 3            

            { "_exhaust", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Loop, 4, "exhaust", new int[] { 1, 2, 2, 3 }) },// 1, 2, 2, 3
            { "_knockout", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.LoopSection, 10, "knockout", new int[] { 1, 2, 3, 4, 5, 6, 7 }) }, // start at 6. 1, 2, 3, 4, 5, 6, 7

            { "_win", new Tuple<tk2dSpriteAnimationClip.WrapMode, int, string, int[]> ( tk2dSpriteAnimationClip.WrapMode.Loop, 4, "win", new int[] { 1, 2, 3, 4, 5, 6 }) }, // 1, 2, 3, 4, 5, 6
        };

        public static void SetupLitterallyEverythingForPunchOut2(CustomCharacterData data)
        {
            if (data.baseCharacter != PlayableCharacters.CoopCultist || data.baseCharacter != PlayableCharacters.Eevee || data.baseCharacter != PlayableCharacters.Cosmonaut || data.baseCharacter != PlayableCharacters.Ninja)
            {
                var player = ResourceManager.LoadAssetBundle("enemies_base_001").LoadAsset<GameObject>("MetalGearRat").GetComponent<AIActor>().GetComponent<MetalGearRatDeathController>().PunchoutMinigamePrefab.GetComponent<PunchoutController>().Player;
                var libary = player.gameObject.GetComponent<tk2dSpriteAnimator>().Library;
                var collection = libary.clips[0].frames[0].spriteCollection;
                var baseName = GetPlayerStringFromIdentity(data.baseCharacter);
                if (data.punchoutFaceCards != null && data.punchoutFaceCards.Count > 0)
                {
                    for (int i = 0; i < data.punchoutFaceCards.Count; i++)
                    {
                        player.PlayerUiSprite.Atlas.AddNewItemToAtlas(data.punchoutFaceCards[i], $"punch_player_health_{data.nameShort.ToLower()}_00{i + 1}");
                    }
                }
                //ETGModConsole.Log("added to atlas");
                var defMatches = collection.spriteDefinitions.Where(def => def.name.Contains(baseName) && !def.name.Contains("vfx")).Select(def => def);
                List<int> ids = new List<int>();
                //ETGModConsole.Log("pre foreach");
                bool h = false;
                foreach (var def in defMatches)
                {
                    //.Log(def.name.Replace(baseName, data.nameShort.ToLower()) +": " + data.punchoutSprites.Where(sprite => sprite.Key == (def.name.Replace(baseName, data.nameShort.ToLower()))).Select(sprite => sprite).Count().ToString());
                    /*
                    var sList = data.punchoutSprites.Where(sprite => sprite.Key == (def.name.Replace(baseName, data.nameShort.ToLower()))).ToList();
                    if (sList == null || sList.Count <= 0)
                    {
                        ETGModConsole.Log($"An issue occursed while trying to add the sprite: \"{collection.spriteDefinitions[frame.spriteId].name}\"");
                    }
                    */
                    var id = AddSpriteToCollection(data.punchoutSprites.Where(sprite => sprite.Key == (def.name.Replace(baseName, data.nameShort.ToLower()))).First().Value, collection);
                    if (!h)
                    {
                        //ToolsCharApi.ExportTexture(collection.spriteDefinitions[id].material.mainTexture.GetReadable(), "ihateyou", "zatherzyoulittlefucker");
                        h = true;
                    }
                    collection.spriteDefinitions[id].CopyToSelf(def);
                    //ETGModConsole.Log(collection.spriteDefinitions[id].name);
                    ids.Add(id);

                }
                //ETGModConsole.Log("added to collectiom");
                var animMatches = libary.clips.Where(clip => clip.name.Contains(baseName) && !clip.name.Contains("vfx")).Select(clip => clip);
                
                foreach (var clip in animMatches)
                {
                    var newClip = new tk2dSpriteAnimationClip();

                    newClip.CopyFrom(clip);
                    newClip.name = newClip.name.Replace(baseName.ToLower(), data.nameShort.ToLower());

                    foreach (var frame in newClip.frames)
                    {
                        var list = collection.spriteDefinitions.Where(def => def.name == collection.spriteDefinitions[frame.spriteId].name.Replace(baseName.ToLower(), data.nameShort.ToLower()) && !def.name.Contains("vfx")).ToList();
                        if (list == null || list.Count <= 0)
                        {
                            ETGModConsole.Log($"An issue occursed while trying to find sprite info for: \"{collection.spriteDefinitions[frame.spriteId].name}\"");
                        }
                        frame.spriteId = collection.spriteDefinitions.IndexOf(list[0]);
                    }
                    //ETGModConsole.Log(newClip.name);
                    libary.clips = libary.clips.Concat(new tk2dSpriteAnimationClip[] { newClip }).ToArray();
                }

                
            }

        }

        public static string GetPlayerStringFromIdentity(PlayableCharacters identity)
        {
            if (identity == PlayableCharacters.Guide)
            {
                return "hunter";
            }
            else if (identity == PlayableCharacters.Soldier)
            {
                return "marine";
            }
            else if (identity == PlayableCharacters.Gunslinger)
            {
                return "slinger";
            }
            else
            {
                return identity.ToString().ToLower();
            }
        }


        public static void SetupLitterallyEverythingForPunchOut(PlayerController player, CustomCharacterData data)
        {

           
            Dictionary<string, int> spriteIds = new Dictionary<string, int>();

            var libary = ResourceManager.LoadAssetBundle("enemies_base_001").LoadAsset<GameObject>("MetalGearRat").GetComponent<AIActor>().GetComponent<MetalGearRatDeathController>().PunchoutMinigamePrefab.GetComponent<PunchoutController>().Player.gameObject.GetComponent<tk2dSpriteAnimator>().Library;
            var collection = libary.clips[0].frames[0].spriteCollection;


            foreach (var sprite in data.punchoutSprites)
            {
                var id = AddSpriteToCollection(sprite.Value, collection);

                collection.spriteDefinitions[id].position0 = collection.spriteDefinitions[collection.GetSpriteIdByName(sprite.Key.Replace(data.nameShort.ToLower(), "hunter"))].position0;
                collection.spriteDefinitions[id].position1 = collection.spriteDefinitions[collection.GetSpriteIdByName(sprite.Key.Replace(data.nameShort.ToLower(), "hunter"))].position1;
                collection.spriteDefinitions[id].position2 = collection.spriteDefinitions[collection.GetSpriteIdByName(sprite.Key.Replace(data.nameShort.ToLower(), "hunter"))].position2;
                collection.spriteDefinitions[id].position3 = collection.spriteDefinitions[collection.GetSpriteIdByName(sprite.Key.Replace(data.nameShort.ToLower(), "hunter"))].position3;

                spriteIds.Add(sprite.Key, id);
            }


            var matches = spriteIds.Where(pair => pair.Key.Contains("_punch_win_")).Select(pair => pair.Key);


            foreach (var info in punchoutPlayerAnimInfo)
            {
                List<int> ids = new List<int>();

                if (info.Key != "_win")
                {
                    foreach (var num in info.Value.Item4)
                    {
                        //ETGModConsole.Log($"{data.nameShort.ToLower()}_punch_{info.Value.Item3}_00{num}");
                        ids.Add(spriteIds[$"{data.nameShort.ToLower()}_punch_{info.Value.Item3}_00{num}"]);

                    }
                }
                else
                {
                    foreach(var key in matches)
                    {
                        ids.Add(spriteIds[key]);
                    }
                }

                
                var anim = AddAnimation(libary, collection, ids, $"{data.nameShort.ToLower()}{info.Key}", info.Value.Item1, info.Value.Item2);

                if (info.Key == "_knockout")
                {
                    anim.loopStart = 6;
                }
            }

            //foreach (var anim in libary.clips)
            //{
                //ETGModConsole.Log(anim.name);
            //}
        }

        public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData collection, List<int> spriteIDs,
            string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15)
        {
            ////BotsMod.BotsModule.Log("a1");
            if (animator.Library == null)
            {
                animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
                animator.Library.clips = new tk2dSpriteAnimationClip[0];
                animator.Library.enabled = true;

            }
            ////BotsMod.BotsModule.Log("a2");
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spriteIDs.Count; i++)
            {
                ////BotsMod.BotsModule.Log("a2.25");
                tk2dSpriteDefinition sprite = collection.spriteDefinitions[spriteIDs[i]];
                if (sprite.Valid)
                {
                    ////BotsMod.BotsModule.Log("a2.5");
                    frames.Add(new tk2dSpriteAnimationFrame()
                    {
                        spriteCollection = collection,
                        spriteId = spriteIDs[i]
                    });
                }
            }
            ////BotsMod.BotsModule.Log("a3");
            var clip = new tk2dSpriteAnimationClip()
            {
                name = clipName,
                fps = fps,
                wrapMode = wrapMode,
            };
            Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
            animator.Library.clips[animator.Library.clips.Length - 1] = clip;

            clip.frames = frames.ToArray();
            return clip;
        }

        public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimation animaton, tk2dSpriteCollectionData collection, List<int> spriteIDs,
            string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15)
        {
            ////BotsMod.BotsModule.Log("a2");
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spriteIDs.Count; i++)
            {
                ////BotsMod.BotsModule.Log("a2.25");
                tk2dSpriteDefinition sprite = collection.spriteDefinitions[spriteIDs[i]];
                if (sprite.Valid)
                {
                    ////BotsMod.BotsModule.Log("a2.5");
                    frames.Add(new tk2dSpriteAnimationFrame()
                    {
                        spriteCollection = collection,
                        spriteId = spriteIDs[i]
                    });
                }
            }
            ////BotsMod.BotsModule.Log("a3");
            var clip = new tk2dSpriteAnimationClip()
            {
                name = clipName,
                fps = fps,
                wrapMode = wrapMode,
            };
            Array.Resize(ref animaton.clips, animaton.clips.Length + 1);
            animaton.clips[animaton.clips.Length - 1] = clip;

            clip.frames = frames.ToArray();
            return clip;
        }
        public static void HandleSprites(PlayerController player, CustomCharacterData data, Assembly assembly = null)
        {
            if (data.minimapIcon != null)
                HandleMinimapIcons(player, data);

            if (data.bossCard != null)
                HandleBossCards(player, data);

            if ((data.altSprites != null || data.altPlayerSheet != null) && string.IsNullOrEmpty(data.pathForAltSprites))
                HandleAltAnimations(player, data);


            if ((data.sprites != null || data.playerSheet != null) && string.IsNullOrEmpty(data.pathForSprites))
                HandleAnimations(player, data);

            if (!string.IsNullOrEmpty(data.pathForAltSprites))
                SetupLitterallyEverything(player, data, data.pathForAltSprites, true, assembly ?? Assembly.GetCallingAssembly());


            if (!string.IsNullOrEmpty(data.pathForSprites))
                SetupLitterallyEverything(player, data, data.pathForSprites, false, assembly ?? Assembly.GetCallingAssembly());

            

            //face card stuff
            uiAtlas = GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas;
            if (data.faceCard != null)
                HandleFacecards(player, data);

            if (data.punchoutSprites != null && data.punchoutSprites.Count > 0)
            {
                //ETGModConsole.Log("pre punchout setup");
                SetupLitterallyEverythingForPunchOut2(data);
            }
                

            if (data.loadoutSprites != null)
                HandleLoudoutSprites(player, data);

            if (data.coopDeathScreenIcon != null)
            {
                //ETGModConsole.Log($"\"coop_page_death_{data.nameShort.ToLower()}_001\" added");
                uiAtlas.AddNewItemToAtlas(data.coopDeathScreenIcon, $"coop_page_death_{data.nameShort.ToLower()}_001");
                //ToolsCharApi.ExportTexture(ToolsCharApi.LoadAssetFromAnywhere<GameObject>("Ammonomicon Atlas").GetComponent<dfAtlas>().Texture.GetReadable(), "ihateyou", "YoumadeashitofpiecewithyourtrashMTG");
            }
                


        }
        private static tk2dSpriteCollectionData itemCollection = PickupObjectDatabase.GetByEncounterName("singularity").sprite.Collection;
        /// <summary>
        /// Returns an object with a tk2dSprite component with the texture provided
        /// </summary>
        public static GameObject SpriteFromTexture(Texture2D texture, GameObject obj = null)
        {
            if (obj == null)
            {
                obj = new GameObject();
            }
            tk2dSprite sprite;
            sprite = obj.AddComponent<tk2dSprite>();

            int id = AddSpriteToCollection(texture, itemCollection);
            sprite.SetSprite(itemCollection, id);
            sprite.SortingOrder = 0;
            sprite.IsPerpendicular = true;

            obj.GetComponent<BraveBehaviour>().sprite = sprite;

            return obj;
        }

        public static void HandleLoudoutSprites(PlayerController player, CustomCharacterData data)
        {
            //var UIRootPrefab = AmmonomiconAPI.Tools.LoadAssetFromAnywhere<GameObject>("UI Root").GetComponent<GameUIRoot>();

           

            for (int i = 0; i < data.loadoutSprites.Count; i++)
            {
                var sprite = uiAtlas.AddNewItemToAtlas(AddOutlineToTexture(data.loadoutSprites[i], Color.white), data.loadoutSprites[i].name.Replace(" ","_"));
                
                
                data.loadoutSpriteNames.Add(data.loadoutSprites[i].name.Replace(" ", "_"));
            }

        }

        /// <summary>
        /// Constructs a new tk2dSpriteDefinition with the given texture
        /// </summary>
        /// <returns>A new sprite definition with the given texture</returns>
        public static tk2dSpriteDefinition ConstructDefinition(Texture2D texture, Material overrideMat = null)
        {
            RuntimeAtlasSegment ras = ETGMod.Assets.Packer.Pack(texture, true); //pack your resources beforehand or the outlines will turn out weird

            Material material = null;
            if (overrideMat != null)
            {
                material = overrideMat;
            }
            else
            {
                material = new Material(ShaderCache.Acquire(PlayerController.DefaultShaderName));
            }        
            material.mainTexture = ras.texture;
            //material.mainTexture = texture;

            var width = texture.width;
            var height = texture.height;

            var x = 0f;
            var y = 0f;

            var w = width / 16f;
            var h = height / 16f;

            var def = new tk2dSpriteDefinition
            {
                normals = new Vector3[] {
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
            },
                tangents = new Vector4[] {
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            },
                texelSize = new Vector2(1 / 16f, 1 / 16f),
                extractRegion = false,
                regionX = 0,
                regionY = 0,
                regionW = 0,
                regionH = 0,
                flipped = tk2dSpriteDefinition.FlipMode.None,
                complexGeometry = false,
                physicsEngine = tk2dSpriteDefinition.PhysicsEngine.Physics3D,
                colliderType = tk2dSpriteDefinition.ColliderType.None,
                collisionLayer = CollisionLayer.HighObstacle,
                position0 = new Vector3(x, y, 0f),
                position1 = new Vector3(x + w, y, 0f),
                position2 = new Vector3(x, y + h, 0f),
                position3 = new Vector3(x + w, y + h, 0f),
                material = material,
                materialInst = material,
                materialId = 0,
                //uvs = ETGMod.Assets.GenerateUVs(texture, 0, 0, width, height), //uv machine broke
                uvs = ras.uvs,
                boundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
                boundsDataExtents = new Vector3(w, h, 0f),
                untrimmedBoundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
                untrimmedBoundsDataExtents = new Vector3(w, h, 0f),
            };

            def.name = texture.name;
            return def;
        }

        public static Texture2D AddOutlineToTexture(Texture2D sprite, Color color)
        {

            int y = Mathf.FloorToInt(0);
            int width = Mathf.FloorToInt(sprite.width);
            int height = Mathf.FloorToInt(sprite.height);

            var posList = new List<Vector2Int> { new Vector2Int { x = 0, y = 1 }, new Vector2Int { x = 0, y = -1 }, new Vector2Int { x = 1, y = 0 }, new Vector2Int { x = -1, y = 0 }, };

            sprite.AddBorder(1);

            for (int x = 0; y < sprite.height + 1; x++)
            {
                if (x >= sprite.width)
                {
                    x = 0;
                    y++;
                    
                }
                //BotsModule.Log($"{sprite.name} ({x}, {y})");
                var pixel = sprite.GetPixel(x, y);
                if (pixel.a > 0 && pixel != Color.white)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        var pixel1 = sprite.GetPixel(x + posList[i].x, y + posList[i].y);
                        if (pixel1.a == 0)
                        {
                            //BotsModule.Log($"{sprite.name}: white pixel added at ({x + posList[i].x}, {y + posList[i].y})");
                            sprite.SetPixel(x + posList[i].x, y + posList[i].y, color);
                            sprite.Apply();
                        }
                    }
                }
            }

            //ToolsCharApi.ExportTexture(sprite, "SpriteDump/" + "OutlineTest");

            // Color[] pix = sprite.GetPixels(x, y, width, height);
            //Texture2D destTex = new Texture2D(width, height);
            //destTex.SetPixels(pix);
            //destTex.Apply();


            return sprite;
        }

        public static bool AddBorder(this Texture2D tex, int borderSize = 1)
        {
            int width = 0;
            int height = 0;
            if (tex.IsReadable())
            {

                width = tex.width + borderSize * 2;
                height = tex.height + borderSize * 2;

                Color[][] pixels = new Color[Math.Min(tex.width, width)][];


                for (int x = 0; x < Math.Min(tex.width, width); x++)
                {
                    for (int y = 0; y < Math.Min(tex.height, height); y++)
                    {
                        if (pixels[x] == null)
                        {
                            pixels[x] = new Color[Math.Min(tex.height, height)];
                        }
                        pixels[x][y] = tex.GetPixel(x, y);
                    }
                }


                bool result = tex.Resize(width, height);
                for (int x = borderSize; x < tex.width - borderSize; x++)
                {
                    for (int y = borderSize; y < tex.height - borderSize; y++)
                    {
                        bool isInOrigTex = false;
                        if (x - borderSize < pixels.Length)
                        {
                            if (y - borderSize < pixels[x - borderSize].Length)
                            {
                                isInOrigTex = true;
                                tex.SetPixel(x, y, pixels[x - borderSize][y - borderSize]);
                            }
                        }
                        if (!isInOrigTex)
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }

                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {

                        if (tex.GetPixel(x, y) == new Color32(205, 205, 205, 205))
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }

                    }
                }

                tex.Apply();
                return result;
            }
            return tex.Resize(width, height);
        }

        

        public static void SetupLitterallyEverything(PlayerController player, CustomCharacterData data, string path, bool alt, Assembly assembly = null)
        {
            //BotsModule.Log(path);

            //
            List<string> anims = new List<string>();

            GameObject libaryObject = new GameObject((data.nameShort + "Animator").Replace(" ", "_") + (alt ? "_alt" : ""));

            FakePrefab.MarkAsFakePrefab(libaryObject);


            var collection = SpriteBuilder.ConstructCollection(libaryObject, (data.nameShort + "CustomCollection").Replace(" ", "_") + (alt ? "_alt" : ""));

            if (alt)
            {
                data.altCollection = collection;
            }
            else
            {
                data.collection = collection;
            }

            //UnityEngine.Object.DontDestroyOnLoad(data.collection);
            data.animator = player.gameObject.transform.Find("PlayerSprite").gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
            data.animator.Library = libaryObject.AddComponent<tk2dSpriteAnimation>();
            data.animator.Library.clips = new tk2dSpriteAnimationClip[0];
            data.animator.Library.enabled = true;


            foreach (var file in ToolsCharApi.GetResourceNames(assembly ?? Assembly.GetCallingAssembly()))
            {
                var splitPath = file.Split('.');

                if (file.Contains(path) && file.Contains(".png") && splitPath[splitPath.Count() - 2] != (alt ? "hand_alt_001" : "hand_001") && !anims.Contains(splitPath[splitPath.Count() - 3]))// && splitPath[splitPath.Count() - 4] != "custom")
                {
                    anims.Add(splitPath[splitPath.Count() - 3]);

                    var dirName = "";

                    if (splitPath[splitPath.Count() - 4] == "custom")
                    {
                        dirName = "custom";
                    }
                    else if (splitPath[splitPath.Count() - 4] == "breach_idles")
                    {
                        dirName = "breach_idles";
                    }
                    else
                    {
                        dirName = splitPath[splitPath.Count() - 3];
                    }
                    //var dirName = splitPath[splitPath.Count() - 4] == "custom" ? "custom" : splitPath[splitPath.Count() - 3];//(file.Replace(path, ""));//.Replace(".", "");

                    //BotsModule.Log($"{file} -=- {dirName}");
                    var animName = data.nameShort + "_" + dirName;
                    List<Texture2D> textures = new List<Texture2D>();
                    List<int> spriteIds = new List<int>();

                    if (!file.Contains("cc_sprite_placeholder"))
                    {
                        textures = ToolsCharApi.GetTexturesFromResource($"{path}.{dirName}", assembly ?? Assembly.GetCallingAssembly());
                    }

                    if (dirName.Contains("custom"))
                    {
                        //continue;
                        //foreach (var customDir in Directory.GetDirectories(file))
                        //{
                        //var customDirName = customDir.Replace(file, "").Replace(".", "");
                        var customDirName = splitPath[splitPath.Count() - 3];
                        //BotsModule.Log($"Custom animation found \"{customDirName}\"");
                        animName = data.nameShort + "_" + customDirName;
                        textures = ToolsCharApi.GetTexturesFromResource($"{path}.{dirName}.{customDirName}", assembly ?? Assembly.GetCallingAssembly());

                        for (int i = 0; i < textures.Count; i++)
                        {
                            spriteIds.Add(AddSpriteToCollection(textures[i], collection, animName + i.ToString()));
                        }
                        if (textures.Count > 0)
                        {
                            SpriteHandler.AddAnimation(data.animator, collection, spriteIds, customDirName, tk2dSpriteAnimationClip.WrapMode.Loop, 8);
                        }

                        //}
                    }
                    else if (dirName.Contains("breach_idles"))
                    {
                        //continue;

                        var customDirName = splitPath[splitPath.Count() - 3];
                        //BotsModule.Log($"Custom breach animation found \"{customDirName}\"");
                        animName = data.nameShort + "_" + customDirName;
                        textures = ToolsCharApi.GetTexturesFromResource($"{path}.{dirName}.{customDirName}", assembly ?? Assembly.GetCallingAssembly());

                        for (int i = 0; i < textures.Count; i++)
                        {
                            spriteIds.Add(AddSpriteToCollection(textures[i], collection, animName + i.ToString()));
                        }
                        if (textures.Count > 0)
                        {
                            SpriteHandler.AddAnimation(data.animator, collection, spriteIds, customDirName, tk2dSpriteAnimationClip.WrapMode.Loop, 8);
                        }
                        /*
                        foreach (var customDir in Directory.GetDirectories(file))
                        {
                            var customDirName = customDir.Replace(file, "").Replace(".", "");
                            BotsModule.Log($"Breach Idle animation found \"{customDirName}\"");
                            animName = data.nameShort + "_" + customDirName;
                            textures = ToolsCharApi.GetTexturesFromResource(customDir);

                            for (int i = 1; i < textures.Count; i++)
                            {
                                spriteIds.Add(AddSpriteToCollection(textures[i], collection, animName + i.ToString()));
                            }
                            if (textures.Count > 0)
                            {
                                SpriteHandler.AddAnimation(data.animator, collection, spriteIds, customDirName, tk2dSpriteAnimationClip.WrapMode.Loop, 8);
                            }

                        }*/
                    }
                    else
                    {
                        if (((dirName.EndsWith("_hand") || dirName.EndsWith("_twohands"))) && file.Contains("cc_sprite_placeholder"))//textures.Count <= 0)
                        {
                            //BotsModule.Log($"hands located! lethal force engaged against {dirName}");
                            textures = ToolsCharApi.GetTexturesFromResource((path + "." + dirName).Replace("_twohands", "").Replace("_hand", ""), assembly ?? Assembly.GetCallingAssembly());
                        }
                        if (dirName.EndsWith("death_coop") && file.Contains("cc_sprite_placeholder"))//textures.Count <= 0)
                        {
                            //BotsModule.Log($"hands located! lethal force engaged against {dirName}");
                            textures = ToolsCharApi.GetTexturesFromResource((path + "." + dirName).Replace("_coop", ""), assembly ?? Assembly.GetCallingAssembly());
                        }
                        if (textures.Count > 0)
                        {

                            for (int i = 0; i < textures.Count; i++)
                            {
                                spriteIds.Add(AddSpriteToCollection(textures[i], collection, animName + i.ToString()));
                            }
                            //BotsModule.Log($"{dirName}: {textures.Count}/{spriteIds.Count}");

                            if (playerAnimInfo.ContainsKey(dirName.Replace("_armorless", "")))
                            {
                                //BotsModule.Log(spriteIds.Count.ToString());

                                var anim = SpriteHandler.AddAnimation(data.animator, collection, spriteIds, dirName, playerAnimInfo[dirName.Replace("_armorless", "")].First, playerAnimInfo[dirName.Replace("_armorless", "")].Second);

                                for (int i = 0; i <= anim.frames.Length; i++)
                                {
                                    if (anim.name.Contains("run_"))
                                    {
                                        if (i == 2 || i == 5)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_FS");
                                            anim.frames[i].eventAudio = "Play_FS";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }
                                    if (anim.name == "pitfall" || anim.name == "pitfall_down")
                                    {
                                        if (i == 0)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_Fall");
                                            anim.frames[i].eventAudio = "Play_Fall";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }
                                    if (anim.name == "pitfall_return")
                                    {
                                        if (i == 0)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_Respawn");
                                            anim.frames[i].eventAudio = "Play_Respawn";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }

                                    if (anim.name.Contains("dodge"))
                                    {
                                        if (i == 0)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_Leap");
                                            anim.frames[i].eventAudio = "Play_Leap";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                        if (i == 5)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_Roll");
                                            anim.frames[i].eventAudio = "Play_Roll";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }

                                    if (anim.name == "doorway")
                                    {
                                        if (i == 0)
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_CHR_boot_stairs_01");
                                            anim.frames[i].eventAudio = "Play_CHR_boot_stairs_01";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }

                                    if (anim.name == "pet")
                                    {
                                        

                                        if (i == 0)                                            
                                        {
                                           // ETGModConsole.Log($"({anim.frames.Length}){anim.name} [{i}]: Play_CHR_fool_voice_01");
                                            anim.frames[i].eventAudio = "Play_CHR_fool_voice_01";
                                            anim.frames[i].triggerEvent = true;
                                        }
                                    }
                                }

                                if (dirName.Contains("dodge"))
                                {
                                    for (int i = 0; i <= (anim.frames.Length / 2); i++)
                                    {
                                        anim.frames[i].invulnerableFrame = true;
                                        anim.frames[i].groundedFrame = false;
                                    }
                                }

                                if (dirName.Contains("tablekick"))
                                {
                                    for (int i = 0; i < (anim.frames.Length); i++)
                                    {
                                        anim.frames[i].invulnerableFrame = true;
                                    }
                                }

                                if (dirName.Contains("slide"))
                                {
                                    for (int i = 0; i < (anim.frames.Length); i++)
                                    {
                                        anim.frames[i].invulnerableFrame = true;
                                    }
                                }
                            }
                            else
                            {
                                ETGModConsole.Log($"No Anim data found for \"{dirName}\"! please double check animation folder names");
                            }
                        }
                        else
                        {
                            ETGModConsole.Log($"No sprites found in {dirName} please make sure youve actually put sprites in that folder");
                        }
                    }
                }
                else if (splitPath[splitPath.Count() - 2] == (alt ? "hand_alt_001" : "hand_001"))
                {
                    var texture = ToolsCharApi.GetTextureFromResource($"{path}.{(alt ? "hand_alt_001" : "hand_001")}.png", assembly ?? Assembly.GetCallingAssembly());
                    var id = AddSpriteToCollection(texture, collection, "hand" + (alt ? "_alt" : ""));
                    collection.spriteDefinitions[id].position0 = new Vector3(-0.125f, -0.125f, 0);
                    collection.spriteDefinitions[id].position1 = new Vector3(0.125f, -0.125f, 0);
                    collection.spriteDefinitions[id].position2 = new Vector3(-0.125f, 0.125f, 0);
                    collection.spriteDefinitions[id].position3 = new Vector3(0.125f, 0.125f, 0);
                    if (!alt)
                    {
                        player.primaryHand.sprite.Collection = data.collection;
                        player.secondaryHand.sprite.Collection = data.collection;

                        player.primaryHand.sprite.SetSprite(id);
                        player.secondaryHand.sprite.SetSprite(id);
                    }
                }
            }
            if (alt)
            {
                player.AlternateCostumeLibrary = data.animator.Library;
            }
            else
            {
                player.spriteAnimator.Library = data.animator.Library;

                player.sprite.Collection = data.collection;


                player.sprite.spriteId = data.collection.GetSpriteIdByName($"{data.nameShort}_idle0");

                //ToolsCharApi.ExportTexture(data.collection.spriteDefinitions[0].material.mainTexture, "SpriteDump/", data.nameShort);
            }

            //ToolsCharApi.ExportTexture(TextureStitcher.GetReadable(data.collection.textures[0]), "SpriteDump/balls", data.nameShort + UnityEngine.Random.Range(1, 10000));

            //foreach (var anim in data.animator.Library.clips)
            //{
            //    BotsModule.Log($"Name: {anim.name} - Fps: {anim.fps} - Frame Count: {anim.frames.Count()} - Loop Mode: {anim.wrapMode}");
            //}

        }


        /// <summary>
        /// Adds a sprite (from a texture) to a collection
        /// </summary>
        /// <returns>The spriteID of the defintion in the collection</returns>
        public static int AddSpriteToCollection(Texture2D texture, tk2dSpriteCollectionData collection, string name = "")
        {
            var definition = ConstructDefinition(texture); //Generate definition
            if (string.IsNullOrEmpty(name))
            {
                definition.name = texture.name; //naming the definition is actually extremely important 
            }
            else
            {
                definition.name = name; //naming the definition is actually extremely important 
            }


            return AddSpriteToCollection(definition, collection);
        }

        /// <summary>
        /// Adds a sprite (from a texture) to a collection
        /// </summary>
        /// <returns>The spriteID of the defintion in the collection</returns>
        public static int AddSpriteToCollectionWithAnchor(Texture2D texture, tk2dSpriteCollectionData collection, tk2dBaseSprite.Anchor anchor, string name = "")
        {
            var definition = ConstructDefinition(texture); //Generate definition
            if (string.IsNullOrEmpty(name))
            {
                definition.name = texture.name; //naming the definition is actually extremely important 
            }
            else
            {
                definition.name = name; //naming the definition is actually extremely important 
            }
            definition.ConstructOffsetsFromAnchor(anchor);

            return AddSpriteToCollection(definition, collection);
        }

        public static void ConstructOffsetsFromAnchorC(this tk2dSpriteDefinition def, tk2dBaseSprite.Anchor anchor, Vector2? scale = null, bool fixesScale = false, bool changesCollider = true)
        {
            if (!scale.HasValue)
            {
                scale = new Vector2?(def.position3);
            }
            if (fixesScale)
            {
                Vector2 fixedScale = scale.Value - def.position0.XY();
                scale = new Vector2?(fixedScale);
            }
            float xOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.LowerCenter || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.UpperCenter)
            {
                xOffset = -(scale.Value.x / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.LowerRight || anchor == tk2dBaseSprite.Anchor.MiddleRight || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                xOffset = -scale.Value.x;
            }
            float yOffset = 0;
            if (anchor == tk2dBaseSprite.Anchor.MiddleLeft || anchor == tk2dBaseSprite.Anchor.MiddleCenter || anchor == tk2dBaseSprite.Anchor.MiddleLeft)
            {
                yOffset = -(scale.Value.y / 2f);
            }
            else if (anchor == tk2dBaseSprite.Anchor.UpperLeft || anchor == tk2dBaseSprite.Anchor.UpperCenter || anchor == tk2dBaseSprite.Anchor.UpperRight)
            {
                yOffset = -scale.Value.y;
            }
            def.MakeOffset(new Vector2(xOffset, yOffset), changesCollider);
        }


        public static void MakeOffset(this tk2dSpriteDefinition def, Vector2 offset, bool changesCollider = false)
        {
            float xOffset = offset.x;
            float yOffset = offset.y;
            def.position0 += new Vector3(xOffset, yOffset, 0);
            def.position1 += new Vector3(xOffset, yOffset, 0);
            def.position2 += new Vector3(xOffset, yOffset, 0);
            def.position3 += new Vector3(xOffset, yOffset, 0);
            def.boundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.boundsDataExtents += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataCenter += new Vector3(xOffset, yOffset, 0);
            def.untrimmedBoundsDataExtents += new Vector3(xOffset, yOffset, 0);
            if (def.colliderVertices != null && def.colliderVertices.Length > 0 && changesCollider)
            {
                def.colliderVertices[0] += new Vector3(xOffset, yOffset, 0);
            }
        }

        /// <summary>
        /// Adds a sprite (from a texture) to a collection
        /// </summary>
        /// <returns>The spriteID of the defintion in the collection</returns>
        public static int AddSpriteToCollection(Texture2D texture, tk2dSpriteCollectionData collection, Material overrideMat, string name = "")
        {
            var definition = ConstructDefinition(texture, overrideMat); //Generate definition
            if (string.IsNullOrEmpty(name))
            {
                definition.name = texture.name; //naming the definition is actually extremely important 
            }
            else
            {
                definition.name = name; //naming the definition is actually extremely important 
            }


            return AddSpriteToCollection(definition, collection);
        }


        public static void HandleAnimations(PlayerController player, CustomCharacterData data)
        {
            var orig = player.sprite.Collection;
            var copyCollection = GameObject.Instantiate(orig);
            GameObject.DontDestroyOnLoad(copyCollection);



            
            tk2dSpriteDefinition[] copyDefinitions = new tk2dSpriteDefinition[orig.spriteDefinitions.Length];
            for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
            {
                copyDefinitions[i] = orig.spriteDefinitions[i].Copy();
            }
            copyCollection.spriteDefinitions = copyDefinitions;
            //ToolsCharApi.ExportTexture(TextureStitcher.GetReadable(copyCollection.textures[0]), "SpriteDump/balls", TextureStitcher.GetReadable(copyCollection.textures[0]).name + UnityEngine.Random.Range(1, 10000));
            
            if (data.playerSheet != null)
            {
                ToolsCharApi.Print("        Using sprite sheet replacement.", "FFBB00");
                var materialsToCopy = orig.materials;
                copyCollection.materials = new Material[orig.materials.Length];
                for (int i = 0; i < copyCollection.materials.Length; i++)
                {
                    if (materialsToCopy[i] == null) continue;
                    var mat = new Material(materialsToCopy[i]);
                    GameObject.DontDestroyOnLoad(mat);
                    mat.mainTexture = data.playerSheet;
                    mat.name = materialsToCopy[i].name;
                    copyCollection.materials[i] = mat;
                }

                for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
                {
                    foreach (var mat in copyCollection.materials)
                    {
                        if (mat != null && copyDefinitions[i].material.name.Equals(mat.name))
                        {
                            copyDefinitions[i].material = mat;
                            copyDefinitions[i].materialInst = new Material(mat);
                            //copyCollection.materials.co
                        }
                    }
                }
            }
            else if (data.sprites != null)
            {


                /*var materialsToCopy2 = orig.materials;
                copyCollection.materials = new Material[orig.materials.Length];
                for (int i = 0; i < copyCollection.materials.Length; i++)
                {
                    if (materialsToCopy2[i] == null) continue;
                    var mat = new Material(materialsToCopy2[i]);
                    GameObject.DontDestroyOnLoad(mat);
                    mat.mainTexture = data.playerSheet;
                    mat.name = materialsToCopy2[i].name;
                    copyCollection.materials[i] = mat;
                }

                for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
                {
                    foreach (var mat in copyCollection.materials)
                    {
                        if (mat != null && copyDefinitions[i].material.name.Equals(mat.name))
                        {
                            copyDefinitions[i].material = mat;
                            copyDefinitions[i].materialInst = new Material(mat);
                        }
                    }
                }*/

                ToolsCharApi.Print("        Using individual sprite replacement.", "FFBB00");
                bool notSlinger = data.baseCharacter != PlayableCharacters.Gunslinger;

                RuntimeAtlasPage page = new RuntimeAtlasPage();
                for (int i = 0; i < data.sprites.Count; i++)
                {
                    var tex = data.sprites[i];

                    float nw = (tex.width) / 16f;
                    float nh = (tex.height) / 16f;

                    var def = copyCollection.GetSpriteDefinition(tex.name);
                    if (def != null)
                    {

                        if (notSlinger && def.boundsDataCenter != Vector3.zero)
                        {
                            
                            var ras = page.Pack(tex);

                            //def.material = mat;
                            //def.materialInst = new Material(def.material);

                            def.materialInst.mainTexture = ras.texture;
                            def.material.mainTexture = ras.texture;

                            def.uvs = ras.uvs;
                            def.extractRegion = true;
                            def.position0 = new Vector3(0, 0, 0);
                            def.position1 = new Vector3(nw, 0, 0);
                            def.position2 = new Vector3(0, nh, 0);
                            def.position3 = new Vector3(nw, nh, 0);
                            //BotsModule.Log($"{def.name}: [{def.material.name}/{def.materialInst.name}], [{def.material.shader.name}/{def.materialInst.shader.name}]");

                            def.boundsDataCenter = new Vector2(nw / 2, nh / 2);
                            def.untrimmedBoundsDataCenter = def.boundsDataCenter;

                            def.boundsDataExtents = new Vector2(nw, nh);
                            def.untrimmedBoundsDataExtents = def.boundsDataExtents;
                        }
                        else
                        {
                            //ETGModConsole.Log("god fucking damn it", "#fa0000");
                            def.ReplaceTexture(tex);
                        }
                    }
                }

                


                page.Apply();
                //ToolsCharApi.ExportTexture(TextureStitcher.GetReadable(page.Texture), "SpriteDump/balls", TextureStitcher.GetReadable(copyCollection.textures[0]).name + UnityEngine.Random.Range(1, 10000));

                var materialsToCopy = orig.materials;
                copyCollection.materials = new Material[orig.materials.Length];
                copyCollection.materialInsts = new Material[orig.materials.Length];
                for (int i = 0; i < copyCollection.materials.Length; i++)
                {
                    if (materialsToCopy[i] == null) continue;
                    var mat = new Material(materialsToCopy[i]);
                    GameObject.DontDestroyOnLoad(mat);
                    mat.mainTexture = page.Texture;
                    mat.name = materialsToCopy[i].name;
                    copyCollection.materials[i] = mat;
                    copyCollection.materialInsts[i] = mat;
                    
                }
                copyCollection.textures = new Texture[] { page.Texture };
                for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
                {
                    foreach (var mat in copyCollection.materials)
                    {
                        if (mat != null && copyDefinitions[i].material.name.Equals(mat.name))
                        {
                            copyDefinitions[i].material = copyCollection.materials[0];
                            copyDefinitions[i].materialInst = new Material(copyCollection.materials[0]);

                            
                        }
                    }
                }
                copyCollection.textureFilterMode = FilterMode.Point;
                for (int i = 0; i < player.sprite.Collection.spriteDefinitions.Length; i++)
                {
                    //BotsModule.Log($"[{player.sprite.Collection.spriteDefinitions[i].materialId}]: {player.sprite.Collection.spriteDefinitions[i].name} -===- {player.sprite.Collection.spriteDefinitions[i].material} -===- {player.sprite.Collection.spriteDefinitions[i].material.mainTexture}", "#00d9ff");
                    //BotsModule.Log($"[{copyDefinitions[i].materialId}]: {copyDefinitions[i].name} -===- {copyDefinitions[i].material} -===- {copyDefinitions[i].material.mainTexture}", "#a100ba");
                }

            }
            else
            {
                ToolsCharApi.Print("        Not replacing sprites.", "FFFF00");
            }

            

            player.spriteAnimator.Library = GameObject.Instantiate(player.spriteAnimator.Library);
            GameObject.DontDestroyOnLoad(player.spriteAnimator.Library);

           

            foreach (var clip in player.spriteAnimator.Library.clips)
            {
                if(clip.fps != 12)
                {
                    //BotsModule.Log($"{clip.name}: {clip.fps}");
                }

                for (int i = 0; i < clip.frames.Length; i++)
                {
                    if (clip.frames[i].invulnerableFrame)
                    {
                        //BotsModule.Log($"{clip.name} {i}: {clip.frames[i].invulnerableFrame}");
                    }

                    clip.frames[i].spriteCollection = copyCollection;
                }
            }
            foreach (var sdef in copyCollection.materials)
            {
                //BotsModule.Log("Norm: " + sdef.mainTexture.ToString());
                //BotsModule.Log("Norm: " + sdef.ToString());
            }

            foreach (var sdef in copyCollection.materialInsts)
            {
               // BotsModule.Log("Inst: " + sdef.mainTexture.ToString());
                //BotsModule.Log("Inst: " + sdef.ToString());
            }

            copyCollection.name = player.OverrideDisplayName;

            player.primaryHand.sprite.Collection = copyCollection;
            player.secondaryHand.sprite.Collection = copyCollection;
            player.sprite.Collection = copyCollection;
        }

        public static void HandleAltAnimations(PlayerController player, CustomCharacterData data)
        {
            tk2dSpriteCollectionData orig = null;
            if (player.AlternateCostumeLibrary?.clips[0]?.frames[0]?.spriteCollection != null)
            {
                orig = player.AlternateCostumeLibrary.clips[0].frames[0].spriteCollection;
            }
            else
            {
                orig = player.sprite.Collection;
            }
            
            

            var copyCollection = GameObject.Instantiate(orig);
            GameObject.DontDestroyOnLoad(copyCollection);



            tk2dSpriteDefinition[] copyDefinitions = new tk2dSpriteDefinition[orig.spriteDefinitions.Length];
            for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
            {
                copyDefinitions[i] = orig.spriteDefinitions[i].Copy();
            }
            copyCollection.spriteDefinitions = copyDefinitions;

            if (data.altPlayerSheet != null)
            {
                ToolsCharApi.Print("        Using sprite sheet replacement.", "FFBB00");
                var materialsToCopy = orig.materials;
                copyCollection.materials = new Material[orig.materials.Length];
                for (int i = 0; i < copyCollection.materials.Length; i++)
                {
                    if (materialsToCopy[i] == null) continue;
                    var mat = new Material(materialsToCopy[i]);
                    GameObject.DontDestroyOnLoad(mat);
                    mat.mainTexture = data.altPlayerSheet;
                    mat.name = materialsToCopy[i].name;
                    copyCollection.materials[i] = mat;
                }

                for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
                {
                    foreach (var mat in copyCollection.materials)
                    {
                        if (mat != null && copyDefinitions[i].material.name.Equals(mat.name))
                        {
                            copyDefinitions[i].material = mat;
                            copyDefinitions[i].materialInst = new Material(mat);
                        }
                    }
                }
            }
            
            else if (data.altSprites != null)
            {
                //BotsModule.Log("altSprites arent null thank god!");
                ToolsCharApi.Print("        Using individual sprite replacement.", "FFBB00");
                bool notSlinger = data.baseCharacter != PlayableCharacters.Gunslinger;

                RuntimeAtlasPage page = new RuntimeAtlasPage();
                for (int i = 0; i < data.altSprites.Count; i++)
                {
                    var tex = data.altSprites[i];

                    float nw = (tex.width) / 16f;
                    float nh = (tex.height) / 16f;

                    var def = copyCollection.GetSpriteDefinition(tex.name);
                    if (def != null)
                    {
                        //BotsModule.Log("def isnt null thank god!");
                        if (notSlinger && def.boundsDataCenter != Vector3.zero)
                        {
                            var ras = page.Pack(tex);
                            def.materialInst.mainTexture = ras.texture;
                            //def.material.mainTexture = ras.texture;
                            def.uvs = ras.uvs;
                            def.extractRegion = true;
                            def.position0 = new Vector3(0, 0, 0);
                            def.position1 = new Vector3(nw, 0, 0);
                            def.position2 = new Vector3(0, nh, 0);
                            def.position3 = new Vector3(nw, nh, 0);

                            //BotsModule.Log("(alt) " + def.name + ": " + def.material.name + ", " + def.material.shader.name);

                            def.boundsDataCenter = new Vector2(nw / 2, nh / 2);
                            def.untrimmedBoundsDataCenter = def.boundsDataCenter;

                            def.boundsDataExtents = new Vector2(nw, nh);
                            def.untrimmedBoundsDataExtents = def.boundsDataExtents;
                        }
                        else
                        {
                            def.ReplaceTexture(tex);
                        }
                        //BotsModule.Log("def copy shit done thank god!");
                    }
                }
                //BotsModule.Log("pre applying def shit idfk! thank god!");
                page.Apply();
            }
            else
            {
                ToolsCharApi.Print("        Not replacing sprites.", "FFFF00");
            }
            //BotsModule.Log("balls wide... thank god!");
            player.AlternateCostumeLibrary = GameObject.Instantiate(player.AlternateCostumeLibrary);
            GameObject.DontDestroyOnLoad(player.AlternateCostumeLibrary);

            foreach (var clip in player.AlternateCostumeLibrary.clips)
            {
                for (int i = 0; i < clip.frames.Length; i++)
                {
                    clip.frames[i].spriteCollection = copyCollection;
                }
            }


            data.AlternateCostumeLibrary = player.AlternateCostumeLibrary;

            copyCollection.name = player.OverrideDisplayName + "_alt";

            //player.primaryHand.sprite.Collection = copyCollection;
            //player.secondaryHand.sprite.Collection = copyCollection;
            //player.sprite.Collection = copyCollection;
        }

        public static Vector2[] GetMarginUVS(Texture2D orig, Texture2D margined)
        {
            int padding = TextureStitcher.padding;

            //float xOff = 0;
            //float yOff = 0;
            float xOff = (float)(padding) / (margined.width);
            float yOff = (float)(padding) / (margined.height);

            float w = (float)(orig.width) / (margined.width);
            float h = (float)(orig.height) / (margined.height);

            return new Vector2[]
            {
                new Vector2(xOff, yOff),
                new Vector2(xOff + w, yOff),
                new Vector2(xOff, yOff + h),
                new Vector2(xOff + w, yOff + h)
            };
        }

        public static Vector2[] defaultUVS = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };


        public static void HandleMinimapIcons(PlayerController player, CustomCharacterData data)
        {
            player.minimapIconPrefab = GameObject.Instantiate(player.minimapIconPrefab);
            var minimapSprite = player.minimapIconPrefab.GetComponent<tk2dSprite>();
            GameObject.DontDestroyOnLoad(minimapSprite);

            string iconName = "Player_" + player.name + "_001";
            int id = minimapSprite.Collection.GetSpriteIdByName(iconName, -1); //return -1 if not found

            if (id < 0)
            {
                var spriteDef = minimapSprite.GetCurrentSpriteDef();
                var copy = spriteDef.Copy();
                copy.ReplaceTexture(data.minimapIcon);
                copy.name = iconName;
                id = AddSpriteToCollection(copy, minimapSprite.Collection);
            }
            else
            {
                ToolsCharApi.Print("Minimap icon for " + iconName + " already found, not generating a new one");
            }

            //SetMinimapIconSpriteID(minimapSprite.spriteId, id);
            minimapSprite.SetSprite(id);
        }

        public static void HandleBossCards(PlayerController player, CustomCharacterData data)
        {
            //int count = player.BosscardSprites.Count;
            player.BosscardSprites = data.bossCard;
        }

        public static void HandleFacecards(PlayerController player, CustomCharacterData data)
        {


            

            /*var atlas = uiAtlas;
            var atlasTex = atlas.Texture;

            dfAtlas.ItemInfo info = new dfAtlas.ItemInfo();
            info.name = player.name + "_facecard";
            info.region = TextureStitcher.AddFaceCardToAtlas(data.faceCard, atlasTex, uiFaceCards.Count, uiFacecardBounds);
            info.sizeInPixels = faceCardSizeInPixels;

            atlas.AddItem(info);

            if (atlas.Replacement)
            {
                atlas.Replacement.Material.mainTexture = atlasTex;
            }*/
            var sprite = uiAtlas.AddNewItemToAtlas(data.faceCard, player.name + "_facecard");
            uiFaceCards.Add(sprite);
        }

        /*public static void HandlePunchoutSprites(PunchoutPlayerController player, CustomCharacterData data)
        {
            var primaryPlayer = GameManager.Instance.PrimaryPlayer;
            player.PlayerUiSprite.Atlas = uiAtlas;

            if (data != null)
            {
                if (data.punchoutSprites != null && player.sprite.Collection.name != (data.nameShort + " Punchout Collection"))
                    HandlePunchoutAnimations(player, data);

                if (data.faceCard != null)
                {
                    player.PlayerUiSprite.SpriteName = data.nameInternal + "_punchout_facecard1";
                }

            }
        }*/

        public static void HandlePunchoutFaceCards(CustomCharacterData data)
        {
            var atlas = uiAtlas;
            var atlasTex = atlas.Texture;
            if (data.punchoutFaceCards != null)
            {
                ToolsCharApi.Print("Adding punchout facecards");
                int count = Mathf.Min(data.punchoutFaceCards.Count, 3);
                for (int i = 0; i < count; i++)
                {
                    var sprite = uiAtlas.AddNewItemToAtlas(data.punchoutFaceCards[i], $"punch_player_health_{data.nameShort.ToLower()}_00{i + 1}");
                    //dfAtlas.ItemInfo info = new dfAtlas.ItemInfo();
                    //info.name = data.nameInternal + "_punchout_facecard" + (i + 1);
                    //info.region = TextureStitcher.AddFaceCardToAtlas(data.punchoutFaceCards[i], atlasTex, uiFaceCards.Count, uiFacecardBounds);
                    //info.sizeInPixels = faceCardSizeInPixels;

                    //atlas.AddItem(info);
                    uiFaceCards.Add(sprite);
                }
            }
        }

        public static void HandlePunchoutAnimations(PunchoutPlayerController player, CustomCharacterData data)
        {
            ToolsCharApi.Print("Replacing punchout sprites...");

            var orig = player.sprite.Collection;
            var copyCollection = GameObject.Instantiate(orig);

            GameObject.DontDestroyOnLoad(copyCollection);

            tk2dSpriteDefinition[] copyDefinitions = new tk2dSpriteDefinition[orig.spriteDefinitions.Length];
            for (int i = 0; i < copyCollection.spriteDefinitions.Length; i++)
            {
                copyDefinitions[i] = orig.spriteDefinitions[i].Copy();
            }
            copyCollection.spriteDefinitions = copyDefinitions;

            foreach (var tex in data.punchoutSprites)
            {
                var def = copyCollection.GetSpriteDefinition(tex.Value.name);
                if (def != null)
                {
                    def.ReplaceTexture(tex.Value.CropWhiteSpace());
                }
            }

            player.spriteAnimator.Library = GameObject.Instantiate(player.spriteAnimator.Library);
            GameObject.DontDestroyOnLoad(player.spriteAnimator.Library);

            foreach (var clip in player.spriteAnimator.Library.clips)
            {
                for (int i = 0; i < clip.frames.Length; i++)
                {
                    clip.frames[i].spriteCollection = copyCollection;
                }
            }

            copyCollection.name = data.nameShort + " Punchout Collection";
            //CharacterBuilder.storedCollections.Add(data.nameInternal, copyCollection);
            player.sprite.Collection = copyCollection;
            ToolsCharApi.Print("Punchout sprites successfully replaced");
        }

        public static void SetMinimapIconSpriteID(int key, int value)
        {
            if (Minimap.HasInstance)
            {
                var playerMarkers = (List<Tuple<Transform, Renderer>>)m_playerMarkers.GetValue(Minimap.Instance);
                foreach (var marker in playerMarkers)
                {
                    var sprite = marker.First.gameObject.GetComponent<tk2dSprite>();
                    if (sprite != null && sprite.spriteId == key)
                    {
                        sprite.SetSprite(value);
                    }
                }
            }
        }

        public static tk2dSpriteDefinition Copy(this tk2dSpriteDefinition orig)
        {
            tk2dSpriteDefinition copy = new tk2dSpriteDefinition()
            {
                boundsDataCenter = orig.boundsDataCenter,
                boundsDataExtents = orig.boundsDataExtents,
                colliderConvex = orig.colliderConvex,
                colliderSmoothSphereCollisions = orig.colliderSmoothSphereCollisions,
                colliderType = orig.colliderType,
                colliderVertices = orig.colliderVertices,
                collisionLayer = orig.collisionLayer,
                complexGeometry = orig.complexGeometry,
                extractRegion = orig.extractRegion,
                flipped = orig.flipped,
                indices = orig.indices,
                material = orig.material ? new Material(orig.material) : null,
                materialId = orig.materialId,
                materialInst = orig.materialInst ? new Material(orig.materialInst) : null,
                metadata = orig.metadata,
                name = orig.name,
                normals = orig.normals,
                physicsEngine = orig.physicsEngine,
                position0 = orig.position0,
                position1 = orig.position1,
                position2 = orig.position2,
                position3 = orig.position3,
                regionH = orig.regionH,
                regionW = orig.regionW,
                regionX = orig.regionX,
                regionY = orig.regionY,
                tangents = orig.tangents,
                texelSize = orig.texelSize,
                untrimmedBoundsDataCenter = orig.untrimmedBoundsDataCenter,
                untrimmedBoundsDataExtents = orig.untrimmedBoundsDataExtents,
                uvs = orig.uvs
            };
            return copy;
        }

        public static tk2dSpriteDefinition CopyToSelf(this tk2dSpriteDefinition self, tk2dSpriteDefinition targetToCopy)
        {
            self.colliderConvex = targetToCopy.colliderConvex;
            self.colliderSmoothSphereCollisions = targetToCopy.colliderSmoothSphereCollisions;
            self.colliderType = targetToCopy.colliderType;
            self.colliderVertices = targetToCopy.colliderVertices;
            self.collisionLayer = targetToCopy.collisionLayer;
            //self.complexGeometry = targetToCopy.complexGeometry;
            //self.extractRegion = targetToCopy.extractRegion;
            //self.flipped = targetToCopy.flipped;
            self.indices = targetToCopy.indices;
            self.material.shader = targetToCopy.material?.shader != null ? targetToCopy.material?.shader : self.material.shader;
            //self.materialId = targetToCopy.materialId;
            self.materialInst.shader = targetToCopy.materialInst?.shader != null ? targetToCopy.materialInst?.shader : self.materialInst.shader;
            self.metadata = targetToCopy.metadata;
            //self.normals = targetToCopy.normals;
            self.physicsEngine = targetToCopy.physicsEngine;
            self.position0 = targetToCopy.position0;
            self.position1 = targetToCopy.position1;
            self.position2 = targetToCopy.position2;
            self.position3 = targetToCopy.position3;
            //self.regionH = targetToCopy.regionH;
            //self.regionW = targetToCopy.regionW;
            //self.regionX = targetToCopy.regionX;
            //self.regionY = targetToCopy.regionY;
            //self.tangents = targetToCopy.tangents;
            //self.texelSize = targetToCopy.texelSize;
            //self.uvs = targetToCopy.uvs;//
            return self;
        }


        public static tk2dSpriteAnimationClip CopyOf(tk2dSpriteAnimationClip orig)
        {
            return new tk2dSpriteAnimationClip(orig);
        }

        public static int AddSpriteToCollection(tk2dSpriteDefinition spriteDefinition, tk2dSpriteCollectionData collection)
        {
            //Add definition to collection
            var defs = collection.spriteDefinitions;
            var newDefs = defs.Concat(new tk2dSpriteDefinition[] { spriteDefinition }).ToArray();
            collection.spriteDefinitions = newDefs;

            //Reset lookup dictionary
            spriteNameLookup.SetValue(collection, null);  //Set dictionary to null
            collection.InitDictionary(); //InitDictionary only runs if the dictionary is null
            return newDefs.Length - 1;
        }
    }

    public sealed class Tuple<T1, T2, T3, T4>
    {
        
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", this.Item1, this.Item2, this.Item3, this.Item4);
        }

        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
    }

}
