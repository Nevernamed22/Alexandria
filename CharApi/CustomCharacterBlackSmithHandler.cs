using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.NPCAPI;
using HutongGames.PlayMaker.Actions;
using HarmonyLib;
using UnityEngine;

namespace Alexandria.CharacterAPI
{
    [HarmonyPatch]
    public static class CustomCharacterBlackSmithHandler
    {
        public static void AddNewCharacter(this GameObject blacksmith, string name, PlayableCharacters characterEnum)
        {

            //var forgeDungeon = DungeonDatabase.GetOrLoadByName("Base_Forge");


            //8084016647973805596

            //var blacksmith = forgeDungeon.PatternSettings.flows[0].AllNodes.Where(node => node.overrideExactRoom != null && node.overrideExactRoom.name.Contains("Blacksmith_TestRoom")).First().overrideExactRoom.placedObjects
            //.Where(ppod => ppod != null && ppod.nonenemyBehaviour != null && ppod.nonenemyBehaviour.gameObject.name.Contains("NPC_Blacksmith_Forge")).First().nonenemyBehaviour.gameObject;


            var blacksmithFSM = blacksmith.GetComponentInChildren<PlayMakerFSM>();

            blacksmithFSM.fsm.AddEvent($"Is{name}", false);
            var stateToEdit = blacksmithFSM.fsm.GetState("Char Swizzlon 4");
            var stateToEdit2 = blacksmithFSM.fsm.GetState("Char Swizzlon 3");
            var stateToEdit3 = blacksmithFSM.fsm.GetState("Char Swizzlon 2");

            (stateToEdit.Actions[0] as CharacterClassSwitch).AddCharacterToSwitchClass(name, characterEnum);
            (stateToEdit2.Actions[0] as CharacterClassSwitch).AddCharacterToSwitchClass(name, characterEnum);
            (stateToEdit3.Actions[0] as CharacterClassSwitch).AddCharacterToSwitchClass(name, characterEnum);

           




            stateToEdit.AddTransition($"Is{name}", "Check Items", false);
            stateToEdit2.AddTransition($"Is{name}", "Finished Bullet?", false);
            stateToEdit3.AddTransition($"Is{name}", "Do I know you?", false);

            blacksmithFSM.fsm.SaveActions();
            blacksmithFSM.fsm.InitData();

            //forgeDungeon = null;

        }

        public static void AddCharacterToSwitchClass(this CharacterClassSwitch switchClass, string name, PlayableCharacters characterEnum)
        {
            var switchCharacterList = switchClass.compareTo.ToList();
            switchCharacterList.Add(characterEnum);
            switchClass.compareTo = switchCharacterList.ToArray();

            var switchEventList = switchClass.sendEvent.ToList();
            switchEventList.Add(new HutongGames.PlayMaker.FsmEvent($"Is{name}"));
            switchClass.sendEvent = switchEventList.ToArray();
        }




        [HarmonyPatch(typeof(TalkDoerLite), nameof(TalkDoerLite.Start))]
        [HarmonyPrefix]
        public static void BlackSmithFix(TalkDoerLite __instance)
        {

            if (__instance.gameObject.GetComponent<PlayMakerFSM>() && __instance.gameObject.name.Contains("NPC_Blacksmith"))
            {
                foreach (var character in CharacterBuilder.storedCharacters)
                {                  
                    if (character.Value.First.hasPast) __instance.gameObject.AddNewCharacter(character.Value.First.nameShort.Replace(" ", ""), character.Value.First.identity);
                    if (character.Value.First.hasPast) ETGModConsole.Log($"Added \"{character.Value.First.identity}\" to the blacksmith fsm");
                }


               foreach(var state in __instance.gameObject.GetComponent<PlayMakerFSM>().FsmStates)
               {
                    foreach (var action in state.actions.Where(x => x is CharacterClassSwitch))
                    {
                        var a = action as CharacterClassSwitch;
                        for (int i = 0; i < a.sendEvent.Length; i++)
                        {
                            ETGModConsole.Log($"{a.sendEvent[i].name} - {a.compareTo[i]} >> {state.Transitions.Where(x => x.EventName == a.sendEvent[i].name).First().toState}");
                        }
                    }
               }
            }
        }
    }
}
