using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T_Shifter : TileBehavior
{
    static List<Coordinate> blacklist = new List<Coordinate>();
    static int blacklistTick = 0;
    public override void SignalUpdate(int signal)
    {
        if (signal == 0)
        {
            return;
        }
        if (blacklistTick != World.GetTick())
        {
            blacklistTick = World.GetTick();
            blacklist.Clear();
        }
        if (blacklist.Contains(coordinate.Up()))
        {
            return;
        }

        if (signal > 0)
        {
            //if (World.Full(coordinate.Up()))
            //{
            //    if (!World.Full(coordinate.Up().Right()))
            //    {
            //        blacklist.Add(coordinate.Up().Right());
            //        Tile target = World.GetTile(coordinate.Up());
            //        TileBehavior behavior = null;
            //        if (target.HasBehavior())
            //        {
            //            behavior = World.tileBehaviors[coordinate.Up()];
            //        }
            //        World.BreakTile(coordinate.Up());
            //        World.PlaceTile(coordinate.Up().Right(), target.ID(), behavior, target.SpriteIndex());
            //    }
            //}
            World.QueueTileMovement(coordinate.Up(), coordinate.Up().Right());
        }
        else
        {
            //if (World.Full(coordinate.Up()))
            // {
            //    if (!World.Full(coordinate.Up().Left()))
            //    {
            //        blacklist.Add(coordinate.Up().Left());
            //        Tile target = World.GetTile(coordinate.Up());
            //        TileBehavior behavior = null;
            //        if (target.HasBehavior())
            //        {
            //            behavior = World.tileBehaviors[coordinate.Up()];
            //        }
            //        World.BreakTile(coordinate.Up());
            //        World.PlaceTile(coordinate.Up().Left(), target.ID(), behavior, target.SpriteIndex());
            //    }
            // }
            World.QueueTileMovement(coordinate.Up(), coordinate.Up().Left());
        }
    }
}
