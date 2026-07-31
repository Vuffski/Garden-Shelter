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

                        if (plantSelection != null && plantSelection.SelectedPlant != null)
                        {
                            if (economyManager != null)
                            {
                                if (economyManager.CanAfford(plantSelection.SelectedPlant.Cost))
                                {
                                    tileView.SetPlant(plantSelection.SelectedPlant.Icon);
                                    economyManager.Spend(plantSelection.SelectedPlant.Cost);
                                }
                                else
                                {
                                    Debug.Log("Not enough gold");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}