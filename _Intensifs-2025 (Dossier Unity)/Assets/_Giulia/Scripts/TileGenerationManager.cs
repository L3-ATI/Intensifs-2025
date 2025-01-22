using UnityEngine;

public class TileGenerationManager : MonoBehaviour
{
    public TileGenerationSettings tileSettings;
    
    public float GetClusterProbability(int x, int z, float[,] probabilityMap, float basePercentage, float proximityFactor)
    {
        float probability = basePercentage / 100f;
        float proximityBoost = 0f;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < probabilityMap.GetLength(0) && nz >= 0 && nz < probabilityMap.GetLength(1))
                {
                    proximityBoost += probabilityMap[nx, nz];
                }
            }
        }

        probability += proximityBoost * proximityFactor;

        return Mathf.Clamp01(probability);
    }
    
    public float GetWaterProbability(int x, int z, float[,] waterProbabilityMap)
    {
        float waterProbability = tileSettings.waterPercentage / 100f;

        float proximityFactor = 0f;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < waterProbabilityMap.GetLength(0) && nz >= 0 && nz < waterProbabilityMap.GetLength(1))
                {
                    proximityFactor += waterProbabilityMap[nx, nz];
                }
            }
        }

        waterProbability += proximityFactor * tileSettings.waterProximityFactor;
        
        return Mathf.Clamp01(waterProbability);
    }

    public TileType GetRandomTileType(int x, int z, float[,] waterProbabilityMap, float[,] desertProbabilityMap,
        float[,] cityProbabilityMap)
    {
        
        if (cityProbabilityMap[x, z] == 1f)
        {
            Debug.Log($"Forcing City at ({x}, {z}) due to EnsureAtLeastOneCityCluster");
            return TileType.City;
        }

        
        float randomValue = Random.value;

        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);
        if (randomValue < waterProbability)
        {
            Debug.Log($"Selected Water at ({x}, {z}) with random {randomValue} < waterProbability {waterProbability}");
            return TileType.Water;
        }
        float remainingProbability = waterProbability;
        
        // Utiliser la probabilité de cluster pour le désert
        float desertProbability = GetClusterProbability(x, z, desertProbabilityMap, tileSettings.desertPercentage, tileSettings.desertProximityFactor);
        if (randomValue < remainingProbability + desertProbability)
        {
            // Réduire la probabilité des voisins après avoir placé un désert
            UpdateNeighborDesertProbabilities(x, z, desertProbabilityMap);
            return TileType.Desert;
        }
        remainingProbability += desertProbability;
        
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

        float cityProbability = GetCityClusterProbability(x, z, cityProbabilityMap);
        if (randomValue < remainingProbability + cityProbability)
        {
            Debug.Log($"Selected City at ({x}, {z}) with random {randomValue} < cityProbability {cityProbability}");
            return TileType.City;
        }
        remainingProbability += cityProbability;

        return TileType.Grass;

    }
    
    private void UpdateNeighborDesertProbabilities(int x, int z, float[,] desertProbabilityMap)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < desertProbabilityMap.GetLength(0) && nz >= 0 && nz < desertProbabilityMap.GetLength(1) && desertProbabilityMap[nx, nz] != 1f)
                {
                    // Diminuer la probabilité du voisin désert
                    desertProbabilityMap[nx, nz] = Mathf.Max(0f, desertProbabilityMap[nx, nz] - tileSettings.desertNeighborReductionFactor);
                }
            }
        }
    }
    
    private float GetCityClusterProbability(int x, int z, float[,] cityProbabilityMap)
    {
        float baseProbability = cityProbabilityMap[x, z];
        float proximityBoost = 0f;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < cityProbabilityMap.GetLength(0) && nz >= 0 && nz < cityProbabilityMap.GetLength(1))
                {
                    proximityBoost += cityProbabilityMap[nx, nz];
                }
            }
        }

        float finalProbability = baseProbability + proximityBoost;
        return Mathf.Clamp01(finalProbability);
    }
    
    public void EnsureDesertClusters(float[,] desertProbabilityMap)
    {
        int[] clusterSizes = { 5, 3, 2 }; // Tailles des clusters de désert
        int maxDesertClusters = 3;          // Limiter à 3 clusters
        int clustersPlaced = 0;             // Compteur de clusters placés

        foreach (int clusterSize in clusterSizes)
        {
            if (clustersPlaced >= maxDesertClusters)
                break; // Arrêter une fois le nombre maximal atteint

            bool clusterPlaced = false;
            int attempts = 0;

            while (!clusterPlaced && attempts < 100)
            {
                attempts++;
                int startX = Random.Range(0, desertProbabilityMap.GetLength(0));
                int startZ = Random.Range(0, desertProbabilityMap.GetLength(1));

                if (CanPlaceCluster(startX, startZ, clusterSize, desertProbabilityMap))
                {
                    PlaceCluster(startX, startZ, clusterSize, desertProbabilityMap);
                    clusterPlaced = true;
                    clustersPlaced++; // Incrémenter le compteur
                }
            }

            if (!clusterPlaced)
            {
                Debug.LogWarning($"Impossible de placer un cluster de taille {clusterSize} après {attempts} tentatives.");
            }
        }
    }
    
    public void EnsureCityClusters(float[,] cityProbabilityMap)
    {
        int[] clusterSizes = { 5, 3, 2 }; // Tailles des clusters de villes
        int maxCityClusters = 3;          // Limiter à 3 clusters
        int clustersPlaced = 0;           // Compteur de clusters placés

        foreach (int clusterSize in clusterSizes)
        {
            if (clustersPlaced >= maxCityClusters)
                break; // Arrêter une fois le nombre maximal atteint

            bool clusterPlaced = false;
            int attempts = 0;

            while (!clusterPlaced && attempts < 100)
            {
                attempts++;
                int startX = Random.Range(0, cityProbabilityMap.GetLength(0));
                int startZ = Random.Range(0, cityProbabilityMap.GetLength(1));

                if (CanPlaceCluster(startX, startZ, clusterSize, cityProbabilityMap))
                {
                    PlaceCluster(startX, startZ, clusterSize, cityProbabilityMap);
                    clusterPlaced = true;
                    clustersPlaced++; // Incrémenter le compteur
                }
            }

            if (!clusterPlaced)
            {
                Debug.LogWarning($"Impossible de placer un cluster de taille {clusterSize} après {attempts} tentatives.");
            }
        }
    }


    private bool CanPlaceCluster(int startX, int startZ, int clusterSize, float[,] cityProbabilityMap)
    {
        int mapWidth = cityProbabilityMap.GetLength(0);
        int mapHeight = cityProbabilityMap.GetLength(1);
        int radius = Mathf.CeilToInt(Mathf.Sqrt(clusterSize));

        for (int x = startX - radius; x <= startX + radius; x++)
        {
            for (int z = startZ - radius; z <= startZ + radius; z++)
            {
                if (x < 0 || x >= mapWidth || z < 0 || z >= mapHeight || cityProbabilityMap[x, z] == 1f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void PlaceCluster(int startX, int startZ, int clusterSize, float[,] cityProbabilityMap)
    {
        int mapWidth = cityProbabilityMap.GetLength(0);
        int mapHeight = cityProbabilityMap.GetLength(1);
        int tilesPlaced = 0;
        int radius = Mathf.CeilToInt(Mathf.Sqrt(clusterSize));

        for (int x = startX - radius; x <= startX + radius && tilesPlaced < clusterSize; x++)
        {
            for (int z = startZ - radius; z <= startZ + radius && tilesPlaced < clusterSize; z++)
            {
                if (x >= 0 && x < mapWidth && z >= 0 && z < mapHeight && cityProbabilityMap[x, z] != 1f)
                {
                    cityProbabilityMap[x, z] = 1f;
                    tilesPlaced++;
                }
            }
        }
        Debug.Log($"Cluster de taille {clusterSize} placé à partir de ({startX}, {startZ}).");
    }
    
    public float GetClusterProbability(int x, int z, float[,] probabilityMap, float basePercentage, float proximityFactor, float[,] desertProbabilityMap)
    {
        float probability = basePercentage / 100f;
        float proximityBoost = 0f;

        int desertNeighbors = 0;

        // Parcourir les voisins pour accumuler un boost de proximité et compter les voisins désert
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                if (nx >= 0 && nx < probabilityMap.GetLength(0) && nz >= 0 && nz < probabilityMap.GetLength(1))
                {
                    proximityBoost += probabilityMap[nx, nz];

                    // Compter les voisins désert
                    if (desertProbabilityMap[nx, nz] == 1f)
                    {
                        desertNeighbors++;
                    }
                }
            }
        }

        // Réduire la probabilité en fonction du nombre de voisins désert
        float reducedProbability = Mathf.Max(0f, probability - desertNeighbors * tileSettings.desertNeighborReductionFactor);

        probability += proximityBoost * proximityFactor;
        return Mathf.Clamp01(reducedProbability + proximityBoost * proximityFactor);
    }


}
