using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button rail00Button, rail01Button, rail02Button, rail03Button, rail04Button, rail05Button;
    public Button bridgeButton, tunnelButton;

    [Space(20)]
    
    public Button currentlySelectedButton = null; // Bouton actuellement sélectionné
    public static bool isAButtonClicked { get; private set; } = false; // Booléen pour savoir si un bouton est cliqué

    [Space(20)]

    public Vector3 normalScale = Vector3.one; // Échelle par défaut des boutons
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1); // Échelle des boutons sélectionnés
    public float normalOpacity = 1f; // Opacité par défaut
    public float selectedOpacity = 0.6f; // Opacité des boutons sélectionnés
    public float animationDuration = 0.3f; // Durée des animations DOTween

    private void Start()
    {
        stationButton.onClick.AddListener(() => ToggleSelection(stationButton, "Station"));
        rail00Button.onClick.AddListener(() => ToggleSelection(rail00Button, "Rail00"));
        rail01Button.onClick.AddListener(() => ToggleSelection(rail01Button, "Rail01"));
        rail02Button.onClick.AddListener(() => ToggleSelection(rail02Button, "Rail02"));
        rail03Button.onClick.AddListener(() => ToggleSelection(rail03Button, "Rail03"));
        rail04Button.onClick.AddListener(() => ToggleSelection(rail04Button, "Rail04"));
        rail05Button.onClick.AddListener(() => ToggleSelection(rail05Button, "Rail05"));
        bridgeButton.onClick.AddListener(() => ToggleSelection(bridgeButton, "Bridge"));
        tunnelButton.onClick.AddListener(() => ToggleSelection(tunnelButton, "Tunnel"));
    }

    private void ToggleSelection(Button button, string objectType)
    {
        if (currentlySelectedButton == button)
        {
            // Si le bouton cliqué est déjà sélectionné, désélectionnez-le
            DeselectButton();
        }
        else
        {
            // Sélectionner un nouveau bouton
            SelectButton(button, objectType);
        }
    }

    private void SelectButton(Button button, string objectType)
    {
        // Déselectionner le bouton précédent s'il existe
        if (currentlySelectedButton != null)
        {
            DeselectButton();
        }

        // Sélectionner le nouveau bouton
        currentlySelectedButton = button;
        isAButtonClicked = true;

        // Mettre à jour le type d'objet dans GridInteraction
        GridInteraction.Instance.objectTypeToPlace = objectType;

        // Ajouter l'animation pour indiquer la sélection
        AnimateButton(button, selectedScale, selectedOpacity);
    }

    private void DeselectButton()
    {
        // Déselectionner le bouton actuellement sélectionné
        if (currentlySelectedButton != null)
        {
            AnimateButton(currentlySelectedButton, normalScale, normalOpacity);
            currentlySelectedButton = null;
        }

        // Réinitialiser le booléen
        isAButtonClicked = false;

        // Optionnel : Réinitialiser le type d'objet dans GridInteraction
        GridInteraction.Instance.objectTypeToPlace = null;
    }

    private void AnimateButton(Button button, Vector3 targetScale, float targetOpacity)
    {
        // Animation de l'échelle
        button.transform.DOScale(targetScale, animationDuration);

        // Animation de l'opacité
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            buttonImage.DOFade(targetOpacity, animationDuration);
        }
    }
}
