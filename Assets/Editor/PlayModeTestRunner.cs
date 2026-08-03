using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 3);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 12.0f);

        // Log capture
        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 100;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (_testDone) return;

            // Start log capture
            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[PlayModeTest] Setup threw exception: " + e);
                    FinishTest(true, e.Message);
                    return;
                }
                return; // Let one frame pass after Setup before first Tick
            }

            // Tick every frame
            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try
            {
                bool complete = Tick(elapsed);
                if (complete || timedOut)
                {
                    if (timedOut && !complete)
                    {
                        Debug.LogWarning("[PlayModeTest] Test timed out after " + elapsed + "s");
                    }
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Tick threw exception: " + e);
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson;
            try
            {
                resultJson = GetResult();
            }
            catch (System.Exception e)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "GetResult() threw: " + e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }

            // Inject logs and error info if needed
            if (isError && errorMessage != null)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = errorMessage,
                    logs = _capturedLogs.ToArray()
                });
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            _capturedLogs.Add("[" + type + "] " + message);
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
            public List<CameraReport> camerasReport;
        }

        [System.Serializable]
        private class CameraReport
        {
            public string name;
            public string tag;
            public int cullingMask;
            public string cullingMaskLayers;
            public bool orthographic;
            public float orthoSizeOrFov;
            public bool isMainCameraReference;
            public string comparisonToMain;
        }

        private static List<CameraReport> finalReport = new List<CameraReport>();

        private static void Setup()
        {
            Debug.Log("[Test] Setup complete. Loading Garden.unity scene in play mode...");
            string scenePath = "Assets/_Project/Scenes/Garden.unity";
            var loadParams = new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single);
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath, loadParams);
        }

        private static bool IsNaninovelInitialized()
        {
            var type = System.Type.GetType("Naninovel.Engine, Elringus.Naninovel.Runtime");
            if (type == null) return false;
            var prop = type.GetProperty("Initialized", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (prop == null) return false;
            return (bool)prop.GetValue(null);
        }

        private static string GetLayerNamesFromMask(int mask)
        {
            if (mask == -1) return "Everything";
            if (mask == 0) return "Nothing";

            var layers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    string name = LayerMask.LayerToName(i);
                    layers.Add(string.IsNullOrEmpty(name) ? "Layer " + i : name);
                }
            }
            return string.Join(", ", layers);
        }

        private static bool Tick(float elapsed)
        {
            if (!IsNaninovelInitialized())
            {
                return false; // Wait until Naninovel has initialized
            }

            Debug.Log("[Test] Naninovel initialized! Beginning camera inspection.");

            // Find gameplay main camera referenced on Bootstrapper
            Camera mainRef = null;
            var bootstrapper = Object.FindAnyObjectByType<NaninovelBootstrapper>();
            if (bootstrapper != null)
            {
                var field = bootstrapper.GetType().GetField("gameplayCamera", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    mainRef = field.GetValue(bootstrapper) as Camera;
                }
            }

            if (mainRef == null)
            {
                // Fallback to active Camera.main if bootstrapper didn't cache/serialize it
                mainRef = Camera.main;
            }

            Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Debug.Log("[Test] Found " + allCameras.Length + " cameras in play mode.");

            foreach (var cam in allCameras)
            {
                bool isRef = (cam == mainRef);
                var r = new CameraReport
                {
                    name = cam.name,
                    tag = cam.tag,
                    cullingMask = cam.cullingMask,
                    cullingMaskLayers = GetLayerNamesFromMask(cam.cullingMask),
                    orthographic = cam.orthographic,
                    orthoSizeOrFov = cam.orthographic ? cam.orthographicSize : cam.fieldOfView,
                    isMainCameraReference = isRef
                };

                if (isRef)
                {
                    r.comparisonToMain = "This is the Main Camera reference.";
                }
                else if (mainRef != null)
                {
                    string sizeComp = "";
                    if (cam.orthographic && mainRef.orthographic)
                    {
                        sizeComp = Mathf.Approximately(cam.orthographicSize, mainRef.orthographicSize)
                            ? "Identical size (" + cam.orthographicSize + ")"
                            : "Different size (" + cam.orthographicSize + " vs " + mainRef.orthographicSize + ")";
                    }
                    else
                    {
                        sizeComp = "Different projection modes (Ortho vs Perspective)";
                    }

                    int maskOverlap = cam.cullingMask & mainRef.cullingMask;
                    string maskComp = maskOverlap == 0
                        ? "Perfect separation (No overlap)"
                        : "Overlap on layers: " + GetLayerNamesFromMask(maskOverlap);

                    r.comparisonToMain = "Tag check: '" + cam.tag + "' vs Reference: '" + mainRef.tag + "'. Size: " + sizeComp + ". Mask: " + maskComp;
                }
                else
                {
                    r.comparisonToMain = "No Main Camera reference available for comparison.";
                }

                finalReport.Add(r);
                Debug.Log("[Test] [CAMERA REPORT] Name: '" + r.name + "', Tag: '" + r.tag + "', Ortho: " + r.orthographic + ", Size/FOV: " + r.orthoSizeOrFov + ", Layers: " + r.cullingMaskLayers + ". Compare: " + r.comparisonToMain);
            }

            return true; // Finished
        }

        private static string GetResult()
        {
            var res = new TestResult
            {
                success = true,
                camerasReport = finalReport,
                logs = _capturedLogs.ToArray()
            };
            return JsonUtility.ToJson(res);
        }
    }
}
// Touch comment to trigger compile
// Touch comment for clean run
// Touch comment for final run