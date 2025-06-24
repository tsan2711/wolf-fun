using System;
[Serializable]
public class BlueberryCrop : Crop
{
    public override ProductType ProductType => ProductType.Blueberry;
    public override string DisplayName => IsReadyToHarvest() ? Farm.BLUEBERRYMATURE : Farm.BLUEBERRYSEED;

    public BlueberryCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }

    public BlueberryCrop() : base()
    {
        // this.growthTimeMinutes = 25; 
        // this.maxHarvests = 40; 
    }
}