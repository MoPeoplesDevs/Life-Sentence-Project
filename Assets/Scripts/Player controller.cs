using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Playercontroller : MonoBehaviour, IHeal, IDamage
{
    public static event Action OnPlayerDeath;

    public static Playercontroller playerInstance;

    [Header("Player Stats")]
    [SerializeField] GameObject playerCam;
    [Range(1, 10)][SerializeField] float playerSpeed;
    [Range(0, 100)][SerializeField] float playerHP;
    [Range(2, 4)][SerializeField] float evadeSpeed;
    [Range(0, 5)][SerializeField] float evadeTime;
    [Range(1, 5)][SerializeField] float evadeCooldown;
    float evadeDuration;
    float evadeCooldownTimer;
    float deathRotation;
    [SerializeField] float spinSpeed;

    [Header ("Player Abilities")]
    bool isEvading;
    bool isInvincible;
    bool isDead;
    private List<GameObject> weaponInventory = new List<GameObject>();
    private int currentWeapon;

    [Header ("Weapon")]
    [SerializeField] GameObject startingWeapon;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform firePoint;
    [Range(0,1)][SerializeField] float fireRate;
    private float nextFireTime;
    [SerializeField] GameObject loseMenu;
    [SerializeField] private buttonFunctions buttons;

    int playerMaxHP = 100;

    Vector2 playerInput;
    Vector2 mousePos;
    Vector2 mouseWorldPos;
    Vector2 playerDir;
    [SerializeField] LayerMask wallLayer;

    Rigidbody2D rb;
    CircleCollider2D playerCollider;
    [SerializeField] Transform playerSpawn;

    [SerializeField] private Image healthBar;
    [SerializeField] private TMP_Text healthText;

    private Transform laser;

    void Awake()
    {
        if(playerInstance != null && playerInstance != this)
        {
            DontDestroyOnLoad(gameObject);
            return;
        }

        playerInstance = this;
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        laser = transform.Find("Red Dot");

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        AddWeapon(startingWeapon);

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {

        if(isDead != false)
        {
            DeathSpin();
            rb.linearVelocity = new Vector3(0, 0, 0);
            return;
        } 

        if (GameManager.Instance.IsGameOver)
        {
            rb.linearVelocity = new Vector3(0, 0, 0);
            return;
        }

        if (isDead == false)
        {
            Movement();
            TurnPlayer();
            RenderCamera();
            Evade();
            SwitchWeapon();
            UseWeapon();
            UpdateLaser();
        }
	}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.Find("PlayerSpawn");
        isDead = false;

        if(spawn != null)
        {
            playerSpawn = spawn.transform;
            transform.position = playerSpawn.position;
        }

        playerCam = Camera.main.gameObject;
        healthBar = GameObject.Find("Bar").GetComponent<Image>();
        healthText = GameObject.Find("AmountText").GetComponentInChildren<TMP_Text>();
        buttons = FindFirstObjectByType<buttonFunctions>();

        loseMenu = GameObject.Find("UI").transform.Find("MainRoot/LoseMenu").gameObject;

        UpdateHealthUI();
    }


    public void Respawn()
    {
        playerHP = playerMaxHP;
        transform.position = playerSpawn.position;
        UpdateHealthUI();
        isDead = false;
    }

    void RenderCamera()
    {
        playerCam.transform.position = new Vector3(transform.position.x, transform.position.y, playerCam.transform.position.z);
    }

    void Movement()
    {
        if(isDead != false)
        {
            return;
        }

        float x = 0;
        float y = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                x = -1;

            if (Keyboard.current.dKey.isPressed)
                x = 1;

            if (Keyboard.current.wKey.isPressed)
                y = 1;

            if (Keyboard.current.sKey.isPressed)
                y = -1;
        }
        
        playerInput = new Vector2(x, y);
        rb.linearVelocity = playerInput.normalized * playerSpeed;
        
    }

    void TurnPlayer()
    { 
        mousePos = Mouse.current.position.ReadValue();
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        playerDir = mouseWorldPos - new Vector2(transform.position.x, transform.position.y);
        float playerAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg;
        rb.MoveRotation(playerAngle - 90);
    }

    public void Heal(int healAmount)
    {
        playerHP = Mathf.Clamp(playerHP + healAmount, 0, playerMaxHP);
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible == false)
        {
            playerHP = Mathf.Clamp(playerHP - damage, 0, 100);
            UpdateHealthUI();

            if (playerHP <= 0)
                PlayerDeath();
        }
    }

    void Shoot()
    {
        if (buttons.IsPaused || GameManager.Instance.IsGameOver) return;
        if(Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Instantiate(bullet, firePoint.position, transform.rotation);

            nextFireTime = Time.time + fireRate;
        }
    }

    void Evade()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && evadeCooldownTimer <= 0 && !isEvading)
        {
            isEvading = true;
            isInvincible = true;

            playerCollider.excludeLayers = ~wallLayer;

            evadeDuration = evadeTime;
        }
        if (isEvading != false)
        {
            rb.linearVelocity = playerInput * (playerSpeed * evadeSpeed);
            evadeDuration -= Time.deltaTime;

            if (evadeDuration <= 0)
            {
                isEvading = false;
                isInvincible = false;

                playerCollider.excludeLayers = 0;

                evadeCooldownTimer = evadeCooldown;
            }
        }
        if (evadeCooldownTimer > 0)
        {
            evadeCooldownTimer -= Time.deltaTime;
        }
       
    }

    void PlayerDeath()
    {
        isDead = true;
        OnPlayerDeath?.Invoke();
        rb.linearVelocity = Vector2.zero;
    }

    void DeathSpin()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        deathRotation += spinSpeed * Time.deltaTime;

        if(deathRotation >= 1080)
        {
            loseMenu.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdateHealthUI()
    { 
        healthBar.fillAmount = (float)playerHP / playerMaxHP;
        healthText.text = $"Health: {playerHP}";
    }

    public void UseWeapon()
    {
        if(currentWeapon == 0)
        {
            Shoot();
        }
    }

    public void AddWeapon(GameObject weapon)
    {
        if(!weaponInventory.Contains(weapon))
        {
            weaponInventory.Add(weapon);

            if (weaponInventory.Count == 1)
            {
                currentWeapon = 0;
            }

        }
    }

    private void SwitchWeapon()
    {
        if(Keyboard.current.qKey.wasPressedThisFrame)
        {
            currentWeapon++;

            if(currentWeapon >= weaponInventory.Count)
            {
                currentWeapon = 0;
            }
        }

        for (int i = 0; i < weaponInventory.Count; i++)
            {
                if(i == currentWeapon)
                {
                    weaponInventory[i].SetActive(true);

                }
                else
                {
                    weaponInventory[i].SetActive(false);

                }   

            }

    }
    
    private void UpdateLaser()
    {
        int plrLayer = ~LayerMask.GetMask("Player");


        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(plrLayer);
        filter.useTriggers = false;

        float beamLength = 50f;
        RaycastHit2D[] results = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(transform.position, transform.up, filter, results, beamLength);

        if (hitCount > 0)
        {
            RaycastHit2D hit = results[0];
            if (hit != null)
            {
                if (hit.transform.gameObject.name == "Walls")
                    beamLength = hit.distance + 1.5f;
                else
                    beamLength = (hit.transform.position - transform.position).magnitude;
            }
        }

        laser.localScale = new Vector3(0.15f, beamLength, 1f);
        laser.localPosition = new Vector3(0f, beamLength / 2f, 0f);
    }

    public bool AddAmmoToWeapon(AmmoPickup.AmmoType ammoType, int amount)
    {
        foreach (GameObject weapon in weaponInventory)
        {
            if (weapon == null)
            {
                continue;
            }

            if (ammoType == AmmoPickup.AmmoType.MachineGun)
            {
                Debug.Log("Machine gun ammo pickup requested.");

                MachineGun machineGun = weapon.GetComponent<MachineGun>();

                if (machineGun == null)
                {
                    Debug.LogWarning("MachineGun script not found on: " + weapon.name);
                    continue;
                }

                Debug.Log("Found machine gun. Attempting to add " + amount + " ammo.");

                bool ammoAdded = machineGun.AddAmmo(amount);

                Debug.Log("Machine gun ammo added: " + ammoAdded);

                return ammoAdded;
            }

            if (ammoType == AmmoPickup.AmmoType.Shotgun)
            {
                Debug.Log("Shotgun ammo pickup requested.");

                ShotgunWeapon shotgun = weapon.GetComponent<ShotgunWeapon>();

                if (shotgun == null)
                {
                    Debug.LogWarning("ShotgunWeapon script not found on: " + weapon.name);
                    continue;
                }

                Debug.Log("Found shotgun. Attempting to add " + amount + " ammo.");

                bool ammoAdded = shotgun.AddAmmo(amount);

                Debug.Log("Ammo added: " + ammoAdded);

                return ammoAdded;
            }

            if (ammoType == AmmoPickup.AmmoType.Sniper)
            {
                Debug.Log("Sniper ammo pickup requested.");

                SniperWeapon sniper = weapon.GetComponent<SniperWeapon>();

                if (sniper == null)
                {
                    Debug.LogWarning("SniperWeapon script not found on: " + weapon.name);
                    continue;
                }

                Debug.Log("Found sniper. Attempting to add " + amount + " ammo.");

                bool ammoAdded = sniper.AddAmmo(amount);

                Debug.Log("Sniper ammo added: " + ammoAdded);

                return ammoAdded;
            }
        }

        return false;
    }



}
