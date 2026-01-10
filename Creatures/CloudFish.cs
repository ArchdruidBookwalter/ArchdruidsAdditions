using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevConsole;
using DevInterface;
using HarmonyLib;
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

    public int whistleTimer;

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

        AI = new(abstractCreature, world);

        whistleTimer = 0;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        AI.pathFinder.Reset(placeRoom);
    }
    public override void Update(bool eu)
    {
        float section = 0;

        try
        {
            section = 1;
            base.Update(eu);

            GoThroughFloors = true;

            if (!dead)
            {
                ChangeCollisionLayer(1);
                releasedSmoke = false;
            }
            else
            {
                ChangeCollisionLayer(1);
            }

            section = 2;
            if (room != null)
            {
                if (Consious)
                {
                    section = 2.1f;
                    Act();

                    gravity = 0.1f;
                    bounce = 0.2f;

                    IntVector2 tilePos = room.GetTilePosition(firstChunk.pos);
                    Room.Tile tile = room.GetTile(firstChunk.pos);
                    if (shortcutDelay == 0 && tile.Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(tilePos).shortCutType != ShortcutData.Type.DeadEnd)
                    {
                        enteringShortCut = tilePos;
                    }

                    lungs = 1f;
                }
                else
                {
                    section = 2.2f;

                    gravity = 0.9f;
                    if (room.gravity < 0.1f)
                    { bounce = 0.4f; }
                    else
                    { bounce = 0.2f; }

                    AI.behavior = CloudFishAI.Behavior.Flee;
                    AI.fleeCounter = 0;
                }

                section = 2.3f;
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
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "CLOUDFISH UPDATE", section);
        }
    }
    public override void Grabbed(Grasp grasp)
    {
        if (!dead)
        {
            room.PlaySound(Enums.NewSoundID.AA_CloudFishScream, firstChunk.pos, 2f, Random.Range(1f, 1.4f));
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

        room?.PlaySound(Enums.NewSoundID.AA_CloudFishDeath, firstChunk.pos, 2f, Random.Range(1f, 1.4f));

        if (AI != null && AI.trackedCreatures != null && AI.trackedCreatures.Count > 0)
        {
            foreach (CloudFishAI.TrackedObject obj in AI.trackedCreatures)
            {
                if (obj.obj is CloudFish cloudFish && Custom.DistLess(firstChunk.pos, cloudFish.firstChunk.pos, 1000) && cloudFish.room != null)
                {
                    cloudFish.AI.fleeCounter = 0;
                    cloudFish.AI.abstractAI.behavior = CloudFishAI.Behavior.Flee;
                    cloudFish.AI.lastDangerRoom = cloudFish.room;
                }
            }
        }
    }
    public void Act()
    {
        AI.Update();
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
        float newDamage = damage;
        if (type != null && type == DamageType.Stab)
        {
            newDamage *= 5f;
            if (directionAndMomentum.HasValue)
            {
                ReleaseSmoke(-directionAndMomentum.Value.normalized);
            }
            else
            {
                ReleaseSmoke(Custom.RNV() * 20f);
            }
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, newDamage, stunBonus);
    }
    public void ReleaseSmoke(Vector2 dir)
    {
        if (!releasedSmoke)
        {
            CloudFishGraphics graphics = graphicsModule as CloudFishGraphics;
            float bodyHue = Custom.RGB2HSL(graphics.bodyColor).x;
            Color startColor = Custom.HSL2RGB(bodyHue, 0.2f, 0.8f);
            Color endColor = Custom.HSL2RGB(bodyHue, 0.2f, 0.1f);
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

    public Whisker whisker1;
    public Whisker whisker2;
    public Whisker whisker3;
    public Whisker whisker4;
    public float wiggleWhiskers;

    public FSprite eye1;
    public FSprite eye2;

    public TailSegment[] tail;
    public int numOfTailSegments;
    public int numOfFinSegments;
    public int numOfBodySegments;
    public int bodyFin1Length;
    public int bodyFin2Length;

    public bool shakeTail;
    public float shakeTailTimer;
    public float maxTailShake = 5;
    public Vector2 tailShakeDir;

    public Vector2 bodyRotation;
    public Vector2 lastBodyRotation;

    public Vector2 tailDir0;
    public Vector2 tailDir1;
    public Vector2 tailDir2;
    public Vector2 tailDir3;
    public Vector2 tailDir4;

    public Vector2 eyesLookDir;
    public Vector2 lastEyesLookDir;

    public Color bodyColor;
    public Color eyeColor;

    public Color blackColor;
    public float darkness;

    public CloudFishGraphics(PhysicalObject ow) : base(ow, false)
    {
        cloudFish = ow as CloudFish;
        List<BodyPart> list = [];

        Random.State state = Random.state;
        Random.InitState(cloudFish.abstractCreature.ID.RandomSeed);

        #region Body And Tail
        float tailSegmentLength = 10f;
        float tailSegmentWidth = 4f;
        numOfBodySegments = Random.Range(4, 6);
        numOfFinSegments = Random.Range(3, 5);
        numOfTailSegments = numOfBodySegments + (numOfFinSegments - 1);

        tail = new TailSegment[numOfTailSegments];
        tail[0] = new TailSegment(this, tailSegmentWidth, tailSegmentLength, null, 0.8f, 1f, 0f, true);
        for (int i = 1; i < numOfTailSegments; i++)
        {
            tail[i] = new TailSegment(this, Mathf.Lerp(tailSegmentWidth, 0, (float)i / numOfBodySegments), tailSegmentLength, tail[i - 1], 0.8f, 1f, 0f, true);
        }

        foreach (TailSegment segment in tail)
        { list.Add(segment); }
        #endregion


        #region Colors
        float dominance = cloudFish.dominance;
        if (Random.value < 0.001)
        {
            bodyColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0f, 0.1f, 0.6f), Mathf.Pow(dominance, 2) / 3f, Custom.ClampedRandomVariation(0.8f, 0.05f, 0.1f));
            eyeColor = new(1f, 0f, 0f);
        }
        else
        {
            Color baseDomColor = Custom.HSL2RGB(0.52f, 0.4f, 0.5f);
            Color baseSubColor = Custom.HSL2RGB(0.52f, 0f, 0.5f);
            Color baseEyeColor = new(0f, 1f, 1f);
            float spread = 0.1f;

            try
            {
                string domColor = Plugin.RegionData.ReadRegionData(cloudFish.room.world.region.name, "CloudFishColorA");
                if (domColor != null)
                {
                    if (domColor.StartsWith("#"))
                    {
                        domColor = domColor.Remove(0, 1);
                    }
                    baseDomColor = Custom.hexToColor(domColor);
                }

                string subColor = Plugin.RegionData.ReadRegionData(cloudFish.room.world.region.name, "CloudFishColorB");
                if (subColor != null)
                {
                    if (subColor.StartsWith("#"))
                    {
                        subColor = subColor.Remove(0, 1);
                    }
                    baseSubColor = Custom.hexToColor(subColor);
                }

                string eyeColor = Plugin.RegionData.ReadRegionData(cloudFish.room.world.region.name, "CloudFishEyeColor");
                if (eyeColor != null)
                {
                    if (eyeColor.StartsWith("#"))
                    {
                        eyeColor = eyeColor.Remove(0, 1);
                    }
                    baseEyeColor = Custom.hexToColor(eyeColor);
                }

                string hueSpread = Plugin.RegionData.ReadRegionData(cloudFish.room.world.region.name, "CloudFishHueSpread");
                if (hueSpread != null)
                {
                    spread = float.Parse(hueSpread);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Exception occured when trying to get the region color data for a Cloudfish! Is the region properties file formatted correctly? Check the Steam Workshop page for more information.");
                Debug.LogException(e);
            }

            Vector3 colorAVec = Custom.RGB2HSL(baseDomColor);
            Vector3 colorBVec = Custom.RGB2HSL(baseSubColor);

            Color dominantColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(colorAVec.x, spread, 0.6f), colorAVec.y, Custom.ClampedRandomVariation(colorAVec.z, 0.05f, 0.1f));
            Color submissiveColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(colorBVec.x, spread, 0.6f), colorBVec.y, Custom.ClampedRandomVariation(colorBVec.z, 0.05f, 0.1f));

            bodyColor = Color.Lerp(submissiveColor, dominantColor, dominance);
            eyeColor = baseEyeColor;
        }
        #endregion


        #region Whiskers
        whisker1 = new(this, new Vector2(0f, 1f), 20f, Random.Range(10f, 20f), 0.8f, cloudFish.firstChunk);
        whisker2 = new(this, new Vector2(0f, 1f), 20f, Random.Range(10f, 20f), 0.8f, cloudFish.firstChunk);
        whisker3 = new(this, new Vector2(0f, 1f), 20f, Random.Range(10f, 20f), 0.8f, cloudFish.firstChunk);
        whisker4 = new(this, new Vector2(0f, 1f), 20f, Random.Range(10f, 20f), 0.8f, cloudFish.firstChunk);
        list.Add(whisker1.bodyPart);
        list.Add(whisker2.bodyPart);
        list.Add(whisker3.bodyPart);
        list.Add(whisker4.bodyPart);
        #endregion


        #region Fins
        bodyFin1Length = Random.Range(2, 4);
        bodyFin2Length = Random.Range(2, 4);
        #endregion


        Random.state = state;

        bodyParts = list.ToArray();
    }

    public override void Update()
    {
        base.Update();

        tail[0].connectedPoint = cloudFish.firstChunk.pos;

        if (!cloudFish.Consious)
        {
            wiggleWhiskers = 0;
            eyesLookDir = new Vector2(0f, 0f);
        }
        else
        {
            wiggleWhiskers = Random.Range(-10, 10);
            CloudFishAI.MovementType movement = cloudFish.AI.movementType;
            CloudFishAI.Behavior behavior = cloudFish.AI.behavior;

            if (movement == CloudFishAI.MovementType.FollowPath || movement == CloudFishAI.MovementType.GoStraightToPoint || movement == CloudFishAI.MovementType.AvoidWalls)
            {
                this.shakeTail = true;
                if (shakeTailTimer > maxTailShake * 2f)
                {
                    shakeTailTimer = 0;
                }
                else
                {
                    shakeTailTimer += cloudFish.AI.speed / 5f;
                }
            }
            else
            {
                this.shakeTail = false;
                shakeTailTimer = 0;
            }

            if (behavior == CloudFishAI.Behavior.Wander || behavior == CloudFishAI.Behavior.Follow)
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

        float shakeTail = Mathf.PingPong(shakeTailTimer, maxTailShake) - maxTailShake / 2f;
        tailShakeDir = Custom.rotateVectorDeg(bodyRotation, shakeTail);

        Vector2 previousSegmentRotation = Custom.DirVec(tail[0].pos, cloudFish.firstChunk.pos);
        foreach (TailSegment segment in tail)
        {
            int i = tail.IndexOf(segment);

            if (i != 0 && cloudFish.bites == 1)
            {
                break;
            }

            segment.Update();

            if (!cloudFish.Consious && cloudFish.grabbedBy.Count > 0 && i == 0)
            {
                //segment.pos = segment.connectedPoint.Value + Custom.DegToVec(120) * segment.connectionRad;
                //segment.lastPos = segment.pos;
            }

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

            string color = "Green";

            float angle = Custom.Angle(segmentRotation, previousSegmentRotation);
            if (Mathf.Abs(angle) > 20)
            {
                color = "Yellow";
                Vector2 tryPos = segmentConnectPos + Custom.rotateVectorDeg(-previousSegmentRotation, angle > 0 ? 20 : -20) * segment.connectionRad;
                if (cloudFish.room.GetTile(tryPos).Solid && segment.connectedSegment != null)
                {
                    Vector2 conSegConPos = segment.connectedSegment.connectedPoint ?? segment.connectedSegment.connectedSegment.pos;
                    Vector2 conSegRot = Custom.DirVec(conSegConPos, segment.connectedSegment.pos);
                    for (int j = 0; j < 10; j++)
                    {
                        Vector2 newConSegRot = Custom.rotateVectorDeg(conSegRot, (angle > 0 ? 20 : -20) * j);
                        Vector2 newConSegPos = conSegConPos + newConSegRot * segment.connectedSegment.connectionRad;
                        Vector2 newSegPos = newConSegPos + newConSegRot * segment.connectionRad;
                        Vector2 newSegRot = Custom.DirVec(newSegPos, newConSegPos);
                        float newAngle = Custom.Angle(newSegRot, newConSegRot);

                        //Create_Square(cloudFish.room, newConSegPos, 1f, 1f, conSegRot, "White", 0);
                        //Create_Square(cloudFish.room, newSegPos, 1f, 1f, conSegRot, "Purple", 0);
                        if (!cloudFish.room.GetTile(newSegPos).Solid)
                        {
                            //segment.pos = newSegPos;
                            //segment.lastPos = newSegPos;
                            //segment.connectedSegment.pos = newConSegPos;
                            //segment.connectedSegment.lastPos = newConSegPos;
                            //break;
                        }
                    }
                    color = "Red";
                }
                segment.pos = segmentConnectPos + Custom.rotateVectorDeg(-previousSegmentRotation, angle > 0 ? 20 : -20) * segment.connectionRad;
            }

            segment.PushOutOfTerrain(cloudFish.room, cloudFish.firstChunk.pos);

            //Create_Square(cloudFish.room, segment.pos, 2f, 2f, segmentRotation, color, 0);
            previousSegmentRotation = segmentRotation;
        }

        lastBodyRotation = bodyRotation;
        if (cloudFish.bites > 1)
        {
            bodyRotation = Custom.DirVec(tail[0].pos, cloudFish.firstChunk.pos);
        }
        else
        {
            if (cloudFish.firstChunk.ContactPoint.ToVector2().magnitude < 0.1f && cloudFish.firstChunk.vel.magnitude > 0.1f)
            {
                bodyRotation = Custom.rotateVectorDeg(bodyRotation, 1);
            }
        }

        Vector2 whisker1Dir = Custom.RotateAroundOrigo(bodyRotation, 45 + wiggleWhiskers);
        Vector2 whisker2Dir = Custom.RotateAroundOrigo(bodyRotation, -45 - wiggleWhiskers);
        Vector2 whisker3Dir = Custom.RotateAroundOrigo(bodyRotation, 90 + wiggleWhiskers);
        Vector2 whisker4Dir = Custom.RotateAroundOrigo(bodyRotation, -90 - wiggleWhiskers);

        whisker1.Update(whisker1Dir);
        whisker2.Update(whisker2Dir);
        whisker3.Update(whisker3Dir);
        whisker4.Update(whisker4Dir);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[16];

        TriangleMesh bodyMesh = TriangleMesh.MakeLongMesh(numOfTailSegments, true, false);
        sLeaser.sprites[0] = bodyMesh;
        TriangleMesh shineMesh = TriangleMesh.MakeLongMesh(numOfTailSegments, true, false);
        sLeaser.sprites[1] = shineMesh;

        sLeaser.sprites[2] = new FSprite("JetFishEyeA", true);
        sLeaser.sprites[3] = new FSprite("JetFishEyeB", true);

        int tailFinVertices = (numOfFinSegments * 2) - 1;
        int finSegments = (tailFinVertices + 3) / 4;
        TriangleMesh finMesh1 = TriangleMesh.MakeLongMesh(5, false, false);
        sLeaser.sprites[4] = finMesh1;
        TriangleMesh finMesh2 = TriangleMesh.MakeLongMesh(5, false, false);
        sLeaser.sprites[5] = finMesh2;
        TriangleMesh finMesh3 = TriangleMesh.MakeLongMesh(5, false, false);
        sLeaser.sprites[6] = finMesh3;
        TriangleMesh finMesh4 = TriangleMesh.MakeLongMesh(5, false, false);
        sLeaser.sprites[7] = finMesh4;

        TriangleMesh whiskerMesh1 = TriangleMesh.MakeLongMesh(4, true, false);
        sLeaser.sprites[8] = whiskerMesh1;
        TriangleMesh whiskerMesh2 = TriangleMesh.MakeLongMesh(4, true, false);
        sLeaser.sprites[9] = whiskerMesh2;
        TriangleMesh whiskerMesh3 = TriangleMesh.MakeLongMesh(4, true, false);
        sLeaser.sprites[10] = whiskerMesh3;
        TriangleMesh whiskerMesh4 = TriangleMesh.MakeLongMesh(4, true, false);
        sLeaser.sprites[11] = whiskerMesh4;

        sLeaser.sprites[12] = whisker1.triMesh;
        sLeaser.sprites[13] = whisker2.triMesh;
        sLeaser.sprites[14] = whisker3.triMesh;
        sLeaser.sprites[15] = whisker4.triMesh;

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float section = 0;
        try
        {
            Vector2 chunkPos = Vector2.Lerp(cloudFish.firstChunk.lastPos, cloudFish.firstChunk.pos, timeStacker) - camPos;
            Vector2 eyesLookDirection = Vector2.Lerp(lastEyesLookDir, eyesLookDir, timeStacker);
            int bites = cloudFish.bites;

            List<Vector2> tailPositions = [];
            List<Vector2> tailRotations = [];
            List<Vector2> bodyPositions = [];
            List<Vector2> shinePositions = [];
            List<Vector2> bodyFin1Positions = [];
            List<Vector2> bodyFin2Positions = [];
            List<Vector2> tailFin1Positions = [];
            List<Vector2> tailFin2Positions = [];
            tailPositions.Add(chunkPos);

            Vector2 headRot = Vector2.Lerp(lastBodyRotation, bodyRotation, timeStacker);
            float rotX = headRot.x;

            Vector2 headPos1 = chunkPos + headRot * (tail[0].StretchedRad * 2f) - Custom.PerpendicularVector(headRot) * (tail[0].StretchedRad / 2f);
            Vector2 headPos2 = chunkPos + headRot * (tail[0].StretchedRad * 2f) + Custom.PerpendicularVector(headRot) * (tail[0].StretchedRad / 2f);
            Vector2 headPos3 = chunkPos - Custom.PerpendicularVector(headRot) * tail[0].StretchedRad;
            Vector2 headPos4 = chunkPos + Custom.PerpendicularVector(headRot) * tail[0].StretchedRad;

            bodyPositions.Add(headPos1);
            bodyPositions.Add(headPos2);
            bodyPositions.Add(headPos3);
            bodyPositions.Add(headPos4);

            shinePositions.Add(chunkPos + Custom.PerpendicularVector(headRot) * rotX * 2f);

            bodyFin1Positions.Add(headPos3);
            bodyFin2Positions.Add(headPos4);

            section = 1;

            #region Getting Draw Positions 
            Vector2 bodySegConnectPos = chunkPos;
            float bodyFinWidth = 5f;
            int tailFinIndex = 0;
            float tailFinWidth = 5f;

            for (int i = 0; i < numOfTailSegments; i++)
            {
                if (bites < 4 && i > bites - 1)
                {
                    break;
                }

                TailSegment segment = tail[i];

                Vector2 segmentPos = Vector2.Lerp(segment.lastPos, segment.pos, timeStacker) - camPos;
                Vector2 segmentRot = Custom.DirVec(bodySegConnectPos, segmentPos);
                Vector2 perpRot = Custom.PerpendicularVector(segmentRot);
                tailPositions.Add(segmentPos);
                tailRotations.Add(segmentRot);
                float segmentRad = Mathf.Clamp(segment.StretchedRad, 0.1f, 50);

                section = 1.1f;

                if (i == numOfBodySegments)
                {
                    bodyPositions.Add(segmentPos);

                    shinePositions.Add(segmentPos);
                }
                else
                {
                    if (i < numOfBodySegments)
                    {
                        Vector2 bodyPos1 = segmentPos + perpRot * segmentRad;
                        Vector2 bodyPos2 = segmentPos - perpRot * segmentRad;
                        bodyPositions.Add(bodyPos1);
                        bodyPositions.Add(bodyPos2);

                        Vector2 shinePos1 = segmentPos + perpRot * (segmentRad / 4f) - perpRot * rotX;
                        Vector2 shinePos2 = segmentPos - perpRot * (segmentRad / 4f) - perpRot * rotX;
                        shinePositions.Add(shinePos1);
                        shinePositions.Add(shinePos2);
                    }

                    section = 1.2f;

                    if (i < bodyFin1Length - 1)
                    {
                        bodyFin1Positions.Add(segmentPos + perpRot * segmentRad);
                        bodyFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * bodyFinWidth);
                    }
                    else if (i == bodyFin1Length - 1)
                    {
                        bodyFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * bodyFinWidth / 2f);
                        bodyFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * bodyFinWidth);
                    }
                    else if (i == bodyFin1Length)
                    {
                        bodyFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * bodyFinWidth);
                    }

                    section = 1.3f;

                    if (i < bodyFin2Length - 1)
                    {
                        bodyFin2Positions.Add(segmentPos - perpRot * segmentRad);
                        bodyFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * bodyFinWidth);
                    }
                    else if (i == bodyFin2Length - 1)
                    {
                        bodyFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * bodyFinWidth / 2f);
                        bodyFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * bodyFinWidth);
                    }
                    else if (i == bodyFin2Length)
                    {
                        bodyFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * bodyFinWidth);
                    }
                    bodyFinWidth++;
                }

                section = 1.4f;

                if (i > numOfBodySegments - 2)
                {
                    if (tailFinIndex == 0)
                    {
                        tailFin1Positions.Add(segmentPos + perpRot * segmentRad);

                        tailFin2Positions.Add(segmentPos - perpRot * segmentRad);
                    }
                    else if (i == numOfTailSegments - 1)
                    {
                        tailFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * tailFinWidth);

                        tailFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * tailFinWidth);
                    }
                    else if (tailFinIndex == 1)
                    {
                        tailFin1Positions.Add(segmentPos);
                        tailFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * tailFinWidth);

                        tailFin2Positions.Add(segmentPos);
                        tailFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * tailFinWidth);
                    }
                    else
                    {
                        tailFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * tailFinWidth / 2f);
                        tailFin1Positions.Add(segmentPos + perpRot * segmentRad + perpRot * tailFinWidth);

                        tailFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * tailFinWidth / 2f);
                        tailFin2Positions.Add(segmentPos - perpRot * segmentRad - perpRot * tailFinWidth);
                    }
                    tailFinWidth += Mathf.Sqrt(tailFinIndex);
                    tailFinIndex++;
                }

                bodySegConnectPos = segmentPos;
            }
            #endregion

            section = 2;

            #region Setting Mech Vertices
            bodyMech = sLeaser.sprites[0] as TriangleMesh;
            if (bodyPositions.Count > 0)
            {
                for (int i = 0; i < bodyMech.vertices.Length; i++)
                {
                    Vector2 bodyPos = bodyPositions[Mathf.Clamp(i, 0, bodyPositions.Count - 1)];
                    bodyMech.MoveVertice(i, bodyPos);
                }
                bodyMech.alpha = 1f;
            }

            shineMesh = sLeaser.sprites[1] as TriangleMesh;
            if (shinePositions.Count > 0)
            {
                for (int i = 0; i < shineMesh.vertices.Length; i++)
                {
                    Vector2 shinePos = shinePositions[Mathf.Clamp(i, 0, shinePositions.Count - 1)];

                    shineMesh.MoveVertice(i, shinePos);
                }
                shineMesh.alpha = 1f;
            }

            tailFinMesh1 = sLeaser.sprites[4] as TriangleMesh;
            if (tailFin1Positions.Count > 0)
            {
                for (int i = 0; i < tailFinMesh1.vertices.Length; i++)
                {
                    Vector2 finPos = tailFin1Positions[Mathf.Clamp(i, 0, tailFin1Positions.Count - 1)];

                    tailFinMesh1.MoveVertice(i, finPos);
                }
                tailFinMesh1.alpha = 1f;
            }

            tailFinMesh2 = sLeaser.sprites[5] as TriangleMesh;
            if (tailFin2Positions.Count > 0)
            {
                for (int i = 0; i < tailFinMesh2.vertices.Length; i++)
                {
                    Vector2 finPos = tailFin2Positions[Mathf.Clamp(i, 0, tailFin2Positions.Count - 1)];

                    tailFinMesh2.MoveVertice(i, finPos);
                }
                tailFinMesh2.alpha = 1f;
            }

            bodyFinMesh1 = sLeaser.sprites[6] as TriangleMesh;
            if (bodyFin1Positions.Count > 0)
            {
                for (int i = 0; i < bodyFinMesh1.vertices.Length; i++)
                {
                    Vector2 finPos = bodyFin1Positions[Mathf.Clamp(i, 0, bodyFin1Positions.Count - 1)];

                    bodyFinMesh1.MoveVertice(i, finPos);
                }
                bodyFinMesh1.alpha = 1f;
            }

            bodyFinMesh2 = sLeaser.sprites[7] as TriangleMesh;
            if (bodyFin2Positions.Count > 0)
            {
                for (int i = 0; i < bodyFinMesh2.vertices.Length; i++)
                {
                    Vector2 finPos = bodyFin2Positions[Mathf.Clamp(i, 0, bodyFin2Positions.Count - 1)];

                    bodyFinMesh2.MoveVertice(i, finPos);
                }
                bodyFinMesh2.alpha = 1f;
            }
            #endregion

            section = 3;

            #region Eye
            eye1 = sLeaser.sprites[2];
            eye1.SetPosition(chunkPos);
            eye1.rotation = Custom.VecToDeg(headRot);
            eye1.scale = 1f;
            eye1.alpha = 1f;

            eye2 = sLeaser.sprites[3];
            eye2.SetPosition(chunkPos + eyesLookDirection * 1f);
            eye2.rotation = Custom.VecToDeg(headRot);
            eye2.scale = 0.5f;
            eye2.alpha = 1f;
            #endregion

            section = 4;

            #region Whiskers

            whisker1.DrawSprite(camPos, timeStacker, bodyPositions[0], headRot, true);
            whisker2.DrawSprite(camPos, timeStacker, bodyPositions[1], headRot, false);
            whisker3.DrawSprite(camPos, timeStacker, bodyPositions[0], headRot, true);
            whisker4.DrawSprite(camPos, timeStacker, bodyPositions[1], headRot, false);

            #endregion

            section = 5;

            #region Color
            Vector2 actualChunkPos = chunkPos + camPos;
            float roomDarkness = Mathf.Clamp((rCam.room.Darkness(actualChunkPos) - 0.3f) * 1.6f, 0f, 1f);

            float lightSourceExposure = 0;
            float r = 0;
            float g = 0;
            float b = 0;
            List<LightSource> nearbyLightSources = [];
            foreach (LightSource light in rCam.room.lightSources)
            {
                CircularSprite newSprite = new(light.ElementName, 64);
                float trueLightRad = light.rad * (newSprite.width / 2f) * 0.125f;

                if (Custom.DistLess(actualChunkPos, light.pos, trueLightRad))
                {
                    float thisLightExposure = Mathf.Lerp(1, 0, Custom.Dist(actualChunkPos, light.pos) / trueLightRad);
                    lightSourceExposure = Mathf.Max(lightSourceExposure, thisLightExposure);

                    r = Mathf.Max(r, light.color.r * thisLightExposure);
                    g = Mathf.Max(g, light.color.g * thisLightExposure);
                    b = Mathf.Max(b, light.color.b * thisLightExposure);
                }
            }

            float trueDarkness = roomDarkness - lightSourceExposure;
            Color lightColor = new(r, g, b);
            Color baseColor = Color.Lerp(this.bodyColor, Color.Lerp(this.bodyColor, lightColor, Custom.RGB2HSL(lightColor).y), Mathf.Clamp(lightSourceExposure, 0, 0.5f));

            Vector3 vecBodyColor = Custom.RGB2HSL(baseColor);
            Color bodyColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, vecBodyColor.z), blackColor, trueDarkness);
            Color shineColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, Mathf.Clamp(vecBodyColor.z + 0.2f, 0f, 1f)), blackColor, trueDarkness);
            Color finColor = Color.Lerp(Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, Mathf.Clamp(vecBodyColor.z - 0.1f, 0f, 1f)), blackColor, trueDarkness);

            bodyMech.color = bodyColor;
            shineMesh.color = Color.Lerp(shineColor, bodyColor, darkness * 0.5f);

            tailFinMesh1.color = finColor;
            tailFinMesh2.color = finColor;
            bodyFinMesh1.color = finColor;
            bodyFinMesh2.color = finColor;

            whisker1.triMesh.color = finColor;
            whisker2.triMesh.color = finColor;
            whisker3.triMesh.color = finColor;
            whisker4.triMesh.color = finColor;

            eye1.color = blackColor;
            eye2.color = eyeColor;
            #endregion

            section = 6;

            if (cloudFish.bites <= 3)
            {
                tailFinMesh1.alpha = 0f;
                tailFinMesh2.alpha = 0f;
            }

            if (cloudFish.slatedForDeletetion || cloudFish.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "DRAWSPRITES", section);
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

        blackColor = palette.blackColor;
    }

    public class Whisker
    {
        public Vector2 endPos;
        public GenericBodyPart bodyPart;
        public TriangleMesh triMesh;
        public BodyChunk connectedChunk;
        public float lengthA;
        public float lengthB;
        public float width;

        public Whisker(GraphicsModule module, Vector2 dir, float pointDistLength, float actualLength, float width, BodyChunk connectedChunk)
        {
            this.connectedChunk = connectedChunk;
            this.width = width;

            lengthA = pointDistLength;
            lengthB = actualLength;

            endPos = connectedChunk.pos + dir * lengthA;
            bodyPart = new(module, 1f, 0.6f, 0.9f, connectedChunk);
            triMesh = TriangleMesh.MakeLongMesh(4, true, false);
        }

        public void Update(Vector2 newDir)
        {
            endPos = connectedChunk.pos + newDir * lengthA;
            bodyPart.Update();
            bodyPart.ConnectToPoint(endPos, 0f, false, 0f, connectedChunk.vel, 0f, 0f);
        }

        public void DrawSprite(Vector2 camPos, float timeStacker, Vector2 startPos, Vector2 bodyRot, bool flip)
        {
            Vector2 endPos = Vector2.Lerp(bodyPart.lastPos, bodyPart.pos, timeStacker) - camPos;
            float width = flip ? -this.width : this.width;

            Vector2 rot = Custom.DirVec(startPos, endPos);
            for (int j = 0; j < 13; j += 2)
            {
                Vector2 perpRot = Custom.RotateAroundOrigo(rot, 90);
                if (j == 0)
                {
                    triMesh.MoveVertice(j, startPos);
                    triMesh.MoveVertice(j + 1, startPos - bodyRot * 2f);
                }
                else if ((j / 14f * lengthA) > lengthB)
                {
                    float bendFac = (-Mathf.Pow(j - 7f, 2f) + 36f) / 20f * (flip ? -1 : 1);
                    triMesh.MoveVertice(j, Vector2.Lerp(startPos, endPos, j / 14f) + perpRot * bendFac);
                    triMesh.MoveVertice(j + 1, Vector2.Lerp(startPos, endPos, j / 14f) + perpRot * bendFac);
                }
                else
                {
                    float bendFac = (-Mathf.Pow(j - 7f, 2f) + 36f) / 20f * (flip ? -1 : 1);
                    triMesh.MoveVertice(j, Vector2.Lerp(startPos, endPos, j / 14f) + perpRot * width + perpRot * bendFac);
                    triMesh.MoveVertice(j + 1, Vector2.Lerp(startPos, endPos, j / 14f) - perpRot * width + perpRot * bendFac);
                }
            }
            triMesh.MoveVertice(14, endPos);
        }
    }
}



