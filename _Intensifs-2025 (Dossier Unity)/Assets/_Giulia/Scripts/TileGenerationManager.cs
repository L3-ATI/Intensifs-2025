using UnityEngine;

public class TileGenerationManager : MonoBehaviour
{
    public TileGenerationSettings tileSettings;
    
    // Méthode qui permet de récupérer la probabilité d'eau en fonction des voisins
    public float GetWaterProbability(int x, int z, float[,] waterProbabilityMap)
    {
        // Commence avec la probabilité de base définie par les paramètres
        float waterProbability = tileSettings.waterPercentage / 100f;

        // Vérifier les tuiles voisines et augmenter la probabilité si de l'eau est proche
        float proximityFactor = 0f;

        // Vérifier les voisins dans une zone 3x3 autour de la tuile (x, z)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                // Vérifier que les voisins sont dans les limites de la grille
                if (nx >= 0 && nx < waterProbabilityMap.GetLength(0) && nz >= 0 && nz < waterProbabilityMap.GetLength(1))
                {
                    proximityFactor += waterProbabilityMap[nx, nz]; // Ajouter la probabilité des voisins
                }
            }
        }

        // Appliquer le facteur de proximité pour augmenter la probabilité
        waterProbability += proximityFactor * tileSettings.waterProximityFactor;  // Utiliser le facteur de proximité défini dans les paramètres

        // Limiter la probabilité entre 0 et 1 pour éviter les valeurs aberrantes
        return Mathf.Clamp01(waterProbability);
    }

    public TileType GetRandomTileType(int x, int z, float[,] waterProbabilityMap)
    {
        float randomValue = Random.value;

        // Calculer la probabilité d'eau ajustée par la proximité des voisins
        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);

        if (randomValue < waterProbability) return TileType.Water;

        // Si ce n'est pas de l'eau, calculer les autres types comme d'habitude
        float remainingProbability = waterProbability;

        if (randomValue < remainingProbability + tileSettings.mountainPercentage / 100f)
            return TileType.Mountain;

        remainingProbability += tileSettings.mountainPercentage / 100f;

        if (randomValue < remainingProbability + tileSettings.minePercentage / 100f)
            return TileType.Mine;

        remainingProbability += tileSettings.minePercentage / 100f;

        if (randomValue < remainingProbability + tileSettings.sawmillPercentage / 100f)
            return TileType.Sawmill;

        remainingProbability += tileSettings.sawmillPercentage / 100f;

        if (randomValue < remainingProbability + tileSettings.stoneQuarryPercentage / 100f)
            return TileType.StoneQuarry;

        // Si aucune des conditions précédentes n'est remplie, renvoyer de l'herbe
        return TileType.Grass;
    }
}
