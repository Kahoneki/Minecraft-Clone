using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{

    public Vector3Int maxChunkSize = new Vector3Int(40,10,40);
    public Vector3Int startingChunk = new Vector3Int(20,8,18);
    private bool[,,] blockAtPos;

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

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

    readonly Vector3Int[] offsets = {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0)
    };

    void Start() {

        blockAtPos = new bool[maxChunkSize.x,maxChunkSize.y,maxChunkSize.z];

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        PopulateInitialChunk();
        CreateMeshData();
        UpdateMesh();
    }

    void PopulateInitialChunk() {
        for (int x = 0; x < startingChunk.x; x++) {
            for (int y = 0; y < startingChunk.y; y++) {
                for (int z = 0; z < startingChunk.z; z++) {
                    print((x,y,z));
                    blockAtPos[x,y,z] = true;
                }
            }
        }
    }

    void CreateMeshData() {

        //Used for offsetting vertex indices
        int indexOffset = 0;
        print(blockAtPos[0,8,1]);
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
                        AddVertices(new Vector3(x,y,z));
                        AddTriangles(visibleFaces, indexOffset);

                        indexOffset+=24;
                    }
                }
            }
        }
    }

    void AddVertices(Vector3 localChunkPos) {
        for (int i = 0; i < 24; i++)
        {
            vertices.Add(localVertexPositions[i] + localChunkPos);
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


    public void AddBlock(Vector3Int blockPos)
    {
        blockAtPos[blockPos.x, blockPos.y, blockPos.z] = true;
        ClearTriangleAndVertexData();
        CreateMeshData();
        UpdateMesh();
    }


    public void RemoveBlock(Vector3Int blockPos)
    {
        print(blockPos);
        blockAtPos[blockPos.x, blockPos.y, blockPos.z] = false;
        ClearTriangleAndVertexData();
        CreateMeshData();
        UpdateMesh();
    }


    void ClearTriangleAndVertexData()
    {
        vertices.Clear();
        triangles.Clear();
    }


    void UpdateMesh() {

        print("Vertices count is: " + vertices.Count);
        print("Final triangles count is: " + triangles.Count);

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}