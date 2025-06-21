using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameConfig
{
    [Header("Default Values (Fallback)")]
    public int WorkerCost = 500;
    public int EquipmentUpgradeCost = 500;
    public int PlotCost = 500;

    private Dictionary<CropType, int> _seedCosts = new Dictionary<CropType, int>();
    private Dictionary<AnimalType, int> _animalCosts = new Dictionary<AnimalType, int>();
    private Dictionary<ProductType, int> _productValues = new Dictionary<ProductType, int>();
    private Dictionary<string, int> _generalConfig = new Dictionary<string, int>();
    private Dictionary<string, int> _harvestTimes = new Dictionary<string, int>();
    private Dictionary<string, int> _maxHarvests = new Dictionary<string, int>();
    private Dictionary<Rarity, WorkerRarityConfig> _workerConfigs = new Dictionary<Rarity, WorkerRarityConfig>();


    private bool _configLoaded = false;

    public GameConfig()
    {
        LoadConfigFromCSV();
    }

    private void LoadConfigFromCSV()
    {
        if (_configLoaded) return;

        try
        {
            LoadGeneralConfig();
            LoadCropConfig();
            LoadAnimalConfig();
            LoadProductConfig();
            LoadHarvestTimeConfig();
            LoadWorkerConfig();

            _configLoaded = true;
            Debug.Log("Game configuration loaded successfully from CSV files!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load config from CSV: {e.Message}. Using default values.");
            LoadDefaultValues();
        }
    }

    private void LoadWorkerConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "worker_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Worker config file not found at {path}. Using defaults.");
            LoadDefaultWorkerConfig();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        Debug.Log("Worker config lines count: " + lines.Length);
        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 6) // Rarity,Rate,SpeedMin,SpeedMax,DurationMin,DurationMax
            {
                string rarityName = values[0].Trim();
                if (System.Enum.TryParse<Rarity>(rarityName, out Rarity rarity) &&
                    float.TryParse(values[1].Trim(), out float rate) &&
                    float.TryParse(values[2].Trim(), out float speedMin) &&
                    float.TryParse(values[3].Trim(), out float speedMax) &&
                    float.TryParse(values[4].Trim(), out float durationMin) &&
                    float.TryParse(values[5].Trim(), out float durationMax))
                {
                    _workerConfigs[rarity] = new WorkerRarityConfig
                    {
                        Rate = rate,
                        SpeedMin = speedMin,
                        SpeedMax = speedMax,
                        DurationMin = durationMin,
                        DurationMax = durationMax
                    };

                    Debug.Log($"Loaded worker config for {rarity}: Rate={rate}%, Speed={speedMin}-{speedMax}, Duration={durationMin}-{durationMax}");
                }
            }
        }
    }

    private void LoadDefaultWorkerConfig()
    {
        _workerConfigs[Rarity.Common] = new WorkerRarityConfig { Rate = 60f, SpeedMin = 1.5f, SpeedMax = 2.0f, DurationMin = 120f, DurationMax = 150f };
        _workerConfigs[Rarity.Uncommon] = new WorkerRarityConfig { Rate = 25f, SpeedMin = 2.0f, SpeedMax = 2.5f, DurationMin = 100f, DurationMax = 130f };
        _workerConfigs[Rarity.Rare] = new WorkerRarityConfig { Rate = 10f, SpeedMin = 2.5f, SpeedMax = 3.0f, DurationMin = 80f, DurationMax = 110f };
        _workerConfigs[Rarity.Epic] = new WorkerRarityConfig { Rate = 4f, SpeedMin = 3.0f, SpeedMax = 3.5f, DurationMin = 60f, DurationMax = 90f };
        _workerConfigs[Rarity.Legendary] = new WorkerRarityConfig { Rate = 1f, SpeedMin = 3.5f, SpeedMax = 4.0f, DurationMin = 40f, DurationMax = 70f };
    }

    private void LoadHarvestTimeConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "harvest_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Harvest time config file not found at {path}. Using defaults.");
            LoadDefaultHarvestTimes();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        Debug.Log("Harvest time config lines count: " + lines.Length);
        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');
            Debug.Log("Processing line: " + values.Length);
            if (values.Length >= 3) // CẦN ÍT NHẤT 3 CỘT
            {
                string cropName = values[0].Trim();
                if (int.TryParse(values[1].Trim(), out int harvestTime) &&
                    int.TryParse(values[2].Trim(), out int maxHarvests))
                {
                    _harvestTimes[cropName] = harvestTime;
                    _maxHarvests[cropName] = maxHarvests;
                }
                Debug.Log("Loaded harvest time for crop: " + cropName +
                          ", Time: " + harvestTime + "");
            }
        }
    }


    private void LoadDefaultHarvestTimes()
    {
        _harvestTimes["Strawberry"] = 5;
        _harvestTimes["Tomato"] = 10;
        _harvestTimes["Blueberry"] = 15;
        _harvestTimes["Cow"] = 30;

        _maxHarvests["Strawberry"] = 3;
        _maxHarvests["Tomato"] = 4;
        _maxHarvests["Blueberry"] = 2;
        _maxHarvests["Cow"] = 10;
    }

    private void LoadGeneralConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "general_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"General config file not found at {path}. Using defaults.");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 2)
            {
                string configName = values[0].Trim();
                if (int.TryParse(values[1].Trim(), out int configValue))
                {
                    _generalConfig[configName] = configValue;
                }
            }
        }

        // Update class properties
        WorkerCost = GetGeneralConfig("WorkerCost", WorkerCost);
        EquipmentUpgradeCost = GetGeneralConfig("EquipmentUpgradeCost", EquipmentUpgradeCost);
        PlotCost = GetGeneralConfig("PlotCost", PlotCost);
    }

    private void LoadCropConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "crop_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Crop config file not found at {path}. Using defaults.");
            LoadDefaultCropValues();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 2)
            {
                string cropName = values[0].Trim();
                if (System.Enum.TryParse<CropType>(cropName, out CropType cropType) &&
                    int.TryParse(values[1].Trim(), out int seedCost))
                {
                    _seedCosts[cropType] = seedCost;
                }
            }
        }
    }

    private void LoadAnimalConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "animal_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Animal config file not found at {path}. Using defaults.");
            LoadDefaultAnimalValues();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 2)
            {
                string animalName = values[0].Trim();
                if (System.Enum.TryParse<AnimalType>(animalName, out AnimalType animalType) &&
                    int.TryParse(values[1].Trim(), out int animalCost))
                {
                    _animalCosts[animalType] = animalCost;
                }
            }
        }
    }

    private void LoadProductConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Config", "product_config.csv");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Product config file not found at {path}. Using defaults.");
            LoadDefaultProductValues();
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // Skip header
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length >= 2)
            {
                string productName = values[0].Trim();
                if (System.Enum.TryParse<ProductType>(productName, out ProductType productType) &&
                    int.TryParse(values[1].Trim(), out int productValue))
                {
                    _productValues[productType] = productValue;
                }
            }
        }
    }

    private void LoadDefaultValues()
    {
        LoadDefaultCropValues();
        LoadDefaultAnimalValues();
        LoadDefaultProductValues();
        LoadDefaultHarvestTimes();
    }

    private void LoadDefaultCropValues()
    {
        _seedCosts[CropType.Tomato] = 30;
        _seedCosts[CropType.Blueberry] = 50;
        _seedCosts[CropType.Strawberry] = 40;
    }

    private void LoadDefaultAnimalValues()
    {
        _animalCosts[AnimalType.Cow] = 100;
    }

    private void LoadDefaultProductValues()
    {
        _productValues[ProductType.Tomato] = 5;
        _productValues[ProductType.Blueberry] = 8;
        _productValues[ProductType.Strawberry] = 12;
        _productValues[ProductType.Milk] = 15;
    }

    // Public getter methods
    public int GetSeedCost(CropType cropType)
    {
        return _seedCosts.ContainsKey(cropType) ? _seedCosts[cropType] : 30; // Default fallback
    }

    public int GetAnimalCost(AnimalType animalType)
    {
        return _animalCosts.ContainsKey(animalType) ? _animalCosts[animalType] : 100; // Default fallback
    }

    public int GetProductValue(ProductType productType)
    {
        return _productValues.ContainsKey(productType) ? _productValues[productType] : 5; // Default fallback
    }

    public int GetGeneralConfig(string configName, int defaultValue)
    {
        return _generalConfig.ContainsKey(configName) ? _generalConfig[configName] : defaultValue;
    }

    public int GetHarvestTime(string cropType)
    {
        foreach (var key in _harvestTimes.Keys)
        {
            Debug.Log("Harvest time key: " + key);
        }

        return _harvestTimes.ContainsKey(cropType) ? _harvestTimes[cropType] : 10; // Default fallback
    }

    public int GetMaxHarvests(string cropName)
    {
        return _maxHarvests.ContainsKey(cropName) ? _maxHarvests[cropName] : 3; // Default fallback
    }

    public WorkerRarityConfig GetWorkerConfig(Rarity rarity)
    {
        return _workerConfigs.ContainsKey(rarity) ? _workerConfigs[rarity] : _workerConfigs[Rarity.Common];
    }

    public Rarity GenerateWorkerRarity()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulativeRate = 0f;

        // Sort by rate ascending (Common first, Legendary last)
        var sortedRarities = new List<Rarity> { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        foreach (var rarity in sortedRarities)
        {
            if (_workerConfigs.ContainsKey(rarity))
            {
                cumulativeRate += _workerConfigs[rarity].Rate;
                if (randomValue <= cumulativeRate)
                {
                    return rarity;
                }
            }
        }

        return Rarity.Common; // Fallback
    }

    public float GenerateWorkerSpeed(Rarity rarity)
    {
        var config = GetWorkerConfig(rarity);
        return UnityEngine.Random.Range(config.SpeedMin, config.SpeedMax);
    }

    public float GenerateWorkerDuration(Rarity rarity)
    {
        var config = GetWorkerConfig(rarity);
        return UnityEngine.Random.Range(config.DurationMin, config.DurationMax);
    }

    public void ReloadConfig()
    {
        _configLoaded = false;
        _seedCosts.Clear();
        _animalCosts.Clear();
        _productValues.Clear();
        _generalConfig.Clear();
        _harvestTimes.Clear();
        _maxHarvests.Clear();
        _workerConfigs.Clear(); 

        LoadConfigFromCSV();
    }
}
