using UnityEngine;
using TMPro;

public class PlantSelectionManager : MonoBehaviour
{
    public TMP_Text selectedPlantLabel;

    public PlantData SelectedPlant { get; private set; }

    public void SelectPlant(PlantData plant)
    {
        SelectedPlant = plant;
        selectedPlantLabel.text = "<b>" + plant.PlantName + "</b>";
    }
}