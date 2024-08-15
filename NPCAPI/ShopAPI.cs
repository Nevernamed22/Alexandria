using Dungeonator;
using Alexandria.DungeonAPI;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static Alexandria.NPCAPI.CustomShopController;

namespace Alexandria.NPCAPI
{
    public static class ShopAPI
    {

        public static Dictionary<string, GameObject> builtShops = new Dictionary<string, GameObject>();

        public static Vector3[] defaultItemPositions = new Vector3[] { new Vector3(1.125f, 2.125f, 1), new Vector3(2.625f, 1f, 1), new Vector3(4.125f, 2.125f, 1) };
        public static Vector3 defaultTalkPointOffset = new Vector3(0.8125f, 2.1875f, -1.31f);
        public static Vector3 defaultNpcPosition = new Vector3(1.9375f, 3.4375f, 5.9375f);



        public static GameObject SetUpNPC(string name, string prefix, List<string> idleSpritePaths, int idleFps, List<string> talkSpritePaths, int talkFps, Vector3 talkPointOffset, Vector3 npcPosition, VoiceBoxes voiceBox = VoiceBoxes.OLD_MAN, float fortunesFavorRadius = 2,
            IntVector2? hitboxSize = null, IntVector2? hitboxOffset = null)
        {

            try
            {

                var shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
                var shared_auto_002 = ResourceManager.LoadAssetBundle("shared_auto_002");
                var SpeechPoint = PrefabAPI.PrefabBuilder.BuildObject("SpeechPoint");
                SpeechPoint.transform.position = talkPointOffset;



                var npcObj = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], PrefabAPI.PrefabBuilder.BuildObject(prefix + ":" + name), Assembly.GetCallingAssembly());

                npcObj.layer = 22;

                var collection = npcObj.GetComponent<tk2dSprite>().Collection;
                SpeechPoint.transform.parent = npcObj.transform;

                FakePrefab.MarkAsFakePrefab(SpeechPoint);
                UnityEngine.Object.DontDestroyOnLoad(SpeechPoint);
                SpeechPoint.SetActive(true);


                var idleIdsList = new List<int>();
                var talkIdsList = new List<int>();

