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
using Alexandria.Assetbundle;
using BepInEx.Bootstrap;

namespace Alexandria
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Alexandria : BaseUnityPlugin
    {

        public const string GUID = "alexandria.etgmod.alexandria";
        public const string NAME = "Alexandria";

        public const string VERSION = "0.3.34";


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


                this.StartCoroutine(this.delayedstarthandler());
                ETGModConsole.Log("AlexandriaLib started correctly. Ver. : "+VERSION);
                //DungeonAPI.RoomFactory.BuildNewRoomFromResource("Alexandria/Testing/testMegaFinale.newroom");
                //DungeonAPI.RoomFactory.BuildNewRoomFromResource("Alexandria/Testing/KP-Manuel.newroom");
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
            //yield return null;
            //GunInt.FinalizeSprites();
            yield break;
        }
    }
}
