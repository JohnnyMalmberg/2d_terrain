using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : MonoBehaviour
{

    public GameObject beam;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal") * 0.4f;
        float yInput = Input.GetAxis("Vertical") * 0.4f;
        this.transform.Translate(xInput, yInput, 0f);
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Camera.main.orthographicSize -= Input.mouseScrollDelta.y * 0.8f;
            if (Camera.main.orthographicSize > 24)
            {
                Camera.main.orthographicSize = 24;
            }
            else if (Camera.main.orthographicSize < 4)
            {
                Camera.main.orthographicSize = 4;
            }
            if (Input.GetKey(KeyCode.Keypad0))
            {
                Camera.main.orthographicSize = 8;
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



        if (Input.GetKey(KeyCode.F))
        {
            World.tree.FillTile(World.CursorCoordinate());
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            World.InteractAtCursor();
        }


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        if (Input.GetKey(KeyCode.X))
        {
            World.BreakTileAtCursor();
            Vector3 difference = mousePosition - beam.transform.position;
            float distance = difference.magnitude;
            beam.transform.localScale = new Vector3(distance, 1, 1);
            float angle = Vector3.Angle(difference, Vector3.right);
            if (beam.transform.position.y > mousePosition.y)
            {
                angle = -angle;
            }
            beam.transform.eulerAngles = new Vector3(0, 0, angle);
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            beam.transform.localScale = new Vector3(0, 0, 0);
        }


        if (Input.GetKey(KeyCode.G))
        {
            World.PlaceTileAtCursor(TID.BUTTON, new T_Button(true, 8, 64));
        }
        if (Input.GetKey(KeyCode.H))
        {
            World.PlaceTileAtCursor(TID.BUTTON, new T_Button(false, 8, 64));
        }


        if (Input.GetKey(KeyCode.P))
        {
            World.PlaceTileAtCursor(TID.SHIFTER, new T_Shifter());
        }
    }
}
