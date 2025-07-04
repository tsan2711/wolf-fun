using System;
[Serializable]
public class Cow : Crop
{
    public override ProductType ProductType => ProductType.Milk;
    public override string DisplayName => "Cow";

    public Cow(int growthTimeMinutes, int maxHarvests)
        : base(growthTimeMinutes, maxHarvests)
    {
    }

    public Cow() : base()
    {
        // this.growthTimeMinutes = 30;
        // this.maxHarvests = 100;
    }
}