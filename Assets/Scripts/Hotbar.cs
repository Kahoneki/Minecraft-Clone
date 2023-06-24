using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public List<GameObject> outlines = new List<GameObject>();
    public int currentBlock;
    private bool blockChange = false; //Used to determine whether the highlighted hotbar slot needs to change

    void Start()
    {
        currentBlock = 0;
        blockChange = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            blockChange = true;
            currentBlock = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            blockChange = true;
            currentBlock = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            blockChange = true;
            currentBlock = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            blockChange = true;
            currentBlock = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            blockChange = true;
            currentBlock = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) {
            blockChange = true;
            currentBlock = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) {
            blockChange = true;
            currentBlock = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) {
            blockChange = true;
            currentBlock = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) {
            blockChange = true;
            currentBlock = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            blockChange = true;
            currentBlock = 9;
        }
        

        if (blockChange)
        {
            for (int i = 0; i < 10; i++)
            {
                if (i == currentBlock)
                    outlines[i].SetActive(true);
                else
                    outlines[i].SetActive(false);
            }
        }
    }
}
