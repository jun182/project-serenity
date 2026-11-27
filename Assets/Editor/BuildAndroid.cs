using UnityEditor;
using UnityEngine;

// Headless build: Unity -batchmode -quit -executeMethod BuildAndroid.Build
public static class BuildAndroid
{
    public static void Build()
    {
        // Modern phones (e.g. Pixel 9) are 64-bit only: needs IL2CPP + ARM64 only (32-bit dropped on purpose).
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        // Landscape only (both sides), no portrait.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameplayScene.unity" },
            locationPathName = "Builds/project-serenity-debug.apk",
            target = BuildTarget.Android,
            options = BuildOptions.Development,
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build result: " + report.summary.result);
        EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
    }
}
