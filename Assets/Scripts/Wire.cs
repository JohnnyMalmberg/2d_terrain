using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire
{
    int orientation = 0;
    int signal = 0;
    Coordinate coordinate;

    public void SetCoordinate(Coordinate newCoordinate)
    {
        coordinate = newCoordinate;
    }
    public int GetOrientation()
    {
        return orientation;
    }

    public Color GetColor()
    {
        if (signal > 0)
        {
            return new Color(0.5f + (signal) / 64f, 0.25f + (signal) / 128f, 0f);
        }
        else if (signal < 0)
        {
            return new Color(0f, 0f, 0.5f + Mathf.Abs(signal) / 64f);
        }
        else
        {
            return Color.black;
        }
    }

    public void ReOrient()
    {
        orientation = 0;
        orientation = orientation | (World.wires.ContainsKey(coordinate.Up()) ? 8 : 0);
        orientation = orientation | (World.wires.ContainsKey(coordinate.Right()) ? 4 : 0);
        orientation = orientation | (World.wires.ContainsKey(coordinate.Down()) ? 2 : 0);
        orientation = orientation | (World.wires.ContainsKey(coordinate.Left()) ? 1 : 0);
    }

    public int GetSignal()
    {
        return signal;
    }

    public void EndSignal()
    {
        signal = 0;
    }

    public void SetSignal(int newSignal)
    {
        if (Mathf.Abs(newSignal) <= Mathf.Abs(signal))
        {
            return;
        }
        if (newSignal > 32)
        {
            newSignal = 32;
        }
        else if (newSignal < -32)
        {
            newSignal = -32;
        }
        signal = newSignal;

        if (newSignal > 0)
        {
            newSignal--;
        }
        if (newSignal < 0)
        {
            newSignal++;
        }
        if (newSignal != 0)
        {
            Coordinate up = coordinate.Up();
            Coordinate down = coordinate.Down();
            Coordinate left = coordinate.Left();
            Coordinate right = coordinate.Right();
            if (World.wires.ContainsKey(up))
            {
                World.wires[up].SetSignal(newSignal);
            }
            if (World.wires.ContainsKey(right))
            {
                World.wires[right].SetSignal(newSignal);
            }
            if (World.wires.ContainsKey(down))
            {
                World.wires[down].SetSignal(newSignal);
            }
            if (World.wires.ContainsKey(left))
            {
                World.wires[left].SetSignal(newSignal);
            }
        }
    }

    public void Orient()
    {
        orientation = 0;
        Coordinate up = coordinate.Up();
        Coordinate down = coordinate.Down();
        Coordinate left = coordinate.Left();
        Coordinate right = coordinate.Right();
        if (World.wires.ContainsKey(up))
        {
            orientation = orientation | 8;
            World.wires[up].ReOrient();
        }
        if (World.wires.ContainsKey(right))
        {
            orientation = orientation | 4;
            World.wires[right].ReOrient();
        }
        if (World.wires.ContainsKey(down))
        {
            orientation = orientation | 2;
            World.wires[down].ReOrient();
        }
        if (World.wires.ContainsKey(left))
        {
            orientation = orientation | 1;
            World.wires[left].ReOrient();
        }
    }
}
