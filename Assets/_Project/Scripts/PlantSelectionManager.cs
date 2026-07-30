using UnityEngine;

public class PlantSelectionManager : MonoBehaviour
{
    private PlantButtonView selectedButton;

    public PlantData SelectedPlant { get; private set; }

    public void SelectPlant(PlantButtonView button)
    {
        if (selectedButton != null)
        {
            selectedButton.SetBold(false);
        }

        selectedButton = button;

        if (button != null)
        {
            button.SetBold(true);
            SelectedPlant = button.plantData;
        }
    }
}