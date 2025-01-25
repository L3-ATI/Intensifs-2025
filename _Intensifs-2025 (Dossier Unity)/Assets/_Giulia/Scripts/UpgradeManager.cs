using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class UpgradeManager : MonoBehaviour
{
    public Button upgradeButton;
    public RectTransform upgradeIcon;
    public Material upgradeMaterial;
    public Material upgradeSelMaterial;
    public GameObject confirmationPanel;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject upgradePrefab;
    public ParticleSystem upgradeEffect;

    public static bool isUpgradeActive = false;
    private GridInteraction gridInteractionScript;
    private Tile selectedTile = null;
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
            
            else if (lastHoveredTile != null)
            {
                ResetTileMaterial(lastHoveredTile);
                lastHoveredTile = null;
            }

            if (Input.GetMouseButtonDown(0) && lastHoveredTile != null)
            {
                TrySelectTile(lastHoveredTile);
            }
        }
    }
    
    private void HandleHoverTile(Tile tile)
    {
        Renderer tileRenderer = tile.GetComponent<Renderer>();
        if (tileRenderer != null)
        {

            Material[] materials = tileRenderer.materials;
            if (materials.Length > 1)
            {
                materials[1] = upgradeMaterial;
            }
            else
            {
                Array.Resize(ref materials, 2);
                materials[1] = upgradeMaterial;
            }
            
            tileRenderer.materials = materials;
        }
    }
    
    private void ResetTileMaterial(Tile tile)
    {
        if (tile != null)
        {
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                Material[] originalOnly = new Material[] { tileRenderer.materials[0] };
                tileRenderer.materials = originalOnly;
            }
        }
    }
    
    private void ToggleUpgrade()
    {
        if (isUpgradeActive)
        {
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
                List<Material> materials = new List<Material>(tileRenderer.materials);

                if (!materials.Contains(upgradeSelMaterial))
                {
                    materials.Add(upgradeSelMaterial);
                }

                tileRenderer.materials = materials.ToArray();

                selectedTile = tile;
            }
        }
    }
    
    private void OnConfirm()
    {
        if (selectedTile != null)
        {
            Renderer selectedRenderer = selectedTile.GetComponent<Renderer>();
            if (selectedRenderer != null)
            {
                List<Material> materials = new List<Material>(selectedRenderer.materials);
                if (materials.Contains(upgradeSelMaterial))
                {
                    materials.Remove(upgradeSelMaterial);
                }
                selectedRenderer.materials = materials.ToArray();
            }
        }

        ReplaceTileWithUpgrade(selectedTile);
        ToggleUpgrade();
        confirmationPanel.SetActive(false);
        isTileLocked = false;
        ResetTileMaterial(selectedTile);
    }

    private void ReplaceTileWithUpgrade(Tile tile)
    {
        if (upgradePrefab != null)
        {
            if (upgradeEffect != null)
            {
                ParticleSystem effect = Instantiate(upgradeEffect, tile.transform);
                effect.transform.localPosition = Vector3.zero;
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            Quaternion originalRotation = Quaternion.identity;

            if (tile.transform.childCount >= 7)
            {
                Transform oldStation = tile.transform.GetChild(8);
                originalRotation = oldStation.rotation;

                oldStation.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
                oldStation.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        Destroy(oldStation.gameObject);
                    });
            }

            GameObject newObject = Instantiate(upgradePrefab, tile.transform.position, originalRotation, tile.transform);
            newObject.transform.localScale = Vector3.zero;
            newObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            newObject.transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutBack);

            tile.tileType = TileType.UpgradedStation;
        }
    }
    
    private void OnCancel()
    {
        ToggleUpgrade();
        selectedTile = null;
        isTileLocked = false;
        confirmationPanel.SetActive(false);
        ResetTileMaterial(selectedTile);
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