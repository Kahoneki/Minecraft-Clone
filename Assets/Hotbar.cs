using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    public List<Material> blocks = new List<Material>();
    public List<GameObject> outlines = new List<GameObject>();
    static public int currentBlock;
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

        if (blockChange)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == currentBlock)
                    outlines[i].SetActive(true);
                else
                    outlines[i].SetActive(false);
            }
        }
    }
}