public class CloudFishAI : ArtificialIntelligence
{
    public bool exception = false;

    public CloudFish cloudfish;
    public CloudFishAbstractAI abstractAI;
    public Behavior behavior;
    public MovementType movementType;
    public List<TrackedObject> trackedCreatures;
    public List<TrackedObject> trackedObjects;
    public AbstractCreature myLeader;
    public AbstractCreature flockLeader;
    public int lastLastRoom;
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

    public Vector2? AverageDangerPos
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

    public List<BodyChunk> nearbyChunks;
    public Vector2 AverageChunkPos
    {
        get
        {
            float averageX = 0;
            float averageY = 0;
            float highestRad = 0;

            foreach (BodyChunk chunk in nearbyChunks)
            {
                averageX += chunk.pos.x;
                averageY += chunk.pos.y;
                if (chunk.rad > highestRad)
                { highestRad = chunk.rad; }
            }

            Vector2 averageChunkPos = new(averageX / nearbyChunks.Count, averageY / nearbyChunks.Count);

            return averageChunkPos;
        }
    }

    public CosmeticInsect nearestInsect;
    public int huntTimer;
    public int foodEaten;
    public int maxFood;

    public int minCircleTime = 100;
    public int maxCircleTime = 500;
    public int minRoomTime = 300;
    public int maxRoomTime = 1000;
    public int minFleeTime = 100;
    public int maxFleeTime = 500;

