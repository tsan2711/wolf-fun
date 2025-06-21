using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopUIManager : MonoBehaviour, IUIManager
{
    [Header("Prefab References")]
    [SerializeField] private ShopUIItem shopItemPrefab;
    
    [Header("Container References")]
    public Transform ShopContainer;
    [SerializeField] private Transform seedShopContent;
    
    [Header("Icon References")]
    [SerializeField] private InventoryIconData iconData;

    private Dictionary<string, ShopUIItem> _shopItems = new Dictionary<string, ShopUIItem>();

    // Events for purchases
    public System.Action<CropType, int> OnSeedPurchaseRequested;
    public System.Action<AnimalType> OnAnimalPurchaseRequested;

    public void Initialize(GameConfig gameConfig)
    {
        CreateSeedShopItems(gameConfig);
        CreateAnimalShopItems(gameConfig);
    }

    private void CreateSeedShopItems(GameConfig gameConfig)
    {
        foreach (CropType cropType in System.Enum.GetValues(typeof(CropType)))
        {
            GameObject itemGO = Instantiate(shopItemPrefab.gameObject, seedShopContent);
            ShopUIItem item = itemGO.GetComponent<ShopUIItem>();
            
            Debug.Log("Creating shop item for crop type: " + cropType);

            string itemName = $"{cropType} Seeds";
            int price = gameConfig.GetSeedCost(cropType);
            Sprite icon = iconData.GetSeedIcon(cropType);
            
            // Setup buy action
            UnityEngine.Events.UnityAction buyAction = () => OnSeedPurchaseRequested?.Invoke(cropType, 1);
            item.Initialize(itemName, price, icon, buyAction);
            
            _shopItems[$"seed_{cropType}"] = item;
        }
    }

    private void CreateAnimalShopItems(GameConfig gameConfig)
    {
        foreach (AnimalType animalType in System.Enum.GetValues(typeof(AnimalType)))
        {
            GameObject itemGO = Instantiate(shopItemPrefab.gameObject, seedShopContent);
            ShopUIItem item = itemGO.GetComponent<ShopUIItem>();
            
            string itemName = animalType.ToString();
            int price = gameConfig.GetAnimalCost(animalType);
            Sprite icon = iconData.GetAnimalIcon(animalType);
            
            // Setup buy action
            UnityEngine.Events.UnityAction buyAction = () => OnAnimalPurchaseRequested?.Invoke(animalType);
            item.Initialize(itemName, price, icon, buyAction);
            
            _shopItems[$"animal_{animalType}"] = item;
        }
    }

    public void Activate(bool v)
    {
        ShopContainer.gameObject.SetActive(v);
    }
}
