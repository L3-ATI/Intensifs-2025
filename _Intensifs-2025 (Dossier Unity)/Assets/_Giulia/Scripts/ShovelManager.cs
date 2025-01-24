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
        if (isShovelActive)
        {
            Vector2 mousePosition = Input.mousePosition;
            shovelIcon.position = mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Tile hoveredTile = hit.collider.GetComponent<Tile>();

                // Si on survole une nouvelle tuile
                if (hoveredTile != null && hoveredTile != lastHoveredTile)
                {
                    HandleHoverTile(hoveredTile); // Appliquer le matériau de surbrillance
                }

                // Réinitialiser l'ancienne tuile si elle n'est plus survolée
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

            if (tileRenderer.material.ToString() == "MA_Desert")
            {
                tileRenderer.material = highlightWaterMaterialHOV;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone")
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
        if (tile == null || IsPointerOverUIElement()) return;

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
        else
        {
            confirmationPanel.SetActive(true);
        }
        
        SelectTile(tile);
    }


    private void SelectTile(Tile tile)
    {
        if (IsPointerOverUIElement())
        {
            return;
        }
        
        if (tile != null)
        {
            Renderer selectedRenderer = tile.GetComponent<Renderer>();
            if (selectedRenderer != null)
            {
                selectedRenderer.material = originalMaterial;
            }
        }

        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;

            if (tileRenderer.material.ToString() == "MA_Desert")
            {
                tileRenderer.material = highlightWaterMaterial;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone")
            {
                tileRenderer.material = highlightStoneMaterial;
            }
            else
            {
                tileRenderer.material = highlightGrassMaterial;
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedTile == null)
            return;

        Debug.Log("Selected tile type: " + selectedTile.tileType);

        // Vérifie que la tuile est de type modifiable
        if (selectedTile.tileType != TileType.Mountain &&
            selectedTile.tileType != TileType.Mine &&
            selectedTile.tileType != TileType.Sawmill &&
            selectedTile.tileType != TileType.StoneQuarry &&
            selectedTile.tileType != TileType.Water &&
            selectedTile.tileType != TileType.Grass)
        {
            // Parcourt les enfants de la tuile
            for (int i = 7; i < selectedTile.transform.childCount; i++)
            {
                Transform childTransform = selectedTile.transform.GetChild(i);

                // Vérifie que l'objet n'est pas de la végétation avant de le détruire
                if (childTransform.name != "AddedObjectsManager")
                {
                    childTransform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(childTransform.gameObject));
                }
            }

            // Gestion spécifique des types de tuiles
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

                default:
                    ReplaceTile(TileType.Grass);
                    Debug.Log("Tile reset to grass.");
                    selectedTile.GetComponentInChildren<GrassTile>().gameObject.SetActive(true);
                    break;
            }

            selectedTile.GetComponent<Tile>().UpdateVegetation();
            ToggleShovel();
            confirmationPanel.SetActive(false);
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
        ToggleShovel();
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
