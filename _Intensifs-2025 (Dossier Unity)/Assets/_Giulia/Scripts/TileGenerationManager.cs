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

    public float GetDesertProbability(int x, int z, float[,] desertProbabilityMap)
    {
        float desertProbability = tileSettings.desertPercentage / 100f;
        float proximityBoost = GetProximityBoost(x, z, desertProbabilityMap);
        desertProbability += proximityBoost * tileSettings.desertProximityFactor;
        return Mathf.Clamp01(desertProbability);
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
        float randomValue = Random.value;

        // Calcul de la probabilité d'eau
        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);
        if (randomValue < waterProbability)
        {
            return TileType.Water;
        }

        // Probabilité de désert
        float desertProbability = GetDesertProbability(x, z, desertProbabilityMap);
        if (randomValue < waterProbability + desertProbability)
        {
            return TileType.Desert;
        }

        // Autres types de tuiles (montagnes, mines, etc.)
        float remainingProbability = waterProbability + desertProbability;

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

        return TileType.Grass;
    }

    public void EnsureClusterPlacement(float[,] probabilityMap, int[] clusterSizes, int maxClusters, bool isDesert = false)
    {
        if (isDesert)
        {
            for (int i = 0; i < clusterSizes.Length; i++)
            {
                clusterSizes[i] += 5; // Augmente légèrement les clusters pour les déserts
            }
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
    }
}