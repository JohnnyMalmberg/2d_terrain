using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        this.transform.Translate(xInput, yInput, 0f);
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Camera.main.orthographicSize -= Input.mouseScrollDelta.y * 0.8f;
            if (Camera.main.orthographicSize > 40)
            {
                Camera.main.orthographicSize = 40;
            }
            else if (Camera.main.orthographicSize < 8)
            {
                Camera.main.orthographicSize = 8;
            }
            if (Input.GetKey(KeyCode.Keypad0))
            {
                Camera.main.orthographicSize = 24;
            }
        }


        if (Input.GetKey(KeyCode.I))
        {
            World.PlaceWireAtCursor();
        }

        if (Input.GetKey(KeyCode.Delete))
        {
            World.AnnihilateAtCursor();
        }

        if (Input.GetKey(KeyCode.X))
        {
            World.tree.FillTile(World.CursorCoordinate(), false);
        }

        if (Input.GetKey(KeyCode.F))
        {
            World.tree.FillTile(World.CursorCoordinate());
        }


        if (Input.GetKey(KeyCode.E))
        {
            World.InteractAtCursor();
        }


    }
}
