using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Controller UI helpers in the UI style (specs/system/ui-style.md): white disc / pill, black outline, black letter.
public static class PadIcons
{
    // White disc with a black outline and a black letter (A, B, ...). Placeholder shape until real controller art exists.
    public static GameObject Disc(Transform parent, string letter, Vector2 anchor, Vector2 pos, float size)
    {
        var go = new GameObject("Icon" + letter, typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(size, size);
        var img = go.GetComponent<Image>(); img.sprite = UiStyle.Disc(); img.color = Color.white; img.raycastTarget = false;
        Label(go.transform, letter, (int)(size * 0.62f), UiStyle.Dark);
        return go;
    }

    // Small white pill with a black outline and black text, e.g. "START" next to a touch-only button.
    public static GameObject Pill(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize)
    {
        var go = new GameObject("Badge" + text, typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.GetComponent<Image>(); img.sprite = UiStyle.ButtonSprite(false); img.type = Image.Type.Sliced; img.color = Color.white; img.raycastTarget = false;
        Label(go.transform, text, fontSize, UiStyle.Dark);
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

    // Selected button: black with white outline and text; otherwise white with black outline and text.
    public static void Paint(Button b, bool selected) { UiStyle.Select(b, selected); }

    // True on any controller / keyboard-nav input this frame (not Escape: that is also Android Back).
    public static bool AnyPadInput()
    {
        if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f) return true;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow)
            || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) return true;
        for (int i = 0; i < 20; i++) if (Input.GetKeyDown(KeyCode.JoystickButton0 + i)) return true;
        return false;
    }

    public static float Pulse() { return 1f + GameSettings.Get("uiPulse") * 0.01f * Mathf.Sin(Time.unscaledTime * 6f); }

    // Highlight for any selectable (menu buttons, sliders) while a pad is in use: buttons invert and pulse,
    // sliders get a thicker track outline and a pulsing handle.
    public class SelectFx : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        bool on;
        public void OnSelect(BaseEventData e) { on = PauseMenu.PadActive; Paint(on); if (on) Haptics.Select(); }
        public void OnDeselect(BaseEventData e) { on = false; Scale(1f); Paint(false); }
        // Closing the menu hides the page before the selection is cleared, and hidden objects get no OnDeselect:
        // reset here, or the last selected button stays black the next time the page opens.
        void OnDisable() { on = false; Scale(1f); Paint(false); }
        // Unscaled time: the game is paused in menus.
        void Update() { if (on) Scale(Pulse()); }
        void Scale(float k)
        {
            var s = GetComponent<Slider>();
            if (s != null) { if (s.handleRect != null) s.handleRect.localScale = Vector3.one * k; }
            else transform.localScale = Vector3.one * k;
        }
        void Paint(bool v)
        {
            var b = GetComponent<Button>(); if (b != null) UiStyle.Select(b, v);
            var s = GetComponent<Slider>(); if (s != null) UiStyle.SelectSlider(s, v);
        }
    }

    public static void AddFx(Button b) { b.gameObject.AddComponent<SelectFx>(); }
    public static void AddFx(Slider s) { s.gameObject.AddComponent<SelectFx>(); }
}
