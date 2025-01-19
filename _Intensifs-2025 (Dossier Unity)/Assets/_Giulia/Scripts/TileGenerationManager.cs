using UnityEngine;

public class TileGenerationManager : MonoBehaviour
{
    public TileGenerationSettings tileSettings;
    
    // Méthode qui permet de récupérer la probabilité d'eau en fonction des voisins
    public float GetWaterProbability(int x, int z, float[,] waterProbabilityMap)
    {
        float waterProbability = tileSettings.waterPercentage / 100f;

        // Vérifier les tuiles voisines et augmenter la probabilité si de l'eau est proche
        float proximityFactor = 0f;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < waterProbabilityMap.GetLength(0) && nz >= 0 && nz < waterProbabilityMap.GetLength(1))
                {
                    proximityFactor += waterProbabilityMap[nx, nz]; // Ajouter la probabilité des voisins
                }
            }
        }

        // Augmenter la probabilité en fonction de la proximité des tuiles d'eau
        waterProbability += proximityFactor * 0.1f; // Ajustez ce facteur pour avoir plus ou moins d'effet

        return Mathf.Clamp01(waterProbability); // Limiter entre 0 et 1
    }

    public TileType GetRandomTileType(int x, int z, float[,] waterProbabilityMap)
    {
        float randomValue = Random.value;

        // Calculer la probabilité d'eau ajustée par la proximité des voisins
        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);

        if (randomValue < waterProbability) return TileType.Water;

        // Si ce n'est pas de l'eau, calculer les autres types comme d'habitude
        if (randomValue < waterProbability + tileSettings.mountainPercentage / 100f) return TileType.Mountain;
        if (randomValue < waterProbability + tileSettings.mountainPercentage / 100f + tileSettings.minePercentage / 100f) return TileType.Mine;
        if (randomValue < waterProbability + tileSettings.mountainPercentage / 100f + tileSettings.minePercentage / 100f + tileSettings.sawmillPercentage / 100f) return TileType.Sawmill;
        if (randomValue < waterProbability + tileSettings.mountainPercentage / 100f + tileSettings.minePercentage / 100f + tileSettings.sawmillPercentage / 100f + tileSettings.stoneQuarryPercentage / 100f) return TileType.StoneQuarry;

        return TileType.Grass;
    }
}
