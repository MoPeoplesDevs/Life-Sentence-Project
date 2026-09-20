using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class Playercontroller : MonoBehaviour, IHeal, IDamage
{
    
    [SerializeField] GameObject playerCam;
	[Range(1, 10)][SerializeField] float playerSpeed;
    [Range(0,100)][SerializeField] int playerHP;

    int playerMaxHP = 100;

    Vector2 playerInput;
    Vector2 mousePos;
    Vector2 mouseWorldPos;
    Vector2 playerDir;

    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        TurnPlayer();
		RenderCamera();
	}

    void RenderCamera()
    {
        playerCam.transform.position = new Vector3(transform.position.x, transform.position.y, playerCam.transform.position.z);
    }

    void Movement()
    {
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
            if (controllerInput.x != 0 || controllerInput.y != 0)
            {
                // calculates the angle based on the direction of the right stick
                float controllerAngle = Mathf.Atan2(controllerInput.y, controllerInput.x) * Mathf.Rad2Deg;

                // rotates the player int he direction of the right stick
                rb.MoveRotation(controllerAngle - 90);

                // stops here so the mouse does not override the controller 
                return;
            }
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
        playerHP -= damage;

        if (playerHP <= 0)
        {
            playerHP = 0;
        }


    }


}
