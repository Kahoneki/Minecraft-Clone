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

    public WorldGenerator wg;

    public GameObject blockHighlight;

    private readonly Dictionary<Vector3, Vector3> normalToHighlightRotation = new Dictionary<Vector3, Vector3>{
        {Vector3.up, Vector3.forward},
        {Vector3.down, Vector3.back},
        {Vector3.left, Vector3.back},
        {Vector3.right, Vector3.forward},
        {Vector3.forward, Vector3.forward},
        {Vector3.back, Vector3.back}
    };

    void Start() {
        currentDelay = 0;
    }

    void Update() {

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, reachDistance, (1 << LayerMask.NameToLayer("Ground")))) {
            //Sometimes normal will be a weird value, i.e. (0,0,0) or (1,0,1)
            if (normalToHighlightRotation.ContainsKey(hitInfo.normal)) {
                PlaceBlockHighlight(hitInfo);

                if (Input.GetMouseButton(0) && currentDelay <= 0f)
                    LeftClick(hitInfo);
                else if (Input.GetMouseButton(1) && currentDelay <= 0f)
                    RightClick(hitInfo);
                else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                    currentDelay = 0;
                currentDelay -= Time.deltaTime;
            }
        }
        else {
            blockHighlight.SetActive(false);
        }
    }


    void PlaceBlockHighlight(RaycastHit hitInfo) {
        blockHighlight.SetActive(true);

        hitInfo.normal = Vector3Int.FloorToInt(hitInfo.normal);

        float x = Mathf.Floor(hitInfo.point.x) + (hitInfo.normal.x * 0.01f);
        float y = Mathf.Floor(hitInfo.point.y) + (hitInfo.normal.y * 0.01f);
        float z = Mathf.Floor(hitInfo.point.z) + (hitInfo.normal.z * 0.01f);


        blockHighlight.transform.position = new Vector3(x,y,z);
        blockHighlight.transform.rotation = Quaternion.FromToRotation(hitInfo.normal, normalToHighlightRotation[hitInfo.normal]);
    }


    void LeftClick(RaycastHit hitInfo) {
        Vector3Int globalPos = CalculateDestroyPosition(hitInfo);

        int chunkX = Mathf.FloorToInt((float)globalPos.x / (float)wg.startingChunk.x);
        int chunkZ = Mathf.FloorToInt((float)globalPos.z / (float)wg.startingChunk.z);
        Vector2Int chunkIndex = new Vector2Int(chunkX, chunkZ);

        int localX = globalPos.x - (chunkX * wg.startingChunk.x);
        int localZ = globalPos.z - (chunkZ * wg.startingChunk.z);
        Vector3Int localPos = new Vector3Int(localX, globalPos.y, localZ);

        wg.allChunks[chunkIndex].GetComponent<ChunkGenerator>().RemoveBlock(localPos);
        currentDelay = breakPlaceDelay;
    }


    void RightClick(RaycastHit hitInfo) {
        Vector3Int globalPos = CalculatePlacePosition(hitInfo);
        Vector3Int cameraGlobalPos = new Vector3Int(Mathf.FloorToInt(gameCamera.transform.position.x),
                                                    Mathf.FloorToInt(gameCamera.transform.position.y),
                                                    Mathf.FloorToInt(gameCamera.transform.position.z));
        
        
        int chunkX = Mathf.FloorToInt((float)globalPos.x / (float)wg.startingChunk.x);
        int chunkZ = Mathf.FloorToInt((float)globalPos.z / (float)wg.startingChunk.z);
        Vector2Int chunkIndex = new Vector2Int(chunkX, chunkZ);

        int localX = globalPos.x - (chunkX*wg.startingChunk.x);
        int localZ = globalPos.z - (chunkZ*wg.startingChunk.z);
        Vector3Int localPos = new Vector3Int(localX, globalPos.y, localZ);

        int cameraLocalX = cameraGlobalPos.x - (chunkX*wg.startingChunk.x);
        int cameraLocalZ = cameraGlobalPos.z - (chunkX*wg.startingChunk.x);
        Vector3Int cameraLocalPos = new Vector3Int(cameraLocalX, cameraGlobalPos.y, cameraLocalZ);
        
        if (localPos != cameraLocalPos && localPos != cameraLocalPos+Vector3Int.down) {
            wg.allChunks[chunkIndex].GetComponent<ChunkGenerator>().AddBlock(localPos);
            currentDelay = breakPlaceDelay;
        }
    }


    Vector3Int CalculatePlacePosition(RaycastHit hitInfo) {
        int x = (int)Mathf.Floor(hitInfo.point.x + hitInfo.normal.x * 0.5f);
        int y = (int)Mathf.Floor(hitInfo.point.y + hitInfo.normal.y * 0.5f);
        int z = (int)Mathf.Floor(hitInfo.point.z + hitInfo.normal.z * 0.5f);

        return new Vector3Int(x, y, z);
    }


    Vector3Int CalculateDestroyPosition(RaycastHit hitInfo) {    
        int x = (int)Mathf.Floor(hitInfo.point.x - hitInfo.normal.x * 0.5f);
        int y = (int)Mathf.Floor(hitInfo.point.y - hitInfo.normal.y * 0.5f);
        int z = (int)Mathf.Floor(hitInfo.point.z - hitInfo.normal.z * 0.5f);

        return new Vector3Int(x, y, z);
    }
}
