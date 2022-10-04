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
using Alexandria.ChestApi;
using BepInEx;
using Alexandria.CharacterAPI;
using System.Collections;
using HarmonyLib;

namespace Alexandria
{


    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Alexandria : BaseUnityPlugin
    {


        public const string GUID = "alexandria.etgmod.alexandria";
        public const string NAME = "Alexandria";
        public const string VERSION = "0.2.3";


        public void Start()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {

            try
            {
                StaticReferences.Init();
                DungeonHandler.Init();

                FakePrefabHooks.Init();

                //GameStatsManager.Instance

                ItemBuilder.Init();
                //CharApi.Init("nn");
                CustomClipAmmoTypeToolbox.Init();
                EnemyTools.Init();
                EnemyBuilder.Init();
                BossBuilder.Init();
                NPCAPI.NPCHooks.Init();
                EnemyAPI.Hooks.Init();
                CustomActions.InitHooks();
                ChamberGunAPI.Init();
                ExtendedPlayerComponent.Init();
                GenericItemAPIHooks.InitHooks();

                AlexandriaTags.InitGenericTags();

                ChestSpawnHelper.Init();

                Commands.Init();

                BreachShopTools.Init();

                CharacterAPI.Hooks.Init();
                ToolsCharApi.Init();

                ETGMod.StartGlobalCoroutine(this.delayedstarthandler());
                ETGModConsole.Log("AlexandriaLib started correctly.");
                //ETGModConsole.Log("started!!", true);
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }

        public IEnumerator delayedstarthandler()
        {
            yield return null;
            ChamberGunAPI.DelayedInit();
            yield break;
        }
    }
}
