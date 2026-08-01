using UnityEngine;
using UnityEngine.InputSystem;

public class TileClickHandler : MonoBehaviour
{
    public PlantSelectionManager plantSelection;
    public DoggySelectionManager doggySelection;
    public EconomyManager economyManager;
    public HarvestInventory harvestInventory;
    public DoggyFieldManager doggyFieldManager;

    private void Update()
    {
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
                        if (tileView.IsReadyToHarvest)
                        {
                            HarvestResult result = tileView.Harvest();
                            if (result.Plant != null && harvestInventory != null)
                            {
                                harvestInventory.AddHarvest(result.Plant, result.Amount);
                            }
                            return;
                        }
                        else if (plantSelection != null && plantSelection.SelectedPlant != null)
                        {
                            if (economyManager != null)
                            {
                                int cost = plantSelection.SelectedPlant.Cost;

                                if (!economyManager.CanAfford(cost))
                                {
                                    plantSelection.FlashSelectedCost();
                                    return; // Stop here, don't plant
                                }

                                if (tileView.IsOccupied)
                                {
                                    if (tileView.HandleOverwriteClick(() => tileView.SetPlant(plantSelection.SelectedPlant)))
                                    {
                                        economyManager.Spend(cost);
                                    }
                                }
                                else
                                {
                                    tileView.SetPlant(plantSelection.SelectedPlant);
                                    economyManager.Spend(cost);
                                }
                            }
                        }
                        else if (doggySelection != null && doggySelection.SelectedDoggy != null)
                        {
                            DoggyData doggy = doggySelection.SelectedDoggy;

                            if (doggyFieldManager != null && doggyFieldManager.IsDoggyActive(doggy))
                            {
                                TileView activeTile = doggyFieldManager.GetTileFor(doggy);
                                if (activeTile != null)
                                {
                                    activeTile.FlashDuplicateWarning();
                                }
                                return;
                            }

                            if (harvestInventory != null)
                            {
                                if (!harvestInventory.CanAfford(doggy.Costs))
                                {
                                    doggySelection.FlashSelectedCost();
                                    return;
                                }

                                if (tileView.IsOccupied)
                                {
                                    if (tileView.HandleOverwriteClick(() => tileView.PlaceDoggy(doggy)))
                                    {
                                        harvestInventory.Spend(doggy.Costs);
                                    }
                                }
                                else
                                {
                                    tileView.PlaceDoggy(doggy);
                                    harvestInventory.Spend(doggy.Costs);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}