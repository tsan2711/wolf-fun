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
            
            _configLoaded = true;
            Debug.Log("Game configuration loaded successfully from CSV files!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load config from CSV: {e.Message}. Using default values.");
            LoadDefaultValues();
        }
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

    // Reload config at runtime (useful for testing/balancing)
    public void ReloadConfig()
    {
        _configLoaded = false;
        _seedCosts.Clear();
        _animalCosts.Clear();
        _productValues.Clear();
        _generalConfig.Clear();
        
        LoadConfigFromCSV();
    }

    // Debug method to print all loaded config
    public void PrintLoadedConfig()
    {
        Debug.Log("=== LOADED GAME CONFIG ===");
        Debug.Log($"Worker Cost: {WorkerCost}");
        Debug.Log($"Equipment Upgrade Cost: {EquipmentUpgradeCost}");
        Debug.Log($"Plot Cost: {PlotCost}");
        
        Debug.Log("Seed Costs:");
        foreach (var kvp in _seedCosts)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        Debug.Log("Animal Costs:");
        foreach (var kvp in _animalCosts)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        Debug.Log("Product Values:");
        foreach (var kvp in _productValues)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}
