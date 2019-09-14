using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tile
{
    public bool isFilled;
    public int id;
    public Tile(bool isFilled, int id)
    {
        this.isFilled = isFilled;
        this.id = id;
    }

    public Tile(Tile original)
    {
        this.isFilled = original.isFilled;
        this.id = original.id;
    }
    public bool Mergable(Tile b)
    {
        return (this.isFilled == b.isFilled) && (this.id == b.id);
    }
}

public struct Coordinate
{
    public int x;
    public int y;
    public Coordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Coordinate operator +(Coordinate a, Coordinate b)
    {
        return new Coordinate(a.x + b.x, a.y + b.y);
    }


    public static Coordinate operator -(Coordinate a, Coordinate b)
    {
        return new Coordinate(a.x - b.x, a.y - b.y);
    }

    // Adding an int to a Coordinate means adding that value to both the x and y values
    public static Coordinate operator +(Coordinate a, int b)
    {
        return new Coordinate(a.x + b, a.y + b);
    }

    public static Coordinate operator -(Coordinate a, int b)
    {
        return new Coordinate(a.x - b, a.y - b);
    }

    public bool WithinBox(Coordinate a, Coordinate b)
    {
        return (a.x <= this.x) && (this.x <= b.x) && (a.y <= this.y) && (this.y <= b.y);
    }

    public bool WithinCircle(Coordinate a, float radius)
    {
        float distance = Mathf.Sqrt(Mathf.Pow(this.x - a.x, 2f) + Mathf.Pow(this.y - a.y, 2f));
        return (distance <= radius);
    }

    public static Coordinate Origin()
    {
        return new Coordinate(0, 0);
    }
}

public class QuadTree
{
    public QuadTree[] subTrees;

    public Tile tile;

    public Coordinate bottomLeft;
    public Coordinate center;
    public Coordinate topRight;

    public int scale;

    public QuadTree(Coordinate bottomLeft, int scale, Tile newTile)
    {
        this.subTrees = new QuadTree[4];
        this.bottomLeft = Coordinate.Origin() + bottomLeft;
        this.topRight = bottomLeft + (scale);
        this.center = bottomLeft + (scale / 2);
        this.scale = scale;
        this.tile = new Tile(newTile);
    }

    int GetSubTreeIndex(Coordinate coordinate)
    {
        int subTreeIndex = 0;
        if (coordinate.x >= this.center.x)
        {
            subTreeIndex += 2;
        }
        if (coordinate.y >= this.center.y)
        {
            subTreeIndex += 1;
        }
        return subTreeIndex;
    }

    public Tile GetTile(Coordinate coordinate, int minScale = 1)
    {
        if (this.scale == minScale)
        {
            return this.tile;
        }

        QuadTree subTree = this.subTrees[this.GetSubTreeIndex(coordinate)];

        if (subTree != null)
        {
            return subTree.GetTile(coordinate, minScale);
        }
        else
        {
            return this.tile;
        }
    }

    public Coordinate GetSubTreeCoordinate(int subTreeIndex)
    {
        int x = ((subTreeIndex & 2) == 0) ? this.bottomLeft.x : this.center.x;
        int y = ((subTreeIndex & 1) == 0) ? this.bottomLeft.y : this.center.y;
        return new Coordinate(x, y);
    }

    void MakeSubTree(int subTreeIndex, Tile newTile)
    {
        this.subTrees[subTreeIndex] = new QuadTree(this.GetSubTreeCoordinate(subTreeIndex), this.scale / 2, newTile);
    }

    // Returns false if the coordinate is not within this tree
    public bool SetTile(Coordinate coordinate, Tile newTile)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile = new Tile(newTile);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].SetTile(coordinate, newTile);
    }

    public bool SetTileBox(Coordinate bottomLeft, Coordinate topRight, Tile newTile)
    {
        // Make sure box intersects the tree
        if (bottomLeft.x >= this.topRight.x || this.bottomLeft.x > topRight.x ||
            bottomLeft.y >= this.topRight.y || this.bottomLeft.y > topRight.y)
        {
            return false;
        }

        // Check if box engulfs the tree
        if (this.bottomLeft.WithinBox(bottomLeft, topRight) &&
            (this.topRight - 1).WithinBox(bottomLeft, topRight))
        {
            this.subTrees = new QuadTree[4];
            this.tile = new Tile(newTile);
            return true;
        }

        for (int i = 0; i < 4; i++)
        {
            this.SetTileBox_SubTree(i, bottomLeft, topRight, newTile);
        }

        return true;
    }

    public bool SetTileBox_SubTree(int subTreeIndex, Coordinate bottomLeft, Coordinate topRight, Tile newTile)
    {
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
            if (this.subTrees[subTreeIndex].SetTileBox(bottomLeft, topRight, newTile))
            {
                return true;
            }
            else
            {
                this.subTrees[subTreeIndex] = null;
                return false;
            }
        }
        return this.subTrees[subTreeIndex].SetTileBox(bottomLeft, topRight, newTile);

    }

    public bool Simplify()
    {
        if (this.scale == 1)
        {
            return true;
        }

        bool canSimplify = true;
        for (int i = 0; i < 4; i++)
        {
            if (this.subTrees[i] != null)
            {
                if (!this.subTrees[i].Simplify())
                {
                    canSimplify = false;
                }
            }
        }

        if (!canSimplify)
        {
            return false;
        }

        Tile simplifiedTile = (this.subTrees[0] == null) ? new Tile(this.tile) : new Tile(this.subTrees[0].tile);
        for (int i = 1; i < 4; i++)
        {
            Tile newSimplifiedTile;
            if (this.subTrees[i] != null)
            {
                newSimplifiedTile = new Tile(this.subTrees[i].tile);
            }
            else
            {
                newSimplifiedTile = new Tile(this.tile);
            }
            if (!(simplifiedTile.Mergable(newSimplifiedTile)))
            {
                return false;
            }
        }

        this.tile = new Tile(simplifiedTile);
        //for (int i = 0; i < 4; i++)
        //{
        //    this.subTrees[i] = null;
        //}
        this.subTrees = new QuadTree[4];

        return true;
    }

}
