using Alexandria.NPCAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
namespace Alexandria.ItemAPI
{
    public class FakePrefab : Component
    {
        internal static HashSet<GameObject> ExistingFakePrefabs = new HashSet<GameObject>();

        /// <summary>
        /// Checks if an object is marked as a fake prefab.
        /// </summary>
        /// <returns><c>true</c>, if object is in the list of fake prefabs, <c>false</c> otherwise.</returns>
        /// <param name="o">Unity object to test.</param>
        public static bool IsFakePrefab(UnityEngine.Object o)
        {
            if (o is GameObject)
            {
                return ExistingFakePrefabs.Contains((GameObject)o);
            }
            else if (o is Component)
            {
                return ExistingFakePrefabs.Contains(((Component)o).gameObject);
            }
            return false;
        }

        /// <summary>
        /// Marks an object as a fake prefab.
        /// </summary>
        /// <param name="obj">GameObject to add to the list.</param>
        public static void MarkAsFakePrefab(GameObject obj)
        {
            ExistingFakePrefabs.Add(obj);
            DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// Clones a real prefab or a fake prefab into a new fake prefab.
        /// </summary>
        /// <returns>The new game object.</returns>
        /// <param name="obj">GameObject to clone.</param>
        public static GameObject Clone(GameObject obj)
        {
            var already_fake = IsFakePrefab(obj);

            var was_active = obj.activeSelf;
            if (was_active)
                obj.SetActive(false);

            var fakeprefab = UnityEngine.Object.Instantiate(obj);

            if (was_active)
                obj.SetActive(true);

            ExistingFakePrefabs.Add(fakeprefab);
            DontDestroyOnLoad(fakeprefab);         
            return fakeprefab;
        }

        /// <summary>
        /// Activates objects that have been created from a fake prefab, otherwise simply returns them.
        /// </summary>
        /// <returns>The same Unity object as the one passed in <c>new_o</c>, activated if <c>o</c> is a fake prefab..</returns>
        /// <param name="o">Original object.</param>
        /// <param name="new_o">The object instantiated from the original object.</param>
        public static UnityEngine.Object Instantiate(UnityEngine.Object o, UnityEngine.Object new_o)
        {
            if (o is GameObject && ExistingFakePrefabs.Contains((GameObject)o))
            {
                ((GameObject)new_o).SetActive(true);
                if ((new_o as GameObject).GetComponent<CustomShopController>() != null)
                {
                    (new_o as GameObject).GetComponent<CustomShopController>().customCanBuy = (o as GameObject).GetComponent<CustomShopController>().customCanBuy;
                    (new_o as GameObject).GetComponent<CustomShopController>().customPrice = (o as GameObject).GetComponent<CustomShopController>().customPrice;
                    (new_o as GameObject).GetComponent<CustomShopController>().removeCurrency = (o as GameObject).GetComponent<CustomShopController>().removeCurrency;
                    (new_o as GameObject).GetComponent<CustomShopController>().OnSteal = (o as GameObject).GetComponent<CustomShopController>().OnSteal;
                    (new_o as GameObject).GetComponent<CustomShopController>().OnPurchase = (o as GameObject).GetComponent<CustomShopController>().OnPurchase;
                    //(new_o as GameObject).GetComponent<CustomShopController>().customCurrencyAtlas = (o as GameObject).GetComponent<CustomShopController>().customCurrencyAtlas;
                }
            }
            else if (o is Component && ExistingFakePrefabs.Contains(((Component)o).gameObject))
            {
                ((Component)new_o).gameObject.SetActive(true);
                if ((new_o is CustomShopController))
                {
                    (new_o as CustomShopController).customCanBuy = (o as CustomShopController).customCanBuy;
                    (new_o as CustomShopController).customPrice = (o as CustomShopController).customPrice;
                    (new_o as CustomShopController).removeCurrency = (o as CustomShopController).removeCurrency;
                    (new_o as GameObject).GetComponent<CustomShopController>().OnSteal = (o as GameObject).GetComponent<CustomShopController>().OnSteal;
                    (new_o as GameObject).GetComponent<CustomShopController>().OnPurchase = (o as GameObject).GetComponent<CustomShopController>().OnPurchase;
                    //(new_o as CustomShopController).customCurrencyAtlas = (o as CustomShopController).customCurrencyAtlas;
                }


            }


            return new_o;
        }

        /// <summary>
        /// Instantiates a copy of the given gameobject, marks it as a fake prefab, prevents it from being destroyed on load, and sets it as inactive. Returns the copy.
        /// </summary>
        /// <param name="target">The gameobject to be instantiated and fakeprefabbed.</param>
        public static GameObject InstantiateAndFakeprefab( GameObject target)
        {
            GameObject instantiatedTarget = UnityEngine.Object.Instantiate<GameObject>(target);
            instantiatedTarget.SetActive(false);
            FakePrefab.MarkAsFakePrefab(instantiatedTarget);
            UnityEngine.Object.DontDestroyOnLoad(instantiatedTarget);
            return instantiatedTarget;
        }

        /// <summary>
        /// Marks the given object as a Fake Prefab, prevents it from despawning on load, and sets it as inactive.
        /// </summary>
        /// <param name="target">The gameobject to be fakeprefabbed.</param>
        public static void MakeFakePrefab( GameObject target)
        {
            target.SetActive(false);
            FakePrefab.MarkAsFakePrefab(target);
            UnityEngine.Object.DontDestroyOnLoad(target);
        }
    }
    public static class FakePrefabExtensions
    {
        /// <summary>
        /// Marks the given object as a Fake Prefab, prevents it from despawning on load, and sets it as inactive.
        /// </summary>
        /// <param name="target">The gameobject to be fakeprefabbed.</param>
        public static void MakeFakePrefab(this GameObject target) { FakePrefab.MakeFakePrefab(target); }

        /// <summary>
        /// Instantiates a copy of the given gameobject, marks it as a fake prefab, prevents it from being destroyed on load, and sets it as inactive. Returns the copy.
        /// </summary>
        /// <param name="target">The gameobject to be instantiated and fakeprefabbed.</param>
        public static GameObject InstantiateAndFakeprefab(this GameObject target) { return FakePrefab.InstantiateAndFakeprefab(target); }
    }
}
