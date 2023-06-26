using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFly : MonoBehaviour
{

    public float speed;
    public float flightPower;
    private float velocity;

    public CharacterController controller;

    void Start() {controller = GetComponent<CharacterController>();}

    void Update() {

        Vector3 move = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        move *= speed;

        if (Input.GetKeyDown(KeyCode.Space))
            velocity += flightPower;
        else if (Input.GetKeyDown(KeyCode.LeftShift))
            velocity -= flightPower;
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.LeftShift))
            velocity = 0;
        
        move.y = velocity;

        controller.Move(move * Time.deltaTime);
    }
}