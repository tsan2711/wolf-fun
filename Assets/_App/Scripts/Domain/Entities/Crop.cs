using System;
using UnityEngine;

[Serializable]
public abstract class Crop : IPlantable
{
    protected int growthTimeMinutes;
    protected int maxHarvests;

    public virtual TimeSpan GrowthTime => TimeSpan.FromMinutes(growthTimeMinutes);
    public virtual int MaxHarvests => maxHarvests;
    public abstract ProductType ProductType { get; }
    public abstract string DisplayName { get; }

    public int CurrentHarvests { get; private set; }
    public DateTime PlantedTime { get; private set; }
    public DateTime LastHarvestTime { get; private set; }

    public Crop(int growthTimeMinutes = 1, int maxHarvests = 10)
    {
        this.growthTimeMinutes = growthTimeMinutes;
        this.maxHarvests = maxHarvests;
        CurrentHarvests = 0; // Start at 0
        PlantedTime = DateTime.MinValue;
        LastHarvestTime = DateTime.MinValue;
    }

    public virtual void Plant(DateTime plantTime)
    {
        PlantedTime = plantTime;
        LastHarvestTime = plantTime;
        // KHÔNG reset CurrentHarvests ở đây khi restore
    }

    public virtual bool IsReadyToHarvest()
    {
        return DateTime.Now >= LastHarvestTime.Add(GrowthTime) &&
               CurrentHarvests < MaxHarvests;
    }

    public virtual ProductType Harvest()
    {
        if (!IsReadyToHarvest()) return ProductType.None;

        CurrentHarvests++;
        LastHarvestTime = DateTime.Now;
        return ProductType;
    }

    public virtual bool IsExpired()
    {
        return CurrentHarvests >= MaxHarvests;
    }

    public string GetDisplayName() => DisplayName;

    public TimeSpan GetTimeToNextHarvest()
    {
        if (IsReadyToHarvest()) return TimeSpan.Zero;
        var nextHarvest = LastHarvestTime.Add(GrowthTime);
        return nextHarvest > DateTime.Now ? nextHarvest - DateTime.Now : TimeSpan.Zero;
    }

    public int GetCurrentHarvests() => CurrentHarvests;
    public int GetMaxHarvests() => MaxHarvests;

    // Save/load support methods
    public virtual void SetMaxHarvests(int maxHarvests) => this.maxHarvests = maxHarvests;
    public virtual void SetGrowthTime(int growthTimeMinutes) => this.growthTimeMinutes = growthTimeMinutes;
    public virtual int GetGrowthTimeMinutes() => growthTimeMinutes;

    // QUAN TRỌNG: Set exact value, không validate
    public void SetCurrentHarvests(int harvests) => CurrentHarvests = harvests;

    public DateTime GetPlantedTime() => PlantedTime;
    public DateTime GetLastHarvestTime() => LastHarvestTime;
    public void SetPlantedTime(DateTime time) => PlantedTime = time;
    public void SetLastHarvestTime(DateTime time) => LastHarvestTime = time;
}
