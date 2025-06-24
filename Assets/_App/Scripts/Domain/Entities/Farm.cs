using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

[Serializable]
public class Farm : IDeserializationCallback
{
    public int Gold { get; private set; } = 500;
    public int MaxPlot { get; private set; } = 64;
    public int MaxWorker { get; private set; } = 10;

    public const string TOMATOSEED = "Tomato Seed";
    public const string TOMATOMATURE = "Tomato Mature";
    public const string BLUEBERRYSEED = "Blueberry Seed";
    public const string BLUEBERRYMATURE = "Blueberry Mature";
    public const string STRAWBERRYSEED = "Strawberry Seed";
    public const string STRAWBERRYMATURE = "Strawberry Mature";
    public const string COW = "Cow";
    public const string COWMATURE = "Cow Mature";

    public List<Plot> Plots { get; private set; } = new List<Plot>();
    public List<int> WorkerIds { get; private set; } = new List<int>();
    public Inventory Inventory { get; private set; } = new Inventory();
    public DateTime LastSaveTime { get; set; } = DateTime.Now;

    [NonSerialized]
    private List<Worker> _workers;
    [NonSerialized]
    private Dictionary<int, int> _plotReservations;
    [NonSerialized]
    private Dictionary<ProductType, int> _upgradeLevels;

    public event Action<int> GoldChanged;
    public event Action FarmStateChanged;
    public event Action<Plot> PlotStateChanged;

    // Constructor
    public Farm(bool isLoadedGame = false)
    {
        if (!isLoadedGame)
        {
            InitializeStartingFarm();
        }
        else
        {
            Debug.Log("Farm loaded from save, initializing non-serialized fields");
            InitializeNonSerializedFields();
        }
    }

    public void OnDeserialization(object sender)
    {
        Debug.Log("Farm.OnDeserialization called");
        InitializeNonSerializedFields();
    }

    // Method public để manually initialize sau khi load (backup method)
    public void InitializeAfterLoad()
    {
        Debug.Log("Farm.InitializeAfterLoad called manually");
        InitializeNonSerializedFields();
    }

    private void InitializeNonSerializedFields()
    {
        Debug.Log("Initializing NonSerialized fields");
        
        _workers = new List<Worker>();
        _plotReservations = new Dictionary<int, int>();
        _upgradeLevels = new Dictionary<ProductType, int>
        {
            { ProductType.Strawberry, 1 },
            { ProductType.Tomato, 1 },
            { ProductType.Blueberry, 1 },
            { ProductType.Milk, 1 }
        };
        
        Debug.Log("NonSerialized fields initialized successfully");
    }

    private void InitializeStartingFarm()
    {
        // Only initialize if this is a new farm (not loaded)
        if (Plots.Count == 0)
        {
            // Tạo plots theo zones - mỗi zone 3 plots
            CreatePlotsForZone(PlotZone.Strawberry, 3);
            CreatePlotsForZone(PlotZone.Tomato, 3);
            CreatePlotsForZone(PlotZone.Blueberry, 3);
            CreatePlotsForZone(PlotZone.Cow, 3);

            // Starting resources
            Inventory.AddSeeds(CropType.Tomato, 10);
            Inventory.AddSeeds(CropType.Blueberry, 10);
            Inventory.AddSeeds(CropType.Strawberry, 10);
            Inventory.AddAnimals(AnimalType.Cow, 2);

            // 1 starting worker
            WorkerIds.Add(0);
        }
    }

