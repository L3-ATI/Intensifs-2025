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
    public GameObject upgradePrefab;
    public ParticleSystem upgradeEffect;

    public static bool isUpgradeActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
    private Material originalMaterial = null;
    private Tile lastHoveredTile = null;
    
    private bool isTileLocked = false;

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
        if (isUpgradeActive && !isTileLocked)
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

            if (tileRenderer.material.ToString() == "MA_Desert" || tileRenderer.material == upgradeWaterMaterial)
            {
                tileRenderer.material = upgradeWaterMaterialHOV;
            }
            else if (tileRenderer.material.ToString() == "MA_Stone" || tileRenderer.material == upgradeStoneMaterial)
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
            isTileLocked = false;
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
        if (tile == null || IsPointerOverUIElement() || isTileLocked) return;

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

                if (tileRenderer.material.ToString() == "MA_Desert" || tileRenderer.material == upgradeWaterMaterialHOV)
                {
                    tileRenderer.material = upgradeWaterMaterial;
                }
                else if (tileRenderer.material.ToString() == "MA_Stone" || tileRenderer.material == upgradeStoneMaterialHOV)
                {
                    tileRenderer.material = upgradeStoneMaterial;
                }
                else
                {
                    tileRenderer.material = upgradeGrassMaterial;
                }
            }

            selectedTile = tile;
        }
    }

    private void OnConfirm()
    {
        if (selectedTile == null) return;

        ReplaceTileWithUpgrade(selectedTile);
        ToggleUpgrade();
        confirmationPanel.SetActive(false);
        isTileLocked = false;
    }

    private void ReplaceTileWithUpgrade(Tile tile)
    {
        if (upgradePrefab != null)
        {
            // Jouer l'effet de particules si présent
            if (upgradeEffect != null)
            {
                ParticleSystem effect = Instantiate(upgradeEffect, tile.transform);
                effect.transform.localPosition = Vector3.zero;
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            Quaternion originalRotation = Quaternion.identity;

            // Vérifier s'il y a un enfant à l'index 7 (ancien objet à remplacer)
            if (tile.transform.childCount >= 7)
            {
                Transform oldStation = tile.transform.GetChild(8);
                originalRotation = oldStation.rotation;

                // Animation de disparition (rotation + réduction de l'échelle)
                oldStation.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
                oldStation.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        Destroy(oldStation.gameObject); // Supprimer l'ancien objet une fois l'animation terminée
                    });
            }

            // Instancier le nouvel objet avec une animation d'apparition (rotation + agrandissement)
            GameObject newObject = Instantiate(upgradePrefab, tile.transform.position, originalRotation, tile.transform);
            newObject.transform.localScale = Vector3.zero; // Initialement à échelle 0
            newObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // Animation de croissance
            newObject.transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutBack); // Animation de rotation

            tile.tileType = TileType.UpgradedStation;
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
