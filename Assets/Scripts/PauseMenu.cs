using System;
using UnityEngine;
using UnityEngine.Events;
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
        IsPaused = false; MenuOpen = false;
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

        inGame = SceneManager.GetActiveScene().name != "MainMenu";
        GameSpeed = 1f;
        if (!inGame)
        {
            // Start screen: one "Menu" button (top-right) opens the shared menu.
            menuBtn = MakeButton(canvasGo.transform, "Menu", new Vector2(1, 1), new Vector2(-170, -80), new Vector2(280, 90), OpenMenu);
            menuBtn.GetComponentInChildren<Text>().fontSize = 36;
            TouchOnly(menuBtn);
            startBadge = PadIcons.Pill(menuBtn.transform, "START", new Vector2(0, 0.5f), new Vector2(-95, 0), new Vector2(150, 44), 28);
            // Update button (top-left next to Exit, hidden until Updater finds a newer release).
            var ub = MakeButton(canvasGo.transform, "", new Vector2(0, 1), new Vector2(580, -80), new Vector2(600, 90), () => { });
            ub.GetComponentInChildren<Text>().fontSize = 36;
            TouchOnly(ub);
            gameObject.AddComponent<Updater>().Init(ub, ub.GetComponentInChildren<Text>());
            BuildMenu(canvasGo.transform);
            BuildHelpPanel(canvasGo.transform);
            RefreshLabels();
            gameObject.AddComponent<StartScreen>().Init(canvasGo.transform, MakeButton);
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
        menuBtn = MakeButton(canvasGo.transform, "II", new Vector2(0, 1), new Vector2(90, -90), new Vector2(120, 120), OpenMenu);
        TouchOnly(menuBtn);
        startBadge = PadIcons.Pill(menuBtn.transform, "START", new Vector2(1, 0.5f), new Vector2(95, 0), new Vector2(150, 44), 28); // II sits at the left screen edge: no room on its left
        BuildMenu(canvasGo.transform);
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
        fpsCounter.font = UiFont.Get(); fpsCounter.raycastTarget = false; // don't swallow steering touches
        RefreshLabels();
    }

    // ---- Shared menu (start screen and pause): pages main / settings / controls / debug, one centred column. ----
    bool inGame;
    GameObject menuRoot; string page = "main";
    readonly System.Collections.Generic.Dictionary<string, GameObject> pages = new System.Collections.Generic.Dictionary<string, GameObject>();
    public static float GameSpeed = 1f; // debug: Time.timeScale while playing (reset on leaving the game)
    public static bool PadActive { get; private set; } // last input was a controller: show the selection highlight
    public static bool MenuOpen { get; private set; }
    // Touch-only buttons (Menu / II / Exit / Update): hidden while a menu page is open, with a START badge in pad mode.
    readonly System.Collections.Generic.List<CanvasGroup> hideWhenMenu = new System.Collections.Generic.List<CanvasGroup>();
    GameObject startBadge; Button menuBtn;
    // Measured on the Y700 (adb KEYCODE_BUTTON_START): Unity on Android reports Start as JoystickButton10 (A=0, B=1); 7 on desktop mappings.
#if UNITY_ANDROID && !UNITY_EDITOR
    public const KeyCode StartKey = KeyCode.JoystickButton10;
#else
    public const KeyCode StartKey = KeyCode.JoystickButton7;
