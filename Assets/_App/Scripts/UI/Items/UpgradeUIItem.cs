using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private Button upgradeButton;

    public void Initialize(string upgradeName, int cost, Sprite icon, System.Action onUpgradeAction = null)
    {
        upgradeNameText.text = upgradeName;
        upgradeCostText.text = cost.ToString();
        upgradeIcon.sprite = icon;
        upgradeButton.onClick.RemoveAllListeners();
        if (onUpgradeAction != null)
        {
            upgradeButton.onClick.AddListener(() => onUpgradeAction.Invoke());

        }
    }
}
