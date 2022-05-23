using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Alexandria.Misc
{
    class CustomActions
    {
        public static Action<RewardPedestal> OnRewardPedestalSpawned;
        public static Action<Chest> OnChestPostSpawn;
        public static Action<Chest, PlayerController> OnChestPreOpen;
        public static Action<Chest> OnChestBroken;

        public static void InitHooks()
        {
            pedestalSpawnHook = new Hook(
                typeof(RewardPedestal).GetMethod("MaybeBecomeMimic", BindingFlags.Instance | BindingFlags.Public),
                typeof(CustomActions).GetMethod("PostProcessPedestal", BindingFlags.Static | BindingFlags.Public)
            );
            chestPostProcessHook = new Hook(
                typeof(Chest).GetMethod("PossiblyCreateBowler", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("PostProcessChest", BindingFlags.Static | BindingFlags.NonPublic)
            );
            chestPreOpenHook = new Hook(
                typeof(Chest).GetMethod("Open", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("ChestPreOpen", BindingFlags.Static | BindingFlags.NonPublic)
            );
            chestBrokenHook = new Hook(
                typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof(CustomActions).GetMethod("OnBroken", BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        private static Hook pedestalSpawnHook;
        private static Hook chestPostProcessHook;
        private static Hook chestPreOpenHook;
        private static Hook chestBrokenHook;
        public static void PostProcessPedestal(Action<RewardPedestal> orig, RewardPedestal self) { orig(self); if (OnRewardPedestalSpawned != null) { OnRewardPedestalSpawned(self); } }
        private static void OnBroken(Action<Chest> orig, Chest self) { if (OnChestBroken != null) { OnChestBroken(self); } orig(self); }
        private static void PostProcessChest(Action<Chest, bool> orig, Chest self, bool uselssVar) { if (OnChestPostSpawn != null) { OnChestPostSpawn(self); } orig(self, uselssVar); }
        private static void ChestPreOpen(Action<Chest, PlayerController> orig, Chest self, PlayerController opener) { if (OnChestPreOpen != null) { OnChestPreOpen(self, opener); } orig(self, opener); }
    }
}
