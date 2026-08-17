using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using static StringTableManager; // GungeonSupportedLanguages

namespace Alexandria.Integrations
{
  // public API
  public static partial class ItemTipsIntegration
  {
    /// <summary>Add an item tip for gun, passive, or active item. The item must be set up and have a valid PickupObjectId.</summary>
    public static void AddItemTip(this PickupObject pickup, string tip)
    {
      if (!_DidSetup)
        DoInitialSetup();
      if (!_ItemTipsInstalled)
        return;

      int itemID = pickup.PickupObjectId;
      if (itemID < 0)
      {
        ETGModConsole.Log(_LOG_HEADER + $"{pickup.itemName} does not have a valid pickup id, refusing to add an item tip");
        return;
      }

      pickup.CreateTip(Assembly.GetCallingAssembly(), tip);
    }

    /// <summary>Add an item tip for a synergy. If no name is provided for the synergy, the synergy's key will be used.</summary>
    public static void AddItemTip(this AdvancedSynergyEntry synergy, string tip, string name = null)
    {
      if (!_DidSetup)
        DoInitialSetup();
      if (!_ItemTipsInstalled)
        return;

      synergy.CreateTip(Assembly.GetCallingAssembly(), tip, name);
    }

    /// <summary>Set the language for all future item tips created from the calling assembly.</summary>
    public static void SetItemTipLanguage(GungeonSupportedLanguages language)
    {
      _AssemblyToTipLanguage[Assembly.GetCallingAssembly()] = language;
    }

    /// <summary>Set the language for all future item tips created from the calling assembly to RUBEL_TEST, making external tips always take priority.</summary>
    public static void PreferExternalTips(GungeonSupportedLanguages language)
    {
      _AssemblyToTipLanguage[Assembly.GetCallingAssembly()] = GungeonSupportedLanguages.RUBEL_TEST;
    }
  }

  // private API
  public static partial class ItemTipsIntegration
  {
    internal class RuntimItemTipData
    {
      public PickupObject pickup = null;
      public AdvancedSynergyEntry synergy = null;
      public string tip = null;
      public string name = null;
      public GungeonSupportedLanguages language = default;
    }

    private static void CreateTip(this PickupObject pickup, Assembly caller, string tip) =>
      _RuntimeTips.Add(new RuntimItemTipData(){ pickup = pickup, tip = tip, language = caller.TipLanguage() });
    private static void CreateTip(this AdvancedSynergyEntry synergy, Assembly caller, string tip, string name = null) =>
      _RuntimeTips.Add(new RuntimItemTipData(){ synergy = synergy, tip = tip, name = name, language = caller.TipLanguage() });
    private static bool HasItemTip(this PickupObject pickup) =>
      _ItemTipsInstalled && (bool)_GetItemMethod.Invoke(_ItemCachePickups, new object[] { pickup.PickupObjectId, null });
    private static bool HasItemTip(this AdvancedSynergyEntry synergy) =>
      _ItemTipsInstalled && (bool)_GetSynergyMethod.Invoke(_ItemCacheSynergies, new object[] { synergy.NameKey, null });
    private static GungeonSupportedLanguages TipLanguage(this Assembly caller) =>
      _AssemblyToTipLanguage.TryGetValue(caller, out var lang) ? lang : GungeonSupportedLanguages.ENGLISH;

    private const string _LOG_HEADER = "ItemTips integration: ";

    private static bool _DidSetup = false;
    private static bool _ItemTipsInstalled = false;
    private static Dictionary<Assembly, GungeonSupportedLanguages> _AssemblyToTipLanguage = new();
    private static readonly List<RuntimItemTipData> _RuntimeTips = new();

    private static object _ItemCachePickups;
    private static object _ItemCacheSynergies;
    private static Type _ItemDataType;
    private static Type _SynergyDataType;
    private static MethodInfo _AddItemMethod;
    private static MethodInfo _AddSynergyMethod;
    private static MethodInfo _GetItemMethod;
    private static MethodInfo _GetSynergyMethod;
    private static Type _SourceMetaDataType;
    private static FieldInfo _ItemDataNameField;
    private static FieldInfo _ItemDataIdField;
    private static FieldInfo _ItemDataNotesField;
    private static FieldInfo _ItemDataSourceMetadataField;
    private static FieldInfo _SynergyDataNameField;
    private static FieldInfo _SynergyDataKeyField;
    private static FieldInfo _SynergyDataEffectField;
    private static FieldInfo _SynergyDataSourceMetadataField;

