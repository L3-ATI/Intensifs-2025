using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TileType { Grass, Mountain, Water, Station, Rail00, Rail01, Rail02, Rail03, Rail04, Rail05, Bridge, Tunnel }
public class Tile : MonoBehaviour
{
    
    [Space(20)]

    public Material highlightValidMaterial, highlightInvalidMaterial;
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
    
    public static bool isShovelActive = false;
    
    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;
        }
        RemoveHighlight(highlightValidMaterial);
        RemoveHighlight(highlightInvalidMaterial);

        tileCanvas.enabled = false;

        rotateButton.onClick.AddListener(RotateTile);
        validateButton.onClick.AddListener(ValidatePlacement);
        cancelButton.onClick.AddListener(() => CancelPlacement(7));
    }

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
            UpdateNeighbors(neighboringTiles);
        }
    }

    public void RemoveNeighbor(Tile neighbor)
    {
        if (neighboringTiles.Contains(neighbor))
        {
            neighboringTiles.Remove(neighbor);
            UpdateNeighbors(neighboringTiles);
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
        {
            return false;
        }

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
                Debug.LogWarning($"Invalid tile type {tileType} for {objectType}");
                return false;
        }
    }

    public void ApplyHighlight(Material highlightMaterial)
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

    public void RemoveHighlight(Material highlightMaterial)
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
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
    
    
    private void OnMouseDown()
    {
        if (isShovelActive || IsPointerOverUIElement())
        {
            return;
        }
    
        if (GridInteraction.Instance != null)
        {
            if (GridInteraction.Instance.objectTypeToPlace == "RailStraight" || GridInteraction.Instance.objectTypeToPlace == "RailCurved")
            {
                if (!CanPlaceRail(GridInteraction.Instance.objectTypeToPlace))
                {
                    TooltipManager.Instance.ShowTooltip(isOccupied ? 
                        "You can't build on another build." : 
                        "Rails need to be connected to another rail or a station!");
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

            if (GridInteraction.Instance.objectTypeToPlace == "Station")
            {
                ShowPlacementUI(GridInteraction.Instance.stationPrefab);
            }
            else if (GridInteraction.Instance.objectTypeToPlace == "Bridge")
            {
                ShowPlacementUI(GridInteraction.Instance.bridgePrefab);
            }
            else if (GridInteraction.Instance.objectTypeToPlace == "Tunnel")
            {
                if (transform.childCount > 7 && 7 >= 0)
                {
                    Destroy(transform.GetChild(7).gameObject);
                }
                ShowPlacementUI(GridInteraction.Instance.tunnelPrefab);
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
        if (isShovelActive || IsPointerOverUIElement())
            return;

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
        if (isShovelActive || IsPointerOverUIElement())
        {
            return;
        }

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

        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && neighbor.tileType == TileType.Station && neighbor.isOccupied)
            {
                return true;
            }
        }

        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && neighbor.tileType.ToString().StartsWith("Rail") && neighbor.isOccupied)
            {
                return true;
            }
        }

        return false;
    }
    
    public void ShowPlacementUI(GameObject objectToPlacePrefab)
    {
        tileCanvas.enabled = true;
        objectToPlace = objectToPlacePrefab;
    }
    
    private void RotateTile()
    {
        rotationAngle -= 60f;
        if (rotationAngle < 0f) 
            rotationAngle += 360f;
        
        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        
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
        if (isOccupied)
        {
            tileCanvas.enabled = false;
            return;
        }

        if (transform.childCount > 7 && 7 >= 0)
        {
            Destroy(transform.GetChild(7).gameObject);
        }
    
        Debug.Log("Placement validé !");
        PlaceObjectOnTile();
        tileCanvas.enabled = false;
    }

    public void CancelPlacement(int childIndex)
    {
        if (transform.childCount > childIndex && childIndex >= 0)
        {
            Destroy(transform.GetChild(childIndex).gameObject);
        }

        SetOccupied(false);
        tileType = TileType.Grass;
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

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }
    
    public void DestroyChildrenFromIndex(int startIndex)
    {
        if (startIndex < 0 || startIndex >= transform.childCount)
        {
            return;
        }

        for (int i = transform.childCount - 1; i >= startIndex; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }


}
