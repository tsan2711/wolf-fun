using System;

public interface IPlantable
{
    // Existing methods
    void Plant(DateTime plantTime);
    bool IsReadyToHarvest();
    ProductType Harvest();
    bool IsExpired();
    string GetDisplayName();
    TimeSpan GetTimeToNextHarvest();
    int GetCurrentHarvests();
    int GetMaxHarvests();

    // methods for save/load support
    void SetMaxHarvests(int maxHarvests);
    void SetGrowthTime(int growthTimeMinutes);
    int GetGrowthTimeMinutes();
    void SetCurrentHarvests(int harvests);
    DateTime GetPlantedTime();
    DateTime GetLastHarvestTime();
    void SetPlantedTime(DateTime time);
    void SetLastHarvestTime(DateTime time);
}