using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using HUD;

using Random = UnityEngine.Random;
using static ArchdruidsAdditions.Methods.Methods;
using static ArchdruidsAdditions.Objects.HUDObjects.SpiceMeter;
using ArchdruidsAdditions.Data;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Items;

public class FirePepper : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public int bites = 1;
    public Vector2 rotation, lastRotation;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return abstractPhysicalObject as AbstractConsumable;
        }
    }

    public FirePepper(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, default, 4f, 0.2f);
        bodyChunkConnections = [];

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        gravity = 0.9f;
        collisionLayer = 1;

        rotation = Vector2.up;

        bodyColor = Custom.HSL2RGB(0f, 0.8f, 0.3f);
        shineColor = Custom.HSL2RGB(0f, 0.8f, 0.4f);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        if (grabbedBy.Count == 0 && firstChunk.vel.magnitude > 1f)
        {
            rotation = Custom.rotateVectorDeg(rotation, firstChunk.vel.magnitude);
        }
        else if (grabbedBy.Count > 0)
        {
            rotation = Vector2.up;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;

        if (!AbstrConsumable.isConsumed)
        {
            bodyChunks[0].HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
        }
        else
        {
            bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        }
    }

    #region Consumable Stuff
    public int BitesLeft
    { get { return bites; } }

    public int FoodPoints
    { get { return 1; } }

    public bool Edible
    { get { return true; } }

    public bool AutomaticPickUp
    { get { return true; } }

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

    #region Graphics Stuff
    public Color bodyColor, shineColor, blackColor;
    public FSprite pepper;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        pepper = new("FirePepper1")
        {
            scale = 0.8f,
            color = bodyColor
        };
        sprites.Add(pepper);

        sLeaser.sprites = sprites.ToArray();

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
            Vector2 chunkPos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
            Vector2 rot = Vector2.Lerp(lastRotation, rotation, timeStacker);

            pepper.SetPosition(chunkPos);
            pepper.rotation = Custom.VecToDeg(rot);
            pepper.element = Futile.atlasManager.GetElementWithName("FirePepper1");
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}
