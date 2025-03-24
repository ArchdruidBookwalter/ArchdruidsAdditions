using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using DevInterface;

namespace ArchdruidsAdditions.Objects;

public class Potato : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public bool buried;
    public Vector2 rotation, lastRotation;
    new public Color color;
    public int bites;
    public Vector2 homePos;

    /*public AbstractConsumable AbstrConsumable
    {
        get { return this.abstractPhysicalObject as AbstractConsumable; }
    }*/

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

        /*if (room.game.devToolsActive)
        {
            room.AddObject(new ExplosionSpikes(room, bodyChunks[0].pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
            room.AddObject(new ExplosionSpikes(room, bodyChunks[1].pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
        }*/

        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = -Mathf.Abs(rotation.y) + 90;
        }
        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.3f;
        }
        /*if (!AbstrConsumable.isConsumed && (Vector2.Distance(firstChunk.pos, homePos) > 5f || grabbedBy.Count > 0))
        {
            buried = false;
            room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, firstChunk, false, 0.8f, 1.6f + UnityEngine.Random.value / 10f);
            AbstrConsumable.Consume();
        }*/
        rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.3f : 0f) * firstChunk.vel.x).normalized;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        /*if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            homePos = placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos;
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
            return;
        }*/
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        base.Grabbed(grasp);
        UnityEngine.Debug.Log("Picked Up Potato");
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
