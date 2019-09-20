using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class T_Debug : TileBehavior
{
    public override void ActiveUpdate()
    {
        Debug.Log("Debug tile activated");
        active = false;
    }

    public override void SlowUpdate()
    {
        // Behaviors that happen infrequently, but regularly
    }

    public override void ChanceUpdate()
    {
        Debug.Log("Chance update");
    }

    public override void OnDestruction()
    {
        Debug.Log("Debug tile is being destroyed");
    }

    public override void Interaction()
    {
        Debug.Log("Player interacted with debug tile");
    }

    public override void SignalUpdate(int signal)
    {
        if (signal == 0)
        {
            return;
        }
        Debug.Log(string.Format("Debug tile received wire signal: {0}", signal));
    }
}
