using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Unity.Mathematics;
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
    public int lastDirection = 1;

    public int scavAimCharge;
    public float standStillCount;
    public bool scavWithinRange = false;

    public Vector2 camPos;
    public Vector2? aimPos = null;
    public Player.InputPackage? package = null;
    public WorldCoordinate? scavStandPos = null;

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
    public bool getRotation = false;
    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.Update();
        lastRotation = rotation;
        lastStringPos = stringPos;
        lastSpearPos = spearPos;

        if (rotationSpeed > 10f)
        { rotationSpeed = 10f; }
        else if (rotationSpeed < -10f)
        { rotationSpeed = -10f; }

        if (loadedSpear is not null)
        {
            if (loadedSpear.grabbedBy.Count == 0)
            {
                Debug.Log("Spear was not being held!");
                loadedSpear = null; aiming = false;
            }
        }

        if (grabbedBy.Count > 0)
        {
            CollideWithObjects = false;
            CollideWithTerrain = false;

            if (grabbedBy[0].grabber.mainBodyChunk.vel.magnitude < 1 && standStillCount < 10)
            { standStillCount++; }
            else
            { standStillCount = 0; }

            if (aiming)
            {
                rotation = aimDirection;
                AimBow(eu, aimDirection);
                if (aimCharge >= 20)
                {
                    soundLoop.Volume = 0f;
                }
                else if (aimCharge > 1)
                {
                    soundLoop.Volume = 1f;
                    if (soundLoop.Volume > 1f) { soundLoop.Volume = 1f; }
                }
            }
            else
            {
                bowWidth = 10f;
                bowLength = 60f;
                soundLoop.Volume = 0f;
                aimCharge = 0;
                scavAimCharge = 0;
                if (grabbedBy[0].grabber is Player player)
                {
                    getRotation = true;
                    rotation = player.GetHeldItemDirection(grabbedBy[0].graspUsed);
                    getRotation = false;
                }
                else if (grabbedBy[0].grabber is Scavenger scav && grabbedBy[0].graspUsed > 0)
                {
                    rotation = (scav.graphicsModule as ScavengerGraphics).ItemDirection(grabbedBy[0].graspUsed);
                }
                else
                {
                    rotation = Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, firstChunk.pos);
                }
                loadedSpear = null;
            }
        }
        else
        {
            bowWidth = 10f;
            bowLength = 60f;
            soundLoop.Volume = 0f;
            aimCharge = 0;
            scavAimCharge = 0;
            standStillCount = 0;
            scavStandPos = null;

            if (firstChunk.contactPoint.y != 0)
            { rotation = new Vector2(0f, 1f); }
            else
            { rotation = Custom.rotateVectorDeg(rotation, 2f); }

            CollideWithObjects = true;
            CollideWithTerrain = true;

            loadedSpear = null; aiming = false;
        }

        stringPos = firstChunk.pos - rotation * (bowWidth / 2) - rotation * aimCharge;
        spearPos = stringPos + rotation * 20f;
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
        room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, firstChunk.pos, 1f, 1f);

    }
    public void UnloadSpearFromBow()
    {
        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Scavenger)
        { loadedSpear.ChangeOverlap(false); }
        loadedSpear = null;
        aiming = false;
    }
    public void AimBow(bool eu, Vector2 aimDirection)
    {
        if (grabbedBy.Count == 0)
        {
            scavAimCharge = 0;
            return;
        }
        else if (grabbedBy[0].grabber is Player player)
        {
            //Debug.Log(Plugin.Options.aimBowControls.Value);

            scavAimCharge = 0;
            Player.InputPackage input = player.input[0];
            PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
            SlugcatHand arrowHand = graphics.hands[loadedSpear.grabbedBy[0].graspUsed];
            SlugcatHand bowHand = graphics.hands[grabbedBy[0].graspUsed];

            bool isPlayerBusy =
                player.animation == Player.AnimationIndex.ClimbOnBeam ||
                player.animation == Player.AnimationIndex.HangFromBeam ||
                player.animation == Player.AnimationIndex.HangUnderVerticalBeam;

            if (input.thrw && !isPlayerBusy)
            {
                if (aimCharge == 0)
                { aimCharge = 0.01f; }
                if (aimCharge < 20)
                { aimCharge *= 1.5f; }

                bowWidth = 10f + aimCharge / 20f;
                bowLength = 60f - aimCharge / 5f;

                graphics.LookAtPoint(aimPos.Value, 10f);
            }
            else
            {
                bowWidth = 10f;
                bowLength = 60f;
                arrowHand.pushOutOfTerrain = true;
                bowHand.pushOutOfTerrain = true;
                if (aimCharge >= 20 && !isPlayerBusy)
                {
                    float force = 40f;
                    if (player.animation == Player.AnimationIndex.Flip)
                    {
                        force = 60f;
                        loadedSpear.spearDamageBonus += 1f;
                        for (int i = 0; i < Random.Range(10, 12); i++)
                        {
                            player.room.AddObject(new Spark(firstChunk.pos, Custom.rotateVectorDeg(-aimDirection, -3f + i / 2f) * force, new Color(1f, 1f, 1f), null, 6, 8));
                        }
                    }
                    Trackers.ThrowTracker tracker = new(loadedSpear, room, force, eu)
                    {
                        thrownBy = player,
                        startPos = spearPos,
                        shootDir = aimDirection,
                    };
                    loadedSpear.room.AddObject(tracker);
                    loadedSpear.setPosAndTail(player.mainBodyChunk.pos);
                    loadedSpear.rotation = aimDirection;
                    loadedSpear.firstFrameTraceFromPos = spearPos + aimDirection * 1f;

                    IntVector2 vector;
                    if (aimDirection.x > aimDirection.y)
                    { vector = new(Math.Sign(aimDirection.x), 0); }
                    else
                    { vector = new(0, Math.Sign(aimDirection.y)); }

                    loadedSpear.Thrown(player, spearPos, spearPos + aimDirection * 20f, vector, 1f, eu);
                    loadedSpear.AllGraspsLetGoOfThisObject(true);
                    loadedSpear.firstChunk.vel = loadedSpear.firstChunk.vel.normalized * force;
                    loadedSpear.setPosAndTail(player.mainBodyChunk.pos);
                }
                loadedSpear = null;
                aimCharge = 0f;
                aiming = false;
                return;
            }
        }
        else if (grabbedBy[0].grabber is Scavenger scav)
        {
            ScavengerGraphics graphics = scav.graphicsModule as ScavengerGraphics;
            ScavengerGraphics.ScavengerHand bowHand = graphics.hands[0];
            ScavengerGraphics.ScavengerHand arrowHand = graphics.hands[1];

            if (scav.animation is ScavengerAimBowAnimation)
            {
                if (aimCharge == 0)
                { aimCharge = 0.01f; }
                if (aimCharge < 20)
                { aimCharge *= 1.5f; }

                bowWidth = 10f + aimCharge / 20f;
                bowLength = 60f - aimCharge / 5f;
            }
            else
            {
                arrowHand.mode = Limb.Mode.Dangle;
                bowHand.mode = Limb.Mode.Dangle;

                bowWidth = 10f;
                bowLength = 60f;
                if (aimCharge >= 20 && scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
                {
                    Trackers.ThrowTracker tracker = new(loadedSpear, room, 60f, eu)
                    {
                        thrownBy = scav,
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

                    loadedSpear.Thrown(scav, spearPos, spearPos + aimDirection * 20f, vector, 1f, eu);
                    scav.ReleaseGrasp(loadedSpear.grabbedBy[0].graspUsed);
                    loadedSpear.AllGraspsLetGoOfThisObject(true);
                    loadedSpear.firstChunk.vel = loadedSpear.firstChunk.vel.normalized * 40f;
                    loadedSpear.setPosAndTail(spearPos);
                }
                loadedSpear = null;
                aimCharge = 0f;
                scavAimCharge = 0;
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
            this.camPos = camPos;

            float newBowLength = bowLength - aimCharge / 10f;

            Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
            Vector2 handlePos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 handleEnd1Pos = handlePos + (Custom.rotateVectorDeg(rotVec, 90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 handleEnd2Pos = handlePos + (Custom.rotateVectorDeg(rotVec, -90) * (newBowLength / 2 - 2f)) - (rotVec * (bowWidth - 3f));
            Vector2 bowMiddlePos = handlePos - rotVec * ((bowWidth / 2) - 2f);
            Vector2 baseStringPos = Vector2.Lerp(handleEnd1Pos, handleEnd2Pos, 0.5f);
            Vector2 stringPos = baseStringPos - rotVec * aimCharge * 1.1f;

            if (grabbedBy.Count > 0)
            {
                Methods.Methods.ChangeItemSpriteLayer(this, grabbedBy[0].grabber, grabbedBy[0].graspUsed);
                if (grabbedBy[0].grabber is Player player)
                {
                    if (player.input[0].x != 0)
                    { lastDirection = player.input[0].x; }

                    if (Plugin.Options.aimBowControls.Value == "Directional Inputs")
                    { package = player.input[0]; }

                    if (Plugin.Options.aimBowControls.Value == "Mouse")
                    {
                        aimPos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + camPos;
                    }
                    else
                    {
                        if (aiming && package.HasValue && aimPos.HasValue)
                        {
                            float speed = 10f;
                            if (package.Value.pckp)
                            { speed = 3f; }
                            aimPos = new Vector2(aimPos.Value.x + package.Value.analogueDir.normalized.x * speed, aimPos.Value.y + package.Value.analogueDir.normalized.y * speed);
                        }
                        else
                        {
                            aimPos = player.mainBodyChunk.pos + new Vector2(50f * lastDirection, 0f);
                        }
                    }

                    if (aiming && loadedSpear != null && loadedSpear.grabbedBy.Count > 0)
                    {
                        aimDirection = Custom.DirVec(player.mainBodyChunk.pos, aimPos.Value);

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
                    }
                }
                else if (grabbedBy[0].grabber is Scavenger scav)
                {
                    if (scav.animation is ScavengerAimBowAnimation && scav.AI.focusCreature != null && scav.AI.focusCreature.representedCreature.realizedCreature != null)
                    {
                        Vector2 aimPos = scav.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
                        Vector2 scavPos = scav.mainBodyChunk.pos;
                        float distance = Custom.Dist(new Vector2(aimPos.x, 0), new Vector2(scavPos.x, 0));
                        aimDirection = Custom.DirVec(scav.mainBodyChunk.pos, new Vector2(aimPos.x, aimPos.y + distance * 0.05f));

                        ScavengerGraphics graphics = scav.graphicsModule as ScavengerGraphics;
                        ScavengerGraphics.ScavengerHand bowHand = graphics.hands[0];
                        ScavengerGraphics.ScavengerHand arrowHand = graphics.hands[1];

                        arrowHand.pos = stringPos;
                        arrowHand.absoluteHuntPos = stringPos;
                        arrowHand.huntSpeed = 100f;
                        arrowHand.quickness = 1f;
                        arrowHand.pushOutOfTerrain = false;
                        arrowHand.mode = Limb.Mode.HuntAbsolutePosition;

                        bowHand.absoluteHuntPos = scav.mainBodyChunk.pos + aimDirection * 100f;
                        bowHand.huntSpeed = 100f;
                        bowHand.quickness = 1f;
                        bowHand.pushOutOfTerrain = false;
                        bowHand.mode = Limb.Mode.HuntAbsolutePosition;
                    }
                }
                if (aiming && loadedSpear != null)
                {
                    ChangeOverlap(true);
                    loadedSpear.ChangeOverlap(true);
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
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}
