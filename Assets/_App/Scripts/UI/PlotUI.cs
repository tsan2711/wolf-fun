using UnityEngine;
using UnityEngine.UI;

public class UnityPlotUI : MonoBehaviour
{
    [SerializeField] private Text plotIdText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text timeText;
    [SerializeField] private Button plantButton;
    [SerializeField] private Button harvestButton;

    private Plot _plot;
    private MainGameUI _gameUI;

    public void Initialize(Plot plot, MainGameUI gameUI)
    {
        _plot = plot;
        _gameUI = gameUI;
        
        plotIdText.text = $"Plot {plot.Id + 1}";
        
        plantButton?.onClick.AddListener(OnPlantClicked);
        harvestButton?.onClick.AddListener(OnHarvestClicked);
        
        UpdateDisplay();
        InvokeRepeating(nameof(UpdateDisplay), 1f, 1f);
    }

    private void UpdateDisplay()
    {
        if (_plot == null) return;

        // Update status
        if (_plot.Content == null)
        {
            statusText.text = "Empty";
            timeText.text = "";
        }
        else
        {
            statusText.text = _plot.Content.GetDisplayName();
            
            if (_plot.CanHarvest)
            {
                timeText.text = "Ready!";
            }
            else
            {
                var timeToHarvest = _plot.Content.GetTimeToNextHarvest();
                timeText.text = $"{timeToHarvest:mm\\:ss}";
            }
        }

        // Update buttons
        if (plantButton) plantButton.gameObject.SetActive(_plot.CanPlant);
        if (harvestButton) harvestButton.gameObject.SetActive(_plot.CanHarvest);
    }

    private void OnPlantClicked()
    {
        // Simple: just plant tomato for now
        _gameUI?.OnPlotPlantRequested(_plot.Id, CropType.Tomato);
    }

    private void OnHarvestClicked()
    {
        _gameUI?.OnPlotHarvestRequested(_plot.Id);
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
