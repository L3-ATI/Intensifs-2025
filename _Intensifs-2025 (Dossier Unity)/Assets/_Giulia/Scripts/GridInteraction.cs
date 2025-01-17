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
        
        if (Input.GetMouseButtonDown(0)) // Clic gauche
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
                currentTile.CancelPlacement(7);  // Par exemple, pour supprimer l'enfant à l'index 7
                ;
            }
        }
    }

    private bool IsPointerOverUIElement()
    {
        // Crée un objet PointerEventData pour utiliser dans le système d'événements UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Liste des résultats du raycast
        List<RaycastResult> results = new List<RaycastResult>();

        // Effectue le raycast
        EventSystem.current.RaycastAll(pointerData, results);

        // Retourne true si des éléments ont été trouvés, ce qui signifie que la souris est au-dessus de l'UI
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
            // Placement valide
            TooltipManager.Instance.HideTooltip(); // Cache le tooltip si visible

            // Si l'objet à placer est une station
            if (objectTypeToPlace == "Station")
            {
                tile.ShowPlacementUI(objectToPlace);  // Utilise objectToPlace ici
            }
            else if (objectTypeToPlace.StartsWith("Rail"))
            {
                // Vérifie si le rail peut être placé
                if (!tile.CanPlaceRail(objectTypeToPlace))
                {
                    TooltipManager.Instance.ShowTooltip("Rails need to be connected to a station!");
                    return;
                }
                tile.ShowPlacementUI(objectToPlace);  // Utilise objectToPlace ici aussi
            }

            // Place l'objet immédiatement après avoir validé
            if (objectToPlace != null)
            {
                PlaceObject(tile, objectToPlace);  // Utilise objectToPlace
            }
        }
    }


    private string GetPlacementErrorMessage(Tile tile)
    {
        switch (tile.tileType)
        {
            case TileType.Mountain:
                return "Can't build here : there's a mountain.";
            case TileType.Water:
                return "Can't build here : there's water.";
            case TileType.Station:
                return "Can't build here : there's a station.";
            default:
                if (!tile.CanPlaceRail(Instance.objectTypeToPlace))
                {
                    return "Rails need to be connected to a station !";
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
        Debug.Log("Placing object...");
        GameObject structure = Instantiate(prefabToPlace, tile.transform.position, Quaternion.identity);
        structure.transform.SetParent(tile.transform);
        tile.SetOccupied(true);

        // Définir la tuile comme une station ou un rail (selon l'objet)
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
}
