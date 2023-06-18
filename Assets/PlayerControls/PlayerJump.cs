using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 1.5f; // The force applied to make the character jump
    public float groundCheckRadius = 0.2f; // The radius of the sphere used to check if the character is grounded
    public GameObject groundObject;
    public LayerMask groundLayer; // The layer(s) considered as ground

    private CharacterController characterController;
    private bool isGrounded;
    private Vector3 verticalVelocity = Vector3.zero;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Check if the character is grounded
        isGrounded = Physics.CheckSphere(groundObject.transform.position, groundCheckRadius, groundLayer);

        print(isGrounded);

        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // Ensures the character stays grounded by applying a small downward force
        }

        // Process jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y); // Calculate jump velocity
        }

        // Apply gravity
        verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        // Apply vertical velocity to the character controller
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}
