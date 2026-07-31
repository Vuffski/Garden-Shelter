using UnityEngine;

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;
    public SpriteRenderer plantIconRenderer;
    public float tileWorldSize = 1f;

    public string GetLabel()
    {
        char letter = (char)('A' + X);
        return $"{letter}{Y + 1}";
    }

    public void SetPlant(Sprite icon)
    {
        plantIconRenderer.sprite = icon;
        plantIconRenderer.gameObject.SetActive(true);

        float targetSize = tileWorldSize * 0.95f;
        Vector3 spriteSize = icon.bounds.size;
        float scaleX = targetSize / spriteSize.x;
        float scaleY = targetSize / spriteSize.y;
        plantIconRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}