using UnityEngine;
using System.Collections;

// This script moves the character controller forward
// and sideways based on the arrow keys.
// It also jumps when pressing space.
// Make sure to attach a character controller to the same game object.
// It is recommended that you make only one call to Move or SimpleMove per frame.

public class KeyboardMovement : MonoBehaviour
{
    CharacterController characterController;

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if (characterController.isGrounded)
        {
            // Get Horizontal and Vertical Input. 
            // These values will range between 0 and 1
            // Horizontal == side to side input
            // Vertical == front and backward input
            float sidewaysInput = Input.GetAxis("Horizontal");
            float forwardInput = Input.GetAxis("Vertical");

            // Calculate the Direction to Move based on the tranform of the Player
            // transform.forward is always pointing "forward" relative to the player
            // Scale the transform.forward vector by the amount of forward input
            // and same with sideways 
            Vector3 moveDirectionForward = transform.forward * forwardInput;
            Vector3 moveDirectionSide = transform.right * sidewaysInput;

            // Add the forward and sideways vectors together to get the direction
            // of movement, then normalize it
            Vector3 direction = (moveDirectionForward + moveDirectionSide).normalized;

            // Scale the direction by the speed (meters / second) and
            // multiply by the amount of time that has passed
            moveDirection = direction * speed * Time.deltaTime;


            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity.
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection);
    }
}