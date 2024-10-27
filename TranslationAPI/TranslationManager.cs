using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Alexandria.TranslationAPI
{
    [Obsolete("TranslationAPI is deprecated, use ETGMod.Databases.Strings for translations instead.", true)]
    public static class TranslationManager
    {
        public static void Init()
        {
        }

        public static void Unload()
        {
        }

        public static void TranslateItemName(this PickupObject item, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            item.SetName(language, translation);
        }

        public static void TranslateItemShortDescription(this PickupObject item, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            item.SetShortDescription(language, translation);
        }

        public static void TranslateItemLongDescription(this PickupObject item, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            item.SetLongDescription(language, translation);
        }

        public static void TranslateEnemyName(this AIActor enemy, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if (enemy == null || enemy.encounterTrackable == null || enemy.encounterTrackable.journalData == null)
                return;

            ETGMod.Databases.Strings.Enemies.Set(language, enemy.encounterTrackable.journalData.PrimaryDisplayName, translation);
        }

        public static void TranslateEnemyShortDescription(this AIActor enemy, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if (enemy == null || enemy.encounterTrackable == null || enemy.encounterTrackable.journalData == null)
                return;

            ETGMod.Databases.Strings.Enemies.Set(language, enemy.encounterTrackable.journalData.NotificationPanelDescription, translation);
        }

        public static void TranslateEnemyLongDescription(this AIActor enemy, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if (enemy == null || enemy.encounterTrackable == null || enemy.encounterTrackable.journalData == null)
                return;

            ETGMod.Databases.Strings.Enemies.Set(language, enemy.encounterTrackable.journalData.AmmonomiconFullEntry, translation);
        }

        public static void TranslateName(this EncounterTrackable track, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if(track == null || track.journalData == null)
                return;

            var table = track.journalData.IsEnemy ? ETGMod.Databases.Strings.Enemies : ETGMod.Databases.Strings.Items;
            table.Set(language, track.journalData.PrimaryDisplayName, translation);
        }

        public static void TranslateShortDescription(this EncounterTrackable track, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if (track == null || track.journalData == null)
                return;

            var table = track.journalData.IsEnemy ? ETGMod.Databases.Strings.Enemies : ETGMod.Databases.Strings.Items;
            table.Set(language, track.journalData.NotificationPanelDescription, translation);
        }

        public static void TranslateLongDescription(this EncounterTrackable track, StringTableManager.GungeonSupportedLanguages language, string translation)
        {
            if (track == null || track.journalData == null)
                return;

            var table = track.journalData.IsEnemy ? ETGMod.Databases.Strings.Enemies : ETGMod.Databases.Strings.Items;
            table.Set(language, track.journalData.AmmonomiconFullEntry, translation);
        }

        public static void AddTranslation(StringTableManager.GungeonSupportedLanguages language, string key, string value, StringTableType tableType)
        {
            switch (tableType)
            {
                case StringTableType.Core:
                    ETGMod.Databases.Strings.Core.Set(language, key, value);
                    break;
                case StringTableType.Items:
                    ETGMod.Databases.Strings.Items.Set(language, key, value);
                    break;
                case StringTableType.Enemies:
                    ETGMod.Databases.Strings.Enemies.Set(language, key, value);
                    break;
                case StringTableType.UI:
                    ETGMod.Databases.Strings.UI.Set(language switch
                    {
                        StringTableManager.GungeonSupportedLanguages.ENGLISH => dfLanguageCode.EN,
                        StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE => dfLanguageCode.PT,
                        StringTableManager.GungeonSupportedLanguages.FRENCH => dfLanguageCode.FR,
                        StringTableManager.GungeonSupportedLanguages.GERMAN => dfLanguageCode.DE,
                        StringTableManager.GungeonSupportedLanguages.ITALIAN => dfLanguageCode.IT,
                        StringTableManager.GungeonSupportedLanguages.SPANISH => dfLanguageCode.ES,
                        StringTableManager.GungeonSupportedLanguages.POLISH => dfLanguageCode.PL,
                        StringTableManager.GungeonSupportedLanguages.RUSSIAN => dfLanguageCode.RU,
                        StringTableManager.GungeonSupportedLanguages.JAPANESE => dfLanguageCode.JA,
                        StringTableManager.GungeonSupportedLanguages.KOREAN => dfLanguageCode.KO,
                        StringTableManager.GungeonSupportedLanguages.RUBEL_TEST => dfLanguageCode.QU,
                        StringTableManager.GungeonSupportedLanguages.CHINESE => dfLanguageCode.ZH,
                        _ => dfLanguageCode.EN,
                    }, key, value);
                    break;
                case StringTableType.Intro:
                    ETGMod.Databases.Strings.Intro.Set(language, key, value);
                    break;
                case StringTableType.Synergy:
                    ETGMod.Databases.Strings.Synergy.Set(language, key, value);
                    break;
            }
        }

        public static void ForceUpdateTranslation()
        {
        }

        public static Dictionary<string, StringTableManager.StringCollection> SynergyTable => StringDB.SynergyTable;

        public static Dictionary<string, StringTableManager.StringCollection> UITable
        {
            get
            {
                StringTableManager.GetSynergyString("");
                return StringTableManager.m_uiTable;
            }
        }

        public static List<Translation> translations = new();
        public static CustomStringDBTable UIStrings = new();
        public static CustomStringDBTable SynergyStrings = new(ETGMod.Databases.Strings.Synergy);
    }
}
