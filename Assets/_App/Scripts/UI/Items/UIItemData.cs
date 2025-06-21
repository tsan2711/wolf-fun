using UnityEngine;

[System.Serializable]
public class UIItemData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int Level { get; set; }
    public Sprite Icon { get; set; }
    public UIItemType Type { get; set; }
}

