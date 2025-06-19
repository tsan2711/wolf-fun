using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameController : MonoBehaviour
{
    [Header("Worker Prefab")]
    [SerializeField] private GameObject workerPrefab;

    [Header("Farm Settings")]
    [SerializeField] private Transform farmContainer;

    private Farm _farm;
    private GameConfig _config;
    private Coroutine _gameLoopCoroutine;
    private List<Worker> _workerGameObjects = new List<Worker>();

    public event Action<Farm> FarmStateChanged;
    public event Action<string> MessageDisplayed;

    public Farm Farm => _farm;

    private IGameUI _gameUI;



    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _config = new GameConfig();


        InitializeUI();
    }

    private void InitializeUI()
    {
        // Try to find Unity UI first
        _gameUI = FindObjectOfType<MainGameUI>();

        // Fallback to Console if no Unity UI found
        // if (_gameUI == null)
        // {
        //     _gameUI = new ConsoleGameUI();
        // }

        // Subscribe to UI events
        SubscribeToUIEvents();

        Debug.Log($"Using UI System: {_gameUI.GetType().Name}");
    }

    private void SubscribeToUIEvents()
    {
        _gameUI.NewGameRequested += StartNewGame;
        _gameUI.ContinueGameRequested += LoadGame;
        _gameUI.PlantCropRequested += (plotId, cropType) => PlantCrop(plotId, cropType);
        _gameUI.HarvestPlotRequested += (plotId) => HarvestPlot(plotId);
        _gameUI.BuySeedsRequested += (cropType) => BuySeeds(cropType, 1);
        _gameUI.BuyWorkerRequested += () => BuyWorker();
        _gameUI.AutoHarvestRequested += () => AutoHarvestAll();
    }



    public void StartNewGame()
    {
        _farm = new Farm();
        _farm.FarmStateChanged += OnFarmStateChanged;

        // Create initial worker GameObjects
        CreateWorkerGameObjects();

        StartGameLoop();
        FarmStateChanged?.Invoke(_farm);
    }

    public void LoadGame()
    {
        var savedGame = SaveLoadSystem.LoadGame();
        if (savedGame != null)
        {
            _farm = savedGame;
            _farm.FarmStateChanged += OnFarmStateChanged;
            var offlineTime = DateTime.Now - _farm.LastSaveTime;
            if (offlineTime.TotalMinutes > 1)
            {
                CalculateOfflineProgress(offlineTime);
            }

            CreateWorkerGameObjects();
            StartGameLoop();
            OnFarmStateChanged(); // Update UI
        }
        else
        {
            StartNewGame();
        }
    }

    private void CreateWorkerGameObjects()
    {
        // Clear existing workers
        foreach (var worker in _workerGameObjects)
        {
            if (worker != null)
                Destroy(worker.gameObject);
        }
        _workerGameObjects.Clear();

        // Create worker GameObjects for each worker ID
        foreach (var workerId in _farm.WorkerIds)
        {
            CreateWorkerGameObject(workerId);
        }
    }

    private void CreateWorkerGameObject(int workerId)
    {
        GameObject workerGO = Instantiate(workerPrefab, farmContainer);
        workerGO.name = $"Worker_{workerId}";

        Worker worker = workerGO.GetComponent<Worker>();
        if (worker == null)
            worker = workerGO.AddComponent<Worker>();

        worker.Id = workerId;
        worker.TaskCompleted += OnWorkerTaskCompleted;
        worker.StateChanged += OnWorkerStateChanged;

        // Register worker with farm
        _farm.RegisterWorker(worker);
        _workerGameObjects.Add(worker);

        Debug.Log($"Created Worker {workerId}");
    }

    // Manual farming methods
    public bool PlantCrop(int plotId, CropType cropType)
    {
        var plot = _farm.GetPlot(plotId);
        if (plot == null || !plot.CanPlant)
        {
            ShowMessage("Cannot plant here!");
            return false;
        }

        if (!_farm.Inventory.UseSeeds(cropType, 1))
        {
            ShowMessage("Not enough seeds!");
            return false;
        }

        var crop = CreateCrop(cropType);
        plot.Plant(crop);
        OnFarmStateChanged();
        return true;
    }

    public bool PlaceAnimal(int plotId, AnimalType animalType)
    {
        var plot = _farm.GetPlot(plotId);
        if (plot == null || !plot.CanPlant)
        {
            ShowMessage("Cannot place animal here!");
            return false;
        }

        if (!_farm.Inventory.UseAnimals(animalType, 1))
        {
            ShowMessage("No animals available!");
            return false;
        }

        var animal = CreateAnimal(animalType);
        plot.Plant(animal);
        OnFarmStateChanged();
        return true;
    }

    public bool HarvestPlot(int plotId)
    {
        var plot = _farm.GetPlot(plotId);
        if (plot == null || !plot.CanHarvest)
        {
            ShowMessage("Nothing to harvest!");
            return false;
        }

        var product = plot.Harvest();
        if (product.HasValue && product.Value != ProductType.None)
        {
            _farm.Inventory.AddProduct(product.Value, GetProductionBonus());
            ShowMessage($"Harvested {product.Value}!");
            OnFarmStateChanged();
            return true;
        }

        return false;
    }

    // Worker automation methods
    public void AutoAssignTasks()
    {
        var tasks = _farm.GetWorkTasks();

        foreach (var task in tasks)
        {
            var availableWorker = _farm.GetBestAvailableWorker();
            if (availableWorker != null)
            {
                AssignTaskToWorker(availableWorker, task);
            }
            else
            {
                break; // No more available workers
            }
        }
    }

    private void AssignTaskToWorker(Worker worker, SimpleTask task)
    {
        IWorkTask workTask = task.Type switch
        {
            TaskType.Plant => new PlantTask(task.Plot, task.CropType.Value, _farm),
            TaskType.Harvest => new HarvestTask(task.Plot, _farm),
            TaskType.Milk => new MilkTask(task.Plot, _farm),
            _ => null
        };

        if (workTask != null)
        {
            worker.AssignTask(workTask, task.Plot);
            Debug.Log($"Assigned {task.Type} task to Worker {worker.Id} for plot {task.Plot.Id}");
        }
    }

    // Shop methods
    public bool BuySeeds(CropType cropType, int amount)
    {
        var cost = _config.GetSeedCost(cropType) * amount;
        if (_farm.SpendGold(cost))
        {
            _farm.Inventory.AddSeeds(cropType, amount);
            ShowMessage($"Bought {amount} {cropType} seeds for {cost} gold!");
            return true;
        }

        ShowMessage("Not enough gold!");
        return false;
    }

    public bool BuyAnimal(AnimalType animalType)
    {
        var cost = _config.GetAnimalCost(animalType);
        if (_farm.SpendGold(cost))
        {
            _farm.Inventory.AddAnimals(animalType, 1);
            ShowMessage($"Bought {animalType} for {cost} gold!");
            return true;
        }

        ShowMessage("Not enough gold!");
        return false;
    }

    public bool BuyPlot()
    {
        if (_farm.BuyPlot())
        {
            ShowMessage("Bought new plot for 500 gold!");
            return true;
        }

        ShowMessage("Not enough gold!");
        return false;
    }

    public bool BuyWorker()
    {
        if (_farm.BuyWorker())
        {
            // Create the new worker GameObject
            int newWorkerId = _farm.WorkerIds.Last();
            CreateWorkerGameObject(newWorkerId);

            ShowMessage("Hired new worker for 500 gold!");
            return true;
        }

        ShowMessage("Not enough gold!");
        return false;
    }

    public bool UpgradeEquipment()
    {
        if (_farm.SpendGold(_config.EquipmentUpgradeCost))
        {
            _farm.UpgradeEquipment();
            ShowMessage("Equipment upgraded for 500 gold!");
            return true;
        }

        ShowMessage("Not enough gold!");
        return false;
    }

    public bool SellProduct(ProductType productType, int amount)
    {
        if (_farm.Inventory.SellProduct(productType, amount))
        {
            var earnings = _config.GetProductValue(productType) * amount;
            _farm.AddGold(earnings);
            ShowMessage($"Sold {amount} {productType} for {earnings} gold!");
            return true;
        }

        ShowMessage("Not enough products to sell!");
        return false;
    }

    // Auto-harvest/plant methods
    public void AutoHarvestAll()
    {
        var readyPlots = _farm.GetPlotsReadyToHarvest();
        foreach (var plot in readyPlots)
        {
            var availableWorker = _farm.GetBestAvailableWorker();
            if (availableWorker != null)
            {
                var task = new SimpleTask
                {
                    Plot = plot,
                    Type = plot.Content is Cow ? TaskType.Milk : TaskType.Harvest
                };
                AssignTaskToWorker(availableWorker, task);
            }
        }
    }

    public void AutoPlantAll(CropType cropType)
    {
        var emptyPlots = _farm.GetEmptyPlotsForPlanting();
        foreach (var plot in emptyPlots)
        {
            if (_farm.Inventory.GetSeedCount(cropType) > 0)
            {
                var availableWorker = _farm.GetBestAvailableWorker();
                if (availableWorker != null)
                {
                    var task = new SimpleTask
                    {
                        Plot = plot,
                        Type = TaskType.Plant,
                        CropType = cropType
                    };
                    AssignTaskToWorker(availableWorker, task);
                }
            }
        }
    }

    private void StartGameLoop()
    {
        if (_gameLoopCoroutine != null)
            StopCoroutine(_gameLoopCoroutine);

        _gameLoopCoroutine = StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            // Auto-assign tasks every 5 seconds
            if (Time.time % 5f < 1f)
            {
                AutoAssignTasks();
            }

            // Auto-save every minute
            if (Time.time % 60f < 1f)
            {
                SaveGame();
            }

            // Check win condition
            if (_farm.Gold >= 1000000)
            {
                ShowMessage("Congratulations! You've reached 1 million gold!");
                // Handle win condition
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void CalculateOfflineProgress(TimeSpan offlineTime)
    {
        var offlineMinutes = (int)offlineTime.TotalMinutes;
        var message = $"Welcome back! You were offline for {offlineMinutes} minutes.\n";

        // Simple offline calculation - auto harvest ready plots
        int harvestedCount = 0;
        foreach (var plot in _farm.Plots)
        {
            if (plot.Content != null && plot.CanHarvest)
            {
                var product = plot.Harvest();
                if (product.HasValue && product.Value != ProductType.None)
                {
                    _farm.Inventory.AddProduct(product.Value, 1);
                    harvestedCount++;
                }
            }
        }

        if (harvestedCount > 0)
        {
            message += $"Auto-harvested {harvestedCount} products!";
        }
        else
        {
            message += "No products were ready to harvest.";
        }

        ShowMessage(message);
    }

    // Worker event handlers
    private void OnWorkerTaskCompleted(Worker worker)
    {
        Debug.Log($"Worker {worker.Id} completed task");
        OnFarmStateChanged();
    }

    private void OnWorkerStateChanged(Worker worker, WorkerState newState)
    {
        Debug.Log($"Worker {worker.Id} changed to {newState} state");
    }

    // Factory methods
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

    private IPlantable CreateAnimal(AnimalType animalType)
    {
        return animalType switch
        {
            AnimalType.Cow => new Cow(),
            _ => throw new ArgumentException("Unknown animal type")
        };
    }

    private int GetProductionBonus()
    {
        return 1 + (_farm.EquipmentLevel - 1) / 10;
    }

    private void OnFarmStateChanged()
    {
        // Update UI through abstraction
        _gameUI?.UpdateFarmData(_farm);
        
        // Keep existing events for backward compatibility
        FarmStateChanged?.Invoke(_farm);
    }

    private void ShowMessage(string message)
    {
        // Update UI through abstraction
        _gameUI?.ShowMessage(message);
        
        // Keep existing events for backward compatibility
        MessageDisplayed?.Invoke(message);
    }

    public void SaveGame()
    {
        _farm.LastSaveTime = DateTime.Now;
        SaveLoadSystem.SaveGame(_farm);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }

    private void OnDestroy()
    {
        // Clean up worker events
        foreach (var worker in _workerGameObjects)
        {
            if (worker != null)
            {
                worker.TaskCompleted -= OnWorkerTaskCompleted;
                worker.StateChanged -= OnWorkerStateChanged;
            }
        }
    }
}

// Work task implementations
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
