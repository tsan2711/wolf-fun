using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;

public static class SaveLoadSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public static void SaveGame(Farm farm)
    {
        try
        {
            var saveData = new SaveData
            {
                gold = farm.Gold,
                maxPlot = farm.MaxPlot,
                maxWorker = farm.MaxWorker,
                lastSaveTime = farm.LastSaveTime.ToBinary().ToString(),
                workerIds = farm.WorkerIds,
                
                // Inventory
                tomatoSeeds = farm.Inventory.GetSeedCount(CropType.Tomato),
                blueberrySeeds = farm.Inventory.GetSeedCount(CropType.Blueberry),
                strawberrySeeds = farm.Inventory.GetSeedCount(CropType.Strawberry),
                cows = farm.Inventory.GetAnimalCount(AnimalType.Cow),
                
                tomatoProducts = farm.Inventory.GetProductCount(ProductType.Tomato),
                blueberryProducts = farm.Inventory.GetProductCount(ProductType.Blueberry),
                strawberryProducts = farm.Inventory.GetProductCount(ProductType.Strawberry),
                milkProducts = farm.Inventory.GetProductCount(ProductType.Milk),
                
                // Upgrade levels
                strawberryLevel = farm.GetEquipmentLevel(ProductType.Strawberry),
                tomatoLevel = farm.GetEquipmentLevel(ProductType.Tomato),
                blueberryLevel = farm.GetEquipmentLevel(ProductType.Blueberry),
                milkLevel = farm.GetEquipmentLevel(ProductType.Milk),
                
                // Plots - simplified
                plots = CreatePlotData(farm.Plots)
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            
            PlayerPrefs.SetInt("ContinueGame", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"Game saved successfully with {saveData.plots.Count} plots");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public static Farm LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("Save file not found!");
                return null;
            }

            string json = File.ReadAllText(SavePath);
            var saveData = JsonUtility.FromJson<SaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("Failed to parse save data!");
                return null;
            }

            var farm = new Farm(isLoadedGame: true);
            
            farm.SetGold(saveData.gold);
            farm.SetMaxPlot(saveData.maxPlot);
            farm.SetMaxWorker(saveData.maxWorker);
            
            if (long.TryParse(saveData.lastSaveTime, out long timeBinary))
            {
                farm.LastSaveTime = DateTime.FromBinary(timeBinary);
            }

            farm.WorkerIds.Clear();
            farm.WorkerIds.AddRange(saveData.workerIds);

            farm.Inventory.Seeds.Clear();
            farm.Inventory.Animals.Clear();
            farm.Inventory.Products.Clear();
            
            farm.Inventory.AddSeeds(CropType.Tomato, saveData.tomatoSeeds);
            farm.Inventory.AddSeeds(CropType.Blueberry, saveData.blueberrySeeds);
            farm.Inventory.AddSeeds(CropType.Strawberry, saveData.strawberrySeeds);
            farm.Inventory.AddAnimals(AnimalType.Cow, saveData.cows);
            
            farm.Inventory.AddProduct(ProductType.Tomato, saveData.tomatoProducts);
            farm.Inventory.AddProduct(ProductType.Blueberry, saveData.blueberryProducts);
            farm.Inventory.AddProduct(ProductType.Strawberry, saveData.strawberryProducts);
            farm.Inventory.AddProduct(ProductType.Milk, saveData.milkProducts);

            farm.SetEquipmentLevel(ProductType.Strawberry, saveData.strawberryLevel);
            farm.SetEquipmentLevel(ProductType.Tomato, saveData.tomatoLevel);
            farm.SetEquipmentLevel(ProductType.Blueberry, saveData.blueberryLevel);
            farm.SetEquipmentLevel(ProductType.Milk, saveData.milkLevel);

            // Restore plots
            farm.Plots.Clear();
            RestorePlots(farm, saveData.plots);

            Debug.Log($"Game loaded successfully with {farm.Plots.Count} plots");
            return farm;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return null;
        }
    }

    private static List<PlotData> CreatePlotData(List<Plot> plots)
    {
        var plotDatas = new List<PlotData>();
        
        foreach (var plot in plots)
        {
            var plotData = new PlotData
            {
                id = plot.Id,
                zone = (int)plot.Zone,
                lastActionTime = plot.LastActionTime.ToBinary().ToString()
            };

            if (plot.Content != null)
            {
                plotData.hasContent = true;
                plotData.contentType = GetContentTypeString(plot.Content);
                plotData.growthTimeMinutes = plot.Content.GetGrowthTimeMinutes();
                plotData.maxHarvests = plot.Content.GetMaxHarvests();
                plotData.currentHarvests = plot.Content.GetCurrentHarvests();
                plotData.plantedTime = plot.Content.GetPlantedTime().ToBinary().ToString();
                plotData.lastHarvestTime = plot.Content.GetLastHarvestTime().ToBinary().ToString();
                
                Debug.Log($"SAVING Plot {plot.Id}: {plotData.contentType} " +
                         $"Harvests={plotData.currentHarvests}/{plotData.maxHarvests} " +
                         $"Ready={plot.Content.IsReadyToHarvest()}");
            }
            else
            {
                Debug.Log($"SAVING Plot {plot.Id}: Empty");
            }
            
            plotDatas.Add(plotData);
        }
        
        return plotDatas;
    }

    private static void RestorePlots(Farm farm, List<PlotData> plotsData)
    {
        foreach (var plotData in plotsData)
        {
            var plot = new Plot(farm, plotData.id, (PlotZone)plotData.zone);
            
            // Restore last action time
            if (long.TryParse(plotData.lastActionTime, out long timeBinary))
            {
                plot.SetLastActionTime(DateTime.FromBinary(timeBinary));
            }

            // Restore content if exists - DIRECT SET, không qua Plant()
            if (plotData.hasContent && !string.IsNullOrEmpty(plotData.contentType))
            {
                var content = CreateContentFromString(plotData.contentType, plotData);
                if (content != null)
                {
                    // TRỰC TIẾP set content mà KHÔNG gọi Plant()
                    plot.SetContent(content);
                    
                    Debug.Log($"Plot {plotData.id} restored: {content.GetDisplayName()} " +
                             $"[{content.GetCurrentHarvests()}/{content.GetMaxHarvests()}] " +
                             $"Ready: {content.IsReadyToHarvest()}");
                }
            }
            
            farm.Plots.Add(plot);
        }
    }

    private static string GetContentTypeString(IPlantable content)
    {
        return content switch
        {
            TomatoCrop => "TomatoCrop",
            BlueberryCrop => "BlueberryCrop", 
            StrawberryCrop => "StrawberryCrop",
            Cow => "Cow",
            _ => ""
        };
    }

    private static IPlantable CreateContentFromString(string contentType, PlotData data)
    {
        var gameController = GameController.Instance;
        if (gameController == null) return null;

        IPlantable content = contentType switch
        {
            "TomatoCrop" => gameController.CreateCrop(CropType.Tomato),
            "BlueberryCrop" => gameController.CreateCrop(CropType.Blueberry),
            "StrawberryCrop" => gameController.CreateCrop(CropType.Strawberry),
            "Cow" => gameController.CreateAnimal(AnimalType.Cow),
            _ => null
        };

        if (content != null)
        {
            // QUAN TRỌNG: Restore data mà KHÔNG trigger logic Plant()
            RestoreContentData(content, data);
        }

        return content;
    }

    private static void RestoreContentData(IPlantable content, PlotData data)
    {
        // Restore ALL data directly WITHOUT calling Plant() or other methods
        content.SetGrowthTime(data.growthTimeMinutes);
        content.SetMaxHarvests(data.maxHarvests);
        
        // Restore times first
        if (long.TryParse(data.plantedTime, out long plantedBinary))
        {
            content.SetPlantedTime(DateTime.FromBinary(plantedBinary));
        }
        
        if (long.TryParse(data.lastHarvestTime, out long harvestBinary))
        {
            content.SetLastHarvestTime(DateTime.FromBinary(harvestBinary));
        }
        
        // Restore harvest count LAST và EXACTLY như đã save
        content.SetCurrentHarvests(data.currentHarvests);
        
        Debug.Log($"Restored {data.contentType}: Harvests={data.currentHarvests}/{data.maxHarvests}, " +
                 $"Growth={data.growthTimeMinutes}min, Ready={content.IsReadyToHarvest()}");
    }

    public static bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                PlayerPrefs.SetInt("ContinueGame", 0);
                PlayerPrefs.Save();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }
}