    private void CreatePlotsForZone(PlotZone zone, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int plotId = Plots.Count; // Sequential ID
            Plots.Add(new Plot(this, plotId, zone));
        }
    }

    // Worker management với null check
    public void RegisterWorker(Worker worker)
    {
        // Đảm bảo _workers được khởi tạo
        if (_workers == null)
        {
            Debug.LogWarning("_workers was null in RegisterWorker, reinitializing...");
            InitializeNonSerializedFields();
        }

        Debug.Log($"Registering worker {worker.Id} to farm");
        
        if (!_workers.Contains(worker))
        {
            _workers.Add(worker);
            Debug.Log($"Worker {worker.Id} registered successfully. Total workers: {_workers.Count}");
        }
        else
        {
            Debug.LogWarning($"Worker {worker.Id} already registered");
        }
    }

    public void UnregisterWorker(Worker worker)
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
            return;
        }
        
        _workers.Remove(worker);
    }

    public List<Worker> GetWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.ToList();
    }

    public int GetAvailableWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.Count(w => w.IsAvailable);
    }

    public int GetWorkingWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.Count(w => w.GetCurrentState() == WorkerState.Working);
    }

    public int GetSleepingWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.Count(w => w.GetCurrentState() == WorkerState.Sleeping);
    }

    public int GetTotalWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.Count;
    }

    public Worker GetBestAvailableWorker()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.FirstOrDefault(w => w.IsAvailable);
    }

    public bool DoesReachMaxWorkers()
    {
        if (_workers == null)
        {
            InitializeNonSerializedFields();
        }
        return _workers.Count < MaxWorker;
    }

    // Plot reservations với null check
    public bool ReservePlot(int plotId, int workerId)
    {
        if (_plotReservations == null)
        {
            InitializeNonSerializedFields();
        }

        if (_plotReservations.ContainsKey(plotId))
        {
            return false;
        }

        _plotReservations[plotId] = workerId;
        return true;
    }

    public void ReleasePlot(int plotId, int workerId)
    {
        if (_plotReservations == null)
        {
            InitializeNonSerializedFields();
        }

        if (_plotReservations.TryGetValue(plotId, out int reservedByWorker) && reservedByWorker == workerId)
        {
            _plotReservations.Remove(plotId);
        }
    }

    public bool IsPlotReserved(int plotId)
    {
        if (_plotReservations == null)
        {
            InitializeNonSerializedFields();
        }
        return _plotReservations.ContainsKey(plotId);
    }

    // Equipment levels với null check
    public int GetEquipmentLevel(ProductType productType)
    {
        if (_upgradeLevels == null)
        {
            InitializeNonSerializedFields();
        }
        return _upgradeLevels.TryGetValue(productType, out int level) ? level : 1;
    }

    public void UpgradeEquipment(ProductType productType)
    {
        if (_upgradeLevels == null)
        {
            InitializeNonSerializedFields();
        }

        if (_upgradeLevels.ContainsKey(productType))
        {
            int upgradeCost = 500;
            if (SpendGold(upgradeCost))
            {
                _upgradeLevels[productType]++;
                FarmStateChanged?.Invoke();
            }
        }
    }

    // Rest of the existing methods remain the same...
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
            PlotZone targetZone = GetZoneWithFewestPlots();

            Plots.Add(new Plot(this, newPlotId, targetZone));
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
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

    public bool BuyPlotForZone(PlotZone zone)
    {
        if (SpendGold(500))
        {
            int newPlotId = Plots.Count;
            Plots.Add(new Plot(this, newPlotId, zone));
            FarmStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public Plot GetPlot(int id) => Plots.FirstOrDefault(p => p.Id == id);
    public bool DoesReachMaxPlots() => Plots.Count < MaxPlot;

    public int GetEmptyPlots() => Plots.Count(p => p.CanPlant);
    public int GetReadyToHarvestPlots() => Plots.Count(p => p.CanHarvest);
    public int GetTotalPlots() => Plots.Count;

    public List<SimpleTask> GetWorkTasks()
    {
        var tasks = new List<SimpleTask>();

        foreach (var plot in Plots)
        {
            if (plot.Content != null && plot.CanHarvest)
            {
                tasks.Add(new SimpleTask { Plot = plot, Type = TaskType.Harvest });
            }
            else if (plot.Content == null && plot.CanPlant)
            {
                var cropType = GetBestCropForPlot(plot);
                if (cropType.HasValue && Inventory.GetSeedCount(cropType.Value) > 0)
                {
                    tasks.Add(new SimpleTask
                    {
                        Plot = plot,
                        Type = TaskType.Plant,
                        CropType = cropType.Value
                    });
                }
            }
        }

        return tasks;
    }

    private CropType? GetBestCropForPlot(Plot plot)
    {
        return plot.Zone switch
        {
            PlotZone.Tomato => CropType.Tomato,
            PlotZone.Blueberry => CropType.Blueberry,
            PlotZone.Strawberry => CropType.Strawberry,
            PlotZone.Cow => null,
            _ => null
        };
    }

    public bool IsNightTime()
    {
        int hour = DateTime.Now.Hour;
        return hour >= 22 || hour <= 6;
    }

    public void TriggerPlotStateChanged(Plot plot)
    {
        PlotStateChanged?.Invoke(plot);
    }

    public void TriggerFarmStateChanged()
    {
        FarmStateChanged?.Invoke();
    }

    public void OnPlotStateChange(Plot plot)
    {
        PlotStateChanged?.Invoke(plot);
    }

    public void SetEquipmentLevel(ProductType strawberry, int strawberryLevel)
    {
        if (_upgradeLevels == null)
        {
            InitializeNonSerializedFields();
        }

        if (_upgradeLevels.ContainsKey(strawberry))
        {
            _upgradeLevels[strawberry] = strawberryLevel;
            FarmStateChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"ProductType {strawberry} not found in upgrade levels");
        }
    }

    public void SetGold(int gold)
    {
        if (gold < 0)
        {
            Debug.LogWarning("Attempted to set negative gold value, ignoring.");
            return;
        }

        Gold = gold;
        GoldChanged?.Invoke(Gold);
        FarmStateChanged?.Invoke();
    }

    public void SetMaxPlot(int maxPlot)
    {
        if (maxPlot < 0)
        {
            Debug.LogWarning("Attempted to set negative max plot value, ignoring.");
            return;
        }

        MaxPlot = maxPlot;
        FarmStateChanged?.Invoke();
    }

    public void SetMaxWorker(int maxWorker)
    {
        if (maxWorker < 0)
        {
            Debug.LogWarning("Attempted to set negative max worker value, ignoring.");
            return;
        }

        MaxWorker = maxWorker;
        FarmStateChanged?.Invoke();
    }
}