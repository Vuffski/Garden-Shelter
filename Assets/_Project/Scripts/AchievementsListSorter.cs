using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementsListSorter : MonoBehaviour
{
    public AchievementManager manager;
    private AchievementEntryView[] entries;

    private void OnEnable()
    {
        entries = GetComponentsInChildren<AchievementEntryView>(true);
    }

    private void Update()
    {
        if (manager == null || entries == null) return;

        var validEntries = entries.Where(e => e != null && e.achievementData != null).ToList();

        var sortedEntries = validEntries.OrderBy(e =>
        {
            if (manager.IsReadyToCollect(e.achievementData)) return 0;
            if (manager.IsCompleted(e.achievementData)) return 2;
            return 1;
        })
        .ThenByDescending(e =>
        {
            if (manager.IsCompleted(e.achievementData) || manager.IsReadyToCollect(e.achievementData))
            {
                return 0f;
            }

            int progress = manager.GetProgress(e.achievementData);
            int target = e.achievementData.TargetValue;
            if (target <= 0) return 0f;
            return (float)progress / target;
        })
        .ThenBy(e =>
        {
            if (manager.IsReadyToCollect(e.achievementData))
            {
                return e.achievementData.Title;
            }
            return string.Empty;
        })
        .ToList();

        for (int i = 0; i < sortedEntries.Count; i++)
        {
            sortedEntries[i].transform.SetSiblingIndex(i);
        }
    }
}