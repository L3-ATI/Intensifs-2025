using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Singleton
    public static GridManager Instance { get; private set; }

    public int width;
    public int height;
    public float tileSizeX = 1f;
    public float tileSizeZ = 1f;

    public GameObject tilePrefab;
    public GameObject riverTilePrefab;
    public GameObject stoneQuarryTilePrefab;
    public GameObject desertTilePrefab;
    public GameObject mountainPrefab;
    public GameObject minePrefab;
    public GameObject sawmillPrefab;
    public GameObject stoneQuarryPrefab;
    public GameObject cityPrefab;

    private Tile[,] tiles;
    private TileGenerationManager tileGenerationManager;
    private TileGenerationSettings tileGenerationSettings;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        tileGenerationManager = gameObject.GetComponent<TileGenerationManager>();
        tileGenerationSettings = tileGenerationManager.tileSettings;
    }

    void Start()
    {
        GenerateIrregularGrid();
    }
    
    void GenerateIrregularGrid()
    {
        bool[,] isTileValid = new bool[width, height];
        float[,] waterProbabilityMap = new float[width, height];
        float[,] desertProbabilityMap = new float[width, height];
        float[,] cityProbabilityMap = new float[width, height];

        // Remplir les cartes de probabilité pour l'eau, le désert, et la ville
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(width / 2, height / 2));
                isTileValid[x, z] = distance < (width / 2) - Random.Range(0, width / 4);

                if (isTileValid[x, z])
                {
                    waterProbabilityMap[x, z] = Random.Range(0f, 1f);
                    desertProbabilityMap[x, z] = Random.value < TileGenerationSettings.cityClusterProbability ? 1f : 0f;
                    cityProbabilityMap[x, z] = Random.value < TileGenerationSettings.cityClusterProbability ? 1f : 0f;
                }
                else
                {
                    waterProbabilityMap[x, z] = 0f;
                    desertProbabilityMap[x, z] = 0f;
                    cityProbabilityMap[x, z] = 0f;
                }
            }
        }

        // Appeler EnsureClusterPlacement pour chaque type de cluster (eau, désert, ville)
        tileGenerationManager.EnsureClusterPlacement(waterProbabilityMap, tileGenerationSettings.clusterSizes, tileGenerationSettings.maxClusters);
        tileGenerationManager.EnsureClusterPlacement(desertProbabilityMap, tileGenerationSettings.clusterSizes, tileGenerationSettings.maxClusters, true);  // true pour le désert
        tileGenerationManager.EnsureClusterPlacement(cityProbabilityMap, tileGenerationSettings.clusterSizes, tileGenerationSettings.maxClusters);

        tiles = new Tile[width, height];

        // Création des tuiles dans la grille
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (isTileValid[x, z])
                {
                    float xOffset = (z % 2 != 0) ? tileSizeX * 0.5f : 0;
                    Vector3 position = new Vector3(x * tileSizeX + xOffset, 0, z * tileSizeZ);

                    TileType tileType = tileGenerationManager.GetRandomTileType(x, z, waterProbabilityMap, desertProbabilityMap, cityProbabilityMap);
                    GameObject prefabToInstantiate = tilePrefab;

                    // Choisir le préfabriqué en fonction du type de tuile
                    if (tileType == TileType.Water && riverTilePrefab != null)
                    {
                        prefabToInstantiate = riverTilePrefab;
                    }
                    else if (tileType == TileType.StoneQuarry && stoneQuarryTilePrefab != null)
                    {
                        prefabToInstantiate = stoneQuarryTilePrefab;
                    }
                    else if (tileType == TileType.Desert && desertTilePrefab != null)
                    {
                        prefabToInstantiate = desertTilePrefab;
                    }

                    GameObject newTile = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
                    newTile.name = $"Tile_{x}_{z}";

                    Tile tileComponent = newTile.GetComponent<Tile>();
                    tileComponent.SetPosition(x, z);
                    tiles[x, z] = tileComponent;
                    tileComponent.tileType = tileType;

                    // Ajouter des structures si nécessaire
                    if (tileType == TileType.Mountain && mountainPrefab != null)
                    {
                        CreatePrefabOnTile(mountainPrefab, newTile, x, z);
                    }
                    else if (tileType == TileType.Mine && minePrefab != null)
                    {
                        CreatePrefabOnTile(minePrefab, newTile, x, z);
                        tileComponent.tag = "Structure";
                        tileComponent.isOccupied = true;
                    }
                    else if (tileType == TileType.Sawmill && sawmillPrefab != null)
                    {
                        CreatePrefabOnTile(sawmillPrefab, newTile, x, z);
                        tileComponent.tag = "Structure";
                        tileComponent.isOccupied = true;
                    }
                    else if (tileType == TileType.StoneQuarry && stoneQuarryPrefab != null)
                    {
                        CreatePrefabOnTile(stoneQuarryPrefab, newTile, x, z);
                        tileComponent.tag = "Structure";
                        tileComponent.isOccupied = true;
                    }
                    else if (tileType == TileType.City && cityPrefab != null)
                    {
                        tileComponent.tileType = TileType.City;
                        CreatePrefabOnTile(cityPrefab, newTile, x, z);
                        tileComponent.tag = "Structure";
                        tileComponent.isOccupied = true;
                    }

                    // Mettre à jour la végétation si nécessaire
                    tileComponent.UpdateVegetation();
                }
            }
        }
    }
    private void CreatePrefabOnTile(GameObject prefab, GameObject tile, int x, int z)
    {
        GameObject instance = Instantiate(prefab, tile.transform.position, Quaternion.identity, tile.transform);
        instance.name = $"{prefab.name}_{x}_{z}";
    }
}