using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManagerController : MonoBehaviour
{
    // Start is called before the first frame update

    public enum Theme
    {
        Dark,
        LightBrokenMix // white side: Light and Broken crossfaded by health, see SetBrokenTarget
    }

    public enum Sound
    {
        IntoSerenity,
        IntoNoise,
        SerenityBlocked,
        Death,
        Hit
    }

    [SerializeField]
    private AudioSource darkAudio;
    [SerializeField]
    private AudioSource lightAudio;
    [SerializeField]
    private AudioSource brokenAudio;

    [SerializeField]
    private AudioSource intoSerenity;
    [SerializeField]
    private AudioSource intoNoise;
    [SerializeField]
    private AudioSource serenityBlocked;
    [SerializeField]
    private AudioSource death;
    [SerializeField]
    private AudioSource hit1;
    [SerializeField]
    private AudioSource hit2;

    int lastHitSound = 1;


    Theme currentTheme;

    // White-side mix: 0 = only Light, 1 = only Broken. Eases toward brokenTarget (~0.5 s to settle), equal-power volumes.
    const float MixSmoothTime = 0.15f;
    float brokenMix, brokenTarget, brokenVel;
    public float LightVolume { get { return lightAudio.mute ? 0f : lightAudio.volume; } }
    public float BrokenVolume { get { return brokenAudio.mute ? 0f : brokenAudio.volume; } }

    public void SetBrokenTarget(float t) { brokenTarget = Mathf.Clamp01(t); }

    void Start()
    {
        currentTheme = Theme.Dark;

        darkAudio.Play();
        lightAudio.Play();
        lightAudio.mute = true;
        brokenAudio.Play();
        brokenAudio.mute = true;

    }

    // Update is called once per frame
    void Update()
    {
        // Time.deltaTime is 0 while paused, so the mix holds still.
        brokenMix = Mathf.SmoothDamp(brokenMix, brokenTarget, ref brokenVel, MixSmoothTime);
        ApplyMix();
    }

    void ApplyMix()
    {
        float a = Mathf.Clamp01(brokenMix) * Mathf.PI * 0.5f;
        lightAudio.volume = Mathf.Cos(a);
        brokenAudio.volume = Mathf.Sin(a);
    }

    public void PlayTheme(Theme theme)
    {
        switch (theme)
        {

            case (Theme.Dark):
            {
                    darkAudio.mute = false;
                    lightAudio.mute = true;
                    brokenAudio.mute = true;
                    break;
            }
            case (Theme.LightBrokenMix):
            {
                    darkAudio.mute = true;
                    lightAudio.mute = false;
                    brokenAudio.mute = false;
                    // side switch is an instant cut: start at the current target, no glide
                    brokenMix = brokenTarget; brokenVel = 0f;
                    ApplyMix();
                    break;
            }

        }
        currentTheme = theme;
    }

    public void PlaySound(Sound sound)
    {
        switch (sound)
        {
            case Sound.IntoSerenity:
                {
                    intoSerenity.Play();
                    break;
                }
            case Sound.IntoNoise:
                {
                    intoNoise.Play();
                    break;
                }
            case Sound.SerenityBlocked:
                {
                    serenityBlocked.Play();
                    break;
                }
            case Sound.Hit:
                {
                    if (lastHitSound == 1)
                    {
                        hit2.Play();
                        lastHitSound = 2;
                    } else
                    {
                        hit1.Play();
                        lastHitSound = 1;
                    }
                    break;
                }
            case Sound.Death:
                {
                    death.Play();
                    break;
                }
        }
    }

}
