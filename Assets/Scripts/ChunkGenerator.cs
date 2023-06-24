using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{
    public Hotbar hotbar;

    public Vector3Int maxChunkSize = new Vector3Int(40,40,40);
    public Vector3Int startingChunk = new Vector3Int(16,20,16);
    private bool[,,] blockAtPos;
    private byte[,,] blockID;

    private int atlasSize = 4;

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    readonly Vector3[] localVertexPositions = 
        {
        //y- vertices
        new Vector3(1, 0, 1),    //0
        new Vector3(0, 0, 1),   //1
        new Vector3(0, 0, 0),  //2
        new Vector3(1, 0, 0),   //3
        
        //y+ vertices
        new Vector3(1, 1, 0),    //4
        new Vector3(0, 1, 0),   //5
        new Vector3(0, 1, 1),    //6
        new Vector3(1, 1, 1),     //7

        //x- vertices
        new Vector3(0, 0, 0),  //8
        new Vector3(0, 0, 1),   //9
        new Vector3(0, 1, 1),    //10
        new Vector3(0, 1, 0),   //11

        //x+ vertices
        new Vector3(1, 0, 1),    //12
        new Vector3(1, 0, 0),   //13
        new Vector3(1, 1, 0),    //14
        new Vector3(1, 1, 1),     //15

        //z- vertices
        new Vector3(1, 0, 0),   //16
        new Vector3(0, 0, 0),  //17
        new Vector3(0, 1, 0),   //18
        new Vector3(1, 1, 0),    //19

        //z+ vertices
        new Vector3(0, 0, 1),   //20
        new Vector3(1, 0, 1),    //21
        new Vector3(1, 1, 1),     //22
        new Vector3(0, 1, 1)     //23
        };

    readonly Vector2[] uvLookup =
            {
                //y- uvs
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1),

                //y+ uvs
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0),
                new Vector2(0,0),

                //x- uvs
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1),
                new Vector2(0,0),

                //x+ uvs
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1),
                new Vector2(0,0),

                //z- uvs
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1),

                //z+ uvs
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1)
            };

    readonly Vector3Int[] offsets = {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0)
    };

    readonly byte[,] blockIDToTextureIDs = {
        //Pass in block id as index to get array of corresponding texture ids in texture atlas
        //Order: y-, y+, x-, x+, z-, z+

        {0,0,0,0,0,0},          //Stone: 0
        {1,1,1,1,1,1},          //Dirt: 1
        {1,7,2,2,2,2},          //Grass: 2
        {3,3,3,3,3,3},          //Coal ore: 3
        {4,4,4,4,4,4},          //Oak planks: 4
        {6,6,5,5,5,5},          //Oak log: 5
        {8,8,8,8,8,8},          //Cobblestone: 6
        {9,9,9,9,9,9},          //Bedrock: 7
        {10,10,10,10,10,10},    //Sand: 8
        {11,11,11,11,11,11},    //Bricks: 9
        {15,15,12,13,13,13},    //Unlit furnace: 10
        {15,15,14,13,13,13}     //Lit furnace: 11
    };

    public List<Vector2> TextureIDToUVCoords(byte textureID) {

        List<Vector2> uvCoords = new List<Vector2>();
        float floatTextureID = (float)textureID; //Using standard arithmetic with bytes is weird

        //Bottom left
        float y1 = Mathf.Floor(floatTextureID / atlasSize);
        y1 = 0.75f-(y1/atlasSize);

        float x1 = floatTextureID % atlasSize;
        
        x1 /= (float)atlasSize;


        //Top right
        float y2 = y1+0.25f;
        float x2 = x1+0.25f;


        uvCoords.Add(new Vector2(x1,y1));
        uvCoords.Add(new Vector2(x2,y1));
        uvCoords.Add(new Vector2(x2,y2));
        uvCoords.Add(new Vector2(x1,y2));

        return uvCoords;
    }

    public Material meshMaterial;

    void Start() {
        blockAtPos = new bool[maxChunkSize.x,maxChunkSize.y,maxChunkSize.z];
        blockID = new byte[maxChunkSize.x,maxChunkSize.y,maxChunkSize.z];

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        Renderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = meshMaterial;

        PopulateInitialChunk();
        CreateMeshData();
        UpdateMesh();
    }

    void PopulateInitialChunk() {

        //Initiating all blocks to air blocks
        for (int x = 0; x < maxChunkSize.x; x++) {
            for (int y = 0; y < maxChunkSize.y; y++) {
                for (int z = 0; z < maxChunkSize.z; z++) {
                    blockID[x,y,z] = 255;
                }
            }
        }

        //Filling in starting blocks
        for (int x = 0; x < startingChunk.x; x++) {
            for (int y = 0; y < startingChunk.y; y++) {
                for (int z = 0; z < startingChunk.z; z++) {
                    blockAtPos[x,y,z] = true;

                    blockID[x,y,z] = StartingChunkBlockLookup(y);

                    //Display all block types in a line:
                    // blockID[x,y,z] = (byte)z;
                }
            }
        }
    }

    byte StartingChunkBlockLookup(int y) {
        int max = startingChunk.y;

        //Coal Generation
        if (y != 0 && y == Random.Range(0,max/2))
            return 3;

        if (y > max-(max/3)) {
            if (y == max-1)
                return 2;
            else
                return 1;
        }

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
        for (int x = 0; x < maxChunkSize.x; x++) {
            for (int y = 0; y < maxChunkSize.y; y++) {
                for (int z = 0; z < maxChunkSize.z; z++) {
                    //Checking if block should be rendered
                    if (blockAtPos[x,y,z]) {
                        List<Vector3> visibleFaces = new List<Vector3>();
                        foreach (Vector3 offset in offsets) {

                            int offsetX = x + (int)offset.x;
                            int offsetY = y + (int)offset.y;
                            int offsetZ = z + (int)offset.z;

                            //Checking if out of bounds of array (at edge of chunk)
                            if (
                                offsetX < 0 || offsetX >= maxChunkSize.x ||
                                offsetY < 0 || offsetY >= maxChunkSize.y ||
                                offsetZ < 0 || offsetZ >= maxChunkSize.z
                                )
                                visibleFaces.Add(offset);

                            else if (!blockAtPos[offsetX,offsetY,offsetZ])
                                visibleFaces.Add(offset);
                        }
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
        for (int i = 0; i < 24; i++)
            vertices.Add(localVertexPositions[i] + localChunkPos);
    }


    void AddUVs(Vector3Int localChunkPos) {
    byte currentBlock = blockID[localChunkPos.x, localChunkPos.y, localChunkPos.z];
    if (currentBlock != 255) {
        for (int i = 0; i < 6; i++) {
            byte currentFaceTextureID = blockIDToTextureIDs[currentBlock,i];
            List<Vector2> currentFaceUVCoords = TextureIDToUVCoords(currentFaceTextureID);
            
            uvs.Add(currentFaceUVCoords[0]);
            uvs.Add(currentFaceUVCoords[1]);
            uvs.Add(currentFaceUVCoords[2]);
            uvs.Add(currentFaceUVCoords[3]);
            }
        }
    }


    void AddTriangles(List<Vector3> visibleFaces, int indexOffset) {
        List<int> voxelTriangles = new List<int>();

        foreach (Vector3 face in visibleFaces)
        {
            if (face == offsets[0]) {
                int[] currentFace = {20,21,22,22,23,20};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == offsets[1]) {
                int[] currentFace = {16,17,18,18,19,16};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == offsets[2]) {
                int[] currentFace = {12,13,14,14,15,12};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == offsets[3]) {
                int[] currentFace = {8,9,10,10,11,8};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == offsets[4]) {
                int[] currentFace = {4,5,6,6,7,4};
                voxelTriangles.AddRange(currentFace);
            }
            else if (face == offsets[5]) {
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
