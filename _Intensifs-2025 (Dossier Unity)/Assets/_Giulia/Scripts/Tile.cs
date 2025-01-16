using UnityEngine;
using System.Collections.Generic;

public enum TileType { Grass, Mountain, Water, Station }

public class Tile : MonoBehaviour
{
    // Coordonnées de la tuile dans la grille
    public int gridX;
    public int gridZ;

    // État de la tuile
    public bool isOccupied = false;

    // Type de la tuile
    public TileType tileType;

    // Référence au Renderer pour changer la couleur (highlight)
    private Renderer tileRenderer;

    // Liste des voisins détectés via les triggers
    private List<Tile> neighboringTiles = new List<Tile>();

    // Couleurs pour l'état de la tuile
    public Color defaultColor = Color.white;
    public Color highlightValidColor = Color.green;
    public Color highlightInvalidColor = Color.red;

    private void Awake()
    {
        // Initialisation du Renderer
        tileRenderer = GetComponent<Renderer>();
        ResetColor();
    }

    // Définit la position de la tuile dans la grille.
    public void SetPosition(int x, int z)
    {
        gridX = x;
        gridZ = z;
    }
    
    public void AddNeighbor(Tile neighbor)
    {
        if (!neighboringTiles.Contains(neighbor))
        {
            neighboringTiles.Add(neighbor);
            UpdateNeighbors(neighboringTiles); // Méthode pour gérer la logique de mise à jour des voisins
        }
    }

    public void RemoveNeighbor(Tile neighbor)
    {
        if (neighboringTiles.Contains(neighbor))
        {
            neighboringTiles.Remove(neighbor);
            UpdateNeighbors(neighboringTiles); // Met à jour la liste après suppression
        }
    }

    public void UpdateNeighbors(List<Tile> neighbors)
    {
        // Logique pour mettre à jour les voisins dans le Tile (peut-être afficher la liste ou l'utiliser ailleurs)
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            PrintNeighbors();
        }
    }
    private void PrintNeighbors()
    {
        if (neighboringTiles.Count == 0)
        {
            Debug.Log($"Aucun voisin trouvé pour la tuile {name}");
        }
        else
        {
            Debug.Log($"Voisins de la tuile {name}:");
            foreach (Tile neighbor in neighboringTiles)
            {
                // Vérifie si le voisin est une station ou un autre type
                if (neighbor.tileType == TileType.Station)
                {
                    Debug.Log($"- {neighbor.name} (Station)");
                }
                else
                {
                    Debug.Log($"- {neighbor.name} ({neighbor.tileType})");
                }
            }
        }
    }

    // Vérifie si un objet peut être placé sur cette tuile.
    public bool CanPlaceObject(string objectType)
    {
        if (isOccupied)
            return false;

        // Règles de placement spécifiques selon le type de tuile et d'objet
        switch (tileType)
        {
            case TileType.Grass:
                if (objectType == "RailStraight" || objectType == "RailCurved")
                {
                    // Vérifie si le rail peut être placé à proximité d'une station
                    return CanPlaceRail();
                }
                // Aucun objet spécifique n'est interdit sur l'herbe
                return true;

            case TileType.Mountain:
                // Seuls les tunnels peuvent être placés sur les montagnes
                return objectType == "Tunnel";

            case TileType.Water:
                // Seuls les ponts peuvent être placés sur l'eau
                return objectType == "Bridge";

            default:
                return false;
        }
    }

    // Met en surbrillance la tuile selon la validité d'un placement.
    public void HighlightTile(bool isValid)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = isValid ? highlightValidColor : highlightInvalidColor;
        }
    }

    // Réinitialise la couleur de la tuile à sa couleur par défaut.
    public void ResetColor()
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = defaultColor;
        }
    }

    // Marque la tuile comme occupée ou libre.
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    // Quand la souris entre sur la tuile, on change la couleur si c'est un placement valide
    private void OnMouseEnter()
    {
        if (GridInteraction.Instance != null)
        {
            bool canPlace = false;

            // On exclut d'emblée les montagnes et l'eau
            if (tileType == TileType.Mountain || tileType == TileType.Water)
            {
                HighlightTile(false);
                return;
            }

            // Vérification selon le type d'objet
            if (GridInteraction.Instance.objectTypeToPlace == "RailStraight" || GridInteraction.Instance.objectTypeToPlace == "RailCurved")
            {
                canPlace = CanPlaceRail();
            }
            else
            {
                canPlace = CanPlaceObject(GridInteraction.Instance.objectTypeToPlace);
            }

            HighlightTile(canPlace);
        }
    }


    // Quand la souris quitte la tuile, on réinitialise la couleur de la tuile
    private void OnMouseExit()
    {
        ResetColor();
    }
    
    // Exemple de méthode de placement de rail
    public bool CanPlaceRail()
    {
        // La tuile doit être de l'herbe et non occupée
        if (tileType != TileType.Grass || isOccupied)
            return false;

        // Vérifie si un voisin est une station
        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && neighbor.tileType == TileType.Station && neighbor.isOccupied)
            {
                return true;
            }
        }
        return false;
    }

    
}
