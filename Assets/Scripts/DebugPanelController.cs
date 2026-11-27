using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugPanelController : MonoBehaviour
{
    [SerializeField] 
    private GameObject debugPanel;
    [SerializeField] 
    private Slider speedSlider;
    [SerializeField] 
    private Slider maxHealthSlider;
    [SerializeField] 
    private Slider damageOnHitSlider;
    [SerializeField] 
    private Slider healingSpeedSlider;
    [SerializeField] 
    private Slider dragModifierSlider;
    [SerializeField] 
    private Slider accelerationModifierSlider;
    [SerializeField] 
    private Slider maxVelocitySlider;
    [SerializeField] 
    private PlayerController playerController;

    //public KeyCode toggleDebugPanel = KeyCode.P;
    
    public bool isDebugPanelActive = true; // state last applied; real setting is the PlayerPrefs key

  
    // The controller lives on the panel root, so hide children + background instead of deactivating the root.
    void Start()
    {
        if (speedSlider) speedSlider.value = playerController.SPEED;
        if (maxHealthSlider) maxHealthSlider.value = playerController.MAX_HEALTH;
        if (damageOnHitSlider) damageOnHitSlider.value = playerController.DAMAGE_ON_HIT;
        if (healingSpeedSlider) healingSpeedSlider.value = playerController.HEALING_SPEED;
        if (dragModifierSlider) dragModifierSlider.value = playerController.DRAG_MODIFIER;
        if (accelerationModifierSlider) accelerationModifierSlider.value = playerController.ACCELERATION_MODIFIER;
        if (maxVelocitySlider) maxVelocitySlider.value = playerController.MAX_VELOCITY;
        ApplyVisibility(PauseMenu.DebugOn);
    }

    void Update()
    {
        if (GameSettings.DevMode && Input.GetKeyDown(KeyCode.P))
            PlayerPrefs.SetInt(PauseMenu.DebugPrefKey, PlayerPrefs.GetInt(PauseMenu.DebugPrefKey, 0) == 1 ? 0 : 1);
        bool want = PauseMenu.DebugOn;
        if (want != isDebugPanelActive) ApplyVisibility(want);
    }

    void ApplyVisibility(bool visible)
    {
        isDebugPanelActive = visible;
        foreach (Transform c in transform) c.gameObject.SetActive(visible);
        var bg = GetComponent<Image>();
        if (bg) bg.enabled = visible;
    }

    public void OnSpeedChanged() {
    playerController.SPEED = speedSlider.value;
}

    public void OnMaxHealthChanged() {
        playerController.MAX_HEALTH = maxHealthSlider.value;
    }

    public void OnDamageOnHitChanged() {
        playerController.DAMAGE_ON_HIT = damageOnHitSlider.value;
    }

    public void OnHealingSpeedChanged() {
        playerController.HEALING_SPEED = healingSpeedSlider.value;
    }

    public void OnDragModifierChanged() {
        playerController.DRAG_MODIFIER = dragModifierSlider.value;
    }

    public void OnAccelerationModifierChanged() {
        playerController.ACCELERATION_MODIFIER = accelerationModifierSlider.value;
    }

    public void OnMaxVelocityChanged() {
        playerController.MAX_VELOCITY = maxVelocitySlider.value;
    }

}
