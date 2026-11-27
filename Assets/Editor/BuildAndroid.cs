using System.IO;
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
        // Permanent app identity + release signing. The keystore lives OUTSIDE the repo (~/keys, back it up: if it is lost,
        // no update can ever install over existing copies). Password: env SERENITY_KEYPASS or ~/keys/serenity-release.pass.
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "se.vibing.serenity");
        var keys = Path.Combine(System.Environment.GetEnvironmentVariable("HOME") ?? "", "keys");
        var ks = System.Environment.GetEnvironmentVariable("SERENITY_KEYSTORE") ?? Path.Combine(keys, "serenity-release.jks");
        var pw = System.Environment.GetEnvironmentVariable("SERENITY_KEYPASS");
        var pwFile = Path.Combine(keys, "serenity-release.pass");
        if (string.IsNullOrEmpty(pw) && File.Exists(pwFile)) pw = File.ReadAllText(pwFile).Trim();
        if (!File.Exists(ks) || string.IsNullOrEmpty(pw))
        {
            Debug.LogError("Release keystore or password missing (" + ks + "): refusing to build with the debug key.");
            EditorApplication.Exit(1); return;
        }
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = ks; PlayerSettings.Android.keystorePass = pw;
        PlayerSettings.Android.keyaliasName = "serenity"; PlayerSettings.Android.keyaliasPass = pw;
        // Android refuses updates with a lower version code: it follows the build number.
        PlayerSettings.Android.bundleVersionCode = BuildInfo.Number;
        PlayerSettings.bundleVersion = "0.1." + BuildInfo.Number;
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/GameplayScene.unity" },
            locationPathName = "Builds/project-serenity-release.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None, // release build (no "Development Build" watermark)
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build result: " + report.summary.result);
        EditorApplication.Exit(report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
    }
}
