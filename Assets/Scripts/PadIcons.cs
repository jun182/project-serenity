using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Controller UI helpers, black/white like the touch UI (start hint, timer): white disc/pill, black letter, black outline.
public static class PadIcons
{
    static Sprite disc;

    public static readonly Color Normal = new Color(0.15f, 0.15f, 0.15f, 0.9f), Selected = Color.white;

    static Sprite Disc()
    {
        if (disc != null) return disc;
        const int N = 128;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(N / 2f, N / 2f)) / (N / 2f);
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1f - d) * 20f)));
            }
        tex.Apply();
        return disc = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f));
    }

    // Black disc with a white letter and white outline (A, B, ...). Placeholder shape until real controller art exists.
    public static GameObject Disc(Transform parent, string letter, Vector2 anchor, Vector2 pos, float size)
    {
        var go = new GameObject("Icon" + letter, typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(size, size);
        var img = go.GetComponent<Image>(); img.sprite = Disc(); img.color = Color.black; img.raycastTarget = false;
        var ol = go.AddComponent<Outline>(); ol.effectColor = Color.white; ol.effectDistance = new Vector2(4, -4); // visible on white (selected) and dark buttons
        Label(go.transform, letter, (int)(size * 0.62f), Color.white);
        return go;
    }

    // Small white pill with black text, e.g. "START" under a touch-only button.
    public static GameObject Pill(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize)
    {
        var go = new GameObject("Badge" + text, typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.color = Color.white; img.raycastTarget = false;
        Label(go.transform, text, fontSize, Color.black);
        return go;
    }

    static void Label(Transform parent, string text, int fontSize, Color col)
    {
        var tgo = new GameObject("T", typeof(Text));
        tgo.transform.SetParent(parent, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        var t = tgo.GetComponent<Text>();
        t.font = UiFont.Get(); t.fontSize = fontSize; t.text = text; t.alignment = TextAnchor.MiddleCenter; t.color = col; t.raycastTarget = false;
    }

    // Selected button: white with black text; otherwise dark with white text.
    public static void Paint(Button b, bool selected)
    {
        var img = b.GetComponent<Image>();
        img.color = selected ? Selected : Normal;
        var t = b.GetComponentInChildren<Text>();
        if (t != null) t.color = selected ? Color.black : Color.white;
    }

    // True on any controller / keyboard-nav input this frame (not Escape: that is also Android Back).
    public static bool AnyPadInput()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f) return true;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) return true;
        for (int i = 0; i < 20; i++) if (Input.GetKeyDown(KeyCode.JoystickButton0 + i)) return true;
        return false;
    }

    // Highlight for any selectable (menu buttons, sliders) while a pad is in use.
    public class SelectFx : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public Image target; public Color normal; public Color selected = Color.white;
        public void OnSelect(BaseEventData e) { Paint(true); }
        public void OnDeselect(BaseEventData e) { Paint(false); }
        void Paint(bool on)
        {
            if (target == null) return;
            if (on && !PauseMenu.PadActive) return;
            target.color = on ? selected : normal;
            var b = GetComponent<Button>();
            if (b != null) { var t = b.GetComponentInChildren<Text>(); if (t != null) t.color = on ? Color.black : Color.white; }
        }
    }

    public static void AddFx(Button b)
    {
        var fx = b.gameObject.AddComponent<SelectFx>();
        fx.target = b.GetComponent<Image>(); fx.normal = Normal; fx.selected = Selected;
    }

    public static void AddFx(Slider s)
    {
        var bg = s.transform.Find("Background");
        if (bg == null) return;
        var fx = s.gameObject.AddComponent<SelectFx>();
        fx.target = bg.GetComponent<Image>(); fx.normal = new Color(0.3f, 0.3f, 0.3f); fx.selected = new Color(0.75f, 0.75f, 0.75f);
    }
}
