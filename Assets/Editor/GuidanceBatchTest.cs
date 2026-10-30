using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Command-line entry point for the existing manual Play-mode smoke test.
[InitializeOnLoad]
public static class GuidanceBatchTest
{
    public static void RunWithPreview()
    {
        CaveGuidanceSetup.PreviewBatch();
        Run();
    }

    static GuidanceBatchTest()
    {
        if (!SessionState.GetBool("GuidanceBatchTest", false)) return;
        EditorApplication.update += Poll;
        Application.logMessageReceived += Log;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MineDefense.unity");
        File.WriteAllText("ProgressionTestResult.txt", "RUNNING");
        SessionState.SetBool("GuidanceBatchTest", true);
        SessionState.SetBool("GuidanceTestStarted", false);
        SessionState.SetFloat("GuidanceTestStart", (float)EditorApplication.timeSinceStartup);
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
        Application.logMessageReceived -= Log;
        Application.logMessageReceived += Log;
        EditorApplication.EnterPlaymode();
    }

    private static void Poll()
    {
        if (!SessionState.GetBool("GuidanceBatchTest", false)) return;
        if (EditorApplication.timeSinceStartup - SessionState.GetFloat("GuidanceTestStart", 0) > 180)
        { Finish("FAIL: verification timed out.", 1); return; }
        if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;
        var p = Object.FindFirstObjectByType<CaveDefenseProgression>();
        if (p == null || Time.timeSinceLevelLoad < 1f) return;
        if (!SessionState.GetBool("GuidanceTestStarted", false))
        {
            SessionState.SetBool("GuidanceTestStarted", true);
            ProgressionSmokeTest.Run();
        }
        if (File.ReadAllText("ProgressionTestResult.txt").StartsWith("PASS")) Finish(null, 0);
    }

    private static void Log(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || message.Contains("PROGRESSION TEST FAILED")) Finish("FAIL: " + message + "\n" + stack, 1);
    }

    private static void Finish(string message, int code)
    {
        SessionState.SetBool("GuidanceBatchTest", false);
        if (message != null) File.WriteAllText("ProgressionTestResult.txt", message);
        EditorApplication.Exit(code);
    }
}
