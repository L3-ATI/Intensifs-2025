using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collector : MonoBehaviour
{
    [Header("Ressources LV1")]
    public int moneyPerCity = 10;
    public int woodPerSawmill = 5;
    public int stonePerQuarry = 5;
    public int ironPerMine = 5;

    [Header("Ressources LV2 (Upgraded Station)")]
    public int moneyPerCityLV2 = 30;
    public int woodPerSawmillLV2 = 10;
    public int stonePerQuarryLV2 = 10;
    public int ironPerMineLV2 = 10;

    private void OnTriggerEnter(Collider other)
    {
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.tileType == TileType.Station || tile.tileType == TileType.UpgradedStation)
        {
            Debug.Log($"Interaction avec une {tile.tileType}. Vérification des voisins...");
            bool isUpgraded = tile.tileType == TileType.UpgradedStation; // Vérifie si c'est une station améliorée
            CollectResourcesFromNeighbors(tile, isUpgraded);
        }
    }

    private void CollectResourcesFromNeighbors(Tile stationTile, bool isUpgraded)
    {
        int totalMoney = 0;
        int totalWood = 0;
        int totalStone = 0;
        int totalIron = 0;

        foreach (Tile neighbor in stationTile.neighboringTiles)
        {
            if (neighbor == null) continue;

            // Ressources en fonction du type de tuile voisine et du niveau de la station
            switch (neighbor.tileType)
            {
                case TileType.City:
                    totalMoney += isUpgraded ? moneyPerCityLV2 : moneyPerCity;
                    Debug.Log($"Voisin : Ville, +{(isUpgraded ? moneyPerCityLV2 : moneyPerCity)} argent.");
                    break;

                case TileType.Sawmill:
                    totalWood += isUpgraded ? woodPerSawmillLV2 : woodPerSawmill;
                    Debug.Log($"Voisin : Scierie, +{(isUpgraded ? woodPerSawmillLV2 : woodPerSawmill)} bois.");
                    break;

                case TileType.StoneQuarry:
                    totalStone += isUpgraded ? stonePerQuarryLV2 : stonePerQuarry;
                    Debug.Log($"Voisin : Carrière, +{(isUpgraded ? stonePerQuarryLV2 : stonePerQuarry)} pierre.");
                    break;

                case TileType.Mine:
                    totalIron += isUpgraded ? ironPerMineLV2 : ironPerMine;
                    Debug.Log($"Voisin : Mine, +{(isUpgraded ? ironPerMineLV2 : ironPerMine)} fer.");
                    break;

                default:
                    Debug.Log($"Voisin : {neighbor.tileType}, aucune ressource collectée.");
                    break;
            }
        }

        // Ajoute les ressources collectées via le RessourcesManager
        RessourcesManager.Instance.AddResources(totalMoney, totalWood, totalIron, totalStone);

        Debug.Log($"Ressources collectées : Argent={totalMoney}, Bois={totalWood}, Pierre={totalStone}, Fer={totalIron}.");
    }
}
