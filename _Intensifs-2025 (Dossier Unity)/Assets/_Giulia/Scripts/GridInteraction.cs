using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridInteraction : MonoBehaviour
{
    public static GridInteraction Instance;
    public string objectTypeToPlace;
    public GameObject stationPrefab;
    public GameObject railPrefab00, railPrefab01, railPrefab02, railPrefab03, railPrefab04, railPrefab05;
    public GameObject bridgePrefab, tunnelPrefab;

    private GameObject objectToPlace;
    private Tile currentTile; 

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {

        if (objectTypeToPlace == "Station")
        {
            objectToPlace = stationPrefab;
        }
        else if (objectTypeToPlace.StartsWith("Rail"))
        {
            objectToPlace = GetRailPrefab(objectTypeToPlace);
        }
        else if (objectTypeToPlace == "Bridge")
        {
            objectToPlace = bridgePrefab;
        }
        else if (objectTypeToPlace == "Tunnel")
        {
            objectToPlace = tunnelPrefab;
        }
        
        if (IsPointerOverUIElement())
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
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
        if (!tile.CanPlaceObject(objectTypeToPlace))
        {
            string reason = GetPlacementErrorMessage(tile);
            TooltipManager.Instance.ShowTooltip(reason);
        }
        else
        {
            TooltipManager.Instance.HideTooltip();
            tile.ShowPlacementUI(objectToPlace);

            if (objectToPlace != null)
            {
                currentTile.DestroyChildrenFromIndex(7);
                PlaceObject(tile, objectToPlace);
            }
        }
    }


    private string GetPlacementErrorMessage(Tile tile)
    {
        switch (tile.tileType)
        {
            case TileType.Mountain:
                return objectTypeToPlace == "Tunnel" ? "You can only place tunnels on mountains." : "Can't build here: only tunnels allowed on mountains.";

            case TileType.Water:
                return objectTypeToPlace == "Bridge" ? "You can only place bridges on water." : "Can't build here: only bridges allowed on water.";

            case TileType.Station:
                return "Can't build here : there's a station.";
            default:
                if (!tile.CanPlaceRail(Instance.objectTypeToPlace))
                {
                    if (tile.isOccupied)
                    {
                        return "You can't build on another build.";
                    }

                    else
                    {
                        return "Rails need to be connected to another rail or a station !";
                    }
                }
                else
                {
                    return "Can't build here.";
                }
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

        if (prefabToPlace == stationPrefab)
        {
            tile.tileType = TileType.Station;
        }
        else if (prefabToPlace == bridgePrefab)
        {
            tile.tileType = TileType.Bridge;
        }
        else if (prefabToPlace == tunnelPrefab)
        {
            tile.tileType = TileType.Tunnel;
        }
        else
        {
            tile.tileType = (TileType)System.Enum.Parse(typeof(TileType), objectTypeToPlace);
        }
    }
    public Tile GetSelectedTile()
    {
        return currentTile;
    }

}
