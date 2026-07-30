using UnityEngine;

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;
    public SpriteRenderer plantIconRenderer;

    public string GetLabel()
    {
        char letter = (char)('A' + X);
        return $"{letter}{Y + 1}";
    }

    public void SetPlant(Sprite icon)
    {
        plantIconRenderer.sprite = icon;
        plantIconRenderer.gameObject.SetActive(true);
    }
}