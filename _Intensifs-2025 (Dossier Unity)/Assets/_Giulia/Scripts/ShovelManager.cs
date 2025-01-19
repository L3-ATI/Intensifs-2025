using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ShovelManager : MonoBehaviour
{
    public Button shovelButton;
    public RectTransform shovelIcon;
    public Material highlightMaterial; 
    public Material highlightGrassMaterial;
    public Material highlightWaterMaterial;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject mountainPrefab;

    private bool isShovelActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
    private Material originalMaterial = null;
    private TooltipManager tooltipManager;

    private void Start()
    {
        shovelButton.onClick.AddListener(ToggleShovel);
        gridInteractionScript = FindFirstObjectByType<GridInteraction>();
        tooltipManager = FindFirstObjectByType<TooltipManager>();
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
            
            if (Input.GetMouseButtonDown(0))
            {
                TrySelectTile();
            }
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
    
    private void TrySelectTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                if (tile.tileType != TileType.Mountain)
                {
                    confirmationPanel.SetActive(true);
                }
                else
                {
                    if (tile.tileType == TileType.Mountain)
                        tooltipManager.ShowTooltip("You can't destroy a mountain.");
                    else
                        tooltipManager.ShowTooltip("You can't destroy an empty tile.");
                }

                SelectTile(tile);
            }
        }
    }

    private void SelectTile(Tile tile)
    {
        if (IsPointerOverUIElement())
        {
            return;
        }
        
        if (selectedTile != null)
        {
            Renderer selectedRenderer = selectedTile.GetComponent<Renderer>();
            if (selectedRenderer != null)
            {
                selectedRenderer.material = originalMaterial;
            }
        }

        selectedTile = tile;
        Renderer tileRenderer = selectedTile.GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;

            // Applique le bon matériau en fonction du type de tuile
            if (selectedTile.tileType == TileType.Water)
            {
                tileRenderer.material = highlightWaterMaterial;  // Matériel pour Water
            }
            else if (selectedTile.tileType == TileType.Grass)
            {
                tileRenderer.material = highlightGrassMaterial;  // Matériel pour Grass
            }
            else
            {
                tileRenderer.material = highlightMaterial;  // Utilisez le matériau par défaut si ce n'est ni Water ni Grass
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedTile != null)
        {
            Debug.Log("Selected tile type: " + selectedTile.tileType);

            if (selectedTile.tileType != TileType.Mountain)
            {
                for (int i = 7; i < selectedTile.transform.childCount; i++)
                {
                    GameObject childObject = selectedTile.transform.GetChild(i).gameObject;

                    childObject.transform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(childObject));
                    
                    if (selectedTile.tileType == TileType.Tunnel)
                    {
                        selectedTile.isOccupied = false;
                        GameObject newMountain = Instantiate(mountainPrefab, selectedTile.transform.position, Quaternion.identity);
                        newMountain.transform.SetParent(selectedTile.transform);
                        selectedTile.tileType = TileType.Mountain;
                        Debug.Log("Destroyed tunnel, replaced with mountain.");

                        ToggleShovel();
                        confirmationPanel.SetActive(false);
                        return;
                    }
                    else if (selectedTile.tileType == TileType.Bridge)
                    {
                        selectedTile.isOccupied = false;
                        selectedTile.tileType = TileType.Water;

                        ToggleShovel();
                        confirmationPanel.SetActive(false);
                        return;
                    }
                    else
                    {
                        selectedTile.isOccupied = false;
                        selectedTile.tileType = TileType.Grass;

                        ToggleShovel();
                        confirmationPanel.SetActive(false);
                        return;
                    }
                }
            }
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
