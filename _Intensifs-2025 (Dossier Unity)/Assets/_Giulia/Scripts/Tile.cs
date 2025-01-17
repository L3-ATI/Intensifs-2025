using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TileType { Grass, Mountain, Water, Station, Rail00, Rail01, Rail02, Rail03, Rail04, Rail05, Bridge, Tunnel }
public class Tile : MonoBehaviour
{
    
    [Space(20)]

    public Material highlightValidMaterial;
    public Material highlightInvalidMaterial;
    [Space(20)]
    
    public int gridX;
    public int gridZ;

    [Space(20)]

    public bool isOccupied = false;
    public TileType tileType;

    [Space(20)]
    
    public Canvas tileCanvas;
    public Button rotateButton;
    public Button validateButton;
    public Button cancelButton;
    public GameObject objectToPlace;

    [Space(20)]
    
    public List<Tile> neighboringTiles = new List<Tile>();
    public List<GameObject> connectedRails = new List<GameObject>();
    
    private GameObject placedObject;
    private Renderer tileRenderer;
    private float rotationAngle = 0f;
    private Material originalMaterial;

    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material; // Sauvegarde du material de base
        }
        RemoveHighlight(highlightValidMaterial);
        RemoveHighlight(highlightInvalidMaterial);

        tileCanvas.enabled = false;  // Désactive le Canvas par défaut

        // Ajouter des listeners aux boutons
        rotateButton.onClick.AddListener(RotateTile);
        validateButton.onClick.AddListener(ValidatePlacement);
        cancelButton.onClick.AddListener(() => CancelPlacement(7));  // Exemple : supprimer l'enfant à l'index 0
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
        if (Input.GetKeyDown(KeyCode.N))
        {
            PrintNeighbors();
        }
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

    public bool CanPlaceObject(string objectType)
    {
        if (isOccupied)
            return false;

        RemoveHighlight(highlightValidMaterial);
        RemoveHighlight(highlightInvalidMaterial);

        switch (tileType)
        {
            case TileType.Grass:
                if (objectType.StartsWith("Rail"))
                {
                    return CanPlaceRail(objectType);
                }
                return true;

            case TileType.Mountain:
                return objectType == "Tunnel";

            case TileType.Water:
                return objectType == "Bridge";

            default:
                return false;
        }
    }

    private void ApplyHighlight(Material highlightMaterial)
    {
        if (tileRenderer != null && highlightMaterial != null)
        {
            Material[] materials = tileRenderer.materials;
            List<Material> newMaterials = new List<Material>(materials);

            if (!newMaterials.Contains(highlightMaterial))
            {
                newMaterials.Add(highlightMaterial);
                tileRenderer.materials = newMaterials.ToArray();
            }
        }
    }

    private void RemoveHighlight(Material highlightMaterial)
    {
        if (tileRenderer != null && highlightMaterial != null)
        {
            Material[] materials = tileRenderer.materials;
            List<Material> newMaterials = new List<Material>(materials);

            if (newMaterials.Contains(highlightMaterial))
            {
                newMaterials.Remove(highlightMaterial);
                tileRenderer.materials = newMaterials.ToArray();
            }
        }
    }



    // Marque la tuile comme occupée ou libre.
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
    
    
    private void OnMouseDown()
    {
        if (IsPointerOverUIElement())
        {
            return;
        }
        
        if (GridInteraction.Instance != null)
        {
            if (GridInteraction.Instance.objectTypeToPlace == "RailStraight" || GridInteraction.Instance.objectTypeToPlace == "RailCurved")
            {
                if (!CanPlaceRail(GridInteraction.Instance.objectTypeToPlace))
                {
                    TooltipManager.Instance.ShowTooltip("Rails need to be connected to another rail or a station!");
                    return;
                }
            }
            else
            {
                if (!CanPlaceObject(GridInteraction.Instance.objectTypeToPlace))
                {
                    TooltipManager.Instance.ShowTooltip("Can't build here.");
                    return;
                }
            }

            // Si l'objet à placer est une station ou un rail
            if (GridInteraction.Instance.objectTypeToPlace == "Station")
            {
                ShowPlacementUI(GridInteraction.Instance.stationPrefab);
            }
            else if (GridInteraction.Instance.objectTypeToPlace.StartsWith("Rail"))
            {
                GameObject prefab = GridInteraction.Instance.GetRailPrefab(GridInteraction.Instance.objectTypeToPlace);
                ShowPlacementUI(prefab);
            }
        }
    }
    private void OnMouseEnter()
    {
        if (IsPointerOverUIElement())
        {
            return;
        }

        if (GridInteraction.Instance != null)
        {
            OnMouseExit();

            bool canPlace = false;

            if (GridInteraction.Instance.objectTypeToPlace == "RailStraight" || GridInteraction.Instance.objectTypeToPlace == "RailCurved")
            {
                canPlace = CanPlaceRail(GridInteraction.Instance.objectTypeToPlace);
            }
            else
            {
                canPlace = CanPlaceObject(GridInteraction.Instance.objectTypeToPlace);
            }

            if (canPlace)
            {
                ApplyHighlight(highlightValidMaterial);
            }
            else
            {
                ApplyHighlight(highlightInvalidMaterial);
            }
        }
    }

    private void OnMouseExit()
    {
    
        if (tileRenderer != null && originalMaterial != null)
        {
            tileRenderer.materials = new Material[] { originalMaterial };
        }
        else
        {
            Debug.LogWarning("TileRenderer or OriginalMaterial is null. Cannot reset materials.");
        }
    }
    
    public bool CanPlaceRail(string railType)
    {
        if (tileType != TileType.Grass || isOccupied)
            return false;

        // Vérifie s'il est connecté à une station occupée
        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && neighbor.tileType == TileType.Station && neighbor.isOccupied)
            {
                return true;
            }
        }

        // Vérifie s'il est connecté à un autre rail valide
        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && neighbor.tileType.ToString().StartsWith("Rail") && neighbor.isOccupied)
            {
                return true;
            }
        }

        return false;
    }
    
    // Lorsqu'on clique sur la tuile, on active le Canvas
    public void ShowPlacementUI(GameObject objectToPlacePrefab)
    {
        PlaceObjectOnTile();
        tileCanvas.enabled = true;
    }

    private void RotateTile()
    {
        rotationAngle -= 60f;
        if (rotationAngle < 0f) 
            rotationAngle += 360f;
        
        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);


        // Applique une rotation absolue au canvas (en fonction du monde)
        tileCanvas.transform.rotation = Quaternion.Euler(90, 0, 180);
    }
    private void ValidatePlacement()
    {
        bool isConnected = false;

        if (connectedRails.Count == 0 && tileType.ToString().StartsWith("Rail"))
        {
            foreach (Tile neighbor in neighboringTiles)
            {
                if (neighbor.tileType == TileType.Station)
                {
                    isConnected = true;
                    break;
                }
            }

            if (!isConnected)
            {
                TooltipManager.Instance.ShowTooltip("Rails need to be connected to another rail or a station!");
                return;
            }
        }

        Debug.Log("Placement validé !");
        PlaceObjectOnTile();
        tileCanvas.enabled = false;
    }



    public void CancelPlacement(int childIndex)
    {
        // Vérifie si l'index est valide (l'enfant existe)
        if (transform.childCount > childIndex && childIndex >= 0)
        {
            // Trouve et détruit l'enfant spécifique à l'index
            Destroy(transform.GetChild(childIndex).gameObject);
        }

        // Réinitialise l'état de la tuile
        SetOccupied(false);
        tileType = TileType.Grass;  // Réinitialisation du type de tuile à herbe (ou autre type par défaut)
        tileCanvas.enabled = false;
    }

    
    private void PlaceObjectOnTile()
    {
        if (objectToPlace != null && !isOccupied)
        {
            GridInteraction.Instance.PlaceObject(this, objectToPlace);
        }
    }
    private bool IsPointerOverUIElement()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Liste des résultats du raycast
        List<RaycastResult> results = new List<RaycastResult>();

        // Effectue le raycast pour tous les éléments UI
        EventSystem.current.RaycastAll(pointerData, results);

        // Retourne vrai si la souris est sur un élément UI
        return results.Count > 0;
    }

}
