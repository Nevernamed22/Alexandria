﻿using System;
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
using Alexandria.BindingAPI;

namespace Alexandria
{
    public class Alexandria : ETGModule
    {
        public override void Start()
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

                BindingBuilder.Init();
                ETGModConsole.Log("started!!");
                //ETGModConsole.Log("started!!", true);
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }
        public override void Exit() { }
        public override void Init()
        {
            try
            {
                //SaveAPIManager.Setup("aTEST");
                //SaveAPIManager.Init();
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }
    }
}
