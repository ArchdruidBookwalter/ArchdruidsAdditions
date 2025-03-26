using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using IL;
using On;
using MoreSlugcats;

namespace ArchdruidsAdditions.Objects;

public class ParrySword : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public Vector2 aimDirection;
    public float lastDirection;
    public string faceDirection;

    public Player grabbedPlayer;
    int playerMaxKarma;

    public Color swordColor;
    public LightSource lightSource1;
    public LightSource lightSource2;

    public bool useBool, activeBool, spin;
    public float useTime, charge, rejectTime, cooldown;
    public int usedNum;

    new public ChunkDynamicSoundLoop soundLoop;

    public ParrySword(AbstractPhysicalObject abstractPhysicalObject, World world, Color color) : base(abstractPhysicalObject, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, default, 5f, 0.1f);
        bodyChunkConnections = [];

        collisionLayer = 2;
        CollideWithObjects = true;
        CollideWithSlopes = true;
        CollideWithTerrain = true;
        gravity = 0.9f;

        airFriction = 0.999f;
        bounce = 0.5f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 0.2f;

        this.swordColor = color;
        this.rotation = new(0f, 1f);
        this.aimDirection = new(0f, 1f);
        this.faceDirection = "left";
        this.lastDirection = 0f;
        this.soundLoop = new ChunkDynamicSoundLoop(this.firstChunk);
        this.soundLoop.sound = SoundID.Vulture_Grub_Laser_LOOP;
        this.soundLoop.Volume = 0f;
        rejectTime = 0f;
        charge = 0f;
        usedNum = 0;
        activeBool = false;
        playerMaxKarma = 1;
        spin = false;
    }

    #region Behavior

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.Update();

        #region Behavior

        //room.AddObject(new ExplosionSpikes(room, bodyChunks[0].pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));

        lastRotation = rotation;
        firstChunk.collideWithTerrain = grabbedBy.Count == 0;

        #region Grabbed By Player

        if (grabbedBy.Count > 0)
        {
            if (grabbedBy[0].grabber is Player)
            {
                grabbedPlayer = grabbedBy[0].grabber as Player;
                Player.InputPackage input = grabbedPlayer.input[0];

                #region Charge

                playerMaxKarma = grabbedPlayer.KarmaCap + 1;
                if (charge < 1000 / playerMaxKarma)
                {
                    charge++;
                }
                else if (charge == 1000 / playerMaxKarma && !activeBool)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 2f, swordColor, null, 50, 50));
                    }
                    room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, firstChunk.pos, 0.5f, 1.8f);
                    room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 100f, 1f, 5, swordColor));
                    room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 2f, 5f, 7f, 100f, swordColor));
                    room.AddObject(new ShockWave(firstChunk.pos, 100f, 0.05f, 5, false));

                    activeBool = true;
                }

                #endregion

                #region Object Detector

                for (int i = 0; i < 3; i++)
                {
                    var physicalObjects = room.physicalObjects[i];
                    for (var j = 0; j < physicalObjects.Count(); j++)
                    {
                        var obj = physicalObjects[j];

                        /*
                        if (obj is BigNeedleWorm noodlefly)
                        {
                            noodlefly.BigAI.SocialEvent(SocialEventRecognizer.EventID.Killing, grabbedPlayer as Creature, noodlefly, null);
                            Vector2 fangPos1 = noodlefly.bodyChunks[0].pos + (noodlefly.fangLength * Custom.DirVec(noodlefly.bodyChunks[1].pos, noodlefly.bodyChunks[0].pos) * 1f);

                            room.AddObject(new ExplosionSpikes(room, noodlefly.bodyChunks[0].pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                            room.AddObject(new ExplosionSpikes(room, fangPos1, 50, 1f, 2f, 7f, 5f, new(1f, 1f, 1f)));

                            if (noodlefly.AI.behavior == NeedleWormAI.Behavior.Attack)
                            {
                                UnityEngine.Debug.Log("Noodlefly Detected. Velocity: " + noodlefly.mainBodyChunk.vel.magnitude);
                                room.AddObject(new ExplosionSpikes(room, noodlefly.bodyChunks[0].pos, 50, 10f, 50f, 7f, 5f, new(0f, 0f, 1f)));
                            }
                            if (noodlefly.chargingAttack > 0.5f || noodlefly.swishDir != null)
                            {
                                room.AddObject(new ExplosionSpikes(room, noodlefly.BigAI.attackFromPos, 50, 10f, 50f, 7f, 5f, new(1f, 0f, 0f)));
                                room.AddObject(new ExplosionSpikes(room, noodlefly.BigAI.attackTargetPos, 50, 10f, 50f, 7f, 5f, new(1f, 0f, 0f)));
                            }
                        }

                        if (obj is Cicada squidcada)
                        {
                            room.AddObject(new ExplosionSpikes(room, squidcada.bodyChunks[0].pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                            if (squidcada.Charging)
                            {
                                room.AddObject(new ExplosionSpikes(room, squidcada.bodyChunks[0].pos, 50, 10f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                            }
                        }


                        if (obj is Lizard lizard)
                        {
                            Vector2 vel = new(20, 20);
                            vel = Custom.rotateVectorDeg(vel, UnityEngine.Random.Range(0, 360));
                            if (lizard.animation == Lizard.Animation.FightingStance)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 0f, 0f), null, 50, 50)); //Black
                            }
                            if (lizard.animation == Lizard.Animation.HearSound)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 0f, 1f), null, 50, 50)); //Blue
                            }
                            if (lizard.animation == Lizard.Animation.Jumping)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 1f, 0f), null, 50, 50)); //Green
                            }
                            if (lizard.animation == Lizard.Animation.Lounge)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 0f, 0f), null, 50, 50)); //Red
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 0f, 0f), null, 50, 50)); 
                            }
                            if (lizard.animation == Lizard.Animation.PrepareToJump)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 0.5f, 0f), null, 50, 50)); //Dark Green
                            }
                            if (lizard.animation == Lizard.Animation.PrepareToLounge)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0.5f, 0f, 0f), null, 50, 50)); //Dark Red
                            }
                            if (lizard.animation == Lizard.Animation.PreyReSpotted)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 1f, 0f), null, 50, 50)); //Yellow
                            }
                            if (lizard.animation == Lizard.Animation.PreySpotted)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 0f, 1f), null, 50, 50)); //Magenta
                            }
                            if (lizard.animation == Lizard.Animation.ShakePrey)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 1f, 1f), null, 50, 50)); //Cyan
                            }
                            if (lizard.animation == Lizard.Animation.ShootTongue)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 0.5f, 0f), null, 50, 50)); //Orange
                            }
                            if (lizard.animation == Lizard.Animation.Spit)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0.5f, 0f, 1f), null, 50, 50)); //Purple
                            }
                            if (lizard.animation == Lizard.Animation.Standard)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0.5f, 0.5f, 0f), null, 50, 50)); //Dark Yellow
                            }
                            if (lizard.animation == Lizard.Animation.ThreatReSpotted)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(0f, 0.5f, 0.5f), null, 50, 50)); //Dark Magenta
                            }
                            if (lizard.animation == Lizard.Animation.ThreatSpotted)
                            {
                                room.AddObject(new Spark((lizard.graphicsModule as LizardGraphics).head.pos, vel, new(1f, 1f, 1f), null, 50, 50)); //White
                            }
                        }

                        if (obj is Vulture vulture)
                        {
                            room.AddObject(new ExplosionSpikes(room, vulture.Head().pos, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));

                            if (vulture.AI.DynamicRelationship((grabbedPlayer as Creature).abstractCreature).GoForKill)
                            {
                                room.AddObject(new ExplosionSpikes(room, vulture.Head().pos, 50, 10f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                            }

                            if (vulture.IsKing)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    KingTusks.Tusk[] tusks = vulture.kingTusks.tusks;
                                    KingTusks.Tusk tusk = vulture.kingTusks.tusks[k];
                                    Vector2 tuskPos1 = tusk.chunkPoints[0, 0];
                                    Vector2 tuskPos2 = tusk.chunkPoints[1, 0];
                                    Vector2 tuskPoint = tuskPos1 + Custom.DirVec(tuskPos2, tuskPos1) * 50;

                                    room.AddObject(new ExplosionSpikes(room, tuskPos1, 50, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                                    room.AddObject(new ExplosionSpikes(room, tuskPos2, 50, 1f, 2f, 7f, 5f, new(0f, 1f, 0f)));
                                    room.AddObject(new ExplosionSpikes(room, tuskPoint, 50, 1f, 2f, 7f, 5f, new(0f, 0f, 1f)));
                                }
                            }
                        }
                        */
                    }
                }

                #endregion

                #region Reject Player

                if (grabbedPlayer.KarmaCap < 1)
                {
                    rejectTime++;
                    soundLoop.Volume = 1f;
                    soundLoop.Pitch = 1f + rejectTime / 100f;
                }
                else
                {
                    soundLoop.Volume = 0f;
                    rejectTime = 0;
                }
                if (rejectTime > 0 && rejectTime % 10 == 0)
                {
                    room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 100f, 1f, 5, new(1f, 0f, 0f)));
                }
                if (rejectTime == 50f)
                {
                    soundLoop.Volume = 0f;
                    room.PlaySound(SoundID.Centipede_Shock, grabbedPlayer.mainBodyChunk.pos, 1f, 1f);
                    room.AddObject(new Explosion(room, this, firstChunk.pos, 10, 50, 50, 0, 10, 0.1f, null, 0f, 10, 10));
                    room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 100f, 1f, 5, swordColor));
                    room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 2f, 5f, 7f, 100f, swordColor));
                    room.AddObject(new ShockWave(firstChunk.pos, 100f, 0.05f, 5, false));
                    grabbedPlayer.Stun(200);
                    room.AddObject(new CreatureSpasmer(grabbedPlayer, false, grabbedPlayer.stun));
                    grabbedPlayer.LoseAllGrasps();
                    rejectTime = 0f;
                }
                #endregion

                #region Rotation
                int grasp;
                if (grabbedBy[0].graspUsed == 0)
                {
                    grasp = -1;
                    faceDirection = "left";
                }
                else
                {
                    grasp = 1;
                    faceDirection = "right";
                }

                if (input.x != 0 || input.y != 0)
                {
                    aimDirection = Custom.DirVec(this.grabbedBy[0].grabber.mainBodyChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos + new Vector2(input.x, input.y));
                    if (aimDirection.x != 0f)
                    {
                        lastDirection = aimDirection.x;
                    }
                }

                (grabbedPlayer.graphicsModule as PlayerGraphics).LookAtPoint(grabbedPlayer.mainBodyChunk.pos + aimDirection * 100, 10f);
                Vector2 playerUpVec = Custom.DirVec(grabbedPlayer.bodyChunks[1].pos, grabbedPlayer.bodyChunks[0].pos);
                SlugcatHand hand = (grabbedPlayer.graphicsModule as PlayerGraphics).hands[grabbedBy[0].graspUsed];

                if (useBool == false)
                {
                    if (grabbedPlayer.bodyMode == Player.BodyModeIndex.Crawl
                        || grabbedPlayer.bodyMode == Player.BodyModeIndex.CorridorClimb
                        || grabbedPlayer.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut
                        || grabbedPlayer.animation == Player.AnimationIndex.ClimbOnBeam
                        || grabbedPlayer.animation == Player.AnimationIndex.Flip
                        || grabbedPlayer.animation == Player.AnimationIndex.Roll
                        || grabbedPlayer.animation == Player.AnimationIndex.BellySlide
                        || grabbedPlayer.animation == Player.AnimationIndex.ZeroGSwim
                        || grabbedPlayer.animation == Player.AnimationIndex.ZeroGPoleGrab)
                    {
                        rotation = playerUpVec;
                    }
                    else if (grabbedPlayer.animation == Player.AnimationIndex.HangFromBeam)
                    {
                        rotation = Custom.rotateVectorDeg(playerUpVec, 90f);
                    }
                    else
                    {
                        rotation = Custom.DegToVec(30 * grasp);
                    }
                }
                /*
                if (useBool == true && rejectTime == 0f)
                {
                    if (aimDirection.y == 0)
                    {
                        if (grasp == aimDirection.x)
                        {
                            if (grabbedPlayer.animation == Player.AnimationIndex.ZeroGSwim
                                || grabbedPlayer.animation == Player.AnimationIndex.ZeroGPoleGrab)
                            {
                                hand.absoluteHuntPos = grabbedPlayer.mainBodyChunk.pos + new Vector2(aimDirection.x * 20, 0) * 1;
                                rotation = Custom.DegToVec(10 * grasp);
                            }
                            else
                            {
                                hand.absoluteHuntPos = grabbedPlayer.mainBodyChunk.pos + new Vector2(-aimDirection.x * 2, 0) * 1;
                                rotation = Custom.DegToVec(100 * grasp);
                            }
                        }
                        else
                        {
                            if (grabbedPlayer.animation == Player.AnimationIndex.ZeroGSwim
                                || grabbedPlayer.animation == Player.AnimationIndex.ZeroGPoleGrab)
                            {
                                hand.absoluteHuntPos = grabbedPlayer.mainBodyChunk.pos + new Vector2(aimDirection.x * 20, 0) * 1;
                                rotation = Custom.DegToVec(190 * grasp);
                            }
                            else
                            {
                                hand.absoluteHuntPos = grabbedPlayer.mainBodyChunk.pos + new Vector2(-aimDirection.x * 2, 0) * 1;
                                rotation = Custom.DegToVec(-100 * grasp);
                            }
                        }
                    }
                    else
                    {
                        if (aimDirection.y > 0)
                        {
                            if (aimDirection.x == 0)
                            {
                                hand.pos = grabbedPlayer.mainBodyChunk.pos + Custom.rotateVectorDeg(new Vector2(0, 20), 45 * grasp);
                                rotation = Custom.DegToVec(-80 * grasp);
                            }
                            else
                            {
                                hand.pos = grabbedPlayer.mainBodyChunk.pos + Custom.rotateVectorDeg(new Vector2(aimDirection.x * 20, 20), Custom.VecToDeg(aimDirection));
                                rotation = Custom.DegToVec(-45 * aimDirection.x);
                            }
                        }
                        else
                        {
                            hand.pos = grabbedPlayer.mainBodyChunk.pos + Custom.rotateVectorDeg(new Vector2(0, aimDirection.y * 20), 45 * grasp);
                            rotation = Custom.DegToVec(80 * grasp);
                        }
                    }
                }
                */
                #endregion
            }
        }
        else
        {
            if (charge > 0)
            {
                charge--;
            }
            activeBool = false;
            soundLoop.Volume = 0f;
            rejectTime = 0f;

            if (firstChunk.ContactPoint.y < 0)
            {
                if (rotation.x > 0)
                {
                    rotation = Custom.DegToVec(90);
                }
                else
                {
                    rotation = Custom.DegToVec(270);
                }

                BodyChunk firstChunk = base.firstChunk;
                firstChunk.vel.x = firstChunk.vel.x * 0.1f;
            }
            else if (spin)
            {
                rotation = Custom.rotateVectorDeg(rotation, 45f);
            }
        }

        #endregion

        #region Used

        if (usedNum >= 3)
        {
            ActivateLongCooldown();
        }

        if (useBool == true)
        {
            useTime++;
            if (useTime > 10)
            {
                useBool = false;
            }
        }
        else
        {
            useTime = 1;
        }

        #endregion

        if (cooldown > 0)
        {
            cooldown--;
        }

        #endregion

        #region Visuals

        float alpha;
        if (activeBool)
        {
            alpha = 1f;
        }
        else
        {
            alpha = 1f * (charge / (1000 / playerMaxKarma));
        }

        if (lightSource1 == null)
        {
            lightSource1 = new LightSource(firstChunk.pos, true, swordColor, this);
            lightSource1.affectedByPaletteDarkness = 0.5f;
            room.AddObject(lightSource1);
        }
        else
        {
            lightSource1.setPos = new Vector2?(firstChunk.pos + rotation * 30);
            lightSource1.setRad = new float?(100f);
            lightSource1.setAlpha = new float?(0.5f * alpha);
            if (lightSource1.slatedForDeletetion || lightSource1.room != room)
            {
                lightSource1 = null;
            }
        }

        if (lightSource2 == null)
        {
            lightSource2 = new LightSource(firstChunk.pos, true, swordColor, this);
            lightSource2.affectedByPaletteDarkness = 0.5f;
            lightSource2.flat = true;
            room.AddObject(lightSource2);
        }
        else
        {
            lightSource2.HardSetPos(firstChunk.pos + rotation * 30);
            lightSource2.setRad = new float?(40f);
            lightSource2.setAlpha = new float?(0.3f * alpha);
            if (lightSource2.slatedForDeletetion || lightSource2.room != room)
            {
                lightSource2 = null;
            }
        }

        #endregion

    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (firstChunk.vel.magnitude > 30)
        {
            spin = true;
        }
        if (firstChunk.vel.magnitude > 10)
        {
            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, firstChunk.pos, 1f, 1.2f);
            for (int i = 0; i < 3; i++)
            {
                room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 2f, new(1f, 1f, 1f), null, 50, 50));
            }
        }
        else if (firstChunk.vel.magnitude > 5)
        {
            spin = false;
            room.PlaySound(SoundID.Spear_Stick_In_Ground, firstChunk.pos, 1f, 1.2f);
        }
        else
        {
            spin = false;
        }
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }

    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, firstChunk.pos, 2f, 2f);
    }

    public void Use()
    {
        if (cooldown == 0 && activeBool)
        {
            room.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk.pos, 0.2f, 0.6f);
            room.AddObject(new ParryHitbox(this, grabbedBy[0].grabber.mainBodyChunk.pos, aimDirection, 1f, 1f));
            useBool = true;
            cooldown = 10;
        }
    }

    public void ActivateLongCooldown()
    {
        if (grabbedPlayer.KarmaCap == 9)
        {
            return;
        }
        else
        {
            room.PlaySound(SoundID.Centipede_Shock, firstChunk.pos);
            for (int i = 0; i < 3; i++)
            {
                room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 2f, swordColor, null, 50, 50));
            }
            usedNum = 0;
            charge = 0;
            activeBool = false;
        }
    }

    #endregion

    #region Appearance
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[2];

        sLeaser.sprites[0] = new FSprite("ParrySwordBlade", true);
        sLeaser.sprites[1] = new FSprite("ParrySwordHandle", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 posVec = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
        Vector2 horVec = new(-rotVec.y, rotVec.x);

        Vector2 newPosVec = posVec - camPos;

        float xCoordinate;
        float yCoordinate;
        float scale;

        if (faceDirection == "left")
        {
            xCoordinate = -1.5f;
            yCoordinate = 27f;
            scale = -1f;
        }
        else
        {
            xCoordinate = 1.5f;
            yCoordinate = 27f;
            scale = 1f;
        }

        Vector2 spriteVec1 = newPosVec + rotVec * yCoordinate + horVec * xCoordinate;
        sLeaser.sprites[0].x = spriteVec1.x;
        sLeaser.sprites[0].y = spriteVec1.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec);
        sLeaser.sprites[0].scaleX = scale;

        if (activeBool)
        {
            sLeaser.sprites[0].color = swordColor;
        }
        else
        {
            sLeaser.sprites[0].color = new(0.9f, 0.9f, 0.9f);
        }

        sLeaser.sprites[1].x = newPosVec.x;
        sLeaser.sprites[1].y = newPosVec.y;
        sLeaser.sprites[1].rotation = sLeaser.sprites[0].rotation;
        sLeaser.sprites[1].scaleX = scale;

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[1].color = palette.blackColor;
    }
    #endregion

    public class ParryHitbox : UpdatableAndDeletable
    {
        public Vector2 position, rotation;
        public float scaleX, scaleY, lifetime, collisionRange;
        public ParrySword sword;
        public bool entityDetected, alreadyParried;
        public Player player;
        public SlugcatStats.Name playerName;
        public FloatRect collisionRect;

        public ParryHitbox(ParrySword sword, Vector2 position, Vector2 rotation, float scaleX, float scaleY)
        {
            this.sword = sword;
            this.position = position + (rotation * 30);
            this.rotation = rotation;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            lifetime = 10f;
            collisionRange = 30f;
            entityDetected = false;
            alreadyParried = false;
            player = sword.grabbedPlayer;
            playerName = player.SlugCatClass;
            //UnityEngine.Debug.Log("");
            //UnityEngine.Debug.Log("Created Hitbox!");

            Vector2 playerPos = sword.grabbedPlayer.mainBodyChunk.pos;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            /*
            try
            {
                if (!entityDetected)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var physicalObjects = room.physicalObjects[i];
                        for (var j = 0; j < physicalObjects.Count(); j++)
                        {
                            var obj = physicalObjects[j];
                            if (obj is not ParrySword)
                            {
                                if (obj is Vulture vulture && vulture.IsKing)
                                {
                                    for (int k = 0; k < 2; k++)
                                    {
                                        KingTusks.Tusk[] tusks = vulture.kingTusks.tusks;
                                        KingTusks.Tusk tusk = vulture.kingTusks.tusks[k];
                                        Vector2 tuskPos1 = tusk.chunkPoints[0, 0];
                                        Vector2 tuskPos2 = tusk.chunkPoints[1, 0];
                                        Vector2 tuskPoint = tuskPos1 + Custom.DirVec(tuskPos2, tuskPos1) * 50;

                                        //room.AddObject(new ExplosionSpikes(room, tuskPos1, 14, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                                        //room.AddObject(new ExplosionSpikes(room, tuskPos2, 14, 1f, 2f, 7f, 5f, new(0f, 1f, 0f)));
                                        //room.AddObject(new ExplosionSpikes(room, tuskPoint, 14, 1f, 2f, 7f, 5f, new(0f, 0f, 1f)));

                                        if (vulture.kingTusks.tusks[k].mode == KingTusks.Tusk.Mode.ShootingOut && Custom.DistNoSqrt(tuskPoint, position) < collisionRange)
                                        {
                                            UnityEngine.Debug.Log("Parried object, King Vulture Harpoon");
                                            //vulture.kingTusks.tusks[k].SwitchMode(KingTusks.Tusk.Mode.Dangling);
                                            entityDetected = true;
                                        }
                                    }
                                }
                                if (obj is Weapon && obj.firstChunk.vel.magnitude > 5)
                                {
                                    if (Custom.DistNoSqrt(obj.firstChunk.pos, position) < collisionRange)
                                    {
                                        UnityEngine.Debug.Log("Parried object, " + obj.GetType() + "Velocity: " + obj.firstChunk.vel.magnitude);
                                        //(obj as Weapon).WeaponDeflect(Vector2.Lerp(position, obj.firstChunk.pos, 0.5f), rotation * obj.firstChunk.vel.magnitude * 2, 1.1f);
                                        //(obj as Weapon).ChangeMode(Weapon.Mode.Thrown);
                                        entityDetected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                UnityEngine.Debug.Log("ERROR!");
            }
            */

            collisionRect = new(position.x - collisionRange + rotation.x * 8,
                position.y - collisionRange + rotation.y * 8,
                position.x + collisionRange + rotation.x * 8,
                position.y + collisionRange + rotation.y * 8);

            /*
            room.AddObject(new ExplosionSpikes(room, position, 60, 1, 5f, 7f, 5f, new(0f, 0f, 1f)));
            room.AddObject(new ExplosionSpikes(room, collisionRect.GetCorner(FloatRect.CornerLabel.A), 60, 1, 5f, 7f, 5f, new(1f, 0f, 0f)));
            room.AddObject(new ExplosionSpikes(room, collisionRect.GetCorner(FloatRect.CornerLabel.B), 60, 1, 5f, 7f, 5f, new(0f, 1f, 0f)));
            room.AddObject(new ExplosionSpikes(room, collisionRect.GetCorner(FloatRect.CornerLabel.C), 60, 1, 5f, 7f, 5f, new(0f, 0f, 1f)));
            room.AddObject(new ExplosionSpikes(room, collisionRect.GetCorner(FloatRect.CornerLabel.D), 60, 1, 5f, 7f, 5f, new(1f, 1f, 0f)));
            */

            if (!entityDetected)
            {
                for (int i = 0; i < 3; i++)
                {
                    var physicalObjects = room.physicalObjects[i];
                    for (var j = 0; j < physicalObjects.Count(); j++)
                    {
                        var obj = physicalObjects[j];

                        if (obj is DartMaggot maggot)
                        {
                            if (maggot.mode == DartMaggot.Mode.Shot && collisionRect.Vector2Inside(maggot.firstChunk.pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, Dart Maggot");
                                maggot.mode = DartMaggot.Mode.Free;
                                maggot.firstChunk.vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is BigSpider spider)
                        {
                            if (spider.jumping && collisionRect.Vector2Inside(spider.mainBodyChunk.pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, Big Spider");
                                spider.Stun(100);
                                spider.mainBodyChunk.vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is BigNeedleWorm noodlefly)
                        {
                            if (noodlefly.swishDir != null || noodlefly.chargingAttack > 0.8f)
                            {
                                Vector2 aimDir = Custom.DirVec(noodlefly.bodyChunks[1].pos, noodlefly.bodyChunks[0].pos);
                                Vector2 fangPos = noodlefly.bodyChunks[0].pos + (noodlefly.fangLength * aimDir * 1f);
                                if (Custom.VectorRectDistance(fangPos + aimDir * 2, collisionRect) < 50f ||
                                    Custom.VectorRectDistance(fangPos, collisionRect) < 50f)
                                {
                                    //UnityEngine.Debug.Log("Parried object, Noodlefly");
                                    noodlefly.impaleChunk = null;
                                    noodlefly.swishCounter = 0;
                                    noodlefly.swishDir = null;
                                    noodlefly.Stun(100);
                                    noodlefly.bodyChunks[0].vel = rotation * 30;
                                    entityDetected = true;
                                    return;
                                }
                            }
                        }

                        if (obj is Cicada squidcada)
                        {
                            if (squidcada.Charging && collisionRect.Vector2Inside(squidcada.bodyChunks[0].pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, Squidcada");
                                squidcada.Stun(100);
                                squidcada.bodyChunks[0].vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is Lizard lizard)
                        {
                            if (lizard.AI.DynamicRelationship((sword.grabbedPlayer as Creature).abstractCreature).GoForKill &&
                                lizard.JawOpen > 0.3f &&
                                !lizard.Stunned &&
                                collisionRect.Vector2Inside((lizard.graphicsModule as LizardGraphics).head.pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, Lizard");
                                lizard.Stun(100);
                                lizard.bodyChunks[0].vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is Vulture vulture)
                        {
                            if (vulture.ChargingSnap && collisionRect.Vector2Inside(vulture.Head().pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, Vulture");
                                vulture.Stun(100);
                                vulture.Head().vel = rotation * 30;
                                entityDetected = true;
                            }
                            if (vulture.IsKing)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    KingTusks.Tusk[] tusks = vulture.kingTusks.tusks;
                                    KingTusks.Tusk tusk = vulture.kingTusks.tusks[k];
                                    Vector2 tuskPos1 = tusk.chunkPoints[0, 0];
                                    Vector2 tuskPos2 = tusk.chunkPoints[1, 0];
                                    Vector2 tuskPoint = tuskPos1 + Custom.DirVec(tuskPos2, tuskPos1) * 50;
                                    if (vulture.kingTusks.tusks[k].mode == KingTusks.Tusk.Mode.ShootingOut && collisionRect.Vector2Inside(tuskPoint))
                                    {
                                        //UnityEngine.Debug.Log("Parried object, King Vulture Harpoon");
                                        vulture.kingTusks.tusks[k].SwitchMode(KingTusks.Tusk.Mode.Dangling);
                                        entityDetected = true;
                                    }
                                }
                            }
                        }

                        if (obj is Weapon && obj is not ParrySword && obj.firstChunk.vel.magnitude > 5)
                        {
                            if (collisionRect.Vector2Inside(obj.firstChunk.pos))
                            {
                                //UnityEngine.Debug.Log("Parried object, " + obj.GetType() + "Velocity: " + obj.firstChunk.vel.magnitude);
                                var objVel = obj.firstChunk.vel / 10;
                                (obj as Weapon).Thrown(sword.grabbedPlayer, sword.grabbedPlayer.mainBodyChunk.pos,
                                    sword.grabbedPlayer.mainBodyChunk.pos - objVel, new(-(int)objVel.x, -(int)objVel.y), 1f, eu);
                                (obj as Weapon).ChangeMode(Weapon.Mode.Thrown);
                                entityDetected = true;
                            }
                        }
                    }
                }
            }


            if (entityDetected && !alreadyParried)
            {
                Activate();
            }

            if (lifetime <= 0 || entityDetected)
            {
                this.Destroy();
                //UnityEngine.Debug.Log("Hitbox Was Destroyed!");
                //UnityEngine.Debug.Log("");
            }
            lifetime--;
        }

        public void Activate()
        {
            float randomNum = UnityEngine.Random.Range(-0.1f, 0.1f);
            sword.grabbedPlayer.mainBodyChunk.vel += -rotation * 6;
            sword.usedNum++;
            room.AddObject(new Explosion.ExplosionLight(position, 100f, 1f, 5, sword.swordColor));
            room.AddObject(new ExplosionSpikes(room, position, 14, 2f, 5f, 7f, 100f, sword.swordColor));
            room.AddObject(new ShockWave(position, 100f, 0.05f, 5, false));
            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, position, 2f, 0.6f + randomNum);
            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, position, 2f, 1.5f + randomNum);
            alreadyParried = true;
        }

    }
}
