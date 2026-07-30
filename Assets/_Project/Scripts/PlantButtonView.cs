using UnityEngine;
using TMPro;

public class PlantButtonView : MonoBehaviour
{
    public PlantData plantData;
    public TMP_Text label;
    public PlantSelectionManager manager;

    public void SetBold(bool isBold)
    {
        label.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
    }

    public void OnClicked()
    {
        if (manager != null)
        {
            manager.SelectPlant(this);
        }
    }
}