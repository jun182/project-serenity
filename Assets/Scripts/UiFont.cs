using UnityEngine;

// The font of the original Windows build's menus (Alegreya SC Bold), for UI built in code.
// Falls back to Unity's built-in font if the asset is missing.
public static class UiFont
{
    static Font cached;
    public static Font Get()
    {
        if (cached == null)
        {
            cached = Resources.Load<Font>("Fonts/AlegreyaSC-Bold");
            if (cached == null) cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return cached;
    }
}
