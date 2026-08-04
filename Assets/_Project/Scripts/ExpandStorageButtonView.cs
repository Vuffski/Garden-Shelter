using UnityEngine;
using TMPro;
using System.Collections;

public class ExpandStorageButtonView : MonoBehaviour
{
    [SerializeField] private PlantSelectionManager selectionManager;
    [SerializeField] private HarvestInventory harvestInventory;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TMP_Text label;
    [SerializeField] private UnityEngine.UI.Button button;

    private Color originalLabelColor;
    private bool isLabelColorCached = false;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<UnityEngine.UI.Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        if (selectionManager != null)
        {
            selectionManager.OnSelectedPlantChanged += HandleSelectedPlantChanged;
        }

        if (harvestInventory != null)
        {
            harvestInventory.OnCountChanged += HandleInventoryCountChanged;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        if (selectionManager != null)
        {
            selectionManager.OnSelectedPlantChanged -= HandleSelectedPlantChanged;
        }

        if (harvestInventory != null)
        {
            harvestInventory.OnCountChanged -= HandleInventoryCountChanged;
        }
    }

    private void Start()
    {
        CacheLabelColor();
        RefreshButton();
    }

    private void CacheLabelColor()
    {
        if (label != null && !isLabelColorCached)
        {
            originalLabelColor = label.color;
            isLabelColorCached = true;
        }
    }

    private void HandleSelectedPlantChanged(PlantData plant)
    {
        RefreshButton();
    }

    private void HandleInventoryCountChanged(PlantData plant)
    {
        if (selectionManager != null && plant == selectionManager.SelectedPlant)
        {
            RefreshButton();
        }
    }

    private void RefreshButton()
    {
        if (selectionManager == null || harvestInventory == null)
        {
            gameObject.SetActive(false);
            return;
        }

        PlantData selectedPlant = selectionManager.SelectedPlant;
        if (selectedPlant == null || !harvestInventory.HasMoreExpansionLevels(selectedPlant))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (label != null)
        {
            int cost = harvestInventory.GetNextExpansionCost(selectedPlant);
            label.text = "EXPAND - $" + cost;
        }
    }

    private void OnButtonClicked()
    {
        if (selectionManager == null || harvestInventory == null || economyManager == null)
        {
            return;
        }

        PlantData selectedPlant = selectionManager.SelectedPlant;
        if (selectedPlant == null || !harvestInventory.HasMoreExpansionLevels(selectedPlant))
        {
            return;
        }

        int cost = harvestInventory.GetNextExpansionCost(selectedPlant);
        if (economyManager.CanAfford(cost))
        {
            economyManager.Spend(cost);
            harvestInventory.ExpandStorage(selectedPlant);
            RefreshButton();
        }
        else
        {
            FlashLabelRed();
        }
    }

    private void FlashLabelRed()
    {
        if (label != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        label.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        label.color = originalLabelColor;
        flashCoroutine = null;
    }
}
