using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{
    [Header("Worker Config")]
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private FillBar workerFillBar;
    [SerializeField] private Vector3 fillBarOffset = new Vector3(0, 1.5f, 0);

    [Header("Farm Settings")]
    [SerializeField] private Transform farmContainer;

    [Header("3D Farm Layout")]
    [SerializeField] private FarmManager farmLayoutManager;

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

        StartGame();
    }

    private void StartGame()
    {
        int isContinue = PlayerPrefs.GetInt("ContinueGame", 0);
        if (isContinue == 1)
            LoadGame();
        else
            StartNewGame();

    }

    private void InitializeFarm()
    {
        if (farmLayoutManager != null)
        {
            farmLayoutManager.Initialize(_farm);
        }
    }

    private void InitializeUI()
    {
        // Try to find Unity UI first
        _gameUI = FindAnyObjectByType<MainGameUI>();

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
        if (_gameUI == null) return;

        _gameUI.NewGameRequested += StartNewGame;
        _gameUI.ContinueGameRequested += LoadGame;
        _gameUI.BuySeedsRequested += (cropType, amount) => BuySeeds(cropType, amount);
        _gameUI.BuyAnimalRequested += (animalType) => BuyAnimal(animalType);
        _gameUI.BuyWorkerRequested += () => BuyWorker();
        _gameUI.BuyPlotRequested += () => BuyPlot();
        _gameUI.UpgradeEquipmentRequested += () => UpgradeEquipment();
        _gameUI.AutoHarvestRequested += () => AutoHarvestAll();
        _gameUI.AutoPlantRequested += (cropType) => AutoPlantAll(cropType);
        _gameUI.AutoPlaceAnimalRequested += (animalType) => AutoPlaceAnimalAll(animalType); // THÊM MỚI
        _gameUI.SellProductRequested += (productType, amount) => SellProduct(productType, amount);
    }


    public void StartNewGame()
    {
        _farm = new Farm();
        _farm.FarmStateChanged += OnFarmStateChanged;

        // Create initial worker GameObjects
        CreateWorkerGameObjects();

        InitializeFarm();

        StartGameLoop();
        OnFarmStateChanged(); // Update UI
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
        Vector3 scale = farmContainer.localScale;
        workerGO.transform.localScale = new Vector3(
            1f / scale.x,
            1f / scale.y,
            1f / scale.z
        );

        Worker worker = workerGO.GetComponent<Worker>();
        if (worker == null)
            worker = workerGO.AddComponent<Worker>();

        // Ensure NavMeshAgent is attached
        NavMeshAgent navAgent = workerGO.GetComponent<NavMeshAgent>();
        if (navAgent == null)
            navAgent = workerGO.AddComponent<NavMeshAgent>();

        // NEW: Generate random rarity and stats based on config
        Rarity workerRarity = _config.GenerateWorkerRarity();
        float workerSpeed = _config.GenerateWorkerSpeed(workerRarity);
        float workerDuration = _config.GenerateWorkerDuration(workerRarity);

        // Set worker properties
        worker.Id = workerId;
        worker.Rarity = workerRarity;
        worker.MoveSpeed = workerSpeed;
        worker.WorkDuration = workerDuration;


        // Subscribe to events
        worker.TaskCompleted += OnWorkerTaskCompleted;
        worker.StateChanged += OnWorkerStateChanged;

        // Simple spawn: use farmContainer position (already on NavMesh)
        Vector3 spawnPosition = farmContainer.position + Vector3.right * workerId * 2f;
        navAgent.Warp(spawnPosition);

        ValidateWorkerEvents(worker);

        workerGO.name = $"Worker_{workerId}_{workerRarity}";

        // Register worker with farm
        _farm.RegisterWorker(worker);
        _workerGameObjects.Add(worker);

        Debug.Log($"Created Worker {workerId} with rarity {workerRarity} (Speed: {workerSpeed:F2}, Duration: {workerDuration:F1}s) at {spawnPosition}");
    }

    private void ValidateWorkerEvents(Worker worker)
    {
        var fillBar = Instantiate(workerFillBar, worker.transform);
        fillBar.transform.localPosition = fillBarOffset; // Remove += operator
        fillBar.Deactivate(); // Start deactivated

        // Subscribe to TaskStateChanged for progress updates
        worker.TaskStateChanged += (progress) =>
        {
            if (!fillBar.gameObject.activeInHierarchy)
            {
                fillBar.Activate();
            }
            fillBar.SetProgress(progress);
        };

        // Subscribe to TaskCompleted for completion
        worker.TaskCompleted += (w) =>
        {
            fillBar.ResetProgress();
            fillBar.Deactivate();
        };

        // Subscribe to StateChanged for state-based activation
        worker.StateChanged += (w, state) =>
        {
            if (state == WorkerState.Working)
                fillBar.Activate();
            else if (state == WorkerState.Idle)
                fillBar.Deactivate();
        };
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

    public new bool PlaceAnimal(int plotId, AnimalType animalType)
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

        // Visual feedback
        ShowMessage($"Placed {animalType} on plot {plotId}!");
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
        var availableTasks = _farm.GetWorkTasks();
        var availableWorkers = _farm.GetWorkers().Where(w => w.IsAvailable).ToList();

        Debug.Log($"AutoAssignTasks: {availableTasks.Count} tasks, {availableWorkers.Count} available workers");

        // Assign one task per worker
        int assignedTasks = 0;
        for (int i = 0; i < Math.Min(availableTasks.Count, availableWorkers.Count); i++)
        {
            var task = availableTasks[i];
            var worker = availableWorkers[i];

            Debug.Log($"Attempting to assign task for plot {task.Plot.Id} to worker {worker.Id}");

            if (AssignTaskToWorker(worker, task))
            {
                assignedTasks++;
                Debug.Log($"Successfully assigned task to worker {worker.Id}");
            }
            else
            {
                Debug.Log($"Failed to assign task to worker {worker.Id}");
            }
        }

        Debug.Log($"Total tasks assigned: {assignedTasks}");
    }



    private bool AssignTaskToWorker(Worker worker, SimpleTask task)
    {
        // Check if worker is still available
        if (!worker.IsAvailable)
        {
            Debug.Log($"Worker {worker.Id} is no longer available");
            return false;
        }

        // Check if plot is still harvestable and not reserved
        if (!task.Plot.CanHarvest)
        {
            Debug.Log($"Plot {task.Plot.Id} is no longer harvestable");
            return false;
        }

        if (_farm.IsPlotReserved(task.Plot.Id))
        {
            Debug.Log($"Plot {task.Plot.Id} is already reserved");
            return false;
        }

        IWorkTask workTask = task.Type switch
        {
            TaskType.Harvest => new HarvestTask(task.Plot, _farm),
            TaskType.Milk => new MilkTask(task.Plot, _farm),
            _ => null
        };

        if (workTask != null)
        {
            worker.AssignTask(workTask, task.Plot);
            Debug.Log($"Assigned {task.Type} task to Worker {worker.Id} for plot {task.Plot.Id}");
            return true;
        }
        else
        {
            Debug.LogWarning($"No work task created for type: {task.Type}");
            return false;
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
        if (readyPlots.Count == 0)
        {
            ShowMessage("No plots ready to harvest!");
            return;
        }

        // Kiểm tra có workers không
        var availableWorkers = _farm.GetAvailableWorkers();
        if (availableWorkers == 0)
        {
            ShowMessage("No workers available for harvesting!");
            return;
        }

        int tasksAssigned = 0;
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
                tasksAssigned++;
            }
            else
            {
                break; // No more workers available
            }
        }

        if (tasksAssigned > 0)
        {
            ShowMessage($"Assigned {tasksAssigned} harvest tasks to workers!");
        }
        else
        {
            ShowMessage("No workers available for harvesting!");
        }
    }
    public void AutoPlantAll(CropType cropType)
    {
        var emptyPlots = _farm.GetEmptyPlotsForCrop(cropType);
        Debug.Log($"Empty plots for {cropType}: {emptyPlots.Count}");

        if (emptyPlots.Count == 0)
        {
            ShowMessage($"No empty {cropType} plots available!");
            return;
        }

        if (_farm.Inventory.GetSeedCount(cropType) == 0)
        {
            ShowMessage($"No {cropType} seeds available!");
            return;
        }

        int plantsPlanted = 0;
        foreach (var plot in emptyPlots)
        {
            if (_farm.Inventory.GetSeedCount(cropType) > 0)
            {
                if (plot.CanPlantType(cropType) && _farm.Inventory.UseSeeds(cropType, 1))
                {
                    var crop = CreateCrop(cropType);
                    plot.Plant(crop);
                    plantsPlanted++;
                    Debug.Log($"Auto planted {cropType} on plot {plot.Id} (Zone: {plot.Zone})");
                }
            }
            else
            {
                break;
            }
        }

        if (plantsPlanted > 0)
        {
            ShowMessage($"Auto planted {plantsPlanted} {cropType} crops in {cropType} zone!");

            // FORCE trigger farm state changed
            _farm.TriggerFarmStateChanged();
            OnFarmStateChanged();

            Debug.Log("Auto plant completed - FarmStateChanged triggered");
        }
        else
        {
            ShowMessage($"Could not plant any {cropType} in {cropType} zone!");
        }
    }

    public void AutoPlaceAnimalAll(AnimalType animalType)
    {
        var emptyPlots = _farm.GetEmptyPlotsForAnimal(animalType);

        if (emptyPlots.Count == 0)
        {
            ShowMessage($"No empty {animalType} plots available!");
            return;
        }

        if (_farm.Inventory.GetAnimalCount(animalType) == 0)
        {
            ShowMessage($"No {animalType} available!");
            return;
        }

        int animalsPlaced = 0;
        foreach (var plot in emptyPlots)
        {
            if (_farm.Inventory.GetAnimalCount(animalType) > 0)
            {
                if (plot.CanPlaceAnimal(animalType) && _farm.Inventory.UseAnimals(animalType, 1))
                {
                    var animal = CreateAnimal(animalType);
                    plot.Plant(animal);
                    animalsPlaced++;
                    Debug.Log($"Auto placed {animalType} on plot {plot.Id} (Zone: {plot.Zone})");
                }
            }
            else
            {
                break;
            }
        }

        if (animalsPlaced > 0)
        {
            ShowMessage($"Auto placed {animalsPlaced} {animalType} in cow zone!");

            // FORCE trigger farm state changed
            _farm.TriggerFarmStateChanged();
            OnFarmStateChanged();

            Debug.Log("Auto place animal completed - FarmStateChanged triggered");
        }
        else
        {
            ShowMessage($"Could not place any {animalType} in cow zone!");
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
        int harvestTime = _config.GetHarvestTime(cropType.ToString());
        int maxHarvests = _config.GetMaxHarvests(cropType.ToString());

        Debug.Log("Creating crop: " + cropType +
                  " with harvest time: " + harvestTime +
                  " and max harvests: " + maxHarvests);

        return cropType switch
        {
            CropType.Tomato => new TomatoCrop(harvestTime, maxHarvests),
            CropType.Blueberry => new BlueberryCrop(harvestTime, maxHarvests),
            CropType.Strawberry => new StrawberryCrop(harvestTime, maxHarvests),
            _ => throw new ArgumentException("Unknown crop type")
        };
    }

    private IPlantable CreateAnimal(AnimalType animalType)
    {
        int harvestTime = _config.GetHarvestTime(animalType.ToString());
        int maxHarvests = _config.GetMaxHarvests(animalType.ToString());

        Debug.Log("Creating animal: " + animalType +
                  " with harvest time: " + harvestTime +
                  " and max harvests: " + maxHarvests);


        return animalType switch
        {
            AnimalType.Cow => new Cow(harvestTime, maxHarvests),
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