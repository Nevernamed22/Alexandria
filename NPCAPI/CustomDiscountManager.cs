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
            if (shopItemController.gameObject.GetComponent<ShopDiscountController>() != null) { UnityEngine.Object.Destroy(shopItemController.gameObject.GetComponent<ShopDiscountController>()); }
            ShopDiscountController steamSale = shopItemController.gameObject.AddComponent<ShopDiscountController>();
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
            if (isCompleteOverrideCost == true) { return false; }

            if (CanDiscountCondition != null)
            {
                return CanDiscountCondition();
            }
            return true;
        }

        public int ReturnCustomOverrideCost()
        {
            if (CustomCostModifier != null)
            {
                return CustomCostModifier();
            }
            return CustomCost;
        }

        public float ReturnCustomPriceMultiplier()
        {
            if (CustomPriceMultiplier != null)
            {
                return CustomPriceMultiplier();
            }
            return PriceMultiplier;
        }
        public bool isCompleteOverrideCost = false;
        public int CustomCost = -1;
        public Func<int> CustomCostModifier;

    }

    public class ShopDiscountController : MonoBehaviour
    {
        public ShopDiscountController()
        {
        }

        public void UpdatePlacement()
        {
            shopItemSelf = this.GetComponent<ShopItemController>();
        }


        public List<ShopDiscount> localDiscounts = new List<ShopDiscount>();



        private bool FullyInited = false;
        private bool DoManyChecks()
        {
            if (GameManager.Instance == null) { return false; }
            if (GameManager.Instance.PrimaryPlayer == null) { return false; }
            return true;
        }

        public void Update()
        {
            if (FullyInited == false) { return; }
            DoPriceReduction();
        }

        public int DoPriceOverride()
        {
            if (shopItemSelf == null) { return -1; }
            if (GameStatsManager.Instance != null)
            {
                //Payday item failsafes
                if (shopItemSelf.item is PaydayDrillItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_DRILL) == false) { return -1; }
                if (shopItemSelf.item is BankMaskItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKMASK) == false) { return -1; }
                if (shopItemSelf.item is BankBagItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKBAG) == false) { return -1; }
            }
            int Cost = 1;
            if (discounts.Where(self => self.isCompleteOverrideCost).Count() == 0 && localDiscounts.Where(self => self.isCompleteOverrideCost).Count() == 0) { return -1; }

            foreach (var DiscountVar in discounts)
            {
                if (Valid(DiscountVar) == true)
                {
                    if (DiscountVar.CanBeDiscounted() == true && DiscountVar.isCompleteOverrideCost)
                    {
                        Cost += DiscountVar.ReturnCustomOverrideCost();
                    }
                }
            }
            if (discounts.Count > 1)
            {
                Cost /= discounts.Count;
            }
            foreach (var DiscountVar in localDiscounts)
            {
                if (Valid(DiscountVar) == true)
                {
                    if (DiscountVar.CanBeDiscounted() == true && DiscountVar.isCompleteOverrideCost)
                    {
                        Cost += DiscountVar.ReturnCustomOverrideCost();
                    }
                }
            }
            if (localDiscounts.Count > 1)
            {
                Cost /= localDiscounts.Count;
            }
            return Cost;
        }


        public float DoPriceReduction()
        {
            if (shopItemSelf == null) { return 1; }
            if (GameStatsManager.Instance != null)
            {
                //Payday item failsafes
                if (shopItemSelf.item is PaydayDrillItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_DRILL) == false) { return 1; } 
                if (shopItemSelf.item is BankMaskItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKMASK) == false) { return 1; }
                if (shopItemSelf.item is BankBagItem && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_STOLE_BANKBAG) == false) { return 1; }
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
            foreach (var DiscountVar in localDiscounts)
            {
                if (Valid(DiscountVar) == true)
                {
                    if (DiscountVar.CanBeDiscounted() == true)
                    {
                        mult *= DiscountVar.ReturnCustomPriceMultiplier();
                    }
                }
            }
            return mult;
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




        //checks if the item itself is valid in the first place
        private bool Valid(ShopDiscount shopDiscount)
        {
            if (shopItemSelf == null) { return false; }
            if (shopDiscount.ItemIsValidForDiscount != null) { return shopDiscount.ItemIsValidForDiscount(shopItemSelf); }
            return true;
        }


        public ShopItemController ReturnShopItemController()
        {
            return shopItemSelf;
        }

        public List<ShopDiscount> discounts = new List<ShopDiscount>();
        private ShopItemController shopItemSelf;
    }
}
