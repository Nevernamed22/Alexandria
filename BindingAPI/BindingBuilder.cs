using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InControl;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json;
using UnityEngine;

namespace Alexandria.BindingAPI
{
	public class BindingBuilder : PlayerActionSet
	{
		public static Dictionary<string, PlayerAction> bindings = new Dictionary<string, PlayerAction>();
		public static BindingBuilder Instance;
		/// <param name="prefix">the name of the folder your bindings will be saved in, it will be "bindings_{prefix}.json</param>
		public static void Init()
		{
			new BindingBuilder().CreateSingleton();
			BindingHooks.Init();

			ETGModMainBehaviour.Instance.gameObject.AddComponent<BindingLoader>();
			BindingLoader.BindingDataLoc = Path.Combine(ETGMod.ResourcesDirectory, "CustomBindings.json");
			BindingLoader.LoadBindings();
		}
		private void CreateSingleton()
		{
			if (Instance == null)
			{
				Instance = this;
			}
		}
		/// <param name="defaultKey">a button on your keyboard if you want that as the default key</param>
		/// <param name="inputControlType">a button on (i think controller) if you want that as a default key (i think you can have both this and default key, idk ¯\_(ツ)_/¯)</param>
		/// <param name="defaultmouse">a button on mouse if you want that as default key</param>
		public static PlayerAction CreateBinding(string BindingName, Key? defaultKey = null, InputControlType? inputControlType = null, Mouse? defaultmouse = null)
		{

			if (bindings.ContainsKey(BindingName))
				return bindings[BindingName];

			var action =Instance.CreatePlayerAction(BindingName);

			if (defaultKey != null)
				action.AddDefaultBinding(defaultKey.Value);

			if (inputControlType != null)
				action.AddDefaultBinding(inputControlType.Value);

			if (defaultmouse != null)
				action.AddDefaultBinding(defaultmouse.Value);


			bindings.Add(BindingName, action);
			BindingLoader.LoadBindings();
			return action;
		}

		public static PlayerAction CreateBinding(string BindingName, BindingSource[] sources)
		{
			ETGModConsole.Log($"Adding binding {BindingName} with sources {sources}");

			if (bindings.ContainsKey(BindingName))
				return bindings[BindingName];

			var action = Instance.CreatePlayerAction(BindingName);
			action.AddDefaultBinding(sources[0]);

			for (int i = 1; i < sources.Length; i++)
			{
				action.AddBinding(sources[i]);
			}

			bindings.Add(BindingName, action);
			BindingLoader.LoadBindings();
			return action;
		}

		public static OneAxisInputControl CreateOneAxisBinding(PlayerAction negativeAction, PlayerAction positiveAction)
		{
			return Instance.CreateOneAxisPlayerAction(negativeAction, positiveAction);
		}

		public static PlayerTwoAxisAction CreateTwoAxisBinding(PlayerAction negativeXAction, PlayerAction positiveXAction, PlayerAction negativeYAction, PlayerAction positiveYAction)
		{
			return Instance.CreateTwoAxisPlayerAction(negativeXAction, positiveXAction, negativeYAction, positiveYAction);
		}

	}
}
