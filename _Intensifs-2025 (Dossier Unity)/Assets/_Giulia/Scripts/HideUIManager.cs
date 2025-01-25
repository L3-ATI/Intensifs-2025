using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HideUIManager : MonoBehaviour
{
    [System.Serializable]
    public class SlidingElement
    {
        public RectTransform uiElement; // L'élément UI à déplacer
        public Vector2 slideOffset;     // Direction et distance du slide
    }

    public Button toggleButton; // Bouton pour activer/désactiver le slide
    public List<SlidingElement> slidingElements = new List<SlidingElement>();
    public float slideDuration = 0.5f; // Durée du slide
    public Ease slideEase = Ease.OutQuad; // Animation d'entrée et de sortie

    private bool isSlidOut = false; // État actuel des éléments (true = slid out)

    private Dictionary<RectTransform, Vector2> initialPositions = new Dictionary<RectTransform, Vector2>();

    private void Start()
    {
        // Stocke les positions initiales des éléments
        foreach (var element in slidingElements)
        {
            if (element.uiElement != null)
            {
                initialPositions[element.uiElement] = element.uiElement.anchoredPosition;
            }
        }

        // Ajoute un listener pour le bouton
        toggleButton.onClick.AddListener(ToggleSlide);
    }

    private void ToggleSlide()
    {
        foreach (var element in slidingElements)
        {
            if (element.uiElement == null) continue;

            RectTransform uiElement = element.uiElement;
            Vector2 targetPosition;

            if (isSlidOut)
            {
                // Retourner à la position initiale
                targetPosition = initialPositions[uiElement];
            }
            else
            {
                // Calculer la position cible
                targetPosition = initialPositions[uiElement] + element.slideOffset;
            }

            // Animer le déplacement
            uiElement.DOAnchorPos(targetPosition, slideDuration).SetEase(slideEase);
        }

        // Inverser l'état
        isSlidOut = !isSlidOut;
    }
}
