using System;
using UnityEngine;

[Serializable]
public class Plot
{
    public int Id { get; }
    public IPlantable Content { get; private set; }
    public PlotState State { get; private set; } = PlotState.Empty;
    public DateTime LastActionTime { get; private set; }

    public Plot(int id)
    {
        Id = id;
    }

    public bool CanPlant => State == PlotState.Empty;
    public bool CanHarvest => Content?.IsReadyToHarvest() == true;

    public void Plant(IPlantable plantable)
    {
        if (!CanPlant) return;
        
        Content = plantable;
        State = PlotState.Growing;
        LastActionTime = DateTime.Now;
        plantable.Plant(DateTime.Now);
    }

    public ProductType? Harvest()
    {
        if (!CanHarvest) return null;

        var product = Content.Harvest();
        LastActionTime = DateTime.Now;

        if (Content.IsExpired())
        {
            Content = null;
            State = PlotState.Empty;
        }

        return product;
    }
}