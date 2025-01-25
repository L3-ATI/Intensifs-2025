using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    [Header("Resource Counters")]
    public TextMeshProUGUI woodCounter;
    public TextMeshProUGUI stoneCounter;
    public TextMeshProUGUI ironCounter;
    public TextMeshProUGUI moneyCounter;

    [Header("Initial Resources")]
    public int startingWood = 100;
    public int startingStone = 100;
    public int startingIron = 100;
    public int startingMoney = 500;

    private int currentWood;
    private int currentStone;
    private int currentIron;
    private int currentMoney;

    [System.Serializable]
    public class Item
    {
        public string name; // Nom de l'élément (par exemple, "Station", "Rail01", etc.)
        public TextMeshProUGUI priceCounter; // Compteur pour afficher le prix dans l'UI
        public int priceMoney; // Prix en argent
        public int priceWood; // Coût en bois
        public int priceStone; // Coût en pierre
        public int priceIron; // Coût en fer
    }

    [Header("Item Prices")]
    public List<Item> items = new List<Item>();

    private void Start()
    {
        // Initialisation des ressources
        currentWood = startingWood;
        currentStone = startingStone;
        currentIron = startingIron;
        currentMoney = startingMoney;

        // Met à jour les compteurs UI
        UpdateResourceCounters();

        // Met à jour les prix affichés des items
        UpdateItemPriceCounters();
    }

    private void UpdateResourceCounters()
    {
        if (woodCounter) woodCounter.text = currentWood.ToString();
        if (stoneCounter) stoneCounter.text = currentStone.ToString();
        if (ironCounter) ironCounter.text = currentIron.ToString();
        if (moneyCounter) moneyCounter.text = "$" + currentMoney.ToString();
    }

    private void UpdateItemPriceCounters()
    {
        foreach (var item in items)
        {
            if (item.priceCounter)
            {
                item.priceCounter.text = $"$ {item.priceMoney}\nW: {item.priceWood} S: {item.priceStone} I: {item.priceIron}";
            }
        }
    }

    // Méthode pour acheter un élément
    public bool PurchaseItem(string itemName)
    {
        Item item = items.Find(i => i.name == itemName);
        if (item == null)
        {
            Debug.LogWarning("Item not found: " + itemName);
            return false;
        }

        // Vérifie si l'achat est possible
        if (currentMoney >= item.priceMoney &&
            currentWood >= item.priceWood &&
            currentStone >= item.priceStone &&
            currentIron >= item.priceIron)
        {
            // Déduit les ressources
            currentMoney -= item.priceMoney;
            currentWood -= item.priceWood;
            currentStone -= item.priceStone;
            currentIron -= item.priceIron;

            // Met à jour les compteurs
            UpdateResourceCounters();
            return true;
        }
        else
        {
            TooltipManager.Instance.ShowTooltip("TEST");
            return false;
        }
    }

    // Méthode pour ajouter des ressources
    public void AddResources(int wood, int stone, int iron, int money)
    {
        currentWood += wood;
        currentStone += stone;
        currentIron += iron;
        currentMoney += money;

        // Met à jour les compteurs
        UpdateResourceCounters();
    }

    // Méthode pour ajuster dynamiquement le prix d'un item
    public void SetItemPrice(string itemName, int money, int wood, int stone, int iron)
    {
        Item item = items.Find(i => i.name == itemName);
        if (item != null)
        {
            item.priceMoney = money;
            item.priceWood = wood;
            item.priceStone = stone;
            item.priceIron = iron;

            // Met à jour l'affichage du prix
            UpdateItemPriceCounters();
        }
    }
}
