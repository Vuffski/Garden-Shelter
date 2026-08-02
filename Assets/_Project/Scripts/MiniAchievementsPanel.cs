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
        List<AchievementData> incompleteAchievements = new List<AchievementData>();
        
        var all = achievementManager.AllAchievements;
        if (all != null)
        {
            for (int i = 0; i < all.Count; i++)
            {
                AchievementData a = all[i];
                if (a != null && !achievementManager.IsCompleted(a))
                {
                    incompleteAchievements.Add(a);
                }
            }
        }

        // Re-sort descending by current progress.
        incompleteAchievements.Sort((x, y) =>
        {
            int progressX = achievementManager.GetProgress(x);
            int progressY = achievementManager.GetProgress(y);
            return progressY.CompareTo(progressX); // Descending order
        });

        // Reassign all slots unconditionally, calling SetData or Clear() on every single slot every frame.
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            if (i < incompleteAchievements.Count)
            {
                AchievementData achievement = incompleteAchievements[i];
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
