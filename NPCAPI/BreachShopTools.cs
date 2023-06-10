using Dungeonator;
using Alexandria.DungeonAPI;
using HutongGames.PlayMaker;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.NPCAPI
{
    class BreachShopTools
    {
        public static bool BreachShopDebugLogging = false;

        public static void Init()
        {
            bool initialized = BreachShopTools.m_initialized;
            if (!initialized)
            {
                DungeonHooks.OnFoyerAwake += BreachShopTools.PlaceBreachShops;
                DungeonHooks.OnPreDungeonGeneration += delegate (LoopDungeonGenerator generator, Dungeon dungeon, DungeonFlow flow, int dungeonSeed)
                {
                    bool flag = flow.name != "Foyer Flow" && !GameManager.IsReturningToFoyerWithPlayer;
                    if (flag)
                    {
                        BreachShopTools.CleanupBreachShops();
                    }
                };
                BreachShopTools.m_initialized = true;
            }
        }

        private static bool m_initialized;


        private static void CleanupBreachShops()
        {
            foreach (BreachShopComp customShrineController in UnityEngine.Object.FindObjectsOfType<BreachShopComp>())
            {
                if (BreachShopDebugLogging == true)
                {
                    DebugUtility.Print<string>("removed shop " + customShrineController.gameObject.name, "FFFFFF", true);
                }
                bool flag = !FakePrefab.IsFakePrefab(customShrineController);
                if (flag)
                {
                    UnityEngine.Object.Destroy(customShrineController.gameObject);
                }
                else
                {
                    customShrineController.gameObject.SetActive(false);
                }
            }
        }

        public static Dictionary<string, GameObject> registeredShops = new Dictionary<string, GameObject>();

        public static void PlaceBreachShops()
        {
            BreachShopTools.CleanupBreachShops();
            if (BreachShopDebugLogging == true)
            {
                DebugUtility.Print<string>("Placing breach shops: ", "FFFFFF", true);
                DebugUtility.Print<string>(BreachShopTools.registeredShops.Count.ToString(), "FFFFFF", true);
            }
            foreach (GameObject gameObject in BreachShopTools.registeredShops.Values)
            {
                try
                {
                   
                    if (gameObject.GetComponent<BreachShopComp>() != null)
                    {
                        if (BreachShopDebugLogging == true)
                        {
                            DebugUtility.Print<string>("    " + gameObject.name, "FFFFFF", true);
                        }
                        var shop = UnityEngine.Object.Instantiate<GameObject>(gameObject);
                        var comp = gameObject.GetComponent<BreachShopComp>();
                        shop.SetActive(true);
                        //shop.sprite.PlaceAtPositionByAnchor(comp.offset, tk2dBaseSprite.Anchor.LowerCenter);

                        //Vector2 relativePositionFromAnchor = this.GetRelativePositionFromAnchor(anchor);
                        shop.transform.position = comp.offset;// - relativePositionFromAnchor.ToVector3ZUp(0f);

                        var merchant = shop.GetComponent<CustomShopController>().shopkeepFSM.gameObject.GetComponent<TalkDoerLite>();

                        if (merchant && !RoomHandler.unassignedInteractableObjects.Contains(merchant))
                        {
                            RoomHandler.unassignedInteractableObjects.Add(merchant);
                        }
                    }
                }
                catch (Exception e)
                {
                    DebugUtility.Print<string>(e.ToString(), "FF0000", true);
                }
            }
        }
    }

    class BreachShopComp : MonoBehaviour
    {
        public Vector3 offset;
    }
}
