using UnityEngine;
using Naninovel;

public class GameplayVisibilityController : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private TileClickHandler tileClickHandler;

    private IScriptPlayer scriptPlayer;
    private bool? lastPlayingState = null;

    private void Start()
    {
        if (Engine.Initialized)
        {
            scriptPlayer = Engine.GetService<IScriptPlayer>();
        }
    }

    private void Update()
    {
        if (scriptPlayer == null)
        {
            if (Engine.Initialized)
            {
                scriptPlayer = Engine.GetService<IScriptPlayer>();
            }

            if (scriptPlayer == null)
            {
                return;
            }
        }

        bool isPlaying = scriptPlayer.Playing;
        bool isChoiceUIVisible = false;
        if (Engine.Initialized)
        {
            var uiManager = Engine.GetService<IUIManager>();
            var choicePanel = uiManager?.GetUI<Naninovel.UI.ChoiceHandlerPanel>();
            isChoiceUIVisible = choicePanel != null && choicePanel.Visible;
        }

        bool shouldHide = isPlaying || isChoiceUIVisible;

        if (lastPlayingState == null || lastPlayingState.Value != shouldHide)
        {
            lastPlayingState = shouldHide;

            bool shouldShow = !shouldHide;

            if (objectsToHide != null)
            {
                foreach (var obj in objectsToHide)
                {
                    if (obj != null)
                    {
                        obj.SetActive(shouldShow);
                    }
                }
            }

            if (tileClickHandler != null)
            {
                tileClickHandler.enabled = shouldShow;
            }
        }
    }
}
