using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button sellButton;

    public void Initialize(string itemName, int quantity, Sprite icon, UnityEngine.Events.UnityAction sellAction = null)
    {
        itemNameText.text = itemName;
        itemQuantityText.text = quantity.ToString();
        itemIcon.sprite = icon;

        sellButton.gameObject.SetActive(sellAction != null);
        if (sellAction == null) return;

        sellButton.onClick.RemoveAllListeners();
        if (sellAction != null)
        {
            sellButton.onClick.AddListener(sellAction);
        }
    }
}