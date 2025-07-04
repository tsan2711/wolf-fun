using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryUIManager : MonoBehaviour, IUIManager
{
    [Header("Prefab References")]
    [SerializeField] private InventoryUIItem inventoryItemPrefab;

    [Header("Container References")]
    public Transform InventoryContainer;
    [SerializeField] private Transform seedsContent;
    [SerializeField] private Transform productsContent;

    [Header("Icon References")]
    [SerializeField] private InventoryIconData iconData;

    private Dictionary<string, InventoryUIItem> _seedItems = new Dictionary<string, InventoryUIItem>();
    private Dictionary<string, InventoryUIItem> _animalItems = new Dictionary<string, InventoryUIItem>();  // THÊM MỚI
    private Dictionary<string, InventoryUIItem> _productItems = new Dictionary<string, InventoryUIItem>();

    public Action<ProductType, int> OnSellProductRequested;

    public void Initialize()
    {
        CreateSeedItems();
        CreateAnimalItems();
        CreateProductItems();
    }

    private void CreateSeedItems()
    {
        foreach (CropType cropType in Enum.GetValues(typeof(CropType)))
        {
            GameObject itemGO = Instantiate(inventoryItemPrefab.gameObject, seedsContent);
            InventoryUIItem item = itemGO.GetComponent<InventoryUIItem>();

            string itemName = $"{cropType} Seeds";
            Sprite icon = iconData.GetSeedIcon(cropType);

            item.Initialize(itemName, 0, icon, null);
            _seedItems[cropType.ToString()] = item;
        }
    }

    private void CreateAnimalItems()
    {
        foreach (AnimalType animalType in System.Enum.GetValues(typeof(AnimalType)))
        {
            GameObject itemGO = Instantiate(inventoryItemPrefab.gameObject, seedsContent);
            InventoryUIItem item = itemGO.GetComponent<InventoryUIItem>();

            string itemName = animalType.ToString();
            Sprite icon = iconData.GetAnimalIcon(animalType);

            item.Initialize(itemName, 0, icon, null);
            _animalItems[animalType.ToString()] = item;
        }
    }

    private void CreateProductItems()
    {
        foreach (ProductType productType in System.Enum.GetValues(typeof(ProductType)))
        {
            if (productType == ProductType.None) continue;

            GameObject itemGO = Instantiate(inventoryItemPrefab.gameObject, productsContent);
            InventoryUIItem item = itemGO.GetComponent<InventoryUIItem>();

            string itemName = productType.ToString();
            Sprite icon = iconData.GetProductIcon(productType);

            UnityEngine.Events.UnityAction sellAction = () => OnSellProductRequested?.Invoke(productType, 1);
            item.Initialize(itemName, 0, icon, sellAction);
            _productItems[productType.ToString()] = item;
        }
    }

    public void UpdateInventory(Dictionary<CropType, int> seeds, Dictionary<ProductType, int> products, Dictionary<AnimalType, int> animals = null)
    {
        foreach (var kvp in seeds)
        {
            string key = kvp.Key.ToString();
            if (_seedItems.ContainsKey(key))
            {
                string itemName = $"{kvp.Key} Seeds";
                Sprite icon = iconData.GetSeedIcon(kvp.Key);
                _seedItems[key].Initialize(itemName, kvp.Value, icon, null);
            }
        }

        if (animals != null)
        {
            foreach (var kvp in animals)
            {
                string key = kvp.Key.ToString();
                if (_animalItems.ContainsKey(key))
                {
                    string itemName = kvp.Key.ToString();
                    Sprite icon = iconData.GetAnimalIcon(kvp.Key);
                    _animalItems[key].Initialize(itemName, kvp.Value, icon, null);
                }
            }
        }

        foreach (var kvp in products)
        {
            string key = kvp.Key.ToString();
            if (_productItems.ContainsKey(key))
            {
                string itemName = kvp.Key.ToString();
                Sprite icon = iconData.GetProductIcon(kvp.Key);
                UnityEngine.Events.UnityAction sellAction = () => OnSellProductRequested?.Invoke(kvp.Key, 1);
                _productItems[key].Initialize(itemName, kvp.Value, icon, sellAction);
            }
        }
    }

    public void Activate(bool v) => InventoryContainer.gameObject.SetActive(v);
}