using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicEntity : MonoBehaviour
{

    protected static float gravity = 0.02f;

    protected Vector3 velocity = new Vector3(0, 0, 0);

    protected bool canJump = true;

    public int ID;

    void Awake()
    {
        ID = World.RegisterDynamicEntity(this);
    }

    public virtual void PreparePhysics()
    {

    }

    public virtual void Tick()
    {

    }

    public void ApplyPhysics()
    {
        int steps = 60;
        Vector3 velocityStep = velocity / steps;

        for (int i = 0; i < steps; i++)
        {

            transform.position = transform.position + velocityStep;

            // Check right side collision
            if (World.Full(new Coordinate((int)Mathf.Floor(transform.position.x + 0.5f), (int)Mathf.Floor(transform.position.y + 0.2f))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x + 0.5f), (int)Mathf.Floor(transform.position.y + 1f))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x + 0.5f), (int)Mathf.Floor(transform.position.y + 1.8f))))
            {
                transform.position = new Vector3(Mathf.Floor(transform.position.x + 0.5f) - 0.5f, transform.position.y, transform.position.z);
                velocity.x = 0f;
                velocityStep.x = 0f;
            }

            // Check left side collision
            if (World.Full(new Coordinate((int)Mathf.Floor(transform.position.x - 0.5f), (int)Mathf.Floor(transform.position.y + 0.2f))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x - 0.5f), (int)Mathf.Floor(transform.position.y + 1f))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x - 0.5f), (int)Mathf.Floor(transform.position.y + 1.8f))))
            {
                transform.position = new Vector3(Mathf.Floor(transform.position.x - 0.5f) + 1.5f, transform.position.y, transform.position.z);
                velocity.x = 0f;
                velocityStep.x = 0f;
            }

            // Check floor collision
            if (World.Full(new Coordinate((int)Mathf.Floor(transform.position.x - 0.4f), (int)Mathf.Floor(transform.position.y))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x + 0.4f), (int)Mathf.Floor(transform.position.y))))
            {
                transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y + 1f), transform.position.z);
                velocity.y = 0f;
                velocityStep.y = 0f;
                canJump = true;
            }

            // Check ceiling collision
            if (World.Full(new Coordinate((int)Mathf.Floor(transform.position.x - 0.4f), (int)Mathf.Floor(transform.position.y + 2f))) ||
                World.Full(new Coordinate((int)Mathf.Floor(transform.position.x + 0.4f), (int)Mathf.Floor(transform.position.y + 2f))))
            {
                transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y), transform.position.z);
                velocity.y = 0f;
                velocityStep.y = 0f;
            }

        }
    }
}
