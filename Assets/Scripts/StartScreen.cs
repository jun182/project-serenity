using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Input-adaptive start screen (spawned by PauseMenu on MainMenu). Follows the last-used input:
//  touch mode:      Play button (Space also starts on PC/Web), small Exit top-left.
//  controller mode: Play / Exit buttons, stick or d-pad moves the highlight, A or Start confirms.
// The scene's own Play/Exit buttons are hidden; this replaces them.
public class StartScreen : MonoBehaviour
{
    public delegate Button MakeBtn(Transform parent, string text, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction onClick);

    public static bool Controller { get; private set; }

    RectTransform canvasRt;
    Button play, exit, touchExit;
    Button[] items;
    int idx;
    GameObject iconA;
    float prevY;
    const int Konami9 = 9; // PauseMenu.KonamiPos value just before the final A
    public static string Sel = "-"; // for the MENU log line
    bool lastMenuOpen;

    public void Init(Transform canvas, MakeBtn make)
    {
        canvasRt = canvas as RectTransform;
        // Replace the scene's Play/Exit buttons.
        foreach (var n in new[] { "Play", "Exit" }) { var g = GameObject.Find(n); if (g != null) g.SetActive(false); }
        Controller = false;
        foreach (var j in Input.GetJoystickNames()) if (!string.IsNullOrEmpty(j)) Controller = true;

        var c = new Vector2(0.5f, 0.5f);
        play = make(canvas, "Play", c, new Vector2(0, -200), new Vector2(400, 90), StartGame);
        exit = make(canvas, "Exit", c, new Vector2(0, -310), new Vector2(400, 90), PauseMenu.QuitApp);
        items = new[] { play, exit };
        Brush.Register(play.GetComponent<Image>()); Brush.Register(exit.GetComponent<Image>());
        touchExit = make(canvas, "Exit", new Vector2(0, 1), new Vector2(130, -80), new Vector2(220, 90), PauseMenu.QuitApp);
        touchExit.GetComponentInChildren<Text>().fontSize = 36;

        // Controller icon "A" next to Play: black/white like the rest of the UI (white disc, black letter and outline).
        iconA = PadIcons.Disc(play.transform, "A", new Vector2(0, 0.5f), new Vector2(50, 0), 64);

        ApplyMode();
    }

    void StartGame() { SceneManager.LoadScene("GameplayScene"); }

    void ApplyMode()
    {
        if (items == null) return;
        bool vis = !PauseMenu.MenuOpen; // only one page visible at a time: hide the start screen while the menu is open
        play.gameObject.SetActive(vis); exit.gameObject.SetActive(Controller && vis); // Play is a button in both modes (no more "Touch to start")
        touchExit.gameObject.SetActive(!Controller && vis);
        if (iconA) iconA.SetActive(Controller);
        Highlight();
    }

    void Highlight()
    {
        for (int i = 0; i < items.Length; i++) PadIcons.Paint(items[i], Controller && i == idx);
        Sel = Controller ? items[idx].name.Replace("Button", "") : "-";
    }

    void Update()
    {
        // Mode switch by last input.
        bool was = Controller;
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)) Controller = false;
        float y = Input.GetAxis("Vertical");
        bool padInput = PadIcons.AnyPadInput();
        if (padInput && !(Input.touchCount > 0)) Controller = true;
        bool woke = Controller != was; // the press that switches to controller mode only wakes it, no action
        if (woke) { idx = 0; ApplyMode(); }
        bool menuChanged = lastMenuOpen != PauseMenu.MenuOpen; // the A that closes the menu must not also press Play
        if (menuChanged) { lastMenuOpen = PauseMenu.MenuOpen; ApplyMode(); }

        if (!Application.isMobilePlatform && !Controller && !PauseMenu.MenuOpen && Input.GetKeyDown(KeyCode.Space)) StartGame();
        if (Controller && !PauseMenu.MenuOpen && !woke && !menuChanged)
        {
            bool down = (y < -0.5f && prevY >= -0.5f) || Input.GetKeyDown(KeyCode.DownArrow);
            bool up = (y > 0.5f && prevY <= 0.5f) || Input.GetKeyDown(KeyCode.UpArrow);
            if (down) { idx = (idx + 1) % items.Length; Highlight(); Haptics.Select(); }
            else if (up) { idx = (idx + items.Length - 1) % items.Length; Highlight(); Haptics.Select(); }
            // A / Enter confirms. Start opens the menu (PauseMenu), it no longer starts the game.
            bool konamiA = PauseMenu.KonamiPos == Konami9 || PauseMenu.KonamiFrame == Time.frameCount; // the last Konami key (A) must not press Play
            if (!konamiA && (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                items[idx].onClick.Invoke();
        }
        for (int i = 0; i < items.Length; i++) items[i].transform.localScale = Vector3.one * ((Controller && i == idx) ? PadIcons.Pulse() : 1f);
        prevY = y;

    }
}
