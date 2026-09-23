using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DevInterface;
using Watcher;

namespace ArchdruidsAdditions.Objects;

public class ShrineBowl : UpdatableAndDeletable, IDrawable
{
    public DynamicLevelElement element;
    public FSprite symbolSprite;
    public PlacedObject pObj;
    public float depth;

    public CollisionBox collisionBox;
    public Vector2 pos, lastPos;
    public Color symbolColor;

    public bool givenOffering;
    public float symbolBrightness;

    public ShrineBowl(Room room, PlacedObject pObj, float depth)
    {
        this.room = room;
        this.depth = depth;
        this.pObj = pObj;

        int section = 0;

        try
        {
            section = 1;

            string assetName = "Bowl";
            Texture2D texture = new(1, 1);
            string filePath = AssetManager.ResolveFilePath("atlases" + Path.DirectorySeparatorChar + "leveltextures" + Path.DirectorySeparatorChar + assetName + ".png");
            AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + filePath, true, true);
            HeavyTexturesCache.LoadAndCacheAtlasFromTexture(assetName, texture, false);

            section = 2;

            FAtlas newAtlas = Futile.atlasManager.GetAtlasWithName("Bowl");
            if (newAtlas == null || newAtlas.texture == null)
            {
                Debug.Log("");
                Debug.Log("ATLAS WAS NULL");
                Debug.Log("");
            }

            section = 3;

            element = new(pObj.pos, new Vector2(4f, 2f), newAtlas.texture, Mathf.FloorToInt(5f), null, null);
            room.AddObject(element);

            section = 4;

            symbolSprite = new("BowlSymbol", false);

            symbolColor = Custom.HSL2RGB(0.3f, 0.05f, 0.4f);
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "CHAIN_CTOR", section);
        }

        section = 4;

        Vector2[] collisionVertices =
        [
            new Vector2(30, 10),
            new Vector2(5, -5),
            new Vector2(-5, -5),
            new Vector2(-30, 10),
            new Vector2(-30, -20),
            new Vector2(30, -20),
        ];
        collisionBox = new(this, collisionVertices);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        lastPos = pos;

        Vector2 middleTilePos = room.MiddleOfTile(pObj.pos);
        Vector2 inTilePos = pObj.pos - middleTilePos;

        float newX;
        if (inTilePos.x < -15 || inTilePos.x > 15)
        { newX = 20 * Mathf.Sign(inTilePos.x); }
        else if (inTilePos.x < -5 || inTilePos.x > 5)
        { newX = 10 * Mathf.Sign(inTilePos.x); }
        else
        { newX = 0; }

        float newY;
        if (inTilePos.y < -15 || inTilePos.y > 15)
        { newY = 20 * Mathf.Sign(inTilePos.y); }
        else if (inTilePos.y < -5 || inTilePos.y > 5)
        { newY = 10 * Mathf.Sign(inTilePos.y); }
        else
        { newY = 0; }

        pos = middleTilePos + new Vector2(newX, newY) + new Vector2(0f, 5f);

        element.pos = pos;
        element.setDepthOffset = Mathf.FloorToInt(depth * 30f);
        element.rotation = 0;

        collisionBox.Update(pos, Vector2.zero, 0f);

        givenOffering = false;
        for (int i = 0; i < 3; i++)
        {
            List<PhysicalObject> objs = room.physicalObjects[i];
            foreach (PhysicalObject obj in objs)
            {
                if (obj is IPlayerEdible && Custom.DistLess(pos, obj.firstChunk.pos, 30f))
                {
                    givenOffering = true;
                }
            }
        }
    }
    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        symbolSprite = new("BowlSymbol", false)
        {
            scaleX = 0.5f,
            scaleY = 0.4f
        };
        sprites.Add(symbolSprite);

        sLeaser.sprites = [.. sprites];

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            symbolSprite.SetPosition(Vector2.Lerp(lastPos, this.pos, timeStacker) - camPos);
            symbolSprite.color = symbolColor;
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer mainContainer)
    {
        mainContainer ??= rCam.ReturnFContainer("Foreground");

        mainContainer.AddChild(symbolSprite);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

}
public class ShrineBowlData : PlacedObject.Data
{
    public Vector2 panelPos;
    public float depth;

    public ShrineBowl realizedBowl;

    public ShrineBowlData(PlacedObject owner) : base(owner)
    {
        panelPos = new(0, 100);
        depth = 0;
    }

    public string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}", new object[]
        {
            panelPos.x,
            panelPos.y,
            depth
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");

        int failIndex = 0;
        try
        {
            panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            depth = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 3);
        }
        catch
        {
            if (failIndex < 1)
            {
                panelPos.x = 0;
            }
            if (failIndex < 2)
            {
                panelPos.y = 0;
            }
            if (failIndex < 3)
            {
                depth = 0f;
            }
            if (failIndex < 4)
            {
                unrecognizedAttributes = [];
            }
        }
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class ShrineBowlRepresentation : PlacedObjectRepresentation
{
    public ShrineBowlData data;
    public ShrineBowlControlPanel controlPanel;
    public FSprite line;

    public ShrineBowlRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as ShrineBowlData;
        controlPanel = new(owner, "Shrine_Bowl_Panel", this, data.panelPos, new Vector2(250f, 5f), "Shrine Bowl");

        subNodes.Add(controlPanel);

        line = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(line);
        owner.placedObjectsContainer.AddChild(line);
    }

    public class ShrineBowlControlPanel : Panel
    {
        public ShrineBowlData data;
        public ShrineBowl realiedBowl;
        public ShrineBowlControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string name) :
            base(owner, IDstring, parentNode, pos, size, name)
        {
            data = (parentNode as ShrineBowlRepresentation).data;
        }
        public override void Refresh()
        {
            base.Refresh();
        }
    }

    public override void Refresh()
    {
        base.Refresh();

        (pObj.data as ShrineBowlData).panelPos = controlPanel.pos;

        MoveSprite(fSprites.IndexOf(line), absPos);
        line.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        line.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
    }
}
