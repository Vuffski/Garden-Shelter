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
            int progress = manager.GetProgress(e.achievementData);
            int target = e.achievementData.TargetValue;
            bool isComplete = progress >= target;
            return isComplete; // false (incomplete) first, true (complete) last
        })
        .ThenByDescending(e =>
        {
            int progress = manager.GetProgress(e.achievementData);
            int target = e.achievementData.TargetValue;
            bool isComplete = progress >= target;
            if (isComplete) return 0f; // complete ones can have any key, since order doesn't matter

            if (target <= 0) return 0f;
            return (float)progress / target;
        })
        .ToList();

        for (int i = 0; i < sortedEntries.Count; i++)
        {
            sortedEntries[i].transform.SetSiblingIndex(i);
        }
    }
}