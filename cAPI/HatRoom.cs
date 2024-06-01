using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod.RuntimeDetour;
using System.Collections;
using HarmonyLib;
using Alexandria.DungeonAPI;

namespace Alexandria.cAPI
{
  public static class HatRoom
  {
    private const string BASE_RES_PATH = "Alexandria/cAPI";

    private const int LIGHT_SPACING        = 8;
    private const int DEBUG_HAT_MULT       = 1; // used for creating 100s of duplicate hats for stress testing hat room, should be 1 on release
    private const float PEDESTAL_X_SPACING = 3.125f;
    private const float PEDESTAL_Y_SPACING = 3.125f;
    private const float PEDESTAL_Z         = 10.8f;
    private const float HAT_Z_OFFSET       = 1f;

    private static readonly Vector3 ENTRANCE_POSITION = new Vector3(60.0f - 1f/16f, 36.5f, 36.875f);

    private static GameObject hatRoomEntrance       = null;
    private static GameObject hatRoomExit           = null;
    private static GameObject slimPedestal          = null;
    private static bool needToGenHatRoom            = true;
    private static bool createdPrefabs              = false;
    private static List<IntVector2> pedestalOffsets = null;
    private static Vector2 hatRoomCenter            = Vector2.zero;
    private static PrototypeDungeonRoom protoRoom   = null;
    private static RoomHandler runtimeRoom          = null;

