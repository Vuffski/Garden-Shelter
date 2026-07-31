using UnityEngine;

public class DoggySelectionManager : MonoBehaviour
{
    public PlantSelectionManager plantSelection;

    public DoggyData SelectedDoggy { get; private set; }

    private DoggyButtonView selectedButton;

    public void SelectDoggy(DoggyButtonView button)
    {
        if (selectedButton != null)
        {
            selectedButton.SetBold(false);
        }

        selectedButton = button;

        if (button != null)
        {
            button.SetBold(true);
            SelectedDoggy = button.doggyData;
        }

        if (plantSelection != null)
        {
            plantSelection.ClearSelection();
        }
    }

    public void ClearSelection()
    {
        if (selectedButton != null)
        {
            selectedButton.SetBold(false);
        }

        selectedButton = null;
        SelectedDoggy = null;
    }
}

