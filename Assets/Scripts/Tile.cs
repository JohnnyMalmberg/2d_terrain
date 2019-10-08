using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Masks
{
    public const int FILL = unchecked((int)0x80000000);
    public const int BACK = unchecked((int)0x40000000);
    public const int BEHAVIOR = unchecked((int)0x20000000);
    public const int WIRE = unchecked((int)0x10000000);
    public const int OVERLAY = unchecked((int)0x08000000);
    public const int RANDOM_SPRITE = unchecked((int)0x04000000);
    public const int TID = unchecked((int)0x00000FFF);
    public const int BID = unchecked((int)0x003F0000);
    public const int BID_SHIFT = 16;
    public const int SPRITE = unchecked((int)0x0000F000);
    public const int SPRITE_SHIFT = 12;
}

public class Tile
{
    int data;
    public Tile(bool isFilled,
                bool isBacked,
                TID tileID,
                BID backID,
                bool randomSprite = false, int spriteIndex = 0, bool hasBehavior = false, bool hasWire = false, bool hasOverlay = false)
    {
        this.data = isFilled ? Masks.FILL : 0;
        data = data | (isBacked ? Masks.BACK : 0);
        data = data | (hasBehavior ? Masks.BEHAVIOR : 0);
        data = data | (hasWire ? Masks.WIRE : 0);
        data = data | (hasOverlay ? Masks.OVERLAY : 0);
        data = data | (randomSprite ? Masks.RANDOM_SPRITE : 0);
        data = data | (Masks.TID & (int)tileID);
        data = data | (Masks.BID & ((int)backID << Masks.BID_SHIFT));
        data = data | (Masks.SPRITE & (spriteIndex << Masks.SPRITE_SHIFT));
        //this.data = data32;
    }

    public Tile(Tile original)
    {
        this.data = original.data;
    }

    public Tile(ushort data)
    {
        this.data = data;
    }

    public bool Mergable(Tile b)
    {
        return this.data == b.data;
    }

    public bool IsFilled()
    {
        return (this.data & Masks.FILL) != 0;
    }

    public bool IsBacked()
    {
        return (this.data & Masks.BACK) != 0;
    }

    public bool HasBehavior()
    {
        return (this.data & Masks.BEHAVIOR) != 0;
    }

    public bool HasWire()
    {
        return (this.data & Masks.WIRE) != 0;
    }

    public bool HasOverlay()
    {
        return (this.data & Masks.OVERLAY) != 0;
    }

    public bool RandomSprite()
    {
        return (this.data & Masks.RANDOM_SPRITE) != 0;
    }

    public TID ID()
    {
        return (TID)(this.data & Masks.TID);
    }

    public BID BID()
    {
        return (BID)((this.data & Masks.BID) >> Masks.BID_SHIFT);
    }

    public int SpriteIndex()
    {
        return (this.data & Masks.SPRITE) >> Masks.SPRITE_SHIFT;
    }

    public void Fill(bool fill = true)
    {
        this.data = (fill ? (this.data | Masks.FILL) : (this.data & ~Masks.FILL));
    }
    public void Back(bool back = true)
    {
        this.data = (back ? (this.data | Masks.BACK) : (this.data & ~Masks.BACK));
    }
    public void Wire(bool wire = true)
    {
        this.data = (wire ? (this.data | Masks.WIRE) : (this.data & ~Masks.WIRE));
    }

    public void SetSprite(int index)
    {
        this.data = this.data & ~Masks.SPRITE;
        this.data = this.data | ((index << Masks.SPRITE_SHIFT) & Masks.SPRITE);
    }

    public void SetID(TID tileID)
    {
        this.data = this.data & ~Masks.TID;
        this.data = this.data | (((int)tileID) & Masks.TID);
    }

    public void SetBehavior(bool behavior)
    {
        this.data = (behavior ? (this.data | Masks.BEHAVIOR) : (this.data & ~Masks.BEHAVIOR));
    }


}

public enum TID
{
    NULL, STONE, DIRT, WIRE, BUTTON, SHIFTER, GARBAGE_MIXED
}

public enum BID
{
    NULL, STONE, DIRT
}



public static class TileInfo
{
    public static bool spriteDictionaryBuilt = false;
    public static readonly Dictionary<TID, string> names = new Dictionary<TID, string>{
        {TID.NULL, "debug"},
        {TID.STONE, "stone"},
        {TID.DIRT, "dirt"},
        {TID.WIRE, "wire"},
        {TID.BUTTON, "button"},
        {TID.SHIFTER, "shifter"},
        {TID.GARBAGE_MIXED, "garbageMixed"}
    };

    public static readonly Dictionary<TID, int> spriteCounts = new Dictionary<TID, int> {
        {TID.NULL, 1},
        {TID.STONE, 1},
        {TID.DIRT, 1},
        {TID.WIRE, 16},
        {TID.BUTTON, 4},
        {TID.SHIFTER, 1},
        {TID.GARBAGE_MIXED, 7}
    };

    public static readonly Dictionary<TID, bool> hasTransparency = new Dictionary<TID, bool> {
        {TID.NULL, false},
        {TID.STONE, false},
        {TID.DIRT, false},
        {TID.WIRE, true},
        {TID.BUTTON, true},
        {TID.SHIFTER, true},
        {TID.GARBAGE_MIXED, false}
    };

    public static Dictionary<TID, Sprite[]> sprites;

    public static void BuildSpriteDictionary()
    {
        if (!spriteDictionaryBuilt)
        {
            sprites = new Dictionary<TID, Sprite[]>();

            spriteDictionaryBuilt = true;

            foreach (TID id in System.Enum.GetValues(typeof(TID)))
            {
                int spriteCount = spriteCounts.ContainsKey(id) ? spriteCounts[id] : 0;
                Sprite[] spriteArray = new Sprite[spriteCount];
                for (int i = 0; i < spriteCount; i++)
                {
                    spriteArray[i] = Resources.Load<Sprite>(string.Format("Sprites/Tiles/{0}/{1}", names[id], i));
                }
                sprites.Add(id, spriteArray);
            }
        }
    }
}

public static class BackInfo
{
    public static bool spriteDictionaryBuilt = false;
    public static readonly Dictionary<BID, string> names = new Dictionary<BID, string>{
        {BID.NULL, "debug"},
        {BID.STONE, "stone"},
        {BID.DIRT, "dirt"},
    };

    public static Dictionary<BID, Sprite> sprites;

    public static void BuildSpriteDictionary()
    {
        if (!spriteDictionaryBuilt)
        {
            sprites = new Dictionary<BID, Sprite>();

            spriteDictionaryBuilt = true;

            foreach (BID id in System.Enum.GetValues(typeof(BID)))
            {
                Sprite sprite = Resources.Load<Sprite>(string.Format("Sprites/Tiles/{0}/{1}", names[id], 0));
                sprites.Add(id, sprite);
            }
        }
    }
}