using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileRenderer : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        spriteRenderer.sprite = TileInfo.sprites[TID.NULL][0];
    }

    public void SetLayer(WorldLayer layer)
    {
        switch (layer)
        {
            case WorldLayer.OVERLAY:
                transform.position = new Vector3(transform.position.x, transform.position.y, 9);
                spriteRenderer.color = Color.white;
                break;

            case WorldLayer.TILE:
                transform.position = new Vector3(transform.position.x, transform.position.y, 10);
                spriteRenderer.color = Color.white;
                break;

            case WorldLayer.WIRE:
                transform.position = new Vector3(transform.position.x, transform.position.y, 12);
                spriteRenderer.color = Color.black;
                break;

            case WorldLayer.BACK:
                transform.position = new Vector3(transform.position.x, transform.position.y, 15);
                spriteRenderer.color = Color.gray;
                break;

            default:
                break;
        }
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void SetCoordinate(Coordinate coordinate)
    {
        transform.position = new Vector3(coordinate.x, coordinate.y, transform.position.z);
    }

    public void SetVisible(bool visible = true)
    {
        transform.rotation = Quaternion.Euler(visible ? 0 : 90, 0, 0);
    }
}

public enum WorldLayer
{
    OVERLAY,
    TILE,
    WIRE,
    BACK
}
