using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using StatType = PlayerStats.StatType;
using UnityEngine;

using System.Reflection;
using Alexandria.ItemAPI;

namespace CustomCharacters
{
    /*
     * Loads all the character data from the characterdata.txt
     * and then ships it off to CharacterBuilder
     */
    public static class Loader
    {
        public static string DataFile = "characterdata.txt";

        public static List<CustomCharacterData> characterData = new List<CustomCharacterData>();


        public static List<PlayableCharacters> myPlayableCharacters = new List<PlayableCharacters>();

        /*public static void Init()
        {
            try
            {
                if (!Directory.Exists(CharacterDirectory))
                {
                    DirectoryInfo dir = Directory.CreateDirectory(CharacterDirectory);
                    ETGModConsole.Log("Created directory: " + dir.FullName);
                }
            }
            catch (Exception e)
            {
                ToolsCharApi.PrintError("Error creating custom character directory");
                ToolsCharApi.PrintException(e);
            }

            LoadCharacterData();
            foreach (CustomCharacterData data in characterData)
            {
                bool success = true;
                try
                {
                    CharacterBuilder.BuildCharacter(data);
                }
                catch (Exception e)
                {
                    success = false;
                    ToolsCharApi.PrintError("An error occured while creating the character: " + data.name);
                    ToolsCharApi.PrintException(e);
                }
                if (success)
                    ToolsCharApi.Print("Built prefab for: " + data.name);
            }
        }*/
        public static void AddPhase(string character, CharacterSelectIdlePhase phase)
        {
            character = character.ToLower();
            //BotsModule.Log(CharacterBuilder.storedCharacters.GetFirst().Key);
            if (CharacterBuilder.storedCharacters.ContainsKey(character))
            {
                var arraysSuckBalls = CharacterBuilder.storedCharacters[character].First.idleDoer.phases.ToList();

                arraysSuckBalls.Add(phase);

                CharacterBuilder.storedCharacters[character].First.idleDoer.phases = arraysSuckBalls.ToArray();
            }
            else
            {
                ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
            }
        }

        public static tk2dSpriteAnimationClip GetAnimation(string character, string animation, bool alt = false)
        {
            character = character.ToLower();
            if (CharacterBuilder.storedCharacters.ContainsKey(character))
            {
                var library = !alt ? CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().spriteAnimator.Library : CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().AlternateCostumeLibrary;
                if (library.GetClipByName(animation) != null)
                {
                    return library.GetClipByName(animation);
                }
            }
            else
            {
                ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
            }
            return null;
        }

         /*public static void AddFoyerObject(string character, GameObject obj, Vector2 offset, CustomDungeonFlags requiredFlag = CustomDungeonFlags.NONE)
         {
             character = character.ToLower();
             if (CharacterBuilder.storedCharacters.ContainsKey(character))
             {
                 var cc = CharacterBuilder.storedCharacters[character].First;
                 obj.layer = 22;
                 var flagDisabler = obj.AddComponent<CustomSimpleFlagDisabler>();
                 flagDisabler.FlagToCheckFor = requiredFlag;
                 flagDisabler.UseNumberOfAttempts = false;

                 CharacterBuilder.storedCharacters[character].First.randomFoyerBullshitNNAskedFor.Add(new Tuple<GameObject, Vector3>(obj, offset));
             }
             else
             {
                 ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
             }
         }
         public static void AddFoyerObject(string character, GameObject obj, Vector2 offset, CustomTrackedStats requiredStat, int amount)
         {
             character = character.ToLower();
             if (CharacterBuilder.storedCharacters.ContainsKey(character))
             {
                 var cc = CharacterBuilder.storedCharacters[character].First;
                 obj.layer = 22;
                 var flagDisabler = obj.AddComponent<CustomSimpleFlagDisabler>();
                 flagDisabler.UsesStatComparisonInstead = true;
                 flagDisabler.RelevantStat = requiredStat;
                 flagDisabler.minStatValue = amount;
                 flagDisabler.UseNumberOfAttempts = false;

                 CharacterBuilder.storedCharacters[character].First.randomFoyerBullshitNNAskedFor.Add(new Tuple<GameObject, Vector3>(obj, offset));
             }
             else
             {
                 ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
             }
         }*/
        public static void AddFoyerObject(string character, GameObject obj, Vector2 offset, int minRunCount = 2)
        {
            character = character.ToLower();
            if (CharacterBuilder.storedCharacters.ContainsKey(character))
            {
                var cc = CharacterBuilder.storedCharacters[character].First;
                obj.layer = 22;
                //var flagDisabler = obj.AddComponent<CustomSimpleFlagDisabler>();
                //flagDisabler.UseNumberOfAttempts = minRunCount >= 0;
                //flagDisabler.minStatValue = minRunCount;

                CharacterBuilder.storedCharacters[character].First.randomFoyerBullshitNNAskedFor.Add(new Tuple<GameObject, Vector3>(obj, offset));
            }
            else
            {
                ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
            }
        }


