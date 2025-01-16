using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Singleton
    public static GridManager Instance { get; private set; }

    public int width; // Largeur de la grille
    public int height; // Hauteur de la grille
    public float tileSizeX = 1f; // Taille des tuiles sur l'axe X
    public float tileSizeZ = 1f; // Taille des tuiles sur l'axe Z
    public GameObject tilePrefab; // Préfabriqué pour les tuiles
    public GameObject mountainPrefab; // Préfab pour les montagnes
    public GameObject riverTilePrefab; // Préfab pour les tuiles de rivière

    private Tile[,] tiles; // Tableau 2D pour stocker les tuiles

    void Awake()
    {
        // Configurer le singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Assurer qu'il n'y a qu'une seule instance
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

    // Génération de la grille
    void GenerateGrid()
    {
        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Décalage pour les lignes impaires
                float xOffset = (z % 2 != 0) ? tileSizeX * 0.5f : 0;

                // Calculer la position dans le monde
                Vector3 position = new Vector3(x * tileSizeX + xOffset, 0, z * tileSizeZ);

                // Déterminer le type de tuile
                TileType tileType = GetRandomTileType();

                // Sélectionner le prefab en fonction du type de tuile
                GameObject prefabToInstantiate = tilePrefab; // Par défaut, herbe
                if (tileType == TileType.Water && riverTilePrefab != null)
                {
                    prefabToInstantiate = riverTilePrefab; // Utiliser le prefab de rivière
                }

                // Instancier la tuile
                GameObject newTile = Instantiate(prefabToInstantiate, position, Quaternion.identity, transform);
                newTile.name = $"Tile_{x}_{z}";

                // Configurer la tuile
                Tile tileComponent = newTile.GetComponent<Tile>();
                tileComponent.SetPosition(x, z);
                tiles[x, z] = tileComponent;
                tileComponent.tileType = tileType;

                // Si la tuile est une montagne, instancier le prefab de montagne
                if (tileType == TileType.Mountain && mountainPrefab != null)
                {
                    GameObject mountain = Instantiate(mountainPrefab, newTile.transform.position, Quaternion.identity, newTile.transform);
                    mountain.name = $"Mountain_{x}_{z}";
                }
            }
        }
    }



    // Obtenir un type de tuile aléatoire
    TileType GetRandomTileType()
    {
        float randomValue = Random.value;

        if (randomValue < 0.1f) return TileType.Water; // 10% de chance d'eau
        if (randomValue < 0.3f) return TileType.Mountain; // 20% de chance de montagne
        return TileType.Grass; // 70% de chance d'herbe
    }

    // Obtenir une tuile à une position donnée
    public Tile GetTileAtPosition(int x, int z)
    {
        if (IsValidPosition(x, z))
        {
            return tiles[x, z];
        }

        return null;
    }

    // Vérifie si une position est valide dans la grille
    public bool IsValidPosition(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < height;
    }
}
