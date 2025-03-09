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

namespace ArchdruidsAdditions.Objects;

public class ParrySword : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public Vector2 aimDirection;
    public float lastDirection; 
    public string faceDirection;

    public Player grabbedPlayer;

    public Color swordColor;
    public LightSource lightSource;

    public bool useBool;
    public float useTime, cooldown;

    public ParrySword(AbstractPhysicalObject abstractPhysicalObject, World world, Color color) : base(abstractPhysicalObject, world)
    {
        bodyChunks = [new BodyChunk(this, 0, default, 5f, 0.35f)];
        bodyChunkConnections = [];

        collisionLayer = 1;
        CollideWithObjects = true;
        CollideWithSlopes = true;
        CollideWithTerrain = true;
        gravity = 0.9f;

        airFriction = 0.999f;
        bounce = 0.1f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 0.2f;

        this.swordColor = color;
        this.rotation = new(0f, 1f);
        this.aimDirection = new(0f, 1f);
        this.faceDirection = "left";
        this.lastDirection = 0f;
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

        #region Behavior
        lastRotation = rotation;
        firstChunk.collideWithTerrain = grabbedBy.Count == 0;

        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player)
        {
            grabbedPlayer = grabbedBy[0].grabber as Player;
            Player.InputPackage input = grabbedPlayer.input[0];

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

            if (useBool == true)
            {
                if (aimDirection.y == 0)
                {
                    if (grasp == aimDirection.x)
                    {
                        if (grabbedPlayer.animation == Player.AnimationIndex.ZeroGSwim
                            || grabbedPlayer.animation == Player.AnimationIndex.ZeroGPoleGrab)
                        {
                            hand.pos = grabbedPlayer.mainBodyChunk.pos + new Vector2(aimDirection.x * 20, 0) * 1;
                            rotation = Custom.DegToVec(10 * grasp);
                        }
                        else
                        {
                            hand.pos = grabbedPlayer.mainBodyChunk.pos + new Vector2(-aimDirection.x * 2, 0) * 1;
                            rotation = Custom.DegToVec(100 * grasp);
                        }
                    }
                    else
                    {
                        if (grabbedPlayer.animation == Player.AnimationIndex.ZeroGSwim
                            || grabbedPlayer.animation == Player.AnimationIndex.ZeroGPoleGrab)
                        {
                            hand.pos = grabbedPlayer.mainBodyChunk.pos + new Vector2(aimDirection.x * 20, 0) * 1;
                            rotation = Custom.DegToVec(190 * grasp);
                        }
                        else
                        {
                            hand.pos = grabbedPlayer.mainBodyChunk.pos + new Vector2(-aimDirection.x * 2, 0) * 1;
                            rotation = Custom.DegToVec(-100 * grasp);
                        }
                    }
                }
                else
                {
                    if (aimDirection.y == 1)
                    {
                        hand.pos = grabbedPlayer.mainBodyChunk.pos + Custom.rotateVectorDeg(new Vector2(0, aimDirection.y * 20), 45 * grasp);
                        rotation = Custom.DegToVec(-80 * grasp);
                    }
                    else
                    {
                        hand.pos = grabbedPlayer.mainBodyChunk.pos + Custom.rotateVectorDeg(new Vector2(0, aimDirection.y * 20), 45 * grasp);
                        rotation = Custom.DegToVec(80 * grasp);
                    }
                }
            }
        }
        else
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.15f : 0.05f) * firstChunk.vel.x).normalized;
        }

        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.8f;
        }
        #endregion

        #region Visuals
        if (lightSource == null)
        {
            lightSource = new LightSource(firstChunk.pos, false, swordColor, this);
            lightSource.affectedByPaletteDarkness = 0.5f;
            room.AddObject(lightSource);
        }
        else
        {
            lightSource.setPos = new Vector2?(firstChunk.pos);
            lightSource.setRad = new float?(50f);
            lightSource.setAlpha = new float?(1f);
            if (lightSource.slatedForDeletetion || lightSource.room != room)
            {
                lightSource = null;
            }
        }
        #endregion

        if (useBool == true)
        {
            useTime++;
            if (useTime > 10)
            {
                useBool = false;
            }
        }
        if (useBool == false)
        {
            useTime = 1;
        }

        if (cooldown > 0)
        {
            cooldown--;
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        room.PlaySound(SoundID.Spear_Bounce_Off_Wall, firstChunk.pos, 0.35f, 2f);
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.chunk == null)
        {
            return false;
        }
        room.PlaySound(SoundID.Spear_Bounce_Off_Wall, firstChunk.pos, 0.35f, 2f);
        return base.HitSomething(result, eu);
    }

    public override void HitWall()
    {
        room.PlaySound(SoundID.Spear_Bounce_Off_Wall, firstChunk.pos, 0.35f, 2f);
    }

    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, firstChunk.pos, 2f, 2f);
    }

    public void Use()
    {
        if (cooldown == 0)
        {
            room.PlaySound(SoundID.Fly_Wing_Flap, firstChunk.pos, 1f, 1f);
            room.AddObject(new ParryHitbox(this, grabbedBy[0].grabber.mainBodyChunk.pos, aimDirection, 1f, 1f));
            useBool = true;
            cooldown = 10;
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

        rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite("ParrySwordBlade", true);
        sLeaser.sprites[1] = new FSprite("ParrySwordHandle", true);

        sLeaser.sprites[2] = new FSprite("Futile_White", true);
        sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];

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

        sLeaser.sprites[1].x = newPosVec.x;
        sLeaser.sprites[1].y = newPosVec.y;
        sLeaser.sprites[1].rotation = sLeaser.sprites[0].rotation;
        sLeaser.sprites[1].scaleX = scale;

        Vector2 spriteVec2 = newPosVec + rotVec * yCoordinate;
        sLeaser.sprites[2].x = spriteVec2.x;
        sLeaser.sprites[2].y = spriteVec2.y;
        sLeaser.sprites[2].scale = 10f;

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = swordColor;
        sLeaser.sprites[2].color = swordColor;
        sLeaser.sprites[2].alpha = 0.2f;
        sLeaser.sprites[1].color = palette.blackColor;
    }
    #endregion

}

