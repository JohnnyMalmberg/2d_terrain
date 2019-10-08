using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    public Sprite noSprite;

    public GameObject tileRendererPrefab;

    public Camera cam;

    public int previousCamLeft = 0;
    public int previousCamRight = 0;
    public int previousCamTop = 0;
    public int previousCamBottom = 0;

    HashSet<Coordinate> coordinatesToCheck;

    Dictionary<Coordinate, TileRenderer> activeTileRenderers;
    Dictionary<Coordinate, TileRenderer> activeWireRenderers;
    Dictionary<Coordinate, TileRenderer> activeBackRenderers;

    Queue<TileRenderer> freeRenderers;

    public void UpdateAtCoordinate(Coordinate coordinate)
    {
        coordinatesToCheck.Add(coordinate);
    }

    void Awake()
    {
        // Announce existance
        World.worldRenderer = this;

        // Prepare sprites
        TileInfo.BuildSpriteDictionary();
        BackInfo.BuildSpriteDictionary();

        // Acquire camera
        cam = Camera.main;

        // Prepare empty structures of Renderer pooling
        activeTileRenderers = new Dictionary<Coordinate, TileRenderer>();
        activeWireRenderers = new Dictionary<Coordinate, TileRenderer>();
        activeBackRenderers = new Dictionary<Coordinate, TileRenderer>();
        freeRenderers = new Queue<TileRenderer>();
        coordinatesToCheck = new HashSet<Coordinate>();

        // Populate Renderer pool
        CreateRenderers(500);
    }

    void CreateRenderers(int additionalRendererCount)
    {
        for (int i = 0; i < additionalRendererCount; i++)
        {
            GameObject go = Instantiate(tileRendererPrefab);
            go.transform.SetParent(transform);
            TileRenderer tr = go.GetComponent<TileRenderer>();
            tr.SetVisible(false);
            freeRenderers.Enqueue(tr);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenBL = cam.ScreenToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(-1f, -1f, 0f);
        Vector3 screenTR = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);
        int camRight = (int)screenTR.x;
        int camLeft = (int)screenBL.x;
        int camTop = (int)screenTR.y;
        int camBottom = (int)screenBL.y;


        // Possible refactoring: Move these loops to their own function
        // Prepare to check all coordinates that just entered the screen
        for (int i = camLeft; i <= camRight; i++)
        {
            for (int j = camBottom; j <= camTop; j++)
            {
                if ((i > previousCamRight || i < previousCamLeft) || (j > previousCamTop || j < previousCamBottom))
                {
                    coordinatesToCheck.Add(new Coordinate(i, j));
                }

            }
        }

        // Prepare to check all coordinates that just exited the screen
        for (int i = previousCamLeft; i <= previousCamRight; i++)
        {
            for (int j = previousCamBottom; j <= previousCamTop; j++)
            {
                if ((i > camRight || i < camLeft) || (j > camTop || j < camBottom))
                {
                    coordinatesToCheck.Add(new Coordinate(i, j));
                }

            }
        }

        foreach (Coordinate coordinate in coordinatesToCheck)
        {
            if (coordinate.x < camLeft || coordinate.x > camRight || coordinate.y < camBottom || coordinate.y > camTop)
            {
                // Coordinate off screen
                DeactivateRenderer(coordinate, WorldLayer.TILE);
                DeactivateRenderer(coordinate, WorldLayer.BACK);
                DeactivateRenderer(coordinate, WorldLayer.WIRE);
            }
            else
            {
                Tile tile = World.GetTile(coordinate);
                TileRenderer tr;
                tr = GetRenderer(coordinate, WorldLayer.TILE);
                if (tile.IsFilled())
                {
                    tr.SetSprite(TileInfo.sprites[tile.ID()][tile.SpriteIndex()]);
                    tr.SetCoordinate(coordinate);
                    tr.SetLayer(WorldLayer.TILE);
                    tr.SetVisible();
                }
                else
                {
                    DeactivateRenderer(coordinate, WorldLayer.TILE);
                }

                tr = GetRenderer(coordinate, WorldLayer.WIRE);
                if (tile.HasWire())
                {
                    int orientation = World.wires[coordinate].GetOrientation();
                    tr.SetSprite(TileInfo.sprites[TID.WIRE][orientation]);
                    tr.SetCoordinate(coordinate);
                    tr.SetLayer(WorldLayer.WIRE);
                    tr.SetVisible();
                }
                else
                {
                    DeactivateRenderer(coordinate, WorldLayer.WIRE);
                }

                tr = GetRenderer(coordinate, WorldLayer.BACK);
                if (tile.IsBacked())
                {
                    tr.SetSprite(BackInfo.sprites[tile.BID()]);
                    tr.SetCoordinate(coordinate);
                    tr.SetLayer(WorldLayer.BACK);
                    tr.SetVisible();
                }
                else
                {
                    DeactivateRenderer(coordinate, WorldLayer.BACK);
                }

            }
        }

        coordinatesToCheck.Clear();

        previousCamLeft = camLeft;
        previousCamRight = camRight;
        previousCamTop = camTop;
        previousCamBottom = camBottom;
    }

    TileRenderer GetRenderer(Coordinate coordinate, WorldLayer layer)
    {
        Dictionary<Coordinate, TileRenderer> renderDict = GetDictionary(layer);
        if (renderDict.ContainsKey(coordinate))
        {
            return renderDict[coordinate];
        }
        else
        {
            TileRenderer tr = GetFreeRenderer();
            renderDict.Add(coordinate, tr);
            return tr;
        }
    }

    TileRenderer GetFreeRenderer()
    {
        if (freeRenderers.Count == 0)
        {
            CreateRenderers(100);
        }
        return freeRenderers.Dequeue();
    }

    void DeactivateRenderer(Coordinate coordinate, WorldLayer layer)
    {
        Dictionary<Coordinate, TileRenderer> renderDict = GetDictionary(layer);

        if (renderDict.ContainsKey(coordinate))
        {
            TileRenderer tr = renderDict[coordinate];
            renderDict.Remove(coordinate);
            tr.SetVisible(false);
            freeRenderers.Enqueue(tr);
        }
    }

    Dictionary<Coordinate, TileRenderer> GetDictionary(WorldLayer layer)
    {
        switch (layer)
        {
            case WorldLayer.OVERLAY:
                return null;
            case WorldLayer.TILE:
                return activeTileRenderers;
            case WorldLayer.WIRE:
                return activeWireRenderers;
            case WorldLayer.BACK:
                return activeBackRenderers;
            default:
                return null;
        }
    }
}
