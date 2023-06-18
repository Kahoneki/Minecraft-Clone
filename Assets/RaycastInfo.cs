using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInfo : MonoBehaviour
{
    public Camera gameCamera;
    public float reachDistance;

    public Hotbar hotbarUI;

    void Update()
    {
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, reachDistance))
        {
            Debug.DrawLine(gameCamera.transform.position, hitInfo.point,Color.green);
            if (Input.GetMouseButtonDown(1))
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                cube.GetComponent<MeshRenderer>().material = hotbarUI.blocks[Hotbar.currentBlock];
                cube.layer = LayerMask.NameToLayer("Ground");
                cube.transform.position = CalculatePlacePosition(hitInfo);
            }

            else if (Input.GetMouseButtonDown(0) && hitInfo.collider.tag != "Terrain")
            {
                Destroy(hitInfo.collider.gameObject);
            }
        }
    }


    Vector3 CalculatePlacePosition(RaycastHit hitInfo)
    {

        if (hitInfo.collider.tag == "Terrain")
        {
            print("Terrain hit.");
            float x = Mathf.Floor(hitInfo.point.x)+0.5f;
            float z = Mathf.Floor(hitInfo.point.z)+0.5f;
            return new Vector3(x, 0.5f, z);

        }

        return hitInfo.normal+hitInfo.collider.transform.position;
    }
}
