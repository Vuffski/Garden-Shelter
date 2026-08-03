using UnityEngine;
using System.Collections.Generic;

public class MiniAchievementsPanel : MonoBehaviour
{
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private List<MiniAchievementEntryView> slots;
    [SerializeField] private GameObject content;

    private void Update()
    {
        if (achievementsPanel != null && achievementsPanel.activeInHierarchy)
        {
            if (content != null && content.activeSelf)
            {
                content.SetActive(false);
            }
            return;
        }

        if (content != null && !content.activeSelf)
        {
            content.SetActive(true);
        }

        if (achievementManager == null || slots == null) return;

        // Fully rebuild the incomplete achievements list from scratch.
        List<AchievementData> listA = new List<AchievementData>(); // Ready-to-collect
        List<AchievementData> listB = new List<AchievementData>(); // Neither ready nor completed
        
        var all = achievementManager.AllAchievements;
        if (all != null)
        {
            for (int i = 0; i < all.Count; i++)
            {
                AchievementData a = all[i];
                if (a != null && achievementManager.IsUnlocked(a))
                {
                    if (achievementManager.IsCompleted(a))
                    {
                        continue;
                    }

                    if (achievementManager.IsReadyToCollect(a))
                    {
                        listA.Add(a);
                    }
                    else
                    {
                        listB.Add(a);
                    }
                }
            }
        }

        // Re-sort descending by progress ratio.
        listB.Sort((x, y) =>
        {
            float progressX = x.TargetValue > 0 ? (float)achievementManager.GetProgress(x) / x.TargetValue : 0f;
            float progressY = y.TargetValue > 0 ? (float)achievementManager.GetProgress(y) / y.TargetValue : 0f;
            return progressY.CompareTo(progressX); // Descending order
        });

        // Concatenate A followed by B
        List<AchievementData> displayedAchievements = new List<AchievementData>(listA.Count + listB.Count);
        displayedAchievements.AddRange(listA);
        displayedAchievements.AddRange(listB);

        // Reassign all slots unconditionally, calling SetData or Clear() on every single slot every frame.
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            if (i < displayedAchievements.Count)
            {
                AchievementData achievement = displayedAchievements[i];
                float progress = achievement.TargetValue > 0 
                    ? (float)achievementManager.GetProgress(achievement) / achievement.TargetValue 
                    : 0f;
                slots[i].SetData(achievement, progress);
            }
            else
            {
                slots[i].Clear();
            }
        }
    }
}
