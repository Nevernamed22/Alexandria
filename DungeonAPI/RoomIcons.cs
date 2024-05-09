using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.DungeonAPI
{
    public class RoomIcons
    {
        public static  void LoadRoomIcons()
        {
            AssetBundle shared_auto_001 = ResourceManager.LoadAssetBundle("shared_auto_001");
            AssetBundle brave_resources_001 = ResourceManager.LoadAssetBundle("brave_resources_001");

            RoomIcons.BossRoomIcon = shared_auto_001.LoadAsset("assets/data/prefabs/room icons/minimap_boss_icon.prefab") as GameObject;
            RoomIcons.Teleporter_Room_Icon = brave_resources_001.LoadAsset("assets/data/prefabs/room icons/minimap_teleporter_icon.prefab") as GameObject;

            RoomIcons.Black_Chest_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_treasure_icon_black.prefab") as GameObject;
            RoomIcons.Basic_NPC_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_npc_icon.prefab") as GameObject;
            RoomIcons.Cursed_Mirror_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_cursed_mirror_icon.prefab") as GameObject;
            RoomIcons.Trorc_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_truck_merchant_icon.prefab") as GameObject;
            RoomIcons.CrestRoomIcon = brave_resources_001.LoadAsset("assets/resourcesbundle/crestminimapicon.prefab") as GameObject;
            RoomIcons.Mendy_And_Patches_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_smashtent_icon.prefab") as GameObject;
            RoomIcons.Gunsling_King_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_king_icon.prefab") as GameObject;

            RoomIcons.Cursula_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_merchcurse_icon.prefab") as GameObject;
            RoomIcons.Item_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_treasure_icon.prefab") as GameObject;
            RoomIcons.Gun_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_gun_icon.prefab") as GameObject;
            RoomIcons.Blank_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_blank_icon.prefab") as GameObject;
            RoomIcons.Rat_Trapdoor_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/rattrapdoorminimapicon.prefab") as GameObject;
            RoomIcons.Brown_Chest_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_treasure_icon_wood.prefab") as GameObject;
            RoomIcons.Green_Chest_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_treasure_icon_green.prefab") as GameObject;
            RoomIcons.Cell_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_cell_icon.prefab") as GameObject;

            RoomIcons.Goopton_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_merchgoop_icon.prefab") as GameObject;
            RoomIcons.Locked_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/minimap_locked_icon.prefab") as GameObject;
            RoomIcons.Unknown_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/minimap_unknown_icon.prefab") as GameObject;
            RoomIcons.Blocked_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/minimap_blocked_icon.prefab") as GameObject;

            RoomIcons.Gun_Muncher_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_muncher_icon.prefab") as GameObject;

            RoomIcons.Lost_Adventurer_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_lostadventurer_icon.prefab") as GameObject;
            RoomIcons.Shrine_Room_Icon   = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_shrine_icon.prefab") as GameObject;
            RoomIcons.Vampire_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_vampire_icon.prefab") as GameObject;
            RoomIcons.Blacksmith_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_blacksmith_icon.prefab") as GameObject;
            RoomIcons.Demon_Face_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_demon_face_icon.prefab") as GameObject;

            RoomIcons.Synergrace_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_synergrace_icon.prefab") as GameObject;
            RoomIcons.Cell_Key_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_cellkey_icon.prefab") as GameObject;
            RoomIcons.Witches_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_witches_icon.prefab") as GameObject;
            RoomIcons.Old_Red_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_merchblank_icon.prefab") as GameObject;
            RoomIcons.Cleanse_Shrine_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_cleanse_shrine_icon.prefab") as GameObject;
            RoomIcons.Flynt_Room_Icon = brave_resources_001.LoadAsset("assets/resourcesbundle/global prefabs/minimap_merchkey_icon.prefab") as GameObject;

        }


        public static GameObject BossRoomIcon;
        public static GameObject WinchesterRoomIcon;
        public static GameObject CrestRoomIcon;
        public static GameObject Black_Chest_Room_Icon;
        public static GameObject Basic_NPC_Room_Icon;
        public static GameObject Cursed_Mirror_Room_Icon;
        public static GameObject Trorc_Room_Icon;
        public static GameObject Mendy_And_Patches_Room_Icon;
        public static GameObject Gunsling_King_Room_Icon;
        public static GameObject Cursula_Room_Icon;
        public static GameObject _Room_Icon;
        public static GameObject Basic_Treasure_Room_Icon;
        public static GameObject Gun_Room_Icon;
        public static GameObject Item_Room_Icon;
        public static GameObject Blank_Room_Icon;
        public static GameObject Rat_Trapdoor_Room_Icon;
        public static GameObject Brown_Chest_Room_Icon;
        public static GameObject Green_Chest_Room_Icon;
        public static GameObject Cell_Room_Icon;
        public static GameObject Goopton_Room_Icon;
        public static GameObject Locked_Room_Icon;
        public static GameObject Unknown_Room_Icon;
        public static GameObject Blocked_Room_Icon;
        public static GameObject Gun_Muncher_Room_Icon;
        public static GameObject Lost_Adventurer_Room_Icon;
        public static GameObject Shrine_Room_Icon;
        public static GameObject Vampire_Room_Icon;
        public static GameObject Blacksmith_Room_Icon;
        public static GameObject Demon_Face_Room_Icon;
        public static GameObject Synergrace_Room_Icon;
        public static GameObject Cell_Key_Room_Icon;
        public static GameObject Witches_Room_Icon;
        public static GameObject Old_Red_Room_Icon;
        public static GameObject Cleanse_Shrine_Room_Icon;
        public static GameObject Flynt_Room_Icon;
        public static GameObject Teleporter_Room_Icon;

    }
}
