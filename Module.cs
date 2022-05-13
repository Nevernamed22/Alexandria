using Alexandria.Miscellaneous_Helpers;
using EnemyAPI;
using GungeonAPI;
using ItemAPI;
using SaveAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
                NpcApi.NPCHooks.Init();
                EnemyAPI.Hooks.Init();

                // ETGModConsole.Log($"pre Trying to create thingo bot.alexandria - TEST", true);

                //EnumUtility.GetEnumValue<CustomDungeonFlags>("alexandria", "TEST");

                SaveAPIManager.CreateNewDungeonFlag("alexandria", "TEST2");
                SaveAPIManager.CreateNewDungeonFlag("alexandria", "TEST");

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
                SaveAPIManager.Init();
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
            
        }
    }
}
