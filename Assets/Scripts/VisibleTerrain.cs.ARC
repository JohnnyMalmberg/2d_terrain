using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class VisibleTerrain : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;
    List<Vector2> uvs;

    public int drawScale = 1;
    int resolution;
    bool done;

    QuadTree tree;

    void GenerateTerrain()
    {
        Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);

        Debug.Log("--> Filling earth...");

        Tile ground = new Tile(true, false, false, false, 1);
        // Fill bottom half with earth
        this.tree.SetTileBox(worldBottomLeft, new Coordinate(resolution / 2, (int)(resolution * -0.07f)), ground);

        Debug.Log("--> Filling upper earth curve...");
        // Fill in surface curve
        for (int i = worldBottomLeft.x; i <= worldTopRight.x; i++)
        {
            float noise = Mathf.PerlinNoise(5000 + i * 0.0137f, i * 0.01f);
            int height = (int)((noise * resolution) * 0.1f);
            this.tree.SetTileBox(new Coordinate(i, (int)(resolution * -0.07f)), new Coordinate(i, height + (int)(resolution * -0.07f)), ground);
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
        this.tree.Simplify();

        Debug.Log("World generation complete!");

        done = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        resolution = (int)Mathf.Pow(2, 11);
        if (resolution > 8192)
        {
            Debug.LogWarning("Extreme world size may consume excessive computer resources");
        }
        Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);
        this.tree = new QuadTree(worldBottomLeft, resolution, new Tile(false, false, false, false, 0));

        GetComponent<MeshFilter>().mesh = this.mesh = new Mesh();
        this.mesh.name = "Main Terrain Mesh";
        this.vertices = new List<Vector3>();
        this.uvs = new List<Vector2>();
        this.triangles = new List<int>();
        this.colors = new List<Color>();

        done = false;
        Thread worldGenThread = new Thread(new ThreadStart(GenerateTerrain));
        worldGenThread.Start();

        //UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        if (done)
        {
            UpdateMesh();
        }
    }

    void UpdateMesh()
    {
        this.mesh.Clear();
        this.vertices.Clear();
        this.triangles.Clear();
        this.uvs.Clear();

        ConstructMesh(this.vertices, this.triangles, this.tree);

        this.mesh.vertices = vertices.ToArray();
        this.mesh.triangles = triangles.ToArray();
        this.mesh.uv = uvs.ToArray();
        this.mesh.RecalculateNormals();
    }

    void ConstructMesh(List<Vector3> vertices, List<int> triangles, QuadTree tree)
    {
        Coordinate bl = tree.bottomLeft;
        Coordinate tr = tree.topRight;
        Vector3 screenBL = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(-1f, -1f, 0f);
        Vector3 screenTR = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);

        if ((bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
        {
            return;
        }


        bool hasSubTree = false; // TODO: Make this a property of the tree itself
        if (tree.scale > drawScale)
        {
            for (int i = 0; i < 4; i++)
            {
                if (tree.subTrees[i] != null)
                {
                    ConstructMesh(vertices, triangles, tree.subTrees[i]);
                    hasSubTree = true;
                }
            }
        }
        if (tree.tile.IsFilled())
        {
            if (!hasSubTree)
            {
                int firstIndex = vertices.Count;
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, tree.scale / 2));
                uvs.Add(new Vector2(tree.scale / 2, tree.scale / 2));
                uvs.Add(new Vector2(tree.scale / 2, 0));
                vertices.Add(new Vector3(bl.x, bl.y, 0f));
                vertices.Add(new Vector3(bl.x, tr.y, 0f));
                vertices.Add(new Vector3(tr.x, tr.y, 0f));
                vertices.Add(new Vector3(tr.x, bl.y, 0f));
                triangles.Add(firstIndex);
                triangles.Add(firstIndex + 1);
                triangles.Add(firstIndex + 2);

                triangles.Add(firstIndex);
                triangles.Add(firstIndex + 2);
                triangles.Add(firstIndex + 3);
                //for (int i = 0; i < tree.scale; i++)
                //{
                //    for (int j = 0; j < tree.scale; j++)
                //    {
                //        uvs.Add(new Vector2(0, 0));
                //        uvs.Add(new Vector2(0, 0.5f));
                //        uvs.Add(new Vector2(0.5f, 0.5f));
                //        uvs.Add(new Vector2(0.5f, 0));
                //        int firstIndex = vertices.Count;
                //        vertices.Add(new Vector3(bl.x + i, bl.y + j, 0f));
                //        vertices.Add(new Vector3(bl.x + i, bl.y + j + 1, 0f));
                //        vertices.Add(new Vector3(bl.x + i + 1, bl.y + j + 1, 0f));
                //        vertices.Add(new Vector3(bl.x + i + 1, bl.y + j, 0f));
                //
                //        triangles.Add(firstIndex);
                //        triangles.Add(firstIndex + 1);
                //        triangles.Add(firstIndex + 2);
                //
                //        triangles.Add(firstIndex);
                //        triangles.Add(firstIndex + 2);
                //        triangles.Add(firstIndex + 3);
                //    }
                //}
            }
            else
            { // TODO: Refactor away this god-forsaken pyramid
                for (int i = 0; i < 4; i++)
                {
                    if (tree.subTrees[i] == null)
                    {
                        bl = tree.GetSubTreeCoordinate(i);
                        tr = bl + (tree.scale / 2);
                        if (!(bl.x > screenTR.x || bl.y > screenTR.y || tr.x < screenBL.x || tr.y < screenBL.y))
                        {
                            int firstIndex = vertices.Count;

                            uvs.Add(new Vector2(0, 0));
                            uvs.Add(new Vector2(0, tree.scale / 4));
                            uvs.Add(new Vector2(tree.scale / 4, tree.scale / 4));
                            uvs.Add(new Vector2(tree.scale / 4, 0));
                            vertices.Add(new Vector3(bl.x, bl.y, 0f));
                            vertices.Add(new Vector3(bl.x, tr.y, 0f));
                            vertices.Add(new Vector3(tr.x, tr.y, 0f));
                            vertices.Add(new Vector3(tr.x, bl.y, 0f));
                            triangles.Add(firstIndex);
                            triangles.Add(firstIndex + 1);
                            triangles.Add(firstIndex + 2);

                            triangles.Add(firstIndex);
                            triangles.Add(firstIndex + 2);
                            triangles.Add(firstIndex + 3);
                        }
                    }
                }
            }
        }
    }
}
