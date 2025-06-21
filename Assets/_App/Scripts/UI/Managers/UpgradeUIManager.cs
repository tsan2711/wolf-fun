using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private UpgradeUIItem upgradeItemPrefab;
    
    [Header("Container References")]
    public Transform UpgradeContainer;
    [SerializeField] private Transform upgradesContent;
    
    [Header("Icon References")]
    [SerializeField] private InventoryIconData iconData;

    private Dictionary<string, UpgradeUIItem> _upgradeItems = new Dictionary<string, UpgradeUIItem>();

    public Action OnEquipmentUpgradeRequested;

    public void Initialize(GameConfig gameConfig)
    {
        CreateUpgradeItems(gameConfig);
    }

    private void CreateUpgradeItems(GameConfig gameConfig)
    {
        // Equipment upgrade
        CreateUpgradeItem("Strawberry", gameConfig.EquipmentUpgradeCost, iconData.StrawberryUpgradeIcon,
            () => OnEquipmentUpgradeRequested?.Invoke());

        // Worker hiring
        CreateUpgradeItem("Blueberry", gameConfig.EquipmentUpgradeCost, iconData.BlueberryUpgradeIcon,
            () => OnEquipmentUpgradeRequested?.Invoke());

        // Plot expansion
        CreateUpgradeItem("Tomato", gameConfig.EquipmentUpgradeCost, iconData.TomatoUpgradeIcon,
            () => OnEquipmentUpgradeRequested?.Invoke());
            
        CreateUpgradeItem("Cow", gameConfig.EquipmentUpgradeCost, iconData.CowUpgradeIcon,
            () => OnEquipmentUpgradeRequested?.Invoke());
    }

    private void CreateUpgradeItem(string upgradeName, int cost, Sprite icon, System.Action onUpgrade)
    {
        GameObject itemGO = Instantiate(upgradeItemPrefab.gameObject, upgradesContent);
        UpgradeUIItem item = itemGO.GetComponent<UpgradeUIItem>();
        
        item.Initialize(upgradeName, cost, icon);
        
        // Find and setup button (assuming there's a Button component)
        Button upgradeButton = item.GetComponentInChildren<Button>();
        upgradeButton?.onClick.AddListener(() => onUpgrade?.Invoke());
        
        _upgradeItems[upgradeName.ToLower().Replace(" ", "_")] = item;
    }

    public void Activate(bool v)
    {
        UpgradeContainer.gameObject.SetActive(v);
    }
}
