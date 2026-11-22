using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public static class TilesetBuilder
    {
        /// <summary>
        /// Sets up proper collision information and some metadata for the necessary tiles of a tileset.
        /// </summary>
        /// <param name="tilesetSpriteCollection">The sprite collection of the tileset.</param>
        /// <param name="topFaceWalls">The sprite collection of the tileset's upper northern facewalls.</param>
        /// <param name="bottomFacewalls">The sprite names for the tileset's lower northern facewalls.</param>
        /// <param name="ceilingBorders">The sprite names for the tileset's upper ceiling border tiles.</param>
        /// <param name="floorTiles">The sprite names of the tileset's floor tiles.</param>
        public static void SetupTilesetCollisions(tk2dSpriteCollectionData tilesetSpriteCollection, List<string> topFaceWalls, List<string> bottomFacewalls, List<string> ceilingBorders, List<string> floorTiles)
        {
            Texture texture = tilesetSpriteCollection.materials[0].GetTexture("_MainTex"); //Fetches the main texture of the material.
            texture.filterMode = FilterMode.Point;

            //==== Copies the collection materials of an existing floor
            var orLoadByName = DungeonDatabase.GetOrLoadByName("Finalscenario_Soldier");

            Material transparentMat = new Material(orLoadByName.tileIndices.dungeonCollection.materials[1]);
            transparentMat.SetTexture("_MainTex", texture);

            Material defaultMat = new Material(orLoadByName.tileIndices.dungeonCollection.materials[0]);
            defaultMat.SetTexture("_MainTex", texture);
            orLoadByName = null;
            //====

            tilesetSpriteCollection.materials = new Material[] { defaultMat, transparentMat };
            tilesetSpriteCollection.materialInsts = new Material[] { defaultMat, transparentMat };

            tilesetSpriteCollection.textures = new Texture[]
            {
               texture
            };

            for (int me = 1; me < tilesetSpriteCollection.Count; me++)
            {
                tilesetSpriteCollection.SetMaterial(me, 1);
            }
            for (int me = 1; me < tilesetSpriteCollection.Count; me++)
            {
                tilesetSpriteCollection.spriteDefinitions[me].metadata = new TilesetIndexMetadata();
                tilesetSpriteCollection.spriteDefinitions[me].metadata.SetupTileMetaData((TilesetIndexMetadata.TilesetFlagType)0, 0f, 1, 0, -1, false, true);
            }


            foreach (string tilename in topFaceWalls)
            {
                int id = tilesetSpriteCollection.GetSpriteIdByName(tilename);
                SetupTileCollision(tilesetSpriteCollection, id, TileSetupType.HIGH_FACEWALL);
                tilesetSpriteCollection.spriteDefinitions[id].metadata.SetupTileMetaData(TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, 1f, 0, -1, -1, false, false);
                tilesetSpriteCollection.SetMaterial(id, 0);
            }

            foreach (string tilename in ceilingBorders)
            {
                int id = tilesetSpriteCollection.GetSpriteIdByName(tilename);
                SetupTileCollision(tilesetSpriteCollection, id, TileSetupType.WALL_BORDER);
                tilesetSpriteCollection.SetMaterial(id, 0);
            }

            foreach (string tilename in bottomFacewalls)
            {
                int id = tilesetSpriteCollection.GetSpriteIdByName(tilename);
                SetupTileCollision(tilesetSpriteCollection, id, TileSetupType.LOW_FACEWALL);
                tilesetSpriteCollection.spriteDefinitions[id].metadata.SetupTileMetaData(TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, 1f, 0, -1, -1, false, false);
                tilesetSpriteCollection.SetMaterial(id, 0);
            }

            foreach (string tilename in floorTiles)
            {
                int id = tilesetSpriteCollection.GetSpriteIdByName(tilename);
                tilesetSpriteCollection.spriteDefinitions[id].metadata.SetupTileMetaData(TilesetIndexMetadata.TilesetFlagType.FLOOR_TILE, 1f, 0, -1, -1, false, true);
                tilesetSpriteCollection.SetMaterial(id, 0);
            }
        }

        /// <summary>
        /// Creates a blank TileIndexGrid with all tile indices set to an empty (-1) tile.
        /// </summary>
        public static TileIndexGrid CreateTileIndexGrid()
        {
            var indexGrid = ScriptableObject.CreateInstance<TileIndexGrid>();
            var yes = new TileIndexList { indexWeights = new List<float> { 0.1f }, indices = new List<int> { -1 } };

            indexGrid.topLeftIndices = yes;
            indexGrid.topIndices = yes;
            indexGrid.topRightIndices = yes;
            indexGrid.leftIndices = yes;
            indexGrid.centerIndices = yes;
            indexGrid.rightIndices = yes;
            indexGrid.bottomLeftIndices = yes;
            indexGrid.bottomIndices = yes;
            indexGrid.bottomRightIndices = yes;
            indexGrid.horizontalIndices = yes;
            indexGrid.verticalIndices = yes;
            indexGrid.topCapIndices = yes;
            indexGrid.rightCapIndices = yes;
            indexGrid.bottomCapIndices = yes;
            indexGrid.leftCapIndices = yes;
            indexGrid.allSidesIndices = yes;
            indexGrid.topLeftNubIndices = yes;
            indexGrid.topRightNubIndices = yes;
            indexGrid.bottomLeftNubIndices = yes;
            indexGrid.bottomRightNubIndices = yes;

            indexGrid.extendedSet = false;

            indexGrid.topCenterLeftIndices = yes;
            indexGrid.topCenterIndices = yes;
            indexGrid.topCenterRightIndices = yes;
            indexGrid.thirdTopRowLeftIndices = yes;
            indexGrid.thirdTopRowCenterIndices = yes;
            indexGrid.thirdTopRowRightIndices = yes;
            indexGrid.internalBottomLeftCenterIndices = yes;
            indexGrid.internalBottomCenterIndices = yes;
            indexGrid.internalBottomRightCenterIndices = yes;

            indexGrid.borderTopNubLeftIndices = yes;
            indexGrid.borderTopNubRightIndices = yes;
            indexGrid.borderTopNubBothIndices = yes;
            indexGrid.borderRightNubTopIndices = yes;
            indexGrid.borderRightNubBottomIndices = yes;
            indexGrid.borderRightNubBothIndices = yes;
            indexGrid.borderBottomNubLeftIndices = yes;
            indexGrid.borderBottomNubRightIndices = yes;
            indexGrid.borderBottomNubBothIndices = yes;
            indexGrid.borderLeftNubTopIndices = yes;
            indexGrid.borderLeftNubBottomIndices = yes;
            indexGrid.borderLeftNubBothIndices = yes;
            indexGrid.diagonalNubsTopLeftBottomRight = yes;
            indexGrid.diagonalNubsTopRightBottomLeft = yes;
            indexGrid.doubleNubsTop = yes;
            indexGrid.doubleNubsRight = yes;
            indexGrid.doubleNubsBottom = yes;
            indexGrid.doubleNubsLeft = yes;
            indexGrid.quadNubs = yes;
            indexGrid.topRightWithNub = yes;
            indexGrid.topLeftWithNub = yes;
            indexGrid.bottomRightWithNub = yes;
            indexGrid.bottomLeftWithNub = yes;

            indexGrid.diagonalBorderNE = yes;
            indexGrid.diagonalBorderSE = yes;
            indexGrid.diagonalBorderSW = yes;
            indexGrid.diagonalBorderNW = yes;
            indexGrid.diagonalCeilingNE = yes;
            indexGrid.diagonalCeilingSE = yes;
            indexGrid.diagonalCeilingSW = yes;
            indexGrid.diagonalCeilingNW = yes;

            indexGrid.CenterCheckerboard = false;
            indexGrid.CheckerboardDimension = 1;
            indexGrid.CenterIndicesAreStrata = false;

            indexGrid.PitInternalSquareGrids = new List<TileIndexGrid>();

            indexGrid.PitInternalSquareOptions = new PitSquarePlacementOptions { CanBeFlushBottom = false, CanBeFlushLeft = false, CanBeFlushRight = false, PitSquareChance = -1 };

            indexGrid.PitBorderIsInternal = false;

            indexGrid.PitBorderOverridesFloorTile = false;

            indexGrid.CeilingBorderUsesDistancedCenters = false;

            indexGrid.UsesRatChunkBorders = false;
            indexGrid.RatChunkNormalSet = yes;
            indexGrid.RatChunkBottomSet = yes;

            indexGrid.PathFacewallStamp = null;
            indexGrid.PathSidewallStamp = null;

            indexGrid.PathPitPosts = yes;
            indexGrid.PathPitPostsBL = yes;
            indexGrid.PathPitPostsBR = yes;

            indexGrid.PathStubNorth = null;
            indexGrid.PathStubEast = null;
            indexGrid.PathStubSouth = null;
            indexGrid.PathStubWest = null;


            return indexGrid;
        }
        public enum TileSetupType
        {
            HIGH_FACEWALL,
            LOW_FACEWALL,
            WALL_BORDER
        }
        public static void SetupTileCollision(this tk2dSpriteCollectionData collection, int id, TileSetupType type)
        {
            switch (type)
            {
                case TileSetupType.HIGH_FACEWALL:
                    collection.spriteDefinitions[id].SetupTilesetSpriteDefForceCollision(new Vector3[] { new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.1f) }, tk2dSpriteDefinition.ColliderType.Box, CollisionLayer.HighObstacle, true);
                    break;
                case TileSetupType.LOW_FACEWALL:
                    collection.spriteDefinitions[id].SetupTilesetSpriteDefForceCollision(new Vector3[] { new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.1f) }, tk2dSpriteDefinition.ColliderType.Box, CollisionLayer.LowObstacle);
                    break;
                case TileSetupType.WALL_BORDER:
                    collection.spriteDefinitions[id].SetupTilesetSpriteDefForceCollision(new Vector3[] { new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.1f) }, tk2dSpriteDefinition.ColliderType.Box, CollisionLayer.HighObstacle, true);
                    break;
            }
        }
        public static void SetMaterial(this tk2dSpriteCollectionData collection, int spriteId, int matNum, bool textureChange = false)
        {
            if (textureChange == true)
            {
                collection.materials[matNum].mainTexture = collection.spriteDefinitions[spriteId].material.mainTexture;
            }
            collection.spriteDefinitions[spriteId].material = collection.materials[matNum];
            collection.spriteDefinitions[spriteId].materialId = matNum;
        }
        public static void SetupTilesetSpriteDefForceCollision(this tk2dSpriteDefinition def, Vector3[] vector3s, tk2dSpriteDefinition.ColliderType type = tk2dSpriteDefinition.ColliderType.None, CollisionLayer collisionLayer = CollisionLayer.HighObstacle, bool ForceDefaultvertices = false)
        {
            try
            {
                def.boundsDataCenter = new Vector3(0.5f, 0.5f, 0f);
                def.boundsDataExtents = new Vector3(1f, 1f, 0f);
                def.untrimmedBoundsDataCenter = new Vector3(0.5f, 0.5f, 0f);
                def.untrimmedBoundsDataExtents = new Vector3(1f, 1f, 0f);
                def.texelSize = new Vector2(0.625f, 0.625f);
                def.position0 = new Vector3(0f, 0f, 0f);
                def.position1 = new Vector3(1f, 0f, 0f);
                def.position2 = new Vector3(0f, 1f, 0f);
                def.position3 = new Vector3(1f, 1f, 0f);
                def.regionH = 16;
                def.regionW = 16;
                def.colliderType = type;
                def.collisionLayer = collisionLayer;
                if (ForceDefaultvertices == true)
                {
                    def.colliderVertices = new Vector3[]
                    {
                    new Vector3(0f, 1f, -1f),
                    new Vector3(0f, 1f, 1f),
                    new Vector3(0f, 0f, -1f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3(1f, 0f, -1f),
                    new Vector3(1f, 0f, 1f),
                    new Vector3(1f, 1f, -1f),
                    new Vector3(1f, 1f, 1f)
                    };
                }
                else if (vector3s == null)
                {
                    def.colliderVertices = new Vector3[0];
                }
                else
                {
                    def.colliderVertices = vector3s;
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }

        }
        public static void SetupTileMetaData(this TilesetIndexMetadata metadata, TilesetIndexMetadata.TilesetFlagType type, float weight = 1f, int dungeonRoomSubType = 0, int dungeonRoomSubType2 = -1, int dungeonRoomSubType3 = -1, bool animated = false, bool preventStamps = true)
        {
            metadata.type = type;
            metadata.weight = weight;
            metadata.dungeonRoomSubType = dungeonRoomSubType;
            metadata.secondRoomSubType = dungeonRoomSubType2;
            metadata.thirdRoomSubType = dungeonRoomSubType3;
            metadata.usesAnimSequence = animated;
            metadata.usesNeighborDependencies = false;
            metadata.preventWallStamping = preventStamps;
            metadata.usesPerTileVFX = false;
            metadata.tileVFXPlaystyle = TilesetIndexMetadata.VFXPlaystyle.CONTINUOUS;
            metadata.tileVFXChance = 0f;
            metadata.tileVFXPrefab = null;
            metadata.tileVFXOffset = Vector2.zero;
            metadata.tileVFXDelayTime = 1f;
            metadata.tileVFXDelayVariance = 0f;
            metadata.tileVFXAnimFrame = 0;
        }
    }
}
