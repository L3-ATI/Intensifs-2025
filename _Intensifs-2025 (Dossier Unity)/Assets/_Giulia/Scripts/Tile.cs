using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

public enum TileType {
    Grass, Mountain, Water, Station, UpgradedStation,
    Rail00, Rail01, Rail02, Rail03, Rail04, Rail05,
    Bridge, Tunnel, Mine, Sawmill, StoneQuarry,
    Desert, City
}

public class Tile : MonoBehaviour
{
    public bool isConnected = false;
    
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
    public static bool isUpgradeActive = false;
    public GameObject mountainPrefab;
    
    public GameObject vegetation;
    public GameObject houses;
    
    [Space(20)]
    public GameObject placementParticlesPrefab;
    
    private void Awake()
    {
        
        if (vegetation != null)
        {
            vegetation.SetActive(false);
        }
        
        if (houses != null)
        {
            houses.SetActive(false);
        }
        
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
        cancelButton.onClick.AddListener(() => CancelPlacement(8));
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

        RemoveHighlight(highlightValidMaterial);
        RemoveHighlight(highlightInvalidMaterial);

        switch (tileType)
        {
            case TileType.Grass:
            case TileType.Desert:
                if (objectType == "Station")
                {
                    bool hasStructureNeighbor = false;
                    foreach (Tile neighbor in neighboringTiles)
                    {
                        if (neighbor != null && (neighbor.CompareTag("Structure") ||
                                                 neighbor.tileType == TileType.Sawmill ||
                                                 neighbor.tileType == TileType.Mine ||
                                                 neighbor.tileType == TileType.StoneQuarry))
                        {
                            hasStructureNeighbor = true;
                            break;
                        }
                    }
                    if (!hasStructureNeighbor)
                    {
                        return false;
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
                return false;
        }
    }

    private bool CanPlaceTunnelOrBridge(string objectType)
    {
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
        if (isUpgradeActive || isShovelActive || IsPointerOverUIElement() || !UIManager.isAButtonClicked)
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
                BuildableItem stationItem = null;
                foreach (var item in RessourcesManager.Instance.buildableItems)
                {
                    if (item.itemName == "Station")
                    {
                        stationItem = item;
                        break;
                    }
                }

                if (stationItem != null && RessourcesManager.Instance.CanAffordItem(stationItem))
                {
                    ShowPlacementUI(GridInteraction.Instance.stationPrefab);
                    return;
                }
                else
                {
                    TooltipManager.Instance.ShowTooltip("You can't afford the station !");
                    return;
                }
            }
            
            if (GridInteraction.Instance.objectTypeToPlace == "Bridge")
            {
                BuildableItem bridgeItem = null;
                foreach (var item in RessourcesManager.Instance.buildableItems)
                {
                    if (item.itemName == "Bridge")
                    {
                        bridgeItem = item;
                        break;
                    }
                }

                if (bridgeItem != null && RessourcesManager.Instance.CanAffordItem(bridgeItem))
                {
                    ShowPlacementUI(GridInteraction.Instance.bridgePrefab);
                    return;
                }
                else
                {
                    TooltipManager.Instance.ShowTooltip("You can't afford the bridge !");
                    return;
                }
            }
            
            if (GridInteraction.Instance.objectTypeToPlace == "Tunnel")
            {
                BuildableItem tunnelItem = null;
                foreach (var item in RessourcesManager.Instance.buildableItems)
                {
                    if (item.itemName == "Tunnel")
                    {
                        tunnelItem = item;
                        break;
                    }
                }

                if (tunnelItem != null && RessourcesManager.Instance.CanAffordItem(tunnelItem))
                {
                    ShowPlacementUI(GridInteraction.Instance.tunnelPrefab);
                    return;
                }
                else
                {
                    TooltipManager.Instance.ShowTooltip("You can't afford the tunnel !");
                    return;
                }
            }
            
            else if (GridInteraction.Instance.objectTypeToPlace.StartsWith("Rail"))
            {
                string railType = GridInteraction.Instance.objectTypeToPlace;
                BuildableItem railItem = null;

                Dictionary<string, string> railTypeMapping = new Dictionary<string, string>()
                {
                    { "Rail00", "Rail 01" },
                    { "Rail01", "Rail 02" },
                    { "Rail02", "Rail 03" },
                    { "Rail03", "Rail 04" },
                    { "Rail04", "Rail 05" },
                    { "Rail05", "Rail 06" },
                };

                if (railTypeMapping.ContainsKey(railType))
                {
                    string itemName = railTypeMapping[railType];
                    railItem = RessourcesManager.Instance.buildableItems.FirstOrDefault(item => item.itemName == itemName);
                }

                if (railItem != null && RessourcesManager.Instance.CanAffordItem(railItem))
                {
                    GameObject railPrefab = GridInteraction.Instance.GetRailPrefab(railType);
                    ShowPlacementUI(railPrefab);
                    return;
                }
                else
                {
                    TooltipManager.Instance.ShowTooltip($"You can't afford this rail section!");
                    return;
                }
            }

        }
    }
    
