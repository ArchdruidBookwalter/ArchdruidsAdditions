using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevConsole;
using IL.MoreSlugcats;
using RWCustom;
using UnityEngine;
using static ArchdruidsAdditions.Methods.Methods;
using Random = UnityEngine.Random;

namespace ArchdruidsAdditions.Creatures;

public class CloudFish : AirBreatherCreature, IPlayerEdible
{
    public CloudFishAI AI;

    public Vector2 rotation;
    public Vector2 lastRotation;
    public WorldCoordinate lastExitUsed;
    public float dominance;
    public int bites;
    public bool eaten;
    public Color color;
    public Color IDcolor;

    public Smoke.NewVultureSmoke smoke = null;
    public bool emittingSmoke = false;
    public bool releasedSmoke = false;

    public int BitesLeft { get { return bites; } }
    public int FoodPoints { get { return 1; } }
    public bool Edible { get { return true; } }
    public bool AutomaticPickUp { get { return true; } }

    public CloudFish(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new(this, 0, default, 4f, 0.05f);

        bodyChunkConnections = [];

        gravity = 0.1f;
        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        collisionLayer = 1;

        GoThroughFloors = true;
        bites = 4;
        eaten = false;
        rotation = new(1, 0);

        CloudFishAbstractAI abstractAI = abstractCreature.abstractAI as CloudFishAbstractAI;
        dominance = abstractAI.dominance;

        color = Custom.HSL2RGB(Random.Range(0.4f, 0.6f), Mathf.Clamp(0f + dominance / 100f, 0f, 1f), 0.5f);

        IDcolor = Custom.HSL2RGB(dominance / 20f, 1f, 0.5f);

        AI = new(abstractCreature, world);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        AI.pathFinder.Reset(placeRoom);
    }
    public override void Update(bool eu)
    {
        base.Update(eu);

        GoThroughFloors = true;

        if (!dead)
        {
            ChangeCollisionLayer(1);
            releasedSmoke = false;
        }
        else
        {
            ChangeCollisionLayer(0);
        }

        if (room != null)
        {
            if (Consious)
            {
                Act();

                gravity = 0.1f;

                IntVector2 tilePos = room.GetTilePosition(firstChunk.pos);
                Room.Tile tile = room.GetTile(firstChunk.pos);
                if (shortcutDelay == 0 && tile.Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(tilePos).shortCutType != ShortcutData.Type.DeadEnd)
                {
                    enteringShortCut = tilePos;
                }

                if (firstChunk.vel.magnitude > 1f)
                {
                    rotation = Vector2.Lerp(rotation.normalized, firstChunk.vel.normalized, 0.2f);
                }

                lungs = 1f;
            }
            else
            {
                gravity = 0.9f;
                AI.behavior = CloudFishAI.Behavior.Flee;
                AI.fleeCounter = 0;

                TailSegment[] tail = (graphicsModule as CloudFishGraphics).tail;
                rotation = Custom.DirVec(tail[0].pos, firstChunk.pos);
            }

            if (grabbedBy.Count > 0 && grabbedBy[0].grabber is not Player)
            {
                foreach (RoomCamera camera in room.game.cameras)
                {
                    camera.MoveObjectToContainer(graphicsModule, camera.ReturnFContainer("Background"));
                }
            }
            else
            {
                foreach (RoomCamera camera in room.game.cameras)
                {
                    camera.MoveObjectToContainer(graphicsModule, camera.ReturnFContainer("Midground"));
                }
            }
        }

        lastRotation = rotation;
    }
    public override void Grabbed(Grasp grasp)
    {
        if (!dead)
        { room.PlaySound(SoundID.Fly_Caught, firstChunk.pos, 1f, 1.5f); }
        if (grabbedBy.Count > 0)
        {
            firstChunk.vel *= 0.1f;
        }
        base.Grabbed(grasp);
    }
    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);

        foreach (IntVector2 direction in Custom.leftRightUpDown)
        {
            if (room.aimap.getAItile(pos + direction).acc != AItile.Accessibility.Solid)
            {
                firstChunk.vel += direction.ToVector2() * 20f;
            }
        }

        AI.lastShortCutUsed = room.GetWorldCoordinate(pos);
    }
    public override void Die()
    {
        base.Die();
        foreach (CloudFishAI.TrackedObject obj in AI.trackedCreatures)
        {
            if (obj.obj is CloudFish cloudFish && Custom.DistLess(firstChunk.pos, cloudFish.firstChunk.pos, 1000))
            {
                cloudFish.AI.behavior = CloudFishAI.Behavior.Flee;
                cloudFish.AI.fleeCounter = 0;
            }
        }
    }
    public void Act()
    {
        AI.Update();
        if (AI.behavior == CloudFishAI.Behavior.Flee && Random.value > 0.9f)
        {
            room.PlaySound(SoundID.Bat_Afraid_Flying_Sounds, firstChunk.pos, 1f, 1f);
        }
    }
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule == null)
        {
            graphicsModule = new CloudFishGraphics(this);
        }
    }
    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;

        if (!dead)
        { Die(); }

        if (bites == 0) { room.PlaySound(SoundID.Slugcat_Final_Bite_Fly, firstChunk.pos); }
        else { room.PlaySound(SoundID.Slugcat_Bite_Fly, firstChunk.pos); }

        ReleaseSmoke(-rotation);

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
    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (type == DamageType.Stab)
        {
            ReleaseSmoke(-directionAndMomentum.Value.normalized);
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
    public void ReleaseSmoke(Vector2 dir)
    {
        if (!releasedSmoke)
        {
            Color startColor = Custom.HSL2RGB(0.5f, 0.5f, 0.9f);
            Color endColor = Custom.HSL2RGB(0.5f, 0.2f, 0.1f);
            releasedSmoke = true;
            room.AddObject(new GasJet(firstChunk, dir, 4f, 20, startColor, endColor));
        }
    }

    public class GasJet : UpdatableAndDeletable
    {
        new public Room room;
        public Objects.ColoredVultureSmoke smoke;
        public BodyChunk chunk;
        public Vector2 dir;
        public float power;
        public float life;
        public float maxLife;
        public Color color1;
        public Color color2;
        public GasJet(BodyChunk chunk, Vector2 dir, float power, int maxLife, Color color1, Color color2)
        {
            this.chunk = chunk;
            this.dir = dir;
            this.power = power;

            life = maxLife;
            this.maxLife = maxLife;
            room = chunk.owner.room;

            this.color1 = color1;
            this.color2 = color2;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (smoke == null)
            {
                room.PlaySound(SoundID.Vulture_Jet_Start, chunk);
                smoke = new(room, chunk.pos, null, color1, color2);
                room.AddObject(smoke);
            }

            if (life <= 0)
            {
                room.PlaySound(SoundID.Vulture_Jet_Stop, chunk);
                smoke.Destroy();
                Destroy();
            }
            else
            {
                life--;
                smoke.MoveTo(chunk.pos, eu);

                float val = life/maxLife;
                smoke.EmitSmoke(dir * 5f * val, val);
            }
        }
    }
}



public class CloudFishGraphics : GraphicsModule
{
    public CloudFish cloudFish;

    public TriangleMesh bodyMech;
    public TriangleMesh shineMesh;
    public TriangleMesh tailFinMesh1;
    public TriangleMesh tailFinMesh2;
    public TriangleMesh bodyFinMesh1;
    public TriangleMesh bodyFinMesh2;
    public FSprite eye1;
    public FSprite eye2;

    public TailSegment[] tail;

    public Vector2 bodyRotation;
    public Vector2 lastBodyRotation;

    public Vector2 tailDir0;
    public Vector2 tailDir1;
    public Vector2 tailDir2;
    public Vector2 tailDir3;
    public Vector2 tailDir4;

    public Vector2 eyesLookDir;
    public Vector2 lastEyesLookDir;

    public Color color;

    public CloudFishGraphics(PhysicalObject ow) : base(ow, false)
    {
        cloudFish = ow as CloudFish;

        List<BodyPart> list = [];

        tail = new TailSegment[5];
        
        tail[0] = new TailSegment(this, 4f, 15f, null, 0.8f, 1f, 0f, true);
        tail[1] = new TailSegment(this, 3f, 15f, tail[0], 0.8f, 1f, 0f, true);
        tail[2] = new TailSegment(this, 2f, 15f, tail[1], 0.8f, 1f, 0f, true);
        tail[3] = new TailSegment(this, 1f, 15f, tail[2], 0.8f, 1f, 0f, true);
        tail[4] = new TailSegment(this, 1f, 15f, tail[3], 0.8f, 1f, 0f, true);

        foreach (TailSegment segment in tail)
        { list.Add(segment); }

        bodyParts = list.ToArray();

        color = cloudFish.color;
    }

