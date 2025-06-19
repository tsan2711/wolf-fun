using System;
using UnityEngine;


[Serializable]
public abstract class Crop : IPlantable
{
    public abstract TimeSpan GrowthTime { get; }
    public abstract int MaxHarvests { get; }
    public abstract ProductType ProductType { get; }
    public abstract string DisplayName { get; }

    public int CurrentHarvests { get; private set; }
    public DateTime PlantedTime { get; private set; }
    public DateTime LastHarvestTime { get; private set; }

    public virtual void Plant(DateTime plantTime)
    {
        PlantedTime = plantTime;
        LastHarvestTime = plantTime;
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
}