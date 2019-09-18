using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate : System.IEquatable<Coordinate>
{
    public short x;
    public short y;
    public Coordinate(int x, int y)
    {
        this.x = (short)x;
        this.y = (short)y;
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

    public bool WithinCircle(Vector2 a, float radius)
    {
        float distance = Mathf.Sqrt(Mathf.Pow(this.x - a.x, 2f) + Mathf.Pow(this.y - a.y, 2f));
        return (distance <= radius);
    }

    public static Coordinate Origin()
    {
        return new Coordinate(0, 0);
    }

    public Coordinate Up()
    {
        return new Coordinate(this.x, this.y + 1);
    }

    public Coordinate Down()
    {
        return new Coordinate(this.x, this.y - 1);
    }

    public Coordinate Left()
    {
        return new Coordinate(this.x - 1, this.y);
    }

    public Coordinate Right()
    {
        return new Coordinate(this.x + 1, this.y);
    }

    public bool Equals(Coordinate other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return (this.x == other.x && this.y == other.y);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((Coordinate)obj);
    }

    public override int GetHashCode()
    {
        return (this.x << 16) | (int)this.y;
    }
}