    public override void Update()
    {
        base.Update();

        bodyRotation = cloudFish.rotation;

        lastEyesLookDir = eyesLookDir;

        tail[0].connectedPoint = cloudFish.firstChunk.pos;

        Vector2 previousSegmentRotation = bodyRotation;
        foreach (TailSegment segment in tail)
        {
            segment.Update();
            segment.vel *= 0.5f;
            Vector2 segmentConnectPos = segment.connectedPoint ?? segment.connectedSegment.pos;
            Vector2 segmentRotation = Custom.DirVec(segment.pos, segmentConnectPos);

            if (!cloudFish.Consious || cloudFish.grabbedBy.Count > 0)
            {
                segment.vel += new Vector2(0f, -2f);
            }
            if (Custom.DistLess(segment.pos, cloudFish.firstChunk.pos, segment.rad))
            {
                segment.pos = cloudFish.firstChunk.pos - segmentRotation * segment.rad;
            }
            if (Custom.DistLess(segment.pos, segmentConnectPos, segment.connectionRad))
            {
                segment.pos = segmentConnectPos - segmentRotation * segment.connectionRad;
            }

            float angle = Custom.Angle(segmentRotation, previousSegmentRotation);
            if (Mathf.Abs(angle) > 20)
            {
                //Create_Square(cloudFish.room, segment.pos, 2f, 2f, segmentRotation, "Red", 100);
                segment.pos = segmentConnectPos + Custom.rotateVectorDeg(-previousSegmentRotation, angle > 0 ? 20 : -20) * segment.connectionRad;
            }

            previousSegmentRotation = segmentRotation;
        }

        Vector2 dirVec = Custom.DirVec(tail[0].pos, cloudFish.firstChunk.pos);

        if (!cloudFish.Consious)
        {
            eyesLookDir = new Vector2(0f, 0f);
            if (cloudFish.firstChunk.ContactPoint.ToVector2().magnitude > 0.1)
            {
                Vector2 rotation = Custom.DirVec(tail[0].pos, cloudFish.firstChunk.pos);
                tail[0].pos = cloudFish.firstChunk.pos + Custom.PerpendicularVector(cloudFish.firstChunk.ContactPoint.ToVector2()) * (rotation.x < 0 ? 15 : -15);
            }
            if (cloudFish.grabbedBy.Count > 0)
            {
                TailSegment segment = tail[1];
                Vector2 segmentRotation = Custom.DirVec(segment.connectedSegment.pos, segment.pos);
                Vector2 connectedSegmentPoint = segment.connectedSegment.connectedPoint.HasValue ? segment.connectedSegment.connectedPoint.Value : segment.connectedSegment.connectedSegment.pos;
                Vector2 connectedSegmentRotation = Custom.DirVec(connectedSegmentPoint, segment.connectedSegment.pos);
                float angle = Custom.Angle(segmentRotation, connectedSegmentRotation);
                Vector2 segmentRotation2 = Custom.DirVec(tail[0].connectedPoint.Value, tail[0].pos);
                if (angle > 30)
                {
                    tail[0].vel += Custom.PerpendicularVector(Custom.DirVec(tail[0].pos, cloudFish.grabbedBy[0].grabber.mainBodyChunk.pos));
                }
                else if (angle < -30)
                {
                    tail[0].vel -= Custom.PerpendicularVector(Custom.DirVec(tail[0].pos, cloudFish.grabbedBy[0].grabber.mainBodyChunk.pos));
                }
            }
        }
        else
        {
            if (cloudFish.AI.behavior == CloudFishAI.Behavior.Wander || cloudFish.AI.behavior == CloudFishAI.Behavior.Follow)
            {
                Vector2 pos = cloudFish.firstChunk.pos;
                Creature closestCreature = null;
                float threatOfClosestCreature = 0;
                foreach (CloudFishAI.TrackedObject obj in cloudFish.AI.trackedCreatures)
                {
                    Creature creature = obj.obj as Creature;
                    if (Custom.Dist(pos, creature.mainBodyChunk.pos) < 200f && cloudFish.room.VisualContact(pos, creature.mainBodyChunk.pos))
                    {
                        if (closestCreature == null || Custom.Dist(pos, creature.mainBodyChunk.pos) < Custom.Dist(pos, closestCreature.mainBodyChunk.pos) || obj.threat > threatOfClosestCreature)
                        {
                            if (creature is not CloudFish otherFish || (threatOfClosestCreature == 0 && otherFish.dominance > cloudFish.dominance))
                            {
                                closestCreature = creature;
                                threatOfClosestCreature = obj.threat;
                            }
                        }
                    }
                }

                if (closestCreature != null)
                {
                    eyesLookDir = Custom.DirVec(pos, closestCreature.mainBodyChunk.pos);
                }
                else
                {
                    eyesLookDir = cloudFish.rotation;
                }
            }
            else
            {
                eyesLookDir = cloudFish.rotation;
            }
        }

        lastBodyRotation = bodyRotation;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[8];

        TriangleMesh bodyMesh = TriangleMesh.MakeLongMesh(3, true, false);
        sLeaser.sprites[0] = bodyMesh;

        TriangleMesh shineMesh = TriangleMesh.MakeLongMesh(2, true, false);
        sLeaser.sprites[1] = shineMesh;

        sLeaser.sprites[2] = new FSprite("JetFishEyeA", true);
        sLeaser.sprites[3] = new FSprite("JetFishEyeB", true);

        TriangleMesh finMesh1 = TriangleMesh.MakeLongMesh(1, false, false);
        sLeaser.sprites[4] = finMesh1;

        TriangleMesh finMesh2 = TriangleMesh.MakeLongMesh(1, false, false);
        sLeaser.sprites[5] = finMesh2;

        TriangleMesh finMesh3 = TriangleMesh.MakeLongMesh(1, false, false);
        sLeaser.sprites[6] = finMesh3;

        TriangleMesh finMesh4 = TriangleMesh.MakeLongMesh(1, false, false);
        sLeaser.sprites[7] = finMesh4;

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 chunkPos = Vector2.Lerp(cloudFish.firstChunk.lastPos, cloudFish.firstChunk.pos, timeStacker);
        Vector2 rotation = Vector2.Lerp(cloudFish.lastRotation, cloudFish.rotation, timeStacker);
        Vector2 eyesLookDirection = Vector2.Lerp(lastEyesLookDir, eyesLookDir, timeStacker);

        Vector2 tail0Pos = Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker);
        Vector2 tail1Pos = Vector2.Lerp(tail[1].lastPos, tail[1].pos, timeStacker);
        Vector2 tail2Pos = Vector2.Lerp(tail[2].lastPos, tail[2].pos, timeStacker);
        Vector2 tail3Pos = Vector2.Lerp(tail[3].lastPos, tail[3].pos, timeStacker);
        Vector2 tail4Pos = Vector2.Lerp(tail[4].lastPos, tail[4].pos, timeStacker);

        Vector2 tail0Rot = Custom.DirVec(tail0Pos, chunkPos);
        Vector2 tail1Rot = Custom.DirVec(tail1Pos, tail0Pos);
        Vector2 tail2Rot = Custom.DirVec(tail2Pos, tail1Pos);
        Vector2 tail3Rot = Custom.DirVec(tail3Pos, tail2Pos);
        Vector2 tail4Rot = Custom.DirVec(tail4Pos, tail3Pos);

        #region Body
        Vector2 bodyPos0 = chunkPos - camPos + Custom.PerpendicularVector(tail0Rot) * 2f + tail0Rot * 8f;
        Vector2 bodyPos1 = chunkPos - camPos - Custom.PerpendicularVector(tail0Rot) * 2f + tail0Rot * 8f;
        Vector2 bodyPos2 = chunkPos - camPos + Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad + tail0Rot * 5f;
        Vector2 bodyPos3 = chunkPos - camPos - Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad + tail0Rot * 5f;
        Vector2 bodyPos4 = tail0Pos - camPos + Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad;
        Vector2 bodyPos5 = tail0Pos - camPos - Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad;
        Vector2 bodyPos6 = tail1Pos - camPos + Custom.PerpendicularVector(tail1Rot) * tail[1].StretchedRad;
        Vector2 bodyPos7 = tail1Pos - camPos - Custom.PerpendicularVector(tail1Rot) * tail[1].StretchedRad;
        Vector2 bodyPos8 = tail2Pos - camPos + Custom.PerpendicularVector(tail2Rot) * tail[2].StretchedRad;
        Vector2 bodyPos9 = tail2Pos - camPos - Custom.PerpendicularVector(tail2Rot) * tail[2].StretchedRad;
        Vector2 bodyPos10 = tail3Pos - camPos;

        if (cloudFish.bites == 3)
        {
            bodyPos8 = bodyPos6;
            bodyPos9 = bodyPos7;
            bodyPos10 = bodyPos7;
        }
        else if (cloudFish.bites == 2)
        {
            bodyPos6 = bodyPos4;
            bodyPos7 = bodyPos5;
            bodyPos8 = bodyPos4;
            bodyPos9 = bodyPos5;
            bodyPos10 = bodyPos5;
        }
        else if (cloudFish.bites == 1)
        {
            bodyPos4 = Vector2.Lerp(bodyPos2, bodyPos4, 0.5f);
            bodyPos5 = Vector2.Lerp(bodyPos3, bodyPos5, 0.5f);
            bodyPos6 = bodyPos4;
            bodyPos7 = bodyPos5;
            bodyPos8 = bodyPos4;
            bodyPos9 = bodyPos5;
            bodyPos10 = bodyPos5;
        }

        bodyMech = sLeaser.sprites[0] as TriangleMesh;
        bodyMech.MoveVertice(0, bodyPos0);
        bodyMech.MoveVertice(1, bodyPos1);
        bodyMech.MoveVertice(2, bodyPos2);
        bodyMech.MoveVertice(3, bodyPos3);
        bodyMech.MoveVertice(4, bodyPos4);
        bodyMech.MoveVertice(5, bodyPos5);
        bodyMech.MoveVertice(6, bodyPos6);
        bodyMech.MoveVertice(7, bodyPos7);
        bodyMech.MoveVertice(8, bodyPos8);
        bodyMech.MoveVertice(9, bodyPos9);
        bodyMech.MoveVertice(10, bodyPos10);
        bodyMech.alpha = 1f;
        #endregion

        #region Shine
        float rotXfac = tail0Rot.x;
        Vector2 middleShinePos0 = tail0Pos - camPos + Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad * rotXfac * 0.25f;
        Vector2 middleShinePos1 = tail1Pos - camPos + Custom.PerpendicularVector(tail0Rot) * tail[1].StretchedRad * rotXfac * 0.25f;

        Vector2 shinePos0 = chunkPos - camPos + Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad * 0.5f * rotXfac + tail0Rot * 6f;
        Vector2 shinePos1 = middleShinePos0 + Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad * 0.25f;
        Vector2 shinePos2 = middleShinePos0 - Custom.PerpendicularVector(tail0Rot) * tail[0].StretchedRad * 0.25f;
        Vector2 shinePos3 = middleShinePos1 + Custom.PerpendicularVector(tail1Rot) * tail[1].StretchedRad * 0.25f;
        Vector2 shinePos4 = middleShinePos1 - Custom.PerpendicularVector(tail1Rot) * tail[1].StretchedRad * 0.25f;
        Vector2 shinePos5 = tail2Pos - camPos + Custom.PerpendicularVector(tail2Rot) * tail[2].StretchedRad * 0.2f * rotXfac;

        if (cloudFish.bites == 3)
        {
            shinePos5 = shinePos4;
        }
        else if (cloudFish.bites == 2)
        {
            shinePos3 = shinePos1;
            shinePos4 = shinePos2;
            shinePos5 = shinePos2;
        }
        else if (cloudFish.bites == 1)
        {
            shinePos1 = Vector2.Lerp(shinePos1, shinePos0, 0.5f);
            shinePos2 = Vector2.Lerp(shinePos2, shinePos0, 0.5f);
            shinePos3 = shinePos1;
            shinePos4 = shinePos2;
            shinePos5 = shinePos2;
        }

