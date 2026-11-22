using Dungeonator;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Alexandria.DungeonAPI.DungeonFactory;

namespace Alexandria.DungeonAPI
{
    public static class DungeonFactory
    {
        public static Dictionary<string, DungeonData> CustomRegisteredDungeons = new Dictionary<string, DungeonData>();

        /// <summary>
        /// Registers a new Dungeon level with Alexandria.
        /// </summary>
        /// <param name="DungeonKey">
        /// The key used to load the Dungeon. 
        /// Standard vanilla names are formatted as 'base_level' for regular chambers or 'finalscenario_character' for pasts.
        /// EG; base_castle, finalscenario_robot, etc
        /// </param>
        /// <param name="FloorName">The name of the level. Shown in the level loaded pop-up when entering the level.</param>
        /// <param name="FloorNameShort">A shortened name for the level. Shown only on the death Ammonomicon page as the location if you die in this level.</param>
        /// <param name="LevelSubtitle">The subtitle for the level. Shown in the level loaded pop-up when entering the level.</param>
        public static DungeonData RegisterCustomDungeon(
            string DungeonKey,
            string FloorName,
            string FloorNameShort,
            string LevelSubtitle,
            GlobalDungeonData.ValidTilesets tilesetID,
            tk2dSpriteCollectionData spriteCollection,
            Dictionary<DungeonMaterial, int> DungeonMaterials,
            List<DungeonFlow> flows,
            GameLevelDefinition levelDefinition,
            bool isPast = false,
            string musicEventName = "Play_MUS_Castle_Theme_01",
            bool generateLights = true,
            Color? ambientLightColor = null,
            Color? ambientLightColorTwo = null,
            DustUpVFX customDustups = null,
            bool StripPlayerOnArrival = false,
            int masterRoundID = -1,
            string dungeonPrefabTemplate = "Base_Gungeon"
        )
        {
            DungeonData newDungeon = new DungeonData
            {
                DungeonKey = DungeonKey,
                FloorName = FloorName,
                FloorNameShort = FloorNameShort,
                LevelSubtitle = LevelSubtitle,
                IsPast = isPast,

                //Audio
                musicEventName = musicEventName,

                //Decoration 
                DungeonMaterials = DungeonMaterials,
                generateLights = generateLights,
                ambientLightColor = ambientLightColor != null ? (Color)ambientLightColor : new Color(0.5f, 0.5f, 0.5f),
                ambientLightColorTwo = ambientLightColorTwo != null ? (Color)ambientLightColorTwo : new Color(0.5f, 0.5f, 0.5f),
                customDustups = customDustups,

                //Tileset
                tilesetID = tilesetID,
                tilesetSpriteCollection = spriteCollection,

                //Dungeon Flows
                flows = flows,

                //Other
                MasterRoundID = masterRoundID,
                StripPlayerOnArrival = StripPlayerOnArrival,
                levelDefinition = levelDefinition,
                dungeonPrefabTemplate = dungeonPrefabTemplate,
            };
            CustomRegisteredDungeons.Add(DungeonKey, newDungeon);
            return newDungeon;
        }
        public class DungeonData
        {
            public string DungeonKey;
            public string FloorName;
            public string FloorNameShort;
            public string LevelSubtitle;
            public bool IsPast;

            //Audio
            public string musicEventName;

            //Decoration 
            public Dictionary<DungeonMaterial, int> DungeonMaterials;
            public bool generateLights;
            public Color ambientLightColor;
            public Color ambientLightColorTwo;
            public DustUpVFX customDustups;

            //Tileset
            public GlobalDungeonData.ValidTilesets tilesetID;
            public tk2dSpriteCollectionData tilesetSpriteCollection;

            //Dungeon Flows
            public List<DungeonFlow> flows;

            //Other
            public int MasterRoundID;
            public bool StripPlayerOnArrival;

