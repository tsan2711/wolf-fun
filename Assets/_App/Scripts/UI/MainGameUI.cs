using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

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

    [Header("Action Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button autoHarvestButton;
    [SerializeField] private Button autoPlantTomatoButton;
    [SerializeField] private Button autoPlantBlueberryButton;
    [SerializeField] private Button autoPlantStrawberryButton;
    [SerializeField] private Button autoPlaceCowButton;
    [SerializeField] private Button expandPlotButton;

    [Header("Navigation Buttons")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button workerButton;

    [Header("Message System")]
    [SerializeField] private MessageSystem messageSystem;

    [Header("Button Animation Settings")]
    [SerializeField] private float buttonClickScale = 0.9f;
    [SerializeField] private float buttonClickDuration = 0.1f;
    [SerializeField] private float buttonHoverScale = 1.05f;
    [SerializeField] private float buttonColorTransitionDuration = 0.2f;

    [Header("Side Panel Settings")]
    [SerializeField] private RectTransform leftSidePanel;
    [SerializeField] private RectTransform rightSidePanel;
    [SerializeField] private Button leftSideToggleButton;
    [SerializeField] private Button rightSideToggleButton;
    [SerializeField] private float sidePanelAnimationDuration = 0.5f;
    [SerializeField] private float sidePanelOffset = 500f;

    private Color _defaultButtonColor = Color.gray;
    private Color _highlightedButtonColor = new Color(0.8f, 0.8f, 0.2f);

    // Side panel state and original positions
    private bool isLeftPanelOpen = true;
    private bool isRightPanelOpen = true;
    private Vector3 leftPanelOriginalPos;
    private Vector3 rightPanelOriginalPos;
    private bool isLeftAnimating = false;
    private bool isRightAnimating = false;

    // Events from IGameUI interface
    public event Action NewGameRequested;
    public event Action ContinueGameRequested;
    public event Action<CropType, int> BuySeedsRequested;
    public event Action<AnimalType> BuyAnimalRequested;
    public event Action BuyWorkerRequested;
    public event Action BuyPlotRequested;
    public event Action<ProductType> UpgradeEquipmentRequested;
    public event Action AutoHarvestRequested;
    public event Action<CropType> AutoPlantRequested;
    public event Action<AnimalType> AutoPlaceAnimalRequested;
    public event Action<ProductType, int> SellProductRequested;

    private void Start()
    {
        SetupMessageSystem();
        InitializeSidePanels();
        InitializeManagers();
        ConnectEvents();
        SetupButtons();
        FakeButtonSetup();
    }

    private void SetupMessageSystem()
    {
        if (messageSystem != null) return;
        messageSystem = FindAnyObjectByType<MessageSystem>();
    }

    private void InitializeSidePanels()
    {
        // Store original positions
        if (leftSidePanel != null)
        {
            leftPanelOriginalPos = leftSidePanel.anchoredPosition;
        }

        if (rightSidePanel != null)
        {
            rightPanelOriginalPos = rightSidePanel.anchoredPosition;
        }

        // Setup toggle buttons
        SetupSidePanelButtons();
    }

    private void SetupSidePanelButtons()
    {
        // Left side toggle button
        if (leftSideToggleButton != null)
        {
            leftSideToggleButton.onClick.AddListener(ToggleLeftPanel);
            SetupButtonWithClickAnimation(leftSideToggleButton, null); // Animation only, action handled above
        }

        // Right side toggle button
        if (rightSideToggleButton != null)
        {
            rightSideToggleButton.onClick.AddListener(ToggleRightPanel);
            SetupButtonWithClickAnimation(rightSideToggleButton, null); // Animation only, action handled above
        }
    }

    public void ToggleLeftPanel()
    {
        if (isLeftAnimating || leftSidePanel == null) return;

        isLeftAnimating = true;
        isLeftPanelOpen = !isLeftPanelOpen;

        Vector3 targetPosition;
        if (isLeftPanelOpen)
        {
            // Show panel - move to original position
            targetPosition = leftPanelOriginalPos;
        }
        else
        {
            // Hide panel - move left by offset
            targetPosition = leftPanelOriginalPos + Vector3.left * sidePanelOffset;
        }

        // Animate panel movement
        leftSidePanel.DOAnchorPos(targetPosition, sidePanelAnimationDuration)
                     .SetEase(Ease.OutQuart)
                     .OnComplete(() => {
                         isLeftAnimating = false;
                         Debug.Log($"Left panel {(isLeftPanelOpen ? "opened" : "closed")}");
                     });

        // Animate toggle button (optional rotation or scale effect)
        if (leftSideToggleButton != null)
        {
            AnimateToggleButton(leftSideToggleButton, isLeftPanelOpen);
        }
    }

    public void ToggleRightPanel()
    {
        if (isRightAnimating || rightSidePanel == null) return;

        isRightAnimating = true;
        isRightPanelOpen = !isRightPanelOpen;

        Vector3 targetPosition;
        if (isRightPanelOpen)
        {
            // Show panel - move to original position
            targetPosition = rightPanelOriginalPos;
        }
        else
        {
            // Hide panel - move right by offset
            targetPosition = rightPanelOriginalPos + Vector3.right * sidePanelOffset;
        }

        // Animate panel movement
        rightSidePanel.DOAnchorPos(targetPosition, sidePanelAnimationDuration)
                      .SetEase(Ease.OutQuart)
                      .OnComplete(() => {
                          isRightAnimating = false;
                          Debug.Log($"Right panel {(isRightPanelOpen ? "opened" : "closed")}");
                      });

        // Animate toggle button (optional rotation or scale effect)
        if (rightSideToggleButton != null)
        {
            AnimateToggleButton(rightSideToggleButton, isRightPanelOpen);
        }
    }

    private void AnimateToggleButton(Button toggleButton, bool isPanelOpen)
    {
        // Rotate button to indicate state (arrow direction)
        float targetRotation = isPanelOpen ? 0f : 0f;
        
        toggleButton.transform.DORotate(new Vector3(0, 0, targetRotation), sidePanelAnimationDuration * 0.5f)
                    .SetEase(Ease.OutQuart);

        // Scale punch effect
        toggleButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f);
    }

    // Public methods for external control
    public void SetLeftPanelState(bool open, bool animate = true)
    {
        if (leftSidePanel == null || isLeftPanelOpen == open) return;

        if (animate)
        {
            ToggleLeftPanel();
        }
        else
        {
            isLeftPanelOpen = open;
            Vector3 targetPosition = open ? leftPanelOriginalPos : leftPanelOriginalPos + Vector3.left * sidePanelOffset;
            leftSidePanel.anchoredPosition = targetPosition;
        }
    }

    public void SetRightPanelState(bool open, bool animate = true)
    {
        if (rightSidePanel == null || isRightPanelOpen == open) return;

        if (animate)
        {
            ToggleRightPanel();
        }
        else
        {
            isRightPanelOpen = open;
            Vector3 targetPosition = open ? rightPanelOriginalPos : rightPanelOriginalPos + Vector3.right * sidePanelOffset;
            rightSidePanel.anchoredPosition = targetPosition;
        }
    }

    // Getters for panel states
    public bool IsLeftPanelOpen => isLeftPanelOpen;
    public bool IsRightPanelOpen => isRightPanelOpen;

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
        if (shopUIManager != null)
        {
            shopUIManager.OnSeedPurchaseRequested += (cropType, amount) => BuySeedsRequested?.Invoke(cropType, amount);
            shopUIManager.OnAnimalPurchaseRequested += (animalType) => BuyAnimalRequested?.Invoke(animalType);
        }

        if (upgradeUIManager != null)
        {
            upgradeUIManager.OnEquipmentUpgradeRequested += (productType) => UpgradeEquipmentRequested?.Invoke(productType);
        }

        if (inventoryUIManager != null)
        {
            inventoryUIManager.OnSellProductRequested += (productType, amount) => SellProductRequested?.Invoke(productType, amount);
        }

        if (workerUIManager != null)
        {
            workerUIManager.OnHireWorkerRequested += () => BuyWorkerRequested?.Invoke();
        }
    }

    private void SetupButtons()
    {
        // Main menu buttons
        SetupButtonWithClickAnimation(newGameButton, () => NewGameRequested?.Invoke());
        SetupButtonWithClickAnimation(continueGameButton, () => ContinueGameRequested?.Invoke());

        // Action buttons
        SetupButtonWithClickAnimation(autoHarvestButton, () => AutoHarvestRequested?.Invoke());
        SetupButtonWithClickAnimation(autoPlantTomatoButton, () => AutoPlantRequested?.Invoke(CropType.Tomato));
        SetupButtonWithClickAnimation(autoPlantBlueberryButton, () => AutoPlantRequested?.Invoke(CropType.Blueberry));
        SetupButtonWithClickAnimation(autoPlantStrawberryButton, () => AutoPlantRequested?.Invoke(CropType.Strawberry));
        SetupButtonWithClickAnimation(autoPlaceCowButton, () => AutoPlaceAnimalRequested?.Invoke(AnimalType.Cow));
        SetupButtonWithClickAnimation(expandPlotButton, () => BuyPlotRequested?.Invoke());

        // Navigation buttons
        inventoryButton?.onClick.AddListener(() =>
        {
            AnimateNavigationButtonColors(inventoryButton, workerButton);
            inventoryUIManager?.Activate(true);
            workerUIManager?.Activate(false);
        });

        shopButton?.onClick.AddListener(() =>
        {
            AnimateNavigationButtonColors(shopButton, upgradeButton);
            shopUIManager?.Activate(true);
            upgradeUIManager?.Activate(false);
        });

        upgradeButton?.onClick.AddListener(() =>
        {
            AnimateNavigationButtonColors(upgradeButton, shopButton);
            upgradeUIManager?.Activate(true);
            shopUIManager?.Activate(false);
        });

        workerButton?.onClick.AddListener(() =>
        {
            AnimateNavigationButtonColors(workerButton, inventoryButton);
            workerUIManager?.Activate(true);
            inventoryUIManager?.Activate(false);
        });

        // Add hover effects to navigation buttons
        AddButtonHoverEffects(inventoryButton);
        AddButtonHoverEffects(shopButton);
        AddButtonHoverEffects(upgradeButton);
        AddButtonHoverEffects(workerButton);
    }

    private void SetupButtonWithClickAnimation(Button button, System.Action action)
    {
        if (button == null) return;

        // Only add click listener if action is provided
        if (action != null)
        {
            button.onClick.AddListener(() =>
            {
                AnimateButtonClick(button);
                action.Invoke();
            });
        }

        // Always add hover effects
        AddButtonHoverEffects(button);
    }

    private void AnimateButtonClick(Button button)
    {
        // Simple click scale animation
        button.transform.DOScale(buttonClickScale, buttonClickDuration)
                       .SetEase(Ease.OutQuad)
                       .OnComplete(() =>
                       {
                           button.transform.DOScale(1f, buttonClickDuration)
                                          .SetEase(Ease.OutQuad);
                       });
    }

    private void AddButtonHoverEffects(Button button)
    {
        if (button == null) return;

        // Add EventTrigger component if not present
        var eventTrigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        // Hover enter
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) =>
        {
            if (button.interactable)
            {
                button.transform.DOScale(buttonHoverScale, 0.1f).SetEase(Ease.OutQuad);
            }
        });
        eventTrigger.triggers.Add(pointerEnter);

        // Hover exit
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) =>
        {
            button.transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
        });
        eventTrigger.triggers.Add(pointerExit);

        // Pointer down (press)
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) =>
        {
            if (button.interactable)
            {
                button.transform.DOScale(buttonClickScale, 0.05f).SetEase(Ease.OutQuad);
            }
        });
        eventTrigger.triggers.Add(pointerDown);

        // Pointer up (release)
        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) =>
        {
            button.transform.DOScale(buttonHoverScale, 0.05f).SetEase(Ease.OutQuad);
        });
        eventTrigger.triggers.Add(pointerUp);
    }

    private void AnimateNavigationButtonColors(Button activeButton, Button inactiveButton)
    {
        var activeImage = activeButton.GetComponent<Image>();
        var inactiveImage = inactiveButton.GetComponent<Image>();

        // Simple color transitions
        if (activeImage != null)
        {
            activeImage.DOColor(_highlightedButtonColor, buttonColorTransitionDuration);
        }

        if (inactiveImage != null)
        {
            inactiveImage.DOColor(_defaultButtonColor, buttonColorTransitionDuration);
        }
    }

    public void UpdateFarmData(Farm farm)
    {
        UpdateBasicStats(farm);
        UpdateInventory(farm);
        UpdateWorkers(farm);
        UpdateUpgradeUI(farm);
    }

    private void UpdateUpgradeUI(Farm farm)
    {
        if (upgradeUIManager != null)
        {
            var upgradeLevels = new Dictionary<ProductType, int>
            {
                { ProductType.Tomato, farm.GetEquipmentLevel(ProductType.Tomato) },
                { ProductType.Blueberry, farm.GetEquipmentLevel(ProductType.Blueberry) },
                { ProductType.Strawberry, farm.GetEquipmentLevel(ProductType.Strawberry) },
                { ProductType.Milk, farm.GetEquipmentLevel(ProductType.Milk) }
            };

            upgradeUIManager.UpdateUpgradeLevels(upgradeLevels);
        }
        else
        {
            Debug.LogWarning("UpgradeUIManager is not assigned or initialized.");
        }
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

            var animals = new Dictionary<AnimalType, int>
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

            inventoryUIManager.UpdateInventory(seeds, products, animals);
        }
    }

    private void UpdateWorkers(Farm farm)
    {
        workerUIManager?.UpdateWorkers(farm.GetWorkers());
    }

    public void ShowMessage(string message)
    {
        if (messageSystem == null) return;
        messageSystem.ShowMessage(message);
    }

    // Cleanup DOTween animations on destroy
    private void OnDestroy()
    {
        leftSidePanel?.DOKill();
        rightSidePanel?.DOKill();
        leftSideToggleButton?.transform.DOKill();
        rightSideToggleButton?.transform.DOKill();
    }
}