        shineMesh = sLeaser.sprites[1] as TriangleMesh;
        shineMesh.MoveVertice(0, shinePos0);
        shineMesh.MoveVertice(1, shinePos1);
        shineMesh.MoveVertice(2, shinePos2);
        shineMesh.MoveVertice(3, shinePos3);
        shineMesh.MoveVertice(4, shinePos4);
        shineMesh.MoveVertice(5, shinePos5);
        shineMesh.MoveVertice(6, shinePos5);
        shineMesh.alpha = 1f;
        #endregion

        #region Eye
        eye1 = sLeaser.sprites[2];
        eye1.SetPosition(chunkPos - camPos);
        eye1.rotation = Custom.VecToDeg(tail0Rot);
        eye1.scale = 1f;
        eye1.alpha = 1f;

        eye2 = sLeaser.sprites[3];
        eye2.SetPosition(chunkPos - camPos + eyesLookDirection * 1f);
        eye2.rotation = Custom.VecToDeg(tail0Rot);
        eye2.scale = 0.5f;
        eye2.alpha = 1f;
        #endregion

        #region Fins
        Vector2 finPos = tail4Pos - camPos;

        tailFinMesh1 = sLeaser.sprites[4] as TriangleMesh;
        tailFinMesh1.MoveVertice(0, bodyPos9);
        tailFinMesh1.MoveVertice(1, bodyPos10);
        tailFinMesh1.MoveVertice(2, bodyPos10 + Custom.rotateVectorDeg(tail4Rot, 90f) * 5f);
        tailFinMesh1.MoveVertice(3, finPos + Custom.rotateVectorDeg(tail4Rot, 90f) * 5f);
        tailFinMesh1.alpha = 1f;

        tailFinMesh2 = sLeaser.sprites[5] as TriangleMesh;
        tailFinMesh2.MoveVertice(0, bodyPos8);
        tailFinMesh2.MoveVertice(1, bodyPos10);
        tailFinMesh2.MoveVertice(2, bodyPos10 + Custom.rotateVectorDeg(tail4Rot, -90f) * 5f);
        tailFinMesh2.MoveVertice(3, finPos + Custom.rotateVectorDeg(tail4Rot, -90f) * 5f);
        tailFinMesh2.alpha = 1f;

        bodyFinMesh1 = sLeaser.sprites[6] as TriangleMesh;
        bodyFinMesh1.MoveVertice(0, bodyPos3);
        bodyFinMesh1.MoveVertice(1, bodyPos5);
        bodyFinMesh1.MoveVertice(2, bodyPos5 + Custom.rotateVectorDeg(tail0Rot, 90f) * 5f);
        bodyFinMesh1.MoveVertice(3, bodyPos7 + Custom.rotateVectorDeg(tail0Rot, 90f) * 5f);
        bodyFinMesh1.alpha = 1f;

        bodyFinMesh2 = sLeaser.sprites[7] as TriangleMesh;
        bodyFinMesh2.MoveVertice(0, bodyPos2);
        bodyFinMesh2.MoveVertice(1, bodyPos4);
        bodyFinMesh2.MoveVertice(2, bodyPos4 + Custom.rotateVectorDeg(tail0Rot, -90f) * 5f);
        bodyFinMesh2.MoveVertice(3, bodyPos6 + Custom.rotateVectorDeg(tail0Rot, -90f) * 5f);
        bodyFinMesh2.alpha = 1f;
        #endregion

        if (cloudFish.bites <= 3)
        {
            tailFinMesh1.alpha = 0f;
            tailFinMesh2.alpha = 0f;
        }

        if (cloudFish.slatedForDeletetion || cloudFish.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            //cloudFish.room.AddObject(new Objects.ColoredShapes.Text(cloudFish.room, chunkPos + new Vector2(0f, 10f), randomInt.ToString(), "Red", 0));
            //cloudFish.room.AddObject(new Objects.ColoredShapes.Rectangle(cloudFish.room, eyes1.GetPosition(), 1f, 1f, 45f, "Red", 0));
            //cloudFish.room.AddObject(new Objects.ColoredShapes.Rectangle(cloudFish.room, eyes2.GetPosition(), 1f, 1f, 45f, "Red", 0));
        }
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        bodyMech = sLeaser.sprites[0] as TriangleMesh;
        shineMesh = sLeaser.sprites[1] as TriangleMesh;
        eye1 = sLeaser.sprites[2];
        eye2 = sLeaser.sprites[3];
        tailFinMesh1 = sLeaser.sprites[4] as TriangleMesh;
        tailFinMesh2 = sLeaser.sprites[5] as TriangleMesh;
        bodyFinMesh1 = sLeaser.sprites[6] as TriangleMesh;
        bodyFinMesh2 = sLeaser.sprites[7] as TriangleMesh;

        float darkness = (rCam.room.Darkness(cloudFish.mainBodyChunk.pos) * (1f - rCam.room.LightSourceExposure(cloudFish.mainBodyChunk.pos)) - 0.5f) * 2f;

        Vector3 vecBodyColor = Custom.RGB2HSL(color);
        Color bodyColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, vecBodyColor.z), palette.blackColor, darkness);
        Color shineColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, Mathf.Clamp(vecBodyColor.z + 0.2f, 0f, 1f)), palette.blackColor, darkness);
        Color finColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, Mathf.Clamp(vecBodyColor.z - 0.1f, 0f, 1f)), palette.blackColor, darkness);

        bodyMech.color = bodyColor;
        shineMesh.color = Color.Lerp(shineColor, bodyColor, darkness * 0.5f);
        tailFinMesh1.color = finColor;
        tailFinMesh2.color = finColor;
        bodyFinMesh1.color = finColor;
        bodyFinMesh2.color = finColor;

        eye1.color = palette.blackColor;
        eye2.color = new(0f, 1f, 1f);
    }
}



public class CloudFishAI : ArtificialIntelligence
{
    public CloudFish cloudfish;
    public CloudFishAbstractAI abstractAI;
    public Behavior behavior;
    public MovementType movementType;
    public List<TrackedObject> trackedCreatures;
    public List<TrackedObject> trackedObjects;
    public AbstractCreature myLeader;
    public AbstractCreature flockLeader;
    public Vector2 moveDir;
    public float speed;

    public Vector2? relativeSwarmPos = null;
    public WorldCoordinate temporaryWanderDestination;
    public WorldCoordinate? migrateDestination;
    public List<WorldCoordinate> pathToDestination;
    public float stayInRoomDesire;
    public int circleCounter;

    public WorldCoordinate? lastShortCutUsed;
    public int fleeCounter;
    public bool trapped;
    public Vector2 trappedPosition;
    public Vector2 escapeDir;

    public bool pathfinding;
    public bool followingPath;

    public Vector2 goToPoint;

