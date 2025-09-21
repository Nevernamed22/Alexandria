using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;

using HutongCharacter = HutongGames.PlayMaker.Actions.ChangeToNewCharacter;

using Alexandria.ItemAPI;
using Dungeonator;
using UnityEngine.SceneManagement;
using Alexandria.PrefabAPI;
//using PrefabAPI;

namespace Alexandria.CharacterAPI

{
    public static class FoyerCharacterHandler
    {
        private static bool hasInitialized = false;

        public static void Init() { }

        public static List<FoyerCharacterSelectFlag> AddCustomCharactersToFoyer(List<FoyerCharacterSelectFlag> sortedByX)
        {
            if (!hasInitialized)
            {
                Init();
                hasInitialized = true;
            }

            List<FoyerCharacterSelectFlag> list = new List<FoyerCharacterSelectFlag>();

            foreach (var character in CharacterBuilder.storedCharacters)
            {
                try
                {
                    //DebugUtility.Print($"Adding {character.Key} to the breach.");
                    var identity = character.Value.First.baseCharacter;

                    var selectCharacter = AddCharacterToFoyer(character.Key, GetFlagFromIdentity(identity, sortedByX).gameObject);

                    if (selectCharacter.PrerequisitesFulfilled())
                    {
                        list.Add(selectCharacter);
                        Foyer.Instance.OnPlayerCharacterChanged += selectCharacter.OnSelectedCharacterCallback;
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(selectCharacter.gameObject);
                    }
                }
                catch (Exception e)
                {
                    ToolsCharApi.PrintError($"An error occured while adding character {character.Key} to the breach.");
                    ToolsCharApi.PrintException(e);
                }
            }
            
            foreach(var flag in sortedByX)
            {
                ResetToIdle(flag);
            }

            return list;

        }

        public static FoyerCharacterSelectFlag GetFlagFromIdentity(PlayableCharacters character, List<FoyerCharacterSelectFlag> sortedByX)
        {
            string path;
            foreach (var flag in sortedByX)
            {
                path = flag.CharacterPrefabPath.ToLower();
                if (character == PlayableCharacters.Eevee && flag.IsEevee) return flag;
                if (character == PlayableCharacters.Gunslinger && flag.IsGunslinger) return flag;

                if (character == PlayableCharacters.Bullet && path.Contains("bullet")) return flag;
                if (character == PlayableCharacters.Convict && path.Contains("convict")) return flag;
                if (character == PlayableCharacters.Guide && path.Contains("guide")) return flag;
                if (character == PlayableCharacters.Soldier && path.Contains("marine")) return flag;
                if (character == PlayableCharacters.Robot && path.Contains("robot")) return flag;
                if (character == PlayableCharacters.Pilot && path.Contains("rogue")) return flag;
            }
            //DebugUtility.PrintError("Couldn't find foyer select flag for: " + character);
            //DebugUtility.PrintError("    Have you unlocked them yet?");
            return sortedByX[1];
        }

