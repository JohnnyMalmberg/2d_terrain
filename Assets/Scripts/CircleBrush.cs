using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBrush : MonoBehaviour
{
    public float radius;

    void Start()
    {
        transform.localScale = new Vector3(radius, radius, 1);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = -1f;
        transform.position = pos;
        if (Input.GetMouseButton(0))
        {
            World.tree.FillTileCircle(new Vector2(transform.position.x, transform.position.y), radius, false);
            //World.tree.Simplify();
            //Destroy(gameObject);
        }
        if (Input.GetMouseButton(1))
        {
            World.tree.BackTileCircle(new Vector2(transform.position.x, transform.position.y), radius, false);
            //World.tree.Simplify();
        }

        radius += Input.mouseScrollDelta.y;
        if (radius < 0.5f)
        {
            radius = 0.5f;
        }
        transform.localScale = new Vector3(radius, radius, 1);
    }
}
