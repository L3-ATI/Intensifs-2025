using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width;
    public int height;
    public float tileSize;
    public GameObject tilePrefab;

    private Tile[,] tiles;

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
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);

                // Instancier la tuile
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                newTile.name = $"Tile_{x}_{z}";

                // Configurer le script Tile attaché
                Tile tileComponent = newTile.GetComponent<Tile>();
                tileComponent.SetPosition(x, z);
                tiles[x, z] = tileComponent;

                // Définir aléatoirement le type de tuile
                tileComponent.tileType = (Random.value > 0.7f) ? TileType.Mountain : TileType.Grass;
            }
        }
    }
}