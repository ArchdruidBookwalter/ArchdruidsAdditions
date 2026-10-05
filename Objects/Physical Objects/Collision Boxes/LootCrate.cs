using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevInterface;
using Watcher;
using static ArchdruidsAdditions.Objects.AshPepperBushRepresentation;
using static ArchdruidsAdditions.Objects.PotatoRepresentation.PotatoPanel;

namespace ArchdruidsAdditions.Objects;

public class LootCrate : PhysicalObject
{
    public AbstractCrate AbstractCrate
    { get { return abstractPhysicalObject as AbstractCrate; } }

    public DynamicLevelElement element;
    public CollisionBox box;
    public int size;
    public int damage;
    public bool init;

    public Vector2 setPos;

    public int sizeCounter = 1;

    public LootCrate(AbstractPhysicalObject absObj, Room room, int inputSize, int inputDamage) : base(absObj)
    {
        this.room = room;
        size = Mathf.Clamp(inputSize, 2, 5);
        damage = inputDamage;

        float chunkRad = size * 5f;
        float chunkMass = size * 2f;
        float chunkDist = size * 5f;

        bodyChunks = new BodyChunk[4];
        bodyChunks[0] = new(this, 0, room.MiddleOfTile(absObj.pos) + new Vector2(chunkDist, chunkDist), chunkRad, chunkMass);
        bodyChunks[1] = new(this, 0, room.MiddleOfTile(absObj.pos) + new Vector2(-chunkDist, chunkDist), chunkRad, chunkMass);
        bodyChunks[2] = new(this, 0, room.MiddleOfTile(absObj.pos) + new Vector2(-chunkDist, -chunkDist), chunkRad, chunkMass);
        bodyChunks[3] = new(this, 0, room.MiddleOfTile(absObj.pos) + new Vector2(chunkDist, -chunkDist), chunkRad, chunkMass);
        bodyChunkConnections = new BodyChunkConnection[6];
        bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], Custom.Dist(bodyChunks[0].pos, bodyChunks[1].pos), BodyChunkConnection.Type.Normal, 1f, -1f);
        bodyChunkConnections[1] = new(bodyChunks[1], bodyChunks[2], Custom.Dist(bodyChunks[1].pos, bodyChunks[2].pos), BodyChunkConnection.Type.Normal, 1f, -1f);
        bodyChunkConnections[2] = new(bodyChunks[2], bodyChunks[3], Custom.Dist(bodyChunks[2].pos, bodyChunks[3].pos), BodyChunkConnection.Type.Normal, 1f, -1f);
        bodyChunkConnections[3] = new(bodyChunks[3], bodyChunks[0], Custom.Dist(bodyChunks[3].pos, bodyChunks[0].pos), BodyChunkConnection.Type.Normal, 1f, -1f);
        bodyChunkConnections[4] = new(bodyChunks[0], bodyChunks[2], Custom.Dist(bodyChunks[0].pos, bodyChunks[2].pos), BodyChunkConnection.Type.Normal, 1f, -1f);
        bodyChunkConnections[5] = new(bodyChunks[1], bodyChunks[3], Custom.Dist(bodyChunks[1].pos, bodyChunks[3].pos), BodyChunkConnection.Type.Normal, 1f, -1f);

        CollideWithTerrain = true;
        CollideWithObjects = true;

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        gravity = 0.9f;
        windAffectiveness = 0.01f;

        element = new(room.MiddleOfTile(absObj.pos), new Vector2(10f, 10f), GetTexture(size, damage), Mathf.FloorToInt(0f), null, null);
        room.AddObject(element);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        //bodyChunks[0].HardSetPosition(setPos);
        //bodyChunks[0].vel *= 0f;

        Vector2 middlePos = Custom.LineIntersection(bodyChunks[0].pos, bodyChunks[2].pos, bodyChunks[1].pos, bodyChunks[3].pos);
        Vector2 crateRot = Custom.DirVec(bodyChunks[3].pos, bodyChunks[0].pos);

        element.texture = GetTexture(size, damage);
        element.pos = middlePos;
        element.scale = new Vector2(1f, 1f) * (element.texture.width / /*((float)sizeCounter / 50)*/ 15.5f);
        element.rotation = Custom.VecToDeg(crateRot);
        element.depthOffset = 5f / 30f;

        box.Update(middlePos, (bodyChunks[0].vel + bodyChunks[1].vel + bodyChunks[2].vel + bodyChunks[3].vel) / 4f, Custom.VecToDeg(crateRot));
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);

        Vector2 setPos;
        if (size % 2 == 0)
        {
            Vector2 positionInTile = AbstractCrate.pObj.pos - room.MiddleOfTile(AbstractCrate.pObj.pos);
            Vector2 cornerPos = new(positionInTile.x > 0 ? 10 : -10, positionInTile.y > 0 ? 10 : -10);

            setPos = room.MiddleOfTile(AbstractCrate.pObj.pos) + cornerPos;
        }
        else
        {
            setPos = room.MiddleOfTile(AbstractCrate.pObj.pos);
        }

        bodyChunks[0].HardSetPosition(setPos + new Vector2(10f * size, 10f * size));
        bodyChunks[1].HardSetPosition(setPos + new Vector2(-10f * size, 10f * size));
        bodyChunks[2].HardSetPosition(setPos + new Vector2(-10f * size, -10f * size));
        bodyChunks[3].HardSetPosition(setPos + new Vector2(10f * size, -10f * size));
        this.setPos = setPos;

        if (!init)
        {
            init = true;

            Vector2[] boxVertices =
            [
                new Vector2(10f * size, 10f * size),
                new Vector2(-10f * size, 10f * size),
                new Vector2(-10f * size, -10f * size),
                new Vector2(10f * size, -10f * size),
            ];
            box = new(this, boxVertices);
        }
    }

    public Texture GetTexture(int size, int damage)
    {
        string spriteName = "Crate" + size + "x" + size + "_" + damage;

        if (!Futile.atlasManager.DoesContainAtlas(spriteName))
        {
            Texture2D texture = new(1, 1);
            string filePath = AssetManager.ResolveFilePath("atlases" + Path.DirectorySeparatorChar + "leveltextures" + Path.DirectorySeparatorChar + spriteName + ".png");
            AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + filePath, true, true);
            HeavyTexturesCache.LoadAndCacheAtlasFromTexture(spriteName, texture, false);
        }

        FAtlas newAtlas = Futile.atlasManager.GetAtlasWithName(spriteName);
        if (newAtlas == null || newAtlas.texture == null)
        {
            Debug.Log("");
            Debug.Log("ATLAS WAS NULL");
            Debug.Log("");

            return null;
        }

        return newAtlas.texture;
    }
}

