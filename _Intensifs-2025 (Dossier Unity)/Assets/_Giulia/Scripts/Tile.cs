using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

public enum TileType {
    Grass, Mountain, Water, Station,
    Rail00, Rail01, Rail02, Rail03, Rail04, Rail05,
    Bridge, Tunnel, Mine, Sawmill, StoneQuarry
}
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
    private Material originalMaterial0, originalMaterial1;
    
    public static bool isShovelActive = false;
    public GameObject mountainPrefab;
    
    private void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial0 = tileRenderer.materials.Length > 0 ? tileRenderer.materials[0] : null;
            originalMaterial1 = tileRenderer.materials.Length > 1 ? tileRenderer.materials[1] : null;
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
        /*if (isOccupied)
        {
            return false;
        }*/

        RemoveHighlight(highlightValidMaterial);
        RemoveHighlight(highlightInvalidMaterial);

        switch (tileType)
        {
            case TileType.Grass:
                if (objectType == "Station")
                {
                    // Vérifie si au moins un voisin a le tag "Structure" (ex: Sawmill, Mine, etc.)
                    bool hasStructureNeighbor = false;
                    foreach (Tile neighbor in neighboringTiles)
                    {
                        if (neighbor != null && (neighbor.CompareTag("Structure") || neighbor.tileType == TileType.Sawmill || neighbor.tileType == TileType.Mine || neighbor.tileType == TileType.StoneQuarry))
                        {
                            hasStructureNeighbor = true;
                            break;
                        }
                    }
                    if (!hasStructureNeighbor)
                    {
                        return false; // Aucun voisin structure
                    }
                }

                if (objectType.StartsWith("Rail"))
                {
                    return CanPlaceRail(objectType);
                }
                else if (objectType == "Tunnel" || objectType == "Bridge")
                {
                    return CanPlaceTunnelOrBridge(objectType);
                }
                return true;

            case TileType.Mountain:
                return objectType == "Tunnel";

            case TileType.Water:
                return objectType == "Bridge";

            case TileType.Sawmill:
                return objectType == "Sawmill";

            case TileType.Mine:
                return objectType == "Mine";

            case TileType.StoneQuarry:
                return objectType == "Stone Quarry";

            default:
                Debug.LogWarning($"Invalid tile type {tileType} for {objectType}");
                return false;
        }
    }

    private bool CanPlaceTunnelOrBridge(string objectType)
    {
        // Vérifie que le tunnel ou le pont est adjacent à un rail ou une station
        foreach (Tile neighbor in neighboringTiles)
        {
            if (neighbor != null && (neighbor.tileType.ToString().StartsWith("Rail") || neighbor.tileType == TileType.Station))
            {
                return true;
            }
        }

        return false;
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

        if (tileRenderer != null)
        {
            // Supprime tous les matériaux actuels
            tileRenderer.materials = new Material[0];

            // Remet les matériaux originaux
            if (originalMaterial0 != null)
            {
                List<Material> originalMaterials = new List<Material> { originalMaterial0 };

                if (originalMaterial1 != null)
                {
                    originalMaterials.Add(originalMaterial1);
                }

                tileRenderer.materials = originalMaterials.ToArray();
            }
            else
            {
                Debug.LogWarning("OriginalMaterial0 is null. Cannot reset materials.");
            }
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
            if (neighbor != null && neighbor.isOccupied && neighbor.tileType.ToString().StartsWith("Rail") )
            {
                return true;
            }
            if (neighbor != null && neighbor.isOccupied && neighbor.tileType == TileType.Tunnel)
            {
                return true;
            }
            if (neighbor != null && neighbor.isOccupied && neighbor.tileType == TileType.Bridge)
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
        
        else if (tileType == TileType.Tunnel || tileType == TileType.Bridge)
        {
            // Vérifie la connexion pour Tunnel et Bridge
            foreach (Tile neighbor in neighboringTiles)
            {
                if (neighbor.tileType.ToString().StartsWith("Rail") || neighbor.tileType == TileType.Station)
                {
                    isConnected = true;
                    break;
                }
            }

            if (!isConnected)
            {
                TooltipManager.Instance.ShowTooltip("Tunnels and bridges need to be connected to another rail or a station!");
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
        SetTileType();
        
        if (transform.childCount > childIndex && childIndex >= 0)
        {
            GameObject childObject = transform.GetChild(childIndex).gameObject;

            childObject.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    Destroy(childObject);
                });
        }

        tileCanvas.enabled = false;
        isOccupied = false;
    }

    private void SetTileType()
    {
        if (tileType == TileType.Tunnel)
        {
            Object newMountain = Instantiate(mountainPrefab, transform.position, Quaternion.identity);
            newMountain.GameObject().transform.SetParent(transform);
            tileType = TileType.Mountain;
        }
        if (tileType == TileType.Bridge)
        {
            tileType = TileType.Water;
        }
        else if (tileType.ToString().StartsWith("Rail") || tileType == TileType.Station)
        {
            tileType = TileType.Grass;
        }
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
            GameObject child = transform.GetChild(i).gameObject;

            child.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => Destroy(child));
        }
    }
}
