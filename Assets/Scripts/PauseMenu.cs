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
    Text debugLabel, gyroLabel, invertLabel, rateLabel, modeLabel, shakeLabel, invLabel, markLabel;
    GameObject tuningPanel, helpPanel; readonly System.Collections.Generic.List<GameObject> debugOnly = new System.Collections.Generic.List<GameObject>();
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
            var mb = MakeButton(canvasGo.transform, "", new Vector2(1, 0), new Vector2(-230, 80), new Vector2(420, 90), ToggleMode);
            modeLabel = mb.GetComponentInChildren<Text>(); modeLabel.fontSize = 36;
            MakeButton(canvasGo.transform, "How to play", new Vector2(1, 1), new Vector2(-230, -80), new Vector2(420, 90), () => helpPanel.SetActive(true))
                .GetComponentInChildren<Text>().fontSize = 36;
            BuildHelpPanel(canvasGo.transform);
            // Update button (top-left, hidden until Updater finds a newer release).
            var ub = MakeButton(canvasGo.transform, "", new Vector2(0, 1), new Vector2(330, -80), new Vector2(600, 90), () => { });
            ub.GetComponentInChildren<Text>().fontSize = 36;
            gameObject.AddComponent<Updater>().Init(ub, ub.GetComponentInChildren<Text>());
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
        MakeButton(panel.transform, "Resume", center, new Vector2(-340, 360), new Vector2(620, 100), Resume);
        gyroLabel = MakeButton(panel.transform, "", center, new Vector2(-340, 240), new Vector2(620, 100), ToggleGyro).GetComponentInChildren<Text>();
        invertLabel = MakeButton(panel.transform, "", center, new Vector2(-340, 120), new Vector2(620, 100), ToggleInvert).GetComponentInChildren<Text>();
        rateLabel = MakeButton(panel.transform, "", center, new Vector2(-340, 0), new Vector2(620, 100), CycleRate).GetComponentInChildren<Text>();
        debugLabel = MakeButton(panel.transform, "", center, new Vector2(-340, -120), new Vector2(620, 100), ToggleDebug).GetComponentInChildren<Text>();
        MakeButton(panel.transform, "Quit to menu", center, new Vector2(-340, -240), new Vector2(620, 100), QuitToMenu);
        MakeButton(panel.transform, "Quit game", center, new Vector2(-340, -360), new Vector2(620, 100), QuitApp);
        // Right column: options, then debug-only tools (shown while Debug panel is ON).
        shakeLabel = MakeButton(panel.transform, "", center, new Vector2(340, 360), new Vector2(620, 100), ToggleShake).GetComponentInChildren<Text>();
        MakeSlider(panel.transform, new Vector2(340, 205), 560, "endlessDensity", 0.5f, 3f, 0.05f, "Obstacles (Endless mode)");
        var dbg = new[] {
            MakeButton(panel.transform, "Kill me (test death)", center, new Vector2(340, -25), new Vector2(620, 90), DebugKill),
            MakeButton(panel.transform, "", center, new Vector2(340, -125), new Vector2(620, 90), ToggleInvincible),
            MakeButton(panel.transform, "Difficulty level +1", center, new Vector2(340, -225), new Vector2(620, 90), DebugLevelUp),
            MakeButton(panel.transform, "", center, new Vector2(340, -325), new Vector2(620, 90), ToggleMarkers),
            MakeButton(panel.transform, "Tuning sliders...", center, new Vector2(340, -425), new Vector2(620, 90), () => tuningPanel.SetActive(true)) };
        invLabel = dbg[1].GetComponentInChildren<Text>(); markLabel = dbg[3].GetComponentInChildren<Text>();
        foreach (var b in dbg) debugOnly.Add(b.gameObject);
        MakeButton(panel.transform, "How to play", center, new Vector2(340, 80), new Vector2(620, 90), () => helpPanel.SetActive(true));
        BuildTuningPanel(canvasGo.transform);
        BuildHelpPanel(canvasGo.transform);
        // FPS counter, top-right, shown only while the debug panel is on.
        var fgo = new GameObject("FpsCounter", typeof(Text));
        fgo.transform.SetParent(canvasGo.transform, false);
        var frt = fgo.GetComponent<RectTransform>();
        frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(1, 1);
        frt.anchoredPosition = new Vector2(-30, -20); frt.sizeDelta = new Vector2(1100, 420);
        fpsCounter = fgo.GetComponent<Text>();
        fpsCounter.alignment = TextAnchor.UpperRight; fpsCounter.fontSize = 34; fpsCounter.color = Color.yellow;
        fpsCounter.font = UiFont.Get();
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
        t.font = UiFont.Get();
        return go.GetComponent<Button>();
    }

    void Update()
    {
        if (healthFill != null)
        {
            var hp = PlayerController.Instance;
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
                var pc = PlayerController.Instance;
                string diag = pc == null ? "" : "\nbuild " + BuildInfo.Label + " " + PlayerController.DiagMusic + " trig " + PlayerController.DiagTrig + " hp " + Mathf.RoundToInt(pc.health) + " heal " + pc.IsHealing + " x " + pc.transform.position.x.ToString("0.0")
                    + "\nhits " + PlayerController.DiagHits + " serIn " + PlayerController.DiagSerEnter + " serOut " + PlayerController.DiagSerExit + " " + PlayerController.DiagTheme
                    + "\ndead " + pc.IsDead + " err " + ErrCount + " shake " + CameraShake.Trauma.ToString("0.00")
                    + (GameDirector.Instance == null ? "" : "\n" + GameSettings.GameMode + " lvl " + GameDirector.Instance.Level.ToString("0.00")
                        + " rate x" + GameDirector.Instance.RateMultiplier.ToString("0.00") + " spawned " + GameDirector.Instance.Spawned
                        + " near " + GameDirector.Instance.NearMisses)
                    + (ErrCount > 0 ? "\n" + LastErr : "");
                fpsCounter.text = Mathf.RoundToInt(fpsFrames / fpsAccum) + " fps" + diag;
                fpsAccum = 0f; fpsFrames = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (IsPaused) Resume(); else Pause();
        }
    }

    void Pause() { if (panel == null) return; /* start screen has no pause panel */ IsPaused = true; Time.timeScale = 0f; panel.SetActive(true); }
    void Resume() { IsPaused = false; Time.timeScale = 1f; panel.SetActive(false); if (tuningPanel) tuningPanel.SetActive(false); if (helpPanel) helpPanel.SetActive(false); PlayerPrefs.Save(); }
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

    void QuitToMenu() { PlayerPrefs.Save(); IsPaused = false; Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }

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
        if (modeLabel) modeLabel.text = "Mode: " + GameSettings.GameMode;
        if (shakeLabel) shakeLabel.text = "Screen shake: " + (GameSettings.ScreenShake ? "ON" : "OFF");
        if (invLabel) invLabel.text = "Invincible: " + (GameSettings.Invincible ? "ON" : "OFF");
        if (markLabel) markLabel.text = "Zoom test cubes (next run): " + (GameSettings.ZoomMarkers ? "ON" : "OFF");
        bool dbgOn = PlayerPrefs.GetInt(DebugPrefKey, 0) == 1;
        foreach (var g in debugOnly) g.SetActive(dbgOn);
        if (debugLabel) debugLabel.text = "Debug panel: " + (PlayerPrefs.GetInt(DebugPrefKey, 0) == 1 ? "ON" : "OFF");
    }

    void OnDestroy() { IsPaused = false; Time.timeScale = 1f; }

    void ToggleMode() { GameSettings.GameMode = GameSettings.GameMode == GameSettings.Mode.Progressive ? GameSettings.Mode.Endless : GameSettings.Mode.Progressive; RefreshLabels(); }
    void ToggleShake() { GameSettings.ScreenShake = !GameSettings.ScreenShake; RefreshLabels(); }
    void ToggleInvincible() { GameSettings.Invincible = !GameSettings.Invincible; RefreshLabels(); }
    void ToggleMarkers() { GameSettings.ZoomMarkers = !GameSettings.ZoomMarkers; RefreshLabels(); }
    void DebugKill() { if (PlayerController.Instance) PlayerController.Instance.DebugKill(); Resume(); }
    void DebugLevelUp() { if (GameDirector.Instance) GameDirector.Instance.SkipLevel(); }

    const string HelpText =
        "<b>How to play</b>\n\n" +
        "<b>Steer:</b> hold the left or right half of the screen to move left or right.\n" +
        "(Or tilt the phone if Tilt steering is ON in the menu. Keyboard: A / D, Esc = pause.)\n\n" +
        "<b>White side (left):</b> obstacles fall toward you. Every hit costs health:\n" +
        "the green bar at the bottom. When it is empty, you die.\n\n" +
        "<b>Black side (right), Serenity:</b> you slowly heal, but dark blockers push you back out.\n\n" +
        "<b>Progressive mode</b> (default): it gets harder over time. At the top level,\n" +
        "survive the countdown to win.\n" +
        "<b>Endless mode:</b> survive as long as you can. Set the obstacle amount in the pause menu.\n" +
        "Change mode on the start screen (bottom right). Pause: the II button, top left.";

    void BuildHelpPanel(Transform canvas)
    {
        helpPanel = new GameObject("HelpPanel", typeof(Image));
        helpPanel.transform.SetParent(canvas, false);
        var rt = helpPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        helpPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.92f);
        var tgo = new GameObject("HelpText", typeof(Text));
        tgo.transform.SetParent(helpPanel.transform, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f); trt.anchoredPosition = new Vector2(0, 60); trt.sizeDelta = new Vector2(1700, 820);
        var t = tgo.GetComponent<Text>();
        t.font = UiFont.Get(); t.fontSize = 40; t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft; t.supportRichText = true; t.raycastTarget = false; t.text = HelpText;
        MakeButton(helpPanel.transform, "Close", new Vector2(0.5f, 0.5f), new Vector2(0, -440), new Vector2(560, 100), () => helpPanel.SetActive(false));
        helpPanel.SetActive(false);
    }

    // Debug tuning page: one slider per GameSettings.Tunables entry, three columns, values kept in PlayerPrefs.
    // Layout fits the Pixel 9 Pro XL canvas (about 2150 x 965 units at this scaler): x within +-990, y within +-445.
    void BuildTuningPanel(Transform canvas)
    {
        tuningPanel = new GameObject("TuningPanel", typeof(Image));
        tuningPanel.transform.SetParent(canvas, false);
        var rt = tuningPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        tuningPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        var tun = GameSettings.Tunables;
        var refresh = new System.Collections.Generic.List<System.Action>();
        int perCol = Mathf.CeilToInt(tun.Length / 3f);
        float rowGap = Mathf.Min(150f, 600f / Mathf.Max(1, perCol - 1));
        for (int i = 0; i < tun.Length; i++)
        {
            int col = i / perCol, row = i % perCol;
            refresh.Add(MakeSlider(tuningPanel.transform, new Vector2(-680 + col * 680, 330 - row * rowGap), 600, tun[i].key, tun[i].min, tun[i].max, tun[i].step, tun[i].label));
        }
        var c = new Vector2(0.5f, 0.5f);
        MakeButton(tuningPanel.transform, "Reset all to defaults", c, new Vector2(-330, -400), new Vector2(560, 90), () =>
        {
            GameSettings.ResetTunables();
            foreach (var r in refresh) r();
        });
        MakeButton(tuningPanel.transform, "Back", c, new Vector2(330, -400), new Vector2(560, 90), () => { tuningPanel.SetActive(false); PlayerPrefs.Save(); });
        tuningPanel.SetActive(false);
    }

    // Returns an action that re-reads the stored value into the slider (after a reset).
    // Values snap to `step`; the small "Default" button resets just this one.
    System.Action MakeSlider(Transform parent, Vector2 pos, float width, string key, float min, float max, float step, string label)
    {
        var lgo = new GameObject(label + "Label", typeof(Text));
        lgo.transform.SetParent(parent, false);
        var lrt = lgo.GetComponent<RectTransform>();
        lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f); lrt.anchoredPosition = pos + new Vector2(-80, 45); lrt.sizeDelta = new Vector2(width - 160, 50);
        var lt = lgo.GetComponent<Text>();
        lt.font = UiFont.Get(); lt.fontSize = 30; lt.color = Color.white; lt.alignment = TextAnchor.MiddleLeft;
        lt.resizeTextForBestFit = true; lt.resizeTextMinSize = 18; lt.resizeTextMaxSize = 30;
        lt.raycastTarget = false;
        var defBtn = MakeButton(parent, "Default", new Vector2(0.5f, 0.5f), pos + new Vector2(width * 0.5f - 70, 45), new Vector2(140, 50), () => { });
        defBtn.GetComponentInChildren<Text>().fontSize = 26;

        var go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(width, 40);
        // No sprites in DefaultControls.Resources: colour the plain images so the parts are distinguishable.
        foreach (var img in go.GetComponentsInChildren<Image>())
        {
            if (img.name == "Background") img.color = new Color(0.3f, 0.3f, 0.3f);
            else if (img.name == "Fill") img.color = new Color(0.1f, 0.75f, 0.25f);
            else if (img.name == "Handle") { img.color = Color.white; img.rectTransform.sizeDelta = new Vector2(44, 20); }
        }
        var s = go.GetComponent<Slider>();
        s.minValue = min; s.maxValue = max;
        System.Action<float> show = v => lt.text = label + ": " + v.ToString("0.##") + (Mathf.Abs(v - GameSettings.Default(key)) < step * 0.5f ? " (default)" : "");
        System.Action load = () => { float v = GameSettings.Get(key); s.SetValueWithoutNotify(v); show(v); };
        load();
        s.onValueChanged.AddListener(v =>
        {
            v = Mathf.Clamp(Mathf.Round(v / step) * step, min, max);
            s.SetValueWithoutNotify(v);
            GameSettings.Set(key, v); show(v);
        });
        defBtn.onClick.AddListener(() => { GameSettings.Reset(key); load(); });
        return load;
    }
}
