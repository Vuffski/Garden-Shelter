using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DoggyButtonView : MonoBehaviour
{
    private static List<DoggyButtonView> allButtons = new List<DoggyButtonView>();

    public DoggyData doggyData;
    public TMP_Text label;
    public DoggySelectionManager manager;
    public Transform costIconContainer;
    public UnityEngine.UI.Image costIconPrefab;
    public UnlockManager unlockManager;

    private List<UnityEngine.UI.Image> costIcons = new List<UnityEngine.UI.Image>();
    private Coroutine flashCoroutine;
    private bool isInitialized = false;

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
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked += HandleDoggyUnlocked;
        }
    }

    private void OnDisable()
    {
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked -= HandleDoggyUnlocked;
        }
    }

    private void HandleDoggyUnlocked(DoggyData unlockedDoggy)
    {
        if (unlockedDoggy == doggyData)
        {
            gameObject.SetActive(true);
            Initialize();
        }

        // Propagate to any matching inactive button in the static registry
        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            var btn = allButtons[i];
            if (btn != null && !btn.gameObject.activeSelf && btn.doggyData == unlockedDoggy)
            {
                btn.gameObject.SetActive(true);
                btn.Initialize();
            }
        }
    }

    private void Start()
    {
        if (unlockManager != null && !unlockManager.IsDoggyUnlocked(doggyData))
        {
            gameObject.SetActive(false);
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;

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
