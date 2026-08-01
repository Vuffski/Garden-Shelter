using UnityEngine;

public class PlantSelectionManager : MonoBehaviour
{
    public DoggySelectionManager doggySelection;

    private PlantButtonView selectedButton;

    public PlantData SelectedPlant { get; private set; }

    public void SelectPlant(PlantButtonView button)
    {
        Debug.Log("SelectPlant called with: " + (button != null && button.plantData != null ? button.plantData.name : "null"));

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

        if (doggySelection != null)
        {
            doggySelection.ClearSelection();
        }
    }

    public void FlashSelectedCost()
    {
        if (selectedButton != null)
        {
            selectedButton.FlashCostRed();
        }
    }

    public void ClearSelection()
    {
        if (selectedButton != null)
        {
            selectedButton.SetBold(false);
        }
        selectedButton = null;
        SelectedPlant = null;
    }
}