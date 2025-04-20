using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public FixedJoystick joystick;
    public float SpeedMove = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public bool isFrozen = false;




    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Movement
        if (isFrozen)
        {
            // reset any momentum or input
            velocity = Vector3.zero;
            return;
        }


        Vector3 move = transform.right * -joystick.Horizontal + transform.forward * -joystick.Vertical;
        controller.Move(move * SpeedMove * Time.deltaTime);
        


    // Gravity
    velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Call this from a UI Button
    public void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

}
