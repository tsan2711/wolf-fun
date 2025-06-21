using System;
using UnityEngine;

[Serializable]
public class BlueberryCrop : Crop
{
    public override ProductType ProductType => ProductType.Blueberry;
    public override string DisplayName => "Blueberry";

    public BlueberryCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }

    public BlueberryCrop() : base()
    {
        this.growthTimeMinutes = 25; 
        this.maxHarvests = 40; 
    }
}