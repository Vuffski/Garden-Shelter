using UnityEngine;

public class TileView : MonoBehaviour
{
    public int X;
    public int Y;

    public string GetLabel()
    {
        char letter = (char)('A' + X);
        return $"{letter}{Y + 1}";
    }
}