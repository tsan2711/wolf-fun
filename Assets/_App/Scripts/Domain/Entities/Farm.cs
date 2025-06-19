using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Farm
{
    public int Gold { get; private set; } = 150;
    public int EquipmentLevel { get; private set; } = 1;
    public List<Plot> Plots { get; private set; } = new List<Plot>();
    public List<int> WorkerIds { get; private set; } = new List<int>();
    public Inventory Inventory { get; private set; } = new Inventory();
    public DateTime LastSaveTime { get; set; } = DateTime.Now;

    [NonSerialized]
    private List<Worker> _workers = new List<Worker>();

    public event Action<int> GoldChanged;
    public event Action FarmStateChanged;

    public Farm()
    {
        InitializeStartingFarm();
    }

    private void InitializeStartingFarm()
    {
        // 3 starting plots
        for (int i = 0; i < 3; i++)
        {
            Plots.Add(new Plot(i));
        }

        // Starting resources
        Inventory.AddSeeds(CropType.Tomato, 10);
        Inventory.AddSeeds(CropType.Blueberry, 10);
        Inventory.AddAnimals(AnimalType.Cow, 2);
        
        // 1 starting worker
        WorkerIds.Add(0);
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            GoldChanged?.Invoke(Gold);
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        GoldChanged?.Invoke(Gold);
        FarmStateChanged?.Invoke();
    }

    public void UpgradeEquipment()
    {
        if (SpendGold(500))
        {
            EquipmentLevel++;
            FarmStateChanged?.Invoke();
        }
    }

    public bool BuyWorker()
    {
        if (SpendGold(500))
        {
            int newWorkerId = WorkerIds.Count > 0 ? WorkerIds.Max() + 1 : 0;
            WorkerIds.Add(newWorkerId);
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool BuyPlot()
    {
        if (SpendGold(500))
        {
            int newPlotId = Plots.Count;
            Plots.Add(new Plot(newPlotId));
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public Plot GetPlot(int id) => Plots.FirstOrDefault(p => p.Id == id);

    // Worker management
    public void RegisterWorker(Worker worker)
    {
        if (!_workers.Contains(worker))
        {
            _workers.Add(worker);
        }
    }

    public void UnregisterWorker(Worker worker)
    {
        _workers.Remove(worker);
    }

    public List<Worker> GetWorkers() => _workers.ToList();

    // Simple worker counts
    public int GetAvailableWorkers() => _workers.Count(w => w.IsAvailable);
    public int GetWorkingWorkers() => _workers.Count(w => w.GetCurrentState() == WorkerState.Working);
    public int GetSleepingWorkers() => _workers.Count(w => w.GetCurrentState() == WorkerState.Sleeping);
    public int GetTotalWorkers() => _workers.Count;

    // Simple plot counts
    public int GetEmptyPlots() => Plots.Count(p => p.CanPlant);
    public int GetReadyToHarvestPlots() => Plots.Count(p => p.CanHarvest);
    public int GetTotalPlots() => Plots.Count;

    // Get plots for work
    public List<Plot> GetPlotsReadyToHarvest() => Plots.Where(p => p.CanHarvest).ToList();
    public List<Plot> GetEmptyPlotsForPlanting() => Plots.Where(p => p.CanPlant).ToList();

    public Worker GetBestAvailableWorker() => _workers.FirstOrDefault(w => w.IsAvailable);

    // Simple task priority
    public List<SimpleTask> GetWorkTasks()
    {
        var tasks = new List<SimpleTask>();

        // First: Harvest ready plots
        foreach (var plot in GetPlotsReadyToHarvest())
        {
            tasks.Add(new SimpleTask
            {
                Plot = plot,
                Type = plot.Content is Cow ? TaskType.Milk : TaskType.Harvest
            });
        }

        // Second: Plant on empty plots if we have seeds
        foreach (var plot in GetEmptyPlotsForPlanting())
        {
            var cropType = GetNextCropToPlant();
            if (cropType.HasValue)
            {
                tasks.Add(new SimpleTask
                {
                    Plot = plot,
                    Type = TaskType.Plant,
                    CropType = cropType.Value
                });
            }
        }

        return tasks;
    }

    private CropType? GetNextCropToPlant()
    {
        if (Inventory.GetSeedCount(CropType.Tomato) > 0)
            return CropType.Tomato;
        
        if (Inventory.GetSeedCount(CropType.Blueberry) > 0)
            return CropType.Blueberry;
        
        if (Inventory.GetSeedCount(CropType.Strawberry) > 0)
            return CropType.Strawberry;
        
        return null;
    }

    public bool IsNightTime()
    {
        int hour = DateTime.Now.Hour;
        return hour >= 22 || hour <= 6;
    }
}
