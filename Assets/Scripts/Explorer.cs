using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : DynamicEntity
{

    public GameObject beam;
    public GameObject head;

    public HashSet<Coordinate> destructionSet;
    public List<GameObject> destructionMarkers;
    public Queue<Coordinate> destructionQueue;

    Coordinate selector1;
    Coordinate selector2;

    Vector2 vectorSelector1;
    Vector2 vectorSelector2;

    public Sprite markerSprite;

    public int maxEnergy;
    public int energy;

    // Start is called before the first frame update
    void Start()
    {
        destructionSet = new HashSet<Coordinate>();
        destructionQueue = new Queue<Coordinate>();
        destructionMarkers = new List<GameObject>();
        selector1 = selector2 = null;
        maxEnergy = energy = 1000;
    }

    public override void PreparePhysics()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log(transform.position);
        }

        float xInput = Input.GetAxis("Horizontal") * 0.4f;

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            velocity += Vector3.up * 0.42f;
            canJump = false;
        }

        velocity += Vector3.down * gravity;
        velocity += Vector3.right * xInput;

        velocity.x *= 0.4f;
        if (Mathf.Abs(velocity.x) > 15f)
        {
            velocity.x /= Mathf.Abs(velocity.x);
            velocity.x *= 5f;
        }
        if (Mathf.Abs(velocity.y) > 15f)
        {
            velocity.y /= Mathf.Abs(velocity.y);
            velocity.y *= 5f;
        }


        Vector3 oldPosition = Camera.main.transform.position;
        Vector3 cameraTarget = transform.position;
        cameraTarget.z = oldPosition.z;
        Camera.main.transform.position = (oldPosition * 7f + cameraTarget) / 8f;

        if (xInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (xInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public override void Tick()
    {
        if (destructionQueue.Count > 0)
        {
            if (energy >= 20)
            {
                energy -= 20;
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
                beam.transform.localScale = new Vector3(0, 0, 0);
            }
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

    // Update is called once per frame
    void Update()
    {
        if (energy < maxEnergy)
        {
            energy += 1000;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            selector1 = World.CursorCoordinate();
        }
        if (Input.GetKeyUp(KeyCode.R) && selector1 != null)
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
                            sr.color = new Color(0f, 1f, 0f);
                            marker.transform.position = new Vector3(target.x, target.y, -0.5f);
                            destructionMarkers.Add(marker);
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vectorSelector1 = new Vector2(mousePos.x, mousePos.y);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            vectorSelector2 = new Vector2(mousePos.x, mousePos.y);
            float radius = Mathf.Abs((vectorSelector1 - vectorSelector2).magnitude);
            for (int i = (int)(vectorSelector1.x - radius - 1); i <= vectorSelector1.x + radius + 1; i++)
            {
                for (int j = (int)(vectorSelector1.y - radius - 1); j <= vectorSelector1.y + radius + 1; j++)
                {
                    Vector2 pos = new Vector2(i, j);
                    float posRad = Mathf.Abs((vectorSelector1 - pos).magnitude);
                    if (posRad < radius)
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
                                sr.color = new Color(0f, 1f, 0f);
                                marker.transform.position = new Vector3(target.x, target.y, -0.5f);
                                destructionMarkers.Add(marker);
                            }
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
            World.worldRenderer.UpdateAtCoordinate(World.CursorCoordinate());
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
                    sr.color = new Color(0f, 1f, 0f);
                    marker.transform.position = new Vector3(cursor.x, cursor.y, -0.5f);
                    destructionMarkers.Add(marker);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            destructionQueue.Clear();
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


    }
}
