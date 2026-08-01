using UnityEngine;

public class AchievementsPanelToggle : MonoBehaviour
{
    public GameObject panel;
    public CanvasGroup gameplayUIGroup;

    public void TogglePanel()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);

            if (gameplayUIGroup != null)
            {
                bool isOpen = panel.activeSelf;
                gameplayUIGroup.interactable = !isOpen;
                gameplayUIGroup.blocksRaycasts = !isOpen;
            }
        }
    }
}