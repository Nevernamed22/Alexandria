using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HutongGames.PlayMaker;

namespace Alexandria.NPCAPI
{
    public static class NpcAPI
    {

        public static PlayMakerFSM CreateBlankPlayMakerFSM(this GameObject gameObject, string name)
        {
            var pfsm = gameObject.GetOrAddComponent<PlayMakerFSM>();
            //pfsm.ev = false;
            pfsm.fsmTemplate = null;

            pfsm.fsm = new Fsm();

            pfsm.fsm.Reset(pfsm);
            pfsm.fsm.states = new FsmState[0];
            pfsm.fsm.dataVersion = 1;
            pfsm.fsm.usedInTemplate = null;
            pfsm.fsm.name = name;
            pfsm.fsm.RestartOnEnable = true;
            if (pfsm.fsm.Variables.FindFsmString("currentMode") == null)
            {
                List<FsmString> list = new List<FsmString>(pfsm.fsm.Variables.StringVariables);
                list.Add(new FsmString("currentMode")
                {
                    Value = "modeBegin"
                });
                pfsm.fsm.Variables.StringVariables = list.ToArray();
            }

            return pfsm;
        }
    }
}
