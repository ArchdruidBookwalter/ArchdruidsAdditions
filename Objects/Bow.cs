using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using ArchdruidsAdditions.Methods;
using System.Diagnostics.Eventing.Reader;
using IL.Watcher;

namespace ArchdruidsAdditions.Objects;

public class Bow : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public float bowLength = 60f;
    public float bowWidth = 10f;

    public Spear loadedSpear;
    public Vector2 stringPos;
    public Vector2 aimDirection;
    public bool aiming;
    public float aimCharge;

    Vector2 camPos;
    Color blackColor = new(0f, 0f, 0f);

    public Bow(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0, 0), 1f, 0.1f);

        bodyChunkConnections = [];

        collisionLayer = 2;
        CollideWithObjects = true;
        CollideWithSlopes = true;
        CollideWithTerrain = true;
        gravity = 0.9f;

        airFriction = 0.999f;
        bounce = 0.3f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 0.2f;
        rotation = new(0f, 1f);
    }

    #region Behavior
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
    }
    bool oneShot = true;
    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        for (int i = 0; i < room.game.cameras.Length; i++)
        {
            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Items"));
        }

        if (rotationSpeed > 10f)
        { rotationSpeed = 10f; }
        else if (rotationSpeed < -10f)
        { rotationSpeed = -10f; }

        if (grabbedBy.Count > 0)
        {
            foreach (BodyChunk chunk in bodyChunks)
            {
                chunk.collideWithObjects = false;
                chunk.collideWithSlopes = false;
                chunk.collideWithTerrain = false;
            }
            rotation = Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, firstChunk.pos);
            if (aiming)
            {
                AimBow(eu, aimDirection);
            }
        }
        else
        {
            if (firstChunk.contactPoint.y != 0)
            { rotation = new Vector2(0f, 1f); }
            rotation = Custom.rotateVectorDeg(rotation, 2f);
            foreach (BodyChunk chunk in bodyChunks)
            {
                chunk.collideWithObjects = true;
                chunk.collideWithSlopes = true;
                chunk.collideWithTerrain = true;
            }
        }
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }
    public override void PickedUp(Creature upPicker)
    {
        base.PickedUp(upPicker);
    }
    public override void HitByWeapon(Weapon weapon)
    {
    }
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        return false;
    }
    public void LoadSpearIntoBow(Spear spear)
    {
        loadedSpear = spear;
        aiming = true;

    }
    Vector2 mousePos = new(0f, 0f);
    public void AimBow(bool eu, Vector2 aimDirection)
    {
        if (grabbedBy.Count == 0)
        { return; }
        else if (grabbedBy[0].grabber is Player player)
        {
            Player.InputPackage input = player.input[0];
            PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
            SlugcatHand arrowHand = graphics.hands[loadedSpear.grabbedBy[0].graspUsed];
            SlugcatHand bowHand = graphics.hands[grabbedBy[0].graspUsed];

            if (input.thrw)
            {
                if (aimCharge == 0)
                { aimCharge++; }
                else if (aimCharge < 30)
                { aimCharge *= 1.1f; }

                bowWidth = 10f + aimCharge / 20f;
                bowLength = 60f - aimCharge / 5f;

                loadedSpear.thrownPos = firstChunk.pos;
                loadedSpear.setRotation = aimDirection;
                loadedSpear.rotation = aimDirection;
                loadedSpear.lastRotation = aimDirection;
                loadedSpear.ChangeOverlap(true);

                float stringDist = Custom.Dist(player.mainBodyChunk.pos, stringPos);

                arrowHand.reachingForObject = true;
                arrowHand.absoluteHuntPos = stringPos;
                arrowHand.huntSpeed = 100f;
                arrowHand.quickness = 1f;
                arrowHand.pushOutOfTerrain = false;
                room.AddObject(new Trackers.LimbTracker(arrowHand, room, stringPos, 100, false));

                bowHand.reachingForObject = true;
                bowHand.absoluteHuntPos = player.mainBodyChunk.pos + Custom.DirVec(player.mainBodyChunk.pos, mousePos) * 100f;
                bowHand.huntSpeed = 100f;
                bowHand.quickness = 1f;
                bowHand.pushOutOfTerrain = false;

                graphics.LookAtPoint(mousePos, 10f);

                //room.AddObject(new ColoredShapes.Rectangle(room, mousePos, 5f, 5f, Custom.VecToDeg(loadedSpear.rotation), new(1f, 1f, 0f), 1));
            }
            else
            {
                bowWidth = 10f;
                bowLength = 60f;
                arrowHand.pushOutOfTerrain = true;
                bowHand.pushOutOfTerrain = true;
                if (aimCharge >= 30)
                {
                    loadedSpear.thrownPos = firstChunk.pos;
                    loadedSpear.setPosAndTail(firstChunk.pos + aimDirection * 20f);

                    Trackers.ThrowTracker tracker = new(loadedSpear, room, 60f, eu)
                    {
                        thrownBy = player,
                        startPos = firstChunk.pos,
                        shootDir = aimDirection,
                    };
                    loadedSpear.room.AddObject(tracker);

                    IntVector2 vector = new IntVector2((int)Math.Abs(aimDirection.x), (int)Math.Abs(aimDirection.y));
                    loadedSpear.Thrown(player, firstChunk.pos, null, vector, 1f, eu);

                    loadedSpear.AllGraspsLetGoOfThisObject(true);
                    loadedSpear.setRotation = aimDirection;
                    loadedSpear.rotation = aimDirection;
                    loadedSpear.lastRotation = aimDirection;

                    //room.AddObject(new ColoredShapes.Rectangle(room, loadedSpear.thrownPos, 1f, 30f, Custom.VecToDeg(loadedSpear.rotation), new(0f, 1f, 0f), 50));
                    //room.AddObject(new ColoredShapes.Rectangle(room, loadedSpear.thrownPos + aimDirection * 20f, 5f, 5f, Custom.VecToDeg(loadedSpear.rotation), new(0f, 1f, 0f), 50));
                }
                else
                {
                    loadedSpear.setRotation = aimDirection;
                }
                loadedSpear = null;
                aimCharge = 0;
                aiming = false;
                return;
            }
        }
    }
    #endregion

    #region Appearance
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite("Bow", true);
        sLeaser.sprites[1] = new FSprite("pixel", true);
        sLeaser.sprites[2] = new FSprite("pixel", true);

        AddToContainer(sLeaser, rCam, null);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            float newBowLength = bowLength - aimCharge / 10f;

            Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
            Vector2 handlePos = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker);
            Vector2 handleEnd1Pos = handlePos + (Custom.rotateVectorDeg(rotVec, 90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 handleEnd2Pos = handlePos + (Custom.rotateVectorDeg(rotVec, -90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 bowMiddlePos = handlePos - rotVec * ((bowWidth / 2) - 2f);
            Vector2 stringMiddlePos = Vector2.Lerp(handleEnd1Pos, handleEnd2Pos, 0.5f);

            stringPos = stringMiddlePos - rotVec * aimCharge;
            aimDirection = rotVec;
            mousePos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + camPos;
            this.camPos = camPos;

            if (room.game.devToolsActive && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player && player.input[0].y > 0)
            {
                room.AddObject(new ColoredShapes.Rectangle(room, handleEnd1Pos, 1f, 1f, 45f, new Color(1f, 0f, 0f), 1));
                room.AddObject(new ColoredShapes.Rectangle(room, handleEnd2Pos, 1f, 1f, 45f, new Color(1f, 0f, 0f), 1));
            }

            sLeaser.sprites[0].x = bowMiddlePos.x - camPos.x;
            sLeaser.sprites[0].y = bowMiddlePos.y - camPos.y;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(Custom.rotateVectorDeg(rotVec, -90));
            sLeaser.sprites[0].scale = 50f;
            sLeaser.sprites[0].height = newBowLength;
            sLeaser.sprites[0].width = bowWidth;

            sLeaser.sprites[1].SetPosition(Vector2.Lerp(handleEnd1Pos, stringPos, 0.5f) - camPos);
            sLeaser.sprites[1].height = Custom.Dist(stringPos, handleEnd1Pos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(Custom.DirVec(handleEnd1Pos, stringPos));

            sLeaser.sprites[2].SetPosition(Vector2.Lerp(handleEnd2Pos, stringPos, 0.5f) - camPos);
            sLeaser.sprites[2].height = Custom.Dist(stringPos, handleEnd2Pos);
            sLeaser.sprites[2].rotation = Custom.VecToDeg(Custom.DirVec(handleEnd2Pos, stringPos));

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (blink > 0 && Random.value < 0.5f)
                { sLeaser.sprites[i].color = blinkColor; }
                else
                { sLeaser.sprites[i].color = blackColor; }
            }
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}
