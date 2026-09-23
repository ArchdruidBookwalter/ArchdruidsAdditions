using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Watcher;
using static System.Collections.Specialized.BitVector32;

namespace ArchdruidsAdditions.Objects;

public class HangingPlatform : PhysicalObject
{
    public Chain leftChain;
    public Chain rightChain;
    public DynamicLevelElement element;
    public CollisionBox box;

    public Vector2 attachPos;

    public AbstractHangingPlatform AbstractChandelier
    {
        get { return abstractPhysicalObject as AbstractHangingPlatform; }
    }

    public float length;
    public bool init;

    public HangingPlatform(Room room, AbstractPhysicalObject absObj, float length, string spriteName) : base(absObj)
    {
        this.room = room;

        bodyChunks = new BodyChunk[2];
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i] = new BodyChunk(this, i, Vector2.zero, 1f, 20f);
        }
        bodyChunkConnections = new BodyChunkConnection[1];
        bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], length, BodyChunkConnection.Type.Normal, 0.8f, -1f);

        CollideWithTerrain = true;
        CollideWithObjects = true;

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        gravity = 0.9f;
        windAffectiveness = 0.01f;

        Texture2D texture = new(1, 1);
        string filePath = AssetManager.ResolveFilePath("atlases" + Path.DirectorySeparatorChar + "leveltextures" + Path.DirectorySeparatorChar + spriteName + ".png");
        AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + filePath, true, true);
        HeavyTexturesCache.LoadAndCacheAtlasFromTexture(spriteName, texture, false);

        FAtlas newAtlas = Futile.atlasManager.GetAtlasWithName(spriteName);
        if (newAtlas == null || newAtlas.texture == null)
        {
            Debug.Log("");
            Debug.Log("ATLAS WAS NULL");
            Debug.Log("");
        }

        element = new(room.MiddleOfTile(absObj.pos), new Vector2(10f, 10f), newAtlas.texture, Mathf.FloorToInt(0f), null, null);
        room.AddObject(element);

        this.length = length;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (Custom.DistLess(bodyChunks[0].pos, bodyChunks[1].pos, length))
        {
            Vector2 pos = Vector2.Lerp(bodyChunks[0].pos, bodyChunks[1].pos, 0.5f);
            Vector2 dir = Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos);

            bodyChunks[0].pos = pos - dir * (length / 2);
            bodyChunks[1].pos = pos + dir * (length / 2);
        }

        float symmetry = 0.25f;

        RopeObject.RopeSegment leftChainLink = this.leftChain.chain.ropeSegments[this.leftChain.chain.EndIndex];
        Vector2 dirVec1 = Custom.DirVec(bodyChunks[0].pos, leftChainLink.pos);
        float dist1 = Mathf.Max(0, Custom.Dist(bodyChunks[0].pos, leftChainLink.pos) - 5) * 0.5f;
        bodyChunks[0].pos += dirVec1 * dist1 * symmetry;
        bodyChunks[0].vel += dirVec1 * dist1 * symmetry;
        leftChainLink.pos -= dirVec1 * dist1 * (1f / symmetry);
        leftChainLink.vel -= dirVec1 * dist1 * (1f / symmetry);

        RopeObject.RopeSegment rightChainLink = this.rightChain.chain.ropeSegments[this.rightChain.chain.EndIndex];
        Vector2 dirVec2 = Custom.DirVec(bodyChunks[1].pos, rightChainLink.pos);
        float dist2 = Mathf.Max(0, Custom.Dist(bodyChunks[1].pos, rightChainLink.pos) - 5) * 0.5f;
        bodyChunks[1].pos += dirVec2 * dist2 * symmetry;
        bodyChunks[1].vel += dirVec2 * dist2 * symmetry;
        rightChainLink.pos -= dirVec2 * dist2 * (1f / symmetry);
        rightChainLink.vel -= dirVec2 * dist2 * (1f / symmetry);

        leftChain.UpdateGraphics();
        rightChain.UpdateGraphics();

        Vector2 pos2 = Vector2.Lerp(bodyChunks[0].pos, bodyChunks[1].pos, 0.5f);
        Vector2 dir2 = -Custom.PerpendicularVector(Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos));

        element.pos = pos2 + dir2 * 10f;
        element.scale = new Vector2(Custom.Dist(bodyChunks[0].pos, bodyChunks[1].pos) / 12f, 60f / 8f);
        element.rotation = Custom.VecToDeg(dir2);

        box.Update(pos2, bodyChunks[0].vel, Custom.VecToDeg(dir2));

        Create_Square(room, attachPos, 20f, 20f, Vec(45), Color.red, 0);
        Create_Square(room, AbstractChandelier.pObj.pos, 20f, 20f, Vec(45), Color.green, 0);

        Create_Square(room, bodyChunks[0].pos, 20f, 20f, Vec(45), Color.red, 0);
        Create_Square(room, bodyChunks[1].pos, 20f, 20f, Vec(45), Color.green, 0);
        Create_LineBetweenTwoPoints(room, bodyChunks[0].pos, bodyChunks[1].pos, 1f, Color.yellow, 0);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);

        IntVector2 tile = room.GetTilePosition(AbstractChandelier.pObj.pos);
        IntVector2 attachTile = new(0, 0);

        for (int i = tile.y; i < room.Height; i++)
        {
            attachTile = new(tile.x, i);
            if (room.HasAnySolid(attachTile))
            {
                break;
            }
        }

        Vector2 pos = room.MiddleOfTile(tile);
        attachPos = room.MiddleOfTile(attachTile);

        bodyChunks[0].HardSetPosition(pos + new Vector2(length / 2, 0f));
        bodyChunks[1].HardSetPosition(pos + new Vector2(-length / 2, 0f));

        if (!init)
        {
            init = true;

            if (leftChain == null)
            {
                leftChain = new(room, null, 0.1f, 0.2f, attachPos, bodyChunks[0].pos, false);
                placeRoom.AddObject(leftChain);
            }
            if (rightChain == null)
            {
                rightChain = new(room, null, 0.1f, 0.2f, attachPos, bodyChunks[1].pos, false);
                placeRoom.AddObject(rightChain);
            }

            Vector2[] boxVertices =
            [
                new Vector2(40f, 30f),
                new Vector2(-40f, 30f),
                new Vector2(-40f, 10f),
                new Vector2(-110f, 10f),
                new Vector2(-110f, -10f),
                new Vector2(-40f, -10f),
                new Vector2(-40f, -30f),
                new Vector2(40f, -30f),
                new Vector2(40f, -10f),
                new Vector2(110f, -10f),
                new Vector2(110f, 10f),
                new Vector2(40f, 10f)
            ];
            box = new(this, boxVertices);
        }

        Debug.Log(tile);
    }

    public override void Destroy()
    {
        base.Destroy();
        element.Destroy();

        leftChain.Destroy();
        rightChain.Destroy();

        box.boxHandler.collisionBoxes.Remove(box);
    }
}

public class HangingPlatformData : PlacedObject.Data
{
    public HangingPlatformData(PlacedObject owner) : base(owner)
    {
    }
}

public class AbstractHangingPlatform(World world, AbstractPhysicalObject.AbstractObjectType type, PhysicalObject realizedObject, PlacedObject pObj, WorldCoordinate pos, EntityID ID) : AbstractPhysicalObject(world, type, realizedObject, pos, ID)
{
    public PlacedObject pObj = pObj;
}
