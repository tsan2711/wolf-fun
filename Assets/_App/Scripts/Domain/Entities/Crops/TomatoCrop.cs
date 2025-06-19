using System;
using UnityEngine;

[Serializable]
public class TomatoCrop : Crop
{
    public override TimeSpan GrowthTime => TimeSpan.FromMinutes(10);
    public override int MaxHarvests => 40;
    public override ProductType ProductType => ProductType.Tomato;
    public override string DisplayName => "Tomato";
}