using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Image itemIcon;


    public void Initialize(string itemName, int quantity, Sprite icon)
    {
        itemNameText.text = itemName;
        itemQuantityText.text = quantity.ToString();
        itemIcon.sprite = icon;
    }
}
