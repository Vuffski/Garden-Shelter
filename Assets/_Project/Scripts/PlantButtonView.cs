using UnityEngine;
using TMPro;
using System.Collections;

public class PlantButtonView : MonoBehaviour
{
    public PlantData plantData;
    public TMP_Text label;
    public PlantSelectionManager manager;
    public TMP_Text costLabel;
    public TMP_Text ownedLabel;

    private int harvestedCount = 0;
    private Color normalCostColor;
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (ownedLabel != null)
        {
            ownedLabel.text = "0";
        }

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
        if (manager != null)
        {
            manager.SelectPlant(this);
        }
    }

    public void AddHarvested()
    {
        harvestedCount++;
        if (ownedLabel != null)
        {
            ownedLabel.text = harvestedCount.ToString();
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