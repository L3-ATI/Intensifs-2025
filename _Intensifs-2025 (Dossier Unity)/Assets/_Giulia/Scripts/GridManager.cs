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
    public GameObject mountainPrefab;
    public GameObject minePrefab;
    public GameObject sawmillPrefab;
    public GameObject stoneQuarryPrefab; // Préfab de Stone Quarry

    private Tile[,] tiles;

    private TileGenerationManager tileGenerationManager;

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

        tileGenerationManager = FindObjectOfType<TileGenerationManager>();
    }

    void Start()
    {
        GenerateIrregularGrid();
    }
    void GenerateIrregularGrid()
    {
        bool[,] isTileValid = new bool[width, height];
        float[,] waterProbabilityMap = new float[width, height];  // Initialise la carte des probabilités d'eau

        // Remplir ce tableau avec une forme aléatoire ou basée sur une règle.
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Exemple : créer une forme aléatoire en fonction de la distance au centre
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(width / 2, height / 2));
                isTileValid[x, z] = distance < (width / 2) - Random.Range(0, width / 4); // Permet une variation dans la forme

                // Ajoute ici un calcul de probabilité pour l'eau (si nécessaire)
                if (isTileValid[x, z])
                {
                    waterProbabilityMap[x, z] = Random.Range(0f, 1f);  // Exemple simple de probabilité d'eau
                }
                else
                {
                    waterProbabilityMap[x, z] = 0f;  // Pas d'eau dans les cases invalides
                }
            }
        }

        tiles = new Tile[width, height];

        // Génère les tuiles en fonction des cases valides
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (isTileValid[x, z])
                {
                    float xOffset = (z % 2 != 0) ? tileSizeX * 0.5f : 0;
                    Vector3 position = new Vector3(x * tileSizeX + xOffset, 0, z * tileSizeZ);

                    TileType tileType = tileGenerationManager.GetRandomTileType(x, z, waterProbabilityMap);  // Passe waterProbabilityMap
                    GameObject prefabToInstantiate = tilePrefab;

                    if (tileType == TileType.Water && riverTilePrefab != null)
                    {
                        prefabToInstantiate = riverTilePrefab;
                    }
                    else if (tileType == TileType.StoneQuarry && stoneQuarryTilePrefab != null)
                    {
                        prefabToInstantiate = stoneQuarryTilePrefab;
                    }

                    GameObject newTile = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
                    newTile.name = $"Tile_{x}_{z}";

                    Tile tileComponent = newTile.GetComponent<Tile>();
                    tileComponent.SetPosition(x, z);
                    tiles[x, z] = tileComponent;
                    tileComponent.tileType = tileType;

                    if (tileType == TileType.Mountain && mountainPrefab != null)
                    {
                        CreatePrefabOnTile(mountainPrefab, newTile, x, z);
                    }
                    else if (tileType == TileType.Mine && minePrefab != null)
                    {
                        CreatePrefabOnTile(minePrefab, newTile, x, z);
                        tileComponent.tag = "Structure"; // Tag "Structure" pour les Mines
                        tileComponent.isOccupied = true;
                    }
                    else if (tileType == TileType.Sawmill && sawmillPrefab != null)
                    {
                        CreatePrefabOnTile(sawmillPrefab, newTile, x, z);
                        tileComponent.tag = "Structure"; // Tag "Structure" pour les Mines
                        tileComponent.isOccupied = true;
                    }
                    else if (tileType == TileType.StoneQuarry && stoneQuarryPrefab != null)
                    {
                        CreatePrefabOnTile(stoneQuarryPrefab, newTile, x, z);
                        tileComponent.tag = "Structure"; // Tag "Structure" pour les Mines
                        tileComponent.isOccupied = true;
                    }
                    tileComponent.UpdateVegetation();
                }
            }
        }
    }
    private bool IsWaterNear(int x, int z, float[,] waterProbabilityMap)
    {
        // Vérifier les voisins directs pour une probabilité d'eau plus élevée
        float probabilityThreshold = 0.3f;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;

                // Vérifier si le voisin est dans les limites de la grille et si de l'eau existe autour
                if (nx >= 0 && nx < width && nz >= 0 && nz < height && waterProbabilityMap[nx, nz] > probabilityThreshold)
                {
                    return true; // S'il y a de l'eau à proximité
                }
            }
        }
        return false;
    }
    private void CreatePrefabOnTile(GameObject prefab, GameObject tile, int x, int z)
    {
        GameObject instance = Instantiate(prefab, tile.transform.position, Quaternion.identity, tile.transform);
        instance.name = $"{prefab.name}_{x}_{z}";
    }

    public Tile GetTileAtPosition(int x, int z)
    {
        if (IsValidPosition(x, z))
        {
            return tiles[x, z];
        }

        return null;
    }

    public bool IsValidPosition(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height;
    }
}