using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlantButtonView : MonoBehaviour
{
    private static List<PlantButtonView> allButtons = new List<PlantButtonView>();

    public PlantData plantData;
    public TMP_Text label;
    public PlantSelectionManager manager;
    public TMP_Text costLabel;
    public TMP_Text ownedLabel;
    public HarvestInventory inventory;
    public UnlockManager unlockManager;

    private Color normalCostColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        allButtons.Add(this);
    }

    private void OnDestroy()
    {
        allButtons.Remove(this);
    }

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnCountChanged += HandleCountChanged;
        }
        if (unlockManager != null)
        {
            unlockManager.OnPlantUnlocked += HandlePlantUnlocked;
        }
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnCountChanged -= HandleCountChanged;
        }
        if (unlockManager != null)
        {
            unlockManager.OnPlantUnlocked -= HandlePlantUnlocked;
        }
    }

    private void HandleCountChanged(PlantData changedPlant)
    {
        if (changedPlant == plantData)
        {
            RefreshLabel();
        }
    }

    private void HandlePlantUnlocked(PlantData unlockedPlant)
    {
        if (unlockedPlant == plantData)
        {
            gameObject.SetActive(true);
            Initialize();
        }

        // Propagate to any matching inactive button in the static registry
        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            var btn = allButtons[i];
            if (btn != null && !btn.gameObject.activeSelf && btn.plantData == unlockedPlant)
            {
                btn.gameObject.SetActive(true);
                btn.Initialize();
            }
        }
    }

    private void RefreshLabel()
    {
        if (ownedLabel != null && inventory != null)
        {
            ownedLabel.text = inventory.GetCount(plantData).ToString();
        }
    }

    private void Start()
    {
        if (unlockManager != null && !unlockManager.IsPlantUnlocked(plantData))
        {
            gameObject.SetActive(false);
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        RefreshLabel();

        if (plantData != null)
        {
            if (label != null)
            {
                label.text = plantData.PlantName;
            }
            if (costLabel != null)
            {
                costLabel.text = plantData.Cost.ToString();
                normalCostColor = costLabel.color;
            }
        }
    }

    public void SetBold(bool isBold)
    {
        label.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
    }

    public void OnClicked()
    {
        if (plantData != null)
        {
            Debug.Log("Plant button clicked: " + plantData.PlantName);
        }

        if (manager != null)
        {
            manager.SelectPlant(this);
        }
    }

    public void FlashCostRed()
    {
        if (costLabel != null)
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
        costLabel.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        costLabel.color = normalCostColor;
        flashCoroutine = null;
    }
}