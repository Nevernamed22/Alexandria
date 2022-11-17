using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Alexandria.CharacterAPI
{
    [Serializable]
    public class CustomCharacterData
    {

        public PlayableCharacters baseCharacter = PlayableCharacters.Pilot;
        public PlayableCharacters identity;
        public string name, nameShort, nickname, nameInternal, pathForSprites, pathForAltSprites;
        public Dictionary<PlayerStats.StatType, float> stats;
        public List<Texture2D> sprites, altSprites, foyerCardSprites, punchoutFaceCards, loadoutSprites;
        public List<string> loadoutSpriteNames = new List<string>();
        public Texture2D altPlayerSheet, playerSheet, minimapIcon, junkanWinPic, pastWinPic, altObjSprite1, altObjSprite2, coopDeathScreenIcon;
        public Texture2D faceCard;
        public List<Texture2D> bossCard = new List<Texture2D>();
        public Dictionary<string, Texture2D> punchoutSprites;
        public List<Tuple<PickupObject, bool>> loadout, altGun;
        public int characterID, metaCost;
        public float health = 3, armor = 0;
        public tk2dSpriteAnimation AlternateCostumeLibrary;
        public bool removeFoyerExtras;
        public bool useGlow, hasPast;
        public Color emissiveColor;
        public float emissiveColorPower, emissivePower, emissiveThresholdSensitivity;

        public Color altEmissiveColor;
        public float altEmissiveColorPower, altEmissivePower;

        public Func<PlayerController, float> coopBlankReplacement;

        public Material glowMaterial, altGlowMaterial, normalMaterial;

        //public CustomCharacterController customCharacterController;

        public Vector3 skinSwapperPos;
        public Vector3 foyerPos;

        public DungeonPrerequisite[] prerequisites = new DungeonPrerequisite[0];
        
        public tk2dSpriteAnimation libary;      
        public tk2dSpriteAnimator animator;
        public tk2dSpriteCollectionData collection;

        public tk2dSpriteAnimation altLibary;
        public tk2dSpriteAnimator altAnimator;
        public tk2dSpriteCollectionData altCollection;

        public CharacterSelectIdleDoer idleDoer;

        public List<Tuple<GameObject, Vector3>> randomFoyerBullshitNNAskedFor = new List<Tuple<GameObject, Vector3>>();
    }

    class CustomCharacterFoyerController : MonoBehaviour
    {
        public int metaCost;
        public bool useGlow;
        public Color emissiveColor;
        public float emissiveColorPower, emissivePower;
        public CustomCharacterData data;

    }

    public class GlowMatDoer
    {
        public GlowMatDoer(Color emissiveColor, float emissiveColorPower, float emissivePower)
        {
            this.emissiveColor = emissiveColor;
            this.emissiveColorPower = emissiveColorPower;
            this.emissivePower = emissivePower;
            this.emissiveThresholdSensitivity = 0.5f;          
        }

        public GlowMatDoer(Color emissiveColor, float emissiveColorPower, float emissivePower, float emissiveThresholdSensitivity)
        {
            this.emissiveColor = emissiveColor;
            this.emissiveColorPower = emissiveColorPower;
            this.emissivePower = emissivePower;
            this.emissiveThresholdSensitivity = emissiveThresholdSensitivity;
        }

        public Color emissiveColor;
        public float emissiveColorPower, emissivePower, emissiveThresholdSensitivity;
    }

    public class CustomCharacter : MonoBehaviour
    {
        [SerializeField]
        public CustomCharacterData data;
        private bool checkedGuns = false;
        public bool failedToFindData = false;
        private List<int> infiniteGunIDs = new List<int>();
       
        public string past, overrideAnimation;
        public bool hasPast;

        public static Dictionary<string, int> punchoutBullShit = new Dictionary<string, int>();
        

        void Start()
        {
            GetData();
            GameManager.Instance.OnNewLevelFullyLoaded += StartInfiniteGunCheck;
            if (!GameManager.Instance.IsFoyer)
            {
                StartInfiniteGunCheck();
            }
        }

        public bool GetData()
        {
            try
            {
                var gameobjName = this.gameObject.name.ToLower().Replace("(clone)", "").Trim();
                foreach (var cc in CharacterBuilder.storedCharacters.Keys)
                {
                    if (cc.ToLower().Equals(gameobjName))
                        data = CharacterBuilder.storedCharacters[cc].First;
                }
            }
            catch
            {
                failedToFindData = true;
                return !failedToFindData;
            }
            if (data == null) failedToFindData = true;
            return !failedToFindData;
        }

        public void StartInfiniteGunCheck()
        {
            StartCoroutine("CheckInfiniteGuns");
        }

        public IEnumerator CheckInfiniteGuns()
        {
            while (!checkedGuns)
            {
                if (ToolsCharApi.EnableDebugLogging == true)
                {
                    ToolsCharApi.Print("    Data check");
                }

                if (data == null)
                {
                    ToolsCharApi.PrintError("Couldn't find a character data object for this player!");
                    yield return new WaitForSeconds(.1f);
                }

                if (ToolsCharApi.EnableDebugLogging == true)
                {
                    ToolsCharApi.Print("    Loadout check");
                }
                var loadout = data.loadout;
                if (loadout == null)
                {
                    checkedGuns = true;
                    yield break;
                }

                var player = GetComponent<PlayerController>();
                if (player?.inventory?.AllGuns == null)
                {
                    ToolsCharApi.PrintError("Player or inventory not found");
                    yield return new WaitForSeconds(.1f);
                }
                if (ToolsCharApi.EnableDebugLogging == true)
                {
                    ToolsCharApi.Print($"Doing infinite gun check on {player.name}");

                }

                this.infiniteGunIDs = GetInfiniteGunIDs();
                if (ToolsCharApi.EnableDebugLogging == true)
                {
                    ToolsCharApi.Print("    Gun check");
                }
                foreach (var gun in player.inventory.AllGuns)
                {
                    if (infiniteGunIDs.Contains(gun.PickupObjectId))
                    {
                        if (!Hooks.gunBackups.ContainsKey(gun.PickupObjectId))
                        {
                            var backup = new Hooks.GunBackupData()
                            {
                                InfiniteAmmo = gun.InfiniteAmmo,
                                PreventStartingOwnerFromDropping = gun.PreventStartingOwnerFromDropping,
                                CanBeDropped = gun.CanBeDropped,
                                PersistsOnDeath = gun.PersistsOnDeath
                            };
                            Hooks.gunBackups.Add(gun.PickupObjectId, backup);
                            var prefab = PickupObjectDatabase.GetById(gun.PickupObjectId) as Gun;
                            prefab.InfiniteAmmo = true;
                            prefab.PersistsOnDeath = true;
                            prefab.CanBeDropped = false;
                            prefab.PreventStartingOwnerFromDropping = true;
                        }

                        gun.InfiniteAmmo = true;
                        gun.PersistsOnDeath = true;
                        gun.CanBeDropped = false;
                        gun.PreventStartingOwnerFromDropping = true;
                        ToolsCharApi.Print($"        {gun.name} is infinite now.");
                    }
                }
                checkedGuns = true;
                yield break;
            }
        }

        public List<int> GetInfiniteGunIDs()
        {
            var infiniteGunIDs = new List<int>();
            if (data == null) GetData();
            if (data == null || failedToFindData) return infiniteGunIDs;
            foreach (var item in data.loadout)
            {
                var g = item?.First?.GetComponent<Gun>();
                if (g && item.Second)
                    infiniteGunIDs.Add(g.PickupObjectId);
            }
            return infiniteGunIDs;
        }

        //This handles the dueling laser problem
        void FixedUpdate()
        {
            if (GameManager.Instance.IsLoadingLevel || GameManager.Instance.IsPaused) return;
            if (data == null) return;

            foreach (var gun in GetComponent<PlayerController>().inventory.AllGuns)
            {
                if (gun.InfiniteAmmo && infiniteGunIDs.Contains(gun.PickupObjectId))
                {
                    gun.ammo = gun.AdjustedMaxAmmo;
                    gun.RequiresFundsToShoot = false;

                    if (gun.UsesRechargeLikeActiveItem)
                    {
                        if (gun.RemainingActiveCooldownAmount > 0)
                        {
                            gun.RemainingActiveCooldownAmount = Mathf.Max(0f, gun.RemainingActiveCooldownAmount - 25f * BraveTime.DeltaTime);
                        }
                    }

                }
            }
        }

        void OnDestroy()
        {
            GameManager.Instance.OnNewLevelFullyLoaded -= StartInfiniteGunCheck;
        }
    }
}
