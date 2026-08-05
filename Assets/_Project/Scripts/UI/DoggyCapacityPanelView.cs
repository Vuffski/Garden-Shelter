using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoggyCapacityPanelView : MonoBehaviour
{
    [SerializeField] private DoggyFieldManager doggyFieldManager;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color filledSlotColor = Color.white;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private bool preserveIconAspect = true;

    private readonly List<Image> slotImages = new List<Image>();
    private readonly List<Color> slotBaseColors = new List<Color>();
    private Coroutine flashCoroutine;

    private void OnEnable()
    {
        if (doggyFieldManager != null)
        {
            doggyFieldManager.OnActiveDoggiesChanged += Refresh;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (doggyFieldManager != null)
        {
            doggyFieldManager.OnActiveDoggiesChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        if (doggyFieldManager == null || slotContainer == null || slotPrefab == null) return;

        int maxSlots = doggyFieldManager.MaxActiveDoggies;
        List<DoggyData> activeDoggies = new List<DoggyData>(doggyFieldManager.GetActiveDoggies());

        while (slotImages.Count < maxSlots)
        {
            GameObject spawned = Instantiate(slotPrefab, slotContainer);
            Image img = spawned.GetComponent<Image>();
            slotImages.Add(img);
            slotBaseColors.Add(emptySlotColor);
        }

        while (slotImages.Count > maxSlots)
        {
            int lastIndex = slotImages.Count - 1;
            if (slotImages[lastIndex] != null)
            {
                Destroy(slotImages[lastIndex].gameObject);
            }
            slotImages.RemoveAt(lastIndex);
            slotBaseColors.RemoveAt(lastIndex);
        }

        for (int i = 0; i < slotImages.Count; i++)
        {
            Image img = slotImages[i];
            img.preserveAspect = preserveIconAspect;
            if (img == null) continue;

            if (i < activeDoggies.Count && activeDoggies[i] != null)
            {
                img.sprite = activeDoggies[i].Icon;
                img.color = activeDoggies[i].IconColor;
                slotBaseColors[i] = activeDoggies[i].IconColor;
            }
            else
            {
                img.sprite = emptySlotSprite;
                img.color = emptySlotColor;
                slotBaseColors[i] = emptySlotColor;
            }
        }
    }

    public void FlashRed()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        foreach (Image img in slotImages)
        {
            if (img != null)
            {
                img.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].color = slotBaseColors[i];
            }
        }

        flashCoroutine = null;
    }
}