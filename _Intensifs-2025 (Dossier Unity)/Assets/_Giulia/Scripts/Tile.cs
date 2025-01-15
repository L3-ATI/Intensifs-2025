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

    // Vérifie si un objet peut être placé sur cette tuile.
    public bool CanPlaceObject(string objectType)
    {
        if (isOccupied)
            return false;

        // Règles de placement spécifiques selon le type de tuile et d'objet
        switch (tileType)
        {
            case TileType.Grass:
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
            // Vérifier si un objet peut être placé
            bool canPlace = CanPlaceObject(GridInteraction.Instance.objectTypeToPlace);
            HighlightTile(canPlace);
        }
    }

    // Quand la souris quitte la tuile, on réinitialise la couleur de la tuile
    private void OnMouseExit()
    {
        ResetColor();
    }

    // Vérifie si la tuile est connectée à une station
    public bool CanPlaceRail(Tile tile, GridManager gridManager)
    {
        // Récupérer les tuiles voisines à partir de la grille
        foreach (Tile neighbor in GetNeighboringTiles(tile, gridManager))
        {
            if (neighbor.isOccupied && neighbor.tileType == TileType.Station)
            {
                return true;
            }
        }
        return false;
    }

    // Cette méthode doit être implémentée pour récupérer les tuiles voisines à partir de la grille
    private List<Tile> GetNeighboringTiles(Tile tile, GridManager gridManager)
    {
        List<Tile> neighbors = new List<Tile>();
        // Ajouter la logique pour obtenir les tuiles voisines
        return neighbors;
    }
}
