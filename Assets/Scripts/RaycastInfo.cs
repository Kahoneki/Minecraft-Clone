using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInfo : MonoBehaviour
{
    public Camera gameCamera;
    public float reachDistance;

    public float breakPlaceDelay; //Delay between breaking and placing blocks
    private float currentDelay;

    public Hotbar hotbarUI;

    public ChunkGenerator chunkGenerator;

    void Start() {
        currentDelay = 0;
    }

    void Update() {

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, reachDistance)) {
            if (Input.GetMouseButton(1) && currentDelay <= 0f) {
                Vector3Int placePosition = CalculatePlacePosition(hitInfo);
                int x = placePosition.x;
                int y = placePosition.y;
                int z = placePosition.z;
                if (x >= 0 && x < chunkGenerator.maxChunkSize.x &&
                    y >= 0 && y < chunkGenerator.maxChunkSize.y &&
                    z >= 0 && z < chunkGenerator.maxChunkSize.z
                    )
                chunkGenerator.AddBlock(CalculatePlacePosition(hitInfo));
                currentDelay = breakPlaceDelay;
            }

            else if (Input.GetMouseButton(0) && currentDelay <= 0f) {
                Vector3Int destroyPosition = CalculateDestroyPosition(hitInfo);
                int x = destroyPosition.x;
                int y = destroyPosition.y;
                int z = destroyPosition.z;
                if (x >= 0 && x < chunkGenerator.maxChunkSize.x &&
                    y >= 0 && y < chunkGenerator.maxChunkSize.y &&
                    z >= 0 && z < chunkGenerator.maxChunkSize.z
                    )
                    chunkGenerator.RemoveBlock(destroyPosition);
                currentDelay = breakPlaceDelay;
            }

            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                currentDelay = 0;
            
            currentDelay -= Time.deltaTime;
        }
    }


    Vector3Int CalculatePlacePosition(RaycastHit hitInfo)
    {
        int x = (int)Mathf.Floor(hitInfo.point.x + hitInfo.normal.x * 0.5f);
        int y = (int)Mathf.Floor(hitInfo.point.y + hitInfo.normal.y * 0.5f);
        int z = (int)Mathf.Floor(hitInfo.point.z + hitInfo.normal.z * 0.5f);

        return new Vector3Int(x, y, z);
    }


    Vector3Int CalculateDestroyPosition(RaycastHit hitInfo)
    {    
        int x = (int)Mathf.Floor(hitInfo.point.x - hitInfo.normal.x * 0.5f);
        int y = (int)Mathf.Floor(hitInfo.point.y - hitInfo.normal.y * 0.5f);
        int z = (int)Mathf.Floor(hitInfo.point.z - hitInfo.normal.z * 0.5f);

        return new Vector3Int(x, y, z);
    }
}
