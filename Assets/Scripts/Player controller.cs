using UnityEngine;
using UnityEngine.InputSystem;

public class Playercontroller : MonoBehaviour
{
    
    [Range(1, 10)][SerializeField] float playerSpeed;

    Vector2 playerInput;
    Vector2 mousePos;
    Vector2 mouseWorldPos;
    Vector2 playerDir;

    Rigidbody2D rb;
    [SerializeField] RectTransform reticle;
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
        mousePos = Mouse.current.position.ReadValue();
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        playerDir = mouseWorldPos - new Vector2(transform.position.x, transform.position.y);
        float playerAngle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg;
        rb.MoveRotation(playerAngle - 90);
        ReticlePosition();
    }

    void ReticlePosition()
    {
        reticle.position = mousePos;
    }
}
