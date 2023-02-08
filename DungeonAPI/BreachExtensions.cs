using Alexandria.ItemAPI;
using Dungeonator;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    [HarmonyPatch(typeof(Foyer), "Start", MethodType.Normal)]
    public class FoyerStartPatch
    {
        [HarmonyPostfix]
        public static IEnumerator FoyerStartPostFix(IEnumerator enumerator, Foyer __instance)
        {
            yield return enumerator;

            RoomHandler m_TargetRoom = RuntimeDungeonEditing.AddCustomRuntimeRoom(GameManager.Instance.Dungeon, new IntVector2(19, 9), new GameObject(), roomWorldPositionOverride: new IntVector2(-30, 60));

            GameObject floor = new GameObject();
            ItemBuilder.AddSpriteToObjectPerpendicular("tentFloor",  "Alexandria/NativeResources/debugfloor", floor, tk2dBaseSprite.PerpendicularState.FLAT, 0);
            floor.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(m_TargetRoom.area.basePosition.ToVector3(), tk2dBaseSprite.Anchor.LowerLeft);
            floor.transform.parent = m_TargetRoom.hierarchyParent;
            floor.layer = LayerMask.NameToLayer("BG_Critical");
            floor.SetActive(true);
            floor.GetComponent<tk2dBaseSprite>().HeightOffGround = -1.75f;
            floor.GetComponent<tk2dBaseSprite>().UpdateZDepth();

            GameObject walls = new GameObject();
            ItemBuilder.AddSpriteToObjectPerpendicular("tentWalls", "Alexandria/NativeResources/debugwalls", walls, tk2dBaseSprite.PerpendicularState.FLAT, 0);
            walls.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor((m_TargetRoom.area.basePosition + new IntVector2(-1, -1)).ToVector3(), tk2dBaseSprite.Anchor.LowerLeft);
            walls.transform.parent = m_TargetRoom.hierarchyParent;
            walls.layer = LayerMask.NameToLayer("FG_Critical");
            walls.SetActive(true);
            walls.GetComponent<tk2dBaseSprite>().HeightOffGround = 4f;
            walls.GetComponent<tk2dBaseSprite>().UpdateZDepth();

            GameObject northwall = new GameObject();
            ItemBuilder.AddSpriteToObjectPerpendicular("tentWallNorth",  "Alexandria/NativeResources/debugnorthwall", northwall, tk2dBaseSprite.PerpendicularState.UNDEFINED, 0);
            northwall.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor((m_TargetRoom.area.basePosition + new IntVector2(0, 9)).ToVector3(), tk2dBaseSprite.Anchor.LowerLeft);
            northwall.transform.parent = m_TargetRoom.hierarchyParent;
            northwall.layer = LayerMask.NameToLayer("BG_Critical");
            northwall.SetActive(true);
            northwall.GetComponent<tk2dBaseSprite>().HeightOffGround = -1.73f;
            northwall.GetComponent<tk2dBaseSprite>().UpdateZDepth();

            GameObject tent = __instance.gameObject.transform.Find("Livery xform").Find("tont").gameObject;
            Tenteleport tentInt = tent.AddComponent<Tenteleport>();
            tentInt.target = m_TargetRoom;
            GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(tent.transform.position.IntXY()).RegisterInteractable(tentInt);

            yield break;
        }
    }
    public class Tenteleport : MonoBehaviour, IPlayerInteractable
    {
        public RoomHandler target;
        public tk2dSprite sprite
        {
            get
            {
                return base.GetComponent<tk2dSprite>();
            }
        }
        public float GetDistanceToPoint(Vector2 point)
        {
            if (!sprite) return float.MaxValue;
            Bounds bounds = sprite.GetBounds();
            bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
            float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
            float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
            return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
        }
        public void OnEnteredRange(PlayerController interactor)
        {
            //A method that runs whenever the player enters the interaction range of the interactable. This is what outlines it in white to show that it can be interacted with
            SpriteOutlineManager.RemoveOutlineFromSprite(sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(sprite, Color.white);
        }
        public void OnExitRange(PlayerController interactor)
        {
            //A method that runs whenever the player exits the interaction range of the interactable. This is what removed the white outline to show that it cannot be currently interacted with
            SpriteOutlineManager.RemoveOutlineFromSprite(sprite, true);
        }
        public void Interact(PlayerController interactor)
        {
            interactor.WarpToPoint((target.area.basePosition + new IntVector2(2, 2)).ToVector2(), false, false);
        }
        public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
        {
            shouldBeFlipped = false;
            return string.Empty;
        }
        public float GetOverrideMaxDistance()
        {
            return 1f;
        }
    }
}