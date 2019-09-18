using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileBehavior
{
    public bool active;
    public float updateChance = 0f;
    public virtual void ActiveUpdate()
    {
        active = false;
        // Behaviors that happen frequently, but only for objects that are in their active state
    }

    public virtual void SlowUpdate()
    {
        // Behaviors that happen infrequently, but regularly
    }

    public virtual void ChanceUpdate()
    {
        // Behaviors that will happen at random intervals
    }

    public virtual void OnDestruction()
    {
        // Behaviors that should happen when the Tile is destroyed
    }

    public virtual void Interaction()
    {
        // Behavior triggered by the player attempting to use the tile
    }

    public virtual void SignalUpdate(int signal)
    {
        // Behavior triggered by a wire signal
    }

}
