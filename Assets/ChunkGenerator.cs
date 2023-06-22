using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{
    public WorldGenerator worldGenerator;

    public Vector3Int maxChunkSize = new Vector3Int(40,40,40);
    public Vector3Int startingChunk = new Vector3Int(16,8,16);
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

        {0,0,0,0,0,0}, //Stone: 0
        {1,1,1,1,1,1}, //Dirt: 1
        {1,7,2,2,2,2} //Grass: 2

    };

    public Vector2 TextureIDToUVCoord(byte textureID) {
        int y = textureID / atlasSize;
        y = (int)0.75-(y/atlasSize);

        int x = textureID % atlasSize;
        x = x*(1/atlasSize);

        return new Vector2(x,y);
    } 

    public Material meshMaterial;

    void Start() {
        blockAtPos = new bool[maxChunkSize.x,maxChunkSize.y,maxChunkSize.z];
        blockID = new byte[maxChunkSize.x,maxChunkSize.y,maxChunkSize.z];

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        PopulateInitialChunk();
        CreateMeshData();
        UpdateMesh();

        Renderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = meshMaterial;
    }

    void PopulateInitialChunk() {
        for (int x = 0; x < startingChunk.x; x++) {
            for (int y = 0; y < startingChunk.y; y++) {
                for (int z = 0; z < startingChunk.z; z++) {
                    blockAtPos[x,y,z] = true;
                    if (y > (int)(startingChunk.y/2))
                        blockID[x,y,z] = 1; //Dirt
                    else
                        blockID[x,y,z] = 0; //Stone
                }
            }
        }
        for (int x = 0; x < maxChunkSize.x; x++) {
            for (int y = 0; y < maxChunkSize.y; y++) {
                for (int z = 0; z < maxChunkSize.z; z++) {
                    blockID[x,y,z] = 255;
                }
            }
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
                        AddUVs(new Vector3Int(x,y,z), visibleFaces);
                        // AddUVs(new Vector3Int(x,y,z));
                        AddTriangles(visibleFaces, indexOffset);

                        indexOffset+=24;
                    }
                }
            }
        }
    }


    void AddVertices(Vector3Int localChunkPos) {
        for (int i = 0; i < 24; i++)
        {
            vertices.Add(localVertexPositions[i] + localChunkPos);
        }
    }


    void AddUVs(Vector3Int localChunkPos, List<Vector3> visibleFaces) {
    byte currentBlock = blockID[localChunkPos.x, localChunkPos.y, localChunkPos.z];
    for (int i = 0; i < 6; i++) {
        switch (i) {
            case 0: // y- face
                uvs.Add(uvLookup[0]);
                uvs.Add(uvLookup[1]);
                uvs.Add(uvLookup[2]);
                uvs.Add(uvLookup[3]);
                break;
            case 1: // y+ face
                uvs.Add(uvLookup[4]);
                uvs.Add(uvLookup[5]);
                uvs.Add(uvLookup[6]);
                uvs.Add(uvLookup[7]);
                break;
            case 2: // x- face
                uvs.Add(uvLookup[8]);
                uvs.Add(uvLookup[9]);
                uvs.Add(uvLookup[10]);
                uvs.Add(uvLookup[11]);
                break;
            case 3: // x+ face
                uvs.Add(uvLookup[12]);
                uvs.Add(uvLookup[13]);
                uvs.Add(uvLookup[14]);
                uvs.Add(uvLookup[15]);
                break;
            case 4: // z- face
                uvs.Add(uvLookup[16]);
                uvs.Add(uvLookup[17]);
                uvs.Add(uvLookup[18]);
                uvs.Add(uvLookup[19]);
                break;
            case 5: // z+ face
                uvs.Add(uvLookup[20]);
                uvs.Add(uvLookup[21]);
                uvs.Add(uvLookup[22]);
                uvs.Add(uvLookup[23]);
                break;
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

        print(vertices.Count);
        print(uvs.Count);
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
