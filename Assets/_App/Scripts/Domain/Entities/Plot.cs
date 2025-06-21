using System;
using UnityEngine;

[Serializable]
public class Plot
{
    public int Id { get; }
    public PlotZone Zone { get; }  // THÊM MỚI
    public IPlantable Content { get; private set; }
    public PlotState State { get; private set; } = PlotState.Empty;
    public DateTime LastActionTime { get; private set; }

    public Plot(int id, PlotZone zone)  // THÊM PARAMETER
    {
        Id = id;
        Zone = zone;
    }

    public bool CanPlant => State == PlotState.Empty;
    public bool CanHarvest => Content?.IsReadyToHarvest() == true;

    // Kiểm tra xem có thể plant loại này không
    public bool CanPlantType(CropType cropType)
    {
        if (!CanPlant) return false;

        return Zone switch
        {
            PlotZone.Strawberry => cropType == CropType.Strawberry,
            PlotZone.Tomato => cropType == CropType.Tomato,
            PlotZone.Blueberry => cropType == CropType.Blueberry,
            PlotZone.Cow => false, // Cow zone không plant crops
            _ => false
        };
    }

    public bool CanPlaceAnimal(AnimalType animalType)
    {
        if (!CanPlant) return false;

        return Zone switch
        {
            PlotZone.Cow => animalType == AnimalType.Cow,
            _ => false // Chỉ Cow zone mới place animals
        };
    }

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

    public Color GetColor()
    {
        return State switch
        {
            PlotState.Empty => Color.white,
            PlotState.Growing => Color.green,
            PlotState.ReadyToHarvest => Color.yellow,
            PlotState.Dead => Color.red,
            _ => Color.gray
        };
    }    
}