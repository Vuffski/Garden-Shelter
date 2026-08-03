using UnityEngine;

public class NaninovelBootstrapper : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;

    private static NaninovelBootstrapper Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make this object persistent so it carries over between scene loads
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        // If Naninovel is not initialized, initialize it asynchronously
        if (!Naninovel.Engine.Initialized)
        {
            await Naninovel.RuntimeInitializer.Initialize();
        }

        // Fix up any other camera (like Naninovel's main camera)
        NaninovelRuntimeFixup.FixUp(gameplayCamera);

        // Enforce a single EventSystem in the scene
        gameObject.AddComponent<EventSystemEnforcer>();
        EventSystemEnforcer.Enforce();
    }
}

