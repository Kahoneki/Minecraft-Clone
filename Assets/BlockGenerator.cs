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
            new Vector3(-.5f, -.5f, -.5f),
            new Vector3(-.5f, -.5f, .5f),
            new Vector3(.5f, -.5f, -.5f),
            new Vector3(.5f, -.5f, .5f),

            //y+ vertices
            new Vector3(-.5f, .5f, -.5f),
            new Vector3(-.5f, .5f, .5f),
            new Vector3(.5f, .5f, -.5f),
            new Vector3(.5f, .5f, .5f)
        };

        triangles = new List<int>();
        facesToShow = FindVisibleFaces();

        foreach (Vector3 face in facesToShow)
        {
            if (face == offsets[0]) {
                int[] currentFace = {3,7,1,7,5,1};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[1]) {
                int[] currentFace = {0,4,2,4,6,2};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[2]) {
                int[] currentFace = {2,6,3,6,7,3};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[3]) {
                int[] currentFace = {1,5,0,5,4,0};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[4]) {
                int[] currentFace = {4,5,6,5,7,6};
                triangles.AddRange(currentFace);
            }
            else if (face == offsets[5]) {
                int[] currentFace = {2,1,0,2,3,1};
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
