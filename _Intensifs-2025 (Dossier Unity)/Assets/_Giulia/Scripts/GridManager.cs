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
    public GameObject mountainPrefab;
    public GameObject riverTilePrefab;
    public GameObject minePrefab;
    public GameObject sawmillPrefab;
    public GameObject stoneQuarryTilePrefab;

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
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float xOffset = (z % 2 != 0) ? tileSizeX * 0.5f : 0;

                Vector3 position = new Vector3(x * tileSizeX + xOffset, 0, z * tileSizeZ);

                TileType tileType = tileGenerationManager.GetRandomTileType();

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

                // Add prefabs for specific tile types
                if (tileType == TileType.Mountain && mountainPrefab != null)
                {
                    CreatePrefabOnTile(mountainPrefab, newTile, x, z);
                }
                else if (tileType == TileType.Mine && minePrefab != null)
                {
                    CreatePrefabOnTile(minePrefab, newTile, x, z);
                }
                else if (tileType == TileType.Sawmill && sawmillPrefab != null)
                {
                    CreatePrefabOnTile(sawmillPrefab, newTile, x, z);
                }
            }
        }
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
