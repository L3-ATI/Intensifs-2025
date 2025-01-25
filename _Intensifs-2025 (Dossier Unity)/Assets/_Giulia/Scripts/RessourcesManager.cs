using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class BuildableItem
{
    public string itemName;                  // Nom de l'élément (Station, Rail, etc.)
    public int basePrice;                    // Prix en argent
    public int priceIncrement;               // Incrément du prix après chaque achat
    public int woodCost;                     // Coût en bois
    public int stoneCost;                    // Coût en pierre
    public int ironCost;                     // Coût en fer

    [Header("UI References")]
    public TextMeshProUGUI priceText;        // Texte affichant le prix en argent
    public TextMeshProUGUI woodCostText;     // Texte affichant le coût en bois
    public TextMeshProUGUI stoneCostText;    // Texte affichant le coût en pierre
    public TextMeshProUGUI ironCostText;     // Texte affichant le coût en fer
}

public class RessourcesManager : MonoBehaviour
{
    public static RessourcesManager Instance { get; private set; }
    
    [Header("Starting Resources")]
    public int startingMoney = 1000;
    public int startingWood = 500;
    public int startingStone = 500;
    public int startingIron = 500;

    [Header("UI References for Resources")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI ironText;

    [Header("Buildable Items")]
    public BuildableItem[] buildableItems;

    private int currentMoney;
    private int currentWood;
    private int currentStone;
    private int currentIron;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Plus d'une instance de RessourcesManager existe !");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        currentMoney = startingMoney;
        currentWood = startingWood;
        currentStone = startingStone;
        currentIron = startingIron;

        UpdateResourceUI(true);

        foreach (var item in buildableItems)
        {
            UpdateItemUI(item);
        }
    }

    private void UpdateResourceUI(bool instant = false)
    {
        if (instant)
        {
            moneyText.text = $"{currentMoney}";
            woodText.text = $"{currentWood}";
            stoneText.text = $"{currentStone}";
            ironText.text = $"{currentIron}";
        }
        else
        {
            StartCoroutine(AnimateText(moneyText, int.Parse(moneyText.text), currentMoney));
            StartCoroutine(AnimateText(woodText, int.Parse(woodText.text), currentWood));
            StartCoroutine(AnimateText(stoneText, int.Parse(stoneText.text), currentStone));
            StartCoroutine(AnimateText(ironText, int.Parse(ironText.text), currentIron));
        }
    }

    private void UpdateItemUI(BuildableItem item)
    {
        item.priceText.text = $"{item.basePrice}";
        item.woodCostText.text = $"{item.woodCost}";
        item.stoneCostText.text = $"{item.stoneCost}";
        item.ironCostText.text = $"{item.ironCost}";
    }
    
    public bool CanAffordItem(BuildableItem item)
    {
        return currentMoney >= item.basePrice &&
               currentWood >= item.woodCost &&
               currentStone >= item.stoneCost &&
               currentIron >= item.ironCost;
    }

    public void PurchaseItem(BuildableItem item)
    {
        if (CanAffordItem(item))
        {
            currentMoney -= item.basePrice;
            currentWood -= item.woodCost;
            currentStone -= item.stoneCost;
            currentIron -= item.ironCost;

            item.basePrice += item.priceIncrement;

            UpdateResourceUI();
            UpdateItemUI(item);

            Debug.Log($"Purchased: {item.itemName}. New price: {item.basePrice}");
        }
        else
        {
            TooltipManager.Instance.ShowTooltip($"Not enough resources to purchase: {item.itemName}");
        }
    }

    public void AddResources(int money, int wood, int iron, int stone)
    {
        currentMoney += money;
        currentWood += wood;
        currentStone += stone;
        currentIron += iron;

        UpdateResourceUI();
    }

    private IEnumerator AnimateText(TextMeshProUGUI text, int startValue, int endValue, float duration = 0.5f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            text.text = currentValue.ToString();
            yield return null;
        }
        text.text = endValue.ToString();
    }
}
