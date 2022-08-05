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
using CustomCharacters;

namespace Alexandria
{


    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Alexandria : BaseUnityPlugin
    {


        public const string GUID = "alexandria.etgmod.alexandria";
        public const string NAME = "Alexandria";
        public const string VERSION = "0.1.0";


        public void Start()
        {
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

                AlexandriaTags.InitGenericTags();

                AlexandriaHooks.Init();
                ChestSpawnHelper.Init();

                Commands.Init();

                BreachShopTools.Init();

                CustomCharacters.Hooks.Init();
                ToolsCharApi.Init();

                ETGModConsole.Log("started!!");
                //ETGModConsole.Log("started!!", true);
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }
    }
}
