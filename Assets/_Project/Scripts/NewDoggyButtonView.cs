using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class NewDoggyButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnlockManager unlockManager;
    [SerializeField] private TreatManager treatManager;
    [SerializeField] private int treatCost = 1;
    [SerializeField] private UnityEngine.UI.Image glowImage;
    [SerializeField] private TMP_Text label;

    [Header("Glow Pulse Settings")]
    [SerializeField] private Color baseGlowColor = new Color(1f, 0.84f, 0f); // Gold
    [SerializeField] private float brightenMultiplier = 1.4f;
    [SerializeField] private float darkenMultiplier = 0.85f;
    [SerializeField] private float brightenDuration = 0.15f;
    [SerializeField] private float darkenDuration = 0.4f;

    [Header("Unaffordable Color Settings")]
    [SerializeField] private Color unaffordableColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked += HandleDoggyUnlocked;
        }

        if (treatManager != null)
        {
            treatManager.OnTreatsChanged += HandleTreatsChanged;
        }

        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<UnityEngine.UI.Button>();
        }
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        if (unlockManager != null)
        {
            unlockManager.OnDoggyUnlocked -= HandleDoggyUnlocked;
        }

        if (treatManager != null)
        {
            treatManager.OnTreatsChanged -= HandleTreatsChanged;
        }
    }

    private void OnEnable()
    {
        // Restart flash routine if we are active and affordable
        if (unlockManager != null && unlockManager.AnyDoggiesLeftToUnlock())
        {
            if (treatManager != null && treatManager.OwnedTreats >= treatCost)
            {
                StartFlashRoutine();
            }
            else
            {
                StopFlashRoutine();
                if (glowImage != null)
                {
                    glowImage.color = unaffordableColor;
                }
            }
        }
    }

    private void OnDisable()
    {
        StopFlashRoutine();
    }

    private void Start()
    {
        if (label != null)
        {
            label.text = "New Doggy";
        }
        RefreshState();
    }

    private void HandleDoggyUnlocked(DoggyData doggy)
    {
        RefreshState();
    }

    private void HandleTreatsChanged(int currentTreats)
    {
        RefreshState();
    }

    private void RefreshState()
    {
        if (unlockManager == null) return;

        if (!unlockManager.AnyDoggiesLeftToUnlock())
        {
            gameObject.SetActive(false);
            StopFlashRoutine();
            return;
        }

        gameObject.SetActive(true);

        if (treatManager != null)
        {
            if (treatManager.OwnedTreats >= treatCost)
            {
                StartFlashRoutine();
            }
            else
            {
                StopFlashRoutine();
                if (glowImage != null)
                {
                    glowImage.color = unaffordableColor;
                }
            }
        }
    }

    private void OnButtonClicked()
    {
        if (unlockManager == null || treatManager == null) return;

        if (!unlockManager.AnyDoggiesLeftToUnlock())
        {
            return;
        }

        if (treatManager.TrySpendTreats(treatCost))
        {
            unlockManager.UnlockRandomDoggy();
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.Hide();
            }
            RefreshState();
        }
    }

    private void StartFlashRoutine()
    {
        if (gameObject.activeInHierarchy && flashCoroutine == null)
        {
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private void StopFlashRoutine()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (glowImage == null) yield break;

        Color brightColor = GetHSVMultipliedColor(baseGlowColor, brightenMultiplier);
        Color darkColor = GetHSVMultipliedColor(baseGlowColor, darkenMultiplier);

        while (true)
        {
            float elapsed = 0f;
            while (elapsed < brightenDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / brightenDuration);
                glowImage.color = Color.Lerp(darkColor, brightColor, t);
                yield return null;
            }
            glowImage.color = brightColor;

            elapsed = 0f;
            while (elapsed < darkenDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / darkenDuration);
                glowImage.color = Color.Lerp(brightColor, darkColor, t);
                yield return null;
            }
            glowImage.color = darkColor;
        }
    }

    private Color GetHSVMultipliedColor(Color baseColor, float multiplier)
    {
        float h, s, v;
        Color.RGBToHSV(baseColor, out h, out s, out v);
        v = Mathf.Clamp01(v * multiplier);
        Color result = Color.HSVToRGB(h, s, v);
        result.a = baseColor.a;
        return result;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null || treatManager == null) return;

        if (treatManager.OwnedTreats < treatCost)
        {
            TooltipManager.Instance.Show("Insufficient Treats");
        }
        else
        {
            TooltipManager.Instance.Show($"Recruit a random new doggy for {treatCost} Treat(s)");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.Hide();
        }
    }
}
