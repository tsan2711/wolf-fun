using System;
using UnityEngine;

[Serializable]
public class Cow : IPlantable
{
    public TimeSpan ProductionTime => TimeSpan.FromMinutes(30);
    public int MaxProductions => 100;
    public ProductType ProductType => ProductType.Milk;
    public string DisplayName => "Cow";

    public int CurrentProductions { get; private set; }
    public DateTime PlacedTime { get; private set; }
    public DateTime LastProductionTime { get; private set; }

    public void Plant(DateTime plantTime)
    {
        PlacedTime = plantTime;
        LastProductionTime = plantTime;
    }

    public bool IsReadyToHarvest()
    {
        return DateTime.Now >= LastProductionTime.Add(ProductionTime) && 
               CurrentProductions < MaxProductions;
    }

    public ProductType Harvest()
    {
        if (!IsReadyToHarvest()) return ProductType.None;

        CurrentProductions++;
        LastProductionTime = DateTime.Now;
        return ProductType.Milk;
    }

    public bool IsExpired()
    {
        return CurrentProductions >= MaxProductions;
    }

    public string GetDisplayName() => DisplayName;

    public TimeSpan GetTimeToNextHarvest()
    {
        if (IsReadyToHarvest()) return TimeSpan.Zero;
        var nextProduction = LastProductionTime.Add(ProductionTime);
        return nextProduction > DateTime.Now ? nextProduction - DateTime.Now : TimeSpan.Zero;
    }

    public int GetCurrentHarvests() => CurrentProductions;
    public int GetMaxHarvests() => MaxProductions;
}
