using System;
using UnityEngine;
public class PlantTask : IWorkTask
{
    private Plot _plot;
    private CropType _cropType;
    private Farm _farm;

    public PlantTask(Plot plot, CropType cropType, Farm farm)
    {
        _plot = plot;
        _cropType = cropType;
        _farm = farm;
    }

    public void Execute()
    {
        if (_farm.Inventory.UseSeeds(_cropType, 1))
        {
            var crop = CreateCrop(_cropType);
            _plot.Plant(crop);
            Debug.Log($"Worker planted {_cropType} on plot {_plot.Id}");
        }
    }

    public TaskType GetTaskType() => TaskType.Plant;

    private IPlantable CreateCrop(CropType cropType)
    {
        return cropType switch
        {
            CropType.Tomato => new TomatoCrop(),
            CropType.Blueberry => new BlueberryCrop(),
            CropType.Strawberry => new StrawberryCrop(),
            _ => throw new ArgumentException("Unknown crop type")
        };
    }
}