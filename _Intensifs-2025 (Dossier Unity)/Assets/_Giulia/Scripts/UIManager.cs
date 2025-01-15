using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button railStraightButton;
    public Button railCurvedButton;

    private void Start()
    {
        // Assurez-vous que les boutons sont assignés dans l'inspecteur
        stationButton.onClick.AddListener(() => SetObjectType("Station"));
        railStraightButton.onClick.AddListener(() => SetObjectType("RailStraight"));
        railCurvedButton.onClick.AddListener(() => SetObjectType("RailCurved"));
    }

    private void SetObjectType(string objectType)
    {
        GridInteraction.Instance.objectTypeToPlace = objectType;
    }
}