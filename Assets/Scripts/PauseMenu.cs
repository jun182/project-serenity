using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pause menu built entirely in code; spawns itself in GameplayScene (no scene edits needed).
// Open: on-screen button, Esc / Android Back, controller Start.
// Tilt steering (accelerometer). Untested on a real phone: tune DeadZone/FullTilt, "Invert" flips direction.
public static class GyroSettings
{
    public const float DeadZone = 0.06f; // ~3.5 degrees
    public const float FullTilt = 0.40f; // ~23 degrees = full speed
    public static bool Enabled
    {
        get { return PlayerPrefs.GetInt("gyroEnabled", 0) == 1; }
        set { PlayerPrefs.SetInt("gyroEnabled", value ? 1 : 0); PlayerPrefs.Save(); }
    }
    public static bool Invert
    {
        get { return PlayerPrefs.GetInt("gyroInvert", 0) == 1; }
        set { PlayerPrefs.SetInt("gyroInvert", value ? 1 : 0); PlayerPrefs.Save(); }
    }
    public static float Steer()
    {
        // Android reports Input.acceleration already rotated to the screen: x = left/right tilt (steering-wheel roll).
        // (Earlier version used y, which on the phone was forward/back tilt.)
        float tilt = Input.acceleration.x;
        if (Invert) tilt = -tilt;
        float a = Mathf.Abs(tilt);
        if (a < DeadZone) return 0f;
        return Mathf.Sign(tilt) * Mathf.Clamp01((a - DeadZone) / (FullTilt - DeadZone));
    }
}

public class PauseMenu : MonoBehaviour
{
    public const string DebugPrefKey = "debugPanelVisible"; // 0 = hidden (default), 1 = shown

    GameObject panel;
    Text debugLabel, gyroLabel, invertLabel, rateLabel;
    public static bool IsPaused { get; private set; }
    static readonly int[] FrameRates = { 30, 60, 90, 120 };
    public static int FrameRate
    {
        get { return PlayerPrefs.GetInt("targetFps", 60); }
        set { PlayerPrefs.SetInt("targetFps", value); PlayerPrefs.Save(); Application.targetFrameRate = value; }
    }
    Text fpsCounter;
    float fpsAccum; int fpsFrames;

