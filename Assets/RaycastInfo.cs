using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastInfo : MonoBehaviour
{
    public Camera gameCamera;
    public float reachDistance;

    public Hotbar hotbarUI;

    public ChunkGenerator chunkGenerator;

    void Update()
    {
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, reachDistance))
        {
            Debug.DrawLine(gameCamera.transform.position, hitInfo.point,Color.green);
            if (Input.GetMouseButtonDown(1))
                chunkGenerator.AddBlock(CalculatePlacePosition(hitInfo));

            else if (Input.GetMouseButtonDown(0))
            {
                Vector3Int destroyPosition = CalculateDestroyPosition(hitInfo);
                int x = destroyPosition.x;
                int y = destroyPosition.y;
                int z = destroyPosition.z;
                if (x >= 0 && x < chunkGenerator.maxChunkSize.x &&
                    y >= 0 && y < chunkGenerator.maxChunkSize.y &&
                    z >= 0 && z < chunkGenerator.maxChunkSize.z
                    )
                    chunkGenerator.RemoveBlock(destroyPosition);
            }
        }
    }


    Vector3Int CalculatePlacePosition(RaycastHit hitInfo)
    {

        int x = (int)Mathf.Floor(hitInfo.point.x);
        int y = (int)Mathf.Floor(hitInfo.point.y);
        int z = (int)Mathf.Floor(hitInfo.point.z);

        int normalX = (int)hitInfo.normal.x;
        int normalY = (int)hitInfo.normal.y;
        int normalZ = (int)hitInfo.normal.z;

        if (normalX < 0 || normalY < 0 || normalZ < 0)
            return new Vector3Int(x,y,z) + new Vector3Int(normalX,normalY,normalZ);

        else
            return new Vector3Int(x,y,z);
    }


    Vector3Int CalculateDestroyPosition(RaycastHit hitInfo)
    {
        int x = (int)Mathf.Floor(hitInfo.point.x);
        int y = (int)Mathf.Floor(hitInfo.point.y);
        int z = (int)Mathf.Floor(hitInfo.point.z);

        int normalX = (int)hitInfo.normal.x;
        int normalY = (int)hitInfo.normal.y;
        int normalZ = (int)hitInfo.normal.z;

        if (normalX > 0 || normalY > 0 || normalZ > 0)
            return new Vector3Int(x,y,z) - new Vector3Int(normalX,normalY,normalZ);
        else
            return new Vector3Int(x,y,z);
    }
}
