using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Input-adaptive start screen (spawned by PauseMenu on MainMenu). Follows the last-used input:
//  touch mode:      "Touch to start", water ripple + vibration where the finger lands, small Exit top-left.
//  controller mode: Play / Exit buttons, stick or d-pad moves the highlight, A or Start confirms.
// The scene's own Play/Exit buttons are hidden; this replaces them.
public class StartScreen : MonoBehaviour
{
    public delegate Button MakeBtn(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction onClick);

    public static bool Controller { get; private set; }

    class Catcher : MonoBehaviour, IPointerDownHandler
    {
        public System.Action<Vector2> down;
        public void OnPointerDown(PointerEventData e) { if (down != null) down(e.position); }
    }

    RectTransform canvasRt;
    Button play, exit, touchExit;
    Button[] items;
    int idx;
    GameObject hint; Text[] hintTexts; Outline[] hintOutlines;
    GameObject iconA;
    Sprite ring;
    float loadAt = -1f, prevY;
    static readonly Color Normal = new Color(0.15f, 0.15f, 0.15f, 0.9f), Selected = new Color(0.1f, 0.5f, 0.25f, 1f);

    public void Init(Transform canvas, MakeBtn make)
    {
        canvasRt = canvas as RectTransform;
        // Replace the scene's Play/Exit buttons.
        foreach (var n in new[] { "Play", "Exit" }) { var g = GameObject.Find(n); if (g != null) g.SetActive(false); }
        Controller = false;
        foreach (var j in Input.GetJoystickNames()) if (!string.IsNullOrEmpty(j)) Controller = true;

        // Full-screen touch catcher, below every button.
        var cg = new GameObject("StartCatcher", typeof(Image), typeof(Catcher));
        cg.transform.SetParent(canvas, false);
        cg.transform.SetAsFirstSibling();
        var crt = cg.GetComponent<RectTransform>();
        crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = crt.offsetMax = Vector2.zero;
        cg.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        cg.GetComponent<Catcher>().down = OnTouch;

        var c = new Vector2(0.5f, 0.5f);
        play = make(canvas, "Play", c, new Vector2(0, -60), new Vector2(620, 120), StartGame);
        exit = make(canvas, "Exit", c, new Vector2(0, -210), new Vector2(620, 120), PauseMenu.QuitApp);
        items = new[] { play, exit };
        touchExit = make(canvas, "Exit", new Vector2(0, 1), new Vector2(130, -80), new Vector2(220, 90), PauseMenu.QuitApp);
        touchExit.GetComponentInChildren<Text>().fontSize = 36;

        // Touch hint, bottom centre, pulsing. Same trick as the timer: two copies, each clipped to its half of the
        // white/black split: black text + white outline on the white half, white text + black outline on the black half.
        hint = new GameObject("TouchHint", typeof(RectTransform));
        hint.transform.SetParent(canvas, false);
        var hrt = hint.GetComponent<RectTransform>();
        hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one; hrt.offsetMin = hrt.offsetMax = Vector2.zero;
        hintTexts = new Text[2]; hintOutlines = new Outline[2];
        for (int h = 0; h < 2; h++)
        {
            bool left = h == 0;
            var pgo = new GameObject(left ? "HintLeft" : "HintRight", typeof(RectMask2D));
            pgo.transform.SetParent(hint.transform, false);
            var prt = pgo.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(left ? 0f : 0.5f, 0f); prt.anchorMax = new Vector2(left ? 0.5f : 1f, 0f);
            prt.pivot = new Vector2(0.5f, 0f); prt.anchoredPosition = new Vector2(0, 150); prt.sizeDelta = new Vector2(0, 140);
            var tgo = new GameObject("Text", typeof(Text));
            tgo.transform.SetParent(pgo.transform, false);
            var trt = tgo.GetComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(left ? 1f : 0f, 0f);
            trt.pivot = new Vector2(0.5f, 0f); trt.sizeDelta = new Vector2(1400, 140); trt.anchoredPosition = Vector2.zero;
            var t = tgo.GetComponent<Text>();
            t.font = UiFont.Get(); t.fontSize = 72; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false; t.text = "Touch to start";
            var o = tgo.AddComponent<Outline>(); o.effectDistance = new Vector2(3, -3);
            hintTexts[h] = t; hintOutlines[h] = o;
        }

        // Placeholder controller icon ("A") next to Play: replace with real art later.
        iconA = new GameObject("IconA", typeof(Image));
        iconA.transform.SetParent(play.transform, false);
        var irt = iconA.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = new Vector2(0, 0.5f); irt.pivot = new Vector2(0.5f, 0.5f);
        irt.anchoredPosition = new Vector2(60, 0); irt.sizeDelta = new Vector2(80, 80);
        iconA.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.35f); iconA.GetComponent<Image>().raycastTarget = false;
        var itgo = new GameObject("T", typeof(Text));
        itgo.transform.SetParent(iconA.transform, false);
        var itr = itgo.GetComponent<RectTransform>();
        itr.anchorMin = Vector2.zero; itr.anchorMax = Vector2.one; itr.offsetMin = itr.offsetMax = Vector2.zero;
        var it = itgo.GetComponent<Text>(); it.font = UiFont.Get(); it.fontSize = 50; it.text = "A"; it.alignment = TextAnchor.MiddleCenter; it.color = Color.black; it.raycastTarget = false;

