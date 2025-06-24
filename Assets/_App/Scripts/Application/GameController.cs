using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

public class GameController : Singleton<GameController>
{
    [Header("Worker Config")]
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private FillBar workerFillBar;
    [SerializeField] private Vector3 fillBarOffset = new Vector3(0, 1.5f, 0);

    [Header("Farm Settings")]
    [SerializeField] private Transform farmContainer;

    [Header("3D Farm Layout")]
    [SerializeField] private FarmManager farmLayoutManager;
    public FarmManager FarmLayoutManager => farmLayoutManager;
    [Header("Day Night Settings")]
    [SerializeField] private DayNightSystem dayNightSystem;
    public DayNightSystem DayNightSystem => dayNightSystem;

    private Farm _farm;
    private GameConfig _config;
    private Coroutine _gameLoopCoroutine;
    private List<Worker> _workerGameObjects = new List<Worker>();
    public event Action<Farm> FarmStateChanged;
    public event Action<string> MessageDisplayed;
    private CancellationTokenSource _gameLoopCancellation;

    protected override bool IsDontDestroyOnLoad => false;

    public Farm Farm => _farm;

    private IGameUI _gameUI;

    protected override void Awake()
    {
        base.Awake();
        // DontDestroyOnLoad(gameObject);
        _config = new GameConfig();

        InitializeUI();

        StartGame();

        OnFarmStateChanged();
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
        _gameUI.UpgradeEquipmentRequested += (productType) => UpgradeEquipment(productType);
        _gameUI.AutoHarvestRequested += () => AutoHarvestAll();
        _gameUI.AutoPlantRequested += (cropType) => AutoPlantAll(cropType);
        _gameUI.AutoPlaceAnimalRequested += (animalType) => AutoPlaceAnimalAll(animalType); // THÊM MỚI
        _gameUI.SellProductRequested += (productType, amount) => SellProduct(productType, amount);
        _gameUI.UIInitializeCompleted += OnFarmStateChanged; // Ensure UI is ready
        _gameUI.OnHourPassed += HandleHourPassed;
    }


    public void StartNewGame()
    {
        CleanupCurrentGame();

        _farm = new Farm(false);
        _farm.FarmStateChanged += OnFarmStateChanged;

        // Create initial worker GameObjects

        InitializeFarm();

        StartGameLoop();

        CreateWorkerGameObjects();

        OnFarmStateChanged(); // Update UI
    }

    public void LoadGame()
    {
        Debug.Log("Starting game load...");

        var savedGame = SaveLoadSystem.LoadGame();
        if (savedGame == null)
        {
            Debug.LogWarning("Could not load saved game, starting new game instead");
            StartNewGame();
        }




        Debug.Log($"Loaded farm with {savedGame.Plots.Count} plots");

        // Cleanup trước khi load
        CleanupCurrentGame();

        _farm = savedGame;

        if (_farm == null)
        {
            StartNewGame();
            return;
        }

        // Verify farm has necessary collections
        if (_farm.Plots == null)
        {
            StartNewGame();
            return;
        }

        if (_farm.WorkerIds == null)
        {
            StartNewGame();
            return;
        }

        _farm.FarmStateChanged += OnFarmStateChanged;

        // Calculate offline progress
        var offlineTime = DateTime.Now - _farm.LastSaveTime;
        if (offlineTime.TotalMinutes > 1)
        {
            CalculateOfflineProgress(offlineTime);
        }

        InitializeFarm();

        // Tạo workers với safety check
        try
        {
            CreateWorkerGameObjects();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error creating workers: {e.Message}");
        }

        // Start game loop
        StartGameLoop();

        // Update UI
        OnFarmStateChanged();

        Debug.Log($"Game loaded successfully! Plots: {_farm.Plots.Count}, Workers: {_farm.WorkerIds.Count}");
    }

    private void StartGameLoop()
    {
        StopGameLoop();
        _gameLoopCancellation = new CancellationTokenSource();
        var token = _gameLoopCancellation.Token;

        AutoAssignTaskAsync(token).Forget();
        AutoSaveGameAsync(token).Forget();
        AutoCheckWinConditionAsync(token).Forget();
        AutoUpdatePlotStatesAsync(token).Forget();
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
            2f / scale.x,
            2f / scale.y,
            2f / scale.z
        );

        Worker worker = workerGO.GetComponent<Worker>();
        if (worker == null)
            worker = workerGO.AddComponent<Worker>();

        // Ensure NavMeshAgent is attached
        NavMeshAgent navAgent = workerGO.GetComponent<NavMeshAgent>();
        if (navAgent == null)
            navAgent = workerGO.AddComponent<NavMeshAgent>();

        // Generate random rarity and stats based on config
        Rarity workerRarity = _config.GenerateWorkerRarity();
        float workerSpeed = _config.GenerateWorkerSpeed(workerRarity);
        float workerDuration = _config.GenerateWorkerDuration(workerRarity);

        // Set worker properties
        worker.Id = workerId;
        worker.Rarity = workerRarity;
        worker.MoveSpeed = workerSpeed;
        worker.WorkDuration = workerDuration;