    private void OnMouseEnter()
    {
        if (isUpgradeActive || isShovelActive || IsPointerOverUIElement() || !UIManager.isAButtonClicked)
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
        if (isUpgradeActive || isShovelActive || IsPointerOverUIElement() || !UIManager.isAButtonClicked)
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
        if (tileType != TileType.Grass && tileType != TileType.Desert || isOccupied)
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

        if (transform.childCount > 9 && transform.GetChild(9) != null)
        {
            transform.GetChild(9).rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
        else if (transform.childCount > 8 && transform.GetChild(8) != null)
        {
            transform.GetChild(8).rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }

        tileCanvas.transform.rotation = Quaternion.Euler(90, 0, 180);
    }

    
    private void ValidatePlacement()
    {
        isConnected = false;
        if (tileType.ToString().StartsWith("Rail") || tileType == TileType.Tunnel || tileType == TileType.Bridge)
        {

            if (connectedRails.Count > 0)
            {
                isConnected = true;
            }
            else
            {
                foreach (Tile neighbor in neighboringTiles)
                {
                    if (neighbor.tileType == TileType.Station)
                    {
                        isConnected = true;
                        break;
                    }
                }
            }

            if (!isConnected)
            {
                Debug.LogWarning($"Échec de la validation : la tuile {name} n'est pas connectée à une station ou un rail.");
                TooltipManager.Instance.ShowTooltip("Rails, tunnels and bridges needs to be connected to a rail section or a station !");
                return;
            }
        }

        else if (tileType == TileType.Tunnel || tileType == TileType.Bridge)
        {
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
                Debug.LogWarning($"Échec de la validation : Tunnel ou Bridge non connecté correctement.");
                TooltipManager.Instance.ShowTooltip("Rails, tunnels and bridges needs to be connected to a rail section or a station !");
                return;
            }
        }

        if (transform.childCount > 7)
        {
            Transform child = transform.GetChild(7);
            if (child.name != "Vegetation")
            {
                Destroy(child.gameObject);
            }
        }
        
        if (placementParticlesPrefab != null)
        {
            GameObject particles = Instantiate(placementParticlesPrefab, transform.position, Quaternion.identity);
            particles.transform.SetParent(transform);

            Destroy(particles, 2f);
        }
        else
        {
            Debug.LogWarning("Prefab de particules non assigné pour la tuile.");
        }


        HandlePurchase();
        PlaceObjectOnTile();
        tileCanvas.enabled = false;
    }

    private void HandlePurchase()
    {
        RessourcesManager ressourcesManager = FindObjectOfType<RessourcesManager>();

        if (ressourcesManager == null)
        {
            Debug.LogError("RessourcesManager introuvable dans la scène !");
            return;
        }

        BuildableItem itemToPurchase = null;

        foreach (var item in ressourcesManager.buildableItems)
        {
            if (
                (tileType == TileType.Rail00 && item.itemName == "Rail 01") ||
                (tileType == TileType.Rail00 && item.itemName == "Rail 02") ||
                (tileType == TileType.Rail00 && item.itemName == "Rail 03") ||
                (tileType == TileType.Rail00 && item.itemName == "Rail 04") ||
                (tileType == TileType.Rail00 && item.itemName == "Rail 05") ||
                (tileType == TileType.Rail00 && item.itemName == "Rail 06") ||
                (tileType == TileType.Tunnel && item.itemName == "Tunnel") ||
                (tileType == TileType.Bridge && item.itemName == "Bridge") ||
                (tileType == TileType.Station && item.itemName == "Station"))
            {
                itemToPurchase = item;
                break;
            }
        }

        if (itemToPurchase != null)
        {
            ressourcesManager.PurchaseItem(itemToPurchase);
        }
        else
        {
            Debug.LogWarning($"Aucun BuildableItem associé au type {tileType}.");
        }
    }

    public void CancelPlacement(int childIndex)
    {
        ChangeTileTypeToPrevious();
        
        if (transform.childCount > childIndex && childIndex >= 0)
        {
            GameObject childObject = transform.GetChild(childIndex).gameObject;

            if (childObject.name == "Vegetation")
            {
                childObject = transform.GetChild(childIndex+1).gameObject;
            }
            
            childObject.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (childObject.name != "Vegetation")
                    {
                        Destroy(childObject);
                    }
                });
        }

        tileCanvas.enabled = false;
        isOccupied = false;
    }

    private void ChangeTileTypeToPrevious()
    {
        if (tileType == TileType.Tunnel)
        {
            Object newMountain = Instantiate(mountainPrefab, transform.position, Quaternion.identity);
            newMountain.GameObject().transform.SetParent(transform);
            SetTileType(TileType.Mountain);
        }
        if (tileType == TileType.Bridge)
        {
            SetTileType(TileType.Water);
        }
        else if (tileType.ToString().StartsWith("Rail") || tileType == TileType.Station)
        {
            SetTileType(TileType.Grass);
        }
    }
    
    public void SetTileType(TileType newType)
    {
        tileType = newType; // Mettre à jour le type
        UpdateVegetation(); // Mettre à jour l'état de la végétation
        UpdateCity(); // Mettre à jour l'état de la végétation
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

    public void UpdateVegetation()
    {
        if (vegetation != null)
        {
            if (tileType == TileType.Grass)
            {
                vegetation.GetComponent<GrassTile>().isGrassTile = true;
                vegetation.GetComponent<GrassTile>().isDesertTile = false;
                vegetation.SetActive(true);
                vegetation.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InBack);
            }
            else if (tileType == TileType.Desert)
            {
                vegetation.GetComponent<GrassTile>().isDesertTile = true;
                vegetation.GetComponent<GrassTile>().isGrassTile = false;
                vegetation.SetActive(true);
                vegetation.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InBack);
            }
            else
            {
                vegetation.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            }
        }
    }
    public void UpdateCity()
    {
        if (houses != null)
        {
            if (tileType == TileType.City)
            {
                houses.SetActive(true);
                houses.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InBack);
            }
            else
            {
                houses.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            }
        }
    }
}