        ring = MakeRing();
        ApplyMode();
    }

    void StartGame() { SceneManager.LoadScene("GameplayScene"); }

    void ApplyMode()
    {
        if (items == null) return;
        play.gameObject.SetActive(Controller); exit.gameObject.SetActive(Controller);
        touchExit.gameObject.SetActive(!Controller); hint.SetActive(!Controller);
        Highlight();
    }

    void Highlight()
    {
        for (int i = 0; i < items.Length; i++) items[i].GetComponent<Image>().color = (Controller && i == idx) ? Selected : Normal;
    }

    void OnTouch(Vector2 pos)
    {
        if (Controller || loadAt > 0f) return;
        Haptics.Tap();
        for (int i = 0; i < 3; i++) StartCoroutine(Ripple(pos, i * 0.14f));
        loadAt = Time.unscaledTime + 0.55f;
    }

    IEnumerator Ripple(Vector2 screen, float delay)
    {
        var go = new GameObject("Ripple", typeof(Image));
        go.transform.SetParent(canvasRt, false);
        var rt = go.GetComponent<RectTransform>();
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screen, null, out local);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = local;
        var img = go.GetComponent<Image>(); img.sprite = ring; img.raycastTarget = false;
        img.enabled = false;
        for (float t = 0; t < delay; t += Time.unscaledDeltaTime) yield return null;
        img.enabled = true;
        const float dur = 0.9f;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            float k = t / dur;
            rt.sizeDelta = Vector2.one * Mathf.Lerp(60, 520, 1f - (1f - k) * (1f - k));
            img.color = new Color(0.75f, 0.9f, 1f, 0.7f * (1f - k));
            yield return null;
        }
        Destroy(go);
    }

    static Sprite MakeRing()
    {
        const int N = 128;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(N / 2f, N / 2f)) / (N / 2f);
                float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.85f) / 0.1f) ; // thin soft ring near the edge
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        // Mode switch by last input.
        bool was = Controller;
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) Controller = false;
        float y = Input.GetAxis("Vertical");
        bool padInput = Mathf.Abs(y) > 0.5f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f
            || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.Return);
        if (padInput && !(Input.touchCount > 0)) Controller = true;
        if (Controller != was) { idx = 0; ApplyMode(); }

        if (hint != null && hint.activeSelf)
        {
            float a = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 3f);
            for (int h = 0; h < 2; h++)
            {
                Color fill = h == 0 ? Color.black : Color.white, edge = h == 0 ? Color.white : Color.black;
                fill.a = a; edge.a = a;
                hintTexts[h].color = fill; hintOutlines[h].effectColor = edge;
            }
        }

        if (Controller && !PauseMenu.MenuOpen)
        {
            if (y < -0.5f && prevY >= -0.5f) { idx = (idx + 1) % items.Length; Highlight(); }
            else if (y > 0.5f && prevY <= 0.5f) { idx = (idx + items.Length - 1) % items.Length; Highlight(); }
            if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.Return))
                items[idx].onClick.Invoke();
        }
        prevY = y;

        if (loadAt > 0f && Time.unscaledTime >= loadAt) { loadAt = -1f; StartGame(); }
    }
}
