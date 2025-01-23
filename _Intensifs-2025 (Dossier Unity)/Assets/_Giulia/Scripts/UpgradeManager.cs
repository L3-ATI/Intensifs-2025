using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UpgradeManager : MonoBehaviour
{
    public Button upgradeButton;
    public RectTransform upgradeIcon;
    public Material upgradeStoneMaterial; 
    public Material upgradeGrassMaterial;
    public Material upgradeWaterMaterial;
    public Material upgradeStoneMaterialHOV;
    public Material upgradeGrassMaterialHOV;
    public Material upgradeWaterMaterialHOV;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject upgradePrefab; // L'objet qui remplacera l'ancien
    public ParticleSystem upgradeEffect; // Effet de particules

    public static bool isUpgradeActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
    private Material originalMaterial = null;
    private Tile lastHoveredTile = null;

    private void Start()
    {
        upgradeButton.onClick.AddListener(ToggleUpgrade);
        gridInteractionScript = FindFirstObjectByType<GridInteraction>();
        confirmationPanel.SetActive(false);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    private void Update()
    {
        if (isUpgradeActive)
        {
            Vector2 mousePosition = Input.mousePosition;
            upgradeIcon.position = mousePosition;

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
                tileRenderer.material = upgradeWaterMaterialHOV;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone")
            {
                tileRenderer.material = upgradeStoneMaterialHOV;
            }
            else
            {
                tileRenderer.material = upgradeGrassMaterialHOV;
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


    private void ToggleUpgrade()
    {
        if (isUpgradeActive)
        {
            if (selectedTile != null)
            {
                Renderer selectedRenderer = selectedTile.GetComponent<Renderer>();
                if (selectedRenderer != null)
                {
                    selectedRenderer.material = originalMaterial;
                }
            }

            isUpgradeActive = false;
            upgradeIcon.gameObject.SetActive(false);

            SetTilesEnabled(true);
            SetGridInteractionEnabled(true);

            Tile.isUpgradeActive = false;
        }
        else
        {
            isUpgradeActive = true;
            upgradeIcon.gameObject.SetActive(true);

            SetTilesEnabled(false);
            SetGridInteractionEnabled(false);

            Tile.isUpgradeActive = true;
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

        if (tile.tileType == TileType.UpgradedStation)
        {
            TooltipManager.Instance.ShowTooltip("This station is already upgraded !");
            return;
        }

        if (tile.tileType != TileType.Station)
        {
            TooltipManager.Instance.ShowTooltip("You can only upgrade stations.");
            return;
        }

        confirmationPanel.SetActive(true);
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
                tileRenderer.material = upgradeWaterMaterial;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone")
            {
                tileRenderer.material = upgradeStoneMaterial;
            }
            else
            {
                tileRenderer.material = upgradeGrassMaterial;
            }
        }
    }

    private void OnConfirm()
    {
        if (selectedTile == null) return;

        // Remplacement de l'objet et déclenchement de l'effet de particules
        ReplaceTileWithUpgrade(selectedTile);
        ToggleUpgrade();
        confirmationPanel.SetActive(false);
    }

    private void ReplaceTileWithUpgrade(Tile tile)
    {
        // Vérifiez si une amélioration est possible
        if (upgradePrefab != null)
        {
            Quaternion originalRotation = Quaternion.identity; // Initialisation de la rotation

            // Si la tuile a des enfants (par exemple, une gare existante), sauvegardez leur rotation
            if (tile.transform.childCount > 0)
            {
                Transform oldStation = tile.transform.GetChild(0);
                originalRotation = oldStation.rotation;
            }

            // Supprimez les anciens objets de la tuile
            foreach (Transform child in tile.transform)
            {
                Destroy(child.gameObject);
            }

            // Instanciez le nouvel objet avec la même rotation
            GameObject newObject = Instantiate(upgradePrefab, tile.transform.position, originalRotation);
            newObject.transform.SetParent(tile.transform);

            // Déclenchez l'effet de particules
            if (upgradeEffect != null)
            {
                ParticleSystem effect = Instantiate(upgradeEffect, tile.transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            // Mettez à jour le type de la tuile
            tile.tileType = TileType.UpgradedStation; // Nouveau type pour représenter une station améliorée
        }
    }


    private void OnCancel()
    {
        if (selectedTile != null)
        {
            Renderer selectedRenderer = selectedTile.GetComponent<Renderer>();
            if (selectedRenderer != null)
            {
                selectedRenderer.material = originalMaterial;
            }
        }

        ToggleUpgrade();
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
