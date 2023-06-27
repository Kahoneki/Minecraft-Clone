using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WorldGenerator : MonoBehaviour
{
    Hotbar hotbar;

    private int atlasSize = 4;

    public int renderDistance;

    public int amplitude;
    public int localHeightVariation;
    public float noiseScaleX;
    public float noiseScaleZ;
    public float randomNoiseOffsetX;
    public float randomNoiseOffsetZ;

    public int treeDensity;
    public int minTreeHeight;
    public int maxTreeHeight;

    public Vector3Int maxChunkSize = new Vector3Int(40,40,40);
    public Vector3Int startingChunk = new Vector3Int(16,20,16);


    //Used for storing blocks to be passed on to other chunks - i.e. tree's whose leaves pass into different chunks - x,y,z,id
    public List<Vector4> blockBuffer = new List<Vector4>();


    public readonly Vector3[] localVertexPositions = 
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
    public readonly Vector2[] uvLookup =
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
    public readonly Vector3Int[] offsets = {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(0,-1,0)
    };
    public readonly byte[,] blockIDToTextureIDs = {
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
        {12,12,12,12,12,12}     //Leaves: 10
        // {15,15,12,13,13,13},    //Unlit furnace: 10
        // {15,15,14,13,13,13}     //Lit furnace: 11
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
    public Dictionary<Vector2Int, GameObject> allChunks = new Dictionary<Vector2Int, GameObject>();

    public Dictionary<int, Vector3Int[]> leafPosLookupTable = new Dictionary<int, Vector3Int[]>();

    public Material meshMaterial;
    public GameObject chunkPrefab;


    public Transform playerTransform;
    Vector2Int chunkLastPosition = new Vector2Int(1,0); //Chunk that the player was in on the previous frame - Initialised to 1,0 so that LoadChunks() runs on startup.

    void Start() {

        PopulateLeafLookupTable();

        hotbar = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<Hotbar>();

        randomNoiseOffsetX = Random.Range(0f, 100f);
        randomNoiseOffsetZ = Random.Range(0f, 100f);
        amplitude = 8;
        localHeightVariation = 6 * ((startingChunk.x+startingChunk.z)/2);
        noiseScaleX = (float)(startingChunk.x * 100);
        noiseScaleZ = (float)(startingChunk.z * 100);
    }


    void PopulateLeafLookupTable() {
        for (int i = minTreeHeight; i < maxTreeHeight; i++) {
            List<Vector3Int> currentTree = new List<Vector3Int>();
            for (int x=-2; x<=2; x++)
            for (int y=i-1; y<=i; y++)
            for (int z=-2; z<=2; z++)
                if (x != 0 || z!= 0) {
                    if (new Vector2Int(x,z) != new Vector2Int(-2,-2) &&
                        new Vector2Int(x,z) != new Vector2Int(2,-2) &&
                        new Vector2Int(x,z) != new Vector2Int(-2,2) &&
                        new Vector2Int(x,z) != new Vector2Int(2,2))
                        currentTree.Add(new Vector3Int(x,y,z));
                    if (y == i-1)
                        currentTree.Add(new Vector3Int(x,y,z));
                }
            
            //Adding tree top
            for (int y=i; y<i+2; y++) {
                currentTree.Add(new Vector3Int(-1,y,0));
                currentTree.Add(new Vector3Int(1,y,0));
                currentTree.Add(new Vector3Int(0,y,-1));
                currentTree.Add(new Vector3Int(0,y,1));
                currentTree.Add(new Vector3Int(0,y,0));
            }

            leafPosLookupTable.Add(i, currentTree.ToArray());
        }
    }


    void Update() {LoadChunks();}


    void LoadChunks() {
        Vector2Int currentChunk = new Vector2Int(Mathf.FloorToInt(playerTransform.position.x / startingChunk.x), Mathf.FloorToInt(playerTransform.position.z / startingChunk.z));

        //Don't run unless player has changed chunk since last frame.
        if (chunkLastPosition != currentChunk) {
            List<Vector2Int> ChunksVisibleToPlayer = new List<Vector2Int>();
            for (int x=currentChunk.x-renderDistance; x<=currentChunk.x+renderDistance; x++)
            for (int z=currentChunk.y-renderDistance; z<=currentChunk.y+renderDistance; z++)
                ChunksVisibleToPlayer.Add(new Vector2Int(x,z));
            
            foreach (KeyValuePair<Vector2Int, GameObject> chunk in allChunks)
                chunk.Value.SetActive(ChunksVisibleToPlayer.Contains(chunk.Key));
            foreach (Vector2Int visibleChunk in ChunksVisibleToPlayer) {
                if (!allChunks.ContainsKey(visibleChunk)) {
                    GameObject chunk = GameObject.Instantiate(chunkPrefab, new Vector3(visibleChunk.x*startingChunk.x, 0, visibleChunk.y*startingChunk.z), transform.rotation);
                    chunk.transform.parent = transform;
                    allChunks.Add(visibleChunk, chunk);

                    ChunkGenerator cg = chunk.GetComponent<ChunkGenerator>();
                    cg.wg = GetComponentInParent<WorldGenerator>();

                    Mesh mesh = new Mesh();
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    cg.GetComponent<MeshFilter>().mesh = mesh;
                    Renderer meshRenderer = cg.GetComponent<MeshRenderer>();
                    meshRenderer.material = meshMaterial;
                    
                    int xPos = (int)chunk.transform.position.x;
                    int zPos = (int)chunk.transform.position.z;

                    Thread thread = new Thread(() =>
                    {
                        (Vector3[], Vector2[], int[]) result = GenerateChunkData(cg, xPos, zPos, currentChunk);

                        MainThreadDispatcher.RunOnMainThread(() =>
                        {
                            mesh.Clear();
                            mesh.vertices = result.Item1;
                            mesh.uv = result.Item2;
                            mesh.triangles = result.Item3;
                            mesh.RecalculateNormals();
                            MeshCollider meshCollider = cg.GetComponent<MeshCollider>();
                            cg.GetComponent<MeshCollider>().sharedMesh = mesh;
                            cg.gameObject.layer = LayerMask.NameToLayer("Ground");
                        });
                    });
                    thread.Start();
                }
            }
        chunkLastPosition = currentChunk;
        }
    }


    (Vector3[], Vector2[], int[]) GenerateChunkData(ChunkGenerator cg, int xPos, int zPos, Vector2Int currentChunk) {
        return cg.PopulateInitialChunkData(xPos, zPos, hotbar, currentChunk);
    }
}
