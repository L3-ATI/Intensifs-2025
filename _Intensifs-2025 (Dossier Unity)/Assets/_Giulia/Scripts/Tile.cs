using UnityEngine;

public enum TileType { Grass, Mountain, Water }

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
}
