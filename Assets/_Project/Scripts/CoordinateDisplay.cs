using UnityEngine;
using TMPro;

public class CoordinateDisplay : MonoBehaviour
{
    public TMP_Text label;

    public void ShowCoordinate(string coordinate)
    {
        if (label != null)
        {
            label.text = "Coordinates: " + coordinate;
        }
    }
}