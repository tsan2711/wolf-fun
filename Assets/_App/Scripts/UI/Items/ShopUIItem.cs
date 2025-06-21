using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button buy1Button;

    [Header("Animation Settings")]
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonClickScale = 0.9f;

    public void Initialize(string itemName, int price, Sprite icon, UnityEngine.Events.UnityAction buy1Action = null)
    {
        itemNameText.text = itemName;
        itemPriceText.text = price.ToString();
        itemIcon.sprite = icon;
        
        buy1Button.onClick.RemoveAllListeners();
        if (buy1Action != null)
        {
            buy1Button.onClick.AddListener(buy1Action);
            AddButtonAnimations(buy1Button);
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
