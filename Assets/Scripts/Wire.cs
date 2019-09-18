using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire
{
    int orientation = 0;
    int signal = 0;
    public int GetOrientation()
    {
        return orientation;
    }

    public Color GetColor()
    {
        if (signal >= 0)
        {
            return new Color((signal) / 64f, 0f, 0f);
        }
        else
        {
            return new Color(0f, 0f, Mathf.Abs(signal) / 64f);
        }
    }

    public void ReOrient(Dictionary<Coordinate, Wire> wires, Coordinate coord)
    {
        orientation = 0;
        orientation = orientation | (wires.ContainsKey(coord.Up()) ? 8 : 0);
        orientation = orientation | (wires.ContainsKey(coord.Right()) ? 4 : 0);
        orientation = orientation | (wires.ContainsKey(coord.Down()) ? 2 : 0);
        orientation = orientation | (wires.ContainsKey(coord.Left()) ? 1 : 0);
    }

    public int GetSignal()
    {
        return signal;
    }

    public void DiminishSignal()
    {
        signal /= 2;
    }

    public void SetSignal(int newSignal, Dictionary<Coordinate, Wire> wires, Dictionary<Coordinate, TileBehavior> tileBehaviors, Coordinate coord)
    {
        if (Mathf.Abs(newSignal) <= Mathf.Abs(signal))
        {
            return;
        }
        if (newSignal > 64)
        {
            newSignal = 64;
        }
        else if (newSignal < -64)
        {
            newSignal = -64;
        }
        signal = newSignal;
        if (tileBehaviors.ContainsKey(coord))
        {
            tileBehaviors[coord].SignalUpdate(signal);
        }
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
            Coordinate up = coord.Up();
            Coordinate down = coord.Down();
            Coordinate left = coord.Left();
            Coordinate right = coord.Right();
            if (wires.ContainsKey(up))
            {
                wires[up].SetSignal(newSignal, wires, tileBehaviors, up);
            }
            if (wires.ContainsKey(right))
            {
                wires[right].SetSignal(newSignal, wires, tileBehaviors, right);
            }
            if (wires.ContainsKey(down))
            {
                wires[down].SetSignal(newSignal, wires, tileBehaviors, down);
            }
            if (wires.ContainsKey(left))
            {
                wires[left].SetSignal(newSignal, wires, tileBehaviors, left);
            }
        }
    }

    public void Orient(Dictionary<Coordinate, Wire> wires, Coordinate coord)
    {
        orientation = 0;
        Coordinate up = coord.Up();
        Coordinate down = coord.Down();
        Coordinate left = coord.Left();
        Coordinate right = coord.Right();
        if (wires.ContainsKey(up))
        {
            orientation = orientation | 8;
            wires[up].ReOrient(wires, up);
        }
        if (wires.ContainsKey(right))
        {
            orientation = orientation | 4;
            wires[right].ReOrient(wires, right);
        }
        if (wires.ContainsKey(down))
        {
            orientation = orientation | 2;
            wires[down].ReOrient(wires, down);
        }
        if (wires.ContainsKey(left))
        {
            orientation = orientation | 1;
            wires[left].ReOrient(wires, left);
        }
    }
}
