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
    public Color rootColor;
    public Color blackColor = new(0f, 0f, 0f);
    public int bites = 3;
    public Vector2 homePos;

    public Potato(AbstractPhysicalObject abstractPhysicalObject, bool buried, Vector2 rotation, Color color) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[2];
        bodyChunks[0] = new BodyChunk(this, 0, default, 6f, 0.2f);
        bodyChunks[1] = new BodyChunk(this, 0, default, 0.1f, 0.1f);
        bodyChunkConnections = new BodyChunkConnection[1];
        bodyChunkConnections[0] = new BodyChunkConnection(bodyChunks[0], bodyChunks[1], 30f, BodyChunkConnection.Type.Normal, 0.5f, -1f);

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

        bodyChunks[1].collideWithObjects = false;

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        this.buried = buried;
        this.rotation = rotation;
        
        rootColor = color;

        homePos = firstChunk.pos;
    }

    #region Behavior

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        if (room.game.devToolsActive)
        {
            room.AddObject(new ExplosionSpikes(room, bodyChunks[0].pos, 50, 0f, 2f, 7f, 2f, new(1f, 0f, 0f)));
            room.AddObject(new ExplosionSpikes(room, bodyChunks[1].pos, 50, 0f, 2f, 7f, 2f, new(1f, 0f, 0f)));
        }

        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.3f;
        }

        rotation = Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        bodyChunks[1].HardSetPosition(bodyChunks[0].pos + new Vector2(30f, 0f));
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
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite("Potato1", true);
        sLeaser.sprites[1] = new FSprite("PotatoStem1", true);
        sLeaser.sprites[2] = new FSprite("PotatoStem2", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 rootPosVec = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker);
        Vector2 leavesPosVec = Vector2.Lerp(bodyChunks[1].lastPos, bodyChunks[1].pos, timeStacker);
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);

        sLeaser.sprites[0].x = rootPosVec.x - camPos.x;
        sLeaser.sprites[0].y = rootPosVec.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec) - 30;

        sLeaser.sprites[1].x = leavesPosVec.x - camPos.x;
        sLeaser.sprites[1].y = leavesPosVec.y - camPos.y;
        sLeaser.sprites[1].rotation = Custom.VecToDeg(rotVec) + 90;

        Vector2 rootTopPosVec = rootPosVec + rotVec * 10f;

        sLeaser.sprites[2].x = Vector2.Lerp(rootTopPosVec, leavesPosVec, 0.5f).x - camPos.x;
        sLeaser.sprites[2].y = Vector2.Lerp(rootTopPosVec, leavesPosVec, 0.5f).y - camPos.y;
        sLeaser.sprites[2].width = Custom.Dist(rootTopPosVec, leavesPosVec);
        sLeaser.sprites[2].rotation = Custom.VecToDeg(rotVec) + 90;

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
            sLeaser.sprites[1].color = blinkColor;
            sLeaser.sprites[2].color = blinkColor;
        }
        else
        {
            sLeaser.sprites[0].color = rootColor;
            sLeaser.sprites[1].color = blackColor;
            sLeaser.sprites[2].color = blackColor;
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


