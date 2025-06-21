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
    event Action<CropType, int> BuySeedsRequested;
    event Action<AnimalType> BuyAnimalRequested;
    event Action BuyWorkerRequested;
    event Action BuyPlotRequested;
    event Action UpgradeEquipmentRequested;
    event Action AutoHarvestRequested;
    event Action<CropType> AutoPlantRequested;
    event Action<AnimalType> AutoPlaceAnimalRequested;  // New event for auto placing animals
    event Action<ProductType, int> SellProductRequested;
}