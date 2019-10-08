using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class WorldManager : MonoBehaviour
{
    void Awake()
    {
        int worldResolution = (int)Mathf.Pow(2, 12);
        if (worldResolution > 8192)
        {
            Debug.LogWarning("Extreme world size may consume excessive computer resources");
        }

        Vector3 screenBL = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)) + new Vector3(-1f, -1f, 0f);
        Vector3 screenTR = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)) + new Vector3(1f, 1f, 0f);
        Vector3 screenDims = screenTR - screenBL;
        int tileRendererCount = ((int)screenDims.x + 1) * ((int)screenDims.y + 1);
        tileRendererCount *= 12;
        //this.tileRenderers = new GameObject[tileRendererCount];
        //this.spriteRenderers = new SpriteRenderer[tileRendererCount];
        //for (int i = 0; i < tileRendererCount; i++)
        //{
        //    this.tileRenderers[i] = new GameObject();
        //    this.tileRenderers[i].AddComponent<SpriteRenderer>().drawMode = SpriteDrawMode.Tiled;
        //    this.spriteRenderers[i] = this.tileRenderers[i].GetComponent<SpriteRenderer>();
        //    this.tileRenderers[i].SetActive(true);
        //}

        //previousTileCount = tileRendererCount;

        World.SetResolution(worldResolution);
        Thread worldGenThread = new Thread(new ThreadStart(World.Generate));
        worldGenThread.Start();
    }

    void FixedUpdate()
    {
        if (World.treeExists && !World.Paused())
        {
            // This logic should be inside the World class
            foreach (KeyValuePair<int, DynamicEntity> pair in World.dynamicEntities)
            {
                pair.Value.PreparePhysics();
            }
            foreach (KeyValuePair<int, DynamicEntity> pair in World.dynamicEntities)
            {
                pair.Value.ApplyPhysics();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            World.Pause();
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            World.Unpause();
        }
        if (World.treeExists && !World.Paused())
        {
            World.Tick();
            foreach (KeyValuePair<int, DynamicEntity> pair in World.dynamicEntities)
            {
                pair.Value.Tick();
            }
        }

    }
}
