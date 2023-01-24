using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.Misc;
using System.Collections;

namespace Alexandria.NPCAPI
{
    public class CustomDiscountManager
    {
        public static void Init() { CustomActions.OnShopItemStarted += OnMyShopItemStartedGlobal; }

        public static void OnMyShopItemStartedGlobal(ShopItemController shopItemController)
        {
            ShopDiscountController steamSale = shopItemController.gameObject.GetOrAddComponent<ShopDiscountController>();
            steamSale.UpdatePlacement();
            steamSale.discounts = DiscountsToAdd ?? new List<ShopDiscount>() { };
        }

        /// <summary>
        /// The list you add your ShopDiscounts to. These will be added automatically when ShopDiscountController starts anywhere.
        /// </summary>
        public static List<ShopDiscount> DiscountsToAdd = new List<ShopDiscount>();
    }
    public class ShopDiscount : MonoBehaviour
    {
        // The shop discount itself. This class controls how your discount should work, the price reduction amount and the purchase condition, and other things.
        /// <summary>
        /// The name of your discount. Mostly just for organization and other things.
        /// </summary>
        public string IdentificationKey = "ShopDisc";
        /// <summary>
        /// Price multipler, self explanatory. Set it to 0.5f and whatever items you set it to will be half price!
        /// </summary>
        public float PriceMultiplier = 1f;
        /// <summary>
        /// A function for the *validity* in which your discount will be active. Make sure to return it as TRUE when the item is valid to be discounted.
        /// </summary>
        public Func<ShopItemController, bool> ItemIsValidForDiscount;

        /// <summary>
        /// A function for your *condition* in which your discount will be active. Make sure to return it as TRUE when it should be active.
        /// </summary>
        public Func<bool> CanDiscountCondition;

        /// <summary>
        /// A function that lets you give a *custom* price multipler, for more dynamic price reductions.
        /// </summary>
        public Func<float> CustomPriceMultiplier;


        private bool OverridePriceReduction = false;
        /// <summary>
        /// Returns the current override value. Your discount will NOT be active while the override value is TRUE.
        /// </summary>
        public bool GetOverride() { return OverridePriceReduction; }
        /// <summary>
        /// Sets the override value. Your discount will NOT be active while the override value is TRUE.
        /// </summary>
        public void SetOverride(bool overrideType) { OverridePriceReduction = overrideType; }
        /// <summary>
        /// Returns TRUE if your discount is active.
        /// </summary>
        public bool CanBeDiscounted()
        {
            if (OverridePriceReduction == true) { return false; }
            if (CanDiscountCondition != null)
            {
                return CanDiscountCondition();
            }
            return false;
        }


        public float ReturnCustomPriceMultiplier()
        {
            if (CustomPriceMultiplier != null)
            {
                return CustomPriceMultiplier();
            }
            return PriceMultiplier;
        }
    }

    public class ShopDiscountController : MonoBehaviour
    {
        public ShopDiscountController()
        {
        }

        public void UpdatePlacement()
        {
            shopItemSelf = this.GetComponent<ShopItemController>();
            if (shopItemSelf != null)
            {
                shopItemSelf.StartCoroutine(FrameDelay());
            }
        }
        public IEnumerator FrameDelay()
        {
            yield return null;
            if (DoManyChecks() == true)
            {
                if (shopItemSelf is CustomShopItemController)
                {
                    StartPrice = shopItemSelf.OverridePrice ?? (shopItemSelf as CustomShopItemController).ModifiedPrice;//shopItemSelf.ModifiedPrice;
                }
                else
                {
                    StartPrice = shopItemSelf.OverridePrice ?? shopItemSelf.ModifiedPrice;
                }
            }
            FullyInited = true;
            yield break;
        }

        public void ResetPrice(int? currentOverridePrice)
        {
            if (shopItemSelf is CustomShopItemController)
            {
                StartPrice = currentOverridePrice ?? (shopItemSelf as CustomShopItemController).ModifiedPrice;//shopItemSelf.ModifiedPrice;
            }
            else
            {
                StartPrice = currentOverridePrice ?? shopItemSelf.ModifiedPrice;
            }
        }
        private bool FullyInited = false;
        private bool DoManyChecks()
        {
            if (GameManager.Instance == null) { return false; }
            if (GameManager.Instance.PrimaryPlayer == null) { return false; }
            return true;
        }

