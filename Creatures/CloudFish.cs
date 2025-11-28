using System;
using System.Collections.Generic;
using System.Linq;
using DevConsole;
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

                if (firstChunk.ContactPoint.ToVector2().magnitude > 0.1f)
                {
                    rotation = Vector2.Lerp(rotation.normalized, Custom.PerpendicularVector(firstChunk.ContactPoint.ToVector2()), 0.2f);
                }
                else if (grabbedBy.Count > 0 || dead)
                {
                    if (room.aimap.getTerrainProximity(firstChunk.pos) < 2)
                    {
                        rotation = Vector2.Lerp(rotation.normalized, Custom.DegToVec(60f), 0.2f);
                    }
                    else
                    {
                        rotation = Vector2.Lerp(rotation.normalized, new(0f, 1f), 0.2f);
                    }
                }
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
    public FSprite tailSphere1;

    public TailSegment[] tail;

    public Vector2 rotation;

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

        rotation = cloudFish.rotation;

        lastEyesLookDir = eyesLookDir;

        tail[0].connectedPoint = cloudFish.firstChunk.pos;

        foreach (TailSegment segment in tail)
        {
            segment.Update();
            segment.vel *= 0.5f;

            if (!cloudFish.Consious || cloudFish.grabbedBy.Count > 0)
            {
                segment.vel += new Vector2(0f, -2f);
            }
            if (Custom.DistLess(segment.pos, cloudFish.firstChunk.pos, segment.rad))
            {
                segment.pos += Custom.DirVec(segment.pos, cloudFish.firstChunk.pos);
            }

            if (segment.connectedSegment != null)
            {
                if (Custom.DistLess(segment.pos, segment.connectedSegment.pos, segment.rad))
                {
                    segment.pos += Custom.DirVec(segment.pos, cloudFish.firstChunk.pos);
                }
                Vector2 segmentRotation = Custom.DirVec(segment.connectedSegment.pos, segment.pos);
                Vector2 connectedSegmentPoint = segment.connectedSegment.connectedPoint.HasValue ? segment.connectedSegment.connectedPoint.Value : segment.connectedSegment.connectedSegment.pos;
                Vector2 connectedSegmentRotation = Custom.DirVec(connectedSegmentPoint, segment.connectedSegment.pos);
                float angle = Custom.Angle(segmentRotation, connectedSegmentRotation);
                if (angle > 30)
                {
                    Vector2 tryPos = segment.connectedSegment.pos + Custom.rotateVectorDeg(connectedSegmentRotation, Mathf.Lerp(angle, 30, 0.5f)) * 15f;
                    segment.pos = tryPos;
                }
                else if (angle < -30)
                {
                    Vector2 tryPos = segment.connectedSegment.pos + Custom.rotateVectorDeg(connectedSegmentRotation, Mathf.Lerp(angle, -30, 0.5f)) * 15f;
                    segment.pos = tryPos;
                }
            }
        }

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

    public Vector2 moveDir;
    public Vector2? relativeSwarmPos = null;
    public WorldCoordinate temporaryWanderDestination;
    public WorldCoordinate? migrateDestination;
    public List<WorldCoordinate> pathToDestination;
    public List<Vector2> avoidPositions = [];
    public WorldCoordinate? lastShortCutUsed;
    public int fleeCounter;
    public float speed;
    public float stayInRoomDesire;
    public bool circling;
    public int circleCounter;
    public int waitToCircle;

    public bool pathfinding;

    public Behavior behavior;
    public List<TrackedObject> trackedCreatures;
    public List<TrackedObject> trackedObjects;
    public AbstractCreature myLeader;
    public AbstractCreature flockLeader;

    public Vector2? DangerPos
    {
        get
        {
            List<Vector2> dangerPositions = [];

            foreach (TrackedObject obj in trackedCreatures)
            {
                if (obj.threat > 0 && Custom.DistLess(cloudfish.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 200))
                {
                    dangerPositions.Add((obj.obj as Creature).mainBodyChunk.pos);
                }
            }

            foreach (TrackedObject obj in trackedObjects)
            {
                if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(cloudfish.firstChunk.pos, weapon.firstChunk.pos, 200))
                {
                    dangerPositions.Add(weapon.firstChunk.pos);
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
    public Room dangerRoom;
    public Vector2 lastDangerPos;

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
        circling = false;
        circleCounter = Random.Range(100, 300);
        stayInRoomDesire = 5000;
        waitToCircle = 0;

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
                    if (Random.value < 0.5)
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
                        for (int j = 0; j < trackedCreatures.Count - 1; j++)
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
                            if (relationship.type == CreatureTemplate.Relationship.Type.Ignores || relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
                            { threat = 0f; }
                            else if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
                            { threat = 10f + (newCreature.Template.bodySize / 2f) + (relationship.intensity * 20f) + (newCreature.mainBodyChunk.vel.magnitude * 20f); }

                            trackedCreatures.Add(new TrackedObject(cloudfish, newCreature, 250, threat));
                        }
                    }
                    else
                    {
                        bool foundObject = false;
                        for (int j = 0; j < trackedObjects.Count - 1; j++)
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
        int section = 1;
        try
        {
            List<AbstractCreature> flock = abstractAI.flock;
            AbstractCreature flockLeader = abstractAI.flockLeader;
            AbstractCreature newLeader = abstractAI.leader;
            Behavior newBehavior = abstractAI.behavior;

            section = 2;
            foreach (TrackedObject obj in trackedCreatures)
            {
                if (obj.threat > 0 && Custom.DistLess(cloudfish.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 0.5f * obj.threat))
                {
                    fleeCounter = 0;
                    pathFinder.AssignNewDestination(cloudfish.coord);
                    newBehavior = Behavior.Flee;
                }
                if (obj.obj is CloudFish otherCloudFish)
                {
                    if (behavior != Behavior.Flee && otherCloudFish.Consious && otherCloudFish.dominance > cloudfish.dominance)
                    {
                        if (newLeader == null || (newLeader.abstractAI as CloudFishAbstractAI).dominance > otherCloudFish.dominance)
                        {
                            newLeader = otherCloudFish.abstractCreature;
                            abstractAI.leader = newLeader;
                        }
                    }
                }
            }
            foreach (TrackedObject obj in trackedObjects)
            {
                if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(cloudfish.firstChunk.pos, weapon.firstChunk.pos, 50))
                {
                    fleeCounter = 0;
                    pathFinder.AssignNewDestination(cloudfish.coord);
                    newBehavior = Behavior.Flee;
                }
            }

            section = 3;
            if (newBehavior == Behavior.Flee)
            {
                newLeader = null;
                fleeCounter++;
                if (fleeCounter > 100)
                {
                    newBehavior = Behavior.Wander;
                }
                if (DangerPos.HasValue && DangerPos.Value != null)
                {
                    lastDangerPos = DangerPos.Value;
                }
            }

            section = 4;
            if (newBehavior == Behavior.Wander || newBehavior == Behavior.GoToDen)
            {
                foreach (TrackedObject creature in trackedCreatures)
                {
                    if (creature.obj is CloudFish fish && fish.AI.flockLeader == cloudfish.abstractCreature)
                    {
                        if (!flock.Contains(fish.abstractCreature))
                        {
                            flock.Add(fish.abstractCreature);
                        }
                    }
                }
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
                AbstractCreature checkFish = newLeader;
                for (int i = 0; i < 20; i++)
                {
                    if (checkFish.realizedCreature != null)
                    {
                        CloudFishAI checkAI = (checkFish.realizedCreature as CloudFish).AI;
                        if (checkAI.myLeader == null)
                        {
                            flockLeader = checkFish;
                            break;
                        }
                        else if (checkAI.flockLeader != null)
                        {
                            flockLeader = checkAI.flockLeader;
                            break;
                        }
                        else if (checkAI.myLeader != null)
                        {
                            checkFish = checkAI.myLeader;
                        }
                    }
                }

                if (newLeader != null)
                {
                    CloudFishAbstractAI abstractAI = newLeader.abstractAI as CloudFishAbstractAI;
                    if (abstractAI.dominance <= this.abstractAI.dominance)
                    {
                        newLeader = null;
                        newBehavior = Behavior.Wander;
                    }
                    if (newLeader.realizedCreature != null)
                    {
                        CloudFish realizedLeader = newLeader.realizedCreature as CloudFish;
                        if (!realizedLeader.Consious || realizedLeader.grabbedBy.Count > 0)
                        {
                            fleeCounter = 0;
                            pathFinder.AssignNewDestination(cloudfish.coord);
                            newBehavior = Behavior.Flee;
                        }
                    }
                }
                else
                {
                    newBehavior = Behavior.Wander;
                }
            }

            section = 6;

            abstractAI.flock = flock;
            abstractAI.flockLeader = flockLeader;
            abstractAI.leader = newLeader;
            abstractAI.behavior = newBehavior;
            abstractAI.dominance = cloudfish.dominance;
            myLeader = newLeader;
            behavior = newBehavior;

            Create_Text(cloudfish.room, cloudfish.firstChunk.pos + new Vector2(0f, 0f), behavior.ToString(), "Yellow", 0);
        }
        catch (Exception e)
        {
            //Debug.Log("EXCEPTION OCCURED IN UPDATEBEHAVIOR METHOD, SECTION " + section);
        }
    }
    public int leaderMissing = 0;
    public void UpdateMovement()
    {
        AImap map = cloudfish.room.aimap;
        CreatureTemplate template = cloudfish.Template;
        Player player = cloudfish.room.game.RealizedPlayerOfPlayerNumber(0);
        Vector2 destination = cloudfish.room.MiddleOfTile(pathFinder.destination);
        Vector2 pos = cloudfish.firstChunk.pos;
        IntVector2 intPos = cloudfish.room.GetTilePosition(pos);
        WorldCoordinate coordPos = creature.pos;

        pathfinding = false;

        string section = "Beginning";
        try
        {
            IntVector2 destTile = cloudfish.room.GetTilePosition(destination);
            if ((!pathFinder.coveredArea.Includes(destTile) || map.getTerrainProximity(destination) <= 0) && behavior != Behavior.GoToDen)
            {
                Vector2? closestPos = null;
                for (int i = 0; i < 10; i++)
                {
                    IntVector2 testTile = new(destTile.x + Random.Range(-10, 11), destTile.y + Random.Range(-10, 11));
                    Vector2 tilePos2 = cloudfish.room.MiddleOfTile(testTile);
                    if (pathFinder.coveredArea.Includes(testTile) && cloudfish.room.VisualContact(pos, tilePos2))
                    {
                        if (!closestPos.HasValue || Custom.Dist(pos, tilePos2) < Custom.Dist(pos, closestPos.Value))
                        {
                            closestPos = tilePos2;
                        }
                    }
                }
                if (closestPos.HasValue && pathFinder.CoordinateReachable(cloudfish.room.GetWorldCoordinate(closestPos.Value)))
                {
                    pathFinder.AssignNewDestination(cloudfish.room.GetWorldCoordinate(closestPos.Value));
                }
                else
                {
                    pathFinder.AssignNewDestination(coordPos);
                }
            }

            if (cloudfish.room.GetTile(pos).Solid)
            {
                for (int j = 0; j < 20; j++)
                {
                    IntVector2 checkTile2 = new(Random.Range(intPos.x - 5, intPos.x + 6), Random.Range(intPos.y - 5, intPos.y + 6));
                    if (!cloudfish.room.GetTile(checkTile2).Solid)
                    {
                        speed = 10f;
                        moveDir = Custom.DirVec(pos, cloudfish.room.MiddleOfTile(checkTile2));
                    }
                }
            }
            else if (behavior == Behavior.Wander)
            {
                section = "Wander - 0";
                speed = 2f;

                Create_Text(cloudfish.room, cloudfish.firstChunk.pos + new Vector2(0f, -20f), "Stay In Room Desire: - " + stayInRoomDesire.ToString(), "Green", 0);
                if (!migrateDestination.HasValue)
                {
                    if (abstractAI.destination != null && abstractAI.destination.room != cloudfish.room.abstractRoom.index)
                    {
                        migrateDestination = abstractAI.destination;
                    }
                    else
                    {
                        section = "Wander - 1";
                        if (Custom.DistLess(pos, destination, 100) && cloudfish.room.VisualContact(pos, destination))
                        {
                            circleCounter = Random.Range(800, 1000);
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
                        }

                        stayInRoomDesire -= 1;
                        if (stayInRoomDesire <= 0)
                        {
                            section = "Wander - 2";
                            stayInRoomDesire = 5000;
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
                /*
                else if (migrateDestination.Value.room == cloudfish.room.abstractRoom.index)
                {
                    section = "Wander - 3";
                    Vector2 migratePos = cloudfish.room.MiddleOfTile(migrateDestination.Value);
                    if (!pathFinder.CoordinateReachable(migrateDestination.Value) || map.getAItile(migrateDestination.Value).acc == AItile.Accessibility.Solid)
                    {
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
                        WorldCoordinate newMigratePos = cloudfish.room.GetWorldCoordinate(bestPos);
                        migrateDestination = newMigratePos;
                    }
                    else if (Custom.DistLess(migratePos, pos, 100))
                    {
                        migrateDestination = null;
                    }
                }*/

                pathfinding = true;
            }
            else if (behavior == Behavior.Follow)
            {
                section = "Follow - 1";
                circleCounter = 1000;
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
                                    speed = Mathf.Clamp(Custom.Dist(pos, actualSwarmPos) / 20, 1f, 2f);
                                    if (Custom.DistLess(pos, actualSwarmPos, 20))
                                    { speed = 1f; }

                                    moveDir = Custom.DirVec(pos, actualSwarmPos);

                                    if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                    {
                                        pathFinder.AssignNewDestination(realizedLeader.coord);
                                    }
                                }
                                else
                                {
                                    speed = 3f;
                                    if (Custom.DistLess(pos, destination, 100) && cloudfish.room.VisualContact(pos, destination))
                                    {
                                        if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                        {
                                            pathFinder.AssignNewDestination(realizedLeader.coord);
                                        }
                                    }
                                    pathfinding = true;
                                }
                            }
                            else
                            {
                                migrateDestination = realizedLeader.coord;
                            }
                        }
                    }
                    else
                    {
                        migrateDestination = myLeader.pos;
                    }
                }
            }
            else if (behavior == Behavior.Flee)
            {
                stayInRoomDesire = 100;
                circleCounter = 1000;

                speed = Mathf.Clamp(20 / (Custom.Dist(pos, lastDangerPos) / 30), 3f, 5f);

                if (Custom.DistLess(pos, lastDangerPos, 200))
                {
                    lastShortCutUsed = null;
                }

                bool foundShortcut = false;
                foreach (IntVector2 shortcut in cloudfish.room.shortcutsIndex)
                {
                    ShortcutData data = cloudfish.room.shortcutData(shortcut);
                    Vector2 shortcutPos = cloudfish.room.MiddleOfTile(shortcut);
                    if (Custom.DistLess(pos, shortcutPos, 200) &&
                        (data.shortCutType == ShortcutData.Type.Normal || data.shortCutType == ShortcutData.Type.NPCTransportation || data.shortCutType == ShortcutData.Type.RoomExit) &&
                        (!lastShortCutUsed.HasValue || data.startCoord != lastShortCutUsed.Value))
                    {
                        foundShortcut = true;
                        if (data.shortCutType == ShortcutData.Type.Normal || data.shortCutType == ShortcutData.Type.RoomExit)
                        {
                            if (destination != shortcutPos)
                            {
                                WorldCoordinate shortcutCoord = cloudfish.room.GetWorldCoordinate(shortcutPos);
                                if (pathFinder.CoordinateReachable(shortcutCoord))
                                {
                                    pathFinder.AssignNewDestination(shortcutCoord);
                                }
                            }
                        }
                        pathfinding = true;
                    }
                }
                if (!foundShortcut)
                {
                    if (map.getAItile(pos).narrowSpace)
                    {
                        moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? 1 : -1);
                        Vector2 testPos1 = pos + moveDir * 20f;
                        if (map.getAItile(testPos1).acc == AItile.Accessibility.Solid)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                IntVector2 randomDir = Custom.fourDirections[Random.Range(0, 4)];
                                Vector2 testDir = randomDir.ToVector2().normalized * 10f;
                                Vector2 reverseDir = -moveDir;
                                Vector2 testPos = pos + testDir;
                                if (map.getAItile(testPos).acc != AItile.Accessibility.Solid && Mathf.Abs(Custom.Angle(testDir, reverseDir)) > 45)
                                {
                                    moveDir = testDir.normalized;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? 10 : -10);
                        if (Custom.DistLess(pos, lastDangerPos, 100))
                        {
                            moveDir = Custom.DirVec(lastDangerPos, pos);
                        }
                        bool foundNewDirection = false;
                        for (int i = 0; i < 10; i++)
                        {
                            Vector2 testPos = pos + moveDir * 40;
                            if (map.getTerrainProximity(testPos) > 2)
                            {
                                foundNewDirection = true;
                                break;
                            }
                            else
                            {
                                moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? 30 : -30);
                            }
                        }
                        if (!foundNewDirection)
                        {
                            Vector2 bestDir = moveDir;
                            Vector2 currentDir = moveDir;
                            for (int i = 0; i < 10; i++)
                            {
                                currentDir = Custom.rotateVectorDeg(currentDir, Random.value > 0.5 ? 30 : -30);
                                if (map.getTerrainProximity(pos + currentDir * 40) > map.getTerrainProximity(pos + bestDir * 40))
                                {
                                    bestDir = currentDir;
                                }
                            }
                            moveDir = bestDir;
                        }
                    }
                }
            }
            else if (behavior == Behavior.GoToDen)
            {
                circleCounter = 1000;

                if (Custom.DistLess(lastDangerPos, pos, 200))
                {
                    speed = Mathf.Clamp(20 / (Custom.Dist(pos, lastDangerPos) / 30), 3f, 5f);
                }
                else if (rainTracker.rainCycle.TimeUntilRain < 2000)
                {
                    int timeLeft = rainTracker.rainCycle.TimeUntilRain;
                    float panic = 2f / Mathf.Clamp(timeLeft / 100f, 1f, 100f);
                    Create_Text(cloudfish.room, pos, "Panic: " + panic, "Red", 0);
                    speed = 2f + panic;
                }
                else
                {
                    speed = 2f;
                }

                AbstractCreatureAI abstractAI = cloudfish.abstractCreature.abstractAI;
                if (abstractAI.denPosition.HasValue)
                {
                    abstractAI.GoToDen();
                    migrateDestination = abstractAI.denPosition.Value;
                }
                else
                {
                    AbstractRoom checkRoom = cloudfish.room.abstractRoom;
                    WorldCoordinate? closestDenPos = null;
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < checkRoom.nodes.Count(); j++)
                        {
                            if (checkRoom.nodes[j].type == AbstractRoomNode.Type.Den)
                            {
                                if (checkRoom == cloudfish.room.abstractRoom)
                                {
                                    WorldCoordinate newDenCoord = cloudfish.room.LocalCoordinateOfNode(j);
                                    Vector2 denPos = cloudfish.room.MiddleOfTile(newDenCoord);
                                    if (!closestDenPos.HasValue || Custom.Dist(cloudfish.room.MiddleOfTile(closestDenPos.Value), pos) > Custom.Dist(denPos, pos))
                                    {
                                        closestDenPos = newDenCoord;
                                    }
                                }
                                else
                                {
                                    WorldCoordinate newDenCoord = new(checkRoom.index, -1, -1, j);
                                    if (checkRoom.realizedRoom != null)
                                    {
                                        newDenCoord = checkRoom.realizedRoom.LocalCoordinateOfNode(j);
                                    }
                                    closestDenPos = newDenCoord;
                                    break;
                                }
                            }
                        }
                        if (closestDenPos.HasValue)
                        {
                            break;
                        }
                        else
                        {
                            int randomRoomIndex = checkRoom.connections[Random.Range(0, checkRoom.connections.Length)];
                            AbstractRoom randomConnectedRoom = cloudfish.room.world.GetAbstractRoom(randomRoomIndex);
                            checkRoom = randomConnectedRoom;
                        }
                    }
                    if (closestDenPos.HasValue)
                    {
                        Create_Text(cloudfish.room, pos + new Vector2(0f, 30f), "Set Den Position: " + closestDenPos.Value, "Red", 100);
                        abstractAI.denPosition = closestDenPos.Value;
                    }
                }

                pathfinding = true;
            }

            if (migrateDestination.HasValue && behavior != Behavior.Flee && migrateDestination.Value.room != -1)
            {
                section = "Pathfinding 1";
                pathfinding = true;

                int cloudFishRoomIndex = cloudfish.room.abstractRoom.index;
                int destRoomIndex = migrateDestination.Value.room;

                if (destRoomIndex == cloudFishRoomIndex)
                {
                    AbstractRoom migrateRoom = cloudfish.abstractCreature.Room.world.GetAbstractRoom(destRoomIndex);
                    if (migrateRoom.realizedRoom != null)
                    {
                        migrateDestination = null;
                    }
                }
                else
                {
                    abstractAI.destination = migrateDestination.Value;
                    section = "Pathfinding 2";
                    World world = pathFinder.world;
                    AbstractRoom startRoom = cloudfish.abstractCreature.Room;
                    AbstractRoom endRoom = world.GetAbstractRoom(migrateDestination.Value.room);

                    section = "Pathfinding 3";

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
                    if (pathToDestination == null || resetPath)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            List<WorldCoordinate> path = AbstractSpacePathFinder.Path(world, cloudfish.room.GetWorldCoordinate(pos), migrateDestination.Value, template, null);
                            if (path != null)
                            {
                                pathToDestination = path;
                                break;
                            }
                        }
                    }

                    section = "Pathfinding 4";

                    if (pathToDestination != null)
                    {
                        section = "Pathfinding 5";

                        WorldCoordinate? travelNode = null;
                        foreach (WorldCoordinate node in pathToDestination)
                        {
                            AbstractRoom roomOfNode = world.GetAbstractRoom(node.room);
                            if (roomOfNode.realizedRoom != null)
                            {
                                WorldCoordinate actualCoordOfNode = roomOfNode.realizedRoom.LocalCoordinateOfNode(node.abstractNode);
                                if (roomOfNode == cloudfish.room.abstractRoom && travelNode == null)
                                {
                                    travelNode = actualCoordOfNode;
                                    Create_LineAndDot(roomOfNode.realizedRoom, pos, roomOfNode.realizedRoom.MiddleOfTile(actualCoordOfNode), "Yellow", 0);
                                    if (pathFinder.destination != actualCoordOfNode)
                                    {
                                        pathFinder.AssignNewDestination(actualCoordOfNode);
                                    }
                                }
                                else
                                {
                                    Create_Dot(roomOfNode.realizedRoom, roomOfNode.realizedRoom.MiddleOfTile(actualCoordOfNode), "Red", 0);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("");
            Debug.Log("EXCEPTION OCCURED IN UPDATEMOVEMENT METHOD - SECTION: " + section);
            if (migrateDestination.HasValue)
            {
                Debug.Log("MIGRATE DESTINATION: " + migrateDestination.Value);
            }
            else
            {
                Debug.Log("MIGRATE DESTINATION HAS NO VALUE");
            }
            Debug.Log(e);
            Debug.Log("");
        }

        try
        {
            circling = false;
            if (pathfinding)
            {
                pathFinder.stepsPerFrame = 10;
                pathFinder.accessibilityStepsPerFrame = 60;

                PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);

                if (circleCounter > 0 && behavior == Behavior.Wander && map.getTerrainProximity(pos) > 4)
                {
                    circling = true;
                }
                else if (cell.generation != pathFinder.pathGeneration)
                {
                    Create_Square(cloudfish.room, pos, 5f, 5f, Custom.DegToVec(45f), "Red", 0);
                    if (map.getAItile(pos).narrowSpace)
                    {
                        moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5 ? 1 : -1);
                        Vector2 testPos1 = pos + moveDir * 20f;
                        if (map.getAItile(testPos1).acc == AItile.Accessibility.Solid)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                IntVector2 randomDir = Custom.fourDirections[Random.Range(0, 4)];
                                Vector2 testDir = randomDir.ToVector2().normalized * 10f;
                                Vector2 reverseDir = -moveDir;
                                Vector2 testPos = pos + testDir;
                                if (map.getAItile(testPos).acc != AItile.Accessibility.Solid && Mathf.Abs(Custom.Angle(testDir, reverseDir)) > 45)
                                {
                                    moveDir = testDir.normalized;
                                    break;
                                }
                            }
                        }
                    }
                    else if (cloudfish.room.VisualContact(pos, destination))
                    {
                        moveDir = Vector2.Lerp(moveDir, Custom.DirVec(pos, destination), 0.8f);
                    }
                    else
                    {
                        IntVector2 tilePos = temporaryWanderDestination.Tile;
                        if (!pathFinder.coveredArea.Includes(intPos))
                        {
                            if (!cloudfish.room.VisualContact(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) || Custom.Dist(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) > 100)
                            {
                                Vector2? closestPos = null;
                                for (int i = 0; i < 10; i++)
                                {
                                    IntVector2 testTile = new(intPos.x + Random.Range(-10, 11), intPos.y + Random.Range(-10, 11));
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
                                    temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(closestPos.Value);
                                }
                            }
                        }
                        else if (Custom.Dist(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) > 100)
                        {
                            if (Custom.Dist(pos, destination) < 100)
                            {
                                temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(destination);
                            }
                            else
                            {
                                temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(pos);
                            }
                        }
                        else if (Random.value > 0.5 && map.getTerrainProximity(tilePos) < 10)
                        {
                            IntVector2 currentWanderTile = temporaryWanderDestination.Tile;
                            for (int i = 0; i < 10; i++)
                            {
                                IntVector2 testTile = new(currentWanderTile.x + Random.Range(-2, 3), currentWanderTile.y + Random.Range(-2, 3));
                                if (map.getTerrainProximity(currentWanderTile) < map.getTerrainProximity(testTile))
                                {
                                    temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(testTile);
                                }
                            }
                        }

                        if (Custom.Dist(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)) < 100)
                        {
                            moveDir = Vector2.Lerp(moveDir, Custom.rotateVectorDeg(Custom.DirVec(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)), 60f), 0.8f);
                        }
                        else
                        {
                            moveDir = Vector2.Lerp(moveDir, Custom.DirVec(pos, cloudfish.room.MiddleOfTile(temporaryWanderDestination)), 0.8f);
                        }
                    }
                }
                else
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
                        Create_Square(cloudfish.room, Vector2.Lerp(pos1, pos2, 0.5f), 1f, Custom.Dist(pos1, pos2), Custom.DirVec(pos1, pos2), "White", 0);
                        Create_Square(cloudfish.room, Vector2.Lerp(pos2, pos3, 0.5f), 1f, Custom.Dist(pos2, pos3), Custom.DirVec(pos2, pos3), "White", 0);
                        Create_Square(cloudfish.room, Vector2.Lerp(pos3, pos4, 0.5f), 1f, Custom.Dist(pos3, pos4), Custom.DirVec(pos3, pos4), "White", 0);
                    }
                }
            }
            else
            {
                pathFinder.stepsPerFrame = 1;
                pathFinder.accessibilityStepsPerFrame = 1;
            }

            if (circling)
            {
                if (circleCounter > 0)
                {
                    circleCounter--;
                }

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
            }

            foreach (TrackedObject trackedCreature in trackedCreatures)
            {
                Creature creature = trackedCreature.obj as Creature;
                foreach (BodyChunk chunk in creature.bodyChunks)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (Custom.DistLess(pos, chunk.pos, chunk.rad + 20f))
                        {
                            moveDir = Custom.rotateVectorDeg(moveDir, 180f);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            foreach (Vector2 avoidPos in avoidPositions)
            {
                if (Custom.DistLess(pos, avoidPos, 100))
                {
                    moveDir = Custom.DirVec(pos, avoidPos);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("");
            Debug.Log("EXCEPTION OCCURED IN UPDATEMOVEMENT METHOD - SECTION THREE");
            Debug.Log(e);
            Debug.Log("");
        }

        if (migrateDestination.HasValue)
        {
            AbstractRoom destinationRoom = pathFinder.world.GetAbstractRoom(migrateDestination.Value);
            if (destinationRoom.index != cloudfish.room.abstractRoom.index)
            {
                Create_LineAndDot(cloudfish.room, pos, pos + new Vector2(0f, 50f), "White", 0);
                string migrateRoomName = cloudfish.abstractCreature.world.GetAbstractRoom(migrateDestination.Value.room).name;
                Create_Text(cloudfish.room, pos + new Vector2(0f, 70f), "MIGRATING TO ROOM: " + migrateRoomName, "White", 0);
            }
            if (destinationRoom.realizedRoom != null)
            {
                Vector2 destPos = destinationRoom.realizedRoom.MiddleOfTile(migrateDestination.Value);
                Create_Dot(destinationRoom.realizedRoom, destPos, "White", 0);

                if (destinationRoom.realizedRoom == cloudfish.room)
                {
                    Create_LineAndDot(cloudfish.room, pos, cloudfish.room.MiddleOfTile(migrateDestination.Value), "White", 0);
                }
            }
        }

        if (pathfinding && pathFinder.destination != null)
        {
            AbstractRoom destinationRoom = pathFinder.world.GetAbstractRoom(pathFinder.destination);
            if (destinationRoom.realizedRoom != null)
            {
                Vector2 destPos = destinationRoom.realizedRoom.MiddleOfTile(pathFinder.destination);
                Create_Dot(destinationRoom.realizedRoom, destPos, "Green", 0);
            }
        }
        
        if (circling)
        {
            Create_Text(cloudfish.room, cloudfish.firstChunk.pos + new Vector2(0f, -40f), "Circle Timer: - " + circleCounter.ToString(), "Purple", 0);
            Vector2 destPos = cloudfish.room.MiddleOfTile(temporaryWanderDestination);
            Create_LineAndDot(cloudfish.room, pos, destPos, "Purple", 0);
        }

        if (cloudfish.firstChunk.ContactPoint.ToVector2().magnitude > 0.1f && !map.getAItile(pos).narrowSpace)
        {
            //moveDir = -cloudfish.firstChunk.ContactPoint.ToVector2();
        }

        cloudfish.firstChunk.vel *= 0.8f;
        cloudfish.firstChunk.vel += moveDir * speed;
    }
    public override bool WantToStayInDenUntilEndOfCycle()
    {
        if (behavior == Behavior.GoToDen || (behavior == Behavior.Follow && myLeader.abstractAI.WantToStayInDenUntilEndOfCycle()))
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
    public class TrackedEntrance
    {
        public CloudFish cloudFish;
        public Room room;
        public IntVector2 position;
        public string type;
        public TrackedEntrance(CloudFish cloudFish, Room room, IntVector2 position, string type)
        {
            this.cloudFish = cloudFish;
            this.room = room;
            this.position = position;
            this.type = type;
        }
    }
}

public class CloudFishAbstractAI : AbstractCreatureAI
{
    public WorldCoordinate? migrateDestination = null;
    public AbstractCreature leader;
    public AbstractCreature flockLeader;
    public List<AbstractCreature> flock;
    public CloudFishAI.Behavior behavior;
    public int wanderTimer = 0;
    public float dominance;
    public CloudFishAbstractAI(World world, AbstractCreature parent) : base(world, parent)
    {
        this.world = world;
        this.parent = parent;

        behavior = CloudFishAI.Behavior.Wander;
        leader = null;
        dominance = Random.Range(0f, 20f);
    }

    public override void Update(int time)
    {
        int section = 1;
        try
        {
            base.Update(time);

            //Debug.Log("Abstract Fish AI Updated!");

            #region Update_Behavior;
            #endregion;

            #region Update_Movement
            if (behavior == CloudFishAI.Behavior.Flee)
            {
                behavior = CloudFishAI.Behavior.Wander;
            }
            if (behavior == CloudFishAI.Behavior.Follow && flockLeader != null)
            {
                section = 2;
                SetDestination(flockLeader.pos);

                if (flockLeader.realizedCreature == null)
                {
                    parent.Move(flockLeader.pos);
                }
            }
            else if (behavior == CloudFishAI.Behavior.Wander)
            {
                section = 3;
                foreach (AbstractCreature creature in flock)
                {
                    if (creature.realizedCreature == null)
                    {
                        creature.Move(parent.pos);
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
}
