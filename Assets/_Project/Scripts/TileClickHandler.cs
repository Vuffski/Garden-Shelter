using UnityEngine;
using UnityEngine.InputSystem;

public class TileClickHandler : MonoBehaviour
{
    public CoordinateDisplay display;
    public PlantSelectionManager plantSelection;
    public EconomyManager economyManager;

    private void Update()
    {
        if (display == null) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            if (Camera.main != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                worldPos.z = 0f;

                Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
                if (hitCollider != null)
                {
                    TileView tileView = hitCollider.GetComponent<TileView>();
                    if (tileView != null)
                    {
                        display.ShowCoordinate(tileView.GetLabel());

                        if (tileView.IsReadyToHarvest)
                        {
                            PlantData harvested = tileView.Harvest();
                            if (harvested != null)
                            {
                                PlantButtonView[] buttons = FindObjectsOfType<PlantButtonView>();
                                foreach (PlantButtonView button in buttons)
                                {
                                    if (button.plantData == harvested)
                                    {
                                        button.AddHarvested();
                                    }
                                }
                            }
                            return;
                        }

                        if (plantSelection != null && plantSelection.SelectedPlant != null)
                        {
                            if (economyManager != null)
                            {
                                int cost = plantSelection.SelectedPlant.Cost;

                                if (!economyManager.CanAfford(cost))
                                {
                                    plantSelection.FlashSelectedCost();
                                    return; // Stop here, don't plant
                                }

                                if (!tileView.IsOccupied)
                                {
                                    tileView.SetPlant(plantSelection.SelectedPlant);
                                    economyManager.Spend(cost);
                                }
                                else
                                {
                                    if (tileView.HandleOverwriteClick(plantSelection.SelectedPlant))
                                    {
                                        economyManager.Spend(cost);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}