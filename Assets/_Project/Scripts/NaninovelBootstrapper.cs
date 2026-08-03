using UnityEngine;

public class NaninovelBootstrapper : MonoBehaviour
{
    private void Awake()
    {
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
    }
}

