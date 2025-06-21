using DG.Tweening;
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

    [Header("Animation Settings")]
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonClickScale = 0.9f;

    public void Initialize(string itemName, int quantity, Sprite icon, UnityEngine.Events.UnityAction sellAction = null)
    {
        itemNameText.text = itemName;
        itemQuantityText.text = quantity.ToString();
        itemIcon.sprite = icon;

        sellButton.gameObject.SetActive(sellAction != null);
        if (sellAction != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(sellAction);
            
            // Add button animations
            AddButtonAnimations(sellButton);
        }
    }

    private void AddButtonAnimations(Button button)
    {
        var eventTrigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        // Hover effects
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => {
            button.transform.DOScale(buttonHoverScale, 0.1f);
        });
        eventTrigger.triggers.Add(pointerEnter);

        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => {
            button.transform.DOScale(1f, 0.1f);
        });
        eventTrigger.triggers.Add(pointerExit);

        // Click effect
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => {
            button.transform.DOScale(buttonClickScale, 0.05f);
        });
        eventTrigger.triggers.Add(pointerDown);

        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            button.transform.DOScale(buttonHoverScale, 0.05f);
        });
        eventTrigger.triggers.Add(pointerUp);
    }
}