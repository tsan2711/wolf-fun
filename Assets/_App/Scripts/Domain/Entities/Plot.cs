using System;
using UnityEngine;

[Serializable]
public class Plot
{
    public int Id { get; }
    public PlotZone Zone { get; }
    public IPlantable Content { get; private set; }
    public DateTime LastActionTime { get; private set; }
    private readonly Farm _farm;

    // Simple state property
    public PlotState State 
    { 
        get 
        {
            if (Content == null) return PlotState.Empty;
            if (Content.IsExpired()) return PlotState.Dead;
            if (Content.IsReadyToHarvest()) return PlotState.ReadyToHarvest;
            return PlotState.Growing;
        } 
    }

    public Plot(Farm farm, int id, PlotZone zone)
    {
        _farm = farm ?? throw new ArgumentNullException(nameof(farm));
        Id = id;
        Zone = zone;
        LastActionTime = DateTime.Now;
    }

    public bool CanPlant => Content == null;
    public bool CanHarvest => Content?.IsReadyToHarvest() == true;

    public bool CanPlantType(CropType cropType)
    {
        if (!CanPlant) return false;
        return Zone switch
        {
            PlotZone.Strawberry => cropType == CropType.Strawberry,
            PlotZone.Tomato => cropType == CropType.Tomato,
            PlotZone.Blueberry => cropType == CropType.Blueberry,
            PlotZone.Cow => false,
            _ => false
        };
    }

    public bool CanPlaceAnimal(AnimalType animalType)
    {
        if (!CanPlant) return false;
        return Zone switch
        {
            PlotZone.Cow => animalType == AnimalType.Cow,
            _ => false
        };
    }

    public void Plant(IPlantable plantable)
    {
        if (!CanPlant) return;
        Content = plantable;
        LastActionTime = DateTime.Now;
        plantable.Plant(DateTime.Now);
        _farm.TriggerPlotStateChanged(this);
    }

    public ProductType? Harvest()
    {
        if (!CanHarvest) return null;
        var product = Content.Harvest();
        LastActionTime = DateTime.Now;
        _farm.OnPlotStateChange(this);
        
        if (Content.IsExpired())
        {
            Reset();
        }
        return product;
    }

    public void Reset()
    {
        Content = null;
        LastActionTime = DateTime.Now;
        _farm.TriggerPlotStateChanged(this);
    }

    // Simple methods for save/load
    public void SetContent(IPlantable content) => Content = content;
    public void SetLastActionTime(DateTime time) => LastActionTime = time;
}