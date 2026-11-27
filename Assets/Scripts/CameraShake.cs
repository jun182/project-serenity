using UnityEngine;

// Added to the main camera by CameraZoom.Apply (after the zoom, so the zoomed view is the base).
// Hit = strong shake, near miss = small shake, healing in Serenity = slow rocking + a slight zoom pulse
// that peaks on each heartbeat vibration. Everything is an offset from the stored base (no drift). Option: GameSettings.ScreenShake.
public class CameraShake : MonoBehaviour
{
    static CameraShake inst;
    Camera cam;
    Vector3 basePos; Quaternion baseRot; float baseFov, baseSize;
    float trauma, breatheW, lastBeat = -10f;
    public const float BeatPeriod = 0.9f; // same as PlayerController's heartbeat interval

    public static void Hit() { Add(0.7f); }
    public static void NearMiss() { Add(0.3f); }
    public static void Beat() { if (inst) inst.lastBeat = Time.time; }
    static void Add(float t) { if (inst && GameSettings.ScreenShake) inst.trauma = Mathf.Min(1f, inst.trauma + t); }
    public static float Trauma { get { return inst ? inst.trauma : 0f; } }

    void Start()
    {
        inst = this;
        cam = GetComponent<Camera>();
        basePos = transform.localPosition; baseRot = transform.localRotation;
        baseFov = cam.fieldOfView; baseSize = cam.orthographicSize;
    }

    void OnDestroy() { if (inst == this) inst = null; }

    void LateUpdate()
    {
        var pc = PlayerController.Instance;
        bool on = GameSettings.ScreenShake && !PauseMenu.IsPaused;
        // Follows the heartbeat haptics: fades out when beats stop (blocked, full health rules in PlayerController).
        bool breathe = on && pc != null && pc.IsHealing && !pc.IsDead && Time.time - lastBeat < BeatPeriod * 1.3f;
        breatheW = Mathf.MoveTowards(breatheW, breathe ? 1f : 0f, Time.deltaTime / 0.8f);
        trauma = Mathf.Max(0f, trauma - Time.deltaTime * 1.6f);
        if (!on) trauma = 0f;

        float s = trauma * trauma * GameSettings.Get("shakeStrength");
        float t = Time.time * 25f;
        float ox = (Mathf.PerlinNoise(t, 0.3f) - 0.5f) * 2f * 0.4f * s;
        float oy = (Mathf.PerlinNoise(0.7f, t) - 0.5f) * 2f * 0.4f * s;
        float roll = (Mathf.PerlinNoise(t, 5.1f) - 0.5f) * 2f * 3f * s;

        // Breathing: rock over two beats, zoom peaks on each beat and eases out.
        roll += Mathf.Sin(Time.time * Mathf.PI / BeatPeriod) * 0.6f * breatheW;
        float phase = Mathf.Clamp01((Time.time - lastBeat) / BeatPeriod);
        float zoom = GameSettings.Get("breatheZoom") * 0.01f * breatheW * (0.5f + 0.5f * Mathf.Cos(phase * 2f * Mathf.PI));
        transform.localPosition = basePos + baseRot * new Vector3(ox, oy, 0f);
        transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, roll);
        if (cam.orthographic) cam.orthographicSize = baseSize / (1f + zoom);
        else cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(baseFov * 0.5f * Mathf.Deg2Rad) / (1f + zoom)) * Mathf.Rad2Deg;
    }
}