        private float StartPrice = -1;

        public void Update()
        {
            if (FullyInited == false) { return; }
            DoPriceReduction();
        }
        private void DoPriceReduction()
        {
            if (shopItemSelf == null) { return; }

            if (GameStatsManager.Instance != null)
            {
                //Payday item failsafes
                if (shopItemSelf.item is PaydayDrillItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_DRILL) == false) { return; } 
                if (shopItemSelf.item is BankMaskItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKMASK) == false) { return; }
                if (shopItemSelf.item is BankBagItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKBAG) == false) { return; }
            }

            float mult = 1;
            foreach (var DiscountVar in discounts)
            {
                if (Valid(DiscountVar) == true)
                {
                    if (DiscountVar.CanBeDiscounted() == true)
                    {
                        mult *= DiscountVar.ReturnCustomPriceMultiplier();
                    }
                }
            }
            DoTotalDiscount(mult);
        }


        public bool ReturnMoneyCurrencyType()
        {
            return shopItemSelf.CurrencyType == ShopItemController.ShopCurrencyType.COINS;
        }
        private void DoTotalDiscount(float H)
        {
            if (shopItemSelf == null) { return; }
            if (GameManager.Instance == null) { return; }
            if (GameManager.Instance.PrimaryPlayer == null) { return; }

            //GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
            float newCost = StartPrice != -1 ? StartPrice : ReturnMoneyCurrencyType() == false ? shopItemSelf.CurrentPrice : shopItemSelf.ModifiedPrice;
            //float num4 = (lastLoadedLevelDefinition == null) ? 1f : lastLoadedLevelDefinition.priceMultiplier;

            float num3 = GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier);

            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.SecondaryPlayer)
            {
                num3 *= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier);
            }
            newCost *= num3;
            shopItemSelf.OverridePrice = (int)(newCost *= H);
        }

        private void ReturnPriceToDefault()
        {
            if (shopItemSelf == null) { return; }
            GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance != null ? GameManager.Instance.GetLastLoadedLevelDefinition() : null;
            float newCost = StartPrice != -1 ? StartPrice : shopItemSelf.item.PurchasePrice;
            float num4 = (lastLoadedLevelDefinition == null) ? 1f : lastLoadedLevelDefinition.priceMultiplier;

            float num3 = GameManager.Instance.PrimaryPlayer != null ? GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier) : 1;
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.SecondaryPlayer)
            {
                num3 *= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.GlobalPriceMultiplier);
            }

            newCost *= num4 * num3;
            shopItemSelf.OverridePrice = (int)(newCost);
        }

        /// <summary>
        /// Sets the override for a ShopDiscount with a specific IdentificationKey.
        /// </summary>
        public void DisableSetShopDiscount(string stringID, bool b)
        {
            foreach (var DiscountVar in discounts)
            {
                if (DiscountVar.IdentificationKey == stringID) { DiscountVar.SetOverride(b); }
            }
        }
        /// <summary>
        /// Returns a ShopDiscount with a specific IdentificationKey.
        /// </summary>
        public ShopDiscount ReturnShopDiscountFromController(string IDTag)
        {
            foreach (var DiscountVar in discounts)
            {
                if (DiscountVar.IdentificationKey == IDTag) { return DiscountVar; }
            }
            return null;
        }

        private void OnDestroy()
        {
            if (shopItemSelf != null)
            {
                ReturnPriceToDefault();
            }
        }


        //checks if the item itself is valid in the first place
        private bool Valid(ShopDiscount shopDiscount)
        {
            if (shopItemSelf == null) { return false; }
            if (shopDiscount.ItemIsValidForDiscount != null) { return shopDiscount.ItemIsValidForDiscount(shopItemSelf); }
            return false;
        }

        public List<ShopDiscount> discounts = new List<ShopDiscount>();
        private ShopItemController shopItemSelf;
    }
}