    private static void DoInitialSetup()
    {
      _DidSetup = true; // don't run this function more than once

      // check if ItemTips is installed
      // ETGModConsole.Log($"attempting itemtips setup");
      if (!Chainloader.PluginInfos.TryGetValue("glorfindel.etg.itemtips", out PluginInfo itemTipsPluginInfo))
      {
        // ETGModConsole.Log($" itemtips not found, nothing to do");
        return;
      }

      // actually attempt to set up reflection necessary for ItemTips integration
      try
      {
        // get the tip cache
        object itemTipsPlugin = itemTipsPluginInfo.Instance;
        Type itemTipsPluginType = itemTipsPlugin.GetType();
        FieldInfo tipCachefield = itemTipsPluginType.GetField("_tipCache",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        object tipCache = tipCachefield.GetValue(itemTipsPlugin);
        if (tipCache == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "cache missing");
          return;
        }

        // get the pickup dictionary
        Type itemTipsCacheType = tipCache.GetType();
        FieldInfo itemCachePickupsField = itemTipsCacheType.GetField("Pickups",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        _ItemCachePickups = itemCachePickupsField.GetValue(tipCache);
        if (_ItemCachePickups == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "itemCachePickups missing");
          return;
        }

        // get the synergy dictionary
        FieldInfo itemCacheSynergiesField = itemTipsCacheType.GetField("Synergies",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        _ItemCacheSynergies = itemCacheSynergiesField.GetValue(tipCache);
        if (_ItemCacheSynergies == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "itemCacheSynergies missing");
          return;
        }

        // get the itemdata type
        Assembly itemTipsAssembly = itemTipsCacheType.Assembly;
        _ItemDataType = itemTipsAssembly.GetType("ItemTipsMod.ItemData");
        if (_ItemDataType == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "itemDataType missing");
          return;
        }

        // get the synergydata type
        _SynergyDataType = itemTipsAssembly.GetType("ItemTipsMod.SynergyData");
        if (_SynergyDataType == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "synergyDataType missing");
          return;
        }

        // get the ~~Add~~ set_Item method for the itemCachePickups dictionary // NOTE: use set_Item (e.g., dict[key] = ...) so duplicates are overridden
        _AddItemMethod = _ItemCachePickups.GetType().GetMethod("set_Item", new Type[] { typeof(int), _ItemDataType });
        if (_AddItemMethod == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "addItemMethod missing");
          return;
        }