#endif
    int lastSleep = -99; string lastState = "";

    void TouchOnly(Button b)
    {
        var cg = b.gameObject.AddComponent<CanvasGroup>();
        hideWhenMenu.Add(cg);
        var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav;
    }

    void BuildMenu(Transform canvas)
    {
        menuRoot = new GameObject("MenuPanel", typeof(Image));
        menuRoot.transform.SetParent(canvas, false);
        var rt = menuRoot.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        menuRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        Func<GameObject, string, UnityAction, Button> item = null;
        Func<string, GameObject> newPage = n => { var p = new GameObject(n, typeof(RectTransform)); p.transform.SetParent(menuRoot.transform, false); var prt = p.GetComponent<RectTransform>(); prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero; pages[n] = p; return p; };
        float yy = 0;
        item = (pg, text, act) => { var b = MakeButton(pg.transform, text, new Vector2(0.5f, 0.5f), new Vector2(0, yy), new Vector2(700, 100), act); Brush.Register(b.GetComponent<Image>()); PadIcons.AddFx(b); yy -= 120; return b; };
        Action<GameObject> back = pg => item(pg, "Back", () => ShowPage("main"));

        // main
        var main = newPage("main"); yy = 360;
        item(main, inGame ? "Resume" : "Close", CloseMenu);
        item(main, "Settings", () => ShowPage("settings"));
        item(main, "Controls", () => ShowPage("controls"));
        item(main, "How to play", () => OpenSub(helpPanel));
        if (inGame)
        {
            item(main, "Quit to menu", QuitToMenu);
            item(main, "Quit game", QuitApp);
            debugOnly.Add(item(main, "Debug...", () => ShowPage("debug")).gameObject);
        }
        // settings
        var set = newPage("settings"); yy = 360;
        rateLabel = item(set, "", CycleRate).GetComponentInChildren<Text>();
        shakeLabel = item(set, "", ToggleShake).GetComponentInChildren<Text>();
        if (!inGame) modeLabel = item(set, "", ToggleMode).GetComponentInChildren<Text>();
        MakeSlider(set.transform, new Vector2(0, yy - 10), 640, "endlessDensity", 0.5f, 3f, 0.05f, "Obstacles (Endless mode)"); yy -= 120;
        debugLabel = item(set, "", ToggleDebug).GetComponentInChildren<Text>();
        styleLabel = item(set, "", CycleStyle).GetComponentInChildren<Text>();
        var uib = item(set, "", ToggleUiPanel); uiPanelLabel = uib.GetComponentInChildren<Text>(); debugOnly.Add(uib.gameObject);
        back(set);
        // controls
        var ctl = newPage("controls"); yy = 360;
        gyroLabel = item(ctl, "", ToggleGyro).GetComponentInChildren<Text>();
        invertLabel = item(ctl, "", ToggleInvert).GetComponentInChildren<Text>();
        back(ctl);
        // debug (in game only; the main page entry only shows while Debug is ON)
        if (inGame)
        {
            var dbg = newPage("debug"); yy = 360;
            item(dbg, "Kill me (test death)", DebugKill);
            invLabel = item(dbg, "", ToggleInvincible).GetComponentInChildren<Text>();
            item(dbg, "Difficulty level +1", DebugLevelUp);
            markLabel = item(dbg, "", ToggleMarkers).GetComponentInChildren<Text>();
            item(dbg, "Tuning sliders...", () => OpenSub(tuningPanel));
            MakeSlider(dbg.transform, new Vector2(0, yy - 10), 640, "Game speed", 0.5f, 5f, 0.5f, () => 1f, () => GameSpeed, v => GameSpeed = v, () => GameSpeed = 1f); yy -= 120;
            back(dbg);
        }
        BuildUiPanel();
        GameSettings.UiChanged = Brush.ReapplyAll;
        menuRoot.SetActive(false);
    }

    // Live UI tweak panel (debug): right side of the menu, so the menu buttons stay visible while the sliders change them.
    GameObject uiPanel;
    void BuildUiPanel()
    {
        uiPanel = new GameObject("UiTweakPanel", typeof(Image));
        uiPanel.transform.SetParent(menuRoot.transform, false);
        var rt = uiPanel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.anchoredPosition = new Vector2(680, 0); rt.sizeDelta = new Vector2(560, 800);
        uiPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        var tun = GameSettings.UiTunables;
        for (int i = 0; i < tun.Length; i++)
            MakeSlider(uiPanel.transform, new Vector2(0, 280 - i * 120), 480, tun[i].key, tun[i].min, tun[i].max, tun[i].step, tun[i].label);
    }
    void ToggleUiPanel() { PlayerPrefs.SetInt("uiPanel", PlayerPrefs.GetInt("uiPanel", 0) == 1 ? 0 : 1); PlayerPrefs.Save(); RefreshLabels(); }
    void CycleStyle() { Brush.Style = (Brush.Style + 1) % Brush.StyleNames.Length; RefreshLabels(); }
    Text styleLabel, uiPanelLabel;

    void ShowPage(string n)
    {
        page = n;
        foreach (var kv in pages) kv.Value.SetActive(kv.Key == n);
        RefreshLabels();
        var es = EventSystem.current;
        if (es != null)
        {
            es.SetSelectedGameObject(null);
            if (PadActive) { var b = pages[n].GetComponentInChildren<Button>(); if (b != null) es.SetSelectedGameObject(b.gameObject); }
        }
    }

    // Help / tuning pages replace the menu (one page visible at a time); CloseSub returns to the page they came from.
    void OpenSub(GameObject panel)
    {
        menuRoot.SetActive(false); panel.SetActive(true);
        var es = EventSystem.current;
        if (es != null) { es.SetSelectedGameObject(null); if (PadActive) { var sel = panel.GetComponentInChildren<Selectable>(); if (sel != null) es.SetSelectedGameObject(sel.gameObject); } }
    }

    void CloseSub()
    {
        bool tun = tuningPanel != null && tuningPanel.activeSelf;
        if (helpPanel) helpPanel.SetActive(false);
        if (tuningPanel) tuningPanel.SetActive(false);
        PlayerPrefs.Save();
        menuRoot.SetActive(true); ShowPage(tun ? "debug" : "main");
    }

    void OpenMenu()
    {
        if (menuRoot == null) return;
        MenuOpen = true;
        if (inGame) { IsPaused = true; Time.timeScale = 0f; }
        menuRoot.SetActive(true); ShowPage("main");
    }

    void CloseMenu()
    {
        MenuOpen = false;
        if (inGame) { IsPaused = false; Time.timeScale = GameSpeed; }
        menuRoot.SetActive(false);
        if (tuningPanel) tuningPanel.SetActive(false);
        if (helpPanel) helpPanel.SetActive(false);
        var es = EventSystem.current; if (es != null) es.SetSelectedGameObject(null);
        PlayerPrefs.Save();
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
        // Input source for the selection highlight: controller vs touch/mouse.
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) PadActive = false;
        else if (PadIcons.AnyPadInput()) PadActive = true;
        LogKeys();
        bool sub = (helpPanel != null && helpPanel.activeSelf) || (tuningPanel != null && tuningPanel.activeSelf);
        // Touch-only buttons hide while any menu page is open; START badge only in pad mode.
        bool vis = !MenuOpen && !sub;
        foreach (var cg in hideWhenMenu) { if (cg == null) continue; cg.alpha = vis ? 1f : 0f; cg.interactable = cg.blocksRaycasts = vis; }
        if (startBadge != null) startBadge.SetActive(PadActive);
        // Screen: keep awake while playing (a pad gives no touches for long stretches), system timeout on menus/pause.
        int want = (inGame && !MenuOpen) ? (int)SleepTimeout.NeverSleep : (int)SleepTimeout.SystemSetting;
        if (want != lastSleep) { lastSleep = want; Screen.sleepTimeout = want; }
        if (PadActive && (MenuOpen || sub) && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            var root = sub ? (helpPanel.activeSelf ? helpPanel : tuningPanel) : pages[page];
            var b0 = root.GetComponentInChildren<Selectable>();
            if (b0 != null) EventSystem.current.SetSelectedGameObject(b0.gameObject);
        }
        LogState(sub);
        bool back = Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1);
        bool start = Input.GetKeyDown(StartKey);
        if (sub) { if (back) CloseSub(); else if (start) CloseMenu(); return; }
        if (MenuOpen)
        {
            if (start) CloseMenu();
            else if (back) { if (page != "main") ShowPage("main"); else CloseMenu(); }
        }
        else if (inGame) { if (back || start) OpenMenu(); }
        else if (start) OpenMenu();
        else if (Input.GetKeyDown(KeyCode.Escape)) QuitApp(); // start screen: Android Back / Esc quits (pad B does nothing)
    }

    // Text log for tools/menu-check.sh: one line whenever the page or the selection changes (read with adb logcat -s Unity).
    void LogState(bool sub)
    {
        string pg = sub ? (helpPanel.activeSelf ? "help" : "tuning") : MenuOpen ? page : (inGame ? "game" : "start");
        string sel = "-";
        if (MenuOpen || sub)
        {
            var g = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (g != null) { var t = g.GetComponentInChildren<Text>(); sel = g.GetComponent<Button>() != null && t != null ? t.text : g.name; }
        }
        else if (!inGame) sel = StartScreen.Sel;
        string st = "page=" + pg + " sel=" + sel;
        if (st != lastState) { lastState = st; Debug.Log("MENU " + st); }
    }

    // Which joystick button number does the pad / adb key event produce? (b12: verify B and Start mapping)
    static void LogKeys()
    {
        for (int i = 0; i < 20; i++) if (Input.GetKeyDown(KeyCode.JoystickButton0 + i)) Debug.Log("MENU key=JoystickButton" + i);
    }

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

    void QuitToMenu() { PlayerPrefs.Save(); IsPaused = false; MenuOpen = false; GameSpeed = 1f; Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }

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
        if (styleLabel) styleLabel.text = "Button style: " + Brush.StyleNames[Brush.Style];
        if (uiPanelLabel) uiPanelLabel.text = "UI tweak panel: " + (PlayerPrefs.GetInt("uiPanel", 0) == 1 ? "ON" : "OFF");
        if (uiPanel) uiPanel.SetActive(dbgOn && PlayerPrefs.GetInt("uiPanel", 0) == 1);
        if (debugLabel) debugLabel.text = "Debug panel: " + (PlayerPrefs.GetInt(DebugPrefKey, 0) == 1 ? "ON" : "OFF");
    }

    void OnDestroy() { Screen.sleepTimeout = (int)SleepTimeout.SystemSetting; IsPaused = false; MenuOpen = false; GameSpeed = 1f; Time.timeScale = 1f; }

    void ToggleMode() { GameSettings.GameMode = GameSettings.GameMode == GameSettings.Mode.Progressive ? GameSettings.Mode.Endless : GameSettings.Mode.Progressive; RefreshLabels(); }
    void ToggleShake() { GameSettings.ScreenShake = !GameSettings.ScreenShake; RefreshLabels(); }
    void ToggleInvincible() { GameSettings.Invincible = !GameSettings.Invincible; RefreshLabels(); }
    void ToggleMarkers() { GameSettings.ZoomMarkers = !GameSettings.ZoomMarkers; RefreshLabels(); }
    void DebugKill() { if (PlayerController.Instance) PlayerController.Instance.DebugKill(); CloseMenu(); }
    void DebugLevelUp() { if (GameDirector.Instance) GameDirector.Instance.SkipLevel(); }

    const string HelpText =
        "<b>How to play</b>\n\n" +
        "<b>Steer:</b> hold the left or right half of the screen to move left or right.\n" +
        "(Or tilt the phone if Tilt steering is ON in Menu > Controls. Keyboard: A / D, Esc = pause.)\n\n" +
        "<b>White side (left):</b> obstacles fall toward you. Every hit costs health:\n" +
        "the green bar at the bottom. When it is empty, you die.\n\n" +
        "<b>Black side (right), Serenity:</b> you slowly heal, but dark blockers push you back out.\n\n" +
        "<b>Progressive mode</b> (default): it gets harder over time. At the top level,\n" +
        "survive the countdown to win.\n" +
        "<b>Endless mode:</b> survive as long as you can. Set the obstacle amount in Menu > Settings.\n" +
        "Change mode on the start screen (Menu > Settings). Pause: the II button, top left.";

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
        PadIcons.AddFx(MakeButton(helpPanel.transform, "Close", new Vector2(0.5f, 0.5f), new Vector2(0, -440), new Vector2(560, 100), CloseSub));
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
        PadIcons.AddFx(MakeButton(tuningPanel.transform, "Reset all to defaults", c, new Vector2(-330, -400), new Vector2(560, 90), () =>
        {
            GameSettings.ResetTunables();
            foreach (var r in refresh) r();
        }));
        PadIcons.AddFx(MakeButton(tuningPanel.transform, "Back", c, new Vector2(330, -400), new Vector2(560, 90), CloseSub));
        tuningPanel.SetActive(false);
    }

    // Returns an action that re-reads the stored value into the slider (after a reset).
    // Values snap to `step`; the small "Default" button resets just this one.
    System.Action MakeSlider(Transform parent, Vector2 pos, float width, string key, float min, float max, float step, string label)
    {
        return MakeSlider(parent, pos, width, label, min, max, step, () => GameSettings.Default(key), () => GameSettings.Get(key), v => GameSettings.Set(key, v), () => GameSettings.Reset(key));
    }

    System.Action MakeSlider(Transform parent, Vector2 pos, float width, string label, float min, float max, float step, Func<float> def, Func<float> get, Action<float> set, Action reset)
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
        PadIcons.AddFx(defBtn);

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
        go.name = "Slider " + label;
        PadIcons.AddFx(s);
        s.minValue = min; s.maxValue = max;
        System.Action<float> show = v => lt.text = label + ": " + v.ToString("0.##") + (Mathf.Abs(v - def()) < step * 0.5f ? " (default)" : "");
        System.Action load = () => { float v = get(); s.SetValueWithoutNotify(v); show(v); };
        load();
        s.onValueChanged.AddListener(v =>
        {
            v = Mathf.Clamp(Mathf.Round(v / step) * step, min, max);
            s.SetValueWithoutNotify(v);
            set(v); show(v);
        });
        defBtn.onClick.AddListener(() => { reset(); load(); });
        return load;
    }
}
