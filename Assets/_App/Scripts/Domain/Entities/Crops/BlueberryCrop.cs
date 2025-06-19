using System;
using UnityEngine;

[Serializable]
public class BlueberryCrop : Crop
{
    public override TimeSpan GrowthTime => TimeSpan.FromMinutes(15);
    public override int MaxHarvests => 40;
    public override ProductType ProductType => ProductType.Blueberry;
    public override string DisplayName => "Blueberry";
}