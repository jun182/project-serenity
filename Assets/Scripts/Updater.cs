using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// In-app updater (Android). At every start of the start screen: list the GitHub releases of the fork, take the highest tag "alpha-bN"
// (N = leading number, so "alpha-b7-rebuild1" counts as 7) and, if N > BuildInfo.Number, show the button. Tapping it downloads the .apk
// and opens the system installer. Every failure stays silent (offline = no button); the installer only accepts an APK signed with the same key.
public class Updater : MonoBehaviour
{
    const string ListUrl = "https://api.github.com/repos/jun182/project-serenity/releases?per_page=30";
    [Serializable] class Asset { public string name; public string browser_download_url; }
    [Serializable] class Release { public string tag_name; public bool draft; public Asset[] assets; }
    [Serializable] class ReleaseList { public Release[] items; }

    Button button; Text label; string apkUrl; int newNumber; bool busy;

    public void Init(Button b, Text t)
    {
        button = b; label = t;
        button.gameObject.SetActive(false);
        button.onClick.AddListener(() => { if (!busy) StartCoroutine(DownloadAndInstall()); });
        StartCoroutine(Check());
    }

    static int ParseTag(string tag)
    {
        const string p = "alpha-b";
        if (tag == null || !tag.StartsWith(p)) return -1;
        int i = p.Length, n = 0, digits = 0;
        while (i < tag.Length && char.IsDigit(tag[i]) && digits < 6) { n = n * 10 + (tag[i] - '0'); i++; digits++; }
        return digits == 0 ? -1 : n;
    }

    IEnumerator Check()
    {
        using (var req = UnityWebRequest.Get(ListUrl))
        {
            req.SetRequestHeader("User-Agent", "project-serenity-updater");
            req.SetRequestHeader("Accept", "application/vnd.github+json");
            req.timeout = 15;
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) yield break;
            try
            {
                var list = JsonUtility.FromJson<ReleaseList>("{\"items\":" + req.downloadHandler.text + "}");
                foreach (var r in list.items)
                {
                    int n = ParseTag(r.tag_name);
                    if (r.draft || n <= BuildInfo.Number || n <= newNumber || r.assets == null) continue;
                    foreach (var a in r.assets)
                        if (a.name != null && a.name.EndsWith(".apk") && !string.IsNullOrEmpty(a.browser_download_url))
                        { newNumber = n; apkUrl = a.browser_download_url; break; }
                }
            }
            catch (Exception e) { Debug.Log("Update check failed: " + e.Message); yield break; }
        }
        if (apkUrl == null) yield break;
        label.text = "Update available: b" + newNumber;
        button.gameObject.SetActive(true);
    }

    IEnumerator DownloadAndInstall()
    {
        busy = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!CanInstall()) { OpenInstallSettings(); label.text = "Allow installs, then tap again"; busy = false; yield break; }
        string path;
        using (var prov = new AndroidJavaClass("se.vibing.serenity.UpdateProvider"))
            path = prov.CallStatic<string>("apkPath", Activity());
#else
        string path = System.IO.Path.Combine(Application.temporaryCachePath, "update.apk");
#endif
        using (var req = UnityWebRequest.Get(apkUrl))
        {
            req.SetRequestHeader("User-Agent", "project-serenity-updater");
            req.downloadHandler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
            var op = req.SendWebRequest();
            while (!op.isDone) { label.text = "Downloading " + Mathf.RoundToInt(req.downloadProgress * 100f) + " %"; yield return null; }
            if (req.result != UnityWebRequest.Result.Success) { label.text = "Download failed, tap to retry"; busy = false; yield break; }
        }
        label.text = "Installing...";
#if UNITY_ANDROID && !UNITY_EDITOR
        try { Install(); }
        catch (Exception e) { Debug.Log("Install failed: " + e.Message); label.text = "Install failed, tap to retry"; }
#endif
        busy = false;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject Activity()
    {
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            return player.GetStatic<AndroidJavaObject>("currentActivity");
    }

    static bool CanInstall()
    {
        using (var v = new AndroidJavaClass("android.os.Build$VERSION"))
            if (v.GetStatic<int>("SDK_INT") < 26) return true;
        using (var pm = Activity().Call<AndroidJavaObject>("getPackageManager"))
            return pm.Call<bool>("canRequestPackageInstalls");
    }

    static void OpenInstallSettings()
    {
        var act = Activity();
        using (var uriClass = new AndroidJavaClass("android.net.Uri"))
        using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier))
        using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_UNKNOWN_APP_SOURCES", uri))
            act.Call("startActivity", intent);
    }

    static void Install()
    {
        var act = Activity();
        using (var prov = new AndroidJavaClass("se.vibing.serenity.UpdateProvider"))
        using (var uri = prov.CallStatic<AndroidJavaObject>("apkUri"))
        using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
        {
            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", 1 /*GRANT_READ_URI_PERMISSION*/ | 0x10000000 /*NEW_TASK*/);
            act.Call("startActivity", intent);
        }
    }
#endif
}
