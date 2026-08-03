using UnityEngine;
using Naninovel;
using System.Collections;

public class NaninovelGameplayMediator : MonoBehaviour
{
    private Canvas gameplayCanvas;
    private TileClickHandler tileClickHandler;
    private IScriptPlayer scriptPlayer;
    private ITextPrinterManager printerManager;
    private IChoiceHandlerManager choiceManager;
    private bool wasNaninovelActive = false;

    private void Start()
    {
        gameplayCanvas = FindAnyObjectByType<Canvas>();
        tileClickHandler = FindAnyObjectByType<TileClickHandler>();

        if (gameplayCanvas == null)
        {
            Debug.LogWarning("[NaninovelGameplayMediator] Gameplay Canvas not found in scene.");
        }
        if (tileClickHandler == null)
        {
            Debug.LogWarning("[NaninovelGameplayMediator] TileClickHandler not found in scene.");
        }

        StartCoroutine(InitializeNaninovelListener());
    }

    private IEnumerator InitializeNaninovelListener()
    {
        // Wait until Naninovel Engine is fully initialized
        while (!Engine.Initialized)
        {
            yield return null;
        }

        scriptPlayer = Engine.GetService<IScriptPlayer>();
        printerManager = Engine.GetService<ITextPrinterManager>();
        choiceManager = Engine.GetService<IChoiceHandlerManager>();

        if (scriptPlayer != null)
        {
            scriptPlayer.OnPlay += HandlePlay;
            scriptPlayer.OnStop += HandleStop;
            
            // Set initial state strictly
            wasNaninovelActive = IsNaninovelActive();
            UpdateState(wasNaninovelActive);
        }
        else
        {
            Debug.LogError("[NaninovelGameplayMediator] Naninovel IScriptPlayer service not found.");
        }
    }

    private void OnDestroy()
    {
        if (scriptPlayer != null)
        {
            scriptPlayer.OnPlay -= HandlePlay;
            scriptPlayer.OnStop -= HandleStop;
        }
    }

    private void HandlePlay(IScriptTrack track)
    {
        EvaluateState();
    }

    private void HandleStop(IScriptTrack track)
    {
        EvaluateState();
    }

    private void Update()
    {
        if (!Engine.Initialized)
        {
            if (wasNaninovelActive)
            {
                wasNaninovelActive = false;
                UpdateState(false);
            }
            return;
        }

        EvaluateState();
    }

    private void EvaluateState()
    {
        if (printerManager == null) printerManager = Engine.GetService<ITextPrinterManager>();
        if (choiceManager == null) choiceManager = Engine.GetService<IChoiceHandlerManager>();

        bool isCurrentlyActive = IsNaninovelActive();
        if (isCurrentlyActive != wasNaninovelActive)
        {
            wasNaninovelActive = isCurrentlyActive;
            UpdateState(isCurrentlyActive);
        }
    }

    private bool IsNaninovelActive()
    {
        if (!Engine.Initialized) return false;

        // 1. Check if the script player is playing or executing commands
        if (scriptPlayer != null && (scriptPlayer.Playing || scriptPlayer.Executing))
        {
            return true;
        }

        // 2. Check if any text printer (dialogue box) is active and visible
        if (printerManager != null)
        {
            var printer = printerManager.FindActor(p => p.Visible);
            if (printer != null) return true;
        }

        // 3. Check if any choice handler (dialogue choice panel) is active and visible
        if (choiceManager != null)
        {
            var handler = choiceManager.FindActor(h => h.Visible);
            if (handler != null) return true;
        }

        return false;
    }

    private void UpdateState(bool isNaninovelActive)
    {
        // Hide/show the Canvas
        if (gameplayCanvas != null)
        {
            gameplayCanvas.enabled = !isNaninovelActive;
            Debug.Log($"[NaninovelGameplayMediator] Set Canvas enabled to {!isNaninovelActive}");
        }

        // Disable/enable tile click interactions
        if (tileClickHandler != null)
        {
            tileClickHandler.enabled = !isNaninovelActive;
            Debug.Log($"[NaninovelGameplayMediator] Set TileClickHandler enabled to {!isNaninovelActive}");
        }
    }
}
