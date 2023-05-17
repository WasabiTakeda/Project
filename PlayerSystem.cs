using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSystem : MonoBehaviour
{

    public static PlayerSystem Instance {get; private set;}

    // player health & energy
    [SerializeField] GameObject player;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI healthTxt;
    [SerializeField] private TextMeshProUGUI energyTxt;
    [SerializeField] private Slider healCDSlider;
    [SerializeField] private TextMeshProUGUI coins;

    // others
    [SerializeField] private Slider dashingCooldownUI;
    [SerializeField] private Slider stunCooldownUI;
    [SerializeField] private Slider explosionCooldownUI;

    PlayerController playerController;

    float currentVelocity = 0;

    // health
    private float maxHP;
    private float playerHP;
    private float currentPlayerHP;
    // energy 
    public float maxEnergy;
    private float playerEnergy;
    private float currentPlayerEnergy;

    // dashing cooldown
    public float dashingCooldown;
    public float cooldown;
    // stun cooldown
    public float stunningCooldown;
    public float stunCD;
    // explosion cooldown
    public float explosionCooldown;
    public float explosionCD;

    // heal cooldown
    public float healCooldown;
    public float healCD;

    // coins
    private float totalCoins;

    // damages
    public float meleeDamage;
    public float projectileDamage;

    // others
    // main audio 
    private GameObject mainAudio;
    // screens
    [SerializeField] private GameObject pausedMenu;
    
    void Awake() {
        
        if (Instance == null) {
            Instance = this;
        }

    }
    void Start()
    {
        // healtbar
        playerHP = player.GetComponent<PlayerController>().playerHealth;
        currentPlayerHP = player.GetComponent<PlayerController>().currentPlayerHealth;
        maxHP = currentPlayerHP;
        healthSlider.value = maxHP;
        healthTxt.text = (currentPlayerHP * 100) + " / " + (playerHP * 100);
        
        // energy bar
        maxEnergy = player.GetComponent<PlayerController>().currentEnergy;
        energySlider.value = maxEnergy;

        playerEnergy = player.GetComponent<PlayerController>().maxEnergy;
        currentPlayerEnergy = player.GetComponent<PlayerController>().currentEnergy;
        energyTxt.text = (currentPlayerEnergy * 100) + " / " + (playerEnergy * 100);

        // dashing cooldown
        dashingCooldown = player.GetComponent<PlayerController>().dashingCooldown;
        dashingCooldownUI.maxValue = dashingCooldown;
        dashingCooldownUI.minValue = 0;
        dashingCooldownUI.value = dashingCooldown;

        // stun cooldwon
        stunningCooldown = player.GetComponent<PlayerController>().stunCooldown;
        stunCooldownUI.maxValue = stunningCooldown;
        stunCooldownUI.minValue = 0;
        stunCooldownUI.value = stunningCooldown;

        // explosion cooldown
        explosionCooldown = player.GetComponent<PlayerController>().explosionCooldown;
        explosionCooldownUI.maxValue = explosionCooldown;
        explosionCooldownUI.minValue = 0;
        explosionCooldownUI.value = explosionCooldown;

        // healing cooldown
        healCooldown = player.GetComponent<PlayerController>().healCooldown;
        healCDSlider.maxValue = healCooldown;
        healCDSlider.minValue = 0;
        healCDSlider.value = healCooldown;

        // total coins
        totalCoins = PlayerPrefs.GetInt(PlayerObjectsSaver.COIN_KEY, 0);

        // damages
        meleeDamage = PlayerPrefs.GetFloat(PlayerObjectsSaver.MELEE_KEY, 0.1f);
        projectileDamage = PlayerPrefs.GetFloat(PlayerObjectsSaver.PROJECTILE_KEY, 0.1f);

        // main audio
        mainAudio = GameObject.FindWithTag("MainAudio").gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        // healtbar
        OnHealthChanged();
        // health txt
        playerHP = player.GetComponent<PlayerController>().playerHealth;
        currentPlayerHP = player.GetComponent<PlayerController>().currentPlayerHealth;
        maxHP = currentPlayerHP;
        if (!(currentPlayerHP < .1f)) {
            healthTxt.text = Mathf.Floor((currentPlayerHP * 100)) + " / " + (playerHP * 100);
        }

        // energy bar
        maxEnergy = player.GetComponent<PlayerController>().currentEnergy;
        energySlider.value = maxEnergy;
        playerEnergy = player.GetComponent<PlayerController>().maxEnergy;
        currentPlayerEnergy = player.GetComponent<PlayerController>().currentEnergy;
        energyTxt.text = Mathf.Floor((currentPlayerEnergy * 100)) + " / " + (playerEnergy * 100);

        // total coins
        totalCoins = PlayerPrefs.GetInt(PlayerObjectsSaver.COIN_KEY, 0);
        coins.text = totalCoins.ToString();

        // cooldwon bars
        // dashing
        OnDashCoolingDown();

        // stun
        OnStunCoolingDown();

        // explosion
        OnExplosionCoolingDown();

        // healing
        OnHealCoolingDown();

        // paused menu when pressed esc
        if (Input.GetKeyDown(KeyCode.Escape) && !player.GetComponent<PlayerController>().isPlayerDead) {
            pausedMenu.SetActive(true);
            Time.timeScale = 0;
            mainAudio.GetComponentInChildren<AudioSource>().Pause();
        }

        // if player is dead, stop playing main audio
        if (player.GetComponent<PlayerController>().isPlayerDead) {
            mainAudio.GetComponentInChildren<AudioSource>().Stop();
        }

    }

    // uis
    // health changed
    public void OnHealthChanged() {
        float current = Mathf.SmoothDamp(healthSlider.value, maxHP, ref currentVelocity, 200 * Time.deltaTime);
        healthSlider.value = current;
    }
    public void OnHealthDamaged(float damageToTake) {
        maxHP = healthSlider.value - damageToTake;
    }

    // dashing cooldown
    private void OnDashCoolingDown() {
        if (player.GetComponent<PlayerController>().canDash) {
            cooldown = dashingCooldown;
        } else {
            cooldown -= Time.deltaTime;
            dashingCooldownUI.value = cooldown;
            if (cooldown <= 0) {
                dashingCooldownUI.value = dashingCooldown;
            }
        }
    }

    // stunning cooldown
    private void OnStunCoolingDown() {
        if (player.GetComponent<PlayerController>().canStun) {
            stunCD = stunningCooldown;
        } else {
            stunCD -= Time.deltaTime;
            stunCooldownUI.value = stunCD;
            if (stunCD <= 0) {
                stunCooldownUI.value = stunningCooldown;
            }
        }
    }
    
    // explosion cooldown
    private void OnExplosionCoolingDown() {
        if (player.GetComponent<PlayerController>().canExplode) {
            explosionCD = explosionCooldown;
        } else {
            explosionCD -= Time.deltaTime;
            explosionCooldownUI.value = explosionCD;
            if (explosionCD <= 0) {
                explosionCooldownUI.value = explosionCooldown;
            }
        }
    }

    // healing cooldown
    private void OnHealCoolingDown() {
        if (player.GetComponent<PlayerController>().canHeal) {
            healCD = healCooldown;
        } else {
            healCD -= Time.deltaTime;
            healCDSlider.value = healCD;
            if (healCD <= 0) {
                healCDSlider.value = healCooldown;
            }
        }
    }

}
