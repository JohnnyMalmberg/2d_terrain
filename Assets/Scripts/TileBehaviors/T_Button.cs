using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T_Button : TileBehavior
{
    int spriteIndex;

    int holdLength;
    int activeTicks;
    public T_Button(bool positive, int duration = 5, int strength = 32)
    {
        spriteIndex = positive ? 0 : 2;
        holdLength = duration;
        activeTicks = 0;
        emissionValue = positive ? strength : -strength;
    }

    public override void Initialize()
    {
        World.SetSpriteIndex(coordinate, spriteIndex);
    }

    public override void ActiveUpdate()
    {
        if (activeTicks >= holdLength)
        {
            spriteIndex = (spriteIndex & 2);
            World.SetSpriteIndex(coordinate, spriteIndex);
            activeTicks = 0;
            active = false;
            signalEmitter = false;
        }
        else
        {
            activeTicks++;
        }
    }

    public override void Interaction()
    {
        spriteIndex = (spriteIndex & 2) | 1;
        World.SetSpriteIndex(coordinate, spriteIndex);
        active = true;
        signalEmitter = true;
    }
}
