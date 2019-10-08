using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class World
{
    public static QuadTree tree;
    public static Dictionary<Coordinate, TileBehavior> tileBehaviors;
    public static Dictionary<Coordinate, int> overlays;
    public static Dictionary<Coordinate, Wire> wires;

    public static WorldRenderer worldRenderer;

    public static Dictionary<int, DynamicEntity> dynamicEntities = new Dictionary<int, DynamicEntity>();

    static int resolution;

    public static bool treeExists = false;

    static int tick;

    static bool paused = true;

    public static int[][] aestheticRandoms;

    public static int RegisterDynamicEntity(DynamicEntity entity)
    {
        for (int i = 0; i <= dynamicEntities.Count; i++)
        {
            if (!dynamicEntities.ContainsKey(i))
            {
                dynamicEntities.Add(i, entity);
                return i;
            }
        }
        Debug.LogError("DynamicEntity Registration Failed");
        return -1;
    }

    public static void SetResolution(int _resolution)
    {
        resolution = _resolution;

        aestheticRandoms = new int[256][];
        for (int i = 0; i < 256; i++)
        {
            aestheticRandoms[i] = new int[256];
            for (int j = 0; j < 256; j++)
            {
                aestheticRandoms[i][j] = Random.Range(0, 8192);
            }
        }
    }

    public static void Generate()
    {
        Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);
        tree = new QuadTree(worldBottomLeft, resolution, new Tile(false, false, TID.NULL, BID.NULL));



        Debug.Log("--> Filling earth...");

        Tile stone = new Tile(true, true, TID.STONE, BID.STONE);
        Tile dirt = new Tile(true, true, TID.DIRT, BID.DIRT);
        Tile garbage = new Tile(true, false, TID.GARBAGE_MIXED, BID.NULL);
        // Fill bottom half with earth
        tree.SetTileBox(worldBottomLeft, new Coordinate(resolution / 2, 0), stone);

        Debug.Log("--> Filling upper earth curve...");
        // Fill in surface curve
        for (int i = worldBottomLeft.x; i <= worldTopRight.x; i++)
        {
            float noise = Mathf.PerlinNoise(5000 + i * 0.0237f, i * 0.01f);
            int height = (int)((noise * 300) * 0.1f);
            tree.SetTileBox(new Coordinate(i, 0), new Coordinate(i, height - 60), stone);
            tree.SetTileBox(new Coordinate(i, height - 59), new Coordinate(i, height - 20), dirt);
            tree.SetTileBox(new Coordinate(i, height - 19), new Coordinate(i, height), garbage);
            if (i % 256 == 0)
            {
                //Debug.Log("        Simplifying...");
                //this.tree.Simplify();
                //Debug.Log("        Filling...");
                Debug.Log("        256 Columns filled.");
            }
        }

        //Debug.Log("--> First simplification...");
        // First simplification
        //this.tree.Simplify();


        //Debug.Log("--> Begin carving caves...");
        //Tile air = new Tile(false, false, false, false, 0);
        //// Simple caves
        //for (int i = worldBottomLeft.x; i <= worldTopRight.x; i++)
        //{
        //    for (int j = worldBottomLeft.y; j <= worldTopRight.y; j++)
        //    {
        //        float noise = Mathf.PerlinNoise(1000 + i * 0.01f, 800 + j * 0.01f);
        //        if (noise > 0.7f)
        //        {
        //            this.tree.SetTile(new Coordinate(i, j), air);
        //        }
        //        if (i % 256 == 0 && j % 256 == 0)
        //        {
        //            //Debug.Log("        Simplifying...");
        //            //this.tree.Simplify();
        //            //Debug.Log("        Carving...");
        //            Debug.Log("        256^2 Tiles carved");
        //        }
        //    }
        //}


        Debug.Log("--> Simplifying quadtree...");
        // Second simplification
        tree.Simplify();

        Debug.Log("World generation complete!");

        tileBehaviors = new Dictionary<Coordinate, TileBehavior>();
        wires = new Dictionary<Coordinate, Wire>();
        overlays = new Dictionary<Coordinate, int>();



        treeExists = true;
        Unpause();
    }

    public static void Pause()
    {
        paused = true;
    }

    public static void Unpause()
    {
        paused = false;
    }

    public static bool Paused()
    {
        return paused;
    }

    public static void Tick()
    {
        if (tick % 100 == 0)
        {
            tree.Simplify();
        }
        if (tick % 5 == 0)
        {
            foreach (KeyValuePair<Coordinate, TileBehavior> entry in tileBehaviors)
            {
                if (entry.Value.active)
                {
                    entry.Value.ActiveUpdate();
                }
            }
        }
        if ((tick) % 2 == 0)
        {
            foreach (KeyValuePair<Coordinate, Wire> entry in wires)
            {
                if (tileBehaviors.ContainsKey(entry.Key))
                {
                    tileBehaviors[entry.Key].SignalUpdate(entry.Value.GetSignal());
                }
                entry.Value.EndSignal();
            }
            ExecuteTileMovements();
            foreach (KeyValuePair<Coordinate, TileBehavior> entry in tileBehaviors)
            {
                if (entry.Value.signalEmitter)
                {
                    if (wires.ContainsKey(entry.Key))
                    {
                        wires[entry.Key].SetSignal(entry.Value.emissionValue);
                    }
                }
            }
        }

        tick++;
    }

    public static Coordinate CursorCoordinate()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return new Coordinate((int)Mathf.Floor(pos.x), (int)Mathf.Floor(pos.y));
    }

    public static void PlaceWire(Coordinate coordinate)
    {
        tree.WireTile(coordinate);
        Wire newWire = new Wire();
        wires[coordinate] = newWire;
        newWire.SetCoordinate(coordinate);
        newWire.Orient();
        worldRenderer.UpdateAtCoordinate(coordinate);
        worldRenderer.UpdateAtCoordinate(coordinate.Up());
        worldRenderer.UpdateAtCoordinate(coordinate.Down());
        worldRenderer.UpdateAtCoordinate(coordinate.Left());
        worldRenderer.UpdateAtCoordinate(coordinate.Right());
    }

    public static void SetSpriteIndex(Coordinate coordinate, int spriteIndex)
    {
        tree.SetSpriteIndex(coordinate, spriteIndex);
        worldRenderer.UpdateAtCoordinate(coordinate);
    }

    public static void PlaceTile(Coordinate coordinate, TID id, TileBehavior behavior, int spriteIndex)
    {
        Tile oldTile = tree.GetTile(coordinate);
        if (oldTile.IsFilled())
        {
            return;
        }
        tree.InsertTile(coordinate, id, behavior != null, spriteIndex);
        if (behavior != null)
        {
            behavior.coordinate = coordinate;
            behavior.Initialize();
            tileBehaviors[coordinate] = behavior;
        }
        worldRenderer.UpdateAtCoordinate(coordinate);
    }

    public static void BreakTile(Coordinate coordinate)
    {
        if (tileBehaviors.ContainsKey(coordinate))
        {
            tileBehaviors[coordinate].OnDestruction();
            tileBehaviors.Remove(coordinate);
        }
        tree.RemoveTile(coordinate);
        worldRenderer.UpdateAtCoordinate(coordinate);
    }

    public static void BreakTileAtCursor()
    {
        BreakTile(CursorCoordinate());
    }

    public static void PlaceTileAtCursor(TID id, TileBehavior behavior, int spriteIndex = 0)
    {
        PlaceTile(CursorCoordinate(), id, behavior, spriteIndex);
    }

    public static void PlaceWireAtCursor()
    {
        PlaceWire(CursorCoordinate());
    }

    public static void InteractAtCursor()
    {
        Coordinate cursor = CursorCoordinate();
        if (tileBehaviors.ContainsKey(cursor))
        {
            tileBehaviors[cursor].Interaction();
        }
        if (wires.ContainsKey(cursor))
        {
            //wires[cursor].SetSignal(-64, wires, tileBehaviors, cursor);
        }
    }

    public static void Annihilate(Coordinate coordinate)
    {
        tree.SetTile(coordinate, new Tile(false, false, TID.NULL, BID.NULL));
        tileBehaviors.Remove(coordinate);
        wires.Remove(coordinate);
        overlays.Remove(coordinate);
        worldRenderer.UpdateAtCoordinate(coordinate);
    }

    public static void AnnihilateAtCursor()
    {
        Annihilate(CursorCoordinate());
    }

    public static bool Full(Coordinate coordinate)
    {
        return tree.GetTile(coordinate).IsFilled();
    }

    public static Tile GetTile(Coordinate coordinate)
    {
        return tree.GetTile(coordinate);
    }

    public static int GetTick()
    {
        return tick;
    }


    public static List<Coordinate> tilesToMove = new List<Coordinate>();
    public static List<Coordinate> destinations = new List<Coordinate>();
    public static List<int> failedMovements = new List<int>();
    public static void QueueTileMovement(Coordinate coordinate, Coordinate destination)
    {
        if (tree.GetTile(coordinate).IsFilled() && !tree.GetTile(destination).IsFilled())
        {
            int indexA = tilesToMove.IndexOf(coordinate);
            int indexB = destinations.IndexOf(destination);
            if (indexA == -1 && indexB == -1)
            {
                tilesToMove.Add(coordinate);
                destinations.Add(destination);
            }
            else
            {
                if (indexA != -1)
                {
                    failedMovements.Add(indexA);
                }
                if (indexB != -1)
                {
                    failedMovements.Add(indexB);
                }
            }
        }
    }

    public static void ExecuteTileMovements()
    {
        //List<Tile> tiles = new List<Tile>();
        //failedMovements.Sort();
        failedMovements = failedMovements.OrderByDescending(v => v).Distinct().ToList();
        foreach (int failedMovement in failedMovements)
        {
            tilesToMove.RemoveAt(failedMovement);
            destinations.RemoveAt(failedMovement);
        }
        for (int i = 0; i < tilesToMove.Count; i++)
        {
            Tile tile = new Tile(tree.GetTile(tilesToMove[i]));
            tree.RemoveTile(tilesToMove[i]);
            if (tile.HasBehavior())
            {
                TileBehavior behavior = tileBehaviors[tilesToMove[i]];
                behavior.coordinate = destinations[i];
                tileBehaviors.Remove(tilesToMove[i]);
                tileBehaviors.Add(destinations[i], behavior);
            }
            tree.InsertTile(destinations[i], tile.ID(), tile.HasBehavior(), tile.SpriteIndex());
            worldRenderer.UpdateAtCoordinate(tilesToMove[i]);
            worldRenderer.UpdateAtCoordinate(destinations[i]);
        }

        tilesToMove.Clear();
        destinations.Clear();
        failedMovements.Clear();
    }
}
