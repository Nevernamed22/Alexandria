using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public class SpecialComponents
    {
        public class PedestalSetter : MonoBehaviour
        {
            public int Help =-1;
            public RewardPedestal c;
            public void Start()
            {
                if (c == null) {c = this.GetComponent<RewardPedestal>(); }

                c.m_room = this.transform.position.GetAbsoluteRoom();

                if (Help != -1)
                {
                    if (c.m_itemDisplaySprite == null)
                    {
                        c.contents = PickupObjectDatabase.GetById(Help);
                        c.m_itemDisplaySprite = tk2dSprite.AddComponent(new GameObject("Display Sprite")
                        {
                            transform =
                            {
                                parent = c.spawnTransform
                            }
                        }, c.contents.sprite.Collection, c.contents.sprite.spriteId);
                        SpriteOutlineManager.AddOutlineToSprite(c.m_itemDisplaySprite, Color.black, 0.1f, 0.05f, SpriteOutlineManager.OutlineType.NORMAL);
                        c.sprite.AttachRenderer(c.m_itemDisplaySprite);
                        c.m_itemDisplaySprite.HeightOffGround = 0.25f;
                        c.m_itemDisplaySprite.depthUsesTrimmedBounds = true;
                        c.m_itemDisplaySprite.PlaceAtPositionByAnchor(c.spawnTransform.position, tk2dBaseSprite.Anchor.LowerCenter);
                        c.m_itemDisplaySprite.transform.position = c.m_itemDisplaySprite.transform.position.Quantize(0.0625f);
                        GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
                        tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
                        component.PlaceAtPositionByAnchor(c.m_itemDisplaySprite.WorldCenter.ToVector3ZUp(0f), tk2dBaseSprite.Anchor.MiddleCenter);
                        component.HeightOffGround = 5f;
                        component.UpdateZDepth();
                        c.sprite.UpdateZDepth();
                    }
                }
                else
                {
                    if (c.m_room != null)
                    {
                        if (c.m_room.IsRegistered(c))
                        {
                            c.m_room.DeregisterInteractable(c);
                        }
                    }
                }
            }
        }
    }
}
