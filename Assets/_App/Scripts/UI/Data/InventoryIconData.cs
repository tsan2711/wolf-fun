using UnityEngine;

[CreateAssetMenu(fileName = "InventoryIconData", menuName = "Game/Inventory Icon Data")]
public class InventoryIconData : ScriptableObject
{
    [Header("Seed Icons")]
    [SerializeField] private Sprite tomatoSeedIcon;
    [SerializeField] private Sprite blueberrySeedIcon;
    [SerializeField] private Sprite strawberrySeedIcon;

    [Header("Product Icons")]
    [SerializeField] private Sprite tomatoIcon;
    [SerializeField] private Sprite blueberryIcon;
    [SerializeField] private Sprite strawberryIcon;
    [SerializeField] private Sprite milkIcon;

    [Header("Animal Icons")]
    [SerializeField] private Sprite cowIcon;

    [Header("Upgrade Icons")]
    [SerializeField] private Sprite strawberryUpgradeIcon;
    [SerializeField] private Sprite blueberryUpgradeIcon;
    [SerializeField] private Sprite tomatoUpgradeIcon;
    [SerializeField] private Sprite cowUpgradeIcon;

    [Header("Worker Icons")]
    [SerializeField] private Sprite WorkerIcon;

    public Sprite GetSeedIcon(CropType cropType)
    {
        return cropType switch
        {
            CropType.Tomato => tomatoSeedIcon,
            CropType.Blueberry => blueberrySeedIcon,
            CropType.Strawberry => strawberrySeedIcon,
            _ => null
        };
    }

    public Sprite GetProductIcon(ProductType productType)
    {
        return productType switch
        {
            ProductType.Tomato => tomatoIcon,
            ProductType.Blueberry => blueberryIcon,
            ProductType.Strawberry => strawberryIcon,
            ProductType.Milk => milkIcon,
            _ => null
        };
    }

    public Sprite GetAnimalIcon(AnimalType animalType)
    {
        return animalType switch
        {
            AnimalType.Cow => cowIcon,
            _ => null
        };
    }

    public Sprite StrawberryUpgradeIcon => strawberryUpgradeIcon;
    public Sprite BlueberryUpgradeIcon => blueberryUpgradeIcon;
    public Sprite TomatoUpgradeIcon => tomatoUpgradeIcon;
    public Sprite CowUpgradeIcon => cowUpgradeIcon;
    public Sprite GetWorkerIcon()
    {
        return WorkerIcon; // Assuming a single worker icon for simplicity
    }
}
