using System;
using UnityEngine;

public interface IGameUI
{
    // Data updates (from GameController)
    void UpdateFarmData(Farm farm);
    void ShowMessage(string message);
    
    // Input events (to GameController)
    event Action NewGameRequested;
    event Action ContinueGameRequested;
    event Action<int, CropType> PlantCropRequested;
    event Action<int> HarvestPlotRequested;
    event Action<CropType> BuySeedsRequested;
    event Action BuyWorkerRequested;
    event Action AutoHarvestRequested;
    // Add other events as needed
}