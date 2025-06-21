using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEditor.ShaderGraph.Serialization;
using System;

public class MainGameUI : MonoBehaviour, IGameUI
{
    [Header("UI Managers")]
    [SerializeField] private InventoryUIManager inventoryUIManager;
    [SerializeField] private ShopUIManager shopUIManager;
    [SerializeField] private UpgradeUIManager upgradeUIManager;
    [SerializeField] private WorkerUIManager workerUIManager;

    [Header("Basic UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI workersText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messagePanel;

    [Header("Action Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button autoHarvestButton;
    [SerializeField] private Button autoPlantTomatoButton;
    [SerializeField] private Button autoPlantBlueberryButton;
    [SerializeField] private Button autoPlantStrawberryButton;  // THÊM MỚI
    [SerializeField] private Button autoPlaceCowButton;        // THÊM MỚI
    [SerializeField] private Button expandPlotButton;

    [Header("Navigation Buttons")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button workerButton;

    private Color _defaultButtonColor = Color.gray;
    private Color _highlightedButtonColor = new Color(0.8f, 0.8f, 0.2f);

    // Events from IGameUI interface
    public event Action NewGameRequested;
    public event Action ContinueGameRequested;
    public event Action<CropType, int> BuySeedsRequested;
    public event Action<AnimalType> BuyAnimalRequested;
    public event Action BuyWorkerRequested;
    public event Action BuyPlotRequested;
    public event Action UpgradeEquipmentRequested;
    public event Action AutoHarvestRequested;
    public event Action<CropType> AutoPlantRequested;
    public event Action<AnimalType> AutoPlaceAnimalRequested;  // THÊM MỚI
    public event Action<ProductType, int> SellProductRequested;

    private void Start()
    {
        InitializeManagers();
        ConnectEvents();
        SetupButtons();

        if (messagePanel != null)
            messagePanel.SetActive(false);

        FakeButtonSetup();
    }

    private void FakeButtonSetup()
    {
        upgradeButton.onClick.Invoke();
        inventoryButton.onClick.Invoke();
    }

    private void InitializeManagers()
    {
        var gameConfig = new GameConfig();

        inventoryUIManager?.Initialize();
        shopUIManager?.Initialize(gameConfig);
        upgradeUIManager?.Initialize(gameConfig);
        workerUIManager?.Initialize();
    }

    private void ConnectEvents()
    {
        // Connect shop manager events
        if (shopUIManager != null)
        {
            shopUIManager.OnSeedPurchaseRequested += (cropType, amount) => BuySeedsRequested?.Invoke(cropType, amount);
            shopUIManager.OnAnimalPurchaseRequested += (animalType) => BuyAnimalRequested?.Invoke(animalType);
        }

        // Connect upgrade manager events
        if (upgradeUIManager != null)
        {
            upgradeUIManager.OnEquipmentUpgradeRequested += () => UpgradeEquipmentRequested?.Invoke();
        }

        // Connect inventory manager events (for selling)
        if (inventoryUIManager != null)
        {
            inventoryUIManager.OnSellProductRequested += (productType, amount) => SellProductRequested?.Invoke(productType, amount);
        }

        if( workerUIManager != null)
        {
            workerUIManager.OnHireWorkerRequested += () => BuyWorkerRequested?.Invoke();
        }
    }

    private void SetupButtons()
    {
        // Main menu buttons
        newGameButton?.onClick.AddListener(() => NewGameRequested?.Invoke());
        continueGameButton?.onClick.AddListener(() => ContinueGameRequested?.Invoke());

        // Action buttons
        autoHarvestButton?.onClick.AddListener(() => AutoHarvestRequested?.Invoke());
        autoPlantTomatoButton?.onClick.AddListener(() => AutoPlantRequested?.Invoke(CropType.Tomato));
        autoPlantBlueberryButton?.onClick.AddListener(() => AutoPlantRequested?.Invoke(CropType.Blueberry));
        autoPlantStrawberryButton?.onClick.AddListener(() => AutoPlantRequested?.Invoke(CropType.Strawberry));  // THÊM MỚI
        autoPlaceCowButton?.onClick.AddListener(() => AutoPlaceAnimalRequested?.Invoke(AnimalType.Cow));        // THÊM MỚI

        // Navigation buttons
        inventoryButton?.onClick.AddListener(() =>
        {
            inventoryButton.GetComponent<Image>().color = _highlightedButtonColor;
            workerButton.GetComponent<Image>().color = _defaultButtonColor;
            inventoryUIManager?.Activate(true);
            workerUIManager?.Activate(false);
        });
        shopButton?.onClick.AddListener(() =>
        {
            shopButton.GetComponent<Image>().color = _highlightedButtonColor;
            upgradeButton.GetComponent<Image>().color = _defaultButtonColor;
            shopUIManager?.Activate(true);
            upgradeUIManager?.Activate(false);
        });
        upgradeButton?.onClick.AddListener(() =>
        {
            upgradeButton.GetComponent<Image>().color = _highlightedButtonColor;
            shopButton.GetComponent<Image>().color = _defaultButtonColor;
            upgradeUIManager?.Activate(true);
            shopUIManager?.Activate(false);
        });
        workerButton?.onClick.AddListener(() =>
        {
            workerButton.GetComponent<Image>().color = _highlightedButtonColor;
            inventoryButton.GetComponent<Image>().color = _defaultButtonColor;
            workerUIManager?.Activate(true);
            inventoryUIManager?.Activate(false);
        });

        expandPlotButton?.onClick.AddListener(() => BuyPlotRequested?.Invoke());
    }

    public void UpdateFarmData(Farm farm)
    {
        UpdateBasicStats(farm);
        UpdateInventory(farm);
        UpdateWorkers(farm);
    }

    private void UpdateBasicStats(Farm farm)
    {
        if (goldText != null)
            goldText.text = $"Gold: {farm.Gold}";

        if (workersText != null)
            workersText.text = $"Workers: {farm.GetAvailableWorkers()}/{farm.GetTotalWorkers()}";
    }

    private void UpdateInventory(Farm farm)
    {
        if (inventoryUIManager != null)
        {
            var seeds = new Dictionary<CropType, int>
        {
            { CropType.Tomato, farm.Inventory.GetSeedCount(CropType.Tomato) },
            { CropType.Blueberry, farm.Inventory.GetSeedCount(CropType.Blueberry) },
            { CropType.Strawberry, farm.Inventory.GetSeedCount(CropType.Strawberry) }
        };

            var animals = new Dictionary<AnimalType, int>  // THÊM MỚI
        {
            { AnimalType.Cow, farm.Inventory.GetAnimalCount(AnimalType.Cow) }
        };

            var products = new Dictionary<ProductType, int>
        {
            { ProductType.Tomato, farm.Inventory.GetProductCount(ProductType.Tomato) },
            { ProductType.Blueberry, farm.Inventory.GetProductCount(ProductType.Blueberry) },
            { ProductType.Strawberry, farm.Inventory.GetProductCount(ProductType.Strawberry) },
            { ProductType.Milk, farm.Inventory.GetProductCount(ProductType.Milk) }
        };

            // CẬP NHẬT CALL - thêm animals parameter
            inventoryUIManager.UpdateInventory(seeds, products, animals);
        }
    }
    private void UpdateWorkers(Farm farm)
    {
        workerUIManager?.UpdateWorkers(farm.GetWorkers());
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
            Invoke(nameof(HideMessage), 3f);
        }
        else
        {
            Debug.Log($"[GAME MESSAGE] {message}");
        }
    }

    private void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
}