        private static FoyerCharacterSelectFlag AddCharacterToFoyer(string characterPath, GameObject selectFlagPrefab)
        {
            //Gather character data
            var customCharacter = CharacterBuilder.storedCharacters[characterPath.ToLower()];


            //if (!CheckUnlocked(customCharacter.First))
            //{
            //    return null;
            //}

            //DebugUtility.Print("    Got custom character");

            //Create new object



            FoyerCharacterSelectFlag selectFlag = GameObject.Instantiate(selectFlagPrefab).GetComponent<FoyerCharacterSelectFlag>();

            selectFlag.prerequisites = customCharacter.First.prerequisites;

            FakePrefab.MarkAsFakePrefab(selectFlag.gameObject);
            SceneManager.MoveGameObjectToScene(selectFlag.gameObject, SceneManager.GetActiveScene());

            selectFlag.transform.position = customCharacter.First.foyerPos;
            selectFlag.CharacterPrefabPath = characterPath;
            selectFlag.name = "NPC_FoyerCharacter_" + customCharacter.First.nameShort;
            //DebugUtility.Print("    Made select flag");

            //Replace sprites
            HandleSprites(selectFlag, customCharacter.Second.GetComponent<PlayerController>());
            //DebugUtility.Print("    Replaced sprites");

            var td = selectFlag.talkDoer;

            GameObject groundThingHandler = new GameObject($"{customCharacter.First.nameShort}GroundThingHandler");

            groundThingHandler.transform.position = customCharacter.First.foyerPos;

            //Setup overhead card
            if (!string.IsNullOrEmpty(customCharacter.First.pathForSprites))
            {
                var idleDoer = selectFlag.gameObject.GetComponent<CharacterSelectIdleDoer>();

                idleDoer.AnimationLibraries = customCharacter.First.idleDoer.AnimationLibraries;
                idleDoer.coreIdleAnimation = customCharacter.First.idleDoer.coreIdleAnimation;
                idleDoer.onSelectedAnimation = customCharacter.First.idleDoer.onSelectedAnimation;
                idleDoer.EeveeTex = customCharacter.First.idleDoer.EeveeTex;
                idleDoer.idleMax = customCharacter.First.idleDoer.idleMax;
                idleDoer.idleMin = customCharacter.First.idleDoer.idleMin;
                idleDoer.IsEevee = customCharacter.First.idleDoer.IsEevee;
                idleDoer.phases = customCharacter.First.idleDoer.phases;
                
            }

            selectFlag.gameObject.GetComponent<tk2dSpriteAnimator>().DefaultClipId = customCharacter.Second.GetComponent<PlayerController>().spriteAnimator.GetClipIdByName("select_idle");

            if (customCharacter.First.removeFoyerExtras)
            {
                foreach (var child in selectFlag.gameObject.transform)
                {
                    //wow look i did a peta and killed a dog for no reason
                    if (((Transform)child).gameObject.name == "Doggy")
                    {
                        UnityEngine.Object.DestroyImmediate(((Transform)child).gameObject);
                    }
                }
                foreach (var phase in selectFlag.gameObject.GetComponent<CharacterSelectIdleDoer>().phases)
                {
                    phase.vfxTrigger = CharacterSelectIdlePhase.VFXPhaseTrigger.NONE;
                    phase.endVFXSpriteAnimator = null;
                }
            }

            foreach (var thing in customCharacter.First.randomFoyerBullshitNNAskedFor)
            {

                UnityEngine.Object.Instantiate<GameObject>(thing.First, Foyer.Instance.transform.Find("Livery xform")).transform.position = customCharacter.First.foyerPos + thing.Second;
            }

            CreateOverheadCard(selectFlag, customCharacter.First);
            //FakePrefab.MarkAsFakePrefab(selectFlag.OverheadElement);
            //ETGModConsole.Log(selectFlag.OverheadElement.ToString());
            td.OverheadUIElementOnPreInteract = selectFlag.OverheadElement;
            //FakePrefab.MarkAsFakePrefab(td.OverheadUIElementOnPreInteract);
            //DebugUtility.Print("    Made Overhead Card");

            //Change the effect of talking to the character
            foreach (var state in selectFlag.playmakerFsm.Fsm.FsmComponent.FsmStates)
            {
                foreach (var action in state.Actions)
                {
                    if (action is HutongCharacter)
                    {
                        ((HutongCharacter)action).PlayerPrefabPath = characterPath;
                    }
                }
            }

            if (customCharacter.First.altObjSprite1 != null && customCharacter.First.altObjSprite2 != null)
                MakeSkinSwapper(customCharacter.First);
            //DebugUtility.Print("    Added swapper");

            //Make interactable
            if (!Dungeonator.RoomHandler.unassignedInteractableObjects.Contains(td))
                Dungeonator.RoomHandler.unassignedInteractableObjects.Add(td);
            //DebugUtility.Print("    Adjusted Talk-Doer");

            //Player changed callback - Hides and shows player select object
            Foyer.Instance.OnPlayerCharacterChanged += (player) =>
            {
                OnPlayerCharacterChanged(player, selectFlag, characterPath);
            };
            //DebugUtility.Print("    Added callback");

            return selectFlag;
        }

        
        private static void HandleSprites(BraveBehaviour selectCharacter, BraveBehaviour player)
        {


            selectCharacter.spriteAnimator.Library = player.spriteAnimator.Library;
            selectCharacter.sprite.Collection = player.sprite.Collection;


            selectCharacter.renderer.material = new Material(selectCharacter.renderer.material);


            //BotsMod.BotsModule.Log($"{selectCharacter.spriteAnimator.gameObject}");
            
            selectCharacter.sprite.ForceBuild();
            string coreIdleAnimation = selectCharacter.GetComponent<CharacterSelectIdleDoer>().coreIdleAnimation;
            selectCharacter.spriteAnimator.Play(coreIdleAnimation);
        }

