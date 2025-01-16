using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button rail00Button, rail01Button, rail02Button, rail03Button, rail04Button, rail05Button;

    private void Start()
    {
        // Assurez-vous que les boutons sont assignés dans l'inspecteur
        stationButton.onClick.AddListener(() => SetObjectType("Station"));
        rail00Button.onClick.AddListener(() => SetObjectType("Rail00"));
        rail01Button.onClick.AddListener(() => SetObjectType("Rail01"));
        rail02Button.onClick.AddListener(() => SetObjectType("Rail02"));
        rail03Button.onClick.AddListener(() => SetObjectType("Rail03"));
        rail04Button.onClick.AddListener(() => SetObjectType("Rail04"));
        rail05Button.onClick.AddListener(() => SetObjectType("Rail05"));
    }

    private void SetObjectType(string objectType)
    {
        GridInteraction.Instance.objectTypeToPlace = objectType;
    }
}