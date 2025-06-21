using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor.Rendering.Universal;

[Serializable]
public class Farm
{
    public int Gold { get; private set; } = 50000;

    public List<Plot> Plots { get; private set; } = new List<Plot>();
    public List<int> WorkerIds { get; private set; } = new List<int>();
    public Inventory Inventory { get; private set; } = new Inventory();
    public DateTime LastSaveTime { get; set; } = DateTime.Now;

    [NonSerialized]
    private List<Worker> _workers = new List<Worker>();

    public event Action<int> GoldChanged;
    public event Action FarmStateChanged;

    [NonSerialized]
    private Dictionary<int, int> _plotReservations = new Dictionary<int, int>();
    [NonSerialized]
    private Dictionary<ProductType, int> _upgradeLevels = new Dictionary<ProductType, int>
    {
        { ProductType.Strawberry, 1 },
        { ProductType.Tomato, 1 },
        { ProductType.Blueberry, 1 },
        { ProductType.Milk, 1 }
    };

    public Farm()
    {
        InitializeStartingFarm();
    }

    private void InitializeStartingFarm()
    {
        // Tạo plots theo zones - mỗi zone 3 plots
        CreatePlotsForZone(PlotZone.Strawberry, 3);
        CreatePlotsForZone(PlotZone.Tomato, 3);
        CreatePlotsForZone(PlotZone.Blueberry, 3);
        CreatePlotsForZone(PlotZone.Cow, 3);


        // Starting resources - TẤT CẢ SEEDS = 10
        Inventory.AddSeeds(CropType.Tomato, 10);
        Inventory.AddSeeds(CropType.Blueberry, 10);
        Inventory.AddSeeds(CropType.Strawberry, 10);  // THÊM DÒNG NÀY
        Inventory.AddAnimals(AnimalType.Cow, 2);

        // 1 starting worker
        WorkerIds.Add(0);
    }

    private void CreatePlotsForZone(PlotZone zone, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int plotId = Plots.Count; // Sequential ID
            Plots.Add(new Plot(plotId, zone));
        }
    }

    public List<Plot> GetEmptyPlotsForZone(PlotZone zone)
    {
        return Plots.Where(p => p.Zone == zone && p.CanPlant).ToList();
    }

    public List<Plot> GetEmptyPlotsForCrop(CropType cropType)
    {
        PlotZone targetZone = cropType switch
        {
            CropType.Strawberry => PlotZone.Strawberry,
            CropType.Tomato => PlotZone.Tomato,
            CropType.Blueberry => PlotZone.Blueberry,
            _ => throw new ArgumentException($"Unknown crop type: {cropType}")
        };

        return GetEmptyPlotsForZone(targetZone);
    }

    public List<Plot> GetEmptyPlotsForAnimal(AnimalType animalType)
    {
        PlotZone targetZone = animalType switch
        {
            AnimalType.Cow => PlotZone.Cow,
            _ => throw new ArgumentException($"Unknown animal type: {animalType}")
        };

        return GetEmptyPlotsForZone(targetZone);
    }

    public List<Plot> GetPlotsReadyToHarvestInZone(PlotZone zone)
    {
        return Plots.Where(p => p.Zone == zone && p.CanHarvest).ToList();
    }

    // Override existing methods
    public List<Plot> GetEmptyPlotsForPlanting() => Plots.Where(p => p.CanPlant).ToList();
    public List<Plot> GetPlotsReadyToHarvest() => Plots.Where(p => p.CanHarvest).ToList();

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

    public void UpgradeEquipment(ProductType productType)
    {
        if (_upgradeLevels.ContainsKey(productType))
        {
            int currentLevel = _upgradeLevels[productType];
            int upgradeCost = 500;

            if (SpendGold(upgradeCost))
            {
                _upgradeLevels[productType]++;
                FarmStateChanged?.Invoke();
            }
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
    #region Plot Management
    public bool BuyPlot()
    {
        if (SpendGold(500))
        {
            int newPlotId = Plots.Count;
            PlotZone targetZone = GetZoneWithFewestPlots();

            Plots.Add(new Plot(newPlotId, targetZone));
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void ReleasePlot(int plotId, int workerId)
    {
        if (_plotReservations.TryGetValue(plotId, out int reservedByWorker) && reservedByWorker == workerId)
        {
            _plotReservations.Remove(plotId);
        }
    }

    private PlotZone GetZoneWithFewestPlots()
    {
        var zoneCounts = new Dictionary<PlotZone, int>
        {
            { PlotZone.Strawberry, 0 },
            { PlotZone.Tomato, 0 },
            { PlotZone.Blueberry, 0 },
            { PlotZone.Cow, 0 }
        };

        foreach (var plot in Plots)
        {
            zoneCounts[plot.Zone]++;
        }

        return zoneCounts.OrderBy(kvp => kvp.Value).First().Key;
    }

    public bool ReservePlot(int plotId, int workerId)
    {
        if (_plotReservations.ContainsKey(plotId))
        {
            // Plot already reserved by another worker
            return false;
        }

        _plotReservations[plotId] = workerId;
        return true;
    }

    public Plot GetPlot(int id) => Plots.FirstOrDefault(p => p.Id == id);

    // Worker management
    public void RegisterWorker(Worker worker)
    {
        Debug.Log($"Registering worker {worker.Id} to farm");
        if (!_workers.Contains(worker))
        {
            _workers.Add(worker);
        }
    }

    public bool IsPlotReserved(int plotId)
    {
        return _plotReservations.ContainsKey(plotId);
    }



    public bool BuyPlotForZone(PlotZone zone)
    {
        if (SpendGold(500))
        {
            int newPlotId = Plots.Count;
            Plots.Add(new Plot(newPlotId, zone));
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }


    #endregion


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

    public int GetEquipmentLevel(ProductType productType)
    {
        return _upgradeLevels.TryGetValue(productType, out int level) ? level : 1;
    }

    // Get plots for work

    public Worker GetBestAvailableWorker() => _workers.FirstOrDefault(w => w.IsAvailable);

    // Simple task priority
    public List<SimpleTask> GetWorkTasks()
    {
        var tasks = new List<SimpleTask>();

        // Only include plots that are ready to harvest AND not reserved by other workers
        foreach (var plot in GetPlotsReadyToHarvest())
        {
            if (!IsPlotReserved(plot.Id))
            {
                tasks.Add(new SimpleTask
                {
                    Plot = plot,
                    Type = plot.Content is Cow ? TaskType.Milk : TaskType.Harvest
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

    public void TriggerFarmStateChanged()
    {
        FarmStateChanged?.Invoke();
    }

}
