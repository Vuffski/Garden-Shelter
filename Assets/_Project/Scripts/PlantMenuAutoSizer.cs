using UnityEngine;

public class PlantMenuAutoSizer : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private float maxHeight = 300f;

    private UnityEngine.UI.LayoutElement layoutElement;

    private void Awake()
    {
        layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        }
    }

    private void LateUpdate()
    {
        if (content == null || layoutElement == null)
        {
            return;
        }

        float desiredHeight = Mathf.Min(content.rect.height, maxHeight);
        if (!Mathf.Approximately(layoutElement.preferredHeight, desiredHeight))
        {
            layoutElement.preferredHeight = desiredHeight;
        }
    }
}
