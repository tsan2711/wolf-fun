using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button buy1Button;

    public void Initialize(string itemName, int price, Sprite icon, UnityEngine.Events.UnityAction buy1Action = null)
    {
        itemNameText.text = itemName;
        itemPriceText.text = price.ToString();
        itemIcon.sprite = icon;
        buy1Button.onClick.RemoveAllListeners();
        if (buy1Action != null)
        {
            buy1Button.onClick.AddListener(buy1Action);
        }
    }
}
