using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    public GameObject tooltipPanel;
    public TMP_Text tooltipText;
    public float defaultDuration = 2f;
    public Vector2 tooltipOffset = new Vector2(10f, 10f);

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;

            RectTransform canvasRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePosition, null, out Vector2 localPoint);

            localPoint += tooltipOffset;

            tooltipPanel.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        if (Input.GetMouseButtonDown(1) && tooltipPanel.activeSelf)
        {
            HideTooltip();
        }
    }

    public void ShowTooltip(string message)
    {
        ShowTooltip(message, defaultDuration);
    }

    public void ShowTooltip(string message, float duration)
    {
        tooltipText.text = message;
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());

        tooltipPanel.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTooltip();
    }
}