            public GameLevelDefinition levelDefinition;
            public string dungeonPrefabTemplate;
        }
        private static Dungeon BuildLoadCustomDungeon(DungeonData data, Dungeon templateDungeon)
        {
            Dungeon MinesDungeonPrefab = GetOrLoadByName_Orig("Base_Mines");

            templateDungeon.gameObject.name = data.DungeonKey;
            templateDungeon.contentSource = ContentSource.CONTENT_UPDATE_03;
            templateDungeon.DungeonSeed = 0;
            templateDungeon.DungeonFloorName = data.FloorName;
            templateDungeon.DungeonShortName = data.FloorNameShort;
            templateDungeon.DungeonFloorLevelTextOverride = data.LevelSubtitle;
            templateDungeon.LevelOverrideType = GameManager.LevelOverrideState.NONE;
            templateDungeon.debugSettings = new DebugDungeonSettings()
            {
                RAPID_DEBUG_DUNGEON_ITERATION_SEEKER = false,
                RAPID_DEBUG_DUNGEON_ITERATION = false,
                RAPID_DEBUG_DUNGEON_COUNT = 5,

                GENERATION_VIEWER_MODE = false,
                FULL_MINIMAP_VISIBILITY = true,
                COOP_TEST = false,
                DISABLE_ENEMIES = false,
                DISABLE_LOOPS = true,
                DISABLE_SECRET_ROOM_COVERS = false,
                DISABLE_OUTLINES = false,
                WALLS_ARE_PITS = false
            };
            templateDungeon.ForceRegenerationOfCharacters = false;
            templateDungeon.ActuallyGenerateTilemap = true;

            templateDungeon.LevelOverrideType = data.IsPast ? GameManager.LevelOverrideState.CHARACTER_PAST : GameManager.LevelOverrideState.NONE;

            TilemapDecoSettings deco = templateDungeon.decoSettings;
            deco.ambientLightColor = data.ambientLightColor;
            deco.ambientLightColorTwo = data.ambientLightColorTwo;
            deco.generateLights = data.generateLights;

            //Dungeon Materials
            List<DungeonMaterial> registeredMaterials = new List<DungeonMaterial>();
            List<WeightedInt> materialWeights = new List<WeightedInt>();
            int matIndex = 0;
            foreach(DungeonMaterial mat in data.DungeonMaterials.Keys)
            {
                materialWeights.Add(new WeightedInt()
                {
                    value = matIndex,
                    weight = data.DungeonMaterials.ElementAt(matIndex).Value,
                    annotation = $"{data.FloorNameShort} Level Dungeon Material {matIndex}"
                });
                registeredMaterials.Add(mat);
                matIndex++;
            }
            templateDungeon.roomMaterialDefinitions = registeredMaterials.ToArray();
            WeightedIntCollection materialIntCollection = new WeightedIntCollection();
            materialIntCollection.elements = materialWeights.ToArray();
            deco.standardRoomVisualSubtypes = materialIntCollection;


            templateDungeon.tileIndices = new TileIndices()
            {
                tilesetId = data.tilesetID,
                dungeonCollection = data.tilesetSpriteCollection,
                dungeonCollectionSupportsDiagonalWalls = false,
                aoTileIndices = MinesDungeonPrefab.tileIndices.aoTileIndices,
                placeBorders = true,
                placePits = false,

                chestHighWallIndices = new List<TileIndexVariant>() {
                    new TileIndexVariant() {
                        index = 41,
                        likelihood = 0.5f,
                        overrideLayerIndex = 0,
                        overrideIndex = 0
                    }
                },
                decalIndexGrid = null,
                edgeDecorationTiles = null,
            };


            //Dustup VFX
            if (DefaultDungeonDustUps == null)
            {
                DefaultDungeonDustUps = new DustUpVFX()
                {
                    runDustup = MinesDungeonPrefab.dungeonDustups.runDustup,
                    waterDustup = MinesDungeonPrefab.dungeonDustups.waterDustup,
                    additionalWaterDustup = MinesDungeonPrefab.dungeonDustups.additionalWaterDustup,
                    rollNorthDustup = MinesDungeonPrefab.dungeonDustups.rollNorthDustup,
                    rollNorthEastDustup = MinesDungeonPrefab.dungeonDustups.rollNorthEastDustup,
                    rollEastDustup = MinesDungeonPrefab.dungeonDustups.rollEastDustup,
                    rollSouthEastDustup = MinesDungeonPrefab.dungeonDustups.rollSouthEastDustup,
                    rollSouthDustup = MinesDungeonPrefab.dungeonDustups.rollSouthDustup,
                    rollSouthWestDustup = MinesDungeonPrefab.dungeonDustups.rollSouthWestDustup,
                    rollWestDustup = MinesDungeonPrefab.dungeonDustups.rollWestDustup,
                    rollNorthWestDustup = MinesDungeonPrefab.dungeonDustups.rollNorthWestDustup,
                    rollLandDustup = MinesDungeonPrefab.dungeonDustups.rollLandDustup
                };
            }
            templateDungeon.dungeonDustups = data.customDustups != null ? data.customDustups : DefaultDungeonDustUps;

            //Flows
            templateDungeon.PatternSettings = new SemioticDungeonGenSettings()
            {
                flows = data.flows,
                mandatoryExtraRooms = new List<ExtraIncludedRoomData>(0),
                optionalExtraRooms = new List<ExtraIncludedRoomData>(0),
                MAX_GENERATION_ATTEMPTS = 5,

                DEBUG_RENDER_CANVASES_SEPARATELY = false
            };

            //dungeon.stampData = DungeonStampDataSetup.InitCustomDungeonStampData();

            //Other
            templateDungeon.StripPlayerOnArrival = data.StripPlayerOnArrival;
            templateDungeon.musicEventName = data.musicEventName;
            templateDungeon.BossMasteryTokenItemId = data.MasterRoundID;

            //Floor Idea Stuff
            // No idea what this does or why we need to set it, especially since we're telling the floor to not use it- but it surely can't hurt to get all our ducks in a row, even ducks we're not using - NN
            templateDungeon.UsesCustomFloorIdea = false;
            templateDungeon.FloorIdea = new RobotDaveIdea()
            {
                ValidEasyEnemyPlaceables = new DungeonPlaceable[0],
                ValidHardEnemyPlaceables = new DungeonPlaceable[0],
                UseWallSawblades = false,
                UseRollingLogsVertical = true,
                UseRollingLogsHorizontal = true,
                UseFloorPitTraps = false,
                UseFloorFlameTraps = true,
                UseFloorSpikeTraps = true,
                UseFloorConveyorBelts = true,
                UseCaveIns = true,
                UseAlarmMushrooms = false,
                UseChandeliers = true,
                UseMineCarts = false,
                CanIncludePits = false
            };

            //Weird niche settings
            templateDungeon.damageTypeEffectMatrix = MinesDungeonPrefab.damageTypeEffectMatrix; //Defines special wall impact VFX for specific projectile damage types.
            templateDungeon.pathGridDefinitions = new List<TileIndexGrid>() { MinesDungeonPrefab.pathGridDefinitions[0] };
            templateDungeon.dungeonWingDefinitions = new DungeonWingDefinition[0];
            templateDungeon.baseChestContents = MinesDungeonPrefab.baseChestContents;

            MinesDungeonPrefab = null;

            return templateDungeon;
        }
        private static DustUpVFX DefaultDungeonDustUps;
        private static Dungeon GetOrLoadByName_Orig(string name)
        {
            AssetBundle assetBundle = ResourceManager.LoadAssetBundle("dungeons/" + name.ToLower());
            return assetBundle.LoadAsset<GameObject>(name).GetComponent<Dungeon>();
        }

