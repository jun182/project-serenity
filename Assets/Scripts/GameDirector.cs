using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Per-run game state (created fresh for every GameplayScene load): survival timer, difficulty, win.
// Progressive: difficulty 0..5 rises continuously (one level per "Seconds per level"); level 2 = the original spawn rate.
// At level 5 a win countdown starts; surviving it wins. Endless: fixed rate x the Obstacles slider.
public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    // Spawn-rate multiplier per difficulty level (index = level), interpolated between levels. Level 2 = 1 (original rate).
    static readonly float[] LevelRate = { 0.4f, 0.65f, 1f, 1.4f, 1.85f, 2.3f };
    public const int MaxLevel = 5;

    public float Elapsed { get; private set; }
    public float Level { get; private set; }       // continuous 0..5 (Progressive only)
    public float WinLeft { get; private set; } = -1f; // seconds left of the win countdown, -1 = not started
    public bool Won { get; private set; }
    public bool Progressive { get; private set; }
    public int Spawned; public int NearMisses;

    Text timerText, bigText;
    float wonAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (s, m) => { if (s.name == "GameplayScene") new GameObject("GameDirector").AddComponent<GameDirector>(); };
    }

    void Awake()
    {
        Instance = this;
        Progressive = GameSettings.GameMode == GameSettings.Mode.Progressive;
        var cgo = new GameObject("HudCanvas", typeof(Canvas), typeof(CanvasScaler));
        cgo.transform.SetParent(transform, false);
        var c = cgo.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = 900; // under the pause menu (1000)
        var sc = cgo.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = 0.5f;
        timerText = MakeText(cgo.transform, new Vector2(0.5f, 1f), new Vector2(0, -60), 64);
        bigText = MakeText(cgo.transform, new Vector2(0.5f, 0.5f), Vector2.zero, 110);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    static Text MakeText(Transform parent, Vector2 anchor, Vector2 pos, int size)
    {
        var go = new GameObject("HudText", typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(1400, 300);
        var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = new Color(1f, 0.35f, 0.1f);
        t.raycastTarget = false;
        // Outline keeps it readable on both the white and the black half.
        var o = go.AddComponent<Outline>(); o.effectColor = Color.black; o.effectDistance = new Vector2(3, -3);
        return t;
    }

    bool PlayerDead { get { return PlayerController.Instance != null && PlayerController.Instance.IsDead; } }

    // Multiplier on the scene's base spawn rate.
    public float RateMultiplier
    {
        get
        {
            float m = GameSettings.Get("rateMult");
            if (!Progressive) return m * GameSettings.EndlessDensity;
            int i = Mathf.Min(Mathf.FloorToInt(Level), MaxLevel - 1);
            return m * Mathf.Lerp(LevelRate[i], LevelRate[i + 1], Mathf.Clamp01(Level - i));
        }
    }

    public bool SpawningStopped { get { return Won || PlayerDead; } }

    void Update()
    {
        if (Won)
        {
            if (Time.unscaledTime - wonAt > 4f) SceneManager.LoadScene("MainMenu");
            return;
        }
        if (!PlayerDead) Elapsed += Time.deltaTime;
        if (Progressive)
        {
            Level = Mathf.Min(MaxLevel, Elapsed / Mathf.Max(1f, GameSettings.Get("levelSeconds")));
            if (Level >= MaxLevel && !PlayerDead)
            {
                if (WinLeft < 0f) WinLeft = GameSettings.Get("winSeconds");
                WinLeft -= Time.deltaTime;
                if (WinLeft <= 0f) { WinLeft = 0f; Won = true; wonAt = Time.unscaledTime; bigText.text = "you wonned"; }
            }
        }
        timerText.text = Fmt(Elapsed) + (WinLeft >= 0f && !Won ? "\nsurvive " + Mathf.CeilToInt(WinLeft) : "");
    }

    static string Fmt(float s) { int t = Mathf.FloorToInt(s); return (t / 60) + ":" + (t % 60).ToString("00"); }

    // Debug: jump to the start of the next level.
    public void SkipLevel()
    {
        if (!Progressive) return;
        float ls = Mathf.Max(1f, GameSettings.Get("levelSeconds"));
        Elapsed = Mathf.Min(MaxLevel, Mathf.Floor(Level) + 1f) * ls;
    }
}
