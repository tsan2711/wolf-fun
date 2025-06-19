using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MainGameUI : MonoBehaviour, IGameUI
{
    [Header("Resource Display")]
    [SerializeField] private Text goldText;
    [SerializeField] private Text equipmentLevelText;
    [SerializeField] private Text workersText;
    [SerializeField] private Text plotsText;

    [Header("Action Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button buyWorkerButton;
    [SerializeField] private Button autoHarvestButton;
    [SerializeField] private Button autoPlantTomatoButton;

    [Header("Shop Buttons")]
    [SerializeField] private Button buyTomatoSeedsButton;
    [SerializeField] private Button buyBlueberrySeedsButton;

    [Header("Plot UI")]
    [SerializeField] private Transform plotsContainer;
    [SerializeField] private GameObject plotUIPrefab;

    [Header("Message UI")]
    [SerializeField] private Text messageText;
    [SerializeField] private GameObject messagePanel;

    // Events
    public event Action NewGameRequested;
    public event Action ContinueGameRequested;
    public event Action<int, CropType> PlantCropRequested;
    public event Action<int> HarvestPlotRequested;
    public event Action<CropType> BuySeedsRequested;
    public event Action BuyWorkerRequested;
    public event Action AutoHarvestRequested;

    private List<UnityPlotUI> _plotUIs = new List<UnityPlotUI>();

    private void Start()
    {
        SetupButtons();
        messagePanel.SetActive(false);
    }

    private void SetupButtons()
    {
        newGameButton?.onClick.AddListener(() => NewGameRequested?.Invoke());
        continueGameButton?.onClick.AddListener(() => ContinueGameRequested?.Invoke());
        buyWorkerButton?.onClick.AddListener(() => BuyWorkerRequested?.Invoke());
        autoHarvestButton?.onClick.AddListener(() => AutoHarvestRequested?.Invoke());
        autoPlantTomatoButton?.onClick.AddListener(() => AutoPlantRequested?.Invoke(CropType.Tomato));
        
        buyTomatoSeedsButton?.onClick.AddListener(() => BuySeedsRequested?.Invoke(CropType.Tomato));
        buyBlueberrySeedsButton?.onClick.AddListener(() => BuySeedsRequested?.Invoke(CropType.Blueberry));
    }

    public void UpdateFarmData(Farm farm)
    {
        UpdateResourceDisplay(farm);
        UpdatePlots(farm);
    }

    private void UpdateResourceDisplay(Farm farm)
    {
        if (goldText) goldText.text = $"Gold: {farm.Gold}";
        if (equipmentLevelText) equipmentLevelText.text = $"Equipment Level: {farm.EquipmentLevel}";
        if (workersText) workersText.text = $"Workers: {farm.GetAvailableWorkers()}/{farm.GetTotalWorkers()}";
        if (plotsText) plotsText.text = $"Plots: {farm.GetTotalPlots() - farm.GetEmptyPlots()}/{farm.GetTotalPlots()}";
    }

    private void UpdatePlots(Farm farm)
    {
        // Clear existing
        foreach (var plotUI in _plotUIs)
        {
            if (plotUI != null) Destroy(plotUI.gameObject);
        }
        _plotUIs.Clear();

        // Create new plot UIs
        if (plotsContainer && plotUIPrefab)
        {
            foreach (var plot in farm.Plots)
            {
                GameObject plotGO = Instantiate(plotUIPrefab, plotsContainer);
                UnityPlotUI plotUI = plotGO.GetComponent<UnityPlotUI>();
                if (plotUI != null)
                {
                    plotUI.Initialize(plot, this);
                    _plotUIs.Add(plotUI);
                }
            }
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText) messageText.text = message;
        if (messagePanel) 
        {
            messagePanel.SetActive(true);
            Invoke(nameof(HideMessage), 3f);
        }
    }

    private void HideMessage()
    {
        if (messagePanel) messagePanel.SetActive(false);
    }

    // Called by PlotUI
    public void OnPlotPlantRequested(int plotId, CropType cropType)
    {
        PlantCropRequested?.Invoke(plotId, cropType);
    }

    public void OnPlotHarvestRequested(int plotId)
    {
        HarvestPlotRequested?.Invoke(plotId);
    }

    // Add more events as needed...
    public event Action<CropType> AutoPlantRequested;
}