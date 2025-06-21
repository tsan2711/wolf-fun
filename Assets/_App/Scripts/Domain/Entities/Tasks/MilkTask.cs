using UnityEngine;

public class MilkTask : IWorkTask
{
    private Plot _plot;
    private Farm _farm;

    public MilkTask(Plot plot, Farm farm)
    {
        _plot = plot;
        _farm = farm;
    }

    public void Execute()
    {
        var product = _plot.Harvest();
        if (product.HasValue && product.Value == ProductType.Milk)
        {
            _farm.Inventory.AddProduct(ProductType.Milk, 1);
            Debug.Log($"Worker milked cow on plot {_plot.Id}");
        }
    }

    public TaskType GetTaskType() => TaskType.Milk;
}