using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MapGenerator : NetworkBehaviour {

    public enum DrawMode {NoiseMap,ColourMap };
    public DrawMode drawMode;

    public GameObject tilePrefab;
    public GameObject treePrefab;
    Transform tileTransform;

    public Sprite spr1;
    public Sprite spr2;
    public Sprite spr3;
    public Sprite spr4;
    public Sprite spr5;
    public Sprite spr6;
    public Sprite spr7;
    public Sprite spr8;
    
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public NetworkHash128 assetId;


    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    [SyncVar]
    int ServerSeed;



    public Vector2 offset;
    


    public bool autoUpdate;

    void Start()
    {
        if (isServer)
            ServerSeed = (int)Random.Range(0.0f, 10000.0f);
        GenerateMap(250, 250, ServerSeed, 250, 25, 0.526f, 2);
    }


    public void GenerateMap(int mapWidth, int mapHeight, int ServerSeed, float noiseScale, int octaves, float persistance,float lacunarity)
    {
        //placing colours
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, ServerSeed, noiseScale, octaves, persistance, lacunarity, offset);
        offset.x = 0;
        offset.y = 0;
        drawMode = DrawMode.ColourMap;
        TerrainType[] regions = new TerrainType[8];
        regions[0].name = "Water Deep";
        regions[1].name = "Water Shallow";
        regions[2].name = "Sand";
        regions[3].name = "Grass";
        regions[4].name = "Grass 2";
        regions[5].name = "Rock";
        regions[6].name = "Rock 2";
        regions[7].name = "Snow";
        regions[0].height = 0.35f;
        regions[1].height = 0.5f;
        regions[2].height = 0.55f;
        regions[3].height = 0.65f;
        regions[4].height = 0.7f;
        regions[5].height = 0.73f;
        regions[6].height = 0.85f;
        regions[7].height = 1;
        regions[0].colour = new Color32(8, 112, 216, 0);
        regions[1].colour = new Color32(11, 127, 244, 0);
        regions[2].colour = new Color32(231, 240, 72, 0);
        regions[3].colour = new Color32(96, 195, 13, 0);
        regions[4].colour = new Color32(71, 150, 4, 0);
        regions[5].colour = new Color32(169, 169, 169, 0);
        regions[6].colour = new Color32(111, 111, 111, 0);
        regions[7].colour = new Color32(255, 255, 255, 0);
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float CurrentHeight = noiseMap[x, y];
                /*if (CurrentHeight > regions[2].height && CurrentHeight <= regions[3].height)
                {
                    Instantiate(tilePrefab, new Vector3((x+1)/(x+1)*(x+1), y, 0), tilePrefab.transform.rotation);
                }*/
                for (int i = 0; i < regions.Length; i++)
                {
                    if (CurrentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        
        
        // adding tiles
        MapDisplay display = FindObjectOfType<MapDisplay>();
        Texture2D texture = TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                
                var obj = Instantiate(tilePrefab, new Vector3(x, y, 0), tilePrefab.transform.rotation) as GameObject;
                if (texture.GetPixel(x, y) == regions[0].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr1;
                else if (texture.GetPixel(x, y) == regions[1].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr2;
                else if (texture.GetPixel(x, y) == regions[2].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr3;
                else if (texture.GetPixel(x, y) == regions[3].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr4;
                else if (texture.GetPixel(x, y) == regions[4].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr5;
                else if (texture.GetPixel(x, y) == regions[5].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr5;
                else if (texture.GetPixel(x, y) == regions[6].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr4;
                else if (texture.GetPixel(x, y) == regions[7].colour)
                    obj.GetComponent<SpriteRenderer>().sprite = spr5;

                //if (obj.GetComponent<SpriteRenderer>().sprite == spr1 || obj.GetComponent<SpriteRenderer>().sprite == spr2)
                //    obj.tag = "water";
                //   obj.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        //creating objects
        System.Random rng = new System.Random(ServerSeed);
        for (int i=0;i<20;i++)
            generateForest(rng, noiseMap, regions);

        //choosing draw mode
        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColourMap)
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));        

    }
    void generateForest(System.Random rng,float [,] noiseMap, TerrainType[] regions)
    {
        int buildCount = rng.Next(10, 30);
        GameObject buildObject = treePrefab;
        int length = rng.Next(15, 30);
        int width = rng.Next(10, 15);
        bool up = false, down = false;
        int xx = 0,yy = 0;

        int i, j;
        do
        {
            xx = rng.Next(1, 250);
            yy = rng.Next(1, 250);
        }
        while (noiseMap[xx, yy] < regions[3].height && xx+length < mapWidth && yy+width/2 < mapHeight);
        int yy_top = yy;
        int yy_bot = yy;

        int max_bot = yy - (width / 2);
        int max_top = yy + (width / 2);

        //forests
        for (i = 0; i < length && xx+i < mapWidth && xx + i > 0; i++)
        {
            if (yy_top == yy)
                up = false;
            if (up && yy_top > yy)
                yy_top -= rng.Next(0, 2);
            else if (yy_top == max_top)
                up = true;
            else if (yy_top < max_top)
                yy_top += rng.Next(0, 2);

            if (yy_bot == yy)
                down = false;
            if (down && yy_bot < yy)
                yy_bot += rng.Next(0, 2);
            else if (yy_bot == max_bot)
                down = true;
            else if (yy_bot > max_bot)
                yy_bot -= rng.Next(0, 2);

            for (j = yy_bot; j < yy_top && j < mapHeight && j > 0; j++)
            {
                Debug.Log("reaches forest creation");
                if (xx+i < mapWidth-1 && j < mapHeight - 2)
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(xx, yy), 8f, LayerMask.NameToLayer("LevelObject"));
                    if (colliders.Length == 0)
                    {
                        if (noiseMap[xx+i, j] >= regions[3].height)
                            Instantiate(buildObject, new Vector3(xx + i, j, -Mathf.RoundToInt(transform.position.y * 100)), buildObject.transform.rotation);
                    }
                }
            }
        }
        //random trees
        for (int a = 0; a < buildCount; a++)
        {
            do
            {
                xx = rng.Next(1, 250);
                yy = rng.Next(1, 250);
            }
            while (noiseMap[xx, yy] < regions[3].height);
            Debug.Log("reaches random tree creation");
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(xx, yy), 8f, LayerMask.NameToLayer("LevelObject"));
            if (colliders.Length == 0)
            {
                Debug.Log("actually creates tree");
                Instantiate(buildObject, new Vector3(xx, yy, -Mathf.RoundToInt(transform.position.y * 100)), buildObject.transform.rotation);
            }
        }
    }
    void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;
        if (octaves < 1)
            octaves = 1;
        if (lacunarity < 1)
            lacunarity = 1;
    }
    [Command]
    void CmdGenWorld()
    {
        TargetGenWorld(connectionToClient);
    }
    [TargetRpc]
    void TargetGenWorld(NetworkConnection target)
    {
        GenerateMap(1000, 1000, ServerSeed, 500, 25, 0.526f, 2);
    }
    [ClientRpc]
    void RpcGenWorld(int ServerSeed)
    {
        GenerateMap(1000, 1000, ServerSeed, 500, 25, 0.526f, 2);
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}