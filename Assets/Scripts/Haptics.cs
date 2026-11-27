using UnityEngine;

// Android vibration with amplitude (Handheld.Vibrate is fixed and strong). Untested on the phone.
public static class Haptics
{
#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject vibrator;
    static AndroidJavaClass effectClass;
    static int sdk;
    static bool init, ok;

    static void Init()
    {
        init = true;
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
            using (var v = new AndroidJavaClass("android.os.Build$VERSION")) sdk = v.GetStatic<int>("SDK_INT");
            if (sdk >= 26) effectClass = new AndroidJavaClass("android.os.VibrationEffect");
            ok = vibrator != null;
        }
        catch (System.Exception e) { Debug.Log("Haptics init failed: " + e.Message); ok = false; }
    }

    // timings: ms for [off, on, off, on...]; amps: 0-255 per segment (0 = off).
    static void Wave(long[] timings, int[] amps)
    {
        if (!init) Init();
        if (!ok) return;
        try
        {
            if (sdk >= 26)
                using (var eff = effectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amps, -1))
                    vibrator.Call("vibrate", eff);
            else
                vibrator.Call("vibrate", timings, -1);
        }
        catch (System.Exception e) { Debug.Log("Haptics failed: " + e.Message); }
    }
#else
    static void Wave(long[] timings, int[] amps) { }
#endif

    // Strong single thump when the player is hit.
    public static void Hit() { Wave(new long[] { 0, 90 }, new int[] { 0, 255 }); }

    // Soft double-beat "heartbeat" while healing.
    public static void Heartbeat() { Wave(new long[] { 0, 30, 110, 22 }, new int[] { 0, 90, 0, 60 }); }

    // Keeps Unity adding the VIBRATE permission to the manifest (it scans for this call); never executed.
    static void KeepPermission() { if (Time.timeScale < -1f) Handheld.Vibrate(); }
}
