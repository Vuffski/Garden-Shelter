using UnityEngine;
using Naninovel;
using System.Collections;

public class NaninovelGameplayMediator : MonoBehaviour
{
    private Canvas gameplayCanvas;
    private TileClickHandler tileClickHandler;
    private IScriptPlayer scriptPlayer;

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
        if (scriptPlayer != null)
        {
            scriptPlayer.OnPlay += HandlePlay;
            scriptPlayer.OnStop += HandleStop;
            
            // Set initial state based on current playback status
            UpdateState(scriptPlayer.Playing);
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
        UpdateState(true);
    }

    private void HandleStop(IScriptTrack track)
    {
        UpdateState(false);
    }

    private void UpdateState(bool isPlaying)
    {
        // Hide/show the Canvas
        if (gameplayCanvas != null)
        {
            gameplayCanvas.enabled = !isPlaying;
            Debug.Log($"[NaninovelGameplayMediator] Set Canvas enabled to {!isPlaying}");
        }

        // Disable/enable tile click interactions
        if (tileClickHandler != null)
        {
            tileClickHandler.enabled = !isPlaying;
            Debug.Log($"[NaninovelGameplayMediator] Set TileClickHandler enabled to {!isPlaying}");
        }
    }
}
