using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity;
    public Transform playerBody;
    float xRotation = 0f;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
            playerBody.Rotate(Vector3.up * Input.GetAxis("Mouse X")*mouseSensitivity);
            
            xRotation -= Input.GetAxis("Mouse Y")*mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation,-90f,90f);
            transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
    }
}
