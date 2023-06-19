using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{

    public static int xSize = 40;
    public static int ySize = 1;
    public static int zSize = 40;
    private bool[,,] blockAtPos = new bool[xSize,ySize,zSize];

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    Vector3[] localVertexPositions = 
        {
            //y- vertices
            new Vector3(.5f, -.5f, .5f),    //0
            new Vector3(-.5f, -.5f, .5f),   //1
            new Vector3(-.5f, -.5f, -.5f),  //2
            new Vector3(.5f, -.5f, -.5f),   //3
            

            //y+ vertices
            new Vector3(.5f, .5f, -.5f),    //4
            new Vector3(-.5f, .5f, -.5f),   //5
            new Vector3(-.5f, .5f, .5f),    //6
            new Vector3(.5f, .5f, .5f),     //7

            //x- vertices
            new Vector3(-.5f, -.5f, -.5f),  //8
            new Vector3(-.5f, -.5f, .5f),   //9
            new Vector3(-.5f, .5f, .5f),    //10
            new Vector3(-.5f, .5f, -.5f),   //11

            //x+ vertices
            new Vector3(.5f, -.5f, .5f),    //12
            new Vector3(.5f, -.5f, -.5f),   //13
            new Vector3(.5f, .5f, -.5f),    //14
            new Vector3(.5f, .5f, .5f),     //15

            //z- vertices
            new Vector3(.5f, -.5f, -.5f),   //16
            new Vector3(-.5f, -.5f, -.5f),  //17
            new Vector3(-.5f, .5f, -.5f),   //18
            new Vector3(.5f, .5f, -.5f),    //19

            //z+ vertices
            new Vector3(-.5f, -.5f, .5f),   //20
            new Vector3(.5f, -.5f, .5f),    //21
            new Vector3(.5f, .5f, .5f),     //22
            new Vector3(-.5f, .5f, .5f)     //23
        };

    Vector3[] offsets = {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0,0),
        new Vector3(-1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,-1,0)
    };

    void Start() {

        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    blockAtPos[x,y,z] = true;
                }
            }
        }

        int indexOffset = 0;
        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    List<Vector3> visibleFaces = new List<Vector3>();
                    foreach (Vector3 offset in offsets) {

                        int offsetX = x + (int)offset.x;
                        int offsetY = y + (int)offset.y;
                        int offsetZ = z + (int)offset.z;

                        //Checking if in bounds of array
                        if (0 <= offsetX && offsetX < xSize)
                            if (0 <= offsetY && offsetY < ySize)
                                if (0 <= offsetZ && offsetZ < zSize)
                                {
                                    //Checking if there isn't a block at the offset position
                                    if (!blockAtPos[offsetX,offsetY,offsetZ]) {
                                        print("Passed.");
                                        visibleFaces.Add(offset);
                                    }
                                }
                                else
                                    visibleFaces.Add(offset);
                            else
                                visibleFaces.Add(offset);
                        else
                            visibleFaces.Add(offset);
                                
                    }
                    AddVertices(new Vector3(x,y,z));
                    AddTriangles(visibleFaces, indexOffset);

                    indexOffset+=24;
                }
            }
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }


    void AddVertices(Vector3 localChunkPos) {
        for (int i = 0; i < 24; i++)
        {
            vertices.Add(localVertexPositions[i] + localChunkPos);
        }
    }


    void AddTriangles(List<Vector3> visibleFaces, int indexOffset) {
        if (visibleFaces.Count != 0)
            print("Visible faces count is: " + visibleFaces.Count);
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
        // print("Current triangles count is: " + triangles.Count);
    }


    void UpdateMesh() {

        print("Vertices count is: " + vertices.Count);
        print("Final triangles count is: " + triangles.Count);

        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }
}