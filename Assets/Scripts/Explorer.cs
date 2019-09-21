using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : MonoBehaviour
{

    public GameObject beam;
    public GameObject head;

    public HashSet<Coordinate> destructionSet;
    public List<GameObject> destructionMarkers;
    public Queue<Coordinate> destructionQueue;

    Coordinate selector1;
    Coordinate selector2;

    public Sprite markerSprite;


    // Start is called before the first frame update
    void Start()
    {
        destructionSet = new HashSet<Coordinate>();
        destructionQueue = new Queue<Coordinate>();
        destructionMarkers = new List<GameObject>();
        selector1 = selector2 = null;
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal") * 0.4f;
        float yInput = Input.GetAxis("Vertical") * 0.4f;
        this.transform.Translate(xInput, yInput, 0f);
        if (xInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (xInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
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
        if (Input.GetKey(KeyCode.K))
        {
            selector1 = World.CursorCoordinate();
        }
        if (Input.GetKey(KeyCode.L) && selector1 != null)
        {
            selector2 = World.CursorCoordinate();
            int bottomX = selector1.x < selector2.x ? selector1.x : selector2.x;
            int bottomY = selector1.y < selector2.y ? selector1.y : selector2.y;
            int topX = selector1.x > selector2.x ? selector1.x : selector2.x;
            int topY = selector1.y > selector2.y ? selector1.y : selector2.y;
            selector1 = selector2 = null;
            for (int j = topY; j >= bottomY; j--)
            {
                for (int i = bottomX; i <= topX; i++)
                {
                    Coordinate target = new Coordinate(i, j);
                    if (World.Full(target))
                    {
                        if (destructionSet.Add(target))
                        {
                            GameObject marker = new GameObject();
                            marker.AddComponent(typeof(SpriteRenderer));
                            SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                            sr.sprite = markerSprite;
                            sr.color = new Color(0f, 1f, 0f, 0.2f);
                            marker.transform.position = new Vector3(target.x, target.y, -0.5f);
                            destructionMarkers.Add(marker);
                        }
                    }
                }
            }
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


        if (Input.GetMouseButton(0))
        {
            Coordinate cursor = World.CursorCoordinate();
            if (World.Full(cursor))
            {
                if (destructionSet.Add(cursor))
                {
                    GameObject marker = new GameObject();
                    marker.AddComponent(typeof(SpriteRenderer));
                    SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
                    sr.sprite = markerSprite;
                    sr.color = new Color(0f, 1f, 0f, 0.2f);
                    marker.transform.position = new Vector3(cursor.x, cursor.y, -0.5f);
                    destructionMarkers.Add(marker);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (Coordinate target in destructionSet)
            {
                destructionQueue.Enqueue(target);
            }
            foreach (GameObject marker in destructionMarkers)
            {
                Destroy(marker);
            }
            destructionMarkers.Clear();
            destructionSet.Clear();
        }


        if (Input.GetKey(KeyCode.G))
        {
            World.PlaceTileAtCursor(TID.BUTTON, new T_Button(true, 8, 32));
        }
        if (Input.GetKey(KeyCode.H))
        {
            World.PlaceTileAtCursor(TID.BUTTON, new T_Button(false, 8, 32));
        }


        if (Input.GetKey(KeyCode.P))
        {
            World.PlaceTileAtCursor(TID.SHIFTER, new T_Shifter());
        }

        if (destructionQueue.Count > 0)
        {
            Coordinate target = destructionQueue.Dequeue();
            World.BreakTile(target);
            Vector3 targetPosition = new Vector3(target.x + 0.5f, target.y + 0.5f, -1.5f);
            Vector3 difference = targetPosition - beam.transform.position;
            float distance = difference.magnitude;
            beam.transform.localScale = new Vector3(distance, 1, 1);
            float angle = Vector3.Angle(difference, Vector3.right);
            if (head.transform.position.y > targetPosition.y)
            {
                angle = -angle;
            }
            if (transform.localScale.x < 0)
            {
                angle += 180;
            }
            head.transform.eulerAngles = new Vector3(0, 0, angle);
        }
        else
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = head.transform.position.z;
            Vector3 difference = mousePosition - head.transform.position;
            float angle = Vector3.Angle(difference, Vector3.right);
            if (head.transform.position.y > mousePosition.y)
            {
                angle = -angle;
            }
            if (transform.localScale.x < 0)
            {
                angle += 180;
            }
            head.transform.eulerAngles = new Vector3(0, 0, angle);
            beam.transform.localScale = new Vector3(0, 0, 0);
        }
    }
}
