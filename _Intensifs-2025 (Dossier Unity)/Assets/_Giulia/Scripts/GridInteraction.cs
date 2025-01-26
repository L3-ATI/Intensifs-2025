using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
public class GridInteraction : MonoBehaviour
{
    public static GridInteraction Instance;
    public string objectTypeToPlace;
    public GameObject stationPrefab;
    public GameObject railPrefab00, railPrefab01, railPrefab02, railPrefab03, railPrefab04, railPrefab05;
    public GameObject bridgePrefab, tunnelPrefab;

    private GameObject objectToPlace;
    private Tile currentTile; 
    private string currentObjectType;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (objectTypeToPlace != currentObjectType && objectTypeToPlace != null)
        {
            UpdateObjectToPlace();
            currentObjectType = objectTypeToPlace;
        }
        
        if (IsPointerOverUIElement())
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    private void UpdateObjectToPlace()
    {
        switch (objectTypeToPlace)
        {
            case "Station":
                objectToPlace = stationPrefab;
                break;
            case "Bridge":
                objectToPlace = bridgePrefab;
                break;
            case "Tunnel":
                objectToPlace = tunnelPrefab;
                break;
            default:
                if (objectTypeToPlace.StartsWith("Rail"))
                    objectToPlace = GetRailPrefab(objectTypeToPlace);
                break;
        }

    }
    
    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            currentTile = hit.collider.GetComponent<Tile>();
            if (currentTile != null)
            {
                HandleTileClick(currentTile);
            }
        }
        else
        {
            if (currentTile != null)
            {
                currentTile.CancelPlacement(7);
            }
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

    private void HandleTileClick(Tile tile)
    {
        if (ShovelManager.isShovelActive || IsPointerOverUIElement() || !UIManager.isAButtonClicked)
            return;
        
        if (!tile.CanPlaceObject(objectTypeToPlace))
        {
            string reason = GetPlacementErrorMessage(tile);
            TooltipManager.Instance.ShowTooltip(reason);
        }
        else
        {
            
            if (objectTypeToPlace.StartsWith("Rail"))
            {
                if (!tile.CanPlaceRail(objectTypeToPlace))
                {
                    TooltipManager.Instance.ShowTooltip("Rails need to be connected to a station!");
                    return;
                }
                //tile.ShowPlacementUI(objectToPlace);
            }

            /*else
            {
                tile.ShowPlacementUI(objectToPlace);
            }*/

            if (objectToPlace != null)
            {
                if (Instance.objectTypeToPlace == "Station")
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
                        PlaceObject(tile, objectToPlace);
                        return;
                    }
                    else
                    {
                        TooltipManager.Instance.ShowTooltip("You can't afford the station !");
                        return;
                    }
                }
            
                if (Instance.objectTypeToPlace == "Bridge")
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
                        PlaceObject(tile, objectToPlace);
                        return;
                    }
                    else
                    {
                        TooltipManager.Instance.ShowTooltip("You can't afford the bridge !");
                        return;
                    }
                }
            
                if (Instance.objectTypeToPlace == "Tunnel")
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
                        PlaceObject(tile, objectToPlace);
                        return;
                    }
                    else
                    {
                        TooltipManager.Instance.ShowTooltip("You can't afford the tunnel !");
                        return;
                    }
                }
            
                else if (Instance.objectTypeToPlace.StartsWith("Rail"))
                {
                    string railType = Instance.objectTypeToPlace;
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
                        PlaceObject(tile, objectToPlace);
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
    }


    private string GetPlacementErrorMessage(Tile tile)
    {
        switch (tile.tileType)
        {
            case TileType.Mountain:
                return objectTypeToPlace == "Tunnel" 
                    ? "You can only build tunnels on mountains." 
                    : "You can't build here: only tunnels are allowed on mountains.";

            case TileType.Water:
                return objectTypeToPlace == "Bridge" 
                    ? "You can only build bridges on water." 
                    : "You can't build here: only bridges are allowed on water.";

            case TileType.Station:
            case TileType.UpgradedStation:
                return "You can't build here: there's a station.";

            case TileType.City:
                return "You can't build on a city !";
            
            case TileType.Grass:
            case TileType.Desert:
                if (objectTypeToPlace == "Station" && !tile.CanPlaceObject("Station"))
                {
                    return "Stations must be placed next to a structure !";
                }
                if (objectTypeToPlace == "Tunnel")
                {
                    return "You can only build tunnels on mountains.";
                }
                if (objectTypeToPlace == "Bridge")
                {
                    return "You can only build bridges on water.";
                }
                break;

            default:
                return "Can't build here.";
                
        }
        if (objectTypeToPlace.StartsWith("Rail"))
        {
            if (tile.isOccupied)
            {
                return "You can't build on another build.";
            }
            else 
            {
                return "Rails need to be connected to another rail or a station!";
            }
        }
        else
        {
            return "Can't build here.";
        }
        
    }

    public GameObject GetRailPrefab(string railType)
    {
        switch (railType)
        {
            case "Rail00": return railPrefab00;
            case "Rail01": return railPrefab01;
            case "Rail02": return railPrefab02;
            case "Rail03": return railPrefab03;
            case "Rail04": return railPrefab04;
            case "Rail05": return railPrefab05;
            default: return null;
        }
    }

    public void PlaceObject(Tile tile, GameObject prefabToPlace)
    {
        GameObject structure = Instantiate(prefabToPlace, tile.transform.position, Quaternion.identity);
        structure.transform.SetParent(tile.transform);
        tile.SetOccupied(true);

        structure.transform.localScale = Vector3.zero;
        structure.transform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { /* Code après l'animation */ });
        structure.transform.DORotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => { /* Code après l'animation */ });

        if (prefabToPlace == stationPrefab)
        {
            tile.SetTileType(TileType.Station);
        }
        else if (prefabToPlace == bridgePrefab)
        {
            tile.SetTileType(TileType.Bridge);
        }
        else if (prefabToPlace == tunnelPrefab)
        {
            tile.SetTileType(TileType.Tunnel);
        }
        else
        {
            
            tile.SetTileType((TileType)System.Enum.Parse(typeof(TileType), objectTypeToPlace));
            TrainStation[] trainStations = GameObject.FindObjectsByType<TrainStation>(FindObjectsSortMode.None);
            Debug.Log("Nombre de stations = " + trainStations.Length);
        }
    }

}