        static void MakeSkinSwapper(CustomCharacterData data)
        {
            var baseSwapper = FakePrefab.Clone(Foyer.Instance.transform.Find("Livery xform").Find("costume_guide").gameObject);
            var altSwapper = FakePrefab.Clone(Foyer.Instance.transform.Find("Livery xform").Find("costume_guide_alt").gameObject);

            var sprite = baseSwapper.GetComponent<tk2dSprite>();
            var altSprite = altSwapper.GetComponent<tk2dSprite>();

            baseSwapper.transform.parent = Foyer.Instance.transform.Find("Livery xform");
            altSwapper.transform.parent = Foyer.Instance.transform.Find("Livery xform");

            sprite.SetSprite(sprite.Collection, SpriteHandler.AddSpriteToCollection(data.altObjSprite1, sprite.Collection));
            altSprite.SetSprite(altSprite.Collection, SpriteHandler.AddSpriteToCollection(data.altObjSprite2, sprite.Collection));

            altSwapper.name = $"costume_{data.nameShort}_alt";

            baseSwapper.name = $"costume_{data.nameShort}";

            var characterCostumeSwapper = baseSwapper.GetComponent<CharacterCostumeSwapper>();

            characterCostumeSwapper.TargetCharacter = data.identity;

            characterCostumeSwapper.AlternateCostumeSprite = altSprite;

            characterCostumeSwapper.CostumeSprite = sprite;

            characterCostumeSwapper.HasCustomTrigger = false;
            characterCostumeSwapper.CustomTriggerIsFlag = false;
            characterCostumeSwapper.TriggerFlag = GungeonFlags.NONE;
            characterCostumeSwapper.CustomTriggerIsSpecialReserve = false;

            characterCostumeSwapper.TargetLibrary = data.AlternateCostumeLibrary;

            if (sprite.transform == null)
            {
                ETGModConsole.Log("somehow the transform nulled... god is fucking dead and BraveBehaviours killed him");
            }

            if (altSprite.transform == null)
            {
                ETGModConsole.Log("somehow the transform nulled... god is fucking dead (again) and BraveBehaviours killed him");
            }

            baseSwapper.gameObject.SetActive(true);
            altSwapper.gameObject.SetActive(true);

            baseSwapper.transform.position = data.skinSwapperPos;

            altSwapper.transform.position = data.skinSwapperPos;
            

            //BotsMod.BotsModule.Log($"{baseSwapper.name}: {baseSwapper.transform.position}");

            if (!RoomHandler.unassignedInteractableObjects.Contains(baseSwapper.GetComponent<IPlayerInteractable>()))
            {
                RoomHandler.unassignedInteractableObjects.Add(baseSwapper.GetComponent<IPlayerInteractable>());
            }

            if (!RoomHandler.unassignedInteractableObjects.Contains(altSwapper.GetComponent<IPlayerInteractable>()))
            {
                RoomHandler.unassignedInteractableObjects.Add(altSwapper.GetComponent<IPlayerInteractable>());
            }
        }