    // RuntimeInitializeOnLoadMethod runs once at app start, so hook sceneLoaded to also catch later scene loads.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        // Unity on Android defaults to 30 fps unless set; default 60, cycle 30/60/90/120 in the pause menu.
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FrameRate;
        SceneManager.sceneLoaded += (scene, mode) => Spawn(scene.name);
        // Diagnostics: keep the last runtime error/exception for the debug overlay (no adb to the phone).
        Application.logMessageReceived += (msg, stack, type) =>
        {
            if (type != LogType.Exception && type != LogType.Error) return;
            ErrCount++;
            string where = stack.Split('\n')[0];
            LastErr = (msg + " @ " + where).Replace("\n", " ");
            if (LastErr.Length > 120) LastErr = LastErr.Substring(0, 120);
        };
    }
    static int ErrCount; static string LastErr = "";
    Image healthFill;

    static void Spawn(string n)
    {
        if (n != "GameplayScene" && n != "MainMenu") return;
        new GameObject("PauseMenu").AddComponent<PauseMenu>();
    }

    void Awake()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasGo = new GameObject("PauseCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        bool mainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        if (mainMenu)
        {
            // Main menu: only a gyro toggle (bottom-left).
            var g = MakeButton(canvasGo.transform, "", new Vector2(0, 0), new Vector2(230, 80), new Vector2(420, 90), ToggleGyro);
            gyroLabel = g.GetComponentInChildren<Text>();
            gyroLabel.fontSize = 36;
            RefreshLabels();
            return;
        }

        // Health bar: full-width strip at the bottom, black background, green part = health / 100 from the left.
        // raycastTarget off so touches on it still steer.
        var hbBack = new GameObject("HealthBar", typeof(Image));
        hbBack.transform.SetParent(canvasGo.transform, false);
        var hbRt = hbBack.GetComponent<RectTransform>();
        // Height = 2.5% of screen height (anchor-based, independent of resolution; overlay UI, so camera zoom doesn't affect it).
        hbRt.anchorMin = new Vector2(0, 0); hbRt.anchorMax = new Vector2(1, 0.025f);
        hbRt.offsetMin = hbRt.offsetMax = Vector2.zero;
        var hbBackImg = hbBack.GetComponent<Image>(); hbBackImg.color = Color.black; hbBackImg.raycastTarget = false;
        var hbFill = new GameObject("HealthFill", typeof(Image));
        hbFill.transform.SetParent(hbBack.transform, false);
        var hfRt = hbFill.GetComponent<RectTransform>();
        hfRt.anchorMin = Vector2.zero; hfRt.anchorMax = Vector2.one; hfRt.offsetMin = hfRt.offsetMax = Vector2.zero;
        healthFill = hbFill.GetComponent<Image>(); healthFill.color = new Color(0.1f, 0.85f, 0.2f); healthFill.raycastTarget = false;

        // Pause button, top-left.
        var pauseBtn = MakeButton(canvasGo.transform, "II", new Vector2(0, 1), new Vector2(90, -90), new Vector2(120, 120), Pause);

        // Menu panel (dim background + 3 buttons).
        panel = new GameObject("PausePanel", typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        var center = new Vector2(0.5f, 0.5f);
        MakeButton(panel.transform, "Resume", center, new Vector2(0, 360), new Vector2(620, 100), Resume);
        gyroLabel = MakeButton(panel.transform, "", center, new Vector2(0, 240), new Vector2(620, 100), ToggleGyro).GetComponentInChildren<Text>();
        invertLabel = MakeButton(panel.transform, "", center, new Vector2(0, 120), new Vector2(620, 100), ToggleInvert).GetComponentInChildren<Text>();
        rateLabel = MakeButton(panel.transform, "", center, new Vector2(0, 0), new Vector2(620, 100), CycleRate).GetComponentInChildren<Text>();
        debugLabel = MakeButton(panel.transform, "", center, new Vector2(0, -120), new Vector2(620, 100), ToggleDebug).GetComponentInChildren<Text>();
        MakeButton(panel.transform, "Quit to menu", center, new Vector2(0, -240), new Vector2(620, 100), QuitToMenu);
        MakeButton(panel.transform, "Quit game", center, new Vector2(0, -360), new Vector2(620, 100), QuitApp);
        // FPS counter, top-right, shown only while the debug panel is on.
        var fgo = new GameObject("FpsCounter", typeof(Text));
        fgo.transform.SetParent(canvasGo.transform, false);
        var frt = fgo.GetComponent<RectTransform>();
        frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(1, 1);
        frt.anchoredPosition = new Vector2(-30, -20); frt.sizeDelta = new Vector2(900, 200);
        fpsCounter = fgo.GetComponent<Text>();
        fpsCounter.alignment = TextAnchor.UpperRight; fpsCounter.fontSize = 34; fpsCounter.color = Color.yellow;
        fpsCounter.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        panel.SetActive(false);
        RefreshLabels();
    }

    Button MakeButton(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(text + "Button", typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor.y > 0.9f ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        go.GetComponent<Button>().onClick.AddListener(onClick);
        var tgo = new GameObject("Text", typeof(Text));
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        var t = tgo.GetComponent<Text>();
        t.text = text; t.alignment = TextAnchor.MiddleCenter; t.fontSize = 48; t.color = Color.white;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go.GetComponent<Button>();
    }

    void Update()
    {
        if (healthFill != null)
        {
            var hp = FindObjectOfType<PlayerController>();
            float f = hp == null ? 0f : Mathf.Clamp01(hp.health / PlayerController.HealthCap);
            healthFill.rectTransform.anchorMax = new Vector2(f, 1f);
        }
        bool showFps = PlayerPrefs.GetInt(DebugPrefKey, 0) == 1;
        if (fpsCounter != null)
        {
            fpsCounter.enabled = showFps;
            fpsAccum += Time.unscaledDeltaTime; fpsFrames++;
            if (fpsAccum >= 0.5f)
            {
                var pc = FindObjectOfType<PlayerController>();
                string diag = pc == null ? "" : "\nbuild b4 " + PlayerController.DiagMusic + " trig " + PlayerController.DiagTrig + " hp " + Mathf.RoundToInt(pc.health) + " heal " + pc.IsHealing + " x " + pc.transform.position.x.ToString("0.0")
                    + "\nhits " + PlayerController.DiagHits + " serIn " + PlayerController.DiagSerEnter + " serOut " + PlayerController.DiagSerExit + " " + PlayerController.DiagTheme
                    + "\ndead " + pc.IsDead + " err " + ErrCount + (ErrCount > 0 ? "\n" + LastErr : "");
                fpsCounter.text = Mathf.RoundToInt(fpsFrames / fpsAccum) + " fps" + diag;
                fpsAccum = 0f; fpsFrames = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (IsPaused) Resume(); else Pause();
        }
    }

    void Pause() { IsPaused = true; Time.timeScale = 0f; panel.SetActive(true); }
    void Resume() { IsPaused = false; Time.timeScale = 1f; panel.SetActive(false); }
    // Application.Quit() leaves the task in recents on Android, so Back reopens the app: finish and remove the task.
    public static void QuitApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var act = up.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                act.Call("finishAndRemoveTask");
                return;
            }
        }
        catch (System.Exception) { }
#endif
        Application.Quit();
    }

    void QuitToMenu() { IsPaused = false; Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }

    void ToggleDebug()
    {
        PlayerPrefs.SetInt(DebugPrefKey, PlayerPrefs.GetInt(DebugPrefKey, 0) == 1 ? 0 : 1);
        PlayerPrefs.Save();
        RefreshLabels();
    }

    void CycleRate()
    {
        int i = System.Array.IndexOf(FrameRates, FrameRate);
        FrameRate = FrameRates[(i + 1) % FrameRates.Length];
        RefreshLabels();
    }

    void ToggleGyro() { GyroSettings.Enabled = !GyroSettings.Enabled; RefreshLabels(); }
    void ToggleInvert() { GyroSettings.Invert = !GyroSettings.Invert; RefreshLabels(); }

    void RefreshLabels()
    {
        if (gyroLabel) gyroLabel.text = "Tilt steering: " + (GyroSettings.Enabled ? "ON" : "OFF");
        if (rateLabel) rateLabel.text = "Frame rate: " + FrameRate;
        if (invertLabel) invertLabel.text = "Invert tilt: " + (GyroSettings.Invert ? "ON" : "OFF");
        if (debugLabel) debugLabel.text = "Debug panel: " + (PlayerPrefs.GetInt(DebugPrefKey, 0) == 1 ? "ON" : "OFF");
    }

    void OnDestroy() { IsPaused = false; Time.timeScale = 1f; }
}
