using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alexandria.NPCAPI
{
    public static class PlayMakerExtensions
    {
        public static FsmState AddState(this PlayMakerFSM fsm, string name, bool isStartState = false, bool isSequence = false)
        {
            var dest = new FsmState[fsm.FsmStates.Length + 1];
            Array.Copy(fsm.FsmStates, dest, fsm.FsmStates.Length);
            var newState = new FsmState(fsm.Fsm)
            {
                Name = name,
                Transitions = new FsmTransition[0],
                Actions = new FsmStateAction[0],
                Fsm = fsm.fsm,
                IsSequence = isSequence
            };
            dest[fsm.FsmStates.Length] = newState;
            fsm.Fsm.States = dest;
            if (isStartState) fsm.Fsm.StartState = name;
            return newState;
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        public static void RemoveAction(this FsmState s, int i)
        {
            var actions = new FsmStateAction[s.Actions.Length - 1];
            Array.Copy(s.Actions, actions, i);
            Array.Copy(s.Actions, i + 1, actions, i, s.Actions.Length - i - 1);
            s.Actions = actions;
        }

        public static void AddAction(this FsmState s, FsmStateAction a)
        {
            var arr = s.Actions.ToList();
            arr.Add(a);
            s.Actions = arr.ToArray();
            s.SaveActions();
        }

        public static void ReplaceAction(this FsmState s, int i, FsmStateAction a)
        {
            s.Actions[i] = a;
        }

        public static void AddTransition(this FsmState s, string eventName, string toState, bool eventIsGlobal)
        {
            var transitions = new FsmTransition[s.Transitions.Length + 1];
            Array.Copy(s.Transitions, transitions, s.Transitions.Length);

            if (FsmEvent.GetFsmEvent(eventName) == null) s.fsm.AddEvent(eventName, eventIsGlobal);

            transitions[s.Transitions.Length] = new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToState = toState,
            };
            s.Transitions = transitions;
        }

        public static void AddGlobalTransition(this Fsm fsm, string eventName, string toState, bool eventIsGlobal)
        {
            var transitions = new FsmTransition[fsm.globalTransitions.Length + 1];
            Array.Copy(fsm.globalTransitions, transitions, fsm.globalTransitions.Length);

            if (FsmEvent.GetFsmEvent(eventName) == null) fsm.AddEvent(eventName, eventIsGlobal);

            transitions[fsm.globalTransitions.Length] = new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToState = toState,
            };
            fsm.globalTransitions = transitions;
        }

        public static void AddEvent(this Fsm fsm, string eventName, bool global)
        {
            var events = new FsmEvent[fsm.events.Length + 1];
            Array.Copy(fsm.events, events, fsm.events.Length);
            events[fsm.events.Length] = new FsmEvent(eventName)
            {
                IsGlobal = global,
            };
            fsm.events = events;
        }

        public static void RemoveAllTransitions(this FsmState s)
        {
            s.Transitions = new FsmTransition[0];
        }

        public static FsmInt GetFsmInt(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmVariables.IntVariables.FirstOrDefault(v => v.Name == name);
        }

        public static FsmBool GetFsmBool(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmVariables.BoolVariables.FirstOrDefault(v => v.Name == name);
        }
    }
}
