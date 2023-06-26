using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed;
    private float gravity = -9.81f;
    public float jump;

    public Transform groundCheck;
    private float groundDistance = 0.1f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    private Vector3 previousMove;


    void Update() {

        if (Input.GetKeyDown(KeyCode.LeftControl))
            speed = 9f;
        else if (Input.GetKeyDown(KeyCode.LeftShift)) {
            transform.localScale = new Vector3(transform.localScale.x, 0.6f, transform.localScale.z);
            speed = 2f;
        }
        else if (!Input.GetKey(KeyCode.LeftShift))
            transform.localScale = new Vector3(transform.localScale.x, 0.8f, transform.localScale.z);
        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
            speed = 5f;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = Vector3.Normalize(transform.right * x + transform.forward * z);

        move *= speed;

        if(Input.GetKey(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jump * -2f * (gravity*2f));

        velocity.y += (gravity*2f) * Time.deltaTime;

        if(isGrounded && velocity.y < 0)
            velocity.y = -2f;

        move += velocity;

        controller.Move(move * Time.deltaTime);

    }

}
