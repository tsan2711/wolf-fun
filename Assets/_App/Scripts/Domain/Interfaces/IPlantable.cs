using System;
using UnityEngine;

public interface IPlantable
{
    bool IsReadyToHarvest();
    ProductType Harvest();
    bool IsExpired();
    void Plant(DateTime plantTime);
    string GetDisplayName();
    TimeSpan GetTimeToNextHarvest();
    int GetCurrentHarvests();
    int GetMaxHarvests();
    void SetMaxHarvests(int maxHarvests);
    void SetGrowthTime(int growthTimeMinutes);
    int GetGrowthTimeMinutes();
}
