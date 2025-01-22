using UnityEngine;

public class TileGenerationManager : MonoBehaviour
{
    public TileGenerationSettings tileSettings;

    private float GetProximityBoost(int x, int z, float[,] probabilityMap)
    {
        float proximityBoost = 0f;
        int mapWidth = probabilityMap.GetLength(0);
        int mapHeight = probabilityMap.GetLength(1);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < mapWidth && nz >= 0 && nz < mapHeight)
                {
                    proximityBoost += probabilityMap[nx, nz];
                }
            }
        }
        return proximityBoost;
    }

    public float GetClusterProbability(int x, int z, float[,] probabilityMap, float basePercentage, float proximityFactor)
    {
        float probability = basePercentage / 100f;
        float proximityBoost = GetProximityBoost(x, z, probabilityMap);
        return Mathf.Clamp01(probability + proximityBoost * proximityFactor);
    }

    public float GetWaterProbability(int x, int z, float[,] waterProbabilityMap)
    {
        float waterProbability = tileSettings.waterPercentage / 100f;
        float proximityFactor = GetProximityBoost(x, z, waterProbabilityMap);
        waterProbability += proximityFactor * tileSettings.waterProximityFactor;
        return Mathf.Clamp01(waterProbability);
    }

    public TileType GetRandomTileType(int x, int z, float[,] waterProbabilityMap, float[,] desertProbabilityMap, float[,] cityProbabilityMap)
    {

        // Si un désert est forcé, on le place
        if (desertProbabilityMap[x, z] == 1f)
        {
            Debug.Log($"Forcing Desert at ({x}, {z}) due to EnsureAtLeastOneCityCluster");
            return TileType.Desert;
        }
        
        // Si une ville est forcée, on la place
        if (cityProbabilityMap[x, z] == 1f)
        {
            Debug.Log($"Forcing City at ({x}, {z}) due to EnsureAtLeastOneCityCluster");
            return TileType.City;
        }

        // Calcul de la probabilité d'eau
        float randomValue = Random.value;
        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);

        // Vérifier si c'est de l'eau
        if (randomValue < waterProbability)
        {
            Debug.Log($"Selected Water at ({x}, {z}) with random {randomValue} < waterProbability {waterProbability}");
            return TileType.Water;
        }
        
        // Ajout de la probabilité restante pour les autres types
        float remainingProbability = waterProbability;

        // Montagne
        if (randomValue < remainingProbability + tileSettings.mountainPercentage / 100f)
            return TileType.Mountain;
        remainingProbability += tileSettings.mountainPercentage / 100f;

        // Mine
        if (randomValue < remainingProbability + tileSettings.minePercentage / 100f)
            return TileType.Mine;
        remainingProbability += tileSettings.minePercentage / 100f;

        // Scierie
        if (randomValue < remainingProbability + tileSettings.sawmillPercentage / 100f)
            return TileType.Sawmill;
        remainingProbability += tileSettings.sawmillPercentage / 100f;

        // Carrière de pierre
        if (randomValue < remainingProbability + tileSettings.stoneQuarryPercentage / 100f)
            return TileType.StoneQuarry;

        // Probabilité de la ville
        float cityProbability = GetClusterProbability(x, z, cityProbabilityMap, 0f, 0f);
        if (randomValue < remainingProbability + cityProbability)
        {
            Debug.Log($"Selected City at ({x}, {z}) with random {randomValue} < cityProbability {cityProbability}");
            return TileType.City;
        }

        // Si le désert est encore une possibilité, on le force après la ville
        float desertProbability = GetClusterProbability(x, z, desertProbabilityMap, 0f, 0f);
        if (randomValue < remainingProbability + desertProbability)
        {
            Debug.Log($"Selected Desert at ({x}, {z}) with random {randomValue} < desertProbability {desertProbability}");
            return TileType.Desert;
        }

        // Si rien d'autre n'est sélectionné, c'est de l'herbe
        return TileType.Grass;
    }

    public void EnsureClusterPlacement(float[,] probabilityMap, int[] clusterSizes, int maxClusters)
    {
        if (probabilityMap.ToString() == "desertProbabilityMap")
        {
            for (int i = 0; i < clusterSizes.Length; i++)
            {
                clusterSizes[i] += 10;
            }
            maxClusters += 5;
        }
        
        int clustersPlaced = 0;

        foreach (int clusterSize in clusterSizes)
        {
            if (clustersPlaced >= maxClusters)
                break;

            bool clusterPlaced = false;
            int attempts = 0;

            while (!clusterPlaced && attempts < 100)
            {
                attempts++;
                int startX = Random.Range(0, probabilityMap.GetLength(0));
                int startZ = Random.Range(0, probabilityMap.GetLength(1));

                if (CanPlaceCluster(startX, startZ, clusterSize, probabilityMap))
                {
                    PlaceCluster(startX, startZ, clusterSize, probabilityMap);
                    clusterPlaced = true;
                    clustersPlaced++;
                }
            }

            if (!clusterPlaced)
            {
                Debug.LogWarning($"Impossible de placer un cluster de taille {clusterSize} après {attempts} tentatives.");
            }
        }
    }

    private bool CanPlaceCluster(int startX, int startZ, int clusterSize, float[,] targetMap)
    {
        int mapWidth = targetMap.GetLength(0);
        int mapHeight = targetMap.GetLength(1);
        int radius = Mathf.CeilToInt(Mathf.Sqrt(clusterSize));

        for (int x = startX - radius; x <= startX + radius; x++)
        {
            for (int z = startZ - radius; z <= startZ + radius; z++)
            {
                if (x < 0 || x >= mapWidth || z < 0 || z >= mapHeight || targetMap[x, z] == 1f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void PlaceCluster(int startX, int startZ, int clusterSize, float[,] targetMap)
    {
        int mapWidth = targetMap.GetLength(0);
        int mapHeight = targetMap.GetLength(1);
        int tilesPlaced = 0;
        int radius = Mathf.CeilToInt(Mathf.Sqrt(clusterSize));

        for (int x = startX - radius; x <= startX + radius && tilesPlaced < clusterSize; x++)
        {
            for (int z = startZ - radius; z <= startZ + radius && tilesPlaced < clusterSize; z++)
            {
                if (x >= 0 && x < mapWidth && z >= 0 && z < mapHeight && targetMap[x, z] != 1f)
                {
                    targetMap[x, z] = 1f;
                    tilesPlaced++;
                }
            }
        }
        Debug.Log($"Cluster de taille {clusterSize} placé à partir de ({startX}, {startZ}).");
    }
}
