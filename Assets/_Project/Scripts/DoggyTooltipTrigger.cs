using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(DoggyButtonView))]
public class DoggyTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private AchievementManager achievementManager;

    private DoggyButtonView buttonView;

    private void Awake()
    {
        buttonView = GetComponent<DoggyButtonView>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonView == null) return;

        DoggyData doggyData = buttonView.DoggyData;
        if (doggyData == null) return;

        // Resolve current effective stats
        DoggyEffectiveStats stats = DoggyEffectiveStats.Resolve(doggyData, achievementManager);

        List<string> lines = new List<string>();

        // Doggy Name
        lines.Add(doggyData.DoggyName);

        // Format cost readably
        List<string> costParts = new List<string>();
        if (doggyData.Costs != null)
        {
            foreach (var cost in doggyData.Costs)
            {
                if (cost.Plant != null && cost.Amount > 0)
                {
                    costParts.Add($"{cost.Amount} {cost.Plant.PlantName}");
                }
            }
        }
        string costText = costParts.Count > 0 ? string.Join(", ", costParts) : "Free";
        lines.Add($"Cost: {costText}");

        // Growth Speed (if active)
        if (doggyData.UseGrowthSpeed && stats != null)
        {
            lines.Add($"Growth Speed: {stats.growthSpeedModifier}x");
        }

        // Yield (if active)
        if (doggyData.UseYield)
        {
            lines.Add($"Yield Bonus: {doggyData.YieldChance * 100}%");
        }

        // Auto-Harvest (if true)
        if (stats != null && stats.autoHarvestEnabled)
        {
            lines.Add("Auto-Harvest: Yes");
        }

        string content = string.Join("\n", lines);

        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.Show(content);
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
