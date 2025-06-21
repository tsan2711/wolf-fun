using System;
using UnityEngine;

[Serializable]
public class StrawberryCrop : Crop
{
    public override ProductType ProductType => ProductType.Strawberry;
    public override string DisplayName => "Strawberry";
    public StrawberryCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }
    public StrawberryCrop() : base()
    {
        this.growthTimeMinutes = 15;
        this.maxHarvests = 30;
    }
}
