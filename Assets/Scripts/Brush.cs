using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Button shapes for the big buttons (start screen Play/Exit, menu column). Pure white alpha masks, tinted by the Image colour,
// so they stay black/white. Styles: 0 Classic (plain rectangle), 1 Paint (rhombus with dry-brush edges), 2 Rhombus (clean slanted).
// Shape numbers come from GameSettings.UiTunables (live sliders in the "UI tweak panel").
public static class Brush
{
    public static readonly string[] StyleNames = { "Classic", "Paint", "Rhombus" };
    public static int Style
    {
        get { return PlayerPrefs.GetInt("buttonStyle", 1); }
        set { PlayerPrefs.SetInt("buttonStyle", value); PlayerPrefs.Save(); ReapplyAll(); }
    }

    static readonly Dictionary<Image, Vector2> targets = new Dictionary<Image, Vector2>(); // image -> base size
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static void Register(Image img) { targets[img] = img.rectTransform.sizeDelta; Apply(img); }

    public static void ReapplyAll()
    {
        foreach (var kv in cache) { if (kv.Value != null) { Object.Destroy(kv.Value.texture); Object.Destroy(kv.Value); } }
        cache.Clear();
        var dead = new List<Image>();
        foreach (var i in targets.Keys) if (i == null) dead.Add(i);
        foreach (var d in dead) targets.Remove(d);
        foreach (var i in new List<Image>(targets.Keys)) Apply(i);
    }

    static void Apply(Image img)
    {
        var b = targets[img];
        var sz = new Vector2(b.x * GameSettings.Get("uiBtnScale"), b.y);
        img.rectTransform.sizeDelta = sz;
        if (Style == 0) { img.sprite = null; return; }
        img.sprite = Get(Mathf.RoundToInt(sz.x * 0.6f), Mathf.RoundToInt(sz.y * 0.6f), Style == 1);
    }

    static Sprite Get(int w, int h, bool rough)
    {
        string key = w + "x" + h + (rough ? "p" : "r");
        Sprite sp;
        if (cache.TryGetValue(key, out sp)) return sp;
        float skew = GameSettings.Get("uiSkew"), rg = rough ? GameSettings.Get("uiRough") : 0f, dry = rough ? GameSettings.Get("uiDry") : 0f;
        int seed = Mathf.RoundToInt(GameSettings.Get("uiSeed"));
        var px = new Color32[w * h];
        float s = skew * h;                       // horizontal slant in pixels
        float pad = h * 0.12f * rg + 2f;          // room for the wobble
        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            // Per-row "bristle": ragged left/right ends.
            float nl = Hash(y, seed), nr = Hash(y, seed + 101), dr = Hash(y, seed + 202);
            float left = s * (1f - v) + pad + nl * rg * w * 0.05f;
            float right = w - 1 - (s * v + pad) - nr * rg * w * 0.05f;
            // Dry-brush: some rows end early, fading out.
            float dryLen = dr < dry ? dry * w * 0.12f * Hash(y, seed + 303) : 0f;
            for (int x = 0; x < w; x++)
            {
                // Top/bottom edges wave slowly along the stroke.
                float wave = Mathf.Sin(x * 0.045f + seed) * 0.5f + Mathf.Sin(x * 0.11f + seed * 2f) * 0.5f;
                float edge = rg * h * 0.06f * wave;
                float dTop = (h - 1 - y) - (pad * 0.5f) + edge, dBot = y - (pad * 0.5f) - edge;
                float a = Mathf.Clamp01(Mathf.Min(x - (left + dryLen), (right - dryLen) - x, dTop, dBot) + 0.5f);
                // dry rows get a streaky fade near their ends
                if (dryLen > 0f) { float fade = Mathf.Clamp01((Mathf.Min(x - left, right - x)) / (dryLen + 6f)); a *= Mathf.Lerp(1f, fade, dry); }
                px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        tex.SetPixels32(px); tex.Apply();
        sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        cache[key] = sp;
        return sp;
    }

    static float Hash(int n, int seed)
    {
        unchecked { uint x = (uint)(n * 374761393 + seed * 668265263); x = (x ^ (x >> 13)) * 1274126177u; return ((x ^ (x >> 16)) & 0xFFFF) / 65535f; }
    }
}
