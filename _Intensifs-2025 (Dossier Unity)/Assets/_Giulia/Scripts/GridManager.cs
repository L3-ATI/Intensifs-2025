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

    private Tile[,] tiles;

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

                TileType tileType = GetRandomTileType();

                GameObject prefabToInstantiate = tilePrefab;
                if (tileType == TileType.Water && riverTilePrefab != null)
                {
                    prefabToInstantiate = riverTilePrefab;
                }

                GameObject newTile = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
                newTile.name = $"Tile_{x}_{z}";

                Tile tileComponent = newTile.GetComponent<Tile>();
                tileComponent.SetPosition(x, z);
                tiles[x, z] = tileComponent;
                tileComponent.tileType = tileType;

                if (tileType == TileType.Mountain && mountainPrefab != null)
                {
                    GameObject mountain = Instantiate(mountainPrefab, newTile.transform.position, Quaternion.identity, newTile.transform);
                    mountain.name = $"Mountain_{x}_{z}";
                }
            }
        }
    }
    
    TileType GetRandomTileType()
    {
        float randomValue = Random.value;

        if (randomValue < 0.2f) return TileType.Water;
        if (randomValue < 0.3f) return TileType.Mountain;
        return TileType.Grass;
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