        private static void ResetToIdle(BraveBehaviour idler)
        {
            SpriteOutlineManager.RemoveOutlineFromSprite(idler.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(idler.sprite, Color.black);

            //var idle = idler.GetComponent<CharacterSelectIdleDoer>().coreIdleAnimation;
            //idler.sprite.SetSprite(idler.spriteAnimator.GetClipByName(idle).frames[0].spriteId);
            //idler.talkDoer.OnExitRange(null); 
        }

        private static readonly Dictionary<CustomCharacterData, GameObject> _CachedOverheadPrefabs = new();
        private static readonly List<int> _ToCopyAppearAnimIds = new List<int> { 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, };
        private static readonly List<int> _ToCopyIdleAnimIds = new List<int> { 241, 242, 243, 244, };
        private static readonly Vector3 _BasegameFacecardPosition = new Vector3(0, 1.687546f, 0.2250061f);
        private static readonly float _FacecardScaleFactor = 7f; // magic number determined through experimentation

        private static void CreateOverheadCard(FoyerCharacterSelectFlag selectCharacter, CustomCharacterData data)
        {
            try
            {
                if (selectCharacter.OverheadElement == null)
                {
                    if (ToolsCharApi.EnableDebugLogging == true)
                        ETGModConsole.Log($"CHR_{data.nameShort}Panel is null");
                    return;
                }

                if (selectCharacter.OverheadElement?.name == $"CHR_{data.nameShort}Panel")
                {
                    if (ToolsCharApi.EnableDebugLogging == true)
                        ETGModConsole.Log($"CHR_{data.nameShort}Panel already exists");
                    return;
                }

                selectCharacter.ClearOverheadElement();
                if (_CachedOverheadPrefabs.TryGetValue(data, out GameObject overheadPrefab))
                {
                    selectCharacter.OverheadElement = overheadPrefab;
                    return;
                }

                //Create new card instance
                GameObject newOverheadElement = UnityEngine.Object.Instantiate(selectCharacter.OverheadElement);
                GameObject.DontDestroyOnLoad(newOverheadElement);
                newOverheadElement.SetActive(false);
                newOverheadElement.name = $"CHR_{data.nameShort}Panel";
                newOverheadElement.GetComponent<FoyerInfoPanelController>().followTransform = selectCharacter.transform; //NOTE: verify this is correct after instantiation

                // Custom foyer controller setup
                var customFoyerController = selectCharacter.gameObject.AddComponent<CustomCharacterFoyerController>();
                customFoyerController.metaCost = data.metaCost;
                customFoyerController.useGlow = data.useGlow;
                customFoyerController.emissiveColor = data.emissiveColor;
                customFoyerController.emissiveColorPower = data.emissiveColorPower;
                customFoyerController.emissivePower = data.emissivePower;
                customFoyerController.data = data;

                //Change text
                var infoPanel = newOverheadElement.GetComponent<FoyerInfoPanelController>();
                dfLabel nameLabel = infoPanel.textPanel.transform.Find("NameLabel").GetComponent<dfLabel>();
                nameLabel.Text = "#CHAR_" + data.nameShort.ToString().ToUpper();
                if (data.metaCost != 0)
                {
                    dfLabel pastKilledLabel = infoPanel.textPanel.transform.Find("PastKilledLabel").GetComponent<dfLabel>();
                    pastKilledLabel.ProcessMarkup = true;
                    pastKilledLabel.ColorizeSymbols = true;
                    pastKilledLabel.ModifyLocalizedText(pastKilledLabel.Text + " (" + data.metaCost.ToString() + "[sprite \"hbux_text_icon\"])");
                    pastKilledLabel.ModifyLocalizedText("(Past Killed)" + " (" + data.metaCost.ToString() + "[sprite \"hbux_text_icon\"])");
                }

                // Loadout setup
                var spriteObject = FakePrefab.Clone(infoPanel.itemsPanel.GetComponentInChildren<dfSprite>().gameObject);
                foreach (var child in infoPanel.itemsPanel.GetComponentsInChildren<dfSprite>())
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                dfAtlas uiAtlas = GameUIRoot.Instance.ConversationBar.portraitSprite.Atlas;
                for (int i = 0; i < data.loadoutSpriteNames.Count; i++)
                {
                    var sprite = FakePrefab.Clone(spriteObject).GetComponent<dfSprite>();
                    sprite.SpriteName = data.loadoutSpriteNames[i];
                    sprite.Size = new Vector2(data.loadoutSprites[i].width * 3, data.loadoutSprites[i].height * 3);
                    sprite.Atlas = uiAtlas;
                    sprite.transform.parent = infoPanel.itemsPanel.transform;
                    sprite.transform.localPosition = new Vector3(((i + 0.1f) * 0.1f), 0, 0);
                    infoPanel.itemsPanel.Controls.Add(sprite);
                }
                
                // Facecard setup
                if (data.foyerCardSprites != null)
                {
                    CharacterSelectFacecardIdleDoer facecard = newOverheadElement.GetComponentInChildren<CharacterSelectFacecardIdleDoer>();
                    facecard.gameObject.name = data.nameShort + " Sprite FaceCard";// <---------------- this object needs to be shrank
                    facecard.spriteAnimator = facecard.gameObject.GetComponent<tk2dSpriteAnimator>();
                    facecard.transform.localPosition = _BasegameFacecardPosition;
                    facecard.transform.parent.localPosition = Vector3.zero;
                    facecard.transform.parent.localScale *= _FacecardScaleFactor;

                    if (ToolsCharApi.EnableDebugLogging == true)
                    {
                        Debug.Log($"foyer cards arent null. {facecard.gameObject.transform.parent.position}");
                        Debug.Log($"foyer cards arent null. {facecard.gameObject.activeSelf}");
                    }

                    var orig = facecard.sprite.Collection;
                    var idleAnimName = $"{data.nameShort}_facecard_idle";
                    var appearAnimName = $"{data.nameShort}_facecard_appear";
                    List<int> idleAnimIds = new List<int>();
                    List<int> appearAnimIds = new List<int>();

                    foreach (var sprite in data.foyerCardSprites)
                    {
                        if (sprite.name.ToLower().Contains("appear"))
                            appearAnimIds.Add(SpriteHandler.AddSpriteToCollectionWithAnchor(sprite, orig, tk2dBaseSprite.Anchor.LowerCenter, $"{data.nameShort}_{sprite.name}"));
                        else if (sprite.name.ToLower().Contains("idle"))
                            idleAnimIds.Add(SpriteHandler.AddSpriteToCollectionWithAnchor(sprite, orig, tk2dBaseSprite.Anchor.LowerCenter, $"{data.nameShort}_{sprite.name}"));
                    }
                    if (ToolsCharApi.EnableDebugLogging == true)
                        Debug.Log($"anchors done");

                    var oDefs = orig.spriteDefinitions;
                    for (int i = 0; i < appearAnimIds.Count; i++)
                    {
                        bool invalid = (i >= _ToCopyAppearAnimIds.Count || oDefs[_ToCopyAppearAnimIds[i]] == null);
                        var def = oDefs[appearAnimIds[i]];
                        var defToCopy = oDefs[_ToCopyAppearAnimIds[invalid ? 9 : i]];
                        def.position0 = defToCopy.position0;
                        def.position1 = defToCopy.position1;
                        def.position2 = defToCopy.position2;
                        def.position3 = defToCopy.position3;
                    }
                    if (ToolsCharApi.EnableDebugLogging == true)
                        Debug.Log($"appearAnimIds position0-3 done");

                    for (int i = 0; i < idleAnimIds.Count; i++)
                    {
                        bool invalid = (i >= _ToCopyIdleAnimIds.Count || oDefs[_ToCopyIdleAnimIds[i]] == null);
                        var def = oDefs[idleAnimIds[i]];
                        var defToCopy = oDefs[_ToCopyIdleAnimIds[invalid ? 3 : i]];
                        def.position0 = defToCopy.position0;
                        def.position1 = defToCopy.position1;
                        def.position2 = defToCopy.position2;
                        def.position3 = defToCopy.position3;
                    }
                    if (ToolsCharApi.EnableDebugLogging == true)
                        Debug.Log($"idleAnimIds position0-3 done");

                    // NOTE: this doesn't seem to be needed any more after caching the prefab...maybe it's handled automatically somewhere...idk
                    // int oldLength = infoPanel.scaledSprites.Length;
                    // Array.Resize(ref infoPanel.scaledSprites, oldLength + 1);
                    // infoPanel.scaledSprites[oldLength] = facecard.spriteAnimator.sprite.GetComponent<tk2dSprite>();

                    SpriteBuilder.AddAnimation(facecard.spriteAnimator, orig, idleAnimIds, idleAnimName, tk2dSpriteAnimationClip.WrapMode.Loop, 4);
                    SpriteBuilder.AddAnimation(facecard.spriteAnimator, orig, appearAnimIds, appearAnimName, tk2dSpriteAnimationClip.WrapMode.Once, 17);
                    if (ToolsCharApi.EnableDebugLogging == true)
                        Debug.Log($"anims added");
                    facecard.spriteAnimator.DefaultClipId = facecard.spriteAnimator.Library.GetClipIdByName(appearAnimName);
                    facecard.appearAnimation = appearAnimName;
                    facecard.coreIdleAnimation = idleAnimName;

                    if (ToolsCharApi.EnableDebugLogging == true)
                        Debug.Log($"foyer card done");

                    FakePrefab.MakeFakePrefab(newOverheadElement);
                    _CachedOverheadPrefabs[data] = newOverheadElement;
                    selectCharacter.OverheadElement = newOverheadElement;
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log("Overhead setup code broke: " + e);
            }
        }

        private static void OnPlayerCharacterChanged(PlayerController player, FoyerCharacterSelectFlag selectCharacter, string characterPath)
        {
            if (player.name.ToLower().Contains(characterPath))
            {
                //DebugUtility.Print("Selected: " + characterPath);
                if (selectCharacter.gameObject.activeSelf)
                {
                    selectCharacter.ClearOverheadElement();
                    selectCharacter.talkDoer.OnExitRange(null);
                    selectCharacter.gameObject.SetActive(false);
                    selectCharacter.GetComponent<SpeculativeRigidbody>().enabled = false;
                }
            }
            else if (!selectCharacter.gameObject.activeSelf)
            {
                selectCharacter.gameObject.SetActive(true);
                SpriteOutlineManager.RemoveOutlineFromSprite(selectCharacter.sprite, true);
                SpriteOutlineManager.AddOutlineToSprite(selectCharacter.sprite, Color.black);

                selectCharacter.specRigidbody.enabled = true;
                PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(selectCharacter.specRigidbody, null, false);

                CharacterSelectIdleDoer idleDoer = selectCharacter.GetComponent<CharacterSelectIdleDoer>();
                idleDoer.enabled = true;

            }
        }
    }
}
