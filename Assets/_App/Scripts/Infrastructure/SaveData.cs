
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int gold;
    public int maxPlot;
    public int maxWorker;
    public string lastSaveTime;
    public List<int> workerIds;
    
    // Inventory
    public int tomatoSeeds;
    public int blueberrySeeds;
    public int strawberrySeeds;
    public int cows;
    public int tomatoProducts;
    public int blueberryProducts;
    public int strawberryProducts;
    public int milkProducts;
    
    // Upgrade levels
    public int strawberryLevel;
    public int tomatoLevel;
    public int blueberryLevel;
    public int milkLevel;
    
    // Plots
    public List<PlotData> plots;
}

[Serializable]
public class PlotData
{
    public int id;
    public int zone;
    public string lastActionTime;
    
    public bool hasContent;
    public string contentType;
    public int growthTimeMinutes;
    public int maxHarvests;
    public int currentHarvests;
    public string plantedTime;
    public string lastHarvestTime;
}