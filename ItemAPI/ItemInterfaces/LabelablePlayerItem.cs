using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace Alexandria.ItemAPI
{
    public interface ILabelItem
    {
        string GetLabel();
    }

    [HarmonyPatch]
    class LabelablePlayerItemSetup
    {
        [HarmonyPatch(typeof(GameUIItemController), nameof(GameUIItemController.UpdateItem))]
        [HarmonyPostfix]
        private static void GameUIItemControllerUpdateItemPatch(GameUIItemController __instance, PlayerItem current, List<PlayerItem> items)
        {
            if (!current || current is not ILabelItem labelitem)
                return;

            string label = labelitem.GetLabel();
            if (string.IsNullOrEmpty(label))
            {
                __instance.ItemCountLabel.IsVisible = false;
                return;
            }

            __instance.ItemCountLabel.AutoHeight = true; // enable multiline text
            __instance.ItemCountLabel.ProcessMarkup = true; // enable multicolor text
            __instance.ItemCountLabel.IsVisible = true;
            __instance.ItemCountLabel.Text = label;
        }

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void InitLabelHook() { }

        [Obsolete("This method should never be called outside Alexandria and is public for backwards compatability only.", true)]
        public static void UpdateCustomLabelHook(Action<GameUIItemController, PlayerItem, List<PlayerItem>> orig, GameUIItemController self, PlayerItem current, List<PlayerItem> items) { }
    }
}
