using System.Collections;
using System.Collections.Generic;
using System; //For string formatting
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{

    public bool capFPS;
    public int fpsCap;

    public Text fpsCount; int currentFPS;

    public Text position; public Transform playerTransform;

    void Start() {
        foreach(Transform child in transform)
            child.gameObject.SetActive(false);
        if (capFPS)
            Application.targetFrameRate = fpsCap;
        InvokeRepeating("UpdateFPS", 0f, 1f);
    }

    void Update() {

        
        if (Input.GetKeyDown(KeyCode.F3)) {
            foreach(Transform child in transform)
                child.gameObject.SetActive(!child.gameObject.activeSelf);
        }

        if (gameObject.activeSelf) {
            fpsCount.text = "FPS: " + currentFPS;

            position.text = String.Format("(x: {0}, y: {1}, z: {2})", 
            playerTransform.position.x.ToString("F2"), 
            playerTransform.position.y.ToString("F2"), 
            playerTransform.position.z.ToString("F2"));
        }
    }

    void UpdateFPS() {
        // if (capFPS)
        //     currentFPS = currentFPS > fpsCap ? fpsCap : (int)(1.0f/Time.deltaTime);
        // else
        currentFPS = (int)(1.0f/Time.deltaTime);
    }

}
