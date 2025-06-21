using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UpgradeUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private Button upgradeButton;

    [Header("Animation Settings")]
    [SerializeField] private float buttonHoverScale = 1.1f;
    [SerializeField] private float buttonClickScale = 0.9f;

    private ProductType _productType;
    public ProductType ProductType
    {
        get => _productType;
        set
        {
            _productType = value;
            upgradeNameText.text = value.ToString();
        }
    }

    public void Initialize(string upgradeName, int cost, ProductType productType, Sprite icon, System.Action<ProductType> onUpgradeAction = null)
    {
        upgradeNameText.text = upgradeName;
        upgradeCostText.text = cost.ToString();
        upgradeIcon.sprite = icon;
        _productType = productType;

        upgradeButton.onClick.RemoveAllListeners();
        if (onUpgradeAction != null)
        {
            upgradeButton.onClick.AddListener(() => onUpgradeAction.Invoke(_productType));
            AddButtonAnimations(upgradeButton);
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
        pointerEnter.callback.AddListener((data) =>
        {
            button.transform.DOScale(buttonHoverScale, 0.1f);
        });
        eventTrigger.triggers.Add(pointerEnter);

        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) =>
        {
            button.transform.DOScale(1f, 0.1f);
        });
        eventTrigger.triggers.Add(pointerExit);

        // Click effect
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) =>
        {
            button.transform.DOScale(buttonClickScale, 0.05f);
        });
        eventTrigger.triggers.Add(pointerDown);

        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) =>
        {
            button.transform.DOScale(buttonHoverScale, 0.05f);
        });
        eventTrigger.triggers.Add(pointerUp);
    }

    public TextMeshProUGUI UpgradeNameText => upgradeNameText;
    public TextMeshProUGUI LevelText => levelText;
}