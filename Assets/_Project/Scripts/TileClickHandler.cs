using UnityEngine;
using UnityEngine.InputSystem;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;

    public PlantSelectionManager plantSelection;
    public DoggySelectionManager doggySelection;
    public DoggyCapacityPanelView doggyCapacityPanel;
    public EconomyManager economyManager;
    public HarvestInventory harvestInventory;
    public DoggyFieldManager doggyFieldManager;
    public AchievementManager achievementManager;
    public GameObject achievementsPanel;

    private void Update()
    {
        if (achievementsPanel != null && achievementsPanel.activeInHierarchy) return;

        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            if (gameplayCamera != null)
            {
                Vector3 worldPos = gameplayCamera.ScreenToWorldPoint(mouseScreenPos);
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
                                if (achievementManager != null)
                                {
                                    achievementManager.RegisterHarvest(result.Plant, result.Amount);
                                }
                            }
                            return;
                        }

                        Debug.Log("SelectedPlant is currently: " + (plantSelection != null && plantSelection.SelectedPlant != null ? plantSelection.SelectedPlant.name : "NULL"));

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
                            return;
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

                            bool isDoggyForDoggySwap = tileView.IsOccupied && tileView.IsOccupiedByDoggy;

                            if (!isDoggyForDoggySwap && doggyFieldManager != null && !doggyFieldManager.CanPlaceNewDoggy())
                            {
                                if (doggyCapacityPanel != null)
                                {
                                    doggyCapacityPanel.FlashRed();
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