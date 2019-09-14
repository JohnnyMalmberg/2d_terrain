using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleTerrain : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;

    public int drawScale = 1;

    QuadTree tree;

    // Start is called before the first frame update
    void Start()
    {
        int resolution = (int)Mathf.Pow(2, 12);
        Coordinate worldBottomLeft = Coordinate.Origin() - (resolution / 2);
        Coordinate worldTopRight = Coordinate.Origin() + ((resolution / 2) - 1);
        this.tree = new QuadTree(worldBottomLeft, resolution, new Tile(false, 0));

        GetComponent<MeshFilter>().mesh = this.mesh = new Mesh();
        this.mesh.name = "Main Terrain Mesh";
        this.vertices = new List<Vector3>();
        this.triangles = new List<int>();
        this.colors = new List<Color>();


        // Primitive World Generation
        // TODO: This is just a quick and dirty world generation, for the purposes of testing other features. Refactor and improve later
        // Fill bottom half with earth
        this.tree.SetTileBox(worldBottomLeft, new Coordinate(resolution / 2, (int)(resolution * -0.07f)), new Tile(true, 0));

        // Fill in surface curve
        for (int i = worldBottomLeft.x; i <= worldTopRight.x; i++)
        {
            float noise = Mathf.PerlinNoise(5000 + i * 0.0137f, i * 0.01f);
            int height = (int)((noise * resolution) * 0.1f);
            this.tree.SetTileBox(new Coordinate(i, (int)(resolution * -0.07f)), new Coordinate(i, height + (int)(resolution * -0.07f)), new Tile(true, 0));
            if (i % 256 == 0)
            {
                this.tree.Simplify();
            }
        }

        // First simplification
        this.tree.Simplify();

        // Simple caves
        for (int i = worldBottomLeft.x; i < worldTopRight.x; i++)
        {
            for (int j = worldBottomLeft.y; j < worldTopRight.y; j++)
            {
                float noise = Mathf.PerlinNoise(1000 + i * 0.01f, 800 + j * 0.01f);
                if (noise > 0.7f)
                {
                    this.tree.SetTile(new Coordinate(i, j), new Tile(false, 0));
                }
                if (i % 256 == 0 && j % 256 == 0)
                {
                    this.tree.Simplify();
                }
            }
        }

        // Second simplification
        this.tree.Simplify();



        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
    }

    void UpdateMesh()
    {
        this.mesh.Clear();
        this.vertices.Clear();
        this.triangles.Clear();

        ConstructMesh(this.vertices, this.triangles, this.tree);

        this.mesh.vertices = vertices.ToArray();
        this.mesh.triangles = triangles.ToArray();
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
        if (tree.tile.isFilled)
        {
            if (!hasSubTree)
            {
                int firstIndex = vertices.Count;
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
