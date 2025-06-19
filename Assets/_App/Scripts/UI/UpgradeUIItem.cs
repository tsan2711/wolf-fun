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

    public void Initialize(string upgradeName, int cost, Sprite icon)
    {
        upgradeNameText.text = upgradeName;
        upgradeCostText.text = cost.ToString();
        upgradeIcon.sprite = icon;
    }
}
