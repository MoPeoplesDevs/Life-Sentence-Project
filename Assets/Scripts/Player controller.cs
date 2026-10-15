using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Playercontroller : MonoBehaviour, IHeal, IDamage
{
    public static event Action OnPlayerDeath;

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

    bool isEvading;
    bool isInvincible;
    bool isDead;
    private List<GameObject> weaponInventory = new List<GameObject>();
    private int currentWeapon;

    [SerializeField] GameObject startingWeapon;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform firePoint;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        laser = transform.Find("Red Dot");
        transform.position = playerSpawn.position;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        AddWeapon(startingWeapon);
    }

    // Update is called once per frame
    void Update()
    {

        if(isDead != false)
        {
            DeathSpin();
            rb.linearVelocity = new Vector3(0, 0, 0);
            return;
        } else
            loseMenu.SetActive(false);

        if (GameManager.Instance.IsGameOver)
        {
            rb.linearVelocity = new Vector3(0, 0, 0);
            return;
        }
        
        Movement();
        TurnPlayer();
		RenderCamera();
        Evade();
        SwitchWeapon();
        UseWeapon();
        UpdateLaser();
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

        // stores the players horizontal and vertical movement
        float x = 0;
        float y = 0;

        if (Keyboard.current != null)
        {
            //moves the direction when the specified key is held
            if (Keyboard.current.aKey.isPressed)
                x = -1;

            if (Keyboard.current.dKey.isPressed)
                x = 1;

            if (Keyboard.current.wKey.isPressed)
                y = 1;

            if (Keyboard.current.sKey.isPressed)
                y = -1;
        }
        
        //stores the x and y movment
        playerInput = new Vector2(x, y);

        // moves the player
        rb.linearVelocity = playerInput.normalized * playerSpeed;
        
    }

    void TurnPlayer()
    { 
        //Gets the current position of the mouse on the screen
        mousePos = Mouse.current.position.ReadValue();

        // converts teh mouses screen position into a posistion in the game would
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        //gets the direction fromthe player to the mouse
        playerDir = mouseWorldPos - new Vector2(transform.position.x, transform.position.y);
        
        // calculates the angle the player needs toward the mouse
        float playerAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg;

        // Rotates the player to face the calculated direction
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
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            Instantiate(bullet, firePoint.position, transform.rotation);
        }
    }

    void Evade()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
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
}
