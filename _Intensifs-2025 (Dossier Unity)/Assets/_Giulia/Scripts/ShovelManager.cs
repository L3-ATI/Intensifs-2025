using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ShovelManager : MonoBehaviour
{
    public Button shovelButton;
    public RectTransform shovelIcon;
    public Material highlightStoneMaterial; 
    public Material highlightGrassMaterial;
    public Material highlightWaterMaterial;
    public Material highlightStoneMaterialHOV; 
    public Material highlightGrassMaterialHOV;
    public Material highlightWaterMaterialHOV;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject mountainPrefab;

    public static bool isShovelActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
    private Material originalMaterial = null;
    private Tile lastHoveredTile = null;
    
    private bool isTileLocked = false;

    private void Start()
    {
        shovelButton.onClick.AddListener(ToggleShovel);
        gridInteractionScript = FindFirstObjectByType<GridInteraction>();
        confirmationPanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    private void Update()
    {
        if (isShovelActive && !isTileLocked)
        {
            Vector2 mousePosition = Input.mousePosition;
            shovelIcon.position = mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Tile hoveredTile = hit.collider.GetComponent<Tile>();

                if (hoveredTile != null && hoveredTile != lastHoveredTile)
                {
                    HandleHoverTile(hoveredTile);
                }

                if (lastHoveredTile != null && lastHoveredTile != hoveredTile)
                {
                    ResetTileMaterial(lastHoveredTile);
                }

                lastHoveredTile = hoveredTile;
            }

            if (Input.GetMouseButtonDown(0) && lastHoveredTile != null)
            {
                TrySelectTile(lastHoveredTile);
            }
        }
        else if (lastHoveredTile != null)
        {
            ResetTileMaterial(lastHoveredTile);
            lastHoveredTile = null;
        }
    }

    private void HandleHoverTile(Tile tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;

            if (tileRenderer.material.ToString() == "MA_Desert" || tileRenderer.material == highlightWaterMaterial)
            {
                tileRenderer.material = highlightWaterMaterialHOV;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone" || tileRenderer.material == highlightStoneMaterial)
            {
                tileRenderer.material = highlightStoneMaterialHOV;
            }
            else
            {
                tileRenderer.material = highlightGrassMaterialHOV;
            }
        }
    }

    private void ResetTileMaterial(Tile tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null && originalMaterial != null)
        {
            tileRenderer.material = originalMaterial;
        }
    }

    private void ToggleShovel()
    {
        if (isShovelActive)
        {
            if (selectedTile != null)
            {
                Renderer selectedRenderer = selectedTile.GetComponent<Renderer>();
                if (selectedRenderer != null)
                {
                    selectedRenderer.material = originalMaterial;
                }
            }
            
            isShovelActive = false;
            shovelIcon.gameObject.SetActive(false);

            SetTilesEnabled(true);
            SetGridInteractionEnabled(true);

            Tile.isShovelActive = false;
        }
        else
        {
            isShovelActive = true;
            shovelIcon.gameObject.SetActive(true);

            SetTilesEnabled(false);
            SetGridInteractionEnabled(false);

            Tile.isShovelActive = true;
            isTileLocked = false;
        }
    }

    private void SetTilesEnabled(bool isEnabled)
    {
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile tile in allTiles)
        {
            tile.enabled = isEnabled;
        }
    }
    
    private void SetGridInteractionEnabled(bool isEnabled)
    {
        if (gridInteractionScript != null)
        {
            gridInteractionScript.enabled = isEnabled;
        }
    }
    
    private void TrySelectTile(Tile tile)
    {
        if (tile == null || IsPointerOverUIElement() || isTileLocked) return;

        if (tile.tileType == TileType.Mountain)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a mountain.");
            return;
        }
        if (tile.tileType == TileType.City)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a city.");
            return;
        }
        if (tile.tileType == TileType.Mine)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a mine.");
            return;
        }
        if (tile.tileType == TileType.Sawmill)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a sawmill.");
            return;
        }
        if (tile.tileType == TileType.StoneQuarry)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a stone quarry.");
            return;
        }
        if (tile.tileType == TileType.Water)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy water.");
            return;
        }
        if (tile.tileType == TileType.Grass || tile.tileType == TileType.Desert)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy an empty tile.");
            return;
        }

        confirmationPanel.SetActive(true);
        SelectTile(tile);
        isTileLocked = true;
    }


    private void SelectTile(Tile tile)
    {
        if (IsPointerOverUIElement())
        {
            return;
        }

        if (tile != null)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                originalMaterial = tileRenderer.material;

                if (tileRenderer.material.ToString() == "MA_Desert" || tileRenderer.material == highlightWaterMaterialHOV)
                {
                    tileRenderer.material = highlightWaterMaterial;
                }
                else if (tileRenderer.material.ToString() == "MA_Stone" || tileRenderer.material == highlightStoneMaterialHOV)
                {
                    tileRenderer.material = highlightStoneMaterial;
                }
                else
                {
                    tileRenderer.material = highlightGrassMaterial;
                }
            
            }

            selectedTile = tile;
        }
    }

    private void OnConfirm()
    {
        if (selectedTile == null)
            return;

        Debug.Log("Selected tile type: " + selectedTile.tileType);

        if (selectedTile.tileType != TileType.Mountain &&
            selectedTile.tileType != TileType.Mine &&
            selectedTile.tileType != TileType.Sawmill &&
            selectedTile.tileType != TileType.StoneQuarry &&
            selectedTile.tileType != TileType.Water &&
            selectedTile.tileType != TileType.Grass)
        {
            for (int i = 7; i < selectedTile.transform.childCount; i++)
            {
                Transform childTransform = selectedTile.transform.GetChild(i);

                if (childTransform.name != "AddedObjectsManager")
                {
                    childTransform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(childTransform.gameObject));
                }
            }

            switch (selectedTile.tileType)
            {
                case TileType.Tunnel:
                    ReplaceTile(TileType.Mountain, mountainPrefab);
                    Debug.Log("Destroyed tunnel, replaced with mountain.");
                    break;

                case TileType.Bridge:
                    ReplaceTile(TileType.Water);
                    Debug.Log("Destroyed bridge, replaced with water.");
                    break;
                case TileType.Station:
                case TileType.UpgradedStation:
                    if (selectedTile.GetComponent<Renderer>().material.name == "MA_Desert")
                    {
                        ReplaceTile(TileType.Desert);
                        Debug.Log("Destroyed Station, replaced with desert.");
                    }
                    else
                    {
                        ReplaceTile(TileType.Grass);
                        Debug.Log("Destroyed Station, replaced with grass.");
                    }
                    break;

                default:
                    ReplaceTile(TileType.Grass);
                    Debug.Log("Tile reset to grass.");
                    if (selectedTile.GetComponentInChildren<GrassTile>() != null)
                    {
                        selectedTile.GetComponentInChildren<GrassTile>().gameObject.SetActive(true);
                    }                    
                    break;
            }

            selectedTile.GetComponent<Tile>().UpdateVegetation();
            ToggleShovel();
            confirmationPanel.SetActive(false);
            isTileLocked = false;
        }
    }
    
    private void ReplaceTile(TileType newType, GameObject prefab = null)
    {
        selectedTile.isOccupied = false;
        selectedTile.tileType = newType;

        if (prefab != null)
        {
            GameObject newObject = Instantiate(prefab, selectedTile.transform.position, Quaternion.identity);
            newObject.transform.SetParent(selectedTile.transform);
        }
    }

    private void OnCancel()
    {
        if (selectedTile != null)
        {
            Renderer tileRenderer = selectedTile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileRenderer.material = originalMaterial;
            }
        }

        selectedTile = null;
        isTileLocked = false;
        confirmationPanel.SetActive(false);
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
}
