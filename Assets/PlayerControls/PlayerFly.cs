using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFly : MonoBehaviour
{

    public float flightPower;
    private float velocity;

    public CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            velocity += flightPower;
        else if (Input.GetKeyDown(KeyCode.LeftShift))
            velocity -= flightPower;
        else if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.LeftShift))
            velocity = 0;
        
        controller.Move(Vector3.up * velocity * Time.deltaTime);
    }
}