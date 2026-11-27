using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The one UI style (specs/system/ui-style.md), back to the original 2023 look: white rounded buttons, black outline, black text.
// Pad / keyboard selection inverts a button (black fill, white outline, white text). Sliders: black track with a white outline,
// white fill, white handle with a black outline. All sprites are generated here (no image assets) and 9-sliced, so one sprite
// fits every size. Image colours stay white, so Unity's colour tint still darkens a pressed button.
public static class UiStyle
{
    public static readonly Color Light = Color.white, Dark = Color.black;
    public static readonly Color Pressed = new Color(0.78f, 0.78f, 0.78f, 1f); // 2023 default ColorBlock

    // Sizes in canvas units (reference 1920x1080; 1 sprite pixel = 1 unit).
    const int ButtonRadius = 20, ButtonEdge = 4;
    const int TrackRadius = 10, TrackEdge = 2, TrackEdgeSelected = 5;
    const int KnobRadius = 12, KnobEdge = 3;
    const int DiscEdge = 4;

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite ButtonSprite(bool selected) { return selected ? Rounded(ButtonRadius, ButtonEdge, Dark, Light) : Rounded(ButtonRadius, ButtonEdge, Light, Dark); }

    // ---- Buttons ----
    public static void Button(Button b)
    {
        var img = b.GetComponent<Image>();
        img.sprite = ButtonSprite(false); img.type = Image.Type.Sliced; img.color = Color.white;
        SetTint(b);
        var t = b.GetComponentInChildren<Text>();
        if (t != null) t.color = Dark;
    }

    // Selected (pad / keyboard): black fill, white outline, white text. Not selected: white, black outline, black text.
    public static void Select(Button b, bool selected)
    {
        var img = b.GetComponent<Image>();
        if (img != null) img.sprite = ButtonSprite(selected);
        var t = b.GetComponentInChildren<Text>();
        if (t != null) t.color = selected ? Light : Dark;
    }

    // Normal, hover and "selected" stay white: a tap also selects in the EventSystem and must not leave a tinted button.
    static void SetTint(Selectable s)
    {
        var cb = s.colors;
        cb.normalColor = cb.highlightedColor = cb.selectedColor = Color.white;
        cb.pressedColor = Pressed;
        cb.disabledColor = new Color(Pressed.r, Pressed.g, Pressed.b, 0.5f);
        cb.colorMultiplier = 1f; cb.fadeDuration = 0.1f;
        s.colors = cb;
    }

    // ---- Sliders (made by DefaultControls.CreateSlider: Background, Fill Area/Fill, Handle Slide Area/Handle) ----
    public static void Slider(Slider s)
    {
        foreach (var img in s.GetComponentsInChildren<Image>())
        {
            img.type = Image.Type.Sliced; img.color = Color.white;
            if (img.name == "Background") img.sprite = Track(false);
            else if (img.name == "Fill") img.sprite = Rounded(TrackRadius, 0, Light, Light);
            else if (img.name == "Handle") { img.sprite = Rounded(KnobRadius, KnobEdge, Light, Dark); img.rectTransform.sizeDelta = new Vector2(44, 20); }
        }
        SetTint(s);
    }

    // Selected slider: thicker track outline (the handle pulse is in PadIcons.SelectFx). Colours stay, so the value stays readable.
    public static void SelectSlider(Slider s, bool selected)
    {
        var bg = s.transform.Find("Background");
        if (bg != null) bg.GetComponent<Image>().sprite = Track(selected);
    }

    static Sprite Track(bool selected) { return Rounded(TrackRadius, selected ? TrackEdgeSelected : TrackEdge, Dark, Light); }

    // ---- Pad icons ----
    // White disc with a black outline (the "A" icon); the letter is black.
    public static Sprite Disc() { return Circle(32, DiscEdge, Light, Dark); }

    // ---- "Menu button width x" (UI tweak panel): menu column and start screen Play / Exit ----
    static readonly Dictionary<Image, Vector2> widthTargets = new Dictionary<Image, Vector2>(); // image -> base size

    public static void RegisterWidth(Image img) { widthTargets[img] = img.rectTransform.sizeDelta; ApplyWidth(img); }

    public static void ReapplyWidths()
    {
        var dead = new List<Image>();
        foreach (var i in widthTargets.Keys) if (i == null) dead.Add(i);
        foreach (var d in dead) widthTargets.Remove(d);
        foreach (var i in widthTargets.Keys) ApplyWidth(i);
    }

    static void ApplyWidth(Image img)
    {
        var b = widthTargets[img];
        img.rectTransform.sizeDelta = new Vector2(b.x * GameSettings.Get("uiBtnScale"), b.y);
    }

    // ---- Sprite generation ----
    // Rounded rectangle, 9-sliced: corners of `radius`, an outline `edge` wide in edgeCol, the rest fillCol. Antialiased.
    static Sprite Rounded(int radius, int edge, Color fillCol, Color edgeCol)
    {
        string key = "r" + radius + "e" + edge + fillCol + edgeCol;
        Sprite sp;
        if (cache.TryGetValue(key, out sp) && sp != null) return sp;
        int n = radius * 2 + 4, border = radius + 1;
        var tex = Draw(n, radius, edge, fillCol, edgeCol);
        sp = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        sp.name = "UiStyle " + key;
        cache[key] = sp;
        return sp;
    }

    // Circle (radius = half the texture), not sliced.
    static Sprite Circle(int radius, int edge, Color fillCol, Color edgeCol)
    {
        string key = "c" + radius + "e" + edge + fillCol + edgeCol;
        Sprite sp;
        if (cache.TryGetValue(key, out sp) && sp != null) return sp;
        int n = radius * 2;
        sp = Sprite.Create(Draw(n, radius, edge, fillCol, edgeCol), new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        sp.name = "UiStyle " + key;
        cache[key] = sp;
        return sp;
    }

    static Texture2D Draw(int n, int radius, int edge, Color fillCol, Color edgeCol)
    {
        var px = new Color32[n * n];
        float half = n / 2f, inner = half - radius;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                // Signed distance to the rounded square (negative inside).
                float qx = Mathf.Abs(x + 0.5f - half) - inner, qy = Mathf.Abs(y + 0.5f - half) - inner;
                float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                float alpha = Mathf.Clamp01(0.5f - d);
                float fill = edge > 0 ? Mathf.Clamp01(0.5f - (d + edge)) : 1f;
                var c = Color.Lerp(edgeCol, fillCol, fill); c.a = alpha;
                px[y * n + x] = c;
            }
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        tex.SetPixels32(px); tex.Apply();
        return tex;
    }
}
