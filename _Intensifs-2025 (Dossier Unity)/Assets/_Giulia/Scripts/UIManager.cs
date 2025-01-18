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

    private bool isShovelActive = false;
    private Tile selectedTileForDestruction = null;
    private GameObject confirmationUI;

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
        selectedTileForDestruction = GridInteraction.Instance.GetSelectedTile();
            
        if (isShovelActive)
        {
            Vector2 mousePosition = Input.mousePosition;
            shovelIcon.position = mousePosition;

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
        }
        else
        {
            Debug.Log("The shovel is now inactive.");
        }
    }

    private void HandleTileClick()
    {

        if (selectedTileForDestruction != null)
        {
            if (selectedTileForDestruction.isOccupied && (selectedTileForDestruction.tileType != TileType.Mountain))
            {
                DisableAllTileScripts();
                selectedTileForDestruction.isAwaitingDestruction = true;
                ShowConfirmationUI();
            }
            else
            {
                TooltipManager.Instance.ShowTooltip("You can't destroy an empty tile!");
                Debug.Log("You can't destroy an empty tile!");
            }
        }
        else
        {
            TooltipManager.Instance.ShowTooltip("PAS DE TILE SELECTIONNÉE");
            Debug.Log("PAS DE TILE SELECTIONNÉE");
        }
    }

    private void ShowConfirmationUI()
    {
        selectedTileForDestruction.ApplyHighlight(selectedTileForDestruction.highlightDestroyingMaterial);
        selectedTileForDestruction.RemoveHighlight(selectedTileForDestruction.highlightValidMaterial);
        selectedTileForDestruction.RemoveHighlight(selectedTileForDestruction.highlightInvalidMaterial);

        confirmationUI.SetActive(true);
    }

    private void DisableAllTileScripts()
    {
        // Récupérer toutes les tuiles de la grille et désactiver leur script Tile
        Tile[] allTiles = UnityEngine.Object.FindObjectsByType<Tile>(UnityEngine.FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            tile.enabled = false;
        }
    }

    private void EnableAllTileScripts()
    {
        // Récupérer toutes les tuiles de la grille et réactiver leur script Tile
        Tile[] allTiles = UnityEngine.Object.FindObjectsByType<Tile>(UnityEngine.FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            tile.enabled = true;
        }
    }


    public void ConfirmDestruction()
    {
        if (selectedTileForDestruction != null)
        {
            selectedTileForDestruction.DestroyChildrenFromIndex(7);
            
            selectedTileForDestruction.isAwaitingDestruction = false;
            selectedTileForDestruction.RemoveHighlight(selectedTileForDestruction.highlightDestroyingMaterial);
            confirmationUI.SetActive(false);
            isShovelActive = false;
            shovelIcon.gameObject.SetActive(false);
            Debug.Log("Destruction confirmed.");

            EnableAllTileScripts();
        }
    }

    public void CancelDestruction()
    {
        if (selectedTileForDestruction != null)
        {
            selectedTileForDestruction.isAwaitingDestruction = false;
            selectedTileForDestruction.RemoveHighlight(selectedTileForDestruction.highlightDestroyingMaterial);
        }

        confirmationUI.SetActive(false);
        isShovelActive = false;
        shovelIcon.gameObject.SetActive(false);
        Debug.Log("Destruction canceled.");

        EnableAllTileScripts();
    }

    private void SetObjectType(string objectType)
    {
        GridInteraction.Instance.objectTypeToPlace = objectType;
    }
}
