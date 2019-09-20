using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class QuadTree
{
    public QuadTree[] subTrees;

    public Tile tile;

    public Coordinate bottomLeft;
    public Coordinate center;
    public Coordinate topRight;

    // OPTIMIZATION NOTE: If memory usage becomes an issue, center and topRight can be calculated as-needed

    public ushort scale;

    public QuadTree(Coordinate bottomLeft, int scale, Tile newTile)
    {
        this.subTrees = new QuadTree[4];
        this.bottomLeft = Coordinate.Origin() + bottomLeft;
        this.topRight = bottomLeft + (scale);
        this.center = bottomLeft + (scale / 2);
        this.scale = (ushort)scale;
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

    public bool FillTile(Coordinate coordinate, bool fill = true)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.Fill(fill);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].FillTile(coordinate, fill);
    }

    // Without changing any other data at the location, insert a new tile
    public bool InsertTile(Coordinate coordinate, TID id, bool hasBehavior, int spriteIndex)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.Fill();
            this.tile.SetBehavior(hasBehavior);
            this.tile.SetID(id);
            this.tile.SetSprite(spriteIndex);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].InsertTile(coordinate, id, hasBehavior, spriteIndex);
    }

    // Removes ONLY the tile, not the backing, wire, etc.
    // This does NOT handle behaviors: they must be handled externally by the caller
    public bool RemoveTile(Coordinate coordinate)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.Fill(false);
            this.tile.SetBehavior(false);
            //this.tile.SetID(TID.NULL); // TODO: Backing IDs
            this.tile.SetSprite(0);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].RemoveTile(coordinate);
    }

    public bool BackTile(Coordinate coordinate, bool back = true)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.Back(back);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].BackTile(coordinate, back);
    }

    public bool WireTile(Coordinate coordinate, bool wire = true)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.Wire(wire);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].WireTile(coordinate, wire);
    }

    public bool SetSpriteIndex(Coordinate coordinate, int spriteIndex)
    {
        if (!(coordinate.WithinBox(this.bottomLeft, this.topRight - 1)))
        {
            return false;
        }

        if (this.scale == 1)
        {
            this.tile.SetSprite(spriteIndex);
            return true;
        }

        int subTreeIndex = this.GetSubTreeIndex(coordinate);
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
        }
        return this.subTrees[subTreeIndex].SetSpriteIndex(coordinate, spriteIndex);
    }

    public bool SetTileCircle(Vector2 center, float radius, Tile newTile)
    {
        Vector2 bl = center - new Vector2(radius, radius);
        Vector2 tr = center + new Vector2(radius, radius);
        if (bl.x >= this.topRight.x || this.bottomLeft.x > tr.x ||
            bl.y >= this.topRight.y || this.bottomLeft.y > tr.y)
        {
            return false;
        }

        if (this.bottomLeft.WithinCircle(center, radius) &&
            (this.topRight).WithinCircle(center, radius) &&
            (new Coordinate(this.bottomLeft.x, this.topRight.y)).WithinCircle(center, radius) &&
            (new Coordinate(this.topRight.x, this.bottomLeft.y)).WithinCircle(center, radius))
        {
            this.subTrees = new QuadTree[4];
            this.tile = new Tile(newTile);
            return true;
        }

        if (scale == 1)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            this.SetTileCircle_SubTree(i, center, radius, newTile);
        }

        return true;
    }

    bool SetTileCircle_SubTree(int subTreeIndex, Vector2 center, float radius, Tile newTile)
    {
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
            if (this.subTrees[subTreeIndex].SetTileCircle(center, radius, newTile))
            {
                return true;
            }
            else
            {
                this.subTrees[subTreeIndex] = null;
                return false;
            }
        }
        return this.subTrees[subTreeIndex].SetTileCircle(center, radius, newTile);

    }

    public void Fill(bool fill = true)
    {
        if (this.scale <= 1)
        {
            this.tile.Fill(fill);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (this.subTrees[i] == null)
            {
                this.MakeSubTree(i, this.tile);
                this.subTrees[i].tile.Fill(fill);
            }
            else
            {
                this.subTrees[i].Fill(fill);
            }
        }
    }

    public void Back(bool back = true)
    {
        if (this.scale <= 1)
        {
            this.tile.Back(back);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (this.subTrees[i] == null)
            {
                this.MakeSubTree(i, this.tile);
                this.subTrees[i].tile.Back(back);
            }
            else
            {
                this.subTrees[i].Back(back);
            }
        }
    }

    public void Wire(bool wire = true)
    {
        if (this.scale <= 1)
        {
            this.tile.Wire(wire);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (this.subTrees[i] == null)
            {
                this.MakeSubTree(i, this.tile);
                this.subTrees[i].tile.Wire(wire);
            }
            else
            {
                this.subTrees[i].Wire(wire);
            }
        }
    }

    public bool FillTileCircle(Vector2 center, float radius, bool fill = true)
    {
        Vector2 bl = center - new Vector2(radius, radius);
        Vector2 tr = center + new Vector2(radius, radius);
        if (bl.x >= this.topRight.x || this.bottomLeft.x > tr.x ||
            bl.y >= this.topRight.y || this.bottomLeft.y > tr.y)
        {
            return false;
        }

        if (this.bottomLeft.WithinCircle(center, radius) &&
            (this.topRight).WithinCircle(center, radius) &&
            (new Coordinate(this.bottomLeft.x, this.topRight.y)).WithinCircle(center, radius) &&
            (new Coordinate(this.topRight.x, this.bottomLeft.y)).WithinCircle(center, radius))
        {
            this.Fill(fill);
            return true;
        }

        if (scale == 1)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            this.FillTileCircle_SubTree(i, center, radius, fill);
        }

        return true;
    }

    bool FillTileCircle_SubTree(int subTreeIndex, Vector2 center, float radius, bool fill = true)
    {
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
            if (this.subTrees[subTreeIndex].FillTileCircle(center, radius, fill))
            {
                return true;
            }
            else
            {
                this.subTrees[subTreeIndex] = null;
                return false;
            }
        }
        return this.subTrees[subTreeIndex].FillTileCircle(center, radius, fill);

    }

    public bool BackTileCircle(Vector2 center, float radius, bool back = true)
    {
        Vector2 bl = center - new Vector2(radius, radius);
        Vector2 tr = center + new Vector2(radius, radius);
        if (bl.x >= this.topRight.x || this.bottomLeft.x > tr.x ||
            bl.y >= this.topRight.y || this.bottomLeft.y > tr.y)
        {
            return false;
        }

        if (this.bottomLeft.WithinCircle(center, radius) &&
            (this.topRight).WithinCircle(center, radius) &&
            (new Coordinate(this.bottomLeft.x, this.topRight.y)).WithinCircle(center, radius) &&
            (new Coordinate(this.topRight.x, this.bottomLeft.y)).WithinCircle(center, radius))
        {
            this.Back(back);
            return true;
        }

        if (scale == 1)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            this.BackTileCircle_SubTree(i, center, radius, back);
        }

        return true;
    }

    bool BackTileCircle_SubTree(int subTreeIndex, Vector2 center, float radius, bool back = true)
    {
        if (this.subTrees[subTreeIndex] == null)
        {
            this.MakeSubTree(subTreeIndex, this.tile);
            if (this.subTrees[subTreeIndex].BackTileCircle(center, radius, back))
            {
                return true;
            }
            else
            {
                this.subTrees[subTreeIndex] = null;
                return false;
            }
        }
        return this.subTrees[subTreeIndex].BackTileCircle(center, radius, back);

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

    bool SetTileBox_SubTree(int subTreeIndex, Coordinate bottomLeft, Coordinate topRight, Tile newTile)
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

        Tile simplifiedTile = (this.subTrees[0] == null) ? this.tile : this.subTrees[0].tile;
        for (int i = 1; i < 4; i++)
        {
            Tile newSimplifiedTile;
            if (this.subTrees[i] != null)
            {
                newSimplifiedTile = this.subTrees[i].tile;
            }
            else
            {
                newSimplifiedTile = this.tile;
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
