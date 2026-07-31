using UnityEngine;
using TMPro;

public class DoggyButtonView : MonoBehaviour
{
    public DoggyData doggyData;
    public TMP_Text label;
    public DoggySelectionManager manager;

    private void Start()
    {
        if (doggyData != null && label != null)
        {
            label.text = doggyData.DoggyName;
        }
    }

    public void SetBold(bool isBold)
    {
        if (label != null)
        {
            label.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    public void OnClicked()
    {
        if (manager != null)
        {
            manager.SelectDoggy(this);
        }
    }
}