    /// <summary>Regenerates the hat room every time the Breach loads</summary>
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.ProcessPlayerEnteredFoyer))]
    private class ProcessPlayerEnteredFoyerPatch
    {
        static void Postfix(Foyer __instance, PlayerController p)
        {
          if (!needToGenHatRoom || Hatabase.HatRoomHats.Count == 0)
            return;
          CreatePrefabsIfNeeded();
          CreateHatRoomPrototypeIfNeeded();
          // Set up hat room entrance and warp points
          UnityEngine.Object.Instantiate(hatRoomEntrance, ENTRANCE_POSITION, Quaternion.identity)
            .GetComponent<SpeculativeRigidbody>().OnCollision += WarpToHatRoom;
          runtimeRoom = null;
          needToGenHatRoom = false;
        }
    }

    /// <summary>Marks the hat room in need of regeneration every time the Breach is reloaded</summary>
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.Start))]
    private class OnFoyerStartPatch
    {
        static void Postfix(Foyer __instance)
        {
          needToGenHatRoom = true;
        }
    }

    private static void CreatePrefabsIfNeeded()
    {
      if (createdPrefabs)
        return;

      hatRoomEntrance = ItemAPI.ItemBuilder.AddSpriteToObject("Entrance", $"{BASE_RES_PATH}/hat_room_entrance.png");
      hatRoomEntrance.MakeRigidBody(dimensions: new IntVector2(30, 50), offset: new IntVector2(12, 0));
      hatRoomEntrance.GetComponent<tk2dSprite>().HeightOffGround = -15;

      slimPedestal = ItemAPI.ItemBuilder.AddSpriteToObject("slimPedestal", $"{BASE_RES_PATH}/hat_pedestal.png");
      slimPedestal.MakeRigidBody(dimensions: new IntVector2(20, 21), offset: new IntVector2(0, 0));
      slimPedestal.GetComponent<tk2dSprite>().HeightOffGround = -3;

      GameObject pedestalShadow = UnityEngine.Object.Instantiate(GameManager.Instance.Dungeon.sharedSettingsPrefab.ChestsForBosses.elements[0]
        .gameObject.GetComponent<RewardPedestal>().transform.Find("Pedestal_Shadow").gameObject);
      pedestalShadow.transform.parent = slimPedestal.transform;
      pedestalShadow.transform.localPosition = new Vector2(-0.0625f, -0.25f);
      pedestalShadow.transform.localScale = new Vector3(0.8f, 1f, 1f);

      hatRoomExit = ItemAPI.ItemBuilder.AddSpriteToObject("HatRoomExit", $"{BASE_RES_PATH}/hat_room_exit.png");
      hatRoomExit.MakeRigidBody(dimensions: new IntVector2(22, 16), offset: new IntVector2(12, 8));

      createdPrefabs = true;
    }

    private static void CreateHatRoomPrototypeIfNeeded()
    {
      if (protoRoom != null)
        return;

      // Math our way to figuring out the room size
      GetPedestalRingOffsets(DEBUG_HAT_MULT * Hatabase.HatRoomHats.Count, out int maxRing);
      int roomXSize = Mathf.CeilToInt(2 * (maxRing + 1) * PEDESTAL_X_SPACING);
      int roomYSize = Mathf.CeilToInt(2 * (maxRing + 1) * PEDESTAL_Y_SPACING);

      protoRoom = CreateEmptyLitHatRoom(roomXSize, roomYSize);
    }

    private static PrototypeDungeonRoom CreateEmptyLitHatRoom(int width, int height)
    {
      PrototypeDungeonRoom room = RoomFactory.GetNewPrototypeDungeonRoom(width, height);
      room.usesProceduralLighting = false;
      room.overrideRoomVisualType = 1; // 0 = stone, 1 = wood, 2 = brick
      room.FullCellData = new PrototypeDungeonRoomCellData[width * height];
      int hradius = width / 2;
      int vradius = height / 2;
      for (int y = 0; y < height; y++)
      {
          for (int x = 0; x < width; x++)
          {
              // fancy math to space out lights evenly without too much overlap or dark spots
              bool shouldBeLit = ((hradius - Math.Min(x, width  - (x + 1))) % LIGHT_SPACING == (LIGHT_SPACING / 2)) &&
                                 ((vradius - Math.Min(y, height - (y + 1))) % LIGHT_SPACING == (LIGHT_SPACING / 2));
              room.FullCellData[x + y * width] = new PrototypeDungeonRoomCellData()
              {
                  containsManuallyPlacedLight = shouldBeLit,
                  state = CellType.FLOOR,
                  appearance = new PrototypeDungeonRoomCellAppearance(),
              };
          }
      }

      // Add walls in the middle for the stairs
      for (int y = vradius; y < vradius + 4; y++)
          for (int x = hradius - 4; x < hradius + 4; x++)
              room.FullCellData[x + y * width].state = CellType.WALL;

      return room;
    }

    private static void MakeRigidBody(this GameObject g, IntVector2 dimensions, IntVector2 offset)
    {
      g.AddComponent<SpeculativeRigidbody>().PixelColliders = new() { new ()
      {
          ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual,
          CollisionLayer = CollisionLayer.HighObstacle,
          ManualOffsetX = offset.x,
          ManualOffsetY = offset.y,
          ManualWidth = dimensions.x,
          ManualHeight = dimensions.y,
      }};
    }

    /// <summary>Mostly identical to the base game AddRuntimeRoom() function, with a hack to work around tiles failing to render when adding the room to the Breach</summary>
    private static RoomHandler AddRuntimeHatRoom(Dungeon dungeon, PrototypeDungeonRoom prototype, Action<RoomHandler> postProcessCellData = null, DungeonData.LightGenerationStyle lightStyle = DungeonData.LightGenerationStyle.FORCE_COLOR)
    {
      int wallWidth = 3;
      int borderSize = wallWidth * 2;
      IntVector2 roomDimensions = new IntVector2(prototype.Width, prototype.Height);
      IntVector2 newRoomOffset = new IntVector2(dungeon.data.Width + borderSize, borderSize);
      int newWidth = dungeon.data.Width + borderSize * 2 + roomDimensions.x;
      int newHeight = Mathf.Max(dungeon.data.Height, roomDimensions.y + borderSize * 2);
      CellData[][] array = BraveUtility.MultidimensionalArrayResize(dungeon.data.cellData, dungeon.data.Width, dungeon.data.Height, newWidth, newHeight);
      CellArea cellArea = new CellArea(newRoomOffset, roomDimensions);
      cellArea.prototypeRoom = prototype;
      dungeon.data.cellData = array;
      dungeon.data.ClearCachedCellData();
      RoomHandler roomHandler = new RoomHandler(cellArea);
      for (int i = -borderSize; i < roomDimensions.x + borderSize; i++)
      {
        for (int j = -borderSize; j < roomDimensions.y + borderSize; j++)
        {
          IntVector2 p = new IntVector2(i, j) + newRoomOffset;
          CellData cellData = new CellData(p);
          cellData.positionInTilemap = cellData.positionInTilemap - newRoomOffset + new IntVector2(wallWidth, wallWidth);
          cellData.parentArea = cellArea;
          cellData.parentRoom = roomHandler;
          cellData.nearestRoom = roomHandler;
          cellData.distanceFromNearestRoom = 0f;
          array[p.x][p.y] = cellData;
        }
      }
      roomHandler.WriteRoomData(dungeon.data);
      for (int k = -borderSize; k < roomDimensions.x + borderSize; k++)
      {
        for (int l = -borderSize; l < roomDimensions.y + borderSize; l++)
        {
          IntVector2 intVector2 = new IntVector2(k, l) + newRoomOffset;
          array[intVector2.x][intVector2.y].breakable = true;
        }
      }
      dungeon.data.rooms.Add(roomHandler);
      GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("RuntimeTileMap"));
      tk2dTileMap component = gameObject.GetComponent<tk2dTileMap>();
      component.Editor__SpriteCollection = dungeon.tileIndices.dungeonCollection;
      GameManager.Instance.Dungeon.data.GenerateLightsForRoom(GameManager.Instance.Dungeon.decoSettings, roomHandler, GameObject.Find("_Lights").transform, lightStyle);
      if (postProcessCellData != null)
      {
        postProcessCellData(roomHandler);
      }
      const int HACKY_SCALING_FACTOR = 32; //HACK: making the tilemap extra wide is the only way I've found to avoid rendering issues with black patches in rooms
      TK2DDungeonAssembler.RuntimeResizeTileMap(component, HACKY_SCALING_FACTOR + roomDimensions.x + wallWidth * 2, roomDimensions.y + wallWidth * 2, dungeon.m_tilemap.partitionSizeX, dungeon.m_tilemap.partitionSizeY);
      for (int m = -wallWidth; m < roomDimensions.x + wallWidth; m++)
      {
        for (int n = -wallWidth; n < roomDimensions.y + wallWidth; n++)
        {
          dungeon.assembler.BuildTileIndicesForCell(dungeon, component, newRoomOffset.x + m, newRoomOffset.y + n);
        }
      }
      tk2dRuntime.TileMap.RenderMeshBuilder.CurrentCellXOffset = newRoomOffset.x - wallWidth;
      tk2dRuntime.TileMap.RenderMeshBuilder.CurrentCellYOffset = newRoomOffset.y - wallWidth;
      component.Build(tk2dTileMap.BuildFlags.ForceBuild);
      tk2dRuntime.TileMap.RenderMeshBuilder.CurrentCellXOffset = 0;
      tk2dRuntime.TileMap.RenderMeshBuilder.CurrentCellYOffset = 0;
      component.renderData.transform.position = new Vector3(newRoomOffset.x - wallWidth, newRoomOffset.y - wallWidth, newRoomOffset.y - wallWidth);
      roomHandler.OverrideTilemap = component;
      Pathfinding.Pathfinder.Instance.InitializeRegion(dungeon.data, roomHandler.area.basePosition + new IntVector2(-wallWidth, -wallWidth), roomHandler.area.dimensions + new IntVector2(wallWidth, wallWidth));
      roomHandler.PostGenerationCleanup();
      DeadlyDeadlyGoopManager.ReinitializeData();
      return roomHandler;
    }

    private static void InstantiateHatRoomIfNeeded()
    {
      if (runtimeRoom != null)
        return;

      // Create the hat room itself
      Dungeon dungeon = GameManager.Instance.Dungeon;
      RoomHandler newRoom = AddRuntimeHatRoom(dungeon, protoRoom, lightStyle: DungeonData.LightGenerationStyle.FORCE_COLOR);
      Pixelator.Instance.MarkOcclusionDirty();
      Pixelator.Instance.ProcessOcclusionChange(newRoom.GetCenterCell(), 1f, newRoom, true);

      // Set up the hat room exit
      hatRoomCenter = newRoom.area.Center;
      GameObject returner = UnityEngine.Object.Instantiate(hatRoomExit);
      tk2dSprite returnerSprite = returner.GetComponent<tk2dSprite>();
      returnerSprite.PlaceAtPositionByAnchor(hatRoomCenter + new Vector2(0, -1.3125f), tk2dBaseSprite.Anchor.LowerCenter);
      returnerSprite.HeightOffGround = -3f;
      returnerSprite.UpdateZDepth();
      returner.GetComponent<SpeculativeRigidbody>().OnPreRigidbodyCollision += PreWarpBackFromHatRoom;
      returner.GetComponent<SpeculativeRigidbody>().OnCollision += WarpBackFromHatRoom;

      // Set up the hat pedestals
      CreateHatPedestals(newRoom);

      runtimeRoom = newRoom;
    }

    /// <summary>Logic for getting offsets in symmetrical rings around the center of the hat room</summary>
    private static void GetPedestalRingOffsets(int length, out int nextRing)
    {
      pedestalOffsets = new(length);
      int remaining = length;
      nextRing = 1;
      while (remaining > 0)
      {
        nextRing += 1;
        int maxRingSize = nextRing * 8;
        int ringSize = Math.Min(remaining, maxRingSize);
        if ((remaining % 2) == 1 || ringSize == maxRingSize)
          pedestalOffsets.Add(new IntVector2(0, nextRing));
        int halfRing = ringSize / 2;
        int x = 0;
        int y = nextRing;
        for (int i = 1; i <= halfRing; ++i)
        {
          if (i == (maxRingSize / 2))
          {
            pedestalOffsets.Add(new IntVector2(0, -nextRing));
            break;
          }
          if (y == -nextRing)
            --x;
          else if (x < nextRing)
            ++x;
          else
            --y;
          pedestalOffsets.Add(new IntVector2(x, y));
          pedestalOffsets.Add(new IntVector2(-x, y));
        }
        remaining -= ringSize;
      }
    }

    private static void CreateHatPedestals(RoomHandler room)
    {
        Vector2 roomCenter = room.area.Center;
        for (int i = 0; i < DEBUG_HAT_MULT * Hatabase.HatRoomHats.Count; i++)
        {
          Hat hat = Hatabase.HatRoomHats[i % Hatabase.HatRoomHats.Count];

          float pedX = roomCenter.x + pedestalOffsets[i].x * PEDESTAL_X_SPACING;
          float pedY = roomCenter.y + pedestalOffsets[i].y * PEDESTAL_Y_SPACING;

          // GameObject pedObj = UnityEngine.Object.Instantiate(hat.goldenPedestal ? goldPedestal : plainPedestal);
          GameObject pedObj = UnityEngine.Object.Instantiate(slimPedestal);
          pedObj.GetComponent<tk2dSprite>().PlaceAtPositionByAnchor(new Vector3(pedX, pedY, PEDESTAL_Z), tk2dBaseSprite.Anchor.LowerCenter);
          SpriteOutlineManager.AddOutlineToSprite(pedObj.GetComponent<tk2dSprite>(), Color.black, zOffset: PEDESTAL_Z);

          HatPedestal pedestal = pedObj.AddComponent<HatPedestal>();
          pedestal.hat = hat;

          if (hat.HasBeenUnlocked || hat.showSilhouetteWhenLocked)
          {
            GameObject pedestalHatObject = new GameObject();
            tk2dSprite sprite = pedestalHatObject.AddComponent<tk2dSprite>();
            sprite.SetSprite(pedestal.hat.sprite.collection, pedestal.hat.sprite.spriteId);
            if (!hat.HasBeenUnlocked)
            {
              sprite.usesOverrideMaterial = true;
              sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/Black");
            }
            sprite.PlaceAtLocalPositionByAnchor(
              new Vector2(pedObj.GetComponent<tk2dSprite>().WorldCenter.x, pedY + 1.3f).ToVector3ZisY(0f),
              tk2dBaseSprite.Anchor.LowerCenter);
            sprite.HeightOffGround = HAT_Z_OFFSET;
            sprite.UpdateZDepth();
            SpriteOutlineManager.AddOutlineToSprite(pedestalHatObject.GetComponent<tk2dSprite>(), Color.black, zOffset: HAT_Z_OFFSET);
          }
          room.RegisterInteractable(pedestal as IPlayerInteractable);
        }
    }

    private static Vector2 HatRoomWarpPoint() => hatRoomCenter + new Vector2(-GameManager.Instance.PrimaryPlayer.SpriteDimensions.x / 2, -2f);
    private static Vector2 HatRoomReturnPoint() => new Vector2(58.5f, 37.25f);

    private static void WarpToHatRoom(CollisionData obj)
    {
      if (obj.OtherRigidbody.gameObject.GetComponent<PlayerController>() is not PlayerController player)
        return;

      GameManager.Instance.StartCoroutine(WarpToPoint(player, HatRoomWarpPoint, Vector2.down, InstantiateHatRoomIfNeeded));
      Pixelator.Instance.DoOcclusionLayer = false;
    }

    private static void PreWarpBackFromHatRoom(SpeculativeRigidbody me, PixelCollider myPixelCollider, SpeculativeRigidbody other, PixelCollider otherPixelCollider)
    {
      if (other.gameObject.GetComponent<PlayerController>() is not PlayerController player)
        return;
      if (player.usingForcedInput || player.m_activeActions.Move.Vector.y <= 0f)
        PhysicsEngine.SkipCollision = true;
    }

    private static void WarpBackFromHatRoom(CollisionData obj)
    {
      if (obj.OtherRigidbody.gameObject.GetComponent<PlayerController>() is not PlayerController player)
        return;
      if (player.m_activeActions.Move.Vector.y <= 0f)
        return; // only enter if we're facing up

      GameManager.Instance.StartCoroutine(WarpToPoint(player, HatRoomReturnPoint, Vector2.left));
      Pixelator.Instance.DoOcclusionLayer = true;
    }

    private static IEnumerator WarpToPoint(PlayerController p, Func<Vector2> positionFunc, Vector2? forceDirection = null, Action preWarpSetup = null)
    {
      p.usingForcedInput = true;
      Pixelator.Instance.FadeToBlack(0.1f, false);
      yield return new WaitForSeconds(0.15f);
      if (preWarpSetup != null)
        preWarpSetup();
      p.ForceStopDodgeRoll();
      p.WarpToPointAndBringCoopPartner(positionFunc(), doFollowers: true);
      GameManager.Instance.MainCameraController.ForceToPlayerPosition(p);
      if (forceDirection.HasValue)
        p.ForceIdleFacePoint(forceDirection.Value);
      yield return new WaitForSeconds(0.05f);
      Pixelator.Instance.FadeToBlack(0.1f, true);
      p.usingForcedInput = false;
    }

    private class HatPedestal : BraveBehaviour, IPlayerInteractable
    {
      public Hat hat;

      public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
      {
        shouldBeFlipped = false; //Some boilerplate code for determining if the interactable should be flipped
        return string.Empty;
      }

      public float GetOverrideMaxDistance()
      {
        return 1.5f;
      }

      public float GetDistanceToPoint(Vector2 point)
      {
        return Vector2.Distance(point, gameObject.GetComponent<tk2dSprite>().WorldCenter);
      }

      public void Interact(PlayerController interactor)
      {
        if (hat.HasBeenUnlocked)
        {
          HatController hatCont = interactor.GetComponent<HatController>();
          if (hatCont.CurrentHat != null && hatCont.CurrentHat.hatName == hat.hatName)
            hatCont.RemoveCurrentHat();
          else
            hatCont.SetHat(hat);
          LootEngine.DoDefaultItemPoof(interactor.sprite.WorldBottomCenter + new Vector2(0f, 1f)); //TODO: sanity check this
        }
        else
          AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
      }

      public void OnEnteredRange(PlayerController interactor)
      {
        SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
        SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
        TextBoxManager.ShowInfoBox(new Vector2(transform.position.x + 0.75f ,transform.position.y + 2), transform, 3600f, hat.HasBeenUnlocked ? hat.hatName : hat.UnlockText); // 1 hour duration so it persists
      }

      public void OnExitRange(PlayerController interactor)
      {
        SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
        SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
        TextBoxManager.ShowInfoBox(new Vector2(transform.position.x + 0.75f ,transform.position.y + 2), transform, 0f, hat.HasBeenUnlocked ? hat.hatName : hat.UnlockText); // 0 duration = disappears instantly
      }
    }
  }
}
