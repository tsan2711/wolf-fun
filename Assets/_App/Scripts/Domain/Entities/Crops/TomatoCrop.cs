using System;
[Serializable]
public class TomatoCrop : Crop
{
    public override ProductType ProductType => ProductType.Tomato;
    public override string DisplayName => IsReadyToHarvest() ? Farm.TOMATOMATURE : Farm.TOMATOSEED;

    public TomatoCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }

    public TomatoCrop() : base()
    {
        // this.growthTimeMinutes = 20;
        // this.maxHarvests = 50;
    }
}