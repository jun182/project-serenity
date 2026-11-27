using UnityEngine;

// Player options and debug tunables, all stored in PlayerPrefs so they can be changed on the phone without a rebuild.
public static class GameSettings
{
    public enum Mode { Progressive = 0, Endless = 1 }

    static Mode? gameMode;
    public static Mode GameMode
    {
        get { if (!gameMode.HasValue) gameMode = (Mode)PlayerPrefs.GetInt("gameMode", 0); return gameMode.Value; }
        set { PlayerPrefs.SetInt("gameMode", (int)value); gameMode = value; PlayerPrefs.Save(); }
    }

    static bool? screenShake;
    public static bool ScreenShake
    {
        get { if (!screenShake.HasValue) screenShake = PlayerPrefs.GetInt("screenShake", 1) == 1; return screenShake.Value; }
        set { PlayerPrefs.SetInt("screenShake", value ? 1 : 0); screenShake = value; PlayerPrefs.Save(); }
    }

    // Endless mode obstacle amount, 1 = the original rate (one obstacle per 0.15 s).
    public static float EndlessDensity { get { return Get("endlessDensity"); } set { Set("endlessDensity", value); } }

    // name, default, min, max, label. Debug tunables listed here appear as sliders in the pause menu's debug column.
    // step: sliders snap to multiples of it, so defaults (and round values) can be hit exactly.
    public struct Tunable { public string key, label; public float def, min, max, step; }
    public static readonly Tunable[] Tunables =
    {
        T("levelSeconds", "Seconds per level", 30, 3, 60, 1),
        T("winSeconds", "Win countdown (s)", 30, 3, 60, 1),
        T("rateMult", "Spawn rate x (all modes)", 1, 0.25f, 3, 0.05f),
        T("obstacleSpeed", "Obstacle speed x", 1, 0.5f, 2, 0.05f),
        T("spawnMaxX", "Spawn max x (border=5)", 5, 2, 5, 0.1f),
        T("damage", "Damage per hit", 10, 0, 50, 1),
        T("healSpeed", "Heal per second", 2, 0, 20, 0.5f),
        T("steerSpeed", "Steer speed x", 1, 0.5f, 2, 0.05f),
        T("steerAccel", "Steer response x", 1, 0.5f, 3, 0.1f),
        T("shakeStrength", "Shake strength", 1, 0, 3, 0.1f),
        T("breatheZoom", "Breathing zoom %", 1.5f, 0, 6, 0.1f),
        T("maxTimeInLight", "Plain track time (s)", 0, 0, 30, 1),
        T("nearMissDist", "Near-miss distance", 1.5f, 0.5f, 4, 0.1f),
        T("nearMissBuzz", "Near-miss vibration (0-255)", 160, 0, 255, 5),
        T("nearMissSlide", "Near-miss slide (minus = flip)", 0.35f, -1.5f, 1.5f, 0.05f),
    };
    static Tunable T(string k, string l, float d, float min, float max, float step) { return new Tunable { key = k, label = l, def = d, min = min, max = max, step = step }; }

    // Cached: obstacles read these every frame and PlayerPrefs on Android is slow.
    static readonly System.Collections.Generic.Dictionary<string, float> cache = new System.Collections.Generic.Dictionary<string, float>();
    public static float Default(string key)
    {
        foreach (var t in Tunables) if (t.key == key) return t.def;
        return key == "endlessDensity" ? 1f : 0f;
    }
    public static float Get(string key)
    {
        float v;
        if (!cache.TryGetValue(key, out v)) { v = PlayerPrefs.GetFloat("t_" + key, Default(key)); cache[key] = v; }
        return v;
    }
    // No Save() here (sliders call this every drag frame); PauseMenu saves on resume/quit.
    public static void Set(string key, float v) { cache[key] = v; PlayerPrefs.SetFloat("t_" + key, v); }
    public static void Reset(string key) { PlayerPrefs.DeleteKey("t_" + key); cache.Remove(key); }
    public static void ResetTunables() { foreach (var t in Tunables) PlayerPrefs.DeleteKey("t_" + t.key); cache.Clear(); PlayerPrefs.Save(); }

    public static bool Invincible
    {
        get { return PlayerPrefs.GetInt("invincible", 0) == 1; }
        set { PlayerPrefs.SetInt("invincible", value ? 1 : 0); PlayerPrefs.Save(); }
    }
    public static bool ZoomMarkers
    {
        get { return PlayerPrefs.GetInt("zoomMarkers", 0) == 1; }
        set { PlayerPrefs.SetInt("zoomMarkers", value ? 1 : 0); PlayerPrefs.Save(); }
    }
}
