using System;
using UnityEngine;

public class HarvestTask : IWorkTask
{
    private Plot _plot;
    private Farm _farm;

    public HarvestTask(Plot plot, Farm farm)
    {
        _plot = plot;
        _farm = farm;
    }

    public void Execute()
    {
        var product = _plot.Harvest();
        if (product.HasValue && product.Value != ProductType.None)
        {
            _farm.Inventory.AddProduct(product.Value, 1);
            Debug.Log($"Worker harvested {product.Value} from plot {_plot.Id}");
        }
    }

    public TaskType GetTaskType() => TaskType.Harvest;
}