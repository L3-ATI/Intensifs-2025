using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button rail00Button, rail01Button, rail02Button, rail03Button, rail04Button, rail05Button;
    public Button bridgeButton, tunnelButton;
    
    public Button shovelButton;

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
        
        shovelButton.onClick.AddListener(DestroyChildrenOnSelectedTile);
    }

    private void SetObjectType(string objectType)
    {
        GridInteraction.Instance.objectTypeToPlace = objectType;
    }
    
    private void DestroyChildrenOnSelectedTile()
    {
        // Vérifiez si une tuile est sélectionnée via GridInteraction (ou une autre logique de sélection)
        Tile selectedTile = GridInteraction.Instance.GetSelectedTile();

        if (selectedTile != null)
        {
            int startIndex = 7; // Indice à partir duquel détruire les enfants
            selectedTile.DestroyChildrenFromIndex(startIndex);
        }
        else
        {
            Debug.LogWarning("Aucune tuile sélectionnée pour la destruction.");
        }
    }
}