using UnityEngine;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;

public static class ConfigCSVGenerator
{
    [MenuItem("Tools/Generate Config CSVs")]
    public static void GenerateConfigCSVs()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "Config");
        
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }

        GenerateGeneralConfigCSV(configPath);
        GenerateCropConfigCSV(configPath);
        GenerateAnimalConfigCSV(configPath);
        GenerateProductConfigCSV(configPath);
        
        AssetDatabase.Refresh();
        Debug.Log($"Config CSV files generated at: {configPath}");
    }

    private static void GenerateGeneralConfigCSV(string path)
    {
        string filePath = Path.Combine(path, "general_config.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("ConfigName,Value");
            writer.WriteLine("WorkerCost,500");
            writer.WriteLine("EquipmentUpgradeCost,500");
            writer.WriteLine("PlotCost,500");
        }
    }

    private static void GenerateCropConfigCSV(string path)
    {
        string filePath = Path.Combine(path, "crop_config.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("CropType,SeedCost");
            writer.WriteLine("Tomato,30");
            writer.WriteLine("Blueberry,50");
            writer.WriteLine("Strawberry,40");
        }
    }

    private static void GenerateAnimalConfigCSV(string path)
    {
        string filePath = Path.Combine(path, "animal_config.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("AnimalType,AnimalCost");
            writer.WriteLine("Cow,100");
        }
    }

    private static void GenerateProductConfigCSV(string path)
    {
        string filePath = Path.Combine(path, "product_config.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("ProductType,ProductValue");
            writer.WriteLine("Tomato,5");
            writer.WriteLine("Blueberry,8");
            writer.WriteLine("Strawberry,12");
            writer.WriteLine("Milk,15");
        }
    }
}
#endif