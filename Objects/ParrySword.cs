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
using System.IO.Ports;
using Smoke;

namespace ArchdruidsAdditions.Objects;

public class ParrySword : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public Vector2 aimDirection;
    public float lastDirection;
    public string faceDirection;

    public Player playerHeldBy;
    int playerMaxKarma;

    public Color swordColor;
    public Color blackColor;
    public LightSource lightSource1;
    public LightSource lightSource2;

    public bool 
        useBool,
        charged, 
        spinning;
    public float
        rejectTime, 
        cooldown, 
        charge, 
        spinSpeed,
        lightPulse = 0f;
    public int 
        usedNum, 
        useTime = 1, maxUseTime = 10,
        parryNum;

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
        bounce = 0.3f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 0.2f;

        swordColor = color;
        blackColor = new(0f, 0f, 0f);
        rotation = new(0f, 1f);
        aimDirection = new(0f, 1f);
        faceDirection = "left";
        lastDirection = 0f;
        soundLoop = new ChunkDynamicSoundLoop(this.firstChunk);
        soundLoop.sound = SoundID.Vulture_Grub_Laser_LOOP;
        soundLoop.Volume = 0f;
        rejectTime = 0f;
        charge = 0f;
        usedNum = 0;
        parryNum = 0;
        charged = false;
        playerMaxKarma = 1;
        spinning = false;
        spinSpeed = 0f;
    }

    #region Behavior

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }
    Vector2 holdDir;
    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.Update();

        #region Behavior

        lastRotation = rotation;
        firstChunk.collideWithTerrain = grabbedBy.Count == 0;

        #region Grabbed By Player

        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
        {
            spinning = false;
            spinSpeed = 0f;
            playerHeldBy = grabbedBy[0].grabber as Player;
            Player.InputPackage input = playerHeldBy.input[0];

            #region Charge

            playerMaxKarma = playerHeldBy.KarmaCap + 1;
            if (charge < 1000 / playerMaxKarma)
            {
                charge++;
            }
            else if (charge == 1000 / playerMaxKarma && !charged)
            {
                for (int a = 0; a < 3; a++)
                {
                    room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 2f, swordColor, null, 50, 50));
                }
                room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, firstChunk.pos, 0.5f, 1.8f);
                room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 100f, 1f, 5, swordColor));
                room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 2f, 5f, 7f, 100f, swordColor));
                room.AddObject(new ShockWave(firstChunk.pos, 100f, 0.05f, 5, false));

                charged = true;
            }

            #endregion

            #region Object Detector

            /*
            for (int i = 0; i < 3; i++)
            {
                var physicalObjects = room.physicalObjects[i];
                for (var j = 0; j < physicalObjects.Count(); j++)
                {
                    var obj = physicalObjects[j];

                    
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
                    
                }
            }*/

            #endregion

            #region Reject Player

            if (playerHeldBy.KarmaCap < 1)
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
                room.PlaySound(SoundID.Centipede_Shock, playerHeldBy.mainBodyChunk.pos, 1f, 1f);
                room.AddObject(new Explosion(room, this, firstChunk.pos, 10, 50, 50, 0, 10, 0.1f, null, 0f, 10, 10));
                room.AddObject(new Explosion.ExplosionLight(firstChunk.pos, 100f, 1f, 5, swordColor));
                room.AddObject(new ExplosionSpikes(room, firstChunk.pos, 14, 2f, 5f, 7f, 100f, swordColor));
                room.AddObject(new ShockWave(firstChunk.pos, 100f, 0.05f, 5, false));
                playerHeldBy.Stun(200);
                room.AddObject(new CreatureSpasmer(playerHeldBy, false, playerHeldBy.stun));
                playerHeldBy.LoseAllGrasps();
                rejectTime = 0f;
            }
            #endregion

            #region Rotation

            int grasp;
            if (grabbedBy[0].graspUsed == 0) { grasp = -1; }
            else { grasp = 1; }

            if (input.x != 0 || input.y != 0)
            {
                aimDirection = Custom.DirVec(this.grabbedBy[0].grabber.mainBodyChunk.pos, this.grabbedBy[0].grabber.mainBodyChunk.pos + new Vector2(input.x, input.y));
                if (aimDirection.x != 0f)
                {
                    lastDirection = aimDirection.x;
                }
            }

            (playerHeldBy.graphicsModule as PlayerGraphics).LookAtPoint(playerHeldBy.mainBodyChunk.pos + aimDirection * 100, 10f);
            Vector2 playerUpVec = Custom.DirVec(playerHeldBy.bodyChunks[1].pos, playerHeldBy.bodyChunks[0].pos);
            SlugcatHand hand = (playerHeldBy.graphicsModule as PlayerGraphics).hands[grabbedBy[0].graspUsed];

            if (useBool == false)
            {
                useTime = 1;
                if (grasp == -1) { faceDirection = "left"; }
                if (grasp == 1) { faceDirection = "right"; }
                if (playerHeldBy.bodyMode == Player.BodyModeIndex.Crawl
                    || playerHeldBy.bodyMode == Player.BodyModeIndex.CorridorClimb
                    || playerHeldBy.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut
                    || playerHeldBy.animation == Player.AnimationIndex.ClimbOnBeam
                    || playerHeldBy.animation == Player.AnimationIndex.Flip
                    || playerHeldBy.animation == Player.AnimationIndex.Roll
                    || playerHeldBy.animation == Player.AnimationIndex.BellySlide
                    || playerHeldBy.animation == Player.AnimationIndex.ZeroGSwim
                    || playerHeldBy.animation == Player.AnimationIndex.ZeroGPoleGrab)
                {
                    rotation = playerUpVec;
                }
                else if (playerHeldBy.animation == Player.AnimationIndex.HangFromBeam)
                {
                    rotation = Custom.rotateVectorDeg(playerUpVec, 90f);
                }
                else
                {
                    rotation = Custom.rotateVectorDeg(playerUpVec, 30f * grasp);
                }
                holdDir = Custom.DirVec(playerHeldBy.mainBodyChunk.pos, firstChunk.pos);
            }

            if (useBool == true)
            {
                useTime++;
                hand.reachingForObject = true;
                hand.huntSpeed = 400f / maxUseTime;

                float maxRotation = 60f;
                if (holdDir != null && holdDir.x * aimDirection.x > 0)
                {
                    maxRotation = 100f;
                }

                if (usedNum == 0)
                {
                    if (useTime < maxUseTime * 0.4f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, -maxRotation) * 80f;
                        faceDirection = "right";
                    }
                    else if (useTime < maxUseTime * 0.6f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, -30f) * 80f;
                        faceDirection = "right";
                    }
                    else if (useTime < maxUseTime * 0.8f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, 30f) * 80f;
                        faceDirection = "right";
                    }
                    else if (useTime >= maxUseTime * 0.8f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, maxRotation) * 80f;
                        faceDirection = "right";
                    }
                }
                else
                {
                    if (useTime < maxUseTime * 0.4f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, maxRotation) * 80f;
                        faceDirection = "left";
                    }
                    else if (useTime < maxUseTime * 0.6f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, 30f) * 80f;
                        faceDirection = "left";
                    }
                    else if (useTime < maxUseTime * 0.8f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, -30f) * 80f;
                        faceDirection = "left";
                    }
                    else if (useTime >= maxUseTime * 0.8f)
                    {
                        hand.absoluteHuntPos = playerHeldBy.mainBodyChunk.pos + Custom.rotateVectorDeg(aimDirection, -maxRotation) * 80f;
                        faceDirection = "left";
                    }
                }
                if (useTime > 3)
                {
                    rotation = Custom.DirVec(playerHeldBy.mainBodyChunk.pos, hand.pos);
                }
                if (useTime == 3)
                {
                    room.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk.pos, 0.5f, 1.5f);
                }
                if (useTime > maxUseTime)
                {
                    useBool = false;
                }
            }
            
            #endregion
        }
        else
        {
            playerHeldBy = null;
            if (charge > 0)
            {
                charge--;
            }
            charged = false;
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
            else
            {
                rotation = Custom.rotateVectorDeg(rotation, spinSpeed);
                if (spinSpeed < 0)
                {
                    faceDirection = "left";
                }
                else
                {
                    faceDirection = "right";
                }
            }
        }

        #endregion

        #region Used

        if (parryNum >= 3)
        {
            ActivateLongCooldown();
        }

        #endregion

        if (cooldown > 0)
        {
            cooldown--;
        }

        #endregion

        #region Visuals

        float alpha;
        if (charged)
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
        #endregion
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        if (speed > 1)
        {
            spinning = true;
            if (spinSpeed > 0)
            { spinSpeed = -speed * 0.9f; }
            else
            { spinSpeed = speed * 0.9f; }

            if (speed > 20)
            {
                room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, firstChunk.pos, 1f, 1.2f);
                for (int i = 0; i < 3; i++)
                {
                    room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * 2f, new(1f, 1f, 1f), null, 50, 50));
                }
            }
            else
            {
                room.PlaySound(SoundID.Spear_Stick_In_Ground, firstChunk.pos, 1f, 1.2f);
            }

            if ( spinSpeed > 80f)
            { spinSpeed = 80f; }
            else if (spinSpeed < -80f)
            { spinSpeed = -80f; }
        }
        else
        {
            spinning = false;
            spinSpeed = 0f;
        }
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }

    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, firstChunk.pos, 2f, 2f);
    }

    public void Use()
    {
        if (cooldown == 0 && useTime == 1 && charged)
        {
            TailSegment[] playerTail = (playerHeldBy.graphicsModule as PlayerGraphics).tail;
            TailSegment tailEnd = playerTail[playerTail.Length - 1];
            if (Custom.DistLess(tailEnd.pos, playerHeldBy.bodyChunks[1].pos + aimDirection * 10f, 30f))
            {
                tailEnd.vel.x -= aimDirection.x * 30f;
            }
            room.AddObject(new ParryHitbox(this, playerHeldBy.mainBodyChunk.pos, aimDirection, 1f, 1f));
            useBool = true;
            cooldown = 10;
            playerHeldBy.bodyChunks[0].vel += aimDirection * 2f;
            playerHeldBy.bodyChunks[1].vel -= aimDirection * 2f;
            /*
            if (usedNum == 0)
            { room.AddObject(new ColoredRectangle(room, playerHeldBy.mainBodyChunk.pos, 20f, 20f, 45f, new(1f, 0f, 0f), 10)); }
            else
            { room.AddObject(new ColoredRectangle(room, playerHeldBy.mainBodyChunk.pos, 20f, 20f, 45f, new(0f, 0f, 1f), 10)); }
            */
            usedNum++;
            if (usedNum > 1)
            {
                usedNum = 0;
            }
        }
    }

    public void ActivateLongCooldown()
    {
        if (playerHeldBy.KarmaCap == 9)
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
            parryNum = 0;
            usedNum = 0;
            charge = 0;
            charged = false;
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
            if (sLeaser.sprites.IndexOf(fsprite) != 2)
            {
                newContainer.AddChild(fsprite);
            }
        }
        rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[2]);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[5];

        sLeaser.sprites[0] = new FSprite("ParrySwordBlade", true);
        sLeaser.sprites[1] = new FSprite("ParrySwordHandle", true);
        sLeaser.sprites[2] = new FSprite("Futile_White", true);
        sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
        sLeaser.sprites[3] = TriangleMesh.MakeLongMesh(3, true, true);
        TriangleMesh.Triangle[] array = [new TriangleMesh.Triangle(0, 1, 2)];
        sLeaser.sprites[4] = TriangleMesh.MakeLongMesh(1, false, true);

        AddToContainer(sLeaser, rCam, null);
    }
    bool firstLoad = true;
    Vector2[] lastTipPositions = new Vector2[5];
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        //Debug.Log(timeStacker);
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            #region Normal Sprites
            float scale;
            if (faceDirection == "left")
            {
                scale = -1f;
            }
            else
            {
                scale = 1f;
            }

            Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
            Vector2 handlePos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 bladePos = handlePos + rotVec * 27f + Custom.rotateVectorDeg(rotVec, 90f) * -1.5f * scale;
            Vector2 tipPos = handlePos + rotVec * 50f + Custom.rotateVectorDeg(rotVec, 90f) * -4f * scale;

            sLeaser.sprites[0].x = bladePos.x - camPos.x;
            sLeaser.sprites[0].y = bladePos.y - camPos.y;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec);
            sLeaser.sprites[0].scaleX = scale;

            sLeaser.sprites[1].x = handlePos.x - camPos.x;
            sLeaser.sprites[1].y = handlePos.y - camPos.y;
            sLeaser.sprites[1].rotation = Custom.VecToDeg(rotVec);
            sLeaser.sprites[1].scaleX = scale;

            sLeaser.sprites[2].x = bladePos.x - camPos.x;
            sLeaser.sprites[2].y = bladePos.y - camPos.y;
            sLeaser.sprites[2].color = swordColor;

            lightPulse += UnityEngine.Random.Range(-0.1f, 0.1f);
            if (lightPulse < -1)
            { lightPulse = -1; }
            else if (lightPulse > 1)
            { lightPulse = 1; }

            float alpha;
            if (charged)
            { alpha = 0.4f; }
            else
            {
                alpha = 0.4f * (charge / (1000 / playerMaxKarma));
                lightPulse = 0;
            }

            sLeaser.sprites[2].scale = 5f + lightPulse;
            sLeaser.sprites[2].alpha = alpha + lightPulse / 10f;

            float darkness = rCam.room.Darkness(bladePos) * (1f - rCam.room.LightSourceExposure(bladePos));

            if (blink > 0 && UnityEngine.Random.value < 0.5f)
            {
                sLeaser.sprites[0].color = blinkColor;
                sLeaser.sprites[1].color = blinkColor;
            }
            else if (charged)
            {
                sLeaser.sprites[0].color = swordColor;
                sLeaser.sprites[1].color = blackColor;
            }
            else
            {
                sLeaser.sprites[0].color = Color.Lerp(new(0.9f, 0.9f, 0.9f), blackColor, darkness);
                sLeaser.sprites[1].color = blackColor;
            }
            #endregion

            if (firstLoad)
            {
                for (int i = 0; i < lastTipPositions.Length; i++)
                { lastTipPositions[i] = tipPos; }
                firstLoad = false;
            }
            else
            {
                for (int i = lastTipPositions.Length - 1; i > 1; i--)
                { lastTipPositions[i] = lastTipPositions[i - 1]; }
                lastTipPositions[1] = tipPos;
                lastTipPositions[0] = tipPos + Custom.rotateVectorDeg(Custom.DirVec(handlePos, tipPos) * 50f, 120f * scale);
            }

            List<Vector3> nodes = [];
            foreach (Vector2 pos in lastTipPositions)
            {
                Vector3 newPos = new(pos.x, pos.y, 0f);
                nodes.Add(newPos);
            }

            GoSplineCatmullRomSolver splineSolver = new(nodes);

            Vector2[] positions = new Vector2[6];
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 newPos = splineSolver.getPoint(1f * (i / (float)positions.Length));
                //Debug.Log("I: " + i + " Length: " + positions.Length + " Result: " + i / (float)positions.Length);
                positions[i] = new Vector2(newPos.x, newPos.y);
            }

            TriangleMesh mesh = sLeaser.sprites[3] as TriangleMesh;
            TriangleMesh mesh2 = sLeaser.sprites[4] as TriangleMesh;

            mesh.color = sLeaser.sprites[0].color;
            mesh.MoveVertice(0, handlePos + rotVec * 10f - camPos);
            mesh.MoveVertice(1, tipPos - camPos);
            mesh.MoveVertice(2, positions[0] - camPos);
            mesh.MoveVertice(3, Vector2.Lerp(handlePos, positions[0], 0.2f) - camPos);
            mesh.MoveVertice(4, positions[1] - camPos);
            mesh.MoveVertice(5, Vector2.Lerp(handlePos, positions[1], 0.4f) - camPos);
            mesh.MoveVertice(6, positions[2] - camPos);
            mesh.MoveVertice(7, Vector2.Lerp(handlePos, positions[2], 0.6f) - camPos);
            mesh.MoveVertice(8, positions[3] - camPos);
            mesh.MoveVertice(9, Vector2.Lerp(handlePos, positions[3], 0.8f) - camPos);
            mesh.MoveVertice(10, positions[4] - camPos);

            mesh2.color = sLeaser.sprites[0].color;
            mesh2.MoveVertice(0, bladePos - camPos);
            mesh2.MoveVertice(1, handlePos + rotVec * 10f - camPos);
            mesh2.MoveVertice(2, tipPos - camPos);
            mesh2.MoveVertice(3, bladePos + Custom.rotateVectorDeg(rotVec, 90 * -scale) * 3f - camPos);

            if ((playerHeldBy != null && playerHeldBy.animation == Player.AnimationIndex.Flip) || spinSpeed > 10f || spinSpeed < -10f || (usedNum == 0 && useTime > 6) || (usedNum > 0 && useTime > 6))
            {
                mesh.isVisible = true;
                mesh2.isVisible = true;

                if (room.game.devToolsActive)
                {
                    foreach (Vector2 position in positions)
                    { room.AddObject(new ColoredShapes.Rectangle(room, position, 1.5f, 1.5f, 45f, "Red", 1)); }
                    foreach (Vector2 position in lastTipPositions)
                    { room.AddObject(new ColoredShapes.Rectangle(room, position, 1.5f, 1.5f, 45f, "Green", 1)); }
                    room.AddObject(new ColoredShapes.Rectangle(room, lastTipPositions[0], 2f, 2f, 45f, "Yellow", 1));
                }
            }
            else
            { mesh.isVisible = false; mesh2.isVisible = false; }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
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
            player = sword.playerHeldBy;
            playerName = player.SlugCatClass;

            Vector2 playerPos = sword.playerHeldBy.mainBodyChunk.pos;
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
                                maggot.mode = DartMaggot.Mode.Free;
                                maggot.firstChunk.vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is BigSpider spider)
                        {
                            if (spider.jumping && collisionRect.Vector2Inside(spider.mainBodyChunk.pos))
                            {
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
                                squidcada.Stun(100);
                                squidcada.bodyChunks[0].vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is Lizard lizard)
                        {
                            if (lizard.AI.DynamicRelationship((sword.playerHeldBy as Creature).abstractCreature).GoForKill &&
                                lizard.JawOpen > 0.3f &&
                                !lizard.Stunned &&
                                collisionRect.Vector2Inside((lizard.graphicsModule as LizardGraphics).head.pos))
                            {
                                lizard.Stun(100);
                                lizard.bodyChunks[0].vel = rotation * 30;
                                entityDetected = true;
                            }
                        }

                        if (obj is Vulture vulture)
                        {
                            if (vulture.ChargingSnap && collisionRect.Vector2Inside(vulture.Head().pos))
                            {
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
                                        vulture.kingTusks.tusks[k].SwitchMode(KingTusks.Tusk.Mode.Dangling);
                                        entityDetected = true;
                                    }
                                }
                            }
                        }

                        if (obj is Weapon && obj is not ParrySword && obj.firstChunk.vel.magnitude > 10 && obj.grabbedBy.Count == 0)
                        {
                            if (collisionRect.Vector2Inside(obj.firstChunk.pos))
                            {
                                var objVel = obj.firstChunk.vel / 10;
                                (obj as Weapon).Thrown(sword.playerHeldBy, sword.playerHeldBy.mainBodyChunk.pos,
                                    sword.playerHeldBy.mainBodyChunk.pos - objVel, new(-(int)objVel.x, -(int)objVel.y), 1f, eu);
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
                Destroy();
            }
            lifetime--;
        }

        public void Activate()
        {
            float randomNum = UnityEngine.Random.Range(-0.1f, 0.1f);
            sword.playerHeldBy.mainBodyChunk.vel += -rotation * 6;
            if (sword.playerHeldBy.animation == Player.AnimationIndex.Flip)
            { sword.playerHeldBy.mainBodyChunk.vel += new Vector2(0f, 60f); }
            sword.parryNum++;
            room.AddObject(new Explosion.ExplosionLight(position, 100f, 1f, 5, sword.swordColor));
            room.AddObject(new ExplosionSpikes(room, position, 14, 2f, 5f, 7f, 100f, sword.swordColor));
            room.AddObject(new ShockWave(position, 100f, 0.05f, 5, false));

            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, position, 2f, 0.6f + randomNum);
            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, position, 2f, 1.5f + randomNum);
            alreadyParried = true;
        }

    }
}
