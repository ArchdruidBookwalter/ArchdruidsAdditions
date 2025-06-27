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
using IL.MoreSlugcats;

namespace ArchdruidsAdditions.Objects;

public class Bow : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public float bowLength = 60f;
    public float bowWidth = 10f;
    public string lastLayer;

    public Spear loadedSpear;
    public Vector2 stringPos, lastStringPos;
    public Vector2 spearPos, lastSpearPos;
    public Vector2 aimDirection = new(0, 1);
    public bool aiming;
    public float aimCharge;

    public bool getRotation;

    Vector2 camPos;
    Color blackColor = new(0f, 0f, 0f);

    new public ChunkDynamicSoundLoop soundLoop;

    public Bow(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0, 0), 5f, 0.1f);

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
        lastRotation = rotation;
        stringPos = firstChunk.pos;
        lastStringPos = stringPos;
        spearPos = stringPos;
        lastSpearPos = spearPos;

        lastLayer = "Items";
        getRotation = false;

        soundLoop = new ChunkDynamicSoundLoop(this.firstChunk);
        soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
        soundLoop.Volume = 0f;
        soundLoop.Pitch = 0.5f;
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
        soundLoop.Update();
        lastRotation = rotation;
        lastStringPos = stringPos;
        lastSpearPos = spearPos;

        for (int i = 0; i < room.game.cameras.Length; i++)
        {
            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Items"));
        }

        if (rotationSpeed > 10f)
        { rotationSpeed = 10f; }
        else if (rotationSpeed < -10f)
        { rotationSpeed = -10f; }

        //bool grabbedByPlayer = false;

        if (loadedSpear is not null)
        {
            if (loadedSpear.grabbedBy.Count == 0)
            { loadedSpear = null; aiming = false; }
        }

        if (grabbedBy.Count > 0)
        {
            CollideWithObjects = false;
            CollideWithTerrain = false;

            if (aiming)
            {
                rotation = aimDirection;
                AimBow(eu, aimDirection);
                if (aimCharge >= 20)
                {
                    soundLoop.Volume = 0f;
                }
                else if (aimCharge > 5)
                {
                    soundLoop.Volume = 1f;
                    if (soundLoop.Volume > 1f) { soundLoop.Volume = 1f; }
                }
            }
            else
            {
                soundLoop.Volume = 0f;
                if (grabbedBy[0].grabber is Player player)
                {
                    //grabbedByPlayer = true;

                    float dist = Custom.Dist(firstChunk.pos, player.bodyChunks[1].pos);
                    dist = (dist - 9f) / 9f;

                    getRotation = true;
                    rotation = player.GetHeldItemDirection(grabbedBy[0].graspUsed);
                    getRotation = false;

                    //Methods.Methods.CreateLineBetweenTwoPoints(firstChunk.pos, firstChunk.pos + rotation * 20, room, new(1f, 1f, 0f));
                }
                else if (grabbedBy[0].grabber is Scavenger scav)
                { rotation = (scav.graphicsModule as ScavengerGraphics).ItemDirection(grabbedBy[0].graspUsed); }
                else
                { rotation = Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, firstChunk.pos); }
            }
        }
        else
        {
            if (firstChunk.contactPoint.y != 0)
            { rotation = new Vector2(0f, 1f); }
            else
            { rotation = Custom.rotateVectorDeg(rotation, 2f); }

            CollideWithObjects = true;
            CollideWithTerrain = true;
        }

        stringPos = firstChunk.pos - rotation * (bowWidth / 2) - rotation * aimCharge;
        spearPos = stringPos + rotation * 20f;

        //room.AddObject(new ColoredShapes.Rectangle(room, firstChunk.pos, 0.2f, 5f, Custom.VecToDeg(rotation), new(0f, 1f, 0f), 10));
        //room.AddObject(new ColoredShapes.Rectangle(room, spearPos, 1f, 1f, Custom.VecToDeg(rotation), new(1f, 1f, 0f), 10));
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
                { aimCharge = 0.01f; }
                if (aimCharge < 20)
                { aimCharge *= 1.2f; }

                bowWidth = 10f + aimCharge / 20f;
                bowLength = 60f - aimCharge / 5f;

                loadedSpear.ChangeOverlap(true);

                graphics.LookAtPoint(mousePos, 10f);
            }
            else
            {
                bowWidth = 10f;
                bowLength = 60f;
                arrowHand.pushOutOfTerrain = true;
                bowHand.pushOutOfTerrain = true;
                if (aimCharge >= 20)
                {
                    Trackers.ThrowTracker tracker = new(loadedSpear, room, 60f, eu)
                    {
                        thrownBy = player,
                        startPos = spearPos,
                        shootDir = aimDirection,
                    };
                    loadedSpear.room.AddObject(tracker);
                    loadedSpear.setPosAndTail(spearPos);
                    loadedSpear.rotation = aimDirection;
                    loadedSpear.firstFrameTraceFromPos = spearPos + aimDirection * 1f;

                    IntVector2 vector = new IntVector2();
                    if (aimDirection.x > aimDirection.y)
                    { vector = new(Math.Sign(aimDirection.x), 0); }
                    else
                    { vector = new(0, Math.Sign(aimDirection.y)); }

                    loadedSpear.Thrown(player, spearPos, spearPos + aimDirection * 20f, vector, 1f, eu);
                    loadedSpear.AllGraspsLetGoOfThisObject(true);
                }
                loadedSpear = null;
                aimCharge = 0f;
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
        newContainer ??= rCam.ReturnFContainer(lastLayer);

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
            Vector2 handlePos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 handleEnd1Pos = handlePos + (Custom.rotateVectorDeg(rotVec, 90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 handleEnd2Pos = handlePos + (Custom.rotateVectorDeg(rotVec, -90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 bowMiddlePos = handlePos - rotVec * ((bowWidth / 2) - 2f);
            Vector2 baseStringPos = Vector2.Lerp(handleEnd1Pos, handleEnd2Pos, 0.5f);
            Vector2 stringPos = baseStringPos - rotVec * aimCharge * 1.1f;

            this.camPos = camPos;

            if (grabbedBy.Count > 0)
            {
                Methods.Methods.ChangeItemSpriteLayer(this, grabbedBy[0].grabber, grabbedBy[0].graspUsed);
                if (grabbedBy[0].grabber is Player player)
                {
                    if (aiming && loadedSpear != null && loadedSpear.grabbedBy.Count > 0)
                    {
                        mousePos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + camPos;
                        aimDirection = Custom.DirVec(player.mainBodyChunk.pos, mousePos);

                        PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
                        SlugcatHand arrowHand = graphics.hands[loadedSpear.grabbedBy[0].graspUsed];
                        SlugcatHand bowHand = graphics.hands[grabbedBy[0].graspUsed];

                        if (rotVec.x < 0)
                        { arrowHand.pos = stringPos + Custom.PerpendicularVector(rotVec) * 2f; }
                        else { arrowHand.pos = stringPos - Custom.PerpendicularVector(rotVec) * 2f; }

                        arrowHand.reachingForObject = true;
                        arrowHand.absoluteHuntPos = stringPos;
                        arrowHand.huntSpeed = 100f;
                        arrowHand.quickness = 1f;
                        arrowHand.pushOutOfTerrain = false;

                        bowHand.reachingForObject = true;
                        bowHand.absoluteHuntPos = player.mainBodyChunk.pos + aimDirection * 100f;
                        bowHand.huntSpeed = 100f;
                        bowHand.quickness = 1f;
                        bowHand.pushOutOfTerrain = false;

                        //Vector2 arrowHandPos = Vector2.Lerp(arrowHand.lastPos, arrowHand.pos, timeStacker);
                        //room.AddObject(new ColoredShapes.Rectangle(room, arrowHandPos, 1f, 1f, 45f, new(1f, 0f, 0f), 1));
                        //room.AddObject(new ColoredShapes.Rectangle(room, stringPos, 1f, 1f, 45f, new(1f, 1f, 0f), 1));

                        loadedSpear.ChangeOverlap(true);
                    }
                    if (room.game.devToolsActive && player.input.Length > 0 && player.input[0].y > 0)
                    {
                        //room.AddObject(new ColoredShapes.Rectangle(room, handleEnd1Pos, 1f, 1f, 45f, new Color(1f, 0f, 0f), 1));
                        //room.AddObject(new ColoredShapes.Rectangle(room, handleEnd2Pos, 1f, 1f, 45f, new Color(1f, 0f, 0f), 1));
                    }
                }
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

            Debug.Log(sLeaser.sprites[0].container);
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}
