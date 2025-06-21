using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEditor;

public class UpgradeUIManager : MonoBehaviour, IUIManager
{
    [Header("Prefab References")]
    [SerializeField] private UpgradeUIItem upgradeItemPrefab;

    [Header("Container References")]
    public Transform UpgradeContainer;
    [SerializeField] private Transform upgradesContent;

    [Header("Icon References")]
    [SerializeField] private InventoryIconData iconData;

    private Dictionary<string, UpgradeUIItem> _upgradeItems = new Dictionary<string, UpgradeUIItem>();

    public Action<ProductType> OnEquipmentUpgradeRequested;

    public void Initialize(GameConfig gameConfig)
    {
        CreateUpgradeItems(gameConfig);
    }

    private void CreateUpgradeItems(GameConfig gameConfig)
    {
        // Equipment upgrade
        CreateUpgradeItem("Strawberry", gameConfig.EquipmentUpgradeCost, iconData.StrawberryUpgradeIcon, ProductType.Strawberry,
            (productType) => OnEquipmentUpgradeRequested?.Invoke(productType));

        // Worker hiring
        CreateUpgradeItem("Blueberry", gameConfig.EquipmentUpgradeCost, iconData.BlueberryUpgradeIcon, ProductType.Blueberry,
            (productType) => OnEquipmentUpgradeRequested?.Invoke(productType));

        // Plot expansion
        CreateUpgradeItem("Tomato", gameConfig.EquipmentUpgradeCost, iconData.TomatoUpgradeIcon, ProductType.Tomato,
            (productType) => OnEquipmentUpgradeRequested?.Invoke(productType));

        CreateUpgradeItem("Cow", gameConfig.EquipmentUpgradeCost, iconData.CowUpgradeIcon, ProductType.Milk,
            (productType) => OnEquipmentUpgradeRequested?.Invoke(productType));
    }

    private void CreateUpgradeItem(string upgradeName, int cost, Sprite icon, ProductType productType, Action<ProductType> onUpgrade)
    {
        GameObject itemGO = Instantiate(upgradeItemPrefab.gameObject, upgradesContent);
        UpgradeUIItem item = itemGO.GetComponent<UpgradeUIItem>();

        item.Initialize(upgradeName, cost, productType, icon, (_productType) => onUpgrade?.Invoke(_productType));
        _upgradeItems[upgradeName.ToLower().Replace(" ", "_")] = item;
    }

    public void Activate(bool v)
    {
        UpgradeContainer.gameObject.SetActive(v);
    }

    public void UpdateUpgradeLevels(Dictionary<ProductType, int> upgradeLevels)
    {
        Debug.Log("Updating upgrade levels: " + upgradeLevels.Count);
        foreach (var kvp in upgradeLevels)
        {
            foreach (UpgradeUIItem item in _upgradeItems.Values)
            {
                if (item.ProductType != kvp.Key) continue;

                item.LevelText.text = $"Level: {kvp.Value}";
                item.UpgradeNameText.text = kvp.Key.ToString();
                break;
            }
        }
    }
}
