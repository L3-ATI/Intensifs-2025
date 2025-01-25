using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Button stationButton;
    public Button rail00Button, rail01Button, rail02Button, rail03Button, rail04Button, rail05Button;
    public Button bridgeButton, tunnelButton, shovelButton, upgradeButton;

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
    public float hoverSlideDuration = 0.2f;

    [Header("Hover Offsets")]
    public Vector2 defaultHoverSlideOffset = new Vector2(-80f, 0f);
    public Dictionary<Button, Vector2> buttonHoverOffsets = new Dictionary<Button, Vector2>();

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
        shovelButton.onClick.AddListener(() => ToggleSelection(shovelButton, "Shovel"));
        upgradeButton.onClick.AddListener(() => ToggleSelection(upgradeButton, "Upgrade"));

        buttonHoverOffsets[stationButton] = new Vector2(0f, -80f);
        buttonHoverOffsets[rail00Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[rail01Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[rail02Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[rail03Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[rail04Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[rail05Button] = defaultHoverSlideOffset;
        buttonHoverOffsets[bridgeButton] = new Vector2(0f, 80f);
        buttonHoverOffsets[tunnelButton] = new Vector2(0f, 80f);

        AddHoverEffect(stationButton);
        AddHoverEffect(rail00Button);
        AddHoverEffect(rail01Button);
        AddHoverEffect(rail02Button);
        AddHoverEffect(rail03Button);
        AddHoverEffect(rail04Button);
        AddHoverEffect(rail05Button);
        AddHoverEffect(bridgeButton);
        AddHoverEffect(tunnelButton);
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
            if (button != shovelButton && button != upgradeButton)
            {
                buttonImage.DOColor(targetColor, animationDuration);
            }
            buttonImage.DOFade(targetOpacity, animationDuration);
        }
    }

    private void AddHoverEffect(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        Vector2 initialPosition = rectTransform.anchoredPosition;

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onEnter.callback.AddListener((_) =>
        {
            if (currentlySelectedButton != button)
            {
                Vector2 offset = buttonHoverOffsets.ContainsKey(button) ? buttonHoverOffsets[button] : defaultHoverSlideOffset;
                rectTransform.DOAnchorPos(initialPosition + offset, hoverSlideDuration).SetEase(Ease.OutQuad);
            }
        });
        trigger.triggers.Add(onEnter);

        EventTrigger.Entry onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onExit.callback.AddListener((_) =>
        {
            if (currentlySelectedButton != button)
            {
                rectTransform.DOAnchorPos(initialPosition, hoverSlideDuration).SetEase(Ease.OutQuad);
            }
        });
        trigger.triggers.Add(onExit);

        button.onClick.AddListener(() =>
        {
            Vector2 offset = buttonHoverOffsets.ContainsKey(button) ? buttonHoverOffsets[button] : defaultHoverSlideOffset;
            rectTransform.anchoredPosition = initialPosition + offset;
        });

        button.onClick.AddListener(() =>
        {
            if (currentlySelectedButton == button)
            {
                rectTransform.DOAnchorPos(initialPosition, animationDuration).SetEase(Ease.OutQuad);
            }
        });
    }

}
