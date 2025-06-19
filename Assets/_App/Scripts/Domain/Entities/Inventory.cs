using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    public Dictionary<CropType, int> Seeds { get; private set; } = new Dictionary<CropType, int>();
    public Dictionary<AnimalType, int> Animals { get; private set; } = new Dictionary<AnimalType, int>();
    public Dictionary<ProductType, int> Products { get; private set; } = new Dictionary<ProductType, int>();

    public void AddSeeds(CropType type, int amount)
    {
        if (!Seeds.ContainsKey(type)) Seeds[type] = 0;
        Seeds[type] += amount;
    }

    public bool UseSeeds(CropType type, int amount)
    {
        if (Seeds.ContainsKey(type) && Seeds[type] >= amount)
        {
            Seeds[type] -= amount;
            return true;
        }
        return false;
    }

    public void AddAnimals(AnimalType type, int amount)
    {
        if (!Animals.ContainsKey(type)) Animals[type] = 0;
        Animals[type] += amount;
    }

    public bool UseAnimals(AnimalType type, int amount)
    {
        if (Animals.ContainsKey(type) && Animals[type] >= amount)
        {
            Animals[type] -= amount;
            return true;
        }
        return false;
    }

    public void AddProduct(ProductType type, int amount)
    {
        if (!Products.ContainsKey(type)) Products[type] = 0;
        Products[type] += amount;
    }

    public bool SellProduct(ProductType type, int amount)
    {
        if (Products.ContainsKey(type) && Products[type] >= amount)
        {
            Products[type] -= amount;
            return true;
        }
        return false;
    }

    public int GetSeedCount(CropType type) => Seeds.ContainsKey(type) ? Seeds[type] : 0;
    public int GetAnimalCount(AnimalType type) => Animals.ContainsKey(type) ? Animals[type] : 0;
    public int GetProductCount(ProductType type) => Products.ContainsKey(type) ? Products[type] : 0;
}
