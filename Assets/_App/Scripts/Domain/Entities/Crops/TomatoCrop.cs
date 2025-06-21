using System;
using UnityEngine;

[Serializable]
public class TomatoCrop : Crop
{
    public override ProductType ProductType => ProductType.Tomato;
    public override string DisplayName => "Tomato";

    public TomatoCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }

    public TomatoCrop() : base()
    {
        this.growthTimeMinutes = 20;
        this.maxHarvests = 50;
    }
}