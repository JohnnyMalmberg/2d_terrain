using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class SpriteTerrain : MonoBehaviour
{
    //Mesh mesh;
    //List<Vector3> vertices;
    //List<int> triangles;
    //List<Color> colors;
    //List<Vector2> uvs;
    GameObject[] tileRenderers;
    SpriteRenderer[] spriteRenderers;

    public int drawScale = 1;
    int resolution;
    int previousTileCount;

    public Sprite debugSprite;
    //public Sprite[] terrainSprites;

    //public QuadTree tree;

    // Start is called before the first frame update
    void Start()
    {
        resolution = (int)Mathf.Pow(2, 12);
        if (resolution > 8192)
        {
            Debug.LogWarning("Extreme world size may consume excessive computer resources");
        }
        //Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        //Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);
        //this.tree = new QuadTree(worldBottomLeft, resolution, new Tile(false, false, false, false, 0));

        //GetComponent<MeshFilter>().mesh = this.mesh = new Mesh();
        //this.mesh.name = "Main Terrain Mesh";
        //this.vertices = new List<Vector3>();
        //this.uvs = new List<Vector2>();
        //this.triangles = new List<int>();
        //this.colors = new List<Color>();
        Vector3 screenBL = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(-1f, -1f, 0f);
        Vector3 screenTR = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);
        Vector3 screenDims = screenTR - screenBL;
        int tileRendererCount = ((int)screenDims.x + 1) * ((int)screenDims.y + 1);
        tileRendererCount *= 2;
        this.tileRenderers = new GameObject[tileRendererCount];
        this.spriteRenderers = new SpriteRenderer[tileRendererCount];
        for (int i = 0; i < tileRendererCount; i++)
        {
            this.tileRenderers[i] = new GameObject();
            this.tileRenderers[i].AddComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Tiled;
            this.spriteRenderers[i] = this.tileRenderers[i].GetComponent<SpriteRenderer>();
            this.tileRenderers[i].SetActive(true);
        }

        previousTileCount = tileRendererCount;

        World.SetResolution(resolution);
        Thread worldGenThread = new Thread(new ThreadStart(World.Generate));
        worldGenThread.Start();

        TileInfo.BuildSpriteDictionary();

        //UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (World.treeExists)
        {
            UpdateSprites();
            World.Tick();
        }
    }

    void UpdateSprites()
    {
        int tileCount = PlaceTiles(World.tree);
        Vector3 offScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);
        for (int i = tileCount; i < tileRenderers.Length; i++)
        {
            tileRenderers[i].transform.position = offScreen;
        }
    }

    int PlaceTiles(QuadTree tree, int spriteRendererIndex = 0)
    {
        Coordinate bl = tree.bottomLeft;
        Coordinate tr = tree.topRight;
        Vector3 screenBL = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(-1f, -1f, 0f);
        Vector3 screenTR = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);

        if ((bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
        {
            return spriteRendererIndex;
        }


        bool hasSubTree = false; // TODO: Make this a property of the tree itself
        if (tree.scale > drawScale)
        {
            for (int i = 0; i < 4; i++)
            {
                if (tree.subTrees[i] != null)
                {
                    spriteRendererIndex = PlaceTiles(tree.subTrees[i], spriteRendererIndex);
                    hasSubTree = true;
                }
            }
        }
        if (tree.tile.IsFilled())
        {
            if (!hasSubTree)
            {
                TID id = tree.tile.ID();
                int spriteIndex = tree.tile.SpriteIndex();
                Sprite sprite = TileInfo.sprites[id][spriteIndex];
                if (tree.scale < 128)
                {
                    this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x, bl.y, 10);
                    spriteRenderers[spriteRendererIndex].sprite = sprite;
                    spriteRenderers[spriteRendererIndex].size = new Vector2(tree.scale, tree.scale);
                    spriteRenderers[spriteRendererIndex].color = Color.white;
                    spriteRendererIndex++;
                }
                else
                {
                    Vector2 srSize = new Vector2(64, 64);
                    for (int i = 0; i < tree.scale; i += 64)
                    {
                        for (int j = 0; j < tree.scale; j += 64)
                        {
                            if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i + 64 < screenBL.x || bl.y + j + 64 < screenBL.y))
                            {
                                this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 10);
                                spriteRenderers[spriteRendererIndex].sprite = sprite;
                                spriteRenderers[spriteRendererIndex].size = srSize;
                                spriteRenderers[spriteRendererIndex].color = Color.white;
                                spriteRendererIndex++;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int quadIndex = 0; quadIndex < 4; quadIndex++)
                {
                    if (tree.subTrees[quadIndex] == null)
                    {
                        bl = tree.GetSubTreeCoordinate(quadIndex);
                        tr = bl + (tree.scale / 2);
                        if (!(bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
                        {
                            TID id = tree.tile.ID();
                            int spriteIndex = tree.tile.SpriteIndex();
                            Sprite sprite = TileInfo.sprites[id][spriteIndex];
                            if (tree.scale / 2 < 128)
                            {
                                this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x, bl.y, 10);
                                spriteRenderers[spriteRendererIndex].sprite = sprite;
                                spriteRenderers[spriteRendererIndex].size = new Vector2(tree.scale / 2, tree.scale / 2);
                                spriteRenderers[spriteRendererIndex].color = Color.white;
                                spriteRendererIndex++;
                            }
                            else
                            {
                                Vector2 srSize = new Vector2(64, 64);
                                for (int i = 0; i < tree.scale / 2; i += 64)
                                {
                                    for (int j = 0; j < tree.scale / 2; j += 64)
                                    {
                                        if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i + 64 < screenBL.x || bl.y + j + 64 < screenBL.y))
                                        {
                                            this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 10);
                                            spriteRenderers[spriteRendererIndex].sprite = sprite;
                                            spriteRenderers[spriteRendererIndex].size = srSize;
                                            spriteRenderers[spriteRendererIndex].color = Color.white;
                                            spriteRendererIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (tree.tile.IsBacked())
            {
                if (!hasSubTree)
                {
                    TID id = tree.tile.ID();
                    int spriteIndex = tree.tile.SpriteIndex();
                    Sprite sprite = TileInfo.sprites[id][spriteIndex];
                    if (tree.scale < 128)
                    {
                        this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x, bl.y, 15);
                        spriteRenderers[spriteRendererIndex].sprite = sprite;
                        spriteRenderers[spriteRendererIndex].size = new Vector2(tree.scale, tree.scale);
                        spriteRenderers[spriteRendererIndex].color = new Color(0.75f, 0.75f, 0.75f);
                        spriteRendererIndex++;
                    }
                    else
                    {
                        Vector2 srSize = new Vector2(64, 64);
                        for (int i = 0; i < tree.scale; i += 64)
                        {
                            for (int j = 0; j < tree.scale; j += 64)
                            {
                                if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i + 64 < screenBL.x || bl.y + j + 64 < screenBL.y))
                                {
                                    this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 15);
                                    spriteRenderers[spriteRendererIndex].sprite = sprite;
                                    spriteRenderers[spriteRendererIndex].size = srSize;
                                    spriteRenderers[spriteRendererIndex].color = new Color(0.75f, 0.75f, 0.75f);
                                    spriteRendererIndex++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int quadIndex = 0; quadIndex < 4; quadIndex++)
                    {
                        if (tree.subTrees[quadIndex] == null)
                        {
                            bl = tree.GetSubTreeCoordinate(quadIndex);
                            tr = bl + (tree.scale / 2);
                            if (!(bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
                            {
                                TID id = tree.tile.ID();
                                int spriteIndex = tree.tile.SpriteIndex();
                                Sprite sprite = TileInfo.sprites[id][spriteIndex];
                                if (tree.scale / 2 < 128)
                                {
                                    this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x, bl.y, 15f);
                                    spriteRenderers[spriteRendererIndex].sprite = sprite;
                                    spriteRenderers[spriteRendererIndex].size = new Vector2(tree.scale / 2, tree.scale / 2);
                                    spriteRenderers[spriteRendererIndex].color = new Color(0.75f, 0.75f, 0.75f);
                                    spriteRendererIndex++;
                                }
                                else
                                {
                                    Vector2 srSize = new Vector2(64, 64);
                                    for (int i = 0; i < tree.scale / 2; i += 64)
                                    {
                                        for (int j = 0; j < tree.scale / 2; j += 64)
                                        {
                                            if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i + 64 < screenBL.x || bl.y + j + 64 < screenBL.y))
                                            {
                                                this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 15);
                                                spriteRenderers[spriteRendererIndex].sprite = sprite;
                                                spriteRenderers[spriteRendererIndex].size = srSize;
                                                spriteRenderers[spriteRendererIndex].color = new Color(0.75f, 0.75f, 0.75f);
                                                spriteRendererIndex++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (tree.tile.HasWire())
            {
                if (!hasSubTree)
                {


                    Vector2 srSize = new Vector2(1, 1);
                    for (int i = 0; i < tree.scale; i++)
                    {
                        for (int j = 0; j < tree.scale; j++)
                        {
                            if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i < screenBL.x || bl.y + j < screenBL.y))
                            {
                                Coordinate localCoord = new Coordinate(bl.x + i, bl.y + j);
                                int spriteIndex = World.wires[localCoord].GetOrientation();
                                Sprite[] spriteArray = TileInfo.sprites[TID.WIRE];
                                Sprite sprite = spriteArray[spriteIndex];
                                this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 12);
                                spriteRenderers[spriteRendererIndex].sprite = sprite;
                                spriteRenderers[spriteRendererIndex].size = srSize;
                                spriteRenderers[spriteRendererIndex].color = World.wires[localCoord].GetColor();
                                spriteRendererIndex++;
                            }
                        }
                    }

                }
                else
                {
                    for (int quadIndex = 0; quadIndex < 4; quadIndex++)
                    {
                        if (tree.subTrees[quadIndex] == null)
                        {
                            bl = tree.GetSubTreeCoordinate(quadIndex);
                            tr = bl + (tree.scale / 2);
                            if (!(bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
                            {


                                Vector2 srSize = new Vector2(1, 1);
                                for (int i = 0; i < tree.scale / 2; i++)
                                {
                                    for (int j = 0; j < tree.scale / 2; j++)
                                    {
                                        if (!(bl.x + i > screenTR.x || bl.y + j > screenTR.y || bl.x + i < screenBL.x || bl.y + j < screenBL.y))
                                        {
                                            Coordinate localCoord = new Coordinate(bl.x + i, bl.y + j);
                                            int spriteIndex = World.wires[localCoord].GetOrientation();
                                            Sprite[] spriteArray = TileInfo.sprites[TID.WIRE];
                                            Sprite sprite = spriteArray[spriteIndex];
                                            this.tileRenderers[spriteRendererIndex].transform.position = new Vector3(bl.x + i, bl.y + j, 12);
                                            spriteRenderers[spriteRendererIndex].sprite = sprite;
                                            spriteRenderers[spriteRendererIndex].size = srSize;
                                            spriteRenderers[spriteRendererIndex].color = World.wires[localCoord].GetColor();
                                            spriteRendererIndex++;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
        return spriteRendererIndex;
    }
}
