using UnityEngine;
using TMPro;

public class PlantButtonView : MonoBehaviour
{
    public PlantData plantData;
    public TMP_Text label;
    public PlantSelectionManager manager;
    public TMP_Text costLabel;

    private void Start()
    {
        if (plantData != null)
        {
            if (label != null)
            {
                label.text = plantData.PlantName;
            }
            if (costLabel != null)
            {
                costLabel.text = plantData.Cost.ToString();
            }
        }
    }

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