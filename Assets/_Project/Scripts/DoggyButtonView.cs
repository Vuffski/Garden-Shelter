using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DoggyButtonView : MonoBehaviour
{
    public DoggyData doggyData;
    public TMP_Text label;
    public DoggySelectionManager manager;
    public Transform costIconContainer;
    public UnityEngine.UI.Image costIconPrefab;

    private List<UnityEngine.UI.Image> costIcons = new List<UnityEngine.UI.Image>();
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (doggyData != null && label != null)
        {
            label.text = doggyData.DoggyName;
        }

        if (doggyData != null && costIconContainer != null && costIconPrefab != null)
        {
            if (doggyData.Costs != null)
            {
                foreach (var cost in doggyData.Costs)
                {
                    if (cost.Plant == null) continue;
                    for (int i = 0; i < cost.Amount; i++)
                    {
                        UnityEngine.UI.Image iconInstance = Instantiate(costIconPrefab, costIconContainer);
                        iconInstance.sprite = cost.Plant.Icon;
                        costIcons.Add(iconInstance);
                    }
                }
            }
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

    public void FlashCostRed()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (var icon in costIcons)
        {
            if (icon != null)
            {
                icon.color = Color.red;
            }
        }

        yield return new WaitForSeconds(0.3f);

        foreach (var icon in costIcons)
        {
            if (icon != null)
            {
                icon.color = Color.white;
            }
        }

        flashCoroutine = null;
    }
}
