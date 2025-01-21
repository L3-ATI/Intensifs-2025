using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ShovelManager : MonoBehaviour
{
    public Button shovelButton;
    public RectTransform shovelIcon;
    public Material highlightBaseMaterial; 
    public Material highlightGrassMaterial;
    public Material highlightWaterMaterial;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject mountainPrefab;

    public static bool isShovelActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
    private Material originalMaterial = null;

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
            // Met à jour la position de l'icône de la pelle
            Vector2 mousePosition = Input.mousePosition;
            shovelIcon.position = mousePosition;

            // Raycast pour détecter les tuiles sous la souris
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && tile != selectedTile) // Assure que la tuile survolée est différente de la sélectionnée
                {
                    // Appel de TrySelectTile pour sélectionner la tuile lorsqu'elle est survolée
                    TrySelectTile(tile);
                }
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
    
    private void TrySelectTile(Tile tile)
    {
        if (tile == null) return;

        // Vérifications pour s'assurer que la tuile peut être modifiée
        if (tile.tileType == TileType.Mountain)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy a mountain.");
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
        if (tile.tileType == TileType.Grass)
        {
            TooltipManager.Instance.ShowTooltip("You can't destroy an empty tile.");
            return;
        }
        else
        {
            // Si la tuile est modifiable, montre le panneau de confirmation
            confirmationPanel.SetActive(true);
            SelectTile(tile);
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

            if (selectedTile.tileType == TileType.Water)
            {
                tileRenderer.material = highlightWaterMaterial;
            }
            else if (selectedTile.tileType == TileType.StoneQuarry)
            {
                tileRenderer.material = highlightBaseMaterial;
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
                if (childTransform.name != "Vegetation")
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
                    break;
            }

            // Désactive la pelle et le panneau de confirmation
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