public class LootCrateData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    new public int minRegen;
    new public int maxRegen;
    public int size;

    public Room room;

    public LootCrateData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        minRegen = 2;
        maxRegen = 3;
        size = 2;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", new object[]
        {
            panelPos.x,
            panelPos.y,
            minRegen,
            maxRegen,
            size
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        size = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class LootCrateRep : ConsumableRepresentation
{
    public LootCrateData Data { get { return pObj.data as LootCrateData; } }
    new public LootCratePanel controlPanel;
    public FSprite panelLine;

    public FSprite rectSprite1;
    public FSprite rectSprite2;
    public FSprite rectSprite3;
    public FSprite rectSprite4;

    public class LootCratePanel : ConsumableControlPanel
    {
        public SizeSlider sizeSlider;
        public LootCrateData Data { get { return (parentNode as LootCrateRep).Data; } }
        public LootCratePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) :
            base(owner, IDstring, parentNode, pos, name)
        {
            size = new Vector2(250f, 65f);

            subNodes.Add(sizeSlider = new(owner, "Size", this, new Vector2(5f, 45f)));
        }
        public override void Refresh()
        {
            base.Refresh();
        }
        public class SizeSlider : Slider
        {
            public LootCrateData Data
            {
                get
                {
                    return (parentNode as LootCratePanel).Data;
                }
            }

            public SizeSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
            : base(owner, IDstring, parentNode, pos, "Crate Size: ", false, 110f)
            {
            }

            public override void Refresh()
            {
                base.Refresh();

                float newNubPos = 0;

                if (IDstring == "Size")
                {
                    newNubPos = (Data.size - 2) / 3f;

                    NumberText = Data.size.ToString();
                }

                RefreshNubPos(newNubPos);
            }

            public override void NubDragged(float nubPos)
            {
                if (IDstring == "Size")
                {
                    Data.size = Mathf.RoundToInt((nubPos * 3f) + 2);
                }

                parentNode.parentNode.Refresh();
                Refresh();
            }
        }
    }

    public LootCrateRep(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        controlPanel = new(owner, "Loot_Crate_Panel", this, Data.panelPos, "Consumable: Loot Crate");

        (pObj.data as PlacedObject.ConsumableObjectData).minRegen = Data.minRegen;
        (pObj.data as PlacedObject.ConsumableObjectData).maxRegen = Data.maxRegen;

        subNodes[0].ClearSprites();
        subNodes.RemoveAt(0);

        subNodes.Add(controlPanel);

        panelLine = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(panelLine);
        owner.placedObjectsContainer.AddChild(panelLine);


        rectSprite1 = new FSprite("pixel");
        fSprites.Add(rectSprite1);
        owner.placedObjectsContainer.AddChild(rectSprite1);

        rectSprite2 = new FSprite("pixel");
        fSprites.Add(rectSprite2);
        owner.placedObjectsContainer.AddChild(rectSprite2);

        rectSprite3 = new FSprite("pixel");
        fSprites.Add(rectSprite3);
        owner.placedObjectsContainer.AddChild(rectSprite3);

        rectSprite4 = new FSprite("pixel");
        fSprites.Add(rectSprite4);
        owner.placedObjectsContainer.AddChild(rectSprite4);
    }

    public override void Refresh()
    {
        base.Refresh();

        MoveSprite(fSprites.IndexOf(panelLine), absPos);
        panelLine.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        panelLine.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as LootCrateData).panelPos = controlPanel.pos;

        Data.minRegen = (pObj.data as PlacedObject.ConsumableObjectData).minRegen;
        Data.maxRegen = (pObj.data as PlacedObject.ConsumableObjectData).maxRegen;

        Vector2 rectPos;
        if (Data.size % 2 == 0)
        {
            IntVector2 intTilePos = new(Mathf.RoundToInt(pObj.pos.x / 20f), Mathf.RoundToInt(pObj.pos.y / 20f));
            rectPos = (intTilePos.ToVector2() * 20f) - owner.room.game.cameras[0].pos;
        }
        else
        {
            IntVector2 intTilePos = new(Mathf.FloorToInt(pObj.pos.x / 20f), Mathf.FloorToInt(pObj.pos.y / 20f));
            Vector2 tileCornerPos = intTilePos.ToVector2() * 20f;
            rectPos = new Vector2(tileCornerPos.x + 10f, tileCornerPos.y + 10f) - owner.room.game.cameras[0].pos;
        }

        float lineWidth = 1f;

        Vector2 cornerPos1 = rectPos + new Vector2(10f * Data.size, 10f * Data.size);
        Vector2 cornerPos2 = rectPos + new Vector2(-10f * Data.size, 10f * Data.size);
        Vector2 cornerPos3 = rectPos + new Vector2(-10f * Data.size, -10f * Data.size);
        Vector2 cornerPos4 = rectPos + new Vector2(10f * Data.size, -10f * Data.size);

        MoveSprite(fSprites.IndexOf(rectSprite1), Vector2.Lerp(cornerPos1, cornerPos2, 0.5f));
        rectSprite1.rotation = Custom.VecToDeg(Custom.DirVec(cornerPos1, cornerPos2));
        rectSprite1.scaleX = lineWidth;
        rectSprite1.scaleY = Custom.Dist(cornerPos1, cornerPos2);

        MoveSprite(fSprites.IndexOf(rectSprite2), Vector2.Lerp(cornerPos2, cornerPos3, 0.5f));
        rectSprite2.rotation = Custom.VecToDeg(Custom.DirVec(cornerPos2, cornerPos3));
        rectSprite2.scaleX = lineWidth;
        rectSprite2.scaleY = Custom.Dist(cornerPos2, cornerPos3);

        MoveSprite(fSprites.IndexOf(rectSprite3), Vector2.Lerp(cornerPos3, cornerPos4, 0.5f));
        rectSprite3.rotation = Custom.VecToDeg(Custom.DirVec(cornerPos3, cornerPos4));
        rectSprite3.scaleX = lineWidth;
        rectSprite3.scaleY = Custom.Dist(cornerPos3, cornerPos4);

        MoveSprite(fSprites.IndexOf(rectSprite4), Vector2.Lerp(cornerPos4, cornerPos1, 0.5f));
        rectSprite4.rotation = Custom.VecToDeg(Custom.DirVec(cornerPos4, cornerPos1));
        rectSprite4.scaleX = lineWidth;
        rectSprite4.scaleY = Custom.Dist(cornerPos4, cornerPos1);
    }
}

public class AbstractCrate(World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, PlacedObject pObj, WorldCoordinate pos, EntityID ID, int originRoom, int placedObjectIndex, LootCrateData data) : AbstractConsumable(world, type, realizedObject, pos, ID, originRoom, placedObjectIndex, data)
{
    public PlacedObject pObj = pObj;
}