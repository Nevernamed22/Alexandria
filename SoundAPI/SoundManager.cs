using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AK.Wwise;
using HarmonyLib;
using UnityEngine;

namespace Alexandria.SoundAPI
{
    [HarmonyPatch]
    public static class SoundManager
    {
        // Hierarchy:
        //   Original Event
        //     Switch Group
        //       Switch Value
        //         Events for custom switch.
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, List<SwitchedEvent>>>> CustomSwitchData = new();

        // Dictionary where keys are the names of original events and the values are the switched events that will be played in addition to the original.
        private static readonly Dictionary<string, List<SwitchedEvent>> SwitchlessAddedEvents = new();

        // Original version of the Post Event method that won't play any custom switched events.
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject))]
        [HarmonyReversePatch(HarmonyReversePatchType.Original)]
        internal static uint PostEvent_Orig(string in_pszEventName, GameObject in_gameObjectID) => throw new NotImplementedException();

        // Original version of the Set Switch method that won't store the new switch.
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.SetSwitch), typeof(string), typeof(string), typeof(GameObject))]
        [HarmonyReversePatch(HarmonyReversePatchType.Original)]
        internal static uint SetSwitch_Orig(string in_pszSwitchGroup, string in_pszSwitchState, GameObject in_gameObjectID) => throw new NotImplementedException();

        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject))]
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject), typeof(uint))]
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object))]
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object), typeof(uint))]
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object), typeof(uint), typeof(AkExternalSourceInfo))]
        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.PostEvent), typeof(string), typeof(GameObject), typeof(uint), typeof(AkCallbackManager.EventCallback), typeof(object), typeof(uint), typeof(AkExternalSourceInfo), typeof(uint))]
        [HarmonyPrefix]
        private static bool OverrideEvent(ref uint __result, string in_pszEventName, GameObject in_gameObjectID)
        {
            // Check if the game object exists.
            if(in_gameObjectID == null)
            {
                return true;
            }

            // Check if there are any switchless added events attached to the played event.
            if (SwitchlessAddedEvents.TryGetValue(in_pszEventName.ToLowerInvariant(), out var switchlessEvents) && switchlessEvents != null)
            {
                foreach (var e in switchlessEvents)
                {
                    // Ignore null events.
                    if (e == null)
                    {
                        continue;
                    }

                    // Play the added switchless event without saving the result.
                    e.Play(in_gameObjectID);
                }
            }

            // Get the switch storer.
            var storer = in_gameObjectID.GetComponent<SwitchStorer>();

            // Check if the switch storer has any saved switches and then check if the played event has any custom switch data.
            if (storer == null || storer.Switches == null || storer.Switches.Count <= 0 || !CustomSwitchData.TryGetValue(in_pszEventName.ToLowerInvariant(), out var dat))
            {
                return true;
            }

            // By default, consider that no matching custom switch data was found.
            var runOrinal = true;

            // Go through all of the game object's switches.
            foreach (var s in storer.Switches)
            {
                // Check if the custom switch data for the event has keys for both the switch group and the switch value
                if (!dat.TryGetValue(s.Key.ToLowerInvariant(), out var dat2) || !dat2.TryGetValue(s.Value.ToLowerInvariant(), out var events))
                {
                    continue;
                }

                // A custom switch data was found, cancel the original method.
                runOrinal = false;

                // If the switch data has no events, don't play them.
                if (events == null)
                {
                    continue;
                }

                // Go through all of the events in the switch data.
                foreach (var e in events)
                {
                    // Ignore null events.
                    if (e == null)
                    {
                        continue;
                    }

                    // Play the event and save the result.
                    __result = e.Play(in_gameObjectID);
                }
            }

            // Run or cancel the original method based on if a replacement was found.
            return runOrinal;
        }

        [HarmonyPatch(typeof(AkSoundEngine), nameof(AkSoundEngine.SetSwitch), typeof(string), typeof(string), typeof(GameObject))]
        [HarmonyPostfix]
        public static void StoreNewSwitch(string in_pszSwitchGroup, string in_pszSwitchState, GameObject in_gameObjectID)
        {
            // Check if the game object exists and the switch group isn't null.
            if(in_gameObjectID == null || string.IsNullOrEmpty(in_pszSwitchGroup))
            {
                return;
            }

            // Add or get an existing switch storer.
            var storer = in_gameObjectID.GetOrAddComponent<SwitchStorer>();

            // If the switches dictionary is somehow null, initialize it.
            if(storer.Switches == null)
            {
                storer.Switches = new();
            }

            // Add or update the switch value for the group. If the new switch value is null, set it to an empty string instead.
            storer.Switches[in_pszSwitchGroup] = in_pszSwitchState ?? "";
        }

        /// <summary>
        /// Adds a switch override to the event <paramref name="originalEventName"/> that will play the <paramref name="overrideEvents"/> when the value of the group <paramref name="switchGroup"/> is <paramref name="switchValue"/>.
        /// </summary>
        /// <param name="switchGroup">The name of the switch group the switch override will apply to.</param>
        /// <param name="switchValue">The name of the switch value the <paramref name="switchGroup"/> will need to have for the overrides to apply.</param>
        /// <param name="originalEventName">The name of the original event the overrides will apply to.</param>
        /// <param name="overrideEvents">An array of the new events that will be played. Can be null or empty to make the event make no sound.</param>
        public static void AddCustomSwitchData(string switchGroup, string switchValue, string originalEventName, params SwitchedEvent[] overrideEvents)
        {
            // Check if the switch group, value and original event name aren't null.
            if(string.IsNullOrEmpty(switchGroup) || switchValue == null || string.IsNullOrEmpty(originalEventName))
            {
                return;
            }

            // Get or create data for the event name.
            if (!CustomSwitchData.TryGetValue(originalEventName.ToLowerInvariant(), out var dat))
            {
                CustomSwitchData[originalEventName.ToLowerInvariant()] = dat = new();
            }

            // Get or create data for the switch group.
            if(!dat.TryGetValue(switchGroup.ToLowerInvariant(), out var dat2))
            {
                dat[switchGroup.ToLowerInvariant()] = dat2 = new();
            }

            // Get or create event data.
            if(!dat2.TryGetValue(switchValue.ToLowerInvariant(), out var events))
            {
                dat2[switchValue.ToLowerInvariant()] = events = new();
            }

            // If override events is null or empty, don't add them to the events list.
            if(overrideEvents == null || overrideEvents.Length <= 0)
            {
                return;
            }

            // Add the override events to the events list.
            events.AddRange(overrideEvents);
        }

        /// <summary>
        /// Makes the <paramref name="additionalEvents"/> always be played in addition to <paramref name="originalEventName"/>.
        /// </summary>
        /// <param name="originalEventName">The name of the original event the additional events will be added to.</param>
        /// <param name="additionalEvents">An array of the events that will be played alongside <paramref name="originalEventName"/></param>
        public static void AddSwitchlessAdditionalEvent(string originalEventName, params SwitchedEvent[] additionalEvents)
        {
            // Check if the switch group, value and original event name aren't null.
            if (string.IsNullOrEmpty(originalEventName) || additionalEvents == null || additionalEvents.Length <= 0)
            {
                return;
            }

            // Get or create event data.
            if(!SwitchlessAddedEvents.TryGetValue(originalEventName.ToLowerInvariant(), out var events))
            {
                SwitchlessAddedEvents[originalEventName.ToLowerInvariant()] = events = new();
            }

            // Add the additional events to the events list.
            events.AddRange(additionalEvents);
        }
    }
}
