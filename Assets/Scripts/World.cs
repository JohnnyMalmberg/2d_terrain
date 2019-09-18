using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class World
{
    public static QuadTree tree;
    public static Dictionary<Coordinate, TileBehavior> tileBehaviors;
    public static Dictionary<Coordinate, int> overlays;
    public static Dictionary<Coordinate, Wire> wires;

    static int resolution;

    public static bool treeExists = false;

    static int tick;

    public static void SetResolution(int _resolution)
    {
        resolution = _resolution;
    }

    public static void Generate()
    {
        Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);
        tree = new QuadTree(worldBottomLeft, resolution, new Tile(TID.NULL));



        Debug.Log("--> Filling earth...");

        Tile stone = new Tile(TID.STONE, true, true);
        Tile dirt = new Tile(TID.DIRT, true, true);
        // Fill bottom half with earth
        tree.SetTileBox(worldBottomLeft, new Coordinate(resolution / 2, 0), stone);

        Debug.Log("--> Filling upper earth curve...");
        // Fill in surface curve
        for (int i = worldBottomLeft.x; i <= worldTopRight.x; i++)
        {
            float noise = Mathf.PerlinNoise(5000 + i * 0.0237f, i * 0.01f);
            int height = (int)((noise * 300) * 0.1f);
            tree.SetTileBox(new Coordinate(i, 0), new Coordinate(i, height - 5), stone);
            tree.SetTileBox(new Coordinate(i, height - 4), new Coordinate(i, height), dirt);
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
    }

    public static void Tick()
    {
        if (tick % 10 == 0)
        {
            tree.Simplify();
        }
        if ((tick + 1) % 5 == 0)
        {
            foreach (KeyValuePair<Coordinate, Wire> entry in wires)
            {
                entry.Value.DiminishSignal();
            }
            //foreach (KeyValuePair<Coordinate, TileBehavior> entry in tileBehaviors)
            //{
            //
            //}
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
        newWire.Orient(wires, coordinate);
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
            wires[cursor].SetSignal(-64, wires, tileBehaviors, cursor);
        }
    }

    public static void Annihilate(Coordinate coordinate)
    {
        tree.SetTile(coordinate, new Tile(TID.NULL));
        tileBehaviors.Remove(coordinate);
        wires.Remove(coordinate);
        overlays.Remove(coordinate);
    }

    public static void AnnihilateAtCursor()
    {
        Annihilate(CursorCoordinate());
    }
}
