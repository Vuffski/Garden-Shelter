using UnityEngine;
using TMPro;

public class SellPanelView : MonoBehaviour
{
    public PlantSelectionManager plantSelection;
    public HarvestInventory harvestInventory;
    public EconomyManager economyManager;
    public GameObject sellButtonObject;
    public TMP_Text sellLabel;

    private PlantData currentPlant;

    private void Update()
    {
        if (plantSelection == null) return;

        PlantData selected = plantSelection.SelectedPlant;
        if (selected != currentPlant)
        {
            currentPlant = selected;
            if (currentPlant != null)
            {
                if (sellButtonObject != null)
                {
                    sellButtonObject.SetActive(true);
                }
                if (sellLabel != null)
                {
                    sellLabel.text = "SELL - $" + currentPlant.SellValue;
                }
            }
            else
            {
                if (sellButtonObject != null)
                {
                    sellButtonObject.SetActive(false);
                }
            }
        }
    }

    public void OnSellClicked()
    {
        if (currentPlant == null) return;
        if (harvestInventory == null || economyManager == null) return;

        if (harvestInventory.GetCount(currentPlant) <= 0) return;

        harvestInventory.SpendOne(currentPlant);
        economyManager.Earn(currentPlant.SellValue);
    }
}