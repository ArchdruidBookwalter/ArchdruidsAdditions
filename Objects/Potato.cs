using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using DevInterface;
using System.Security.Policy;
using System.Globalization;

namespace ArchdruidsAdditions.Objects;

public class Potato : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public bool buried;
    public Vector2 rotation, lastRotation;
    new public Color color;
    public int bites;
    public Vector2 homePos;
    public PotatoStem stem;

    public Potato(AbstractPhysicalObject abstractPhysicalObject, bool buried, Vector2 rotation, Color color) : base(abstractPhysicalObject)
    {
        bodyChunks = [new BodyChunk(this, 0, default, 8f, 0.2f)];
        bodyChunkConnections = [];

        if (buried)
        {
            collisionLayer = 0;
            CollideWithObjects = false;
            CollideWithSlopes = false;
            CollideWithTerrain = false;
            gravity = 0f;
        }
        else
        {
            collisionLayer = 1;
            CollideWithObjects = true;
            CollideWithSlopes = true;
            CollideWithTerrain = true;
            gravity = 0.9f;
        }

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        this.buried = buried;
        this.rotation = rotation;
        this.color = color;

        homePos = firstChunk.pos;
        bites = 3;
    }

    #region Behavior

    public override void Update(bool eu)
    {
        base.Update(eu);

        lastRotation = rotation;


        #region
        if (room.game.devToolsActive)
        {
            room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
        }
        #endregion


        #region
        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = -Mathf.Abs(rotation.y) + 90;
        }
        #endregion


        #region
        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.3f;
        }
        #endregion


        #region
        #endregion


        rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.3f : 0f) * firstChunk.vel.x).normalized;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));

        AbstractPhysicalObject newObj = new(abstractPhysicalObject.world, Enums.AbstractObjectType.PotatoStem, null, abstractPhysicalObject.pos, room.game.GetNewID());
        stem = new(newObj, this);
        room.AddObject(stem);
        stem.firstChunk.pos = firstChunk.pos;

        //new BasicAbstractStick(abstractPhysicalObject, stem.abstractPhysicalObject);
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        base.Grabbed(grasp);
        Debug.Log("Picked Up Potato");
    }

    #region Edible Stuff
    public int BitesLeft
    {
        get { return bites; }
    }

    public int FoodPoints
    {
        get { return 1; }
    }

    public bool Edible
    {
        get { return true; }
    }

    public bool AutomaticPickUp
    {
        get { return true; }
    }

    public void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        bites--;

        if (bites == 0) { room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk.pos); }
        else { room.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos); }

        if (bites < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public void ThrowByPlayer()
    {
    }
    #endregion

    #endregion

    #region Visuals
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("Potato1", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 posVec = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);

        sLeaser.sprites[0].x = posVec.x - camPos.x;
        sLeaser.sprites[0].y = posVec.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec) - 30;

        if (buried)
        {
            sLeaser.sprites[0].alpha = 0;
        }

        if (bites > 0)
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("Potato" + (4 - bites).ToString());
        }

        if (blink > 0 && UnityEngine.Random.value < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
        }
        else
        {
            sLeaser.sprites[0].color = color;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }
    #endregion
}

public class PotatoStem : PlayerCarryableItem, IDrawable
{
    public Vector2 rotation, lastRotation;
    public Color blackColor;
    public Potato potato;
    public PotatoStem(AbstractPhysicalObject abstractPhysicalObject, Potato potato) : base(abstractPhysicalObject)
    {
        this.potato = potato;
        
        bodyChunks = [new(this, 0, default, 0f, 0f)];
        bodyChunkConnections = [];

        collisionLayer = 0;
        CollideWithTerrain = true;
        gravity = 0.9f;
        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        blackColor = new(0f, 0f, 0f);
    }

    #region Behavior
    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        rotation = Custom.DirVec(firstChunk.pos, potato.firstChunk.pos);

        if (room.game.devToolsActive)
        {
            room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
        }

        if (potato.slatedForDeletetion)
        {
            Destroy();
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
    }
    #endregion

    #region Visuals
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("PotatoStem1", true);
        sLeaser.sprites[1] = new FSprite("PotatoStem2", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 chunk1Vec = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 chunk2Vec = Vector2.Lerp(potato.firstChunk.lastPos, potato.firstChunk.pos, timeStacker);
        Vector2 stemPos = Vector2.Lerp(chunk1Vec, chunk2Vec, 0.5f);
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);

        sLeaser.sprites[0].x = chunk1Vec.x - camPos.x;
        sLeaser.sprites[0].y = chunk1Vec.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec) + 90;

        sLeaser.sprites[1].x = stemPos.x - camPos.x;
        sLeaser.sprites[1].y = stemPos.y - camPos.y;
        sLeaser.sprites[1].rotation = Custom.VecToDeg(rotVec) + 90;

        sLeaser.sprites[1].width = Custom.Dist(chunk1Vec, chunk2Vec);

        if (blink > 0 && UnityEngine.Random.value < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
            sLeaser.sprites[1].color = blinkColor;
        }
        else
        {
            sLeaser.sprites[0].color = blackColor;
            sLeaser.sprites[1].color = blackColor;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}


