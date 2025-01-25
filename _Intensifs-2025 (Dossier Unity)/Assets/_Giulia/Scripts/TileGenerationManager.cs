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

        desertProbability = Mathf.Clamp01(desertProbability);
        return desertProbability;
    }

    public float GetWaterProbability(int x, int z, float[,] waterProbabilityMap)
    {
        float waterProbability = tileSettings.waterPercentage / 100f;
        float proximityBoost = GetProximityBoost(x, z, waterProbabilityMap);
        waterProbability += proximityBoost * tileSettings.waterProximityFactor;
        return Mathf.Clamp01(waterProbability);
    }

    public TileType GetRandomTileType(int x, int z, float[,] waterProbabilityMap, float[,] desertProbabilityMap, float[,] cityProbabilityMap)
    {
        float randomValue = Random.value;

        if (cityProbabilityMap[x, z] > 0f)
        {
            return TileType.City;
        }

        float waterProbability = GetWaterProbability(x, z, waterProbabilityMap);
        if (randomValue < waterProbability)
        {
            return TileType.Water;
        }

        float desertProbability = GetDesertProbability(x, z, desertProbabilityMap);
        if (randomValue < waterProbability + desertProbability)
        {
            return TileType.Desert;
        }

        float remainingProbability = waterProbability + desertProbability;

        float mountainProbability = tileSettings.mountainPercentage / 100f;
        float stoneQuarryProbability = tileSettings.stoneQuarryPercentage / 100f;
        float mineProbability = tileSettings.minePercentage / 100f;
        float sawmillProbability = tileSettings.sawmillPercentage / 100f;

        if (randomValue < remainingProbability + mountainProbability)
        {
            return TileType.Mountain;
        }
        remainingProbability += mountainProbability;
        
        if (randomValue < remainingProbability + stoneQuarryProbability)
        {
            return TileType.StoneQuarry;
        }
        remainingProbability += stoneQuarryProbability;

        if (randomValue < remainingProbability + mineProbability)
        {
            return TileType.Mine;
        }
        remainingProbability += mineProbability;

        if (randomValue < remainingProbability + sawmillProbability)
        {
            return TileType.Sawmill;
        }

        return TileType.Grass;
    }

    public void EnsureClusterPlacement(float[,] probabilityMap, int[] clusterSizes, int maxClusters, bool isDesert = false)
    {
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
