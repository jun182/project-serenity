using UnityEngine;
using UnityEngine.SceneManagement;

// Zooms the gameplay camera in by ZoomFactor (1 = original view). Perspective camera: narrows the vertical FOV.
public static class CameraZoom
{
    public const float ZoomFactor = 1.25f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => { if (scene.name == "GameplayScene") Apply(); };
    }

    static void Apply()
    {
        var cam = Camera.main;
        if (cam == null) return;
        if (ShowTestMarkers) AddMarkers(cam);
        if (cam.orthographic) cam.orthographicSize /= ZoomFactor;
        else cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / ZoomFactor) * Mathf.Rad2Deg;
    }

    // Zoom test: cubes placed near the screen edges of the UNZOOMED view (viewport 4% / 96%).
    // With the zoom working they fall outside the cropped view; if they are visible, the zoom did not apply.
    public const bool ShowTestMarkers = true;

    static void AddMarkers(Camera cam)
    {
        Vector2[] spots = { new Vector2(0.04f, 0.5f), new Vector2(0.96f, 0.5f), new Vector2(0.5f, 0.96f), new Vector2(0.5f, 0.04f) };
        foreach (var v in spots)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(go.GetComponent<Collider>()); // must not touch gameplay triggers
            go.name = "ZoomTestMarker";
            go.transform.position = cam.ViewportToWorldPoint(new Vector3(v.x, v.y, 4f));
            go.transform.localScale = Vector3.one * 0.25f;
            go.transform.SetParent(cam.transform, true);
        }
    }
}