    public Vector2? DangerPos
    {
        get
        {
            List<Vector2> dangerPositions = [];

            foreach (TrackedObject obj in trackedCreatures)
            {
                if (obj.obj.room == cloudfish.room)
                {
                    if (obj.obj.VisibilityBonus > -0.6)
                    {
                        Vector2 enemyPos = (obj.obj as Creature).mainBodyChunk.pos;
                        if (obj.threat > 10 && Custom.DistLess(cloudfish.firstChunk.pos, enemyPos, 200) && cloudfish.room.VisualContact(cloudfish.firstChunk.pos, enemyPos))
                        {
                            dangerPositions.Add(enemyPos);
                        }
                    }
                }
            }

            foreach (TrackedObject obj in trackedObjects)
            {
                if (obj.obj.room == cloudfish.room)
                {
                    if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(cloudfish.firstChunk.pos, weapon.firstChunk.pos, 200))
                    {
                        dangerPositions.Add(weapon.firstChunk.pos);
                    }
                }
            }

            if (dangerPositions.Count == 0)
            { return null; }

            Vector2 middleDangerPos = new (0, 0);
            foreach (Vector2 dangerPos in dangerPositions)
            {
                middleDangerPos.x += dangerPos.x;
                middleDangerPos.y += dangerPos.y;
            }
            middleDangerPos.x /= dangerPositions.Count;
            middleDangerPos.y /= dangerPositions.Count;

            return middleDangerPos;
        }
    }
    public Vector2 lastDangerPos;
    public Room lastDangerRoom;

    public int minCircleTime = 100;
    public int maxCircleTime = 500;
    public int minRoomTime = 300;
    public int maxRoomTime = 1000;
    public int minFleeTime = 100;
    public int maxFleeTime = 500;

    public CloudFishAI(AbstractCreature creature, World world) : base(creature, world)
    {
        cloudfish = creature.realizedCreature as CloudFish;
        cloudfish.AI = this;

        abstractAI = cloudfish.abstractCreature.abstractAI as CloudFishAbstractAI;
        
        AddModule(new CicadaPather(this, world, creature));
        pathFinder.stepsPerFrame = 10;
        pathFinder.accessibilityStepsPerFrame = 60;
        //pathFinder.visualizePath = true;

        AddModule(new RainTracker(this));

        speed = 1f;
        moveDir = new Vector2(1f, 0f);
        lastDangerPos = new Vector2(0, 0);

        behavior = abstractAI.behavior;
        circleCounter = Random.Range(minCircleTime, maxCircleTime);
        stayInRoomDesire = Random.Range(minRoomTime, maxRoomTime);

        movementType = MovementType.StandStill;

        trackedCreatures = [];
        trackedObjects = [];
    }
    public override void Update()
    {
        //Debug.Log("------------------------------");
        //Debug.Log("CLOUDFISH: " + cloudfish.abstractCreature.ID.number);
        try
        {
            try
            { base.Update(); }
            catch (Exception e)
            { Debug.Log("EXCEPTION OCCURED IN BASEUPDATE METHOD!"); }

            if (cloudfish != null)
            {
                if (cloudfish.room != null)
                {
                    if (Random.value < 0.2)
                    { UpdateTrackedObjects(); }

                    try
                    { UpdateBehavior(); }
                    catch (Exception e)
                    { Debug.Log("EXCEPTION OCCURED IN UPDATEBEHAVIOR METHOD!"); }

                    try
                    { UpdateMovement(); }
                    catch (Exception e)
                    { Debug.Log("EXCEPTION OCCURED IN UPDATEMOVEMENT METHOD - SECTION ONE"); }
                }
            }
        }
        catch (Exception e)
        { Debug.Log("EXCEPTION OCCURED IN UPDATE METHOD!"); }
        //Debug.Log("------------------------------");
    }
    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
    }
    public void UpdateTrackedObjects()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (PhysicalObject obj in cloudfish.room.physicalObjects[i])
            {
                if (obj != null && obj != cloudfish && cloudfish.room.VisualContact(cloudfish.firstChunk.pos, obj.firstChunk.pos))
                {
                    if (obj is Creature newCreature)
                    {
                        float threat = 10;
                        bool foundCreature = false;
                        for (int j = 0; j < trackedCreatures.Count; j++)
                        {
                            if (trackedCreatures[j].obj == newCreature)
                            {
                                foundCreature = true;
                                break;
                            }
                        }

                        if (!foundCreature)
                        {
                            CreatureTemplate.Relationship relationship = StaticRelationship(newCreature.abstractCreature);
                            if (relationship.type == CreatureTemplate.Relationship.Type.Ignores)
                            { threat = 0f; }
                            if (relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
                            { threat = 5f; }
                            else if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
                            { threat = 10f + (newCreature.Template.bodySize / 2f) + (relationship.intensity * 20f) + (newCreature.mainBodyChunk.vel.magnitude * 20f); }

                            trackedCreatures.Add(new TrackedObject(cloudfish, newCreature, 250, threat));
                        }
                    }
                    else
                    {
                        bool foundObject = false;
                        for (int j = 0; j < trackedObjects.Count; j++)
                        {
                            if (trackedObjects[j].obj == obj)
                            {
                                foundObject = true;
                                break;
                            }
                        }

                        if (!foundObject)
                        { trackedObjects.Add(new TrackedObject(cloudfish, obj, 100, 0)); }
                    }
                }
            }
        }

        for (int i = 0; i < trackedCreatures.Count - 1; i++)
        {
            Creature creature = trackedCreatures[i].obj as Creature;
            //CloudFish.room.AddObject(new Objects.ColoredShapes.SmallRectangle(CloudFish.room, creature.mainBodyChunk.pos, "Yellow", 10));
            if (cloudfish.room.VisualContact(cloudfish.firstChunk.pos, creature.mainBodyChunk.pos))
            {
                trackedCreatures[i].forgetTimer = 250;
            }
            else
            {
                trackedCreatures[i].forgetTimer--;
                if (trackedCreatures[i].forgetTimer <= 0)
                { trackedCreatures.RemoveAt(i); }
            }
        }
        for (int i = 0; i < trackedObjects.Count - 1; i++)
        {
            if (cloudfish.room.VisualContact(cloudfish.firstChunk.pos, trackedObjects[i].obj.firstChunk.pos))
            {
                trackedObjects[i].forgetTimer = 100;
            }
            else
            {
                trackedObjects[i].forgetTimer--;
                if (trackedObjects[i].forgetTimer <= 0)
                { trackedObjects.RemoveAt(i); }
            }
        }
    }
    public void UpdateBehavior()
    {
        float section = 1;
        try
        {
            Vector2 pos = cloudfish.mainBodyChunk.pos;
            AbstractCreature newLeader = abstractAI.leader;
            Behavior newBehavior = abstractAI.behavior;
            cloudfish.dominance = abstractAI.dominance;

            section = 2;
            if (Random.value < 0.5)
            {
                foreach (TrackedObject obj in trackedCreatures)
                {
                    if (obj.obj.room == cloudfish.room)
                    {
                        Vector2 enemyPos = (obj.obj as Creature).mainBodyChunk.pos;
                        if (obj.threat > 10 && Custom.DistLess(cloudfish.firstChunk.pos, enemyPos, 100 * (obj.obj.VisibilityBonus + 1)) && cloudfish.room.VisualContact(cloudfish.firstChunk.pos, enemyPos))
                        {
                            if (obj.obj.VisibilityBonus > -0.6)
                            {
                                fleeCounter = 0;
                                newBehavior = Behavior.Flee;
                                lastDangerRoom = cloudfish.room;
                            }
                        }
                        section = 31;
                        if (abstractAI.flock.count < abstractAI.flock.maxCount)
                        {
                            if (obj.obj is CloudFish otherCloudFish && newBehavior != Behavior.Flee)
                            {
                                Vector2 otherPos = otherCloudFish.mainBodyChunk.pos;
                                if (Custom.DistLess(pos, otherPos, 400) && cloudfish.room.VisualContact(pos, otherPos))
                                {
                                    CloudFishAbstractAI otherAI = otherCloudFish.AI.abstractAI;
                                    if (otherAI.dominance > cloudfish.dominance && otherAI.flock.count + abstractAI.flock.count <= otherAI.flock.maxCount)
                                    {
                                        section = 32;
                                        if (newLeader == null || ((CloudFishAbstractAI)newLeader.abstractAI).dominance > otherAI.dominance)
                                        {
                                            newLeader = otherCloudFish.abstractCreature;
                                            otherAI.flock.AddNewMember(cloudfish.abstractCreature);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //Debug.Log("");
                foreach (TrackedObject obj in trackedObjects)
                {
                    if (obj.obj.room == cloudfish.room)
                    {
                        if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(cloudfish.firstChunk.pos, weapon.firstChunk.pos, 100))
                        {
                            fleeCounter = 0;
                            newBehavior = Behavior.Flee;
                            lastDangerRoom = cloudfish.room;
                        }
                    }
                }
            }

            section = 4;
            if (newBehavior == Behavior.Flee)
            {
                if (newLeader != null)
                {
                    abstractAI.ResetFlock();
                }
                newLeader = null;

                fleeCounter++;
                if (fleeCounter > Random.Range(minFleeTime, maxFleeTime))
                {
                    newBehavior = Behavior.Wander;
                }
                if (DangerPos.HasValue && DangerPos.Value != null)
                {
                    lastDangerPos = DangerPos.Value;
                }
            }
            else
            {
                abstractAI.flock.Update();
            }

            section = 4;
            if (newBehavior == Behavior.Wander || newBehavior == Behavior.GoToDen)
            {
                if (newLeader != null)
                {
                    newBehavior = Behavior.Follow;
                }
                else if (rainTracker.rainCycle.TimeUntilRain < 2000)
                {
                    newBehavior = Behavior.GoToDen;
                }
            }

            section = 5;
            if (newBehavior == Behavior.Follow)
            {
                section = 5.1f;
                if (newLeader == null)
                {
                    Create_Text(cloudfish.room, cloudfish.firstChunk.pos + new Vector2(0, 60), "?", "Red", 0);
                }
                else if (newLeader != null)
                {
                    CloudFishAbstractAI leaderAI = newLeader.abstractAI as CloudFishAbstractAI;
                    leaderAI.flock.AddNewMember(cloudfish.abstractCreature);
                }
            }

            section = 6;

            if (abstractAI.flock != null)
            {
                Create_Text(cloudfish.room, pos + new Vector2(0, 40), abstractAI.flock.ID.ToString(), abstractAI.flock.color, 0);
                Create_Text(cloudfish.room, pos + new Vector2(0, 20), "COUNT: " + abstractAI.flock.count.ToString(), abstractAI.flock.color, 0);

                section = 7;

                if (newLeader != null && newLeader.realizedCreature != null && newLeader.realizedCreature.room == cloudfish.room)
                { Create_LineBetweenTwoPoints(cloudfish.room, pos, newLeader.realizedCreature.mainBodyChunk.pos, abstractAI.flock.color, 0); }
            }

            section = 8;

            if (newBehavior != null)
            {
                Create_Text(cloudfish.room, pos + new Vector2(0, 60), newBehavior.value, "Yellow", 0);
            }

            abstractAI.leader = newLeader;
            abstractAI.behavior = newBehavior;
            abstractAI.dominance = cloudfish.dominance;
            myLeader = newLeader;
            behavior = newBehavior;
        }
        catch (Exception e)
        {
            Debug.Log("EXCEPTION OCCURED IN UPDATEBEHAVIOR METHOD, SECTION " + section);
            Debug.Log("EXCEPTION: " + e);
        }
    }
    public int leaderMissing = 0;
    public void UpdateMovement()
    {
        int section = 0;

        try
        {
            Room room = cloudfish.room;
            AImap map = room.aimap;
            CreatureTemplate template = cloudfish.Template;
            Player player = cloudfish.room.game.RealizedPlayerOfPlayerNumber(0);
            Vector2 destination = cloudfish.room.MiddleOfTile(pathFinder.destination);
            Vector2 pos = cloudfish.firstChunk.pos;
            IntVector2 intPos = cloudfish.room.GetTilePosition(pos);
            WorldCoordinate coordPos = creature.pos;

            bool smallNarrowSpace = map.getAItile(pos).narrowSpace;
            bool largeNarrowSpace = true;

            if (!smallNarrowSpace)
            {
                int count = 0;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 testPos = pos + Custom.rotateVectorDeg(new Vector2(0f, 1f), 30 * i) * 100;
                    if (map.getTerrainProximity(testPos) > 2 && room.VisualContact(pos, testPos))
                    {
                        count++;
                        if (count > 3)
                        {
                            largeNarrowSpace = false;
                            break;
                        }
                    }
                }
            }

            pathfinding = false;

            IntVector2 intDest = pathFinder.destination.Tile;
            intDest = new(Custom.IntClamp(intDest.x, 0, room.TileWidth), Custom.IntClamp(intDest.y, 0, room.TileHeight));
            destination = cloudfish.room.MiddleOfTile(intDest);

            #region Behavior Control
            if (!pathFinder.coveredArea.Includes(intPos))
            {
                IntVector2 closestTile = new(Mathf.Clamp(intPos.x, 0, room.TileWidth - 1), Mathf.Clamp(intPos.y, 0, room.TileHeight - 1));
                IntVector2 testTile = closestTile;
                for (int i = 0; i < 20; i++)
                {
                    Vector2 tilePos = room.MiddleOfTile(testTile);
                    if (map.getAItile(testTile).acc != AItile.Accessibility.Solid && room.VisualContact(pos, tilePos))
                    {
                        goToPoint = tilePos;
                        movementType = MovementType.GoStraightToPoint;
                        break;
                    }
                    else
                    {
                        testTile = new(closestTile.x + Random.Range(-10, 11), closestTile.y + Random.Range(-10, 11));
                        testTile = new(Mathf.Clamp(testTile.x, 0, room.TileWidth - 1), Mathf.Clamp(testTile.y, 0, room.TileHeight - 1));
                    }
                }
            }
            else if (behavior == Behavior.Wander)
            {
                section = 1;
                speed = 2f;

                if (!migrateDestination.HasValue)
                {
                    if (abstractAI.destination != null && abstractAI.destination.room != cloudfish.room.abstractRoom.index)
                    {
                        migrateDestination = abstractAI.destination;
                    }
                    else
                    {
                        //section = "Wander - 1";
                        if (Custom.DistLess(pos, destination, 100) && cloudfish.room.VisualContact(pos, destination))
                        {
                            circleCounter = Random.Range(minCircleTime, maxCircleTime);

                            IntVector2 bestPos = intPos;
                            for (int i = 0; i < 10; i++)
                            {
                                IntVector2 checkTile1 = cloudfish.room.RandomTile();
                                for (int j = 0; j < 20; j++)
                                {
                                    IntVector2 checkTile2 = new(Random.Range(checkTile1.x - 20, checkTile1.x + 21), Random.Range(checkTile1.y - 20, checkTile1.y + 21));
                                    if (pathFinder.CoordinateReachable(cloudfish.room.GetWorldCoordinate(checkTile2)) &&
                                        Custom.ManhattanDistance(checkTile2, intPos) > Custom.ManhattanDistance(bestPos, intPos) &&
                                        (map.getTerrainProximity(checkTile2) > 5 || map.getTerrainProximity(checkTile2) > map.getTerrainProximity(bestPos)) &&
                                        !cloudfish.room.GetTile(checkTile1).AnyWater)
                                    {
                                        bestPos = checkTile2;
                                    }
                                }
                            }
                            if (pathFinder.CoordinateReachable(cloudfish.room.GetWorldCoordinate(bestPos)))
                            {
                                pathFinder.AssignNewDestination(cloudfish.room.GetWorldCoordinate(bestPos));
                            }

                            temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(destination);
                        }

                        stayInRoomDesire -= 1;
                        if (stayInRoomDesire <= 0)
                        {
                            //section = "Wander - 2";
                            stayInRoomDesire = Random.Range(minRoomTime, maxRoomTime);
                            World world = cloudfish.room.world;
                            AbstractRoom mostAttractiveRoom = null;
                            WorldCoordinate? accessibleNode = null;

                            for (int i = 0; i < 20; i++)
                            {
                                AbstractRoom checkRoom = world.abstractRooms[Random.Range(0, world.abstractRooms.Length)];
                                if (checkRoom.AttractionValueForCreature(template.type) > 0.25)
                                {
                                    for (int j = 0; j < 5; j++)
                                    {
                                        WorldCoordinate randomNode = new(checkRoom.index, -1, -1, checkRoom.RandomRelevantNode(template));
                                        List<WorldCoordinate> path = AbstractSpacePathFinder.Path(world, cloudfish.room.GetWorldCoordinate(pos), randomNode, template, null);
                                        if (path != null)
                                        {
                                            mostAttractiveRoom = checkRoom;
                                            accessibleNode = randomNode;
                                            goto End;
                                        }
                                    }
                                }
                            }
                        End:

                            if (mostAttractiveRoom != null)
                            {
                                migrateDestination = accessibleNode.Value;
                            }
                        }
                    }
                }
                pathfinding = true;

                PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                if (cell.generation != pathFinder.pathGeneration || circleCounter > 0)
                {
                    if (largeNarrowSpace)
                    {
                        movementType = MovementType.WanderThroughTunnel;
                        section = 2;
                    }
                    else
                    {
                        movementType = MovementType.CircleInPlace;
                        section = 3;
                    }
                }
                else if (cloudfish.room.VisualContact(pos, destination))
                {
                    goToPoint = destination;
                    movementType = MovementType.GoStraightToPoint;
                    section = 4;

                    if (cell.generation == pathFinder.pathGeneration && map.getTerrainProximity(pos) < 3)
                    {
                        movementType = MovementType.FollowPath;
                        section = 5;
                    }

                    section = 100;
                }
                else if (cell.generation == pathFinder.pathGeneration)
                {
                    movementType = MovementType.FollowPath;
                    section = 6;
                }

                //Create_Text(room, pos + new Vector2(0f, 40f), "Circle Timer: " + circleCounter, "Purple", 0);

                //Create_Text(room, pos + new Vector2(0f, 60f), "Leave Room Timer: " + stayInRoomDesire, "Red", 0);
            }
            else if (behavior == Behavior.Follow)
            {
                section = 10;
                circleCounter = 1000;

                if (movementType != MovementType.CircleInPlace)
                {
                    temporaryWanderDestination = coordPos;
                }

                if (myLeader != null)
                {
                    abstractAI.destination = myLeader.pos;
                    if (myLeader.realizedCreature != null)
                    {
                        CloudFish realizedLeader = myLeader.realizedCreature as CloudFish;
                        if (realizedLeader.room != null)
                        {
                            if (realizedLeader.room == cloudfish.room)
                            {
                                section = 5;

                                migrateDestination = null;

                                if (!relativeSwarmPos.HasValue || map.getAItile(realizedLeader.firstChunk.pos + relativeSwarmPos.Value).acc == AItile.Accessibility.Solid)
                                {
                                    for (int i = 0; i < 10; i++)
                                    {
                                        Vector2 relativeTestPos = Custom.RNV() * 20f;
                                        Vector2 actualTestPos = realizedLeader.firstChunk.pos + relativeTestPos;
                                        if (map.getAItile(actualTestPos).acc != AItile.Accessibility.Solid)
                                        {
                                            relativeSwarmPos = relativeTestPos;
                                            break;
                                        }
                                    }
                                }
                                Vector2 actualSwarmPos = realizedLeader.firstChunk.pos + relativeSwarmPos.Value;
                                if (cloudfish.room.VisualContact(pos, actualSwarmPos))
                                {
                                    //Create_LineAndDot(room, pos, realizedLeader.mainBodyChunk.pos, "Green", 0);

                                    speed = Mathf.Clamp(Custom.Dist(pos, actualSwarmPos) / 20, 1f, 2f);
                                    if (Custom.DistLess(pos, actualSwarmPos, 20))
                                    { speed = 1f; }

                                    goToPoint = actualSwarmPos;
                                    movementType = MovementType.GoStraightToPoint;

                                    if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                    {
                                        pathFinder.AssignNewDestination(realizedLeader.coord);
                                    }
                                }
                                else
                                {
                                    speed = 3f;

                                    //Create_LineAndDot(room, pos, realizedLeader.mainBodyChunk.pos, "Red", 0);

                                    if (Custom.DistLess(pos, destination, 200) && cloudfish.room.VisualContact(pos, destination))
                                    {
                                        if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                        {
                                            pathFinder.AssignNewDestination(realizedLeader.coord);
                                        }
                                    }

                                    pathfinding = true;
                                    PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                                    if (cell.generation == pathFinder.pathGeneration)
                                    {
                                        movementType = MovementType.FollowPath;
                                    }
                                    else if (map.getTerrainProximity(pos) > 2)
                                    {
                                        movementType = MovementType.CircleInPlace;
                                    }
                                    else
                                    {
                                        movementType = MovementType.WanderThroughTunnel;
                                    }
                                }
                            }
                            else
                            {
                                section = 4;

                                migrateDestination = realizedLeader.coord;

                                PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                                if (cloudfish.room.VisualContact(pos, destination))
                                {
                                    goToPoint = destination;
                                    movementType = MovementType.GoStraightToPoint;
                                    if (cell.generation == pathFinder.pathGeneration && map.getTerrainProximity(pos) < 3)
                                    {
                                        movementType = MovementType.FollowPath;
                                    }
                                }
                                else if (cell.generation == pathFinder.pathGeneration)
                                {
                                    movementType = MovementType.FollowPath;
                                }
                                else if (map.getTerrainProximity(pos) > 2)
                                {
                                    movementType = MovementType.CircleInPlace;
                                }
                                else
                                {
                                    movementType = MovementType.WanderThroughTunnel;
                                }
                            }
                        }
                        else if (realizedLeader.inShortcut)
                        {
                            section = 3;

                            if (realizedLeader.NPCTransportationDestination.room == room.abstractRoom.index)
                            {
                                if (Custom.DistLess(pos, destination, 200) && cloudfish.room.VisualContact(pos, destination))
                                {
                                    if (pathFinder.CoordinateReachable(realizedLeader.NPCTransportationDestination))
                                    {
                                        pathFinder.AssignNewDestination(realizedLeader.NPCTransportationDestination);
                                    }
                                }
                            }
                            else
                            {
                                migrateDestination = realizedLeader.NPCTransportationDestination;
                            }

                            PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                            if (cloudfish.room.VisualContact(pos, destination))
                            {
                                goToPoint = destination;
                                movementType = MovementType.GoStraightToPoint;
                                if (cell.generation == pathFinder.pathGeneration && map.getTerrainProximity(pos) < 3)
                                {
                                    movementType = MovementType.FollowPath;
                                }
                            }
                            else if (cell.generation == pathFinder.pathGeneration)
                            {
                                movementType = MovementType.FollowPath;
                            }
                            else if (map.getTerrainProximity(pos) > 2)
                            {
                                movementType = MovementType.CircleInPlace;
                            }
                            else
                            {
                                movementType = MovementType.WanderThroughTunnel;
                            }
                        }
                    }
                    else
                    {
                        section = 2;

                        migrateDestination = myLeader.pos;

                        PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                        if (cloudfish.room.VisualContact(pos, destination))
                        {
                            goToPoint = destination;
                            movementType = MovementType.GoStraightToPoint;
                            if (cell.generation == pathFinder.pathGeneration && map.getTerrainProximity(pos) < 3)
                            {
                                movementType = MovementType.FollowPath;
                            }
                        }
                        else if (cell.generation == pathFinder.pathGeneration)
                        {
                            movementType = MovementType.FollowPath;
                        }
                        else if (map.getTerrainProximity(pos) > 2)
                        {
                            movementType = MovementType.CircleInPlace;
                        }
                        else
                        {
                            movementType = MovementType.WanderThroughTunnel;
                        }
                    }
                }
            }
            else if (behavior == Behavior.Flee)
            {
                section = 20;
                stayInRoomDesire = minRoomTime;
                circleCounter = Random.Range(minCircleTime, maxCircleTime);

                speed = Mathf.Lerp(6f, 3f, Custom.Dist(pos, lastDangerPos) / 500);

                if (Custom.DistLess(pos, lastDangerPos, 200))
                {
                    if (lastShortCutUsed.HasValue)
                    {
                        //Create_Square(room, room.MiddleOfTile(lastShortCutUsed.Value), 20f, 20f, Custom.DegToVec(45), "Red", 1);
                    }

                    Vector2? closestShortcutPos = null;
                    ShortcutData? closestShortcutData = null;
                    foreach (IntVector2 shortcut in cloudfish.room.shortcutsIndex)
                    {
                        if (!lastShortCutUsed.HasValue || shortcut != lastShortCutUsed.Value.Tile)
                        {
                            ShortcutData data = cloudfish.room.shortcutData(shortcut);
                            if (data.shortCutType == ShortcutData.Type.RoomExit || data.shortCutType == ShortcutData.Type.Normal)
                            {
                                if (!lastShortCutUsed.HasValue || data.shortCutType != ShortcutData.Type.Normal || data.destinationCoord != lastShortCutUsed.Value)
                                {
                                    Vector2 shortcutPos = cloudfish.room.MiddleOfTile(shortcut);
                                    if (room.VisualContact(pos, lastDangerPos) && Mathf.Abs(Custom.Angle(Custom.DirVec(pos, lastDangerPos), Custom.DirVec(pos, shortcutPos))) < 90)
                                    {
                                        //Create_Square(room, shortcutPos, 20f, 20f, Custom.DegToVec(45), "Red", 1);
                                    }
                                    else if (!closestShortcutPos.HasValue || Custom.Dist(shortcutPos, pos) < Custom.Dist(closestShortcutPos.Value, pos))
                                    {
                                        closestShortcutPos = shortcutPos;
                                        closestShortcutData = data;
                                    }
                                }
                            }
                        }
                    }

                    if (closestShortcutPos.HasValue && closestShortcutData.HasValue)
                    {
                        //Create_Square(room, room.MiddleOfTile(closestShortcutPos.Value), 20f, 20f, Custom.DegToVec(45), "Green", 1);
                        if (Custom.DistLess(closestShortcutPos.Value, pos, 200))
                        {
                            if (room.VisualContact(pos, closestShortcutPos.Value) && map.getTerrainProximity(pos) > 1)
                            {
                                goToPoint = closestShortcutPos.Value;
                                movementType = MovementType.GoStraightToPoint;
                            }
                            else
                            {
                                if (closestShortcutData.Value.shortCutType == ShortcutData.Type.RoomExit)
                                {
                                    WorldCoordinate shortCutCoord = cloudfish.room.GetWorldCoordinate(closestShortcutPos.Value);
                                    if (pathFinder.destination != shortCutCoord)
                                    {
                                        //Create_Square(room, closestShortcutPos.Value + Custom.RNV() * 10f, 5f, 5f, Custom.DegToVec(45), "Red", 100);
                                        pathFinder.AssignNewDestination(shortCutCoord);
                                    }
                                }
                                else if (closestShortcutData.Value.shortCutType == ShortcutData.Type.Normal)
                                {
                                    WorldCoordinate destCoord = closestShortcutData.Value.destinationCoord;
                                    if (pathFinder.destination != destCoord)
                                    {
                                        //Create_Square(room, room.MiddleOfTile(destCoord) + Custom.RNV() * 10f, 5f, 5f, Custom.DegToVec(45), "Red", 100);
                                        pathFinder.AssignNewDestination(destCoord);
                                    }
                                }

                                pathfinding = true;
                                PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                                if (cell.generation == pathFinder.pathGeneration)
                                {
                                    movementType = MovementType.FollowPath;
                                }
                                else if (smallNarrowSpace)
                                {
                                    movementType = MovementType.WanderThroughTunnel;
                                }
                                else
                                {
                                    movementType = MovementType.AvoidWalls;
                                }
                            }
                        }
                        else
                        {
                            if (smallNarrowSpace)
                            {
                                movementType = MovementType.WanderThroughTunnel;
                            }
                            else
                            {
                                movementType = MovementType.AvoidWalls;
                            }
                        }
                    }
                    else
                    {
                        if (smallNarrowSpace)
                        {
                            movementType = MovementType.WanderThroughTunnel;
                        }
                        else
                        {
                            movementType = MovementType.AvoidWalls;
                        }
                    }
                }
                else
                {
                    if (smallNarrowSpace)
                    {
                        movementType = MovementType.WanderThroughTunnel;
                    }
                    else
                    {
                        movementType = MovementType.AvoidWalls;
                    }
                }
            }
            #endregion

            section = 30;

            switch (behavior.value)
            {
                case "Wander":
                    //Create_Text(room, pos + new Vector2(0f, 50f), section.ToString(), "Cyan", 0);
                    break;
                case "Flee":
                    //Create_Text(room, pos + new Vector2(0f, 50f), section.ToString(), "Red", 0);
                    break;
                case "Follow":
                    //Create_Text(room, pos + new Vector2(0f, 50f), section.ToString(), "Yellow", 0);
                    break;
                case "GoToDen":
                    //Create_Text(room, pos + new Vector2(0f, 50f), section.ToString(), "Green", 0);
                    break;
            }

            #region Migration Control
            if (migrateDestination.HasValue && behavior != Behavior.Flee && migrateDestination.Value.room != -1)
            {
                section = 31;
                pathfinding = true;

                int cloudFishRoomIndex = cloudfish.room.abstractRoom.index;
                int destRoomIndex = migrateDestination.Value.room;

                if (destRoomIndex == cloudFishRoomIndex)
                {
                    section = 32;
                    AbstractRoom migrateRoom = cloudfish.abstractCreature.Room.world.GetAbstractRoom(destRoomIndex);
                    if (migrateRoom.realizedRoom != null)
                    {
                        migrateDestination = null;
                    }
                }
                else
                {
                    section = 33;
                    abstractAI.destination = migrateDestination.Value;
                    //section = "Pathfinding 2";
                    World world = pathFinder.world;
                    AbstractRoom startRoom = cloudfish.abstractCreature.Room;
                    AbstractRoom endRoom = world.GetAbstractRoom(migrateDestination.Value.room);

                    //section = "Pathfinding 3";

                    bool resetPath = false;
                    if (pathToDestination != null)
                    {
                        if (!pathToDestination.Contains(migrateDestination.Value))
                        {
                            resetPath = true;
                        }
                        else
                        {
                            bool foundRoom = false;
                            foreach (WorldCoordinate coord in pathToDestination)
                            {
                                if (coord.room == startRoom.index)
                                {
                                    foundRoom = true;
                                    break;
                                }
                            }
                            if (!foundRoom)
                            {
                                resetPath = true;
                            }
                        }
                    }
                    section = 34;
                    if (pathToDestination == null || resetPath)
                    {
                        WorldCoordinate startCoord = QuickConnectivity.DefineNodeOfLocalCoordinate(cloudfish.abstractCreature.pos, world, cloudfish.Template);
                        WorldCoordinate destCoord = QuickConnectivity.DefineNodeOfLocalCoordinate(migrateDestination.Value, world, cloudfish.Template);
                        for (int i = 0; i < 5; i++)
                        {
                            List<WorldCoordinate> path = AbstractSpacePathFinder.Path(world, startCoord, destCoord, template, null);
                            if (path != null)
                            {
                                pathToDestination = path;
                                break;
                            }
                        }
                    }

                    //section = "Pathfinding 4";

                    section = 35;
                    if (pathToDestination != null)
                    {
                        //section = "Pathfinding 5";

                        //Debug.Log("");
                        //Debug.Log("CALCULATING PATH TO DESTINATION. LENGTH: " + pathToDestination.Count);

                        WorldCoordinate? travelNode = null;
                        foreach (WorldCoordinate node in pathToDestination)
                        {
                            if (node != null)
                            {
                                AbstractRoom roomOfNode = world.GetAbstractRoom(node.room);
                                section = 36;
                                //Debug.Log(roomOfNode.name + " - " + node.abstractNode);
                                if (roomOfNode.realizedRoom != null)
                                {
                                    section = 37;
                                    WorldCoordinate actualCoordOfNode = roomOfNode.realizedRoom.LocalCoordinateOfNode(node.abstractNode);
                                    if (roomOfNode == cloudfish.room.abstractRoom && travelNode == null)
                                    {
                                        section = 38;
                                        travelNode = actualCoordOfNode;
                                        //Create_LineAndDot(roomOfNode.realizedRoom, pos, roomOfNode.realizedRoom.MiddleOfTile(actualCoordOfNode), "Yellow", 0);
                                        if (pathFinder.destination != actualCoordOfNode)
                                        {
                                            pathFinder.AssignNewDestination(actualCoordOfNode);
                                        }
                                    }
                                    else
                                    {
                                        //Create_Dot(roomOfNode.realizedRoom, roomOfNode.realizedRoom.MiddleOfTile(actualCoordOfNode), "Red", 0);
                                    }
                                }
                            }
                        }
                        //Debug.Log("");
                    }
                }
            }
            #endregion

            section = 40;

            #region Movement Control
            if (movementType == MovementType.StandStill)
            {
                if (temporaryWanderDestination == null || Custom.Dist(pos, room.MiddleOfTile(temporaryWanderDestination)) > 200)
                {
                    temporaryWanderDestination = coordPos;
                }

                speed = 1f;
                moveDir = Vector2.Lerp(Custom.DirVec(pos, room.MiddleOfTile(temporaryWanderDestination)), moveDir, 0.99f);
            }
            else if (movementType == MovementType.FollowPath)
            {
                MovementConnection connection1 = (pathFinder as CicadaPather).FollowPath(coordPos, false);
                MovementConnection connection2 = (pathFinder as CicadaPather).FollowPath(connection1.destinationCoord, false);
                MovementConnection connection3 = (pathFinder as CicadaPather).FollowPath(connection2.destinationCoord, false);

                if (!pathFinder.coveredArea.Includes(intPos))
                {
                    Vector2? closestPos = null;
                    for (int i = 0; i < 20; i++)
                    {
                        IntVector2 testTile = new(intPos.x + Random.Range(-5, 6), intPos.y + Random.Range(-5, 6));
                        Vector2 tilePos2 = cloudfish.room.MiddleOfTile(testTile);
                        if (pathFinder.coveredArea.Includes(testTile) && cloudfish.room.VisualContact(pos, tilePos2))
                        {
                            if (!closestPos.HasValue || Custom.Dist(pos, tilePos2) < Custom.Dist(pos, closestPos.Value))
                            {
                                closestPos = tilePos2;
                            }
                        }
                    }
                    if (closestPos.HasValue)
                    {
                        WorldCoordinate newPathPos = cloudfish.room.GetWorldCoordinate(closestPos.Value);
                        connection1 = (pathFinder as CicadaPather).FollowPath(newPathPos, false);
                        connection2 = (pathFinder as CicadaPather).FollowPath(connection1.destinationCoord, false);
                        connection3 = (pathFinder as CicadaPather).FollowPath(connection2.destinationCoord, false);
                    }
                }

                Vector2 wantMoveDir;
                if (cloudfish.room.VisualContact(pos, cloudfish.room.MiddleOfTile(connection3.destinationCoord)) && !map.getAItile(pos).narrowSpace && !map.getAItile(connection1.destinationCoord).narrowSpace && !map.getAItile(connection2.destinationCoord).narrowSpace)
                {
                    wantMoveDir = Custom.DirVec(pos, cloudfish.room.MiddleOfTile(connection3.destinationCoord));
                    moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.1f);
                }
                else
                {
                    speed = 2f;
                    wantMoveDir = Custom.DirVec(pos, cloudfish.room.MiddleOfTile(connection1.destinationCoord));
                    moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.8f);
                }

                if (connection1.type == MovementConnection.MovementType.ShortCut || connection1.type == MovementConnection.MovementType.NPCTransportation)
                {
                    cloudfish.enteringShortCut = connection1.StartTile;
                    cloudfish.NPCTransportationDestination = connection1.destinationCoord;
                    cloudfish.lastExitUsed = connection1.destinationCoord;
                }

                if (connection1 != default && connection2 != default && connection3 != default)
                {
                    Vector2 pos1 = cloudfish.room.MiddleOfTile(connection1.StartTile);
                    Vector2 pos2 = cloudfish.room.MiddleOfTile(connection1.DestTile);
                    Vector2 pos3 = cloudfish.room.MiddleOfTile(connection2.DestTile);
                    Vector2 pos4 = cloudfish.room.MiddleOfTile(connection3.DestTile);
                    //Create_Square(cloudfish.room, Vector2.Lerp(pos1, pos2, 0.5f), 1f, Custom.Dist(pos1, pos2), Custom.DirVec(pos1, pos2), "White", 0);
                    //Create_Square(cloudfish.room, Vector2.Lerp(pos2, pos3, 0.5f), 1f, Custom.Dist(pos2, pos3), Custom.DirVec(pos2, pos3), "White", 0);
                    //Create_Square(cloudfish.room, Vector2.Lerp(pos3, pos4, 0.5f), 1f, Custom.Dist(pos3, pos4), Custom.DirVec(pos3, pos4), "White", 0);
                }

                //Create_LineAndDot(room, pos, destination, "Green", 0);
            }
            else if (movementType == MovementType.CircleInPlace)
            {
                if (circleCounter > 0)
                { circleCounter--; }

                if (temporaryWanderDestination == null ||
                        Custom.Dist(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) > 200 ||
                        !cloudfish.room.VisualContact(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) ||
                        map.getTerrainProximity(cloudfish.room.MiddleOfTile(temporaryWanderDestination)) < 6
                        )
                {
                    bool foundDestination = false;
                    for (int i = 0; i < 10; i++)
                    {
                        IntVector2 testPos = new(intPos.x + Random.Range(-5, 6), intPos.y + Random.Range(-5, 6));
                        if (map.getAItile(testPos).acc != AItile.Accessibility.Solid && cloudfish.room.VisualContact(pos, cloudfish.room.MiddleOfTile(testPos)) && map.getTerrainProximity(testPos) >= 6)
                        {
                            temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(testPos);
                            foundDestination = true;
                            break;
                        }
                    }
                    if (!foundDestination)
                    {
                        temporaryWanderDestination = cloudfish.coord;
                    }
                }

                if (map.getTerrainProximity(temporaryWanderDestination.Tile) < 10)
                {
                    foreach (IntVector2 dir in Custom.eightDirections)
                    {
                        IntVector2 testPos = temporaryWanderDestination.Tile + dir;
                        if (map.getTerrainProximity(testPos) > map.getTerrainProximity(temporaryWanderDestination.Tile))
                        {
                            temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(testPos);
                            break;
                        }
                    }
                }

                Vector2 rotationPos = cloudfish.room.MiddleOfTile(temporaryWanderDestination);
                if (Custom.DistLess(pos, rotationPos, 100))
                {
                    moveDir = Vector2.Lerp(moveDir, Custom.rotateVectorDeg(Custom.DirVec(pos, rotationPos), 60f), 0.8f);
                }
                else
                {
                    moveDir = Vector2.Lerp(moveDir, Custom.DirVec(pos, rotationPos), 0.8f);
                }

                AvoidChunks();

                Vector2 destPos = cloudfish.room.MiddleOfTile(temporaryWanderDestination);
                //Create_LineAndDot(cloudfish.room, pos, destPos, "Purple", 0);
            }
            else if (movementType == MovementType.AvoidWalls)
            {
                //Create_LineAndDot(room, pos, pos + moveDir * 60, "Red", 0);

                bool fleeFromEnemy = Custom.DistLess(pos, lastDangerPos, 200) && room.VisualContact(pos, lastDangerPos);
                if (fleeFromEnemy)
                {
                    lastShortCutUsed = null;

                    if (trapped)
                    {
                        if (Custom.Dist(pos, trappedPosition) < 200)
                        {
                            Vector2 testPos = pos + escapeDir * 40f;
                            if (!room.VisualContact(pos, testPos) || map.getAItile(testPos).acc == AItile.Accessibility.Solid)
                            {
                                trapped = false;
                            }
                            else
                            {
                                moveDir = escapeDir;
                            }
                        }
                        else
                        {
                            trapped = false;
                        }
                    }

                    if (!trapped)
                    {
                        Vector2 testDir = Custom.DirVec(lastDangerPos, pos);
                        Vector2 testPos = pos + testDir * 40f;
                        if (room.VisualContact(pos, testPos) && map.getAItile(testPos).acc != AItile.Accessibility.Solid)
                        {
                            moveDir = testDir;
                        }
                        else
                        {
                            trapped = true;
                            trappedPosition = pos;
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 testDir2 = Custom.rotateVectorDeg(Custom.DirVec(pos, lastDangerPos), i == 3 ? 0 : (i == 0 ? 30 : -30));
                                if (map.getAItile(pos + testDir2 * 40).acc != AItile.Accessibility.Solid && pathFinder.coveredArea.Includes(room.GetTilePosition(pos + testDir2 * 40)))
                                {
                                    escapeDir = testDir2;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? -2 : 2);
                }

                Vector2 checkPos = pos + moveDir * 40f;
                if (room.GetTilePosition(checkPos) == null || room.GetTilePosition(pos) == null || map.getTerrainProximity(checkPos) < 3 || map.getTerrainProximity(pos) < 3)
                {
                    Vector2 bestDir = moveDir;
                    Vector2 testDir = moveDir;
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 testPos = pos + testDir * 40;
                        if (room.VisualContact(pos, testPos))
                        {
                            if (map.getTerrainProximity(testPos) >= 3)
                            {
                                bestDir = testDir;
                                break;
                            }
                            else if (map.getTerrainProximity(testPos) > map.getTerrainProximity(pos + bestDir * 40))
                            {
                                bestDir = testDir;
                            }
                        }
                        testDir = Custom.rotateVectorDeg(testDir, Random.value > 0.5 ? 12 : -12);
                    }
                    moveDir = bestDir;
                }
                else
                {
                    moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? 1 : -1);
                }

                AvoidChunks();
            }
            else if (movementType == MovementType.GoStraightToPoint)
            {
                moveDir = Vector2.Lerp(moveDir, Custom.DirVec(pos, goToPoint), 0.8f);
                if (Custom.Dist(pos, goToPoint) > 50)
                {
                    moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? -1 : 1);
                }
                if (behavior != Behavior.Follow)
                {
                    //Create_LineAndDot(room, pos, goToPoint, "Green", 0);
                }

                AvoidChunks();
            }
            else if (movementType == MovementType.WanderThroughTunnel)
            {
                IntVector2 forwardsDir = new(Mathf.RoundToInt(moveDir.x), Mathf.RoundToInt(moveDir.y));
                if (Mathf.Abs(forwardsDir.x) > 0 && Mathf.Abs(forwardsDir.y) > 0)
                {
                    forwardsDir.y = 0;
                }

                IntVector2 backwardsDir = new(-forwardsDir.x, -forwardsDir.y);

                bool fleeFromEnemy = lastDangerRoom == room && Custom.DistLess(pos, lastDangerPos, 200) && room.VisualContact(pos, lastDangerPos);
                if (fleeFromEnemy)
                {
                    Vector2 dangerDir = Custom.DirVec(lastDangerPos, pos);
                    forwardsDir = new(Mathf.RoundToInt(dangerDir.x), Mathf.RoundToInt(dangerDir.y));
                }

                bool foundNewDirection = false;
                IntVector2 testDir = Random.value > 0.1f ? forwardsDir : Custom.PerpIntVec(forwardsDir) * (Random.value > 0.5f ? 1 : -1);
                for (int i = 0; i < 4; i++)
                {
                    AItile.Accessibility acc = map.getAItile(intPos + testDir).acc;
                    Room.Tile.TerrainType terrain = room.GetTile(intPos + testDir).Terrain;
                    if (testDir != backwardsDir && acc != AItile.Accessibility.Solid && terrain != Room.Tile.TerrainType.Slope)
                    {
                        if (lastShortCutUsed.HasValue)
                        {
                            Vector2 shortCutPos = room.MiddleOfTile(lastShortCutUsed.Value);
                            float angle = Mathf.Abs(Custom.Angle(testDir.ToVector2(), Custom.DirVec(pos, shortCutPos)));
                            //Debug.Log(angle);
                            if (angle < 45)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    IntVector2 testPos = intPos + testDir * (j + 1);
                                    //Create_Square(room, room.MiddleOfTile(testPos), 10f, 10f, Custom.DegToVec(0), "White", 0);
                                    if (testPos == lastShortCutUsed.Value.Tile)
                                    {
                                        //Create_Square(room, room.MiddleOfTile(intPos + testDir), 5f, 5f, Custom.DegToVec(45), "Blue", 0);
                                        goto Skip;
                                    }
                                    else if (map.getAItile(testPos).narrowSpace == false)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        foundNewDirection = true;
                        moveDir = testDir.ToVector2().normalized;
                        //Create_Square(room, room.MiddleOfTile(intPos + testDir), 5f, 5f, Custom.DegToVec(45), "Green", 0);
                        break;
                    }
                //Create_Square(room, room.MiddleOfTile(intPos + testDir), 5f, 5f, Custom.DegToVec(45), "Red", 0);
                Skip:;
                    testDir = Custom.PerpIntVec(testDir);
                }

                if (!foundNewDirection)
                {
                    moveDir = backwardsDir.ToVector2().normalized;
                }
            }
            #endregion

            if (cloudfish.firstChunk.ContactPoint.ToVector2().magnitude > 0.1f)
            {
                Vector2 contactDir = cloudfish.firstChunk.ContactPoint.ToVector2();
                Vector2 velDir = cloudfish.firstChunk.vel.normalized;
                float angle = Custom.Angle(velDir, contactDir);
                Vector2 bounceDir = Custom.rotateVectorDeg(contactDir, -angle);

                //CreateLineBetweenTwoPoints(room, pos, pos + velDir * 20, "Green", 100);
                //CreateLineBetweenTwoPoints(room, pos, pos + contactDir * 20, "Yellow", 100);
                //CreateLineBetweenTwoPoints(room, pos, pos + bounceDir * 20, "Red", 100);

                cloudfish.firstChunk.vel += velDir * 2 - contactDir * 2;
            }

            cloudfish.firstChunk.vel *= 0.8f;
            cloudfish.firstChunk.vel += moveDir * speed;
        }
        catch (Exception e)
        {
            Debug.Log("");
            Debug.Log("EXCEPTION OCCURED IN UPDATEMOVEMENT METHOD, SECTION: " + section);
            Debug.Log("EXCEPTION: " + e);
            Debug.Log("");
        }
    }
    public void AvoidChunks()
    {
        Vector2 pos = cloudfish.firstChunk.pos;
        Room room = cloudfish.room;

        Vector2? nearestChunkPos = null;
        float chunkRadius = 0;
        foreach (TrackedObject obj in trackedCreatures)
        {
            foreach (BodyChunk chunk in obj.obj.bodyChunks)
            {
                if (Custom.DistLess(pos, chunk.pos, chunk.rad + 5f))
                {
                    nearestChunkPos = chunk.pos;
                    chunkRadius = chunk.rad;
                    goto End;
                }
            }
        }
        End:;
        if (nearestChunkPos.HasValue)
        {
            Vector2 bestDir = moveDir;
            Vector2 testDir = moveDir;
            for (int i = 0; i < 30; i++)
            {
                Vector2 testPos = pos + testDir * 20;
                if (room.VisualContact(pos, testPos))
                {
                    if (Custom.Dist(testPos, nearestChunkPos.Value) > chunkRadius + 5f)
                    {
                        bestDir = testDir;
                        break;
                    }
                    else if (Custom.Dist(testPos, nearestChunkPos.Value) > Custom.Dist(pos + bestDir * 20, nearestChunkPos.Value))
                    {
                        bestDir = testDir;
                    }
                }
                testDir = Custom.rotateVectorDeg(testDir, Random.value > 0.5 ? 12 : -12);
            }
            moveDir = bestDir;
        }
    }
    public override bool WantToStayInDenUntilEndOfCycle()
    {
        if (behavior == Behavior.GoToDen || (behavior == Behavior.Follow && myLeader != null && myLeader.abstractAI.WantToStayInDenUntilEndOfCycle()))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public class Behavior : ExtEnum<Behavior>
    {
        public Behavior(string value, bool register = false) : base(value, register)
        {
        }

        public static Behavior Wander = new ("Wander", true);
        public static Behavior Flee = new ("Flee", true);
        public static Behavior Follow = new("Follow", true);
        public static Behavior GoToDen = new("GoToDen", true);
    }
    public class MovementType : ExtEnum<MovementType>
    {
        public MovementType(string value, bool register = false) : base(value, register)
        { }

        public static MovementType StandStill = new("StandStill", true);
        public static MovementType FollowPath = new("FollowPath", true);
        public static MovementType CircleInPlace = new("CircleInPlace", true);
        public static MovementType AvoidWalls = new("AvoidWalls", true);
        public static MovementType GoStraightToPoint = new("GoToPoint", true);
        public static MovementType WanderThroughTunnel = new("WanderThroughTunnel", true);
    }
    public class TrackedObject
    {
        public CloudFish cloudFish;
        public PhysicalObject obj;
        public int forgetTimer;
        public float threat;
        public TrackedObject(CloudFish cloudFish, PhysicalObject obj, int forgetTimer, float threat)
        {
            this.cloudFish = cloudFish;
            this.obj = obj;
            this.forgetTimer = forgetTimer;
            this.threat = threat;
        }
    }
}



public class CloudFishAbstractAI : AbstractCreatureAI
{
    public WorldCoordinate? migrateDestination = null;
    public AbstractCreature leader;
    public CloudFishAI.Behavior behavior;
    public int wanderTimer = 0;
    public float dominance;

    public CloudFishFlock flock;

    public CloudFishAbstractAI(World world, AbstractCreature parent) : base(world, parent)
    {
        this.world = world;
        this.parent = parent;

        behavior = CloudFishAI.Behavior.Wander;
        leader = null;
        dominance = Random.Range(0f, 20f);

        flock = new CloudFishFlock(parent);
    }

    public override void Update(int time)
    {
        int section = 1;
        try
        {
            base.Update(time);

            #region Update_Behavior;

            if (leader != null || flock.leader != parent)
            {
                behavior = CloudFishAI.Behavior.Follow;
            }
            else
            {
                behavior = CloudFishAI.Behavior.Wander;
            }

            #endregion;

            #region Update_Movement
            
            if (behavior == CloudFishAI.Behavior.Follow)
            {
                flock.Update();
                SetDestination(flock.leader.pos);
                if (parent.realizedCreature == null && flock.leader.realizedCreature == null)
                {
                    parent.Move(flock.leader.pos);
                }
            }
            else if (behavior == CloudFishAI.Behavior.Wander)
            {
                if (flock.count < flock.maxCount)
                {
                    AssembleFlock();
                }
                else
                {
                    flock.Update();
                }

                foreach (AbstractCreature creature in flock.list)
                {
                    CloudFishAbstractAI AI = creature.abstractAI as CloudFishAbstractAI;
                    if (AI.behavior == CloudFishAI.Behavior.Follow)
                    {
                        AI.flock.Update();
                        AI.SetDestination(flock.leader.pos);
                        if (creature.realizedCreature == null && flock.leader.realizedCreature == null)
                        {
                            creature.Move(flock.leader.pos);
                        }
                    }
                }
            }

            #endregion
        }
        catch (Exception e)
        {
            Debug.Log("CLOUDFISH ABSTRACT UPDATE METHOD EXPERIENCED AN ERROR AT: " + section);
        }
    }

    public void ResetFlock()
    {
        flock.RemoveMember(parent);
        flock.Update();
        flock = new CloudFishFlock(parent);
    }

    public void AssembleFlock()
    {
        if (flock.count >= flock.maxCount)
        {
            goto End;
        }

        foreach (AbstractCreature creature in parent.Room.creatures)
        {
            if (flock.count >= flock.maxCount)
            {
                goto End;
            }
            if (creature.abstractAI is CloudFishAbstractAI AI && !flock.list.Contains(creature) && AI.flock.count + flock.count < 10)
            {
                flock.AddNewMember(creature);
            }
        }
        foreach (AbstractWorldEntity entity in parent.Room.entitiesInDens)
        {
            if (entity is AbstractCreature creature)
            {
                if (flock.count >= flock.maxCount)
                {
                    goto End;
                }
                if (creature.abstractAI is CloudFishAbstractAI AI && !flock.list.Contains(creature) && AI.flock.count + flock.count < 10)
                {
                    flock.AddNewMember(creature);
                }
            }
        }
        End:;

        flock.Update();
    }

    public class CloudFishFlock
    {
        public AbstractCreature leader;
        public List<AbstractCreature> list;
        public Color color;
        public int ID;
        public int count;
        public int maxCount;
        public CloudFishFlock(AbstractCreature flockLeader)
        {
            color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            ID = Random.Range(0, 999);
            list = [];
            list.Add(flockLeader);
            count = 1;
            maxCount = Random.Range(10, 20);

            leader = flockLeader;
        }
        public void Update()
        {
            count = list.Count;

            AbstractCreature mostDominantCreature = null;
            foreach (AbstractCreature creature in list)
            {
                CloudFishAbstractAI AI = creature.abstractAI as CloudFishAbstractAI;
                AI.flock = this;
                if (mostDominantCreature == null || ((CloudFishAbstractAI)mostDominantCreature.abstractAI).dominance < AI.dominance)
                {
                    mostDominantCreature = creature;
                }
            }
            if (mostDominantCreature != null)
            {
                leader = mostDominantCreature;
            }
        }
        public void AddNewMember(AbstractCreature newMember)
        {
            if (list.Contains(newMember) || count >= maxCount)
            {
                return;
            }

            CloudFishAbstractAI AI = newMember.abstractAI as CloudFishAbstractAI;
            AI.flock.RemoveMember(newMember);

            list.Add(newMember);
            Update();
        }
        public void RemoveMember(AbstractCreature newMember)
        {
            if (!list.Contains(newMember))
            {
                return;
            }

            list.Remove(newMember);
            Update();
        }
    }
}
