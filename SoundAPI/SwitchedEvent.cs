using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.SoundAPI
{
    /// <summary>
    /// A class representing an event being played with a specific switch.
    /// </summary>
    public class SwitchedEvent
    {
        /// <summary>
        /// The name of the event that will be played.
        /// </summary>
        public string EventName;

        /// <summary>
        /// The switch group for the switch that will be played. If null or empty, the event will be played normally.
        /// </summary>
        public string SwitchGroup;
        /// <summary>
        /// The value of the switch that will be played.
        /// </summary>
        public string SwitchValue;

        /// <summary>
        /// Creates a new SwitchedEvent and sets its SwitchGroup and SwitchValue.
        /// </summary>
        /// <param name="eventName">The name of the event that will be played.</param>
        /// <param name="switchGroup">The switch group for the switch that will be played. If null or empty, the event will be played normally.</param>
        /// <param name="switchValue">The value of the switch that will be played.</param>
        public SwitchedEvent(string eventName, string switchGroup, string switchValue)
        {
            EventName = eventName;
            SwitchGroup = switchGroup;
            SwitchValue = switchValue;
        }

        /// <summary>
        /// Creates a new SwitchedEvent without any switch overrides.
        /// </summary>
        /// <param name="eventName">The name of the event that will be played.</param>
        public SwitchedEvent(string eventName)
        {
            EventName = eventName;
        }

        public static implicit operator SwitchedEvent(string e)
        {
            return new(e);
        }

        /// <summary>
        /// Plays the event. If SwitchGroup isn't null, tepmorarily sets its switch value to SwitchValue.
        /// </summary>
        /// <param name="go">The object the event will be played on.</param>
        /// <returns></returns>
        public uint Play(GameObject go)
        {
            // If game object or event is null, do nothing.
            if(go == null || string.IsNullOrEmpty(EventName))
            {
                return 0;
            }

            // If this SwitchedEvent has a specific switch group to play, temporarily change the object's switch value for that group to this SwitchedEvent's SwitchValue.
            if(!string.IsNullOrEmpty(SwitchGroup))
            {
                SoundManager.SetSwitch_Orig(SwitchGroup, SwitchValue ?? "", go);
            }

            // Play the sound.
            var res = SoundManager.PostEvent_Orig(EventName, go);

            var store = go.GetComponent<SwitchStorer>();

            if (!string.IsNullOrEmpty(SwitchGroup))
            {
                // If the object has any stored switch data, read the original switch from there. Otherwise, set the original switch to an empty string (object didn't have a switch before).
                if (store == null || store.Switches == null || !store.Switches.TryGetValue(SwitchGroup, out var returnVal) || returnVal == null)
                {
                    returnVal = "";
                }

                // Return the object's switch to its original value.
                SoundManager.SetSwitch_Orig(SwitchGroup, returnVal, go);
            }

            return res;
        }
    }
}
