using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CombatTest.Editor
{
    public static class CombatTestSmoke
    {
        public static void Run()
        {
            CombatTestBuilder.RepairSensors();
            EditorSceneManager.OpenScene(CombatTestBuilder.ScenePath);
            double timeout = EditorApplication.timeSinceStartup + 150;
            EditorApplication.update += () =>
            {
                if (EditorApplication.isPlaying && UnityEngine.Object.FindAnyObjectByType<CombatTestSmokeProbe>() == null)
                    new GameObject("Automated combat regression (editor only)").AddComponent<CombatTestSmokeProbe>();
                // The user's Console Error Pause must not strand an unattended batch run.
                if (EditorApplication.isPaused) EditorApplication.isPaused = false;
                if (EditorApplication.timeSinceStartup > timeout) EditorApplication.Exit(2);
            };
            EditorApplication.EnterPlaymode();
        }
    }
}
