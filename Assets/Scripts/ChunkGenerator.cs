using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{
    public Hotbar hotbar;

    private int amplitude;
    private int minPerlinNoiseHeight;
    private bool[,,] blockAtPos;
    private byte[,,] blockID;

    private bool firstGeneration;

    private WorldGenerator wg;

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    void Start() {

        hotbar = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<Hotbar>();

        wg = GetComponentInParent<WorldGenerator>();

        firstGeneration = true;

        blockAtPos = new bool[wg.maxChunkSize.x, wg.maxChunkSize.y, wg.maxChunkSize.z];
        blockID = new byte[wg.maxChunkSize.x, wg.maxChunkSize.y, wg.maxChunkSize.z];

        amplitude = 10;
        minPerlinNoiseHeight = wg.startingChunk.y-(int)(wg.startingChunk.y/4);

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        Renderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = wg.meshMaterial;

        PopulateInitialChunk();
        CreateMeshData();
        UpdateMesh();
        firstGeneration = false;
    }

    void PopulateInitialChunk() {

        //Initiating all blocks to air blocks
        for (int x = 0; x < wg.maxChunkSize.x; x++) {
            for (int y = 0; y < wg.maxChunkSize.y; y++) {
                for (int z = 0; z < wg.maxChunkSize.z; z++) {
                    blockID[x,y,z] = 255;
                }
            }
        }

        //Filling in starting blocks

        float randomNoiseOffsetX = Random.Range(0f, 100f);
        float randomNoiseOffsetZ = Random.Range(0f, 100f);

        for (int x = 0; x < wg.startingChunk.x; x++) {
            for (int y = 0; y < wg.startingChunk.y; y++) {
                for (int z = 0; z < wg.startingChunk.z; z++) {

                    if (y >= minPerlinNoiseHeight && firstGeneration) {
                        int height = minPerlinNoiseHeight + GetHeightAtPos(x + randomNoiseOffsetX,z + randomNoiseOffsetZ);
                        for (int i=minPerlinNoiseHeight; i<=height; i++) {
                            blockAtPos[x, i, z] = true;
                            blockID[x,i,z] = StartingChunkBlockLookup(i);
                        }
                    }
                    else {
                        blockAtPos[x, y, z] = true;
                        blockID[x,y,z] = StartingChunkBlockLookup(y);
                    }
                }
            }
        }
    }


    int GetHeightAtPos(float x, float z) {
        float xScale = (float)wg.startingChunk.x;
        float zScale = (float)wg.startingChunk.z;

        float xOrg = ((x/xScale));
        float zOrg = ((z/zScale));

        return (int)((Mathf.PerlinNoise(xOrg, zOrg) * amplitude));
    }


    byte StartingChunkBlockLookup(int y) {
        int max = wg.startingChunk.y;

        //Coal Generation
        if (y != 0 && y == Random.Range(0,max/2))
            return 3;

        if (y > max-(max/3))
            return 1;

        else {
            if (y == 0)
                return 7;
            else
                return 0;
        }
    }


    void CreateMeshData() {

        //Used for offsetting vertex indices
        int indexOffset = 0;
        for (int x = 0; x < wg.maxChunkSize.x; x++) {
            for (int y = 0; y < wg.maxChunkSize.y; y++) {
                for (int z = 0; z < wg.maxChunkSize.z; z++) {
                    //Checking if block should be rendered
                    if (blockAtPos[x,y,z]) {
                        List<Vector3> visibleFaces = new List<Vector3>();
                        foreach (Vector3 offset in wg.offsets) {

                            int offsetX = x + (int)offset.x;
                            int offsetY = y + (int)offset.y;
                            int offsetZ = z + (int)offset.z;

                            //Checking if out of bounds of array (at edge of chunk)
                            if (
                                offsetX < 0 || offsetX >= wg.maxChunkSize.x ||
                                offsetY < 0 || offsetY >= wg.maxChunkSize.y ||
                                offsetZ < 0 || offsetZ >= wg.maxChunkSize.z
                                )
                                visibleFaces.Add(offset);

                            else if (!blockAtPos[offsetX,offsetY,offsetZ])
                                visibleFaces.Add(offset);
                        }

                        if (visibleFaces.Contains(wg.offsets[4]) && firstGeneration) //Top layer
                            blockID[x,y,z] = 2;

                        AddVertices(new Vector3Int(x,y,z));
                        AddUVs(new Vector3Int(x,y,z));
                        AddTriangles(visibleFaces, indexOffset);

                        indexOffset+=24;
                    }
                }
            }
        }
    }


    void AddVertices(Vector3Int localChunkPos) {
        for (int i = 0; i < 24; i++) {
            vertices.Add(wg.localVertexPositions[i] + localChunkPos);
        }
    }


    void AddUVs(Vector3Int localChunkPos) {
    byte currentBlock = blockID[localChunkPos.x, localChunkPos.y, localChunkPos.z];

    if (currentBlock == 255) return;

    for (int i = 0; i < 6; i++) {
        byte currentFaceTextureID = wg.blockIDToTextureIDs[currentBlock,i];
        List<Vector2> currentFaceUVCoords = wg.TextureIDToUVCoords(currentFaceTextureID);
        
        uvs.Add(currentFaceUVCoords[0]);
        uvs.Add(currentFaceUVCoords[1]);
        uvs.Add(currentFaceUVCoords[2]);
        uvs.Add(currentFaceUVCoords[3]);
        }
    }


    void AddTriangles(List<Vector3> visibleFaces, int indexOffset) {
        List<int> voxelTriangles = new List<int>();

        foreach (Vector3 face in visibleFaces)
        {
            if (face == wg.offsets[0]) {
                int[] currentFace = {20,21,22,22,23,20};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == wg.offsets[1]) {
                int[] currentFace = {16,17,18,18,19,16};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == wg.offsets[2]) {
                int[] currentFace = {12,13,14,14,15,12};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == wg.offsets[3]) {
                int[] currentFace = {8,9,10,10,11,8};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == wg.offsets[4]) {
                int[] currentFace = {4,5,6,6,7,4};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == wg.offsets[5]) {
                int[] currentFace = {0,1,2,2,3,0};
                voxelTriangles.AddRange(currentFace);
            }
        }

        for (int i = 0; i < voxelTriangles.Count; i++)
        {
            voxelTriangles[i] += indexOffset;
        }
        triangles.AddRange(voxelTriangles);
    }


    public void AddBlock(Vector3Int blockPos) {
        blockAtPos[blockPos.x, blockPos.y, blockPos.z] = true;
        blockID[blockPos.x, blockPos.y, blockPos.z] = (byte)hotbar.currentBlock;
        ClearChunkData();
        CreateMeshData();
        UpdateMesh();
    }


    public void RemoveBlock(Vector3Int blockPos) {
        blockAtPos[blockPos.x, blockPos.y, blockPos.z] = false;

        ClearChunkData();
        CreateMeshData();
        UpdateMesh();
    }


    void ClearChunkData() {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }


    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
