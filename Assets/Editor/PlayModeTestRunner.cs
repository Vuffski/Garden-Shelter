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
        }

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

        private static bool _testExecuted = false;

        private static bool Tick(float elapsed)
        {
            if (!IsNaninovelInitialized())
            {
                return false; // Wait until Naninovel has initialized
            }

            if (_testExecuted) return true;
            _testExecuted = true;

            Debug.Log("[Test] Naninovel initialized! Beginning Economy and Achievement Test.");

            // 1. Find EconomyManager
            var economyManager = Object.FindAnyObjectByType<EconomyManager>();
            if (economyManager == null)
            {
                throw new System.Exception("EconomyManager not found in scene!");
            }

            // 2. Verify initial gold is capped at 10
            int initialMax = economyManager.MaxGold;
            Debug.LogFormat("[Test] MaxGold on start: {0}", initialMax);
            if (initialMax != 10)
            {
                throw new System.Exception($"Expected initial MaxGold to be 10, but got {initialMax}");
            }

            // Verify label style is bold and red
            if (economyManager.goldLabel == null)
            {
                throw new System.Exception("EconomyManager goldLabel is null!");
            }

            string labelText = economyManager.goldLabel.text;
            TMPro.FontStyles fontStyle = economyManager.goldLabel.fontStyle;
            Color color = economyManager.goldLabel.color;

            Debug.LogFormat("[Test] goldLabel text on start: '{0}', fontStyle: {1}, color: {2}", labelText, fontStyle, color);
            if (!labelText.Contains("/10"))
            {
                throw new System.Exception($"Expected goldLabel to contain max cap /10, but text is '{labelText}'");
            }
            if ((fontStyle & TMPro.FontStyles.Bold) == 0)
            {
                throw new System.Exception("Expected goldLabel to be BOLD when at cap.");
            }
            if (color != Color.red)
            {
                throw new System.Exception("Expected goldLabel color to be RED when at cap.");
            }

            // 3. Find Beet plant asset
            var beetPlant = AssetDatabase.LoadAssetAtPath<PlantData>("Assets/_Project/ScriptableObjects/Plants/Beet.asset");
            if (beetPlant == null)
            {
                throw new System.Exception("Beet plant asset not found!");
            }

            // 4. Trigger 1 harvest
            Debug.Log("[Test] Adding 1 Beet harvest...");
            HarvestInventory.Instance.AddHarvest(beetPlant, 1);

            // 5. Find Harvest1 achievement
            var harvest1 = AssetDatabase.LoadAssetAtPath<AchievementData>("Assets/_Project/ScriptableObjects/Achievements/Harvest1.asset");
            if (harvest1 == null)
            {
                throw new System.Exception("Harvest1 achievement asset not found!");
            }

            // 6. Verify achievement is ready to collect
            if (!AchievementManager.Instance.IsReadyToCollect(harvest1))
            {
                throw new System.Exception("Harvest1 achievement is not ready to collect after harvesting!");
            }

            // 7. Claim achievement
            Debug.Log("[Test] Claiming Harvest1 achievement...");
            AchievementManager.Instance.ClaimAchievement(harvest1);

            // 8. Verify MaxGold increased by 15 (making it 25)
            int newMax = economyManager.MaxGold;
            Debug.LogFormat("[Test] MaxGold after claiming: {0}", newMax);
            if (newMax != 25)
            {
                throw new System.Exception($"Expected new MaxGold to be 25, but got {newMax}");
            }

            // 9. Verify label text, style, and color have updated
            labelText = economyManager.goldLabel.text;
            fontStyle = economyManager.goldLabel.fontStyle;
            color = economyManager.goldLabel.color;

            Debug.LogFormat("[Test] goldLabel text after claiming: '{0}', fontStyle: {1}, color: {2}", labelText, fontStyle, color);
            if (!labelText.Contains("/25"))
            {
                throw new System.Exception($"Expected goldLabel to contain max cap /25, but text is '{labelText}'");
            }
            if ((fontStyle & TMPro.FontStyles.Bold) != 0)
            {
                throw new System.Exception("Expected goldLabel NOT to be bold when below cap.");
            }
            if (color == Color.red)
            {
                throw new System.Exception("Expected goldLabel color NOT to be red when below cap.");
            }

            Debug.Log("[Test] All Economy and Achievement tests passed successfully!");
            return true;
        }

        private static string GetResult()
        {
            var res = new TestResult
            {
                success = true,
                logs = _capturedLogs.ToArray()
            };
            return JsonUtility.ToJson(res);
        }
    }
}
// Touch comment to trigger compile
// Touch comment for clean run
// Touch comment for final run