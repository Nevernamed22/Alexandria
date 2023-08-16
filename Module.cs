using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using Alexandria.SaveAPI;
using Alexandria.NPCAPI;
using Alexandria.EnemyAPI;
using Alexandria.DungeonAPI;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using Alexandria.ChestAPI;
using BepInEx;
using Alexandria.CharacterAPI;
using System.Collections;
using HarmonyLib;
using System.Reflection;
using Dungeonator;

namespace Alexandria
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Alexandria : BaseUnityPlugin
    {

        public const string GUID = "alexandria.etgmod.alexandria";
        public const string NAME = "Alexandria";

        public const string VERSION = "0.3.13";

        public void Start()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            EasyEnumExtender.ExtendEnumsInAssembly(GUID);

            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {

            try
            {

                //The Most important classes, which must be initialised first
                StaticReferences.Init();
                DungeonHandler.Init();
                FakePrefabHooks.Init();
                ItemBuilder.Init();


                //Medium Priority
                CustomActions.InitHooks();
                ExtendedPlayerComponent.Init();
                EnemyTools.Init();
                EnemyBuilder.Init();
                BossBuilder.Init();
                NPCAPI.NPCHooks.Init();
                EnemyAPI.Hooks.Init();
                CustomDiscountManager.Init();
                GoopUtility.Init();

                //Low Priority
                CustomClipAmmoTypeToolbox.Init();
                ChamberGunAPI.Init();
                GenericItemAPIHooks.InitHooks();
                AlexandriaTags.InitGenericTags();
                ChestSpawnHelper.Init();
                Commands.Init();
                BreachShopTools.Init();
                AmmoPickupFixer.Init();
                LabelablePlayerItemSetup.InitLabelHook();
                MasteryOverrideHandler.Init();
                RoomRewardAPI.Init();


                //Character API
                CharacterAPI.Hooks.Init();
                ToolsCharApi.Init();


                ETGMod.StartGlobalCoroutine(this.delayedstarthandler());
                ETGModConsole.Log("AlexandriaLib started correctly. Ver. : "+VERSION);

                ETGModConsole.Commands.AddGroup("alex", args =>
                {
                });
                ETGModConsole.Commands.GetGroup("alex").AddUnit("roomname", Lock);
                ETGModConsole.Commands.GetGroup("alex").AddUnit("subtype", Lock2);
                ETGModConsole.Commands.GetGroup("alex").AddUnit("npcparadisce", Lock3);

            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }
        public static void Lock(string[] s)
        {
            RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
            ETGModConsole.Log(currentRoom.GetRoomName());
        }
        public static void Lock2(string[] s)
        {
            RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
            ETGModConsole.Log(currentRoom.RoomVisualSubtype);
        }
        public static void Lock3(string[] s)
        {
            GameManager.Instance.LoadCustomFlowForDebug("NPCParadise", "Base_Castle", "tt_castle");
        }

        public IEnumerator delayedstarthandler()
        {
            yield return null;
            ChamberGunAPI.DelayedInit();
            yield break;
        }
    }
}
