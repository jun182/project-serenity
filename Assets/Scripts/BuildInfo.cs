// Single source of the build number: shown in the debug overlay, used as the Android versionCode (BuildAndroid.cs)
// and compared with the GitHub release tags (alpha-bN) by Updater. Bump it for every release.
public static class BuildInfo
{
    public const int Number = 11;
    public static string Label { get { return "b" + Number; } }
}
