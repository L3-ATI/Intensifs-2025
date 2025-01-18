using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button rail00Button, rail01Button, rail02Button, rail03Button, rail04Button, rail05Button;
    public Button bridgeButton, tunnelButton;
    public Button shovelButton;

    public RectTransform shovelIcon;
    public Material constructionMaterial; // Matériau temporaire de "construction"

    private bool isShovelActive = false;
    private Tile selectedTileForDestruction = null;
    private Tile previousTileHovered = null;
    private GameObject confirmationUI;

    private Material[] originalMaterials; // Sauvegarde des matériaux originaux de la tuile ciblée

    private void Start()
    {
        stationButton.onClick.AddListener(() => SetObjectType("Station"));
        rail00Button.onClick.AddListener(() => SetObjectType("Rail00"));
        rail01Button.onClick.AddListener(() => SetObjectType("Rail01"));
        rail02Button.onClick.AddListener(() => SetObjectType("Rail02"));
        rail03Button.onClick.AddListener(() => SetObjectType("Rail03"));
        rail04Button.onClick.AddListener(() => SetObjectType("Rail04"));
        rail05Button.onClick.AddListener(() => SetObjectType("Rail05"));
        bridgeButton.onClick.AddListener(() => SetObjectType("Bridge"));
        tunnelButton.onClick.AddListener(() => SetObjectType("Tunnel"));

        shovelButton.onClick.AddListener(ToggleShovel);

        confirmationUI = GameObject.Find("ConfirmationUI");
        confirmationUI.SetActive(false);
    }

    private void Update()
    {
        if (isShovelActive)
        {
            Vector2 mousePosition = Input.mousePosition;
            shovelIcon.position = mousePosition;

            Tile hoveredTile = GridInteraction.Instance.GetSelectedTile();

            HandleTileHover(hoveredTile);

            if (Input.GetMouseButtonDown(0))
            {
                HandleTileClick();
            }
        }
    }

    private void ToggleShovel()
    {
        isShovelActive = !isShovelActive;
        shovelIcon.gameObject.SetActive(isShovelActive);

        if (isShovelActive)
        {
            TooltipManager.Instance.ShowTooltip("Click on a tile to destroy it.");
            Debug.Log("Click on a tile to destroy it.");
            SetAllTilesEnabled(false);
        }
        else
        {
            Debug.Log("The shovel is now inactive.");
            RestoreOriginalMaterialsToPreviousTile();
            SetAllTilesEnabled(true);
        }
    }

    private void RestoreOriginalMaterialsToPreviousTile()
    {
        if (previousTileHovered != null)
        {
            Renderer renderer = previousTileHovered.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
            {
                renderer.materials = originalMaterials;
            }
            previousTileHovered = null;
        }
    }

    private void HandleTileHover(Tile hoveredTile)
    {
        // Si la tuile survolée change
        if (hoveredTile != previousTileHovered)
        {
            // Restauration des matériaux de la tuile précédemment survolée
            RestoreOriginalMaterialsToPreviousTile();

            // Appliquer le matériau temporaire sur la nouvelle tuile survolée
            if (hoveredTile != null && hoveredTile != selectedTileForDestruction)
            {
                Renderer renderer = hoveredTile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials = renderer.materials;
                    renderer.materials = new Material[] { constructionMaterial };
                }
            }

            // Mise à jour de la référence à la tuile survolée
            previousTileHovered = hoveredTile;
        }
    }

    private void HandleTileClick()
{
    if (previousTileHovered != null)
    {
        // Si la tuile survolée est valide et n'est pas une montagne ou déjà vide
        if (previousTileHovered.isOccupied && previousTileHovered.tileType != TileType.Mountain)
        {
            // Si une tuile a été précédemment sélectionnée pour destruction
            if (selectedTileForDestruction != null)
            {
                // Si ce n'est pas la même tuile qui était déjà sélectionnée
                if (selectedTileForDestruction != previousTileHovered)
                {
                    // Restaurer les matériaux de la tuile précédemment sélectionnée
                    RestoreOriginalMaterialsToTile(selectedTileForDestruction);
                }
            }

            // Marquer la nouvelle tuile comme celle à détruire
            selectedTileForDestruction = previousTileHovered;

            // Appliquer le matériau de construction à la nouvelle tuile sélectionnée
            Renderer renderer = selectedTileForDestruction.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials = renderer.materials;
                renderer.materials = new Material[] { constructionMaterial };
            }

            // Afficher l'UI de confirmation de destruction
            ShowConfirmationUI();

            Debug.Log("Nouvelle tuile sélectionnée pour destruction : " + selectedTileForDestruction.name);
        }
        else
        {
            TooltipManager.Instance.ShowTooltip("Vous ne pouvez pas détruire une tuile vide !");
            Debug.Log("Vous ne pouvez pas détruire une tuile vide !");
        }
    }
}


    private void ShowConfirmationUI()
    {
        confirmationUI.SetActive(true);
    }

    public void ConfirmDestruction()
    {
        if (selectedTileForDestruction != null)
        {
            selectedTileForDestruction.DestroyChildrenFromIndex(7);

            Debug.Log("Destruction confirmed for tile: " + selectedTileForDestruction.name);

            confirmationUI.SetActive(false);
            isShovelActive = false;
            shovelIcon.gameObject.SetActive(false);

            selectedTileForDestruction = null;
            RestoreOriginalMaterialsToPreviousTile();
            SetAllTilesEnabled(true);
            
        }
    }

    public void CancelDestruction()
    {
        if (selectedTileForDestruction != null)
        {
            Renderer renderer = selectedTileForDestruction.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
            {
                renderer.materials = originalMaterials;
            }

            selectedTileForDestruction = null;
        }

        confirmationUI.SetActive(false);
        isShovelActive = false;
        shovelIcon.gameObject.SetActive(false);
        Debug.Log("Destruction canceled.");
        SetAllTilesEnabled(true);
    }

    private void SetObjectType(string objectType)
    {
        GridInteraction.Instance.objectTypeToPlace = objectType;
    }

    private void SetAllTilesEnabled(bool isEnabled)
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        foreach (Tile tile in allTiles)
        {
            tile.enabled = isEnabled;
        }
    }
    private void RestoreOriginalMaterialsToTile(Tile tile)
    {
        if (tile != null)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
            {
                renderer.materials = originalMaterials;
            }
        }
    }

}