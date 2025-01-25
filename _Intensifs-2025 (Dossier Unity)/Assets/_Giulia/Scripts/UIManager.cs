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

    public Button currentlySelectedButton = null;
    public static bool isAButtonClicked { get; private set; } = false;

    [Space(20)]

    public Vector3 normalScale = Vector3.one;
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1);
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public float normalOpacity = 1f;
    public float selectedOpacity = 0.6f;
    public float animationDuration = 0.3f;

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
            DeselectButton();
        }
        else
        {
            SelectButton(button, objectType);
        }
    }

    private void SelectButton(Button button, string objectType)
    {
        if (currentlySelectedButton != null)
        {
            DeselectButton();
        }

        currentlySelectedButton = button;
        isAButtonClicked = true;

        GridInteraction.Instance.objectTypeToPlace = objectType;

        AnimateButton(button, selectedScale, selectedOpacity, selectedColor);
    }

    private void DeselectButton()
    {
        if (currentlySelectedButton != null)
        {
            AnimateButton(currentlySelectedButton, normalScale, normalOpacity, normalColor);
            currentlySelectedButton = null;
        }

        isAButtonClicked = false;

        GridInteraction.Instance.objectTypeToPlace = null;
    }

    private void AnimateButton(Button button, Vector3 targetScale, float targetOpacity, Color targetColor)
    {
        button.transform.DOScale(targetScale, animationDuration);

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.DOColor(targetColor, animationDuration);
            buttonImage.DOFade(targetOpacity, animationDuration);
        }
    }
}
