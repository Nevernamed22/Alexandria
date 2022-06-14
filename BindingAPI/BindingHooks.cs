using InControl;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.BindingAPI
{
	public static class BindingHooks
	{

		private static PauseMenuController controller;
		public static void Init()
		{
			new Hook(typeof(GungeonActions).GetMethod("GetActionFromType", BindingFlags.Instance | BindingFlags.Public), typeof(BindingHooks).GetMethod("GetActionFromType", BindingFlags.Static | BindingFlags.Public));
			new Hook(typeof(FullOptionsMenuController).GetMethod("InitializeKeyboardBindingsPanel", BindingFlags.Instance | BindingFlags.NonPublic), typeof(BindingHooks).GetMethod("InitializeKeyboardBindingsPanel", BindingFlags.Static | BindingFlags.Public));
		}

		public static void InitializeKeyboardBindingsPanel(Action<FullOptionsMenuController> orig, FullOptionsMenuController self)
		{
			orig(self);

			controller = GameObject.FindObjectOfType<PauseMenuController>();
			KeyboardBindingMenuOption componentInChildren = controller.OptionsMenu.TabKeyboardBindings.GetComponentInChildren<KeyboardBindingMenuOption>();
			KeyboardBindingMenuOption previousMenuOption = componentInChildren;
			dfPanel component = componentInChildren.GetComponent<dfPanel>();
			MethodInfo dynMethod = typeof(FullOptionsMenuController).GetMethod("AddKeyboardBindingLine", BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var binding in BindingBuilder.bindings)
			{
				if(binding.Key.GetHashCode() > 100 || binding.Key.GetHashCode() < -1)
					dynMethod.Invoke(controller.OptionsMenu, new object[] { component.Parent, component.gameObject, (GungeonActions.GungeonActionType)binding.Key.GetHashCode(), binding.Key, previousMenuOption, false });
				else
					dynMethod.Invoke(controller.OptionsMenu, new object[] { component.Parent, component.gameObject, (GungeonActions.GungeonActionType)binding.Key.GetHashCode() + 100, binding.Key, previousMenuOption, false }); ;
			}
		}


		public static PlayerAction GetActionFromType(Func<GungeonActions, GungeonActions.GungeonActionType, PlayerAction> orig, GungeonActions self, GungeonActions.GungeonActionType type)
		{
			foreach (var binding in BindingBuilder.bindings)
			{
				if (binding.Key.GetHashCode() > 100 || binding.Key.GetHashCode() < -1)
				{
					if ((int)type == binding.Key.GetHashCode())
					{
						return binding.Value;
					}
				}
				else
				{
					if ((int)type == binding.Key.GetHashCode() + 100)
					{
						return binding.Value;
					}
				}
			}

			return orig(self, type);
		}
	}
}
