using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Camera uiCamera;

    [SerializeField] private Vector2 positionOffset = new Vector2(20f, 20f);

    private Canvas parentCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (tooltipPanel != null)
        {
            parentCanvas = tooltipPanel.GetComponentInParent<Canvas>();
        }
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.gameObject.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void Show(string content)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(true);
        }

        if (contentText != null)
        {
            contentText.text = content;
        }

        if (tooltipPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
            UpdatePosition(); // Position immediately to avoid 1-frame visual jump
        }
    }

    public void Hide()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    private void UpdatePosition()
    {
        if (Mouse.current == null || tooltipPanel == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Find the canvas to convert from screen space
        if (parentCanvas == null)
        {
            parentCanvas = tooltipPanel.GetComponentInParent<Canvas>();
        }

        RectTransform parentRect = tooltipPanel.parent as RectTransform;
        if (parentRect == null) return;

        Camera cam = null;
        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = uiCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, cam, out Vector2 localPoint))
        {
            Vector2 targetPosition = localPoint + positionOffset;

            Rect parentRectBounds = parentRect.rect;
            float panelWidth = tooltipPanel.rect.width;
            float panelHeight = tooltipPanel.rect.height;

            float clampedX = Mathf.Clamp(targetPosition.x, parentRectBounds.xMin, parentRectBounds.xMax - panelWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, parentRectBounds.yMin, parentRectBounds.yMax - panelHeight);

            tooltipPanel.anchoredPosition = new Vector2(clampedX, clampedY);
        }
    }
}