        // Subscribe to events

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

        worker.StateChanged += OnWorkerStateChanged;

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
            OnWorkerTaskCompleted(w);
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
        if (!worker.IsAvailable) return false;

        bool canAssignTask = task.Type switch
        {
            TaskType.Harvest => task.Plot.CanHarvest, // Cả harvest và milk
            TaskType.Plant => task.Plot.CanPlant && task.CropType.HasValue &&
                             _farm.Inventory.GetSeedCount(task.CropType.Value) > 0,
            _ => false
        };

        if (!canAssignTask || _farm.IsPlotReserved(task.Plot.Id)) return false;

        // CREATE WORK TASK
        IWorkTask workTask = task.Type switch
        {
            TaskType.Harvest => new HarvestTask(task.Plot, _farm), // SỬ DỤNG CHUNG
            TaskType.Plant => new PlantTask(task.Plot, task.CropType.Value, _farm),
            _ => null
        };

        if (workTask != null)
        {
            worker.AssignTask(workTask, task.Plot);
            Debug.Log($"Assigned {task.Type} task to Worker {worker.Id} for plot {task.Plot.Id}");
            return true;
        }

        return false;
    }

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
        if (!_farm.DoesReachMaxPlots())
        {
            ShowMessage("Cannot expand farm, max plots reached!");
            return false;
        }
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
        if (!_farm.DoesReachMaxWorkers())
        {
            ShowMessage("Cannot hire more workers, max workers reached!");
            return false;
        }

        if (_farm.BuyWorker())
        {
            // Create the new worker GameObject
            int newWorkerId = _farm.WorkerIds.Last();
            CreateWorkerGameObject(newWorkerId);

            OnFarmStateChanged();
            ShowMessage("Hired new worker for 500 gold!");
            return true;
        }


        ShowMessage("Not enough gold!");
        return false;
    }

    public bool UpgradeEquipment(ProductType productType)
    {
        if (_farm.SpendGold(_config.EquipmentUpgradeCost))
        {
            _farm.UpgradeEquipment(productType);
            ShowMessage("Equipment upgraded for 500 gold! " + productType);
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



    private async UniTaskVoid AutoUpdatePlotStatesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(10000, cancellationToken: token); // Check every 10 seconds

            if (_farm?.Plots != null)
            {
                bool anyStateChanged = false;

                foreach (var plot in _farm.Plots)
                {
                    if (plot.Content != null)
                    {
                        // Check if content is ready to harvest and update plot state
                        bool wasReady = plot.CanHarvest;
                        bool isReady = plot.Content.IsReadyToHarvest();

                        if (wasReady != isReady)
                        {
                            anyStateChanged = true;
                            _farm.TriggerPlotStateChanged(plot);

                            Debug.Log($"Plot {plot.Id} state changed - Ready to harvest: {isReady}");
                        }
                    }
                }

                if (anyStateChanged)
                {
                    OnFarmStateChanged();
                }
            }
        }
    }
    private async UniTaskVoid AutoCheckWinConditionAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_farm.Gold >= 1000000)
            {
                FinishGame();
                return;
            }
            await UniTask.Delay(1000, cancellationToken: token);
        }
    }

    private async UniTaskVoid AutoAssignTaskAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(5000, cancellationToken: token);
            AutoAssignTasks();
        }
    }

    private async UniTaskVoid AutoSaveGameAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(60000, cancellationToken: token);
            SaveGame();
        }
    }


    private void StopGameLoop()
    {
        if (_gameLoopCancellation == null) return;
        _gameLoopCancellation?.Cancel();
        _gameLoopCancellation?.Dispose();
        _gameLoopCancellation = null;
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
        _gameUI?.UpdateWorkerState();
    }

    // Factory methods
    public IPlantable CreateCrop(CropType cropType)
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

    public IPlantable CreateAnimal(AnimalType animalType)
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

    private int GetProductionBonus(ProductType productType = ProductType.None)
    {
        if (productType == ProductType.None) return 0;

        var equipmentLevel = _farm.GetEquipmentLevel(productType);
        return (int)(equipmentLevel * 0.1f);
    }
    private void OnFarmStateChanged()
    {
        // Update UI through abstraction
        _gameUI?.UpdateFarmData(_farm);

        // Keep existing events for backward compatibility
        FarmStateChanged?.Invoke(_farm);
    }

    private void HandleHourPassed(int hour) => dayNightSystem?.HandleHourUpdate(hour);


    #region Cleanup Methods

    private void CleanupCurrentGame()
    {
        Debug.Log("=== Starting New Game Cleanup ===");

        StopGameLoop();

        CleanupWorkers();

        CleanupFarmVisuals();

        CleanupFarmData();

        ClearGameSaveData();

        Debug.Log("=== New Game Cleanup Complete ===");
    }

    private void ClearGameSaveData()
    {
        PlayerPrefs.SetInt("ContinueGame", 0);

        PlayerPrefs.Save();
        Debug.Log("Game save data cleared");
    }

    private void CleanupFarmData()
    {
        if (_farm != null)
        {
            Debug.Log("Cleaning up old farm data");

            // Unsubscribe from farm events
            _farm.FarmStateChanged -= OnFarmStateChanged;
            _farm.GoldChanged -= null; // Clear any other subscriptions

            // Clear any farm-specific data
            _farm = null;
        }
    }

    private void CleanupFarmVisuals()
    {
        if (farmLayoutManager != null)
        {
            // Let FarmManager handle its own cleanup
            farmLayoutManager.CleanupForNewGame();
        }
    }

    private void CleanupWorkers()
    {
        Debug.Log($"Cleaning up {_workerGameObjects.Count} workers");

        foreach (var worker in _workerGameObjects)
        {
            if (worker != null)
            {
                // Unsubscribe from events to prevent memory leaks
                worker.TaskCompleted -= OnWorkerTaskCompleted;
                worker.StateChanged -= OnWorkerStateChanged;
                worker.TaskStateChanged -= null; // Clear any lambda subscriptions

                // Destroy GameObject
                if (worker.gameObject != null)
                {
                    Destroy(worker.gameObject);
                }
            }
        }

        _workerGameObjects.Clear();
        Debug.Log("Worker cleanup complete");
    }





    #endregion

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



    protected override void OnDestroy()
    {
        base.OnDestroy();
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


    #region Unit Test

    [ContextMenu("Add 100k Gold")]
    private void Add100kGold()
    {
        if (Farm == null) return;

        Farm.AddGold(100000);
    }

    [ContextMenu("Add 1M Gold")]
    private void Add1MGold()
    {
        if (Farm == null) return;

        Farm.AddGold(1000000);
    }

    [ContextMenu("Add Full Inventory")]
    private void AddFullInventory()
    {
        if (Farm == null) return;
        if (Farm.Inventory == null) return;

        var inventory = Farm.Inventory;
        inventory.AddSeeds(CropType.Tomato, 50);
        inventory.AddSeeds(CropType.Blueberry, 50);
        inventory.AddSeeds(CropType.Strawberry, 50);
        inventory.AddAnimals(AnimalType.Cow, 10);
        inventory.AddProduct(ProductType.Tomato, 100);
        inventory.AddProduct(ProductType.Blueberry, 100);
        inventory.AddProduct(ProductType.Strawberry, 100);
        inventory.AddProduct(ProductType.Milk, 100);

        Debug.Log("📦 Filled inventory with items");
    }


    [ContextMenu("Plant All Crop Types")]
    private void PlantAllCropTypes()
    {
        if (Farm?.Inventory == null) return;

        // Add seeds first
        Farm.Inventory.AddSeeds(CropType.Tomato, 5);
        Farm.Inventory.AddSeeds(CropType.Blueberry, 5);
        Farm.Inventory.AddSeeds(CropType.Strawberry, 5);

        // Auto plant all
        AutoPlantAll(CropType.Tomato);
        AutoPlantAll(CropType.Blueberry);
        AutoPlantAll(CropType.Strawberry);

        Debug.Log("Planted all crop types for testing");
    }

    [ContextMenu("Add 3 Workers")]
    private void Add3Workers()
    {
        Farm.AddGold(2000); // Enough for 3 workers (500 each)

        for (int i = 0; i < 3; i++)
        {
            BuyWorker();
        }

        Debug.Log($"Added workers. Total: {Farm.GetTotalWorkers()}");
    }
    [ContextMenu("Expand Farm")]
    private void TestExpandFarm()
    {
        Farm.AddGold(5000);

        int initialPlots = Farm.GetTotalPlots();

        // Buy 10 new plots
        for (int i = 0; i < 10; i++)
        {
            BuyPlot();
        }

        int newPlots = Farm.GetTotalPlots() - initialPlots;
        Debug.Log($"🏡 Expanded farm by {newPlots} plots. Total: {Farm.GetTotalPlots()}");
    }



    #endregion




    private async void FinishGame()
    {
        ShowMessage("Congratulations! You've reached 1 million gold!");

        await UniTask.Delay(2000);


        // Stop any ongoing tasks
        StopGameLoop();

        // Clear farm data
        CleanupFarmData();

        CleanupUI();

        // Clear workers
        CleanupWorkers();

        // Clear farm visuals
        CleanupFarmVisuals();

        DestroyInstance();


        if (_gameLoopCancellation != null)
        {
            _gameLoopCancellation.Cancel();
            _gameLoopCancellation.Dispose();
            _gameLoopCancellation = null;
        }

        SceneController.Menu();

        Debug.Log("Game finished successfully!");
    }

    private void DestroyInstance()
    {
        if (Instance == this)
        {
            Destroy(gameObject);
        }
    }

    private void CleanupUI()
    {
        if (_gameUI != null)
        {
            // Unsubscribe from UI events
            _gameUI.NewGameRequested -= StartNewGame;
            _gameUI.ContinueGameRequested -= LoadGame;
            _gameUI.BuySeedsRequested -= (cropType, amount) => BuySeeds(cropType, amount);
            // ... unsubscribe all events

            _gameUI = null;
        }

        // Clear events
        FarmStateChanged = null;
        MessageDisplayed = null;
    }
}