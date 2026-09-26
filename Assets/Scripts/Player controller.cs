using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class Playercontroller : MonoBehaviour, IHeal, IDamage
{

    [SerializeField] GameObject playerCam;
    [Range(1, 10)][SerializeField] float playerSpeed;
    [Range(0, 100)][SerializeField] int playerHP;
    [Range(2, 4)][SerializeField] float evadeSpeed;
    [Range(0, 5)][SerializeField] float evadeTime;
    [Range(1, 5)] [SerializeField] float evadeCooldown;
    float evadeDuration;
    float evadeCooldownTimer;
    float deathRotation;
    [SerializeField]float spinSpeed;

    bool isEvading;
    bool isInvincible;
    bool isDead;
    bool usingController;

    [SerializeField] GameObject bullet;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject loseMenu;

    int playerMaxHP = 100;

    Vector2 playerInput;
    Vector2 mousePos;
    Vector2 mouseWorldPos;
    Vector2 playerDir;
    [SerializeField] LayerMask wallLayer;

    Rigidbody2D rb;
    CircleCollider2D playerCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {

        if(isDead != false)
        {
            DeathSpin();
            return;
        }
        Movement();
        TurnPlayer();
		RenderCamera();
        Shoot();
        Evade();
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

        // checks if a controller is connected before trying to read controller inputs
        if (Gamepad.current != null)
        {
            //gets both the x and y movements fromthe controllers left stick
            Vector2 controllerInput = Gamepad.current.leftStick.ReadValue();

            // adds the controller's horizontal and vertical input to our movement
            x += controllerInput.x;
            y += controllerInput.y;
        }

        //stores the x and y movment
        playerInput = new Vector2(x, y);

        // moves the player
        rb.linearVelocity = playerInput.normalized * playerSpeed;
        
    }

    void TurnPlayer()
    {
      
        // Checks if a controller connected before reading controller inputs
        if (Gamepad.current != null)
        {
            // gets the horizontal and vertical input from the controllers right stick
            Vector2 controllerInput = Gamepad.current.rightStick.ReadValue();

            //checks if the right stick is being moved
            if (controllerInput.magnitude > 0.1f)
            {
                //sets controller aiming as the current aiming method
                usingController = true;

                // calculates the angle based on the direction of the right stick
                float controllerAngle = Mathf.Atan2(controllerInput.y, controllerInput.x) * Mathf.Rad2Deg;

                // rotates the player int he direction of the right stick
                rb.MoveRotation(controllerAngle - 90);

            }
            // stops here so the mouse does not override the controller 
            return;
        }

        //checks if the mouse has been moved 
        if(Mouse.current.delta.ReadValue().magnitude > 0.1f)
        {
            //Switcher the player back to mouse aiming
            usingController = false;
        }

        //Stops the mouse code from running while using the controller
        if(usingController)
        {
            return;
        }

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
        playerHP += healAmount;

        if (playerHP > playerMaxHP)
        { 
            playerHP = playerMaxHP;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible == false)
        {
            playerHP -= damage;

            if (playerHP <= 0)
            {
                playerHP = 0;
            }
            if(playerHP <= 0)
            {
                PlayerDeath();
            }
        }
    }

    void Shoot()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame || Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            Instantiate(bullet, firePoint.position, transform.rotation);
        }
    }

    void Evade()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame && evadeCooldownTimer <=0)
        {
            isEvading = true;
            isInvincible = true;

            playerCollider.includeLayers = ~wallLayer;

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

            Destroy(gameObject);
        }
    }


}