        public static void SetupCustomBreachAnimation(string character, string animation, int fps, tk2dSpriteAnimationClip.WrapMode wrapMode, int loopStart = 0, float maxFidgetDuration = 0, float minFidgetDuration = 0)
        {
            character = character.ToLower();
            //BotsModule.Log(CharacterBuilder.storedCharacters.GetFirst().Key);
            if (CharacterBuilder.storedCharacters.ContainsKey(character) && CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().spriteAnimator != null)
            {
                var library = CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().spriteAnimator.Library;
                if (library.GetClipByName(animation) != null)
                {
                    library.GetClipByName(animation).fps = fps;
                    library.GetClipByName(animation).wrapMode = wrapMode;
                    if (wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
                    {
                        library.GetClipByName(animation).loopStart = loopStart;
                    }
                    if (wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopFidget)
                    {
                        library.GetClipByName(animation).maxFidgetDuration = maxFidgetDuration;
                        library.GetClipByName(animation).minFidgetDuration = minFidgetDuration;
                    }
                }
                else
                {
                    ETGModConsole.Log($"No animation found under the name \"{animation}\"");
                }
            }
            else
            {
                ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
            }

        }


        public static void SetupCustomAnimation(string character, string animation, int fps, tk2dSpriteAnimationClip.WrapMode wrapMode, int loopStart = 0)
        {
            SetupCustomBreachAnimation(character, animation, fps, wrapMode, loopStart);
            /*character = character.ToLower();
            BotsModule.Log(CharacterBuilder.storedCharacters.GetFirst().Key);
            if (CharacterBuilder.storedCharacters.ContainsKey(character) && CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().spriteAnimator != null)
            {
                var library = CharacterBuilder.storedCharacters[character].Second.GetComponent<PlayerController>().spriteAnimator.Library;
                if (library.GetClipByName(animation) != null)
                {
                    library.GetClipByName(animation).fps = fps;
                    library.GetClipByName(animation).wrapMode = wrapMode;
                    if (wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
                    {
                        library.GetClipByName(animation).loopStart = loopStart;
                    }
                } 
                else
                {
                    ETGModConsole.Log($"No animation found under the name \"{animation}\"");
                }
                
            }
            else
            {
                ETGModConsole.Log($"No character found under the name \"{character}\" or tk2dSpriteAnimator is null");
            }*/

        }

        public static CustomCharacterData BuildCharacter(string filePath, string guid, Vector3 foyerPos, bool hasAltSkin, Vector3 altSwapperPos, bool removeFoyerExtras = true, bool hasArmourlessAnimations = false, bool usesArmourNotHealth = false, bool paradoxUsesSprites = true,
            bool useGlow = false, GlowMatDoer glowVars = null, GlowMatDoer altGlowVars = null, int metaCost = 0, bool hasCustomPast = false, string customPast = "")
        {
            //ETGModConsole.Log(BotsModule.FilePath);
            //ETGModConsole.Log(BotsModule.ZipFilePath);

            var data = GetCharacterData(filePath, Assembly.GetCallingAssembly());
            data.foyerPos = foyerPos;
            data.idleDoer = new CharacterSelectIdleDoer
            {
                onSelectedAnimation = "select_choose",
                coreIdleAnimation = "select_idle",
                idleMax = 10,
                idleMin = 4,
                EeveeTex = null,
                IsEevee = false,
                AnimationLibraries = new tk2dSpriteAnimation[0],
                phases = new CharacterSelectIdlePhase[0],

            };
           
           


            data.skinSwapperPos = altSwapperPos;

            data.identity = ETGModCompatibility.ExtendEnum<PlayableCharacters>(guid, data.nameShort);

            bool success = true;

            try
            {
                CharacterBuilder.BuildCharacter(data, hasAltSkin, paradoxUsesSprites, removeFoyerExtras, hasArmourlessAnimations, usesArmourNotHealth, hasCustomPast, customPast, metaCost, useGlow, glowVars, altGlowVars, Assembly.GetCallingAssembly());
                myPlayableCharacters.Add(data.identity);
            }
            catch (Exception e)
            {
                success = false;
                ToolsCharApi.PrintError("An error occured while creating the character: " + data.name);
                ToolsCharApi.PrintException(e);
            }


            if (success)
            {
                ToolsCharApi.Print("Built prefab for: " + data.name);
                return data;
            }
            return null;
        }


        public static void AddCoopBlankOverride(string character, Func<PlayerController, float> overrideMethod)
        {
            CharacterBuilder.storedCharacters[character.ToLower()].First.coopBlankReplacement = overrideMethod;
        }

        private static CustomCharacterData GetCharacterData(string filePath, Assembly assembly = null)
        {

            filePath = filePath.Replace("/", ".").Replace("\\", ".");

            ToolsCharApi.StartTimer("Loading data for " + Path.GetFileName(filePath));
            ToolsCharApi.Print("");
            ToolsCharApi.Print("--Loading " + Path.GetFileName(filePath) + "--", "0000FF");
            //string customCharacterDir = Path.Combine(CharacterDirectory, filePath).Replace("/", ".").Replace("\\", ".");
            string dataFilePath = Path.Combine(filePath, "characterdata.txt").Replace("/", ".").Replace("\\", ".");


            assembly = assembly ?? Assembly.GetCallingAssembly();
            var lines = new string[0];

            using (Stream stream = assembly.GetManifestResourceStream(dataFilePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                var linesList = new List<string>();
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    linesList.Add(line);
                }
                //ToolsCharApi.PrintError(linesList.Count().ToString());
                lines = linesList.ToArray();
            }



            if (lines.Count() <= 0)
            {
                ToolsCharApi.PrintError($"No \"{DataFile}\" file found for " + Path.GetFileName(filePath));
                return null;
            }
            

            //var lines = ToolsCharApi.GetLinesFromFile(dataFilePath);
            var data = ParseCharacterData(lines);

            string spritesDir = Path.Combine(filePath, "sprites").Replace("/", ".").Replace("\\", ".");
            string newSpritesDir = Path.Combine(filePath, "newspritesetup").Replace("/", ".").Replace("\\", ".");
            string newAltSpritesDir = Path.Combine(filePath, "newaltspritesetup").Replace("/", ".").Replace("\\", ".");
            string altSpritesDir = Path.Combine(filePath, "alt_sprites").Replace("/", ".").Replace("\\", ".");
            string loadoutDir = Path.Combine(filePath, "loadoutsprites").Replace("/", ".").Replace("\\", ".");
            string foyerDir = Path.Combine(filePath, "foyercard").Replace("/", ".").Replace("\\", ".");
            string punchoutDir = Path.Combine(filePath, "punchout").Replace("/", ".").Replace("\\", ".");
            string punchoutSpritesDir = Path.Combine(filePath, "punchout.sprites").Replace("/", ".").Replace("\\", ".");


            string[] resources = ToolsCharApi.GetResourceNames(assembly);
            
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].Contains(filePath))
                {

                    if (resources[i].StartsWith(spritesDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.sprites == null)
                    {
                        //ToolsCharApi.PrintError("Found: Sprites folder");
                        data.sprites = ToolsCharApi.GetTexturesFromResource(spritesDir, assembly);
                    }

                    

                    if (resources[i].StartsWith(altSpritesDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.altSprites == null)
                    {
                        //ToolsCharApi.PrintError("Found: Alt Sprites folder");
                        data.altSprites = ToolsCharApi.GetTexturesFromResource(altSpritesDir, assembly);
                    }

                    if (resources[i].StartsWith(newSpritesDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(data.pathForSprites))
                    {
                        //ToolsCharApi.PrintError("Found: New Sprites folder");
                        data.pathForSprites = newSpritesDir;
                    }

                    if (resources[i].StartsWith(newAltSpritesDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(data.pathForAltSprites))
                    {
                        //ToolsCharApi.PrintError("Found: New Sprites folder");
                        data.pathForAltSprites = newAltSpritesDir;
                    }

                    
                    if (resources[i].StartsWith(foyerDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.foyerCardSprites == null)
                    {
                        //ToolsCharApi.PrintError("Found: Foyer card folder");
                        data.foyerCardSprites = ToolsCharApi.GetTexturesFromResource(foyerDir, assembly);
                    }
                   

                    
                    if (resources[i].StartsWith(loadoutDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.loadoutSprites == null)
                    {
                        //ToolsCharApi.PrintError("Found: Loadout card folder");

                        data.loadoutSprites = ToolsCharApi.GetTexturesFromResource(loadoutDir, assembly);

                        //ToolsCharApi.PrintError(data.loadoutSprites.Count.ToString());
                    }
                   
                    
                    if (resources[i].StartsWith(punchoutSpritesDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.punchoutSprites == null)
                    {
                        ToolsCharApi.Print("Found: Punchout Sprites folder");
                        ETGModConsole.Log("Found: Punchout Sprites folder");
                        data.punchoutSprites = new Dictionary<string, Texture2D>();
                        foreach (var tex in ToolsCharApi.GetTexturesFromResource(punchoutSpritesDir, assembly))
                        {
                            data.punchoutSprites.Add(tex.name, tex);
                        }

                        
                    }

                    if (resources[i].StartsWith(punchoutDir.Replace('/', '.'), StringComparison.OrdinalIgnoreCase) && data.punchoutFaceCards == null)
                    {
                        data.punchoutFaceCards = new List<Texture2D>();
                        //ETGModConsole.Log(punchoutDir);
                        var punchoutSprites = ToolsCharApi.GetTexturesFromResource(punchoutDir, assembly);
                        foreach (var tex in punchoutSprites)
                        {
                            string name = tex.name.ToLower();
                            if (name.Contains("facecard1") || name.Contains("facecard2") || name.Contains("facecard3"))
                            {
                                data.punchoutFaceCards.Add(tex);
                                ToolsCharApi.Print("Found: Punchout facecard " + tex.name);
                            }
                        }
                    }

                }
            }


            //ToolsCharApi.PrintError("new sprites");

            //ToolsCharApi.PrintError("alt sprites");
            //ToolsCharApi.PrintError("foyer card sprites");

            //ToolsCharApi.PrintError("loadout sprites");
            List<Texture2D> miscTextures = ToolsCharApi.GetTexturesFromResource(filePath, assembly ?? Assembly.GetCallingAssembly());
            foreach (var tex in miscTextures)
            {
                string name = tex.name.ToLower();
                if (name.Equals("icon"))
                {
                    //ToolsCharApi.PrintError("Found: Icon ");
                    data.minimapIcon = tex;
                }
                if (name.Equals("coop_page_death"))
                {
                    //ToolsCharApi.PrintError("Found: Icon ");
                    data.coopDeathScreenIcon = tex;
                }
                if (name.Contains("bosscard_"))
                {
                    //ToolsCharApi.PrintError("Found: Bosscard");
                    //BotsModule.Log(name.ToLower().Replace("bosscard_", "").Replace("0", ""));
                    data.bossCard.Add(tex);
                }
                if (name.Equals("playersheet"))
                {
                    //ToolsCharApi.PrintError("Found: Playersheet");
                    data.playerSheet = tex;
                }
                if (name.Equals("facecard"))
                {
                    //ToolsCharApi.PrintError("Found: Facecard");
                    data.faceCard = tex;
                }
                if (name.Equals("win_pic_junkan"))
                {
                    //ToolsCharApi.PrintError("Found: Junkan Win Pic");
                    data.junkanWinPic = tex;
                }
                if (name.Equals("win_pic"))
                {
                    //ToolsCharApi.PrintError("Found: Past Win Pic");
                    data.pastWinPic = tex;
                }

                if (name.Equals("alt_skin_obj_sprite_001"))
                {
                    //ToolsCharApi.PrintError("Found: alt_skin_obj_sprite_001");
                    data.altObjSprite1 = tex;
                }
                if (name.Equals("alt_skin_obj_sprite_002"))
                {
                    //ToolsCharApi.PrintError("Found: alt_skin_obj_sprite_002");
                    data.altObjSprite2 = tex;
                }


            }
            //ToolsCharApi.PrintError("other sprites");
           
                
            //ToolsCharApi.StopTimerAndReport("Loading data for " + Path.GetFileName(directories[i]));

            return data;
            
        }       

        //Main parse loop
        public static CustomCharacterData ParseCharacterData(string[] lines)
        {
            CustomCharacterData data = new CustomCharacterData();
            
            for (int i = 0; i < lines.Length; i++)
            {
                
                string line = lines[i].ToLower().Trim();
                string lineCaseSensitive = lines[i].Trim();

                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("#")) continue;

                if (line.StartsWith("<loadout>"))
                {
                    data.loadout = GetLoadout(lines, i + 1, out i);
                    continue;
                }

                if (line.StartsWith("<altguns>"))
                {
                    //ToolsCharApi.PrintError("alt guns found");
                    data.altGun = GetAltguns(lines, i + 1, out i);
                    continue;
                }

                if (line.StartsWith("<stats>"))
                {
                    data.stats = GetStats(lines, i + 1, out i);
                    continue;
                }

                int dividerIndex = line.IndexOf(':');
                if (dividerIndex < 0) continue;


                string value = lineCaseSensitive.Substring(dividerIndex + 1).Trim();
                if (line.StartsWith("base:"))
                {
                    data.baseCharacter = GetCharacterFromString(value);
                    if (data.baseCharacter == PlayableCharacters.Robot)
                        data.armor = 6;
                    continue;
                }
                if (line.StartsWith("name:"))
                {
                    data.name = value;
                    continue;
                }


                /*if (line.StartsWith("unlock:"))
                {
                    data.unlockFlag = GetFlagFromString(value);
                    //BotsModule.Log(data.unlockFlag+"", BotsModule.LOST_COLOR);
                    continue;
                }*/


               
                if (line.StartsWith("name short:"))
                {
                    data.nameShort = value.Replace(" ", "_");
                    data.nameInternal = "Player" + data.nameShort;
                    continue;
                }
                if (line.StartsWith("nickname:"))
                {
                    data.nickname = value;
                    continue;
                }               
                if (line.StartsWith("armor:"))
                {
                    float floatValue;
                    if (!float.TryParse(value, out floatValue))
                    {
                        ToolsCharApi.PrintError("Invalid armor value: " + line);
                        continue;
                    }
                    data.armor = floatValue;
                    continue;
                }

                ToolsCharApi.PrintError($"Line {i} in {DataFile} did not meet any expected criteria:");
                ToolsCharApi.PrintRaw("----" + line, true);
            }
            return data;
        }

        //Character name aliasing
        public static PlayableCharacters GetCharacterFromString(string characterName)
        {
            characterName = characterName.ToLower();
            foreach (PlayableCharacters character in Enum.GetValues(typeof(PlayableCharacters)))
            {
                var name = character.ToString().ToLower().Replace("coop", "");
                if (name.Equals(characterName))
                {
                    return character;
                }
            }

            if (characterName.Equals("marine"))
                return PlayableCharacters.Soldier;
            if (characterName.Equals("hunter"))
                return PlayableCharacters.Guide;
            if (characterName.Equals("paradox"))
                return PlayableCharacters.Eevee;

            ToolsCharApi.Print("Failed to find character base: " + characterName);
            return PlayableCharacters.Pilot;
        }

        

        //Stats
        public static Dictionary<StatType, float> GetStats(string[] lines, int startIndex, out int endIndex)
        {
            endIndex = startIndex;

            Dictionary<PlayerStats.StatType, float> stats = new Dictionary<PlayerStats.StatType, float>();
            string line;
            string[] args;
            for (int i = startIndex; i < lines.Length; i++)
            {
                endIndex = i;
                line = lines[i].ToLower().Trim();
                if (line.StartsWith("</stats>")) return stats;

                args = line.Split(':');
                if (args.Length == 0) continue;
                if (string.IsNullOrEmpty(args[0])) continue;
                if (args.Length < 2)
                {
                    ToolsCharApi.PrintError("Invalid stat line: " + line);
                    continue;
                }

                StatType stat = StatType.Accuracy;
                bool foundStat = false;
                foreach (StatType statType in Enum.GetValues(typeof(StatType)))
                {
                    if (statType.ToString().ToLower().Equals(args[0].ToLower()))
                    {
                        stat = statType;
                        foundStat = true;
                        break;
                    }
                }
                if (!foundStat)
                {
                    ToolsCharApi.PrintError("Unable to find stat: " + line);
                    continue;
                }

                float value;
                bool foundValue = float.TryParse(args[1].Trim(), out value);
                if (!foundValue)
                {
                    ToolsCharApi.PrintError("Invalid stat value: " + line);
                    continue;
                }

                stats.Add(stat, value);
            }
            ToolsCharApi.PrintError("Invalid stats setup, expecting '</stats>' but found none");
            return new Dictionary<StatType, float>();
        }

        //Loadout
        public static List<Tuple<PickupObject, bool>> GetLoadout(string[] lines, int startIndex, out int endIndex)
        {
            endIndex = startIndex;

            ToolsCharApi.Print("Getting loadout...");
            List<Tuple<PickupObject, bool>> items = new List<Tuple<PickupObject, bool>>();

            string line;
            string[] args;
            for (int i = startIndex; i < lines.Length; i++)
            {
                endIndex = i;
                line = lines[i].ToLower().Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("</loadout>")) return items;

                args = line.Split(' ');
                if (args.Length == 0) continue;

                if (!Gungeon.Game.Items.ContainsID(args[0]))
                {
                    ToolsCharApi.PrintError("Could not find item with ID: \"" + args[0] + "\"");
                    continue;
                }
                var item = Gungeon.Game.Items[args[0]];
                if (item == null)
                {
                    ToolsCharApi.PrintError("Could not find item with ID: \"" + args[0] + "\"");
                    continue;
                }

                if (args.Length > 1 && args[1].Contains("infinite"))
                {
                    var gun = item.GetComponent<Gun>();

                    if (gun != null)
                    {
                        if (!CharacterBuilder.guns.Contains(gun) && !gun.InfiniteAmmo)
                            CharacterBuilder.guns.Add(gun);

                        items.Add(new Tuple<PickupObject, bool>(item, true));
                        ToolsCharApi.Print("    " + item.EncounterNameOrDisplayName + " (infinite)");
                        continue;
                    }
                    else
                    {
                        ToolsCharApi.PrintError(item.EncounterNameOrDisplayName + " is not a gun, and therefore cannot be infinite");
                    }
                }
                else
                {
                    items.Add(new Tuple<PickupObject, bool>(item, false));
                    ToolsCharApi.Print("    " + item.EncounterNameOrDisplayName);
                }

            }

            ToolsCharApi.PrintError("Invalid loadout setup, expecting '</loadout>' but found none");
            return new List<Tuple<PickupObject, bool>>();
        }

        public static List<Tuple<PickupObject, bool>> GetAltguns(string[] lines, int startIndex, out int endIndex)
        {
            endIndex = startIndex;

            ToolsCharApi.Print("altguns loadout...");
            List<Tuple<PickupObject, bool>> items = new List<Tuple<PickupObject, bool>>();
            //ToolsCharApi.PrintError("go fuck yourself");
            string line;
            string[] args;
            for (int i = startIndex; i < lines.Length; i++)
            {
                endIndex = i;
                line = lines[i].ToLower().Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("</altguns>")) return items;

                args = line.Split(' ');
                if (args.Length == 0) continue;

                if (!Gungeon.Game.Items.ContainsID(args[0]))
                {
                    ToolsCharApi.PrintError("Could not find item with ID: \"" + args[0] + "\"");
                    continue;
                }
                var item = Gungeon.Game.Items[args[0]];
                if (item == null)
                {
                    ToolsCharApi.PrintError("Could not find item with ID: \"" + args[0] + "\"");
                    continue;
                }
                var gun = item.GetComponent<Gun>();

                if (gun == null)
                {
                    ToolsCharApi.PrintError("\"" + args[0] + "\" isn't a gun...");
                    continue;
                }

                if (args.Length > 1 && args[1].Contains("infinite"))
                {
                    

                    if (gun != null)
                    {
                        if (!CharacterBuilder.guns.Contains(gun) && !gun.InfiniteAmmo)
                            CharacterBuilder.guns.Add(gun);

                        items.Add(new Tuple<PickupObject, bool>(item, true));
                        ToolsCharApi.Print("    " + item.EncounterNameOrDisplayName + " (infinite)");
                        continue;
                    }
                    else
                    {
                        ToolsCharApi.PrintError(item.EncounterNameOrDisplayName + " is not a gun, and therefore cannot be infinite");
                    }
                }
                else
                {
                    items.Add(new Tuple<PickupObject, bool>(item, false));
                    ToolsCharApi.Print("    " + item.EncounterNameOrDisplayName);
                }

            }

            ToolsCharApi.PrintError("Invalid loadout setup, expecting '</altguns>' but found none");
            return new List<Tuple<PickupObject, bool>>();
        }

    }
}
