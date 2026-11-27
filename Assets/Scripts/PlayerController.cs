using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public float SPEED;

    [SerializeField]
    public float MAX_HEALTH;
    [SerializeField]
    public float DAMAGE_ON_HIT;
    [SerializeField]
    public float HEALING_SPEED;

    [SerializeField]
    public float DRAG_MODIFIER;
    [SerializeField]
    public float ACCELERATION_MODIFIER;

    [SerializeField]
    public float health = 100;

    [SerializeField]
    public float MAX_VELOCITY = 1000;



    private bool healing = false;
    private float heartTimer;
    // Diagnostics shown in the debug overlay (PauseMenu).
    public static int DiagHits, DiagSerEnter, DiagSerExit, DiagTrig;
    public static string DiagTheme = "-", DiagMusic = "-";
    public bool IsHealing { get { return healing; } }
    public bool IsDead { get { return hasDied; } }
    public const float HealthCap = 100f; // healing stops at 100; scene MAX_HEALTH is 0 and unused
    private bool blocked = false;

    public float topSpeed;
    public float accelerationSpeed;
    private float speed;

    private float deathFloat;
    public float deathSpeed;
    public Material blitMat;

    private bool hasDied;


    [SerializeField]
    private MusicManagerController musicManagerController;

    public GameObject playerVisuals;


    // private Rigidbody rigidbody;
    float timeInLight;

    [SerializeField]
    float maxTimeInLight;

    // Start is called before the first frame update
    void Start()
    {
        // rigidbody = gameObject.GetComponent<Rigidbody>();
        // The scene's serialized reference is empty (also upstream): every PlayTheme/PlaySound threw, so hits and theme changes never completed.
        if (musicManagerController == null) musicManagerController = FindObjectOfType<MusicManagerController>();
        DiagMusic = musicManagerController != null ? "mm ok" : "mm NULL";
        hasDied = false;
        deathFloat = -1f;
        blitMat.SetFloat("_DeathFloat", deathFloat);
    }

    // Update is called once per frame
    void Update()
{
    if (PauseMenu.IsPaused) return; // movement adds `speed` per frame without dt, so it must not run while paused

    float hInput = Convert.ToSingle(Input.GetKey(KeyCode.D)) - Convert.ToSingle(Input.GetKey(KeyCode.A));
    // Touch: hold the right half of the screen to move right, the left half to move left.
    bool gyroOn = GyroSettings.Enabled;
    for (int i = 0; i < Input.touchCount && !gyroOn; i++)
    {
        Touch t = Input.GetTouch(i);
        if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
        if (PauseMenu.IsPaused || (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(t.fingerId))) continue;
        hInput += t.position.x > Screen.width * 0.5f ? 1f : -1f;
    }
    if (gyroOn && !PauseMenu.IsPaused) hInput += GyroSettings.Steer();
    hInput = Mathf.Clamp(hInput, -1f, 1f);
    float targetSpeed = hInput * topSpeed;
    if (blocked)
        targetSpeed = -topSpeed;
    float x = gameObject.transform.position.x;
    if (x >= 14)
        targetSpeed = -topSpeed;
    else if (x <= -4)
        targetSpeed = topSpeed;
    speed = Mathf.Lerp(speed, targetSpeed * Time.deltaTime, accelerationSpeed * Time.deltaTime);
    gameObject.transform.position += new Vector3(speed, 0f, 0f);
    // Serenity side is decided by x position (same boundary as HideObjectsPerSide): the physics trigger exit never fired on the phone.
    bool inSerenity = gameObject.transform.position.x > 5f;
    if (inSerenity != healing && !hasDied)
    {
        healing = inSerenity;
        if (healing)
        {
            DiagSerEnter++; DiagTheme = "Dark(in)";
            musicManagerController.PlayTheme(MusicManagerController.Theme.Dark);
            musicManagerController.PlaySound(MusicManagerController.Sound.IntoSerenity);
        }
        else
        {
            DiagSerExit++; DiagTheme = "Light(out)";
            musicManagerController.PlayTheme(MusicManagerController.Theme.Light);
            musicManagerController.PlaySound(MusicManagerController.Sound.IntoNoise);
        }
    }
    if (healing && !blocked && health > 0)
    {
        heartTimer -= Time.deltaTime;
        if (heartTimer <= 0f) { Haptics.Heartbeat(); heartTimer = 0.9f; }
    }
    else heartTimer = 0f;
    // Death check runs on both sides (was inside the !healing branch); b3 showed hp -660 with hasDied still false.
    if (hasDied || health <= 0)
    {
        if (!hasDied)
        {
            hasDied = true;
            if (playerVisuals != null) playerVisuals.SetActive(false);
            musicManagerController.PlaySound(MusicManagerController.Sound.Death);
        }
        deathFloat = Mathf.Lerp(deathFloat, 1.1f, deathSpeed * Time.deltaTime);
        blitMat.SetFloat("_DeathFloat", deathFloat);
        if (deathFloat > 1f)
        {
            blitMat.SetFloat("_DeathFloat", -1f);
            SceneManager.LoadScene("MainMenu");
        }
    }
    if (healing && health < 100 && !blocked)
    {

        if (healing && health < 100 && !blocked)
        {
            health += HEALING_SPEED * Time.deltaTime;
        }

    }
    else
    {
        timeInLight = 0;
    }



        if (!healing)
        {
            timeInLight += Time.deltaTime;
            if (timeInLight >= maxTimeInLight)
            {
                musicManagerController.PlayTheme(MusicManagerController.Theme.Broken);
            }
            // if (!dragging){
            //     transform.position += new Vector3(velocity.x * Time.deltaTime, 0, 0) * SPEED;
            // } else {
            //     transform.position += new Vector3(velocity.x * Time.deltaTime, 0, 0) * SPEED / DRAG_MODIFIER;
            // }


            if (!healing) {
                timeInLight += Time.deltaTime;
                if (timeInLight >= maxTimeInLight)
                {
                    musicManagerController.PlayTheme(MusicManagerController.Theme.Broken);
                }
            }
            else
            {
                timeInLight = 0;
            }

        }
    }
    void OnTriggerEnter(Collider collider)
        {
            
            Debug.Log("TriggerEnter");
            DiagTrig++;
        if (!hasDied)
        {
            if (collider.gameObject.tag == "Obstacle")
            {
                // Debug.Log("Hit an obstacle!");
                musicManagerController.PlaySound(MusicManagerController.Sound.Hit);
                health -= DAMAGE_ON_HIT;
                DiagHits++;
                Haptics.Hit();
            }
            else if (collider.gameObject.tag == "Border")
            {

            }
            if (collider.gameObject.tag == "SerenityBlocker")
            {
                musicManagerController.PlaySound(MusicManagerController.Sound.SerenityBlocked);
                blocked = true;
            }
        }

        }

        void OnTriggerExit(Collider collider)
        {
            Debug.Log("TriggerExit");
            if (collider.gameObject.tag == "SerenityBlocker")
            {
                blocked = false;
            }

        
    }
}