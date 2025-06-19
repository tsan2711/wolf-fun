using System;
using UnityEngine;

[Serializable]
public class StrawberryCrop : Crop
{
    public override TimeSpan GrowthTime => TimeSpan.FromMinutes(5);
    public override int MaxHarvests => 20;
    public override ProductType ProductType => ProductType.Strawberry;
    public override string DisplayName => "Strawberry";
}