                foreach (string sprite in idleSpritePaths)
                {
                    idleIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                foreach (string sprite in talkSpritePaths)
                {
                    talkIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                tk2dSpriteAnimator spriteAnimator = npcObj.AddComponent<tk2dSpriteAnimator>();

                SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "idle", tk2dSpriteAnimationClip.WrapMode.Loop, idleFps);
                SpriteBuilder.AddAnimation(spriteAnimator, collection, talkIdsList, "talk", tk2dSpriteAnimationClip.WrapMode.Loop, talkFps);


                if (hitboxSize == null) hitboxSize = new IntVector2(20, 18);
                if (hitboxOffset == null) new IntVector2(5, 0);

                SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.LowObstacle, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, hitboxSize, hitboxOffset);
                rigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBlocker));

                //SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.BulletBlocker, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, new IntVector2(20, 18), new IntVector2(5, 0));

                TalkDoerLite talkDoer = npcObj.AddComponent<TalkDoerLite>();

                talkDoer.placeableWidth = 4;
                talkDoer.placeableHeight = 3;
                talkDoer.difficulty = 0;
                talkDoer.isPassable = true;
                talkDoer.usesOverrideInteractionRegion = false;
                talkDoer.overrideRegionOffset = Vector2.zero;
                talkDoer.overrideRegionDimensions = Vector2.zero;
                talkDoer.overrideInteractionRadius = -1;
                talkDoer.PreventInteraction = false;
                talkDoer.AllowPlayerToPassEventually = true;
                talkDoer.speakPoint = SpeechPoint.transform;
                talkDoer.SpeaksGleepGlorpenese = false;
                talkDoer.audioCharacterSpeechTag = ReturnVoiceBox(voiceBox);
                talkDoer.playerApproachRadius = 5;
                talkDoer.conversationBreakRadius = 5;
                talkDoer.echo1 = null;
                talkDoer.echo2 = null;
                talkDoer.PreventCoopInteraction = false;
                talkDoer.IsPaletteSwapped = false;
                talkDoer.PaletteTexture = null;
                talkDoer.OutlineDepth = 0.5f;
                talkDoer.OutlineLuminanceCutoff = 0.05f;
                talkDoer.MovementSpeed = 3;
                talkDoer.PathableTiles = CellTypes.FLOOR;


                UltraFortunesFavor dreamLuck = npcObj.AddComponent<UltraFortunesFavor>();

                dreamLuck.goopRadius = fortunesFavorRadius;
                dreamLuck.beamRadius = fortunesFavorRadius;
                dreamLuck.bulletRadius = fortunesFavorRadius;
                dreamLuck.bulletSpeedModifier = 0.8f;

                dreamLuck.vfxOffset = 0.625f;
                dreamLuck.sparkOctantVFX = shared_auto_001.LoadAsset<GameObject>("FortuneFavor_VFX_Spark");


                AIAnimator aIAnimator = GenerateBlankAIAnimator(npcObj);
                aIAnimator.spriteAnimator = spriteAnimator;
                aIAnimator.IdleAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "idle",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }

                };

                aIAnimator.TalkAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "talk",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }
                };

                PlayMakerFSM iHaveNoFuckingClueWhatThisIs = npcObj.AddComponent<PlayMakerFSM>();

                npcObj.name = prefix + ":" + name;
                return npcObj;
            }
            catch (Exception message)
            {
                ETGModConsole.Log(message.ToString());
                return null;
            }
        }

        /*
        /// <summary>
        /// Creates a shop object along with an npc
        /// </summary>
        /// <param name="name">Name of the npc</param> 
        /// <param name="prefix">Mod prefix (for example Bot)</param> 
        /// 
        /// <param name="idleSpritePaths">List of *FULL* sprite paths for the idle animation</param> 
        /// <param name="idleFps">Fps of the idle animation (base game tends to use around 6)</param> 
        /// 
        /// <param name="talkSpritePaths">List of *FULL* sprite paths for the talk animation</param> 
        /// <param name="talkFps">Fps of the talk animation (base game tends to use around 8)</param> 
        /// 
        /// <param name="lootTable">Shop loot table</param> 
        /// <param name="currency">What is used to buy items at the shop</param> 
        /// 
        /// <param name="runBasedMultilineGenericStringKey">String key for normal convos</param> 
        /// <param name="runBasedMultilineStopperStringKey">String key for if you try talking to an npc to much</param> 
        /// <param name="purchaseItemStringKey">String key for when the player buys something</param> 
        /// <param name="purchaseItemFailedStringKey">String key for when the player tries but fails to buy something</param> 
        /// <param name="introStringKey">String key for when the player enters the room</param> 
        /// <param name="attackedStringKey">String key for when the player shoots at the npc</param> 
        /// <param name="costModifier">The multiplier for shop prices</param> 
        /// <param name="itemPositions">The offset for the item(s) sold by your npc, the amount of items sold is based off how many offsets you add here (if you just want the 3 normally items spots you can use ItsDaFuckinShopApi.defaultItemPositions or just leave it as null)</param> 
        /// <param name="giveStatsOnPurchase">Whether the shop modifies stats after the player buys an item for example how cursula gives curse</param> 
        /// <param name="statsToGiveOnPurchase"> The stats given when the player buys an item (will be ingored if statsToGiveOnPurchase is false)</param> 
        /// 
        /// <param name="CustomCanBuy">The method that gets called to check if the player can buy an item (useless if currency isnt set to CUSTOM)</param> 
        /// <param name="CustomRemoveCurrency">The method that gets called remove currency from the player (useless if currency isnt set to CUSTOM)</param> 
        /// <param name="CustomPrice">The method that gets called to get the price of an item (useless if currency isnt set to CUSTOM)</param> 
        /// 
        /// <param name="currencyIconPath">Sprite path for your custom currency sprite</param> 
        /// <param name="currencyName">The name you want your custom currecy sprite to have (i should probably remove this...)</param> 
        ///
        /// <param name="hasCarpet">Whether the shop has a carpet or something else that they sit on</param> 
        /// <param name="carpetSpritePath">Sprite path for the carpet or whatever</param> 
        ///         
        /// <param name="hasMinimapIcon">Whether the shop has a minimap icon to show what room theyre in</param> 
        /// <param name="minimapIconSpritePath">Sprite path minimap icon leave blank to just use deafult smiley face</param> 
        /// 
        /// <param name="addToMainNpcPool">Whether the shop should be added to the pool of npcs that show up in the main shop a long side bello</param> 
        /// <param name="percentChanceForMainPool">How likely it is for the shop to show up in the main pool base game shops use 0.1</param> 
        /// 
        /// <param name="prerequisites">These do unlocks and shit</param> 
        /// <returns></returns>
        public static GameObject SetUpShop(string name, string prefix, List<string> idleSpritePaths, int idleFps, List<string> talkSpritePaths, int talkFps, GenericLootTable lootTable, CustomShopItemController.ShopCurrencyType currency, string runBasedMultilineGenericStringKey,
            string runBasedMultilineStopperStringKey, string purchaseItemStringKey, string purchaseItemFailedStringKey, string introStringKey, string attackedStringKey, Vector3 talkPointOffset, Vector3[] itemPositions = null, float costModifier = 1, bool giveStatsOnPurchase = false,
            StatModifier[] statsToGiveOnPurchase = null, Func<CustomShopController, PlayerController, int, bool> CustomCanBuy = null, Func<CustomShopController, PlayerController, int, int> CustomRemoveCurrency = null, Func<CustomShopController, CustomShopItemController, PickupObject, int> CustomPrice = null,
            Func<PlayerController, PickupObject, int, bool> OnPurchase = null, Func<PlayerController, PickupObject, int, bool> OnSteal = null, string currencyIconPath = "", string currencyName = "", bool canBeRobbed = true, bool hasCarpet = false, string carpetSpritePath = "", bool hasMinimapIcon = false,
            string minimapIconSpritePath = "", bool addToMainNpcPool = false, float percentChanceForMainPool = 0.1f, DungeonPrerequisite[] prerequisites = null, IntVector2? hitboxSize = null, IntVector2? hitboxOffset = null)
        {

            try
            {

                if (prerequisites == null)
                {
                    prerequisites = new DungeonPrerequisite[0];
                }
                //bool isBreachShop = false;
                Vector3 breachPos = Vector3.zero;

                var shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
                var shared_auto_002 = ResourceManager.LoadAssetBundle("shared_auto_002");
                var SpeechPoint = new GameObject("SpeechPoint");
                SpeechPoint.transform.position = talkPointOffset;



                var npcObj = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], new GameObject(prefix + ":" + name), Assembly.GetCallingAssembly());

                FakePrefab.MarkAsFakePrefab(npcObj);
                UnityEngine.Object.DontDestroyOnLoad(npcObj);
                npcObj.SetActive(false);

                npcObj.layer = 22;

                var collection = npcObj.GetComponent<tk2dSprite>().Collection;
                SpeechPoint.transform.parent = npcObj.transform;

                FakePrefab.MarkAsFakePrefab(SpeechPoint);
                UnityEngine.Object.DontDestroyOnLoad(SpeechPoint);
                SpeechPoint.SetActive(true);


                var idleIdsList = new List<int>();
                var talkIdsList = new List<int>();

                foreach (string sprite in idleSpritePaths)
                {
                    idleIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                foreach (string sprite in talkSpritePaths)
                {
                    talkIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                tk2dSpriteAnimator spriteAnimator = npcObj.AddComponent<tk2dSpriteAnimator>();

                SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "idle", tk2dSpriteAnimationClip.WrapMode.Loop, idleFps);
                SpriteBuilder.AddAnimation(spriteAnimator, collection, talkIdsList, "talk", tk2dSpriteAnimationClip.WrapMode.Loop, talkFps);

                if (hitboxSize == null) hitboxSize = new IntVector2(20, 18);
                if (hitboxOffset == null) new IntVector2(5, 0);

                SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.BulletBlocker, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, hitboxSize, hitboxOffset);

                TalkDoerLite talkDoer = npcObj.AddComponent<TalkDoerLite>();

                talkDoer.placeableWidth = 4;
                talkDoer.placeableHeight = 3;
                talkDoer.difficulty = 0;
                talkDoer.isPassable = true;
                talkDoer.usesOverrideInteractionRegion = false;
                talkDoer.overrideRegionOffset = Vector2.zero;
                talkDoer.overrideRegionDimensions = Vector2.zero;
                talkDoer.overrideInteractionRadius = -1;
                talkDoer.PreventInteraction = false;
                talkDoer.AllowPlayerToPassEventually = true;
                talkDoer.speakPoint = SpeechPoint.transform;
                talkDoer.SpeaksGleepGlorpenese = false;
                talkDoer.audioCharacterSpeechTag = "oldman";
                talkDoer.playerApproachRadius = 5;
                talkDoer.conversationBreakRadius = 5;
                talkDoer.echo1 = null;
                talkDoer.echo2 = null;
                talkDoer.PreventCoopInteraction = false;
                talkDoer.IsPaletteSwapped = false;
                talkDoer.PaletteTexture = null;
                talkDoer.OutlineDepth = 0.5f;
                talkDoer.OutlineLuminanceCutoff = 0.05f;
                talkDoer.MovementSpeed = 3;
                talkDoer.PathableTiles = CellTypes.FLOOR;


                UltraFortunesFavor dreamLuck = npcObj.AddComponent<UltraFortunesFavor>();

                dreamLuck.goopRadius = 2;
                dreamLuck.beamRadius = 2;
                dreamLuck.bulletRadius = 2;
                dreamLuck.bulletSpeedModifier = 0.8f;

                dreamLuck.vfxOffset = 0.625f;
                dreamLuck.sparkOctantVFX = shared_auto_001.LoadAsset<GameObject>("FortuneFavor_VFX_Spark");


                AIAnimator aIAnimator = GenerateBlankAIAnimator(npcObj);
                aIAnimator.spriteAnimator = spriteAnimator;
                aIAnimator.IdleAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "idle",
                    AnimNames = new string[]
                    {
                        name + "_idle"
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }

                };

                aIAnimator.TalkAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "talk",
                    AnimNames = new string[]
                    {
                       name + "_talk"
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }
                };

                var basenpc = ResourceManager.LoadAssetBundle("shared_auto_001").LoadAsset<GameObject>("Merchant_Key").transform.Find("NPC_Key").gameObject;

                PlayMakerFSM iHaveNoFuckingClueWhatThisIs = npcObj.AddComponent<PlayMakerFSM>();

                UnityEngine.JsonUtility.FromJsonOverwrite(UnityEngine.JsonUtility.ToJson(basenpc.GetComponent<PlayMakerFSM>()), iHaveNoFuckingClueWhatThisIs);

                FieldInfo fsmStringParams = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[1].ActionData) as List<FsmString>)[0].Value = runBasedMultilineGenericStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[1].ActionData) as List<FsmString>)[1].Value = runBasedMultilineStopperStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[4].ActionData) as List<FsmString>)[0].Value = purchaseItemStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[5].ActionData) as List<FsmString>)[0].Value = purchaseItemFailedStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[7].ActionData) as List<FsmString>)[0].Value = introStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[8].ActionData) as List<FsmString>)[0].Value = attackedStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)[0].Value = "#SUBSHOP_GENERIC_CAUGHT_STEALING";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[10].ActionData) as List<FsmString>)[0].Value = "#SHOP_GENERIC_NO_SALE_LABEL";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[12].ActionData) as List<FsmString>)[0].Value = "#COOP_REBUKE";

                foreach (var state in iHaveNoFuckingClueWhatThisIs.Fsm.FsmComponent.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        if (action is DialogueBox && (action as DialogueBox).dialogue[0].Value == purchaseItemStringKey)
                        {
                            //((DialogueBox)action).OverrideTalkAnim = name + "_talk";
                            //((DialogueBox)action).SuppressDefaultAnims.Value = true;
                            //((DialogueBox)action).zombieTime = 0.01f;
                        }
                    }
                }

                npcObj.name = prefix + ":" + name;

                var posList = new List<Transform>();
                if (itemPositions == null)
                {
                    itemPositions = defaultItemPositions;
                }

                for (int i = 0; i < itemPositions.Length; i++)
                {

                    var ItemPoint = new GameObject("ItemPoint" + i);
                    ItemPoint.transform.position = itemPositions[i];
                    FakePrefab.MarkAsFakePrefab(ItemPoint);
                    UnityEngine.Object.DontDestroyOnLoad(ItemPoint);
                    ItemPoint.SetActive(true);
                    posList.Add(ItemPoint.transform);
                }

                var ItemPoint1 = new GameObject("ItemPoint1");
                ItemPoint1.transform.position = new Vector3(1.125f, 2.125f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint1);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint1);
                ItemPoint1.SetActive(true);
                var ItemPoint2 = new GameObject("ItemPoint2");
                ItemPoint2.transform.position = new Vector3(2.625f, 1f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint2);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint2);
                ItemPoint2.SetActive(true);
                var ItemPoint3 = new GameObject("ItemPoint3");
                ItemPoint3.transform.position = new Vector3(4.125f, 2.125f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint3);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint3);
                ItemPoint3.SetActive(true);


                var shopObj = new GameObject(prefix + ":" + name + "_Shop").AddComponent<CustomShopController>();
                FakePrefab.MarkAsFakePrefab(shopObj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(shopObj.gameObject);

                shopObj.gameObject.SetActive(false);

                shopObj.currencyType = currency;

                shopObj.ActionAndFuncSetUp(CustomCanBuy, CustomRemoveCurrency, CustomPrice, OnPurchase, OnSteal);

                if (!string.IsNullOrEmpty(currencyIconPath))
                {
                    shopObj.customPriceSprite = AddCustomCurrencyType(currencyIconPath, $"{prefix}:{currencyName}", Assembly.GetCallingAssembly());
                }
                else
                {
                    shopObj.customPriceSprite = currencyName;
                }


                //GungeonAPI.ToolsCharApi.AddNewItemToAtlas()

                shopObj.canBeRobbed = canBeRobbed;

                shopObj.placeableHeight = 5;
                shopObj.placeableWidth = 5;
                shopObj.difficulty = 0;
                shopObj.isPassable = true;
                shopObj.baseShopType = BaseShopController.AdditionalShopType.TRUCK;//shopType;

                shopObj.FoyerMetaShopForcedTiers = false;
                shopObj.IsBeetleMerchant = false;
                shopObj.ExampleBlueprintPrefab = null;
                shopObj.shopItems = lootTable;
                shopObj.spawnPositions = posList.ToArray();//{ ItemPoint1.transform, ItemPoint2.transform, ItemPoint3.transform };

                foreach (var pos in shopObj.spawnPositions)
                {
                    pos.parent = shopObj.gameObject.transform;
                }

                shopObj.shopItemsGroup2 = null;
                shopObj.spawnPositionsGroup2 = null;
                shopObj.spawnGroupTwoItem1Chance = 0.5f;
                shopObj.spawnGroupTwoItem2Chance = 0.5f;
                shopObj.spawnGroupTwoItem3Chance = 0.5f;
                shopObj.shopkeepFSM = npcObj.GetComponent<PlayMakerFSM>();
                shopObj.shopItemShadowPrefab = shared_auto_001.LoadAsset<GameObject>("Merchant_Key").GetComponent<BaseShopController>().shopItemShadowPrefab;

                shopObj.prerequisites = prerequisites;
                //shopObj.shopItemShadowPrefab = 

                shopObj.cat = null;


                if (hasMinimapIcon)
                {
                    if (!string.IsNullOrEmpty(minimapIconSpritePath))
                    {
                        shopObj.OptionalMinimapIcon = SpriteBuilder.SpriteFromResource(minimapIconSpritePath, assembly: Assembly.GetCallingAssembly());
                        UnityEngine.Object.DontDestroyOnLoad(shopObj.OptionalMinimapIcon);
                        FakePrefab.MarkAsFakePrefab(shopObj.OptionalMinimapIcon);
                    }
                    else
                    {
                        shopObj.OptionalMinimapIcon = ResourceCache.Acquire("Global Prefabs/Minimap_NPC_Icon") as GameObject;
                    }
                }

                shopObj.ShopCostModifier = costModifier;
                shopObj.FlagToSetOnEncounter = GungeonFlags.NONE;

                shopObj.giveStatsOnPurchase = giveStatsOnPurchase;
                shopObj.statsToGive = statsToGiveOnPurchase;

                //shopObj.

                /*if (isBreachShop)
                {
                    shopObj.gameObject.AddComponent<BreachShopComp>().offset = breachPos;
                    BreachShopTools.registeredShops.Add(prefix + ":" + name, shopObj.gameObject);

                    shopObj.FoyerMetaShopForcedTiers = true;

                    var exampleBlueprintObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_ExampleBlueprintPrefab"));
                    exampleBlueprintObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                    FakePrefab.MarkAsFakePrefab(exampleBlueprintObj);
                    UnityEngine.Object.DontDestroyOnLoad(exampleBlueprintObj);
                    exampleBlueprintObj.SetActive(false);

                    //var item = exampleBlueprintObj.AddComponent<ItemBlueprintItem>();
                    //item.quality = PickupObject.ItemQuality.SPECIAL;
                    //item.PickupObjectId = 99999999;
                    


                    shopObj.ExampleBlueprintPrefab = shared_auto_001.LoadAsset<GameObject>("NPC_Beetle_Merchant_Foyer").GetComponent<BaseShopController>().ExampleBlueprintPrefab;
                }/

                npcObj.transform.parent = shopObj.gameObject.transform;
                npcObj.transform.position = new Vector3(1.9375f, 3.4375f, 5.9375f);




                if (hasCarpet)
                {
                    var carpetObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_Carpet"), Assembly.GetCallingAssembly());
                    carpetObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                    FakePrefab.MarkAsFakePrefab(carpetObj);
                    UnityEngine.Object.DontDestroyOnLoad(carpetObj);
                    carpetObj.SetActive(true);

                    carpetObj.transform.position = new Vector3(0, 0, 1.7f);
                    carpetObj.transform.parent = shopObj.gameObject.transform;
                    carpetObj.layer = 20;
                }
                npcObj.SetActive(true);

                if (addToMainNpcPool)
                {
                    shared_auto_002.LoadAsset<DungeonPlaceable>("shopannex_contents_01").variantTiers.Add(new DungeonPlaceableVariant
                    {
                        percentChance = percentChanceForMainPool,
                        unitOffset = new Vector2(-0.5f, -1.25f),
                        nonDatabasePlaceable = shopObj.gameObject,
                        enemyPlaceableGuid = "",
                        pickupObjectPlaceableId = -1,
                        forceBlackPhantom = false,
                        addDebrisObject = false,
                        prerequisites = prerequisites, //shit for unlocks gose here sooner or later
                        materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0],

                    });
                }

                ShopAPI.builtShops.Add(prefix + ":" + name, shopObj.gameObject);
                return shopObj.gameObject;
            }
            catch (Exception message)
            {
                ETGModConsole.Log(message.ToString());
                return null;
            }
        }*/

        /// <summary>
        /// Creates a shop object along with an npc
        /// </summary>
        /// <param name="name">Name of the npc</param> 
        /// <param name="prefix">Mod prefix (for example Bot)</param> 
        /// 
        /// <param name="idleSpritePaths">List of *FULL* sprite paths for the idle animation</param> 
        /// <param name="idleFps">Fps of the idle animation (base game tends to use around 6)</param> 
        /// 
        /// <param name="talkSpritePaths">List of *FULL* sprite paths for the talk animation</param> 
        /// <param name="talkFps">Fps of the talk animation (base game tends to use around 8)</param> 
        /// 
        /// <param name="lootTable">Shop loot table</param> 
        /// <param name="currency">What is used to buy items at the shop</param> 
        /// 
        /// <param name="runBasedMultilineGenericStringKey">String key for normal convos</param> 
        /// <param name="runBasedMultilineStopperStringKey">String key for if you try talking to an npc to much</param> 
        /// <param name="purchaseItemStringKey">String key for when the player buys something</param> 
        /// <param name="purchaseItemFailedStringKey">String key for when the player tries but fails to buy something</param> 
        /// <param name="introStringKey">String key for when the player enters the room</param> 
        /// <param name="attackedStringKey">String key for when the player shoots at the npc</param> 
        /// <param name="costModifier">The multiplier for shop prices</param> 
        /// <param name="itemPositions">The offset for the item(s) sold by your npc, the amount of items sold is based off how many offsets you add here (if you just want the 3 normally items spots you can use ItsDaFuckinShopApi.defaultItemPositions)</param> 
        /// <param name="giveStatsOnPurchase">Whether the shop modifies stats after the player buys an item for example how cursula gives curse</param> 
        /// <param name="statsToGiveOnPurchase"> The stats given when the player buys an item (will be ingored if statsToGiveOnPurchase is false)</param> 
        /// 
        /// <param name="CustomCanBuy">The method that gets called to check if the player can buy an item (useless if currency isnt set to CUSTOM)</param> 
        /// <param name="CustomRemoveCurrency">The method that gets called remove currency from the player (useless if currency isnt set to CUSTOM)</param> 
        /// <param name="CustomPrice">The method that gets called to get the price of an item (useless if currency isnt set to CUSTOM)</param> 
        /// 
        /// <param name="currencyIconPath">Sprite path for your custom currency sprite</param> 
        /// <param name="currencyName">The name you want your custom currecy sprite to have (i should probably remove this...)</param> 
        ///
        /// <param name="hasCarpet">Whether the shop has a carpet or something else that they sit on</param> 
        /// <param name="carpetSpritePath">Sprite path for the carpet or whatever</param> 
        ///         
        /// <param name="hasMinimapIcon">Whether the shop has a minimap icon to show what room theyre in</param> 
        /// <param name="minimapIconSpritePath">Sprite path minimap icon leave blank to just use deafult smiley face</param> 
        /// 
        /// <param name="addToMainNpcPool">Whether the shop should be added to the pool of npcs that show up in the main shop a long side bello</param> 
        /// <param name="percentChanceForMainPool">How likely it is for the shop to show up in the main pool base game shops use 0.1</param> 
        /// 
        /// <param name="prerequisites">These do unlocks and shit</param> 
        /// <param name="fortunesFavorRadius">The radius of the fortunes favor effect.</param> 
        /// <param name="poolType">Determines how your shop pool will pick items. DEFAULT is how basegame does it, DUPES allows your shop to sell deplicates and DUPES_AND_NOEXCLUSION allows your shop to sell duplicate items and EXCLUDED tier items.</param> 
        /// <param name="RainbowModeImmunity">Enables/Disables immunity to the dreaded capitalism killer, Rainbow mode.</param> 

        /// <param name="stolenFromStringKey">String key for when the NPC is stolen from.</param> 
        /// <param name="talkPointOffset">The offset at which the NPCs text box will appear.</param>
        /// <param name="npcPosition">Additional offset for where the NPC is positioned relative to its goods.</param> 

        /// <param name="voiceBox">An enum for selecting which pre-existing voice the NPC will use when talking.</param> 
        /// <param name="OnPurchase">An action for when the NPC is purchased from.</param> 
        /// <param name="OnSteal">An action for when the NPC is stolen from.</param> 
        /// <param name="canBeRobbed">Toggles whether an NPC can be stolen from.</param> 

        /// <param name="CarpetOffset">The offset of your carpet, relative to your NPC.</param> 
        /// <param name="hitboxSize">The size of your enemies hitbox. Remember, 1 is equal to 16 pixels, not 1.</param> 
        /// <param name="hitboxOffset">The offset of your enemies hitbox. Remember, 1 is equal to 16 pixels, not 1.</param> 

        /// <returns></returns>
        /// 


        public static GameObject SetUpShop(string name, string prefix, List<string> idleSpritePaths, int idleFps, List<string> talkSpritePaths, int talkFps, GenericLootTable lootTable, CustomShopItemController.ShopCurrencyType currency, string runBasedMultilineGenericStringKey,
            string runBasedMultilineStopperStringKey, string purchaseItemStringKey, string purchaseItemFailedStringKey, string introStringKey, string attackedStringKey, string stolenFromStringKey, Vector3 talkPointOffset, Vector3 npcPosition, VoiceBoxes voiceBox = VoiceBoxes.OLD_MAN, Vector3[] itemPositions = null, float costModifier = 1, bool giveStatsOnPurchase = false,
            StatModifier[] statsToGiveOnPurchase = null, Func<CustomShopController, PlayerController, int, bool> CustomCanBuy = null, Func<CustomShopController, PlayerController, int, int> CustomRemoveCurrency = null, Func<CustomShopController, CustomShopItemController, PickupObject, int> CustomPrice = null,
            Func<PlayerController, PickupObject, int, bool> OnPurchase = null, Func<PlayerController, PickupObject, int, bool> OnSteal = null, string currencyIconPath = "", string currencyName = "", bool canBeRobbed = true, bool hasCarpet = false, string carpetSpritePath = "",
            Vector2? CarpetOffset = null, bool hasMinimapIcon = false, string minimapIconSpritePath = "", bool addToMainNpcPool = false, float percentChanceForMainPool = 0.1f, DungeonPrerequisite[] prerequisites = null, float fortunesFavorRadius = 2,
            ShopItemPoolType poolType = ShopItemPoolType.DEFAULT, bool RainbowModeImmunity = false, IntVector2? hitboxSize = null, IntVector2? hitboxOffset = null)
        {

            try
            {

                if (prerequisites == null)
                {
                    prerequisites = new DungeonPrerequisite[0];
                }
                //bool isBreachShop = false;
                Vector3 breachPos = Vector3.zero;

                var shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
                var shared_auto_002 = ResourceManager.LoadAssetBundle("shared_auto_002");
                var SpeechPoint = new GameObject("SpeechPoint");
                SpeechPoint.transform.position = talkPointOffset;



                var npcObj = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], new GameObject(prefix + ":" + name), Assembly.GetCallingAssembly());

                FakePrefab.MarkAsFakePrefab(npcObj);
                UnityEngine.Object.DontDestroyOnLoad(npcObj);
                npcObj.SetActive(false);

                npcObj.layer = 22;

                var collection = npcObj.GetComponent<tk2dSprite>().Collection;
                SpeechPoint.transform.parent = npcObj.transform;

                FakePrefab.MarkAsFakePrefab(SpeechPoint);
                UnityEngine.Object.DontDestroyOnLoad(SpeechPoint);
                SpeechPoint.SetActive(true);


                var idleIdsList = new List<int>();
                var talkIdsList = new List<int>();

                foreach (string sprite in idleSpritePaths)
                {
                    idleIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                foreach (string sprite in talkSpritePaths)
                {
                    talkIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                tk2dSpriteAnimator spriteAnimator = npcObj.AddComponent<tk2dSpriteAnimator>();

                SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "idle", tk2dSpriteAnimationClip.WrapMode.Loop, idleFps);
                SpriteBuilder.AddAnimation(spriteAnimator, collection, talkIdsList, "talk", tk2dSpriteAnimationClip.WrapMode.Loop, talkFps);


                if (hitboxSize == null) hitboxSize = new IntVector2(20, 18);
                if (hitboxOffset == null) new IntVector2(5, 0);

                SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.LowObstacle, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, hitboxSize, hitboxOffset);
                rigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBlocker));

                //SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.BulletBlocker, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, new IntVector2(20, 18), new IntVector2(5, 0));

                TalkDoerLite talkDoer = npcObj.AddComponent<TalkDoerLite>();

                talkDoer.placeableWidth = 4;
                talkDoer.placeableHeight = 3;
                talkDoer.difficulty = 0;
                talkDoer.isPassable = true;
                talkDoer.usesOverrideInteractionRegion = false;
                talkDoer.overrideRegionOffset = Vector2.zero;
                talkDoer.overrideRegionDimensions = Vector2.zero;
                talkDoer.overrideInteractionRadius = -1;
                talkDoer.PreventInteraction = false;
                talkDoer.AllowPlayerToPassEventually = true;
                talkDoer.speakPoint = SpeechPoint.transform;
                talkDoer.SpeaksGleepGlorpenese = false;
                talkDoer.audioCharacterSpeechTag = ReturnVoiceBox(voiceBox);
                talkDoer.playerApproachRadius = 5;
                talkDoer.conversationBreakRadius = 5;
                talkDoer.echo1 = null;
                talkDoer.echo2 = null;
                talkDoer.PreventCoopInteraction = false;
                talkDoer.IsPaletteSwapped = false;
                talkDoer.PaletteTexture = null;
                talkDoer.OutlineDepth = 0.5f;
                talkDoer.OutlineLuminanceCutoff = 0.05f;
                talkDoer.MovementSpeed = 3;
                talkDoer.PathableTiles = CellTypes.FLOOR;


                UltraFortunesFavor dreamLuck = npcObj.AddComponent<UltraFortunesFavor>();

                dreamLuck.goopRadius = fortunesFavorRadius;
                dreamLuck.beamRadius = fortunesFavorRadius;
                dreamLuck.bulletRadius = fortunesFavorRadius;
                dreamLuck.bulletSpeedModifier = 0.8f;

                dreamLuck.vfxOffset = 0.625f;
                dreamLuck.sparkOctantVFX = shared_auto_001.LoadAsset<GameObject>("FortuneFavor_VFX_Spark");


                AIAnimator aIAnimator = GenerateBlankAIAnimator(npcObj);
                aIAnimator.spriteAnimator = spriteAnimator;
                aIAnimator.IdleAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "idle",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }

                };

                aIAnimator.TalkAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "talk",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }
                };



                var basenpc = ResourceManager.LoadAssetBundle("shared_auto_001").LoadAsset<GameObject>("Merchant_Key").transform.Find("NPC_Key").gameObject;

                PlayMakerFSM iHaveNoFuckingClueWhatThisIs = npcObj.AddComponent<PlayMakerFSM>();

                UnityEngine.JsonUtility.FromJsonOverwrite(UnityEngine.JsonUtility.ToJson(basenpc.GetComponent<PlayMakerFSM>()), iHaveNoFuckingClueWhatThisIs);

                FieldInfo fsmStringParams = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[1].ActionData) as List<FsmString>)[0].Value = runBasedMultilineGenericStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[1].ActionData) as List<FsmString>)[1].Value = runBasedMultilineStopperStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[4].ActionData) as List<FsmString>)[0].Value = purchaseItemStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[5].ActionData) as List<FsmString>)[0].Value = purchaseItemFailedStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[7].ActionData) as List<FsmString>)[0].Value = introStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[8].ActionData) as List<FsmString>)[0].Value = attackedStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)[0].Value = stolenFromStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)[1].Value = stolenFromStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[10].ActionData) as List<FsmString>)[0].Value = "#SHOP_GENERIC_NO_SALE_LABEL";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[12].ActionData) as List<FsmString>)[0].Value = "#COOP_REBUKE";

                /*
                foreach (FsmString fuck in fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)
                {
                    ETGModConsole.Log(fuck.Value);
                }
                */

                npcObj.name = prefix + ":" + name;

                var posList = new List<Transform>();
                for (int i = 0; i < itemPositions.Length; i++)
                {

                    var ItemPoint = new GameObject("ItemPoint" + i);
                    ItemPoint.transform.position = itemPositions[i];
                    FakePrefab.MarkAsFakePrefab(ItemPoint);
                    UnityEngine.Object.DontDestroyOnLoad(ItemPoint);
                    ItemPoint.SetActive(true);
                    posList.Add(ItemPoint.transform);
                }

                var ItemPoint1 = new GameObject("ItemPoint1");
                ItemPoint1.transform.position = new Vector3(1.125f, 2.125f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint1);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint1);
                ItemPoint1.SetActive(true);
                var ItemPoint2 = new GameObject("ItemPoint2");
                ItemPoint2.transform.position = new Vector3(2.625f, 1f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint2);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint2);
                ItemPoint2.SetActive(true);
                var ItemPoint3 = new GameObject("ItemPoint3");
                ItemPoint3.transform.position = new Vector3(4.125f, 2.125f, 1);
                FakePrefab.MarkAsFakePrefab(ItemPoint3);
                UnityEngine.Object.DontDestroyOnLoad(ItemPoint3);
                ItemPoint3.SetActive(true);


                var shopObj = new GameObject(prefix + ":" + name + "_Shop").AddComponent<CustomShopController>();
                shopObj.AllowedToSpawnOnRainbowMode = RainbowModeImmunity;
                FakePrefab.MarkAsFakePrefab(shopObj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(shopObj.gameObject);

                shopObj.gameObject.SetActive(false);

                shopObj.currencyType = currency;

                shopObj.ActionAndFuncSetUp(CustomCanBuy, CustomRemoveCurrency, CustomPrice, OnPurchase, OnSteal);

                if (currency == CustomShopItemController.ShopCurrencyType.CUSTOM)
                {
                    if (!string.IsNullOrEmpty(currencyIconPath))
                    {
                        shopObj.customPriceSprite = AddCustomCurrencyType(currencyIconPath, $"{prefix}:{currencyName}", Assembly.GetCallingAssembly());
                    }
                    else
                    {
                        shopObj.customPriceSprite = currencyName;
                    }
                }



                //GungeonAPI.ToolsGAPI.AddNewItemToAtlas()

                shopObj.canBeRobbed = canBeRobbed;

                shopObj.placeableHeight = 5;
                shopObj.placeableWidth = 5;
                shopObj.difficulty = 0;
                shopObj.isPassable = true;
                shopObj.baseShopType = BaseShopController.AdditionalShopType.TRUCK;//shopType;

                shopObj.FoyerMetaShopForcedTiers = false;
                shopObj.IsBeetleMerchant = false;
                shopObj.ExampleBlueprintPrefab = null;
                shopObj.poolType = poolType;
                shopObj.shopItems = lootTable;
                shopObj.spawnPositions = posList.ToArray();//{ ItemPoint1.transform, ItemPoint2.transform, ItemPoint3.transform };

                foreach (var pos in shopObj.spawnPositions)
                {
                    pos.parent = shopObj.gameObject.transform;
                }

                shopObj.shopItemsGroup2 = null;
                shopObj.spawnPositionsGroup2 = null;
                shopObj.spawnGroupTwoItem1Chance = 0.5f;
                shopObj.spawnGroupTwoItem2Chance = 0.5f;
                shopObj.spawnGroupTwoItem3Chance = 0.5f;
                shopObj.shopkeepFSM = npcObj.GetComponent<PlayMakerFSM>();
                shopObj.shopItemShadowPrefab = shared_auto_001.LoadAsset<GameObject>("Merchant_Key").GetComponent<BaseShopController>().shopItemShadowPrefab;

                shopObj.prerequisites = prerequisites;
                //shopObj.shopItemShadowPrefab = 

                shopObj.cat = null;


                if (hasMinimapIcon)
                {
                    if (!string.IsNullOrEmpty(minimapIconSpritePath))
                    {
                        shopObj.OptionalMinimapIcon = SpriteBuilder.SpriteFromResource(minimapIconSpritePath, null, Assembly.GetCallingAssembly());
                        UnityEngine.Object.DontDestroyOnLoad(shopObj.OptionalMinimapIcon);
                        FakePrefab.MarkAsFakePrefab(shopObj.OptionalMinimapIcon);
                    }
                    else
                    {
                        shopObj.OptionalMinimapIcon = ResourceCache.Acquire("Global Prefabs/Minimap_NPC_Icon") as GameObject;
                    }
                }

                shopObj.ShopCostModifier = costModifier;
                shopObj.FlagToSetOnEncounter = GungeonFlags.NONE;
                shopObj.giveStatsOnPurchase = giveStatsOnPurchase;
                shopObj.statsToGive = statsToGiveOnPurchase;

                //shopObj.

                /*if (isBreachShop)
                {
                    shopObj.gameObject.AddComponent<BreachShopComp>().offset = breachPos;
                    BreachShopTools.registeredShops.Add(prefix + ":" + name, shopObj.gameObject);

                    shopObj.FoyerMetaShopForcedTiers = true;

                    var exampleBlueprintObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_ExampleBlueprintPrefab"));
                    exampleBlueprintObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                    FakePrefab.MarkAsFakePrefab(exampleBlueprintObj);
                    UnityEngine.Object.DontDestroyOnLoad(exampleBlueprintObj);
                    exampleBlueprintObj.SetActive(false);

                    //var item = exampleBlueprintObj.AddComponent<ItemBlueprintItem>();
                    //item.quality = PickupObject.ItemQuality.SPECIAL;
                    //item.PickupObjectId = 99999999;
                    


                    shopObj.ExampleBlueprintPrefab = shared_auto_001.LoadAsset<GameObject>("NPC_Beetle_Merchant_Foyer").GetComponent<BaseShopController>().ExampleBlueprintPrefab;
                }*/

                npcObj.transform.parent = shopObj.gameObject.transform;
                npcObj.transform.position = npcPosition;//new Vector3(1.9375f, 3.4375f, 5.9375f) + npcPositionOffset;




                if (hasCarpet)
                {
                    var carpetObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_Carpet"), Assembly.GetCallingAssembly());
                    carpetObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                    FakePrefab.MarkAsFakePrefab(carpetObj);
                    UnityEngine.Object.DontDestroyOnLoad(carpetObj);
                    carpetObj.SetActive(true);

                    if (CarpetOffset == null) CarpetOffset = Vector2.zero;

                    carpetObj.transform.position = new Vector3(CarpetOffset.Value.x, CarpetOffset.Value.y, 1.7f);
                    carpetObj.transform.parent = shopObj.gameObject.transform;
                    carpetObj.layer = 20;
                }
                npcObj.SetActive(true);

                if (addToMainNpcPool)
                {
                    shared_auto_002.LoadAsset<DungeonPlaceable>("shopannex_contents_01").variantTiers.Add(new DungeonPlaceableVariant
                    {
                        percentChance = percentChanceForMainPool,
                        unitOffset = new Vector2(-0.5f, -1.25f),
                        nonDatabasePlaceable = shopObj.gameObject,
                        enemyPlaceableGuid = "",
                        pickupObjectPlaceableId = -1,
                        forceBlackPhantom = false,
                        addDebrisObject = false,
                        prerequisites = prerequisites, //shit for unlocks gose here sooner or later
                        materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0],

                    });
                }

                ShopAPI.builtShops.Add(prefix + ":" + name, shopObj.gameObject);
                return shopObj.gameObject;
            }
            catch (Exception message)
            {
                ETGModConsole.Log(message.ToString());
                return null;
            }
        }

        public static GameObject SetUpFoyerShop(string name, string prefix, Vector2 position, List<string> idleSpritePaths, int idleFps, List<string> talkSpritePaths, int talkFps, string bluePrintSpritePath, GenericLootTable lootTable, CustomShopItemController.ShopCurrencyType currency, string runBasedMultilineGenericStringKey,
            string runBasedMultilineStopperStringKey, string purchaseItemStringKey, string purchaseItemFailedStringKey, string introStringKey, Vector3 talkPointOffset, Vector3 npcPosition, VoiceBoxes voiceBox = VoiceBoxes.OLD_MAN,
            Vector3[] itemPositions = null, float costModifier = 1, Func<CustomShopController, PlayerController, int, bool> CustomCanBuy = null, Func<CustomShopController, PlayerController, int, int> CustomRemoveCurrency = null, Func<CustomShopController, CustomShopItemController,
            PickupObject, int> CustomPrice = null, Func<PlayerController, PickupObject, int, bool> OnPurchase = null, Func<PlayerController, PickupObject, int, bool> OnSteal = null, string currencyIconPath = "", string currencyName = "", bool hasCarpet = false, 
            string carpetSpritePath = "", Vector2? CarpetOffset = null, DungeonPrerequisite[] prerequisites = null, IntVector2? hitboxSize = null, IntVector2? hitboxOffset = null)
        {

            try
            {

                //bool isBreachShop = false;
                Vector3 breachPos = Vector3.zero;

                var shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
                var shared_auto_002 = ResourceManager.LoadAssetBundle("shared_auto_002");
                var SpeechPoint = new GameObject("SpeechPoint");
                SpeechPoint.transform.position = talkPointOffset;



                var npcObj = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], new GameObject(prefix + ":" + name), Assembly.GetCallingAssembly());

                FakePrefab.MarkAsFakePrefab(npcObj);
                UnityEngine.Object.DontDestroyOnLoad(npcObj);
                npcObj.SetActive(false);

                npcObj.layer = 22;

                var collection = npcObj.GetComponent<tk2dSprite>().Collection;
                SpeechPoint.transform.parent = npcObj.transform;

                FakePrefab.MarkAsFakePrefab(SpeechPoint);
                UnityEngine.Object.DontDestroyOnLoad(SpeechPoint);
                SpeechPoint.SetActive(true);


                var idleIdsList = new List<int>();
                var talkIdsList = new List<int>();

                foreach (string sprite in idleSpritePaths)
                {
                    idleIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                foreach (string sprite in talkSpritePaths)
                {
                    talkIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection, Assembly.GetCallingAssembly()));
                }

                tk2dSpriteAnimator spriteAnimator = npcObj.AddComponent<tk2dSpriteAnimator>();

                SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "idle", tk2dSpriteAnimationClip.WrapMode.Loop, idleFps);
                SpriteBuilder.AddAnimation(spriteAnimator, collection, talkIdsList, "talk", tk2dSpriteAnimationClip.WrapMode.Loop, talkFps);


                if (hitboxSize == null) hitboxSize = new IntVector2(20, 18);
                if (hitboxOffset == null) new IntVector2(5, 0);

                SpeculativeRigidbody rigidbody = GenerateOrAddToRigidBody(npcObj, CollisionLayer.LowObstacle, PixelCollider.PixelColliderGeneration.Manual, true, true, true, false, false, false, false, true, hitboxSize, hitboxOffset);
                rigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.BulletBlocker));

                TalkDoerLite talkDoer = npcObj.AddComponent<TalkDoerLite>();

                talkDoer.placeableWidth = 4;
                talkDoer.placeableHeight = 3;
                talkDoer.difficulty = 0;
                talkDoer.isPassable = true;
                talkDoer.usesOverrideInteractionRegion = false;
                talkDoer.overrideRegionOffset = Vector2.zero;
                talkDoer.overrideRegionDimensions = Vector2.zero;
                talkDoer.overrideInteractionRadius = -1;
                talkDoer.PreventInteraction = false;
                talkDoer.AllowPlayerToPassEventually = true;
                talkDoer.speakPoint = SpeechPoint.transform;
                talkDoer.SpeaksGleepGlorpenese = false;
                talkDoer.audioCharacterSpeechTag = ReturnVoiceBox(voiceBox);
                talkDoer.playerApproachRadius = 5;
                talkDoer.conversationBreakRadius = 5;
                talkDoer.echo1 = null;
                talkDoer.echo2 = null;
                talkDoer.PreventCoopInteraction = false;
                talkDoer.IsPaletteSwapped = false;
                talkDoer.PaletteTexture = null;
                talkDoer.OutlineDepth = 0.5f;
                talkDoer.OutlineLuminanceCutoff = 0.05f;
                talkDoer.MovementSpeed = 3;
                talkDoer.PathableTiles = CellTypes.FLOOR;


                AIAnimator aIAnimator = GenerateBlankAIAnimator(npcObj);
                aIAnimator.spriteAnimator = spriteAnimator;
                aIAnimator.IdleAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "idle",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }

                };

                aIAnimator.TalkAnimation = new DirectionalAnimation
                {
                    Type = DirectionalAnimation.DirectionType.Single,
                    Prefix = "talk",
                    AnimNames = new string[]
                    {
                        ""
                    },
                    Flipped = new DirectionalAnimation.FlipType[]
                    {
                        DirectionalAnimation.FlipType.None
                    }
                };

                //idle, msb, end convo, purchas (purchaseItemStringKey), failed buy (purchaseItemFailedStringKey), end convon, ouch (attackedStringKey), has talked, drip (runBasedMultilineGenericStringKey, runBasedMultilineStopperStringKey), fist chat (foyerFirstChat), check coop,
                //tell coop to fuck off (runBasedMultilineStopperStringKey), end state 5

                var basenpc = ResourceManager.LoadAssetBundle("shared_auto_002").LoadAsset<GameObject>("NPC_Truck_Merchant_Foyer").transform.Find("NPC_Trucker_Foyer").gameObject;

                PlayMakerFSM iHaveNoFuckingClueWhatThisIs = npcObj.AddComponent<PlayMakerFSM>();

                UnityEngine.JsonUtility.FromJsonOverwrite(UnityEngine.JsonUtility.ToJson(basenpc.GetComponent<PlayMakerFSM>()), iHaveNoFuckingClueWhatThisIs);

                FieldInfo fsmStringParams = typeof(ActionData).GetField("fsmStringParams", BindingFlags.NonPublic | BindingFlags.Instance);

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[3].ActionData) as List<FsmString>)[0].Value = purchaseItemStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[3].ActionData) as List<FsmString>)[1].Value = "";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[4].ActionData) as List<FsmString>)[0].Value = purchaseItemFailedStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[4].ActionData) as List<FsmString>)[1].Value = "";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[6].ActionData) as List<FsmString>)[0].Value = "";

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[8].ActionData) as List<FsmString>)[0].Value = runBasedMultilineGenericStringKey;
                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[8].ActionData) as List<FsmString>)[1].Value = runBasedMultilineStopperStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)[0].Value = introStringKey;

                (fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[11].ActionData) as List<FsmString>)[0].Value = runBasedMultilineStopperStringKey;


                
                //foreach (FsmString fuck in fsmStringParams.GetValue(iHaveNoFuckingClueWhatThisIs.FsmStates[9].ActionData) as List<FsmString>)
                //{
                //    ETGModConsole.Log(fuck.Value);
                //}
                

                npcObj.name = prefix + ":" + name;

                var posList = new List<Transform>();
                for (int i = 0; i < itemPositions.Length; i++)
                {

                    var ItemPoint = new GameObject("ItemPoint" + i);
                    ItemPoint.transform.position = itemPositions[i];
                    FakePrefab.MarkAsFakePrefab(ItemPoint);
                    UnityEngine.Object.DontDestroyOnLoad(ItemPoint);
                    ItemPoint.SetActive(true);
                    posList.Add(ItemPoint.transform);
                }

                var shopObj = new GameObject(prefix + ":" + name + "_Shop").AddComponent<CustomShopController>();
                FakePrefab.MarkAsFakePrefab(shopObj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(shopObj.gameObject);

                shopObj.gameObject.SetActive(false);

                shopObj.currencyType = currency;

                shopObj.ActionAndFuncSetUp(CustomCanBuy, CustomRemoveCurrency, CustomPrice, OnPurchase, OnSteal);

                if (currency == CustomShopItemController.ShopCurrencyType.CUSTOM)
                {
                    if (!string.IsNullOrEmpty(currencyIconPath))
                    {
                        shopObj.customPriceSprite = AddCustomCurrencyType(currencyIconPath, $"{prefix}:{currencyName}", Assembly.GetCallingAssembly());
                    }
                    else
                    {
                        shopObj.customPriceSprite = currencyName;
                    }
                }


                shopObj.placeableHeight = 5;
                shopObj.placeableWidth = 5;
                shopObj.difficulty = 0;
                shopObj.isPassable = true;
                shopObj.baseShopType = BaseShopController.AdditionalShopType.FOYER_META;//shopType;

                shopObj.ExampleBlueprintPrefab = GenerateBluePrint($"{name} Blueprint", prefix, bluePrintSpritePath, Assembly.GetCallingAssembly());
                shopObj.IsBeetleMerchant = false;
                //shopObj.ExampleBlueprintPrefab = null;
                shopObj.shopItems = lootTable;
                shopObj.spawnPositions = posList.ToArray();//{ ItemPoint1.transform, ItemPoint2.transform, ItemPoint3.transform };

                foreach (var pos in shopObj.spawnPositions)
                {
                    pos.parent = shopObj.gameObject.transform;
                }

                shopObj.shopItemsGroup2 = null;
                shopObj.spawnPositionsGroup2 = null;
                shopObj.spawnGroupTwoItem1Chance = 0.5f;
                shopObj.spawnGroupTwoItem2Chance = 0.5f;
                shopObj.spawnGroupTwoItem3Chance = 0.5f;
                shopObj.shopkeepFSM = npcObj.GetComponent<PlayMakerFSM>();
                shopObj.shopItemShadowPrefab = shared_auto_001.LoadAsset<GameObject>("Merchant_Key").GetComponent<BaseShopController>().shopItemShadowPrefab;

                shopObj.prerequisites = prerequisites ?? new DungeonPrerequisite[0];
                //shopObj.shopItemShadowPrefab = 

                shopObj.cat = null;
                shopObj.ShopCostModifier = costModifier;
                shopObj.FlagToSetOnEncounter = GungeonFlags.NONE;


                //var exampleBlueprintObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_ExampleBlueprintPrefab"), Assembly.GetCallingAssembly());
                //exampleBlueprintObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                //FakePrefab.MarkAsFakePrefab(exampleBlueprintObj);
                //UnityEngine.Object.DontDestroyOnLoad(exampleBlueprintObj);
                //exampleBlueprintObj.SetActive(false);

                //var item = exampleBlueprintObj.AddComponent<ItemBlueprintItem>();
                //item.quality = PickupObject.ItemQuality.SPECIAL;
                //item.PickupObjectId = 99999999;

                shopObj.gameObject.AddComponent<BreachShopComp>().offset = position;
                BreachShopTools.registeredShops.Add(prefix + ":" + name + "_Shop", shopObj.gameObject);

                npcObj.transform.parent = shopObj.gameObject.transform;
                npcObj.transform.position = npcPosition;//new Vector3(1.9375f, 3.4375f, 5.9375f) + npcPositionOffset;




                if (hasCarpet)
                {
                    var carpetObj = SpriteBuilder.SpriteFromResource(carpetSpritePath, new GameObject(prefix + ":" + name + "_Carpet"), Assembly.GetCallingAssembly());
                    carpetObj.GetComponent<tk2dSprite>().SortingOrder = 2;
                    FakePrefab.MarkAsFakePrefab(carpetObj);
                    UnityEngine.Object.DontDestroyOnLoad(carpetObj);
                    carpetObj.SetActive(true);

                    if (CarpetOffset == null) CarpetOffset = Vector2.zero;

                    carpetObj.transform.position = new Vector3(CarpetOffset.Value.x, CarpetOffset.Value.y, 1.7f);
                    carpetObj.transform.parent = shopObj.gameObject.transform;
                    carpetObj.layer = 20;
                }
                npcObj.SetActive(true);
                
                ShopAPI.builtShops.Add(prefix + ":" + name, shopObj.gameObject);
                return shopObj.gameObject;
            }
            catch (Exception message)
            {
                ETGModConsole.Log(message.ToString());
                return null;
            }
        }


        public static GameObject GenerateBluePrint(string name, string prefix, string spritePath, Assembly assembly = null)
        {
            var bluePrint = ItemBuilder.BuildItem<ItemBlueprintItem>(name, prefix, spritePath, "", "", PickupObject.ItemQuality.SPECIAL, assembly ?? Assembly.GetCallingAssembly());
            bluePrint.UsesCustomCost = true;
            bluePrint.CustomCost = 10;
            bluePrint.PersistsOnPurchase = false;

            return bluePrint.gameObject;
        }


        /// <summary>
        /// Adds additional animations to certain events to your NPC. To Note, the aanimation names that are used here for certain animations (If the NPC will have one) are called: 
        /// 
        /// On Purchase Animation Name: purchase
        /// 
        /// On Denied Purchase Animation Name: denied
        /// 
        /// On Stolen From Animation Name: stolen
        /// </summary>
        /// <param name="self">The GameObject SetUpShop() returns.</param> 
        /// <param name="purchaseSpritePaths">Your purchase animation sprite paths.</param> 
        /// <param name="purchaseAnimFPS">Your purchase animation FPS.</param> 
        /// <param name="denyPurchaseSpritePaths">Your purchase denied animation sprite paths.</param> 
        /// <param name="denyPurchaseAnimFPS">Your purchase denied animation FPS.</param> 
        /// <param name="stealSpritePaths">Your on stolen from animation sprite paths.</param> 
        /// <param name="stealAnimFPS">Your on stolen from animation FPS.</param> 
        public static void AddAdditionalAnimationsToShop(GameObject self, List<string> purchaseSpritePaths = null, float purchaseAnimFPS = 6, List<string> denyPurchaseSpritePaths = null, float denyPurchaseAnimFPS = 6, List<string> stealSpritePaths = null, float stealAnimFPS = 6)
        {
            var collection = self.GetComponentInChildren<tk2dSprite>().Collection;
            tk2dSpriteAnimator spriteAnimator = self.GetComponentInChildren<tk2dSpriteAnimator>();
            AIAnimator aianimator = self.GetComponentInChildren<AIAnimator>();

            if (purchaseSpritePaths != null)
            {
                var danceIdsList = new List<int>();
                foreach (string sprite in purchaseSpritePaths)
                {
                    danceIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
                }
                CreateDirectionalAnimation(spriteAnimator, collection, aianimator, danceIdsList, "purchase", purchaseAnimFPS);
            }
            if (denyPurchaseSpritePaths != null)
            {
                var denyIdsList = new List<int>();
                foreach (string sprite in denyPurchaseSpritePaths)
                {
                    denyIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
                }
                CreateDirectionalAnimation(spriteAnimator, collection, aianimator, denyIdsList, "denied", denyPurchaseAnimFPS);
            }
            if (stealSpritePaths != null)
            {
                var stealIdsList = new List<int>();
                foreach (string sprite in stealSpritePaths)
                {
                    stealIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
                }
                CreateDirectionalAnimation(spriteAnimator, collection, aianimator, stealIdsList, "stolen", stealAnimFPS);
            }
        }

        /// <summary>
        /// Changes the voice that your NPC makes
        /// </summary>
        /// <param name="self">The GameObject SetUpShop() returns.</param> 
        /// <param name="voicebox">The given VoiceBox to change to.</param> 
        public static void ChangeVoiceBox(GameObject self, VoiceBoxes voicebox)
        {
            TalkDoerLite talker = self.GetComponentInChildren<TalkDoerLite>();
            if (self == null || talker == null)
            {
                ETGModConsole.Log("NPCAPI: Unable to detect TalkDoerLite/GameObject in given object!");
                return;
            }
            talker.audioCharacterSpeechTag = ReturnVoiceBox(voicebox);
        }

        /// <summary>
        /// Add a new DIRECTIONAL animation to your NPC. You can then play it when you need to using CustomShopController.TryPlayAnimation()
        /// </summary>
        /// <param name="self">The GameObject SetUpShop() returns.</param> 
        /// <param name="yourPaths">The sprite paths for your animation.</param> 
        /// <param name="YourAnimFPS">Your animations FPS.</param> 
        /// <param name="AnimationName">Your DIRECTIONAL animations name, along with the animations name.</param> 

        public static void AddParentedAnimationToShop(GameObject self, List<string> yourPaths, float YourAnimFPS, string AnimationName)
        {
            var collection = self.GetComponentInChildren<tk2dSprite>().Collection;
            tk2dSpriteAnimator spriteAnimator = self.GetComponentInChildren<tk2dSpriteAnimator>();
            AIAnimator aianimator = self.GetComponentInChildren<AIAnimator>();
            if (yourPaths != null)
            {
                var stealIdsList = new List<int>();
                foreach (string sprite in yourPaths)
                {
                    stealIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
                }
                CreateDirectionalAnimation(spriteAnimator, collection, aianimator, stealIdsList, AnimationName, YourAnimFPS);
            }
        }

        /// <summary>
        /// Add a new animation to your NPC. This animation is NOT a directional one, but can still be played via switching a pre-existing directional animations AnimNames with this animations name or other means.
        /// </summary>
        /// <param name="self">The GameObject SetUpShop() returns.</param> 
        /// <param name="yourPaths">The sprite paths for your animation.</param> 
        /// <param name="YourAnimFPS">Your animations FPS.</param> 
        /// <param name="AnimationName">Your NON DIRECTIONAL animations name.</param> 
        public static void AddUnparentedAnimationToShop(GameObject self, List<string> yourPaths, float YourAnimFPS, string AnimationName)
        {
            var collection = self.GetComponentInChildren<tk2dSprite>().Collection;
            tk2dSpriteAnimator spriteAnimator = self.GetComponentInChildren<tk2dSpriteAnimator>();
            AIAnimator aianimator = self.GetComponentInChildren<AIAnimator>();
            if (yourPaths != null)
            {
                var IdsList = new List<int>();
                foreach (string sprite in yourPaths)
                {
                    IdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
                }
                SpriteBuilder.AddAnimation(spriteAnimator, collection, IdsList, AnimationName, tk2dSpriteAnimationClip.WrapMode.Once, YourAnimFPS);
            }
        }
        /// <summary>
        /// Modifies a pre-existing directional animation in your NPC to use a different NON-Directional animation that it ALSO has.
        /// </summary>
        /// <param name="self">The GameObject SetUpShop() returns.</param> 
        /// <param name="DirectionalAnimationPrefixToModify">The Prefix of your DIRECTIONAL animation.</param> 
        /// <param name="YourNonDirectionalAnimationName">The NAME of your NON DIRECTIONAL animation.</param> 
        public static void ModifyPreExistingDirectionalAnimation(GameObject self, string DirectionalAnimationPrefixToModify, string YourNonDirectionalAnimationName)
        {
            List<AIAnimator.NamedDirectionalAnimation> lists2 = self.GetComponentInChildren<AIAnimator>().OtherAnimations;
            for (int k = 0; k < lists2.Count; k++)
            {
                if (lists2[k].anim.Prefix == DirectionalAnimationPrefixToModify)
                {
                    lists2[k].anim.AnimNames = new string[] { YourNonDirectionalAnimationName };
                }
            }
        }


        private static void CreateDirectionalAnimation(tk2dSpriteAnimator spriteAnimator, tk2dSpriteCollectionData collection, AIAnimator aianimator, List<int> IdsList, string animationName, float FPS)
        {
            SpriteBuilder.AddAnimation(spriteAnimator, collection, IdsList, animationName, tk2dSpriteAnimationClip.WrapMode.Once, FPS);
            DirectionalAnimation aa = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.Single,
                Prefix = animationName,
                AnimNames = new string[1],
                Flipped = new DirectionalAnimation.FlipType[1]
            };
            if (aianimator.OtherAnimations != null)
            {
                aianimator.OtherAnimations.Add(
                new AIAnimator.NamedDirectionalAnimation
                {
                    name = animationName,
                    anim = aa
                });
            }
            else
            {
                aianimator.OtherAnimations = new List<AIAnimator.NamedDirectionalAnimation>
                {
                    new AIAnimator.NamedDirectionalAnimation
                    {
                        name = animationName,
                        anim = aa
                    }
                };
            }


        }

        public enum VoiceBoxes
        {
            FEMALE,
            BROTHER_ALBERN,
            FRUMP,
            SER_MANUEL,
            OLD_MAN,
            AGUNIM,
            BELLO,
            MANLY,
            EMP_ROR,
            ROBOT,
            RESOURCEFUL_RAT,
            WITCH_1,
            WITCH_2,
            WITCH_3,
            MALE,
            DAISUKE,
            BOWLER,
            TONIC,
            GUNSLING_KING,
            TEEN,
            SYNERGRACE,
            DOUG,
            ALIEN,
            WINCHESTER,
            OX,
            BRAT,
            JOLLY,
            MONSTER_MANUEL,
            FOOL,
            CONVICT,
            VAMPIRE,
            CO_OP,
            SPACE_ROGUE,
            COMPUTER
        };



        public static string ReturnVoiceBox(VoiceBoxes voicebox)
        {
            string voice = "oldman";
            switch (voicebox)
            {
                case VoiceBoxes.FEMALE:
                    voice = "female";
                    return voice;
                case VoiceBoxes.BROTHER_ALBERN:
                    voice = "truthknower";
                    return voice;
                case VoiceBoxes.FRUMP:
                    voice = "frump";
                    return voice;
                case VoiceBoxes.SER_MANUEL:
                    voice = "tutorialknight";
                    return voice;
                case VoiceBoxes.OLD_MAN:
                    voice = "oldman";
                    return voice;
                case VoiceBoxes.AGUNIM:
                    voice = "agunim";
                    return voice;
                case VoiceBoxes.BELLO:
                    voice = "shopkeep";
                    return voice;
                case VoiceBoxes.MANLY:
                    voice = "manly";
                    return voice;
                case VoiceBoxes.EMP_ROR:
                    voice = "mainframe";
                    return voice;
                case VoiceBoxes.ROBOT:
                    voice = "robot";
                    return voice;
                case VoiceBoxes.RESOURCEFUL_RAT:
                    voice = "rat";
                    return voice;
                case VoiceBoxes.WITCH_1:
                    voice = "witch1";
                    return voice;
                case VoiceBoxes.WITCH_2:
                    voice = "witch2";
                    return voice;
                case VoiceBoxes.WITCH_3:
                    voice = "witch3";
                    return voice;
                case VoiceBoxes.MALE:
                    voice = "male";
                    return voice;
                case VoiceBoxes.DAISUKE:
                    voice = "dice";
                    return voice;
                case VoiceBoxes.BOWLER:
                    voice = "bower";
                    return voice;
                case VoiceBoxes.TONIC:
                    voice = "goofy";
                    return voice;
                case VoiceBoxes.GUNSLING_KING:
                    voice = "gunslingking";
                    return voice;
                case VoiceBoxes.TEEN:
                    voice = "teen";
                    return voice;
                case VoiceBoxes.SYNERGRACE:
                    voice = "Lady";
                    return voice;
                case VoiceBoxes.DOUG:
                    voice = "bug";
                    return voice;
                case VoiceBoxes.ALIEN:
                    voice = "alien";
                    return voice;
                case VoiceBoxes.WINCHESTER:
                    voice = "gambler";
                    return voice;
                case VoiceBoxes.OX:
                    voice = "golem";
                    return voice;
                case VoiceBoxes.BRAT:
                    voice = "brat";
                    return voice;
                case VoiceBoxes.JOLLY:
                    voice = "jolly";
                    return voice;
                case VoiceBoxes.MONSTER_MANUEL:
                    voice = "owl";
                    return voice;
                case VoiceBoxes.FOOL:
                    voice = "fool";
                    return voice;
                case VoiceBoxes.CONVICT:
                    voice = "convict";
                    return voice;
                case VoiceBoxes.VAMPIRE:
                    voice = "vampire";
                    return voice;
                case VoiceBoxes.CO_OP:
                    voice = "coop";
                    return voice;
                case VoiceBoxes.SPACE_ROGUE:
                    voice = "spacerogue";
                    return voice;
                case VoiceBoxes.COMPUTER:
                    voice = "computer";
                    return voice;
                default:
                    voice = "oldman";
                    return voice;

            }
        }

        public static GameObject SetUpJailedNpc(string name, string prefix, List<string> idleSpritePaths, int idleFps, Vector3 talkPointOffset, GungeonFlags flag)
        {
            var shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
            var shared_auto_002 = ResourceManager.LoadAssetBundle("shared_auto_002");
            var SpeechPoint = new GameObject("SpeechPoint");
            SpeechPoint.transform.position = talkPointOffset;



            var npcObj = SpriteBuilder.SpriteFromResource(idleSpritePaths[0], new GameObject(prefix + ":" + name));

            FakePrefab.MarkAsFakePrefab(npcObj);
            UnityEngine.Object.DontDestroyOnLoad(npcObj);
            npcObj.SetActive(false);

            npcObj.layer = 22;

            var collection = npcObj.GetComponent<tk2dSprite>().Collection;
            SpeechPoint.transform.parent = npcObj.transform;

            FakePrefab.MarkAsFakePrefab(SpeechPoint);
            UnityEngine.Object.DontDestroyOnLoad(SpeechPoint);
            SpeechPoint.SetActive(true);


            var idleIdsList = new List<int>();

            foreach (string sprite in idleSpritePaths)
            {
                idleIdsList.Add(SpriteBuilder.AddSpriteToCollection(sprite, collection));
            }

            tk2dSpriteAnimator spriteAnimator = npcObj.AddComponent<tk2dSpriteAnimator>();

            SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, name + "_idle", tk2dSpriteAnimationClip.WrapMode.Loop, idleFps);

            PlayMakerFSM nightmareNightmareNightmare = npcObj.AddComponent<PlayMakerFSM>();

            var basenpc = shared_auto_002.LoadAsset<GameObject>("NPC_Key_Jailed");

            UnityEngine.JsonUtility.FromJsonOverwrite(UnityEngine.JsonUtility.ToJson(basenpc.GetComponent<PlayMakerFSM>()), nightmareNightmareNightmare);

            foreach (var state in nightmareNightmareNightmare.Fsm.FsmComponent.FsmStates)
            {
                foreach (var action in state.Actions)
                {
                    if (action is SetSaveFlag)
                    {
                        ((SetSaveFlag)action).targetFlag = flag;
                    }
                }
            }

            AIAnimator aIAnimator = GenerateBlankAIAnimator(npcObj);
            aIAnimator.spriteAnimator = spriteAnimator;
            aIAnimator.IdleAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.Single,
                Prefix = name + "_idle",
                AnimNames = new string[]
                {
                        ""
                },
                Flipped = new DirectionalAnimation.FlipType[]
                {
                        DirectionalAnimation.FlipType.None
                }

            };

            TalkDoerLite talkDoer = npcObj.AddComponent<TalkDoerLite>();

            talkDoer.placeableWidth = 4;
            talkDoer.placeableHeight = 3;
            talkDoer.difficulty = 0;
            talkDoer.isPassable = true;
            talkDoer.usesOverrideInteractionRegion = false;
            talkDoer.overrideRegionOffset = Vector2.zero;
            talkDoer.overrideRegionDimensions = Vector2.zero;
            talkDoer.overrideInteractionRadius = -1;
            talkDoer.PreventInteraction = false;
            talkDoer.AllowPlayerToPassEventually = true;
            talkDoer.speakPoint = SpeechPoint.transform;
            talkDoer.SpeaksGleepGlorpenese = false;
            talkDoer.audioCharacterSpeechTag = "oldman";
            talkDoer.playerApproachRadius = 5;
            talkDoer.conversationBreakRadius = 5;
            talkDoer.echo1 = null;
            talkDoer.echo2 = null;
            talkDoer.PreventCoopInteraction = false;
            talkDoer.IsPaletteSwapped = false;
            talkDoer.PaletteTexture = null;
            talkDoer.OutlineDepth = 0.5f;
            talkDoer.OutlineLuminanceCutoff = 0.05f;
            talkDoer.MovementSpeed = 3;
            talkDoer.PathableTiles = CellTypes.FLOOR;

            UltraFortunesFavor dreamLuck = npcObj.AddComponent<UltraFortunesFavor>();

            dreamLuck.goopRadius = 2;
            dreamLuck.beamRadius = 2;
            dreamLuck.bulletRadius = 2;
            dreamLuck.bulletSpeedModifier = 0.8f;

            dreamLuck.vfxOffset = 0.625f;
            dreamLuck.sparkOctantVFX = shared_auto_001.LoadAsset<GameObject>("FortuneFavor_VFX_Spark");

            shared_auto_002.LoadAsset<DungeonPlaceable>("Generic Jailed NPC").variantTiers.Insert(1, new DungeonPlaceableVariant
            {
                percentChance = 0.1f,
                unitOffset = new Vector2(0f, 0f),
                nonDatabasePlaceable = npcObj.gameObject,
                enemyPlaceableGuid = "",
                pickupObjectPlaceableId = -1,
                forceBlackPhantom = false,
                addDebrisObject = false,
                prerequisites = new DungeonPrerequisite[0], //shit for unlocks gose here sooner or later
                materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0],

            });

            return npcObj;
        }

        public static string AddCustomCurrencyType(string ammoTypeSpritePath, string name, Assembly assembly = null)
        {
            return GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas.AddNewItemToAtlas(ResourceExtractor.GetTextureFromResource(ammoTypeSpritePath, assembly ?? Assembly.GetCallingAssembly()), name).name;
        }






        public static void RegisterShopRoom(GameObject shop, PrototypeDungeonRoom protoroom, Vector2 vector)
        {
            protoroom.category = PrototypeDungeonRoom.RoomCategory.NORMAL;
            DungeonPrerequisite[] array = shop.GetComponent<CustomShopController>()?.prerequisites != null ? shop.GetComponent<CustomShopController>().prerequisites : new DungeonPrerequisite[0];
            //Vector2 vector = new Vector2((float)(protoroom.Width / 2) + offset.x, (float)(protoroom.Height / 2) + offset.y);
            protoroom.placedObjectPositions.Add(vector);
            protoroom.placedObjects.Add(new PrototypePlacedObjectData
            {
                contentsBasePosition = vector,
                fieldData = new List<PrototypePlacedObjectFieldData>(),
                instancePrerequisites = array,
                linkedTriggerAreaIDs = new List<int>(),
                placeableContents = new DungeonPlaceable
                {
                    width = 2,
                    height = 2,
                    respectsEncounterableDifferentiator = true,
                    variantTiers = new List<DungeonPlaceableVariant>
                    {
                        new DungeonPlaceableVariant
                        {
                            percentChance = 1f,
                            nonDatabasePlaceable = shop,
                            prerequisites = array,
                            materialRequirements = new DungeonPlaceableRoomMaterialRequirement[0]
                        }
                    }
                }
            });
            RoomFactory.RoomData roomData = new RoomFactory.RoomData
            {
                room = protoroom,
                isSpecialRoom = true,
                category = "SPECIAL",
                specialSubCategory = "WEIRD_SHOP"
            };
            RoomFactory.rooms.Add(shop.name, roomData);
            DungeonHandler.Register(roomData);
        }


        public static AIAnimator GenerateBlankAIAnimator(GameObject targetObject)
        {
            AIAnimator aianimator = targetObject.AddComponent<AIAnimator>();
            aianimator.facingType = AIAnimator.FacingType.Default;
            aianimator.faceSouthWhenStopped = false;
            aianimator.faceTargetWhenStopped = false;
            aianimator.AnimatedFacingDirection = -90f;
            aianimator.directionalType = AIAnimator.DirectionalType.Sprite;
            aianimator.RotationQuantizeTo = 0f;
            aianimator.RotationOffset = 0f;
            aianimator.ForceKillVfxOnPreDeath = false;
            aianimator.SuppressAnimatorFallback = false;
            aianimator.IsBodySprite = true;
            aianimator.IdleAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
            aianimator.MoveAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
            aianimator.FlightAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
            aianimator.HitAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
            aianimator.TalkAnimation = new DirectionalAnimation
            {
                Type = DirectionalAnimation.DirectionType.None,
                Prefix = string.Empty,
                AnimNames = new string[0],
                Flipped = new DirectionalAnimation.FlipType[0]
            };
            aianimator.OtherAnimations = new List<AIAnimator.NamedDirectionalAnimation>(0);
            aianimator.OtherVFX = new List<AIAnimator.NamedVFXPool>(0);
            aianimator.OtherScreenShake = new List<AIAnimator.NamedScreenShake>(0);
            aianimator.IdleFidgetAnimations = new List<DirectionalAnimation>(0);
            aianimator.HitReactChance = 1f;
            aianimator.HitType = AIAnimator.HitStateType.Basic;
            return aianimator;
        }

        public static tk2dSpriteAnimationClip AddAnimation(tk2dSpriteAnimator animator, tk2dSpriteCollectionData collection, List<int> spriteIDs,
            string clipName, tk2dSpriteAnimationClip.WrapMode wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop, float fps = 15)
        {
            var clip = Shared.CreateAnimation(collection, spriteIDs, clipName, wrapMode, fps);

            if (animator.Library == null)
            {
                animator.Library = animator.gameObject.AddComponent<tk2dSpriteAnimation>();
                animator.Library.clips = new tk2dSpriteAnimationClip[0];
                animator.Library.enabled = true;
            }

            Array.Resize(ref animator.Library.clips, animator.Library.clips.Length + 1);
            animator.Library.clips[animator.Library.clips.Length - 1] = clip;

            return clip;
        }

        public static SpeculativeRigidbody GenerateOrAddToRigidBody(GameObject targetObject, CollisionLayer collisionLayer, PixelCollider.PixelColliderGeneration colliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon, bool collideWithTileMap = false, bool CollideWithOthers = true, bool CanBeCarried = true, bool CanBePushed = false, bool RecheckTriggers = false, bool IsTrigger = false, bool replaceExistingColliders = false, bool UsesPixelsAsUnitSize = false, IntVector2? dimensions = null, IntVector2? offset = null)
        {
            SpeculativeRigidbody orAddComponent = targetObject.GetOrAddComponent<SpeculativeRigidbody>();
            orAddComponent.CollideWithOthers = CollideWithOthers;
            orAddComponent.CollideWithTileMap = collideWithTileMap;
            orAddComponent.Velocity = Vector2.zero;
            orAddComponent.MaxVelocity = Vector2.zero;
            orAddComponent.ForceAlwaysUpdate = false;
            orAddComponent.CanPush = false;
            orAddComponent.CanBePushed = CanBePushed;
            orAddComponent.PushSpeedModifier = 1f;
            orAddComponent.CanCarry = false;
            orAddComponent.CanBeCarried = CanBeCarried;
            orAddComponent.PreventPiercing = false;
            orAddComponent.SkipEmptyColliders = false;
            orAddComponent.RecheckTriggers = RecheckTriggers;
            orAddComponent.UpdateCollidersOnRotation = false;
            orAddComponent.UpdateCollidersOnScale = false;
            IntVector2 intVector = IntVector2.Zero;
            IntVector2 intVector2 = IntVector2.Zero;
            if (colliderGenerationMode != PixelCollider.PixelColliderGeneration.Tk2dPolygon)
            {
                if (dimensions != null)
                {
                    intVector2 = dimensions.Value;
                    if (!UsesPixelsAsUnitSize)
                    {
                        intVector2 = new IntVector2(intVector2.x * 16, intVector2.y * 16);
                    }
                }
                if (offset != null)
                {
                    intVector = offset.Value;
                    if (!UsesPixelsAsUnitSize)
                    {
                        intVector = new IntVector2(intVector.x * 16, intVector.y * 16);
                    }
                }
            }
            PixelCollider item = Shared.SetupCollider(collisionLayer, intVector, intVector2, isTrigger: IsTrigger, mode: colliderGenerationMode);
            if (replaceExistingColliders || orAddComponent.PixelColliders == null)
                orAddComponent.PixelColliders = new List<PixelCollider> { item };
            else
                orAddComponent.PixelColliders.Add(item);

            if (orAddComponent.sprite && colliderGenerationMode == PixelCollider.PixelColliderGeneration.Tk2dPolygon)
            {
                Bounds bounds = orAddComponent.sprite.GetBounds();
                orAddComponent.sprite.GetTrueCurrentSpriteDef().colliderVertices = new Vector3[]
                {
                    bounds.center - bounds.extents,
                    bounds.center + bounds.extents
                };
            }
            return orAddComponent;
        }

    }
}