        // get the ~~Add~~ set_Item method for the itemCacheSynergies dictionary
        _AddSynergyMethod = _ItemCacheSynergies.GetType().GetMethod("set_Item", new Type[] { typeof(string), _SynergyDataType });
        if (_AddSynergyMethod == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "addSynergyMethod missing");
          return;
        }

        // get the TryGetValue method for the itemCachePickups dictionary // NOTE: use set_Item (e.g., dict[key] = ...) so duplicates are overridden
        _GetItemMethod = _ItemCachePickups.GetType().GetMethod("TryGetValue", new Type[] { typeof(int), _ItemDataType.MakeByRefType() });
        if (_GetItemMethod == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "getItemMethod missing");
          return;
        }

        // get the TryGetValue method for the itemCacheSynergies dictionary
        _GetSynergyMethod = _ItemCacheSynergies.GetType().GetMethod("TryGetValue", new Type[] { typeof(string), _SynergyDataType.MakeByRefType() });
        if (_GetSynergyMethod == null)
        {
          ETGModConsole.Log(_LOG_HEADER + "getSynergyMethod missing");
          return;
        }

        // get some miscellaneous metadata
        _SourceMetaDataType             = _ItemDataType.GetField("SourceMetadata").FieldType;
        _ItemDataNameField              = _ItemDataType.GetField("Name");
        _ItemDataIdField                = _ItemDataType.GetField("Id");
        _ItemDataNotesField             = _ItemDataType.GetField("Notes");
        _ItemDataSourceMetadataField    = _ItemDataType.GetField("SourceMetadata");
        _SynergyDataNameField           = _SynergyDataType.GetField("Name");
        _SynergyDataKeyField            = _SynergyDataType.GetField("Key");
        _SynergyDataEffectField         = _SynergyDataType.GetField("Effect");
        _SynergyDataSourceMetadataField = _SynergyDataType.GetField("SourceMetadata");

        // patch ItemTips' ScanExternalData() function to run our own item tips
        Alexandria._Harmony.Patch(
          original: itemTipsPluginType.GetMethod("ScanExternalData", bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic),
          postfix: new HarmonyMethod(typeof(ItemTipsIntegration).GetMethod(nameof(AddAlexandriaTips), bindingAttr: BindingFlags.Static | BindingFlags.NonPublic)));

        // we can now register item tips!
        _ItemTipsInstalled = true;
      }
      catch (Exception ex)
      {
        ETGModConsole.Log($" failed to set up ItemTips integration, tell Captain Pretzel");
        UnityEngine.Debug.LogException(ex);
      }
    }

    private static void AddAlexandriaTips()
    {
      // Get the current game language to make sure we're not overwriting translation files
      GungeonSupportedLanguages currentLanguage = StringTableManager.CurrentLanguage;
      // UnityEngine.Debug.Log($"Registering {_RuntimeTips.Count} runtime tips");
      foreach (RuntimItemTipData tipData in _RuntimeTips)
      {
        bool sameLanguage = currentLanguage == tipData.language;
        if (tipData.pickup != null)
        {
          if (!sameLanguage && tipData.pickup.HasItemTip())
          {
            // ETGModConsole.Log($"synergy {tipData.pickup.DisplayName} already has a tip in a different language!");
            continue; // don't overwrite translations
          }
          object itemData = Activator.CreateInstance(_ItemDataType);
          int itemID = tipData.pickup.PickupObjectId;
          _ItemDataNameField.SetValue(itemData, tipData.pickup.itemName);
          _ItemDataIdField.SetValue(itemData, itemID);
          _ItemDataNotesField.SetValue(itemData, tipData.tip);
          _ItemDataSourceMetadataField.SetValue(itemData, GenerateSourceMetadata());
          _AddItemMethod.Invoke(_ItemCachePickups, new object[] { itemID, itemData });
        }
        else if (tipData.synergy != null)
        {
          if (!sameLanguage && tipData.synergy.HasItemTip())
          {
            // ETGModConsole.Log($"synergy {tipData.synergy.NameKey} already has a tip in a different language!");
            continue; // don't overwrite translations
          }
          string synergyId = tipData.synergy.NameKey;
          object synergyData = Activator.CreateInstance(_SynergyDataType);
          _SynergyDataType.GetField("Name").SetValue(synergyData, !string.IsNullOrEmpty(tipData.name) ? tipData.name : synergyId);
          _SynergyDataType.GetField("Key").SetValue(synergyData, synergyId);
          _SynergyDataType.GetField("Effect").SetValue(synergyData, tipData.tip);
          _SynergyDataType.GetField("SourceMetadata").SetValue(synergyData, GenerateSourceMetadata());
          _AddSynergyMethod.Invoke(_ItemCacheSynergies, new object[] { synergyId, synergyData });
        }
      }
    }

    //NOTE: SourceMetadata seems unused except for debugging, so just make it clear these tips are coming from Alexandria
    private static object GenerateSourceMetadata()
    {
      const string META_NAME    = Alexandria.NAME;
      const string META_URL     = "https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/";
      const string META_VERSION = Alexandria.VERSION;
      object sourceMetaData = Activator.CreateInstance(_SourceMetaDataType);
      _SourceMetaDataType.GetField("Name").SetValue(sourceMetaData, META_NAME);
      _SourceMetaDataType.GetField("Url").SetValue(sourceMetaData, META_URL);
      _SourceMetaDataType.GetField("Version").SetValue(sourceMetaData, META_VERSION);
      return sourceMetaData;
    }
  }
}
