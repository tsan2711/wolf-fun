using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerUIItem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI workerNameText;
    [SerializeField] private TextMeshProUGUI workerStatsText;
    [SerializeField] private Image workerIcon;
    [SerializeField] private Image rarityBackground;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color uncommonColor = Color.green;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;

    public void Initialize(string workerName, int cost, Sprite icon)
    {
        workerNameText.text = workerName;
        workerIcon.sprite = icon;

        // Hide stats text if just showing name
        if (workerStatsText != null)
            workerStatsText.text = "";
    }

    public void Initialize(Worker worker, Sprite icon)
    {
        workerNameText.text = $"Worker {worker.Id + 1}";
        workerIcon.sprite = icon;

        // Show worker stats
        if (workerStatsText != null)
        {
            workerStatsText.text = $"{worker.Rarity}\nSpeed: {worker.MoveSpeed:F1}\nDuration: {worker.WorkDuration:F0}s";
        }

        // Set rarity background color
        if (rarityBackground != null)
        {
            rarityBackground.color = GetRarityColor(worker.Rarity);
        }
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => commonColor,
            Rarity.Uncommon => uncommonColor,
            Rarity.Rare => rareColor,
            Rarity.Epic => epicColor,
            Rarity.Legendary => legendaryColor,
            _ => commonColor
        };
    }
}
