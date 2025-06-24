using System;
[Serializable]
public class StrawberryCrop : Crop
{
    public override ProductType ProductType => ProductType.Strawberry;
    public override string DisplayName => IsReadyToHarvest() ? Farm.STRAWBERRYMATURE: Farm.STRAWBERRYSEED;
    public StrawberryCrop(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }
    public StrawberryCrop() : base()
    {
        // this.growthTimeMinutes = 15;
        // this.maxHarvests = 30;
    }
}