public class ParryHitbox : UpdatableAndDeletable
{
    public Vector2 position, rotation;
    public float scaleX, scaleY, lifetime, collisionRange;
    public ParrySword sword;
    public bool entityDetected, alreadyParried;
    public ParryHitbox (ParrySword sword, Vector2 position, Vector2 rotation, float scaleX, float scaleY)
    {
        this.sword = sword;
        this.position = position + (rotation * 30);
        this.rotation = rotation;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        lifetime = 10f;
        collisionRange = 500f;
        entityDetected = false;
        alreadyParried = false;
        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log("Created Hitbox!");
    }
    public override void Update(bool eu)
    {
        base.Update(eu);

        //room.AddObject(new ExplosionSpikes(room, position, 60, collisionRange, 5f, 7f, 100f, new(0f, 0f, 1f)));
        if (!entityDetected)
        {
            for (int i = 0; i < 3; i++)
            {
                var physicalObjects = room.physicalObjects;
                foreach (PhysicalObject obj in physicalObjects[i])
                {
                    if (obj is not ParrySword)
                    {
                        if (obj is Vulture vulture && vulture.IsKing)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                KingTusks.Tusk[] tusks = vulture.kingTusks.tusks;
                                KingTusks.Tusk tusk = vulture.kingTusks.tusks[j];
                                Vector2 tuskPos1 = tusk.chunkPoints[0, 0];
                                Vector2 tuskPos2 = tusk.chunkPoints[1, 0];
                                Vector2 tuskPoint = tuskPos1 + Custom.DirVec(tuskPos2, tuskPos1) * 50;

                                //room.AddObject(new ExplosionSpikes(room, tuskPos1, 14, 1f, 2f, 7f, 5f, new(1f, 0f, 0f)));
                                //room.AddObject(new ExplosionSpikes(room, tuskPos2, 14, 1f, 2f, 7f, 5f, new(0f, 1f, 0f)));
                                //room.AddObject(new ExplosionSpikes(room, tuskPoint, 14, 1f, 2f, 7f, 5f, new(0f, 0f, 1f)));

                                if (vulture.kingTusks.tusks[j].mode == KingTusks.Tusk.Mode.ShootingOut && Custom.DistNoSqrt(tuskPoint, position) < collisionRange)
                                {
                                    UnityEngine.Debug.Log("Parried object, King Vulture Harpoon");
                                    vulture.kingTusks.tusks[j].SwitchMode(KingTusks.Tusk.Mode.Dangling);
                                    entityDetected = true;
                                }
                            }
                        }
                        if (Custom.DistNoSqrt(obj.firstChunk.pos, position) < collisionRange)
                        {
                            if (obj is Weapon && obj.firstChunk.vel.magnitude > 5)
                            {
                                UnityEngine.Debug.Log("Parried object, " + obj.GetType() + "Velocity: " + obj.firstChunk.vel.magnitude);
                                (obj as Weapon).WeaponDeflect(Vector2.Lerp(position, obj.firstChunk.pos, 0.5f), -obj.firstChunk.vel * 2, 1f);
                                (obj as Weapon).ChangeMode(Weapon.Mode.Thrown);
                                entityDetected = true;
                            }
                        }
                    }
                }
            }
        }

        if (entityDetected && !alreadyParried)
        {
            room.AddObject(new Explosion.ExplosionLight(position, 100f, 1f, 5, sword.swordColor));
            room.AddObject(new ExplosionSpikes(room, position, 14, 2f, 5f, 7f, 100f, sword.swordColor));
            room.AddObject(new ShockWave(position, 100f, 0.05f, 5, false));
            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, position, 1f, 2f);
            room.PlaySound(SoundID.Spear_Bounce_Off_Wall, position, 1f, 0.5f);
            alreadyParried = true;
        }

        if (lifetime <= 0 || entityDetected)
        {
            this.Destroy();
            UnityEngine.Debug.Log("Hitbox Was Destroyed!");
            UnityEngine.Debug.Log("");
        }
        lifetime--;
    }

    public void Collide(PhysicalObject otherObj, int myChunk, int otherChunk)
    {

    }
}
