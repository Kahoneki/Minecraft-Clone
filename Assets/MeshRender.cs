using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRender : MonoBehaviour
{

    public class Voxel {
        private Vector3 positionInWorldSpace;
        private Material material;
        private int[] triangles;

        public Voxel(Vector3 pos, Material mat)
        {
            positionInWorldSpace = pos;
            material = mat;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