    public bool shakeTail;
    public int shakeTailTimer;

    public int whistleTimer;

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
        lastLastRoom = -1;

        behavior = abstractAI.behavior;
        circleCounter = Random.Range(minCircleTime, maxCircleTime);
        stayInRoomDesire = Random.Range(minRoomTime, maxRoomTime);
        huntTimer = Random.Range(10, 200);
        maxFood = 5;

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
            { Methods.Methods.Log_Exception(e, "CLOUDFISH AI BASEUPDATE", 0); }

            if (cloudfish != null)
            {
                if (cloudfish.room != null)
                {
                    if (Random.value < 0.2)
                    { UpdateTrackedObjects(); }

                    try
                    { UpdateBehavior(); }
                    catch (Exception e)
                    { Methods.Methods.Log_Exception(e, "CLOUDFISH AI UPDATEBEHAVIOR", 0); }

                    try
                    { UpdateMovement(); }
                    catch (Exception e)
                    { Methods.Methods.Log_Exception(e, "CLOUDFISH AI UPDATEMOVEMENT", 0); }

                    
                    //Room room = cloudfish.room;
                    //Vector2 pos = cloudfish.firstChunk.pos;
                    //Create_Text(room, pos, behavior.value, "Yellow", 0);
                    //Create_Text(room, pos + new Vector2(0f, 20f), movementType.value, "Yellow", 0);
                    
                }
            }
        }
        catch (Exception e)
        { Methods.Methods.Log_Exception(e, "CLOUDFISH AI UPDATE", 0); }
    }
    public override void NewRoom(Room room)
    {
        lastLastRoom = lastRoom;
        base.NewRoom(room);
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
            Room room = cloudfish.room;
            AbstractCreature newLeader = abstractAI.leader;
            Behavior newBehavior = abstractAI.behavior;
            cloudfish.dominance = abstractAI.dominance;

            section = 2;
            if (Random.value < 0.5)
            {
                nearbyChunks = [];

                foreach (TrackedObject obj in trackedCreatures)
                {
                    if (obj.obj.room == cloudfish.room)
                    {
                        Vector2 enemyPos = (obj.obj as Creature).mainBodyChunk.pos;
                        if (obj.threat > 10 && Custom.DistLess(cloudfish.firstChunk.pos, enemyPos, 100 * (obj.obj.VisibilityBonus + 1)) && cloudfish.room.VisualContact(cloudfish.firstChunk.pos, enemyPos))
                        {
                            if (obj.obj.VisibilityBonus > -0.6)
                            {
                                newBehavior = Behavior.Flee;
                                Flee();
                            }
                        }
                        section = 31;
                        if (obj.obj is CloudFish otherCloudFish && newBehavior != Behavior.Flee && otherCloudFish.Consious && otherCloudFish.AI.behavior != Behavior.Hunt)
                        {
                            Vector2 otherPos = otherCloudFish.mainBodyChunk.pos;
                            if (Custom.DistLess(pos, otherPos, 400) && cloudfish.room.VisualContact(pos, otherPos))
                            {
                                CloudFishAbstractAI otherAI = otherCloudFish.AI.abstractAI;
                                if (otherAI.dominance > cloudfish.dominance)
                                {
                                    CloudFishAbstractAI.CloudFishFlock myFlock = abstractAI.flock;
                                    CloudFishAbstractAI.CloudFishFlock otherFlock = otherAI.flock;
                                    if ((myFlock.count < myFlock.maxCount && otherFlock.count < otherFlock.maxCount && otherFlock.count + myFlock.count <= otherFlock.maxCount) || abstractAI.flock.list.Contains(otherCloudFish.abstractCreature))
                                    {
                                        section = 32;
                                        if (newLeader == null || ((CloudFishAbstractAI)newLeader.abstractAI).dominance > otherAI.dominance || (newLeader.realizedCreature != null && (newLeader.realizedCreature as CloudFish).AI.behavior == Behavior.Hunt))
                                        {
                                            newLeader = otherCloudFish.abstractCreature;
                                            otherAI.flock.AddNewMember(cloudfish.abstractCreature);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (BodyChunk chunk in obj.obj.bodyChunks)
                        {
                            if (Custom.DistLess(pos, chunk.pos, chunk.rad + 100f))
                            {
                                nearbyChunks.Add(chunk);
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
                            newBehavior = Behavior.Flee;
                            Flee();
                        }
                    }
                }

                if (foodEaten < maxFood)
                {
                    if (room.insectCoordinator != null)
                    {
                        foreach (CosmeticInsect insect in room.insectCoordinator.allInsects)
                        {
                            if (room.VisualContact(pos, insect.pos) && insect.alive)
                            {
                                //Create_Square(room, insect.pos, 5f, 5f, Vector2.up, "Red", 0);
                                if (nearestInsect == null || nearestInsect.room != room || Custom.Dist(pos, insect.pos) < Custom.Dist(pos, nearestInsect.pos))
                                {
                                    nearestInsect = insect;
                                }
                            }
                        }
                    }
                }

            }

            if (newBehavior != Behavior.Flee && Random.value < 0.001f)
            {
                room.PlaySound(Enums.NewSoundID.RandomCloudFishWhistle(), cloudfish.firstChunk.pos, Random.Range(0.9f, 1.1f), Random.Range(0.8f, 1.2f));
            }
            else if (newBehavior == Behavior.Flee && Random.value < 0.01f)
            {
                room.PlaySound(Enums.NewSoundID.AA_CloudFishScream, cloudfish.firstChunk.pos, Random.Range(0.9f, 1.1f), Random.Range(0.8f, 1.2f));
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
                if (AverageDangerPos.HasValue && AverageDangerPos.Value != null)
                {
                    lastDangerPos = AverageDangerPos.Value;
                }
            }
            else
            {
                abstractAI.flock.Update();
            }

            section = 5;
            float timeUntilRain = rainTracker.rainCycle.TimeUntilRain;

            if (newBehavior == Behavior.Wander || newBehavior == Behavior.GoToDen)
            {
                if (newLeader != null)
                {
                    newBehavior = Behavior.Follow;
                }
                else if (timeUntilRain < 2000)
                {
                    newBehavior = Behavior.GoToDen;
                }
                else if (Random.value < 0.0001f && foodEaten < maxFood && nearestInsect != null && Custom.DistLess(pos, nearestInsect.pos, 400) && room.VisualContact(pos, nearestInsect.pos))
                {
                    newBehavior = Behavior.Hunt;
                }
            }

            section = 6;
            if (newBehavior == Behavior.Follow)
            {
                section = 6.1f;
                if (timeUntilRain > 2000 && Random.value < 0.0001f && foodEaten < maxFood && nearestInsect != null && Custom.DistLess(pos, nearestInsect.pos, 400))
                {
                    newBehavior = Behavior.Hunt;
                }
                else if (newLeader != null)
                {
                    CloudFishAbstractAI leaderAI = newLeader.abstractAI as CloudFishAbstractAI;
                    leaderAI.flock.AddNewMember(cloudfish.abstractCreature);

                    if (leaderAI.denPosition.HasValue)
                    {
                        abstractAI.denPosition = leaderAI.denPosition.Value;
                    }

                    if (newLeader.realizedCreature != null && newLeader.realizedCreature.room == room)
                    {
                        CloudFish leader = newLeader.realizedCreature as CloudFish;

                        if (!leader.Consious)
                        {
                            newBehavior = Behavior.Flee;
                            Flee();
                        }
                        else if (leader.AI.behavior == Behavior.Hunt)
                        {
                            newLeader = null;
                            newBehavior = Behavior.Wander;
                        }
                    }
                }
            }

            if (newBehavior == Behavior.Hunt)
            {
                if (huntTimer <= 0)
                {
                    huntTimer = Random.Range(100, 1000);
                }

                if (Custom.DistLess(pos, nearestInsect.pos, 10))
                {
                    room.PlaySound(SoundID.Slugcat_Bite_Fly, pos, 1f, 1f);
                    nearestInsect.alive = false;
                    nearestInsect.Destroy();
                    nearestInsect = null;
                    foodEaten++;
                }

                if (rainTracker.rainCycle.TimeUntilRain < 2000 || nearestInsect == null || Custom.Dist(pos, nearestInsect.pos) > 500 || !room.VisualContact(pos, nearestInsect.pos))
                {
                    if (newLeader != null)
                    {
                        newBehavior = Behavior.Follow;
                    }
                    else if (rainTracker.rainCycle.TimeUntilRain < 2000)
                    {
                        newBehavior = Behavior.GoToDen;
                    }
                    else
                    {
                        newBehavior = Behavior.Wander;
                    }
                }
            }
            else if (huntTimer > 0)
            {
                huntTimer--;
            }

            section = 7;

            abstractAI.leader = newLeader;
            abstractAI.behavior = newBehavior;
            abstractAI.dominance = cloudfish.dominance;
            myLeader = newLeader;
            behavior = newBehavior;
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "CLOUDFISH AI UPDATEBEHAVIOR", section);
        }
    }
    public void Flee()
    {
        Vector2 pos = cloudfish.firstChunk.pos;
        fleeCounter = 0;
        lastDangerRoom = cloudfish.room;

        whistleTimer = 0;

        foreach (TrackedObject obj in trackedCreatures)
        {
            if (obj.obj is CloudFish fish)
            {
                if (fish.room == cloudfish.room)
                {
                    Vector2 otherPos = fish.firstChunk.pos;
                    if (Custom.DistLess(pos, otherPos, 1000) && fish.room.VisualContact(pos, otherPos))
                    {
                        fish.AI.fleeCounter = 0;
                        fish.AI.lastDangerRoom = lastDangerRoom;
                        fish.AI.abstractAI.behavior = Behavior.Flee;
                    }
                }
            }
        }
    }


    public void UpdateMovement()
    {
        float section = 0;

        try
        {
            Room room = cloudfish.room;
            World world = room.world;
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
                            stayInRoomDesire = Random.Range(minRoomTime, maxRoomTime);
                            AbstractRoom mostAttractiveRoom = null;
                            WorldCoordinate? accessibleNode = null;

                            for (int i = 0; i < 20; i++)
                            {
                                AbstractRoom checkRoom = world.abstractRooms[Random.Range(0, world.abstractRooms.Length)];
                                if (!checkRoom.offScreenDen && checkRoom.AttractionValueForCreature(template.type) > 0.25)
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

                    section = 6;
                }
                else if (cell.generation == pathFinder.pathGeneration)
                {
                    movementType = MovementType.FollowPath;
                    section = 7;
                }

                //Create_Text(room, pos + new Vector2(0f, 40f), "Circle Timer: " + circleCounter, "Purple", 0);

                //Create_Text(room, pos + new Vector2(0f, 60f), "Leave Room Timer: " + stayInRoomDesire, "Red", 0);
            }
            else if (behavior == Behavior.GoToDen)
            {
                section = 20;
                speed = Mathf.Lerp(4f, 2.5f, rainTracker.rainCycle.TimeUntilRain / 2000);

                circleCounter = 0;

                if (!abstractAI.denPosition.HasValue)
                {
                    AbstractRoom abstractRoom = room.abstractRoom;
                    for (int i = 0; i < abstractRoom.nodes.Length - 1; i++)
                    {
                        AbstractRoomNode node = abstractRoom.nodes[i];
                        if (node.type == AbstractRoomNode.Type.Den)
                        {
                            abstractAI.denPosition = room.LocalCoordinateOfNode(i);
                            goto End;
                        }
                    }
                    if (!migrateDestination.HasValue || migrateDestination.Value.room == room.abstractRoom.index)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            AbstractRoom otherRoom = world.GetAbstractRoom(abstractRoom.connections[Random.Range(0, abstractRoom.connections.Length)]);
                            if (otherRoom.index != lastLastRoom || room.abstractRoom.connections.Length < 2)
                            {
                                migrateDestination = new(otherRoom.index, -1, -1, otherRoom.RandomRelevantNode(template));
                                break;
                            }
                        }
                    }
                }
                End:;

                if (abstractAI.denPosition.HasValue && abstractAI.denPosition.Value.room == room.abstractRoom.index)
                {
                    Vector2 denPos = room.MiddleOfTile(abstractAI.denPosition.Value);
                    //Create_Square(room, denPos, 10f, 10f, Custom.DegToVec(45), "Red", 0);
                    if (destination != denPos)
                    {
                        WorldCoordinate denCoord = room.GetWorldCoordinate(denPos);
                        pathFinder.AssignNewDestination(denCoord);
                    }
                }

                pathfinding = true;

                PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
                if (cell.generation != pathFinder.pathGeneration)
                {
                    if (largeNarrowSpace)
                    {
                        movementType = MovementType.WanderThroughTunnel;
                        section = 21;
                    }
                    else
                    {
                        movementType = MovementType.CircleInPlace;
                        section = 22;
                    }
                }
                else if (cloudfish.room.VisualContact(pos, destination))
                {
                    goToPoint = destination;
                    movementType = MovementType.GoStraightToPoint;
                    section = 23;

                    if (cell.generation == pathFinder.pathGeneration && map.getTerrainProximity(pos) < 3)
                    {
                        movementType = MovementType.FollowPath;
                        section = 24;
                    }

                    section = 25;
                }
                else if (cell.generation == pathFinder.pathGeneration)
                {
                    movementType = MovementType.FollowPath;
                    section = 26;
                }
            }
            else if (behavior == Behavior.Follow)
            {
                section = 30;
                circleCounter = 1000;

                if (movementType != MovementType.CircleInPlace)
                {
                    temporaryWanderDestination = coordPos;
                }

                if (myLeader != null)
                {
                    if (myLeader.realizedCreature != null || abstractAI.flock.leader.realizedCreature != null)
                    {
                        CloudFish realizedLeader = myLeader.realizedCreature != null ? myLeader.realizedCreature as CloudFish : abstractAI.flock.leader.realizedCreature as CloudFish;
                        abstractAI.destination = realizedLeader.coord;

                        if (realizedLeader.room != null)
                        {
                            if (realizedLeader.room == cloudfish.room)
                            {
                                section = 31;

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
                                if (!relativeSwarmPos.HasValue)
                                {
                                    relativeSwarmPos = Vector2.zero;
                                }

                                section = 32;

                                Vector2 actualSwarmPos = realizedLeader.firstChunk.pos + relativeSwarmPos.Value;
                                if (cloudfish.room.VisualContact(pos, actualSwarmPos))
                                {
                                    section = 32.1f;
                                    //Create_LineAndDot(room, pos, realizedLeader.mainBodyChunk.pos, "Green", 0);

                                    speed = Mathf.Clamp(Custom.Dist(pos, actualSwarmPos) / 50, 2f, 4f);
                                    if (Custom.DistLess(pos, actualSwarmPos, 20))
                                    { speed = 1f; }

                                    goToPoint = actualSwarmPos;
                                    movementType = MovementType.GoStraightToPoint;

                                    section = 32.2f;

                                    if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                    {
                                        pathFinder.AssignNewDestination(realizedLeader.coord);
                                    }
                                }
                                else
                                {
                                    section = 32.3f;

                                    speed = 3f;

                                    //Create_LineAndDot(room, pos, realizedLeader.mainBodyChunk.pos, "Red", 0);

                                    if (Custom.DistLess(pos, destination, 200) && cloudfish.room.VisualContact(pos, destination))
                                    {
                                        if (pathFinder.CoordinateReachable(realizedLeader.coord))
                                        {
                                            pathFinder.AssignNewDestination(realizedLeader.coord);
                                        }
                                    }

                                    section = 32.4f;

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
                                section = 33;

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
                            section = 34;

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
                        section = 35;
                        abstractAI.destination = myLeader.pos;

                        if (myLeader.InDen && myLeader.abstractAI.denPosition.HasValue)
                        {
                            WorldCoordinate denPos = myLeader.abstractAI.denPosition.Value;
                            if (pathFinder.destination != denPos)
                            {
                                pathFinder.AssignNewDestination(pathFinder.destination);
                            }
                        }

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
                else if (map.getTerrainProximity(pos) > 2)
                {
                    movementType = MovementType.CircleInPlace;
                }
                else
                {
                    movementType = MovementType.WanderThroughTunnel;
                }

                if (rainTracker.rainCycle.TimeUntilRain < 2000)
                {
                    speed = Mathf.Max(speed, Mathf.Lerp(4f, 2.5f, rainTracker.rainCycle.TimeUntilRain / 2000));
                }
            }
            else if (behavior == Behavior.Flee)
            {
                section = 36;
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
            else if (behavior == Behavior.Hunt)
            {
                goToPoint = nearestInsect.pos;
                speed = Mathf.Clamp(4f / Custom.Dist(pos, goToPoint), 2f, 4f);
                movementType = MovementType.GoStraightToPoint;
            }
            #endregion

            section = 40;

            #region Migration Control
            if (migrateDestination.HasValue && behavior != Behavior.Flee && migrateDestination.Value.room != -1)
            {
                section = 41;
                pathfinding = true;

                int cloudFishRoomIndex = cloudfish.room.abstractRoom.index;
                int destRoomIndex = migrateDestination.Value.room;
                AbstractRoom migrateRoom = world.GetAbstractRoom(destRoomIndex);

                if (destRoomIndex == cloudFishRoomIndex)
                {
                    section = 42;
                    if (migrateRoom.realizedRoom != null)
                    {
                        migrateDestination = null;
                    }
                }
                else
                {
                    section = 43;
                    abstractAI.destination = migrateDestination.Value;
                    AbstractRoom startRoom = cloudfish.abstractCreature.Room;

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
                    section = 44;
                    if (pathToDestination == null || resetPath)
                    {
                        WorldCoordinate absPos = cloudfish.abstractCreature.pos;

                        AbstractRoom absRoom = room.abstractRoom;
                        for (int i = 0; i < 5; i++)
                        {
                            WorldCoordinate randomNode = room.LocalCoordinateOfNode(absRoom.RandomRelevantNode(template));

                            WorldCoordinate startCoord = QuickConnectivity.DefineNodeOfLocalCoordinate(randomNode, world, cloudfish.Template);
                            WorldCoordinate destCoord = QuickConnectivity.DefineNodeOfLocalCoordinate(migrateDestination.Value, world, cloudfish.Template);

                            List<WorldCoordinate> path = AbstractSpacePathFinder.Path(world, startCoord, destCoord, template, null);
                            if (path != null)
                            {
                                foreach (WorldCoordinate coord in path)
                                {
                                    if (coord.room == startCoord.room)
                                    {
                                        pathToDestination = path;
                                        goto End;
                                    }
                                }
                            }
                        }
                        End:;
                    }

                    section = 45;
                    if (pathToDestination != null)
                    {
                        WorldCoordinate? travelNode = null;
                        foreach (WorldCoordinate node in pathToDestination)
                        {
                            if (node != null)
                            {
                                AbstractRoom roomOfNode = world.GetAbstractRoom(node.room);
                                if (roomOfNode.realizedRoom != null && roomOfNode.realizedRoom.shortcuts != null)
                                {
                                    WorldCoordinate actualCoordOfNode = roomOfNode.realizedRoom.LocalCoordinateOfNode(node.abstractNode);
                                    //Create_Dot(roomOfNode.realizedRoom, roomOfNode.realizedRoom.MiddleOfTile(actualCoordOfNode), "Green", 0);
                                    if (roomOfNode == cloudfish.room.abstractRoom && travelNode == null)
                                    {
                                        travelNode = actualCoordOfNode;
                                        if (pathFinder.destination != actualCoordOfNode)
                                        {
                                            pathFinder.AssignNewDestination(actualCoordOfNode);
                                            //Create_Square(room, pos, 10f, 10f, Vector2.up, "Red", 100);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            section = 50;

            #region Movement Control
            if (movementType == MovementType.StandStill)
            {
                section = 51;

                shakeTail = false;

                if (temporaryWanderDestination == null || Custom.Dist(pos, room.MiddleOfTile(temporaryWanderDestination)) > 200)
                {
                    temporaryWanderDestination = coordPos;
                }

                speed = 1f;
                moveDir = Vector2.Lerp(Custom.DirVec(pos, room.MiddleOfTile(temporaryWanderDestination)), moveDir, 0.99f);
            }
            else if (movementType == MovementType.FollowPath)
            {
                section = 52;

                shakeTail = true;

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
                    wantMoveDir = AvoidChunks(Custom.DirVec(pos, cloudfish.room.MiddleOfTile(connection3.destinationCoord)));
                    moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.1f);
                }
                else
                {
                    speed = 2f;
                    wantMoveDir = AvoidChunks(Custom.DirVec(pos, cloudfish.room.MiddleOfTile(connection1.destinationCoord)));
                    moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.8f);
                }

                if (connection1.type == MovementConnection.MovementType.ShortCut || connection1.type == MovementConnection.MovementType.NPCTransportation)
                {
                    cloudfish.enteringShortCut = connection1.StartTile;
                    cloudfish.NPCTransportationDestination = connection1.destinationCoord;
                    cloudfish.lastExitUsed = connection1.destinationCoord;
                }

                /*
                if (connection1 != default && connection2 != default && connection3 != default)
                {
                    Vector2 pos1 = cloudfish.room.MiddleOfTile(connection1.StartTile);
                    Vector2 pos2 = cloudfish.room.MiddleOfTile(connection1.DestTile);
                    Vector2 pos3 = cloudfish.room.MiddleOfTile(connection2.DestTile);
                    Vector2 pos4 = cloudfish.room.MiddleOfTile(connection3.DestTile);
                    Create_Square(cloudfish.room, Vector2.Lerp(pos1, pos2, 0.5f), 1f, Custom.Dist(pos1, pos2), Custom.DirVec(pos1, pos2), "White", 0);
                    Create_Square(cloudfish.room, Vector2.Lerp(pos2, pos3, 0.5f), 1f, Custom.Dist(pos2, pos3), Custom.DirVec(pos2, pos3), "White", 0);
                    Create_Square(cloudfish.room, Vector2.Lerp(pos3, pos4, 0.5f), 1f, Custom.Dist(pos3, pos4), Custom.DirVec(pos3, pos4), "White", 0);
                }*/

                //Create_LineAndDot(room, pos, destination, "Green", 0);
            }
            else if (movementType == MovementType.CircleInPlace)
            {
                section = 53;

                shakeTail = true;

                if (circleCounter > 0)
                { circleCounter--; }

                //List<BodyChunk> nearbyChunks = [];
                bool findNewDest = false;

                if (temporaryWanderDestination == null)
                {
                    findNewDest = true;
                }
                else
                {
                    Vector2 wanderPos = room.MiddleOfTile(temporaryWanderDestination);
                    if (Custom.Dist(pos, wanderPos) > 200 || !room.VisualContact(pos, wanderPos) || map.getTerrainProximity(wanderPos) < 6)
                    {
                        findNewDest = true;
                    }
                    else
                    {
                        findNewDest = PointTooCloseToChunk(wanderPos, 10f, 100f);
                    }
                }

                section = 53.1f;

                if (findNewDest)
                {
                    bool foundDestination = false;
                    for (int i = 0; i < 10; i++)
                    {
                        IntVector2 testPosInt = new(intPos.x + Random.Range(-5, 6), intPos.y + Random.Range(-5, 6));
                        Vector2 testPos = room.MiddleOfTile(testPosInt);
                        if (map.getAItile(testPos).acc != AItile.Accessibility.Solid && room.VisualContact(pos, testPos) && map.getTerrainProximity(testPos) >= 6)
                        {
                            bool tooCloseToChunk = PointTooCloseToChunk(testPos, 10f, 100f);

                            if (!tooCloseToChunk)
                            {
                                temporaryWanderDestination = cloudfish.room.GetWorldCoordinate(testPosInt);
                                foundDestination = true;
                                break;
                            }
                        }
                    }
                    if (!foundDestination)
                    {
                        temporaryWanderDestination = cloudfish.coord;
                    }
                }

                section = 53.2f;

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
                    Vector2 dirVec = AvoidChunks(Custom.rotateVectorDeg(Custom.DirVec(pos, rotationPos), 60f));
                    moveDir = Vector2.Lerp(moveDir, dirVec, 0.8f);
                }
                else
                {
                    Vector2 dirVec = AvoidChunks(Custom.DirVec(pos, rotationPos));
                    moveDir = Vector2.Lerp(moveDir, dirVec, 0.8f);
                }

                Vector2 destPos = cloudfish.room.MiddleOfTile(temporaryWanderDestination);

                section = 53.3f;

                //Create_LineAndDot(room, pos, destPos, "Purple", 0);
            }
            else if (movementType == MovementType.AvoidWalls)
            {
                section = 54;

                shakeTail = true;

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
                                Vector2 testPos2 = pos + testDir2 * 40;
                                if (map.getAItile(testPos2).acc != AItile.Accessibility.Solid && pathFinder.coveredArea.Includes(room.GetTilePosition(testPos2)))
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
                bool nearbyChunk = PointTooCloseToChunk(checkPos, 10f, 100f);
                if (nearbyChunk || room.GetTilePosition(checkPos) == null || room.GetTilePosition(pos) == null || map.getTerrainProximity(checkPos) < 3 || map.getTerrainProximity(pos) < 3)
                {
                    Vector2 bestDir = moveDir;
                    Vector2 testDir = moveDir;
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 testPos = pos + testDir * 40;
                        if (room.VisualContact(pos, testPos))
                        {
                            if (map.getTerrainProximity(testPos) >= 3 && !PointTooCloseToChunk(testPos, 10f, 100f))
                            {
                                bestDir = testDir;
                                break;
                            }
                            else if (map.getTerrainProximity(testPos) > map.getTerrainProximity(pos + bestDir * 40) || (nearbyChunk && Custom.Dist(testPos, AverageChunkPos) > Custom.Dist(checkPos, AverageChunkPos)))
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
            }
            else if (movementType == MovementType.GoStraightToPoint)
            {
                section = 55;

                shakeTail = true;
                Vector2 dirVec = AvoidChunks(Custom.DirVec(pos, goToPoint));

                if (Custom.Dist(pos, goToPoint) > 50)
                {
                    moveDir = Vector2.Lerp(moveDir, dirVec, 0.5f);
                }
                else
                {
                    moveDir = Vector2.Lerp(moveDir, dirVec, 0.1f);
                }
            }
            else if (movementType == MovementType.WanderThroughTunnel)
            {
                section = 56;

                shakeTail = false;

                IntVector2 forwardsDir = new(Mathf.RoundToInt(moveDir.x), Mathf.RoundToInt(moveDir.y));
                if (Mathf.Abs(forwardsDir.x) > 0 && Mathf.Abs(forwardsDir.y) > 0)
                {
                    forwardsDir.y = 0;
                }
                else if (forwardsDir.x == 0 && forwardsDir.y == 0)
                {
                    forwardsDir.y = 1;
                }

                IntVector2 backwardsDir = new(-forwardsDir.x, -forwardsDir.y);

                bool fleeFromEnemy = lastDangerRoom == room && Custom.DistLess(pos, lastDangerPos, 200) && room.VisualContact(pos, lastDangerPos);
                if (fleeFromEnemy)
                {
                    Vector2 dangerDir = Custom.DirVec(lastDangerPos, pos);
                    forwardsDir = new(Mathf.RoundToInt(dangerDir.x), Mathf.RoundToInt(dangerDir.y));
                }

                //Create_Square(room, room.MiddleOfTile(intPos), 5f, 5f, Custom.DegToVec(45), "Cyan", 0);

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
                    Skip:;
                    //Create_Square(room, room.MiddleOfTile(intPos + testDir), 5f, 5f, Custom.DegToVec(45), "Red", 0);
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

                cloudfish.firstChunk.vel += velDir * 2 - contactDir * 2;
            }


            Vector2 shakeTailDir = Vector2.zero;
            if (shakeTail)
            {
                Vector2 normalMoveDir = moveDir;
                //Create_LineBetweenTwoPoints(room, pos, pos + normalMoveDir * 20f, "Red", 0);

                if (shakeTailTimer > 80)
                { shakeTailTimer = 1; }
                else
                { shakeTailTimer++; }

                float shakeTailAmount = (Mathf.PingPong(shakeTailTimer, 20) - 10) * 10f;
                shakeTailDir = Custom.rotateVectorDeg(normalMoveDir, shakeTailAmount / 2f);
            }

            if (cloudfish.grabbedBy.Count == 0)
            {
                cloudfish.firstChunk.vel *= 0.8f;
                cloudfish.firstChunk.vel += (moveDir + shakeTailDir).normalized * speed;
            }
            else
            {
                cloudfish.firstChunk.vel *= 0.5f;
                cloudfish.firstChunk.vel += (moveDir + shakeTailDir).normalized * speed * 0.5f;
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "CLOUDFISH AI UPDATEBEHAVIOR", section);
        }
    }
    public bool PointTooCloseToChunk(Vector2 pos, float minRad, float extraRad)
    {
        if (nearbyChunks == null || cloudfish.room == null)
        {
            return false;
        }

        foreach (BodyChunk chunk in nearbyChunks)
        {
            if (chunk.rad > minRad && Custom.DistLess(pos, chunk.pos, chunk.rad + extraRad))
            {
                Room room = cloudfish.room;
                //Create_Square(room, chunk.pos, chunk.rad * 2f, chunk.rad * 2f, Vector2.up, "Green", 0);
                return true;
            }
        }

        return false;
    }
    public Vector2 AvoidChunks(Vector2 startDir)
    {
        Vector2 pos = cloudfish.firstChunk.pos;
        Room room = cloudfish.room;

        Vector2 bestDir = startDir;
        Vector2 testDir = startDir;
        for (int i = 0; i < 30; i++)
        {
            Vector2 testPos = pos + testDir * 40;
            if (room.VisualContact(pos, testPos))
            {
                if (!PointTooCloseToChunk(testPos, 10f, 100f))
                {
                    bestDir = testDir;
                    break;
                }
                else if (Custom.Dist(testPos, AverageChunkPos) > Custom.Dist(pos + bestDir * 40, AverageChunkPos))
                {
                    bestDir = testDir;
                }
            }
            testDir = Custom.rotateVectorDeg(testDir, Random.value > 0.5 ? 12 : -12);
        }

        return bestDir;
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
        public static Behavior Hunt = new("Hunt", true);
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
    public AbstractCreature color;
    public AbstractCreature leader;
    public AbstractCreature follower;
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
        dominance = parent.personality.dominance;

        flock = new CloudFishFlock(parent);
    }

    public override void Update(int time)
    {
        base.Update(time);

        #region Update_Behavior;

        if (leader != null || flock.leader != parent)
        {
            behavior = CloudFishAI.Behavior.Follow;
        }
        else
        {
            if (world.rainCycle.TimeUntilRain < 2000)
            {
                behavior = CloudFishAI.Behavior.GoToDen;
            }
            else
            {
                behavior = CloudFishAI.Behavior.Wander;
            }
        }

        #endregion;

        #region Update_Movement
            
        if (behavior == CloudFishAI.Behavior.Follow)
        {
            AbstractCreature newLeader = leader;
            foreach (AbstractCreature creature in flock.list)
            {
                CloudFishAbstractAI AI = creature.abstractAI as CloudFishAbstractAI;
                if (AI.dominance > dominance)
                {
                    if (newLeader == null || ((CloudFishAbstractAI)newLeader.abstractAI).dominance > AI.dominance)
                    {
                        newLeader = creature;
                    }
                }
            }
            leader = newLeader;

            flock.Update();

            if (flock.leader.realizedCreature != null)
            {
                SetDestination(leader.pos);
                destination = leader.pos;
            }
            else
            {
                SetDestination(flock.leader.pos);
                destination = flock.leader.pos;
            }

            if (parent.realizedCreature == null && flock.leader.realizedCreature == null)
            {
                parent.Move(flock.leader.pos);
            }
        }
        else if (behavior == CloudFishAI.Behavior.Wander || behavior == CloudFishAI.Behavior.GoToDen)
        {
            if (flock.count < flock.maxCount)
            {
                AssembleFlock();
            }
            else
            {
                flock.Update();
            }

            if (behavior == CloudFishAI.Behavior.GoToDen)
            {
                if (!denPosition.HasValue)
                {
                    for (int i = 0; i < parent.Room.nodes.Count(); i++)
                    {
                        AbstractRoomNode node = parent.Room.nodes[i];
                        if (node.type == AbstractRoomNode.Type.Den)
                        {
                            denPosition = new(parent.Room.index, -1, -1, i);
                            break;
                        }
                    }
                }

                if (denPosition.HasValue)
                {
                    SetDestination(denPosition.Value);
                }
                else if (destination.room == parent.Room.index)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        AbstractRoom otherRoom = world.GetAbstractRoom(parent.Room.connections[Random.Range(0, parent.Room.connections.Length)]);
                        if (otherRoom.index != lastRoom || parent.Room.connections.Length < 2)
                        {
                            SetDestination(new WorldCoordinate(otherRoom.index, -1, -1, otherRoom.RandomRelevantNode(parent.creatureTemplate)));
                            break;
                        }
                    }
                }
            }

            foreach (AbstractCreature creature in flock.list)
            {
                CloudFishAbstractAI AI = creature.abstractAI as CloudFishAbstractAI;
                if (AI.dominance > dominance)
                {
                    behavior = CloudFishAI.Behavior.Follow;
                }
                else if (AI.behavior == CloudFishAI.Behavior.Follow)
                {
                    AI.flock.Update();
                    AI.SetDestination(flock.leader.pos);
                    AI.destination = flock.leader.pos;
                    if (creature.realizedCreature == null && flock.leader.realizedCreature == null)
                    {
                        creature.Move(flock.leader.pos);
                    }
                }
            }
        }

        #endregion
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
