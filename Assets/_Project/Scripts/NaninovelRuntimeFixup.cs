using UnityEngine;
using Naninovel;

public static class NaninovelRuntimeFixup
{
    public static void FixUp(Camera gameplayCamera)
    {
        if (!Engine.Initialized)
        {
            Debug.LogWarning("[NaninovelRuntimeFixup] Naninovel Engine is not initialized. Cannot perform fixup.");
            return;
        }

        // Find all cameras in the scene
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

        // Fallback to find the gameplay camera named 'Main Camera' if the passed reference is null
        if (gameplayCamera == null)
        {
            foreach (Camera cam in allCameras)
            {
                if (cam.name == "Main Camera")
                {
                    gameplayCamera = cam;
                    break;
                }
            }
        }

        // Retrieve Naninovel configurations for layers
        var engineConfig = Engine.Configuration;
        var uiConfig = Engine.GetConfiguration<UIConfiguration>();

        int objectsLayer = engineConfig != null ? engineConfig.ObjectsLayer : 5;
        int uiLayer = uiConfig != null ? uiConfig.ObjectsLayer : 5;

        // Build the mask that only keeps Naninovel's own layers (exclude gameplay layers)
        int naninovelMask = (1 << objectsLayer) | (1 << uiLayer);

        // Update the gameplay camera's culling mask to exclude Naninovel layers
        if (gameplayCamera != null)
        {
            // Restore its tag to MainCamera so raycasting and Camera.main continue to work for gameplay
            gameplayCamera.tag = "MainCamera";

            gameplayCamera.cullingMask &= ~naninovelMask;
            Debug.Log($"[NaninovelRuntimeFixup] Set gameplay camera '{gameplayCamera.name}' culling mask to exclude Naninovel layers: {objectsLayer}, {uiLayer}.");
        }

        foreach (Camera cam in allCameras)
        {
            // If this is NOT our gameplay main camera reference
            if (cam != gameplayCamera)
            {
                // Strip its MainCamera tag
                if (cam.CompareTag("MainCamera"))
                {
                    cam.tag = "Untagged";
                }

                // Set culling mask to exclude gameplay layers (only keep Naninovel's layers)
                cam.cullingMask = naninovelMask;

                Debug.Log($"[NaninovelRuntimeFixup] Fixed up camera: '{cam.name}'. Stripped MainCamera tag, set culling mask to Naninovel layers: {objectsLayer}, {uiLayer}.");
            }
        }
    }
}
