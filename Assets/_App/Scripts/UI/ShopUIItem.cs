using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI levelText; 
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button sold1Button;
    [SerializeField] private Button sold10Button;

    public void Initialize(string itemName, int price, Sprite icon)
    {
        itemNameText.text = itemName;
        itemPriceText.text = price.ToString();
        itemIcon.sprite = icon;
    }
}
