using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.NPCAPI;
using HutongGames.PlayMaker.Actions;
using HarmonyLib;
using UnityEngine;
using Alexandria.DungeonAPI;
using FullInspector;
using Alexandria.Misc;

namespace Alexandria.CharacterAPI
{
    [HarmonyPatch]
    public static class CustomCharacterBlackSmithHandler
    {
        public static void AddNewCharacter(this GameObject blacksmith, string name, PlayableCharacters characterEnum)
        {

            var blacksmithFsm = blacksmith.GetComponentInChildren<PlayMakerFSM>();

            blacksmithFsm.fsm.AddEvent($"Is{name}", false);

            foreach (var state in blacksmithFsm.Fsm.States)
            {
                foreach (var switcher in state.Actions.Where(action => action is CharacterClassSwitch))
                {

                    if (state.GetTransition(0).EventName.Contains("Coop")) continue;

                    var action = (CharacterClassSwitch)switcher;

                    var target = state.transitions.First(x => x.EventName == "IsBullet").ToState;
                    state.AddTransition($"Is{name}", target, false);

                    Shared.Append(ref action.compareTo, characterEnum);
                    Shared.Append(ref action.sendEvent, blacksmithFsm.fsm.Events.First(x => x.name == $"Is{name}"));
                }
            }

            blacksmithFsm.fsm.SaveActions();
            blacksmithFsm.fsm.InitData();

        }

        [HarmonyPatch(typeof(TalkDoerLite), nameof(TalkDoerLite.Start))]
        [HarmonyPrefix]
        public static void BlackSmithFix(TalkDoerLite __instance)
        {

            if (__instance.gameObject.GetComponent<SpecialComponents.WinchesterAlterer>() != null)
            {
                var val = __instance.gameObject.GetComponent<SpecialComponents.WinchesterAlterer>();
                var fsms = __instance.GetComponents<PlayMakerFSM>();

                foreach (var winchester in fsms)
                {
                    foreach (var state in winchester.Fsm.States)
                    {
                        foreach (var switcher in state.Actions.Where(action => action is Teleport))
                        {
                            (switcher as Teleport).useEndOfPath = false;
                            (switcher as Teleport).positionDelta.Value = val.movement;
                            (switcher as Teleport).goneTime.Value = val.goneTime;
                        }
                    }
                }
            }

            if (!__instance.gameObject.GetComponent<PlayMakerFSM>() || !__instance.gameObject.name.Contains("NPC_Blacksmith")) return;
            foreach (var character in CharacterBuilder.storedCharacters)
            {
                if (character.Value.First.hasPast) __instance.gameObject.AddNewCharacter(character.Value.First.nameShort.Replace(" ", ""), character.Value.First.identity);
                if (ToolsCharApi.EnableDebugLogging)
                {
                    ETGModConsole.Log($"Added \"{character.Value.First.identity}\" to the blacksmith fsm");
                }
            }


            foreach (var state in __instance.gameObject.GetComponent<PlayMakerFSM>().FsmStates)
            {
                foreach (var action in state.actions.Where(x => x is CharacterClassSwitch))
                {
                    var a = action as CharacterClassSwitch;
                    for (var i = 0; i < a?.sendEvent.Length; i++)
                    {
                        if (ToolsCharApi.EnableDebugLogging)
                        {
                            ETGModConsole.Log($"{a.sendEvent[i].name} - {a.compareTo[i]} >> {state.Transitions.First(x => x.EventName == a.sendEvent[i].name).toState}");
                        }
                    }
                }
            }
        }
    }
}