        //Patches the vanilla GetOrLoadByName method to return our custom registered dungeon.
        [HarmonyPatch(typeof(DungeonDatabase), nameof(DungeonDatabase.GetOrLoadByName))]
        private class GetOrLoadCustomDungeonPatch
        {
            static bool Prefix(string name, ref Dungeon __result)
            {
                Debug.Log($"ALEXANDRIA - DungeonFactory: Loading Dungeon '{name}'");
                if (CustomRegisteredDungeons.ContainsKey(name))
                {
                    Debug.Log($"ALEXANDRIA - DungeonFactory: Dungeon '{name}' corresponds to a custom dungeon registered with Alexandria.");
                    DungeonData dat = CustomRegisteredDungeons[name];

                    // fixes a bug where returning to the breach deletes the floor, thanks Bunny!
                    if (dat.levelDefinition != null && !GameManager.Instance.customFloors.Contains(dat.levelDefinition))
                    {
                        GameManager.Instance.customFloors.Add(dat.levelDefinition);
                    }

                    Dungeon dungeon = null;
                    try
                    {
                        dungeon = BuildLoadCustomDungeon(dat, GetOrLoadByName_Orig(dat.dungeonPrefabTemplate ?? "Base_Gungeon"));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"ALEXANDRIA - DungeonFactory: {e}");
                    }
                    if (!dungeon) return true; // fall back on original method

                    __result = dungeon;
                    return false; // skip original method

                }
                else
                {
                    //If CustomRegisteredDungeons does not contain our key, we allow the original method to run.
                    return true;
                }  
            }
        }
    }
}
