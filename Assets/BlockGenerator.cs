using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class BlockGenerator : MonoBehaviour
{
    
    Mesh mesh;

    Vector3[] vertices;
    List<int> triangles;

    List<Vector3> facesToShow;

    Vector3[] offsets = {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0,0),
        new Vector3(-1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,-1,0)
    };

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[]
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

        triangles = new List<int>();
        facesToShow = FindVisibleFaces();

        foreach (Vector3 face in facesToShow)
        {
            if (face == offsets[0]) {
                int[] currentFace = {20,21,22,22,23,20};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[1]) {
                int[] currentFace = {16,17,18,18,19,16};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[2]) {
                int[] currentFace = {12,13,14,14,15,12};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[3]) {
                int[] currentFace = {8,9,10,10,11,8};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[4]) {
                int[] currentFace = {4,5,6,6,7,4};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[5]) {
                int[] currentFace = {0,1,2,2,3,0};
                triangles.AddRange(currentFace);
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
    }

    List<Vector3> FindVisibleFaces() {
        
        List<Vector3> visibleFaces = new List<Vector3>();
        Ray ray;

        foreach(Vector3 offset in offsets)
        {
            Vector3 offsetPosition = transform.position + offset;
            ray = new Ray(transform.position, offset);
            if (!Physics.Raycast(ray, 1))
            {
                print(offset);
                visibleFaces.Add(offset);
            }
        }

        return visibleFaces;
    }
}
