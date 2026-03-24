using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using static ArchdruidsAdditions.Methods.Methods;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

public class Parasite : AirBreatherCreature, IPlayerEdible
{
    public ParasiteAI AI;

    public ParasiteState ParasiteState
    {
        get
        {
            return abstractCreature.state as ParasiteState;
        }
    }

    public Rope[] connectionRopes;
    public float extraSegmentLength;

    public IntVector2 lastAirPos;

    public int bites;
    public bool eaten;

    public bool buryOnSpawn;
    public int burySpawnAttempts;

    public float burrow;
    public BodyChunk buriedInChunk;
    public override float VisibilityBonus
    {
        get
        {
            if (buriedInChunk != null)
            {
                return -1f;
            }
            return base.VisibilityBonus;
        }
    }

    public Parasite(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        extraSegmentLength = 2f;

        bodyChunks = new BodyChunk[5];
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            bodyChunks[i] = new(this, 0, default, 2f, 0.05f);
        }

        int connectionIndex = 0;
        bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            for (int j = i + 1; j < bodyChunks.Length; j++)
            {
                bodyChunkConnections[connectionIndex] = new(bodyChunks[i], bodyChunks[j], bodyChunks[i].rad + bodyChunks[j].rad + extraSegmentLength, BodyChunkConnection.Type.Push, 1f, -1f);
                connectionIndex++;
            }
        }

        gravity = 0.9f;
        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        collisionLayer = 1;

        bites = 3;

        if (ParasiteState.creatureAttachedTo.HasValue)
        {
            //Debug.Log("");
            //Debug.Log("PARASITE BURIED ON SPAWN!");
            //Debug.Log("");
            buryOnSpawn = true;
        }
    }

    public override void InitiateGraphicsModule()
    {
        if (graphicsModule == null)
        {
            graphicsModule = new ParasiteGraphics(this);
        }
    }
    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);

        connectionRopes = new Rope[bodyChunks.Length - 1];
        for (int i = 0; i < connectionRopes.Length; i++)
        {
            connectionRopes[i] = new(newRoom, bodyChunks[i].pos, bodyChunks[i + 1].pos, 1f);
        }
    }
    public override void Update(bool eu)
    {
        base.Update(eu);

        if (buryOnSpawn)
        {
            if (abstractPhysicalObject.stuckObjects.Count == 0)
            {
                List<AbstractCreature> creatures = room.abstractRoom.creatures;
                for (int i = 0; i < creatures.Count; i++)
                {
                    if (creatures[i].ID == ParasiteState.creatureAttachedTo.Value && creatures[i].realizedCreature != null)
                    {
                        BuryIntoChunk(creatures[i].realizedCreature.bodyChunks[ParasiteState.chunk], true);
                    }
                }
            }
            else
            {
                AbstractParasiteStick abstractStick = abstractPhysicalObject.stuckObjects[0] as AbstractParasiteStick;
                AbstractPhysicalObject stuckObj = abstractStick.B;
                if (stuckObj.Room == room.abstractRoom && stuckObj.realizedObject != null)
                {
                    BuryIntoChunk(stuckObj.realizedObject.bodyChunks[abstractStick.chunk], false);
                    burrow = 5.5f;
                    buryOnSpawn = false;
                }
            }

            burySpawnAttempts++;
            if (burySpawnAttempts >= 40)
            {
                buryOnSpawn = false;
            }
        }

        if (buriedInChunk == null)
        {
            CollideWithObjects = true;

            for (int i = 0; i < connectionRopes.Length; i++)
            {
                ConnectSegments(i, i + 1);
            }
            for (int i = connectionRopes.Length; i > 0; i--)
            {
                ConnectSegments(i, i - 1);
            }

            if (grasps[0] != null)
            {
                if (grasps[0].grabbed is not Creature otherCreature)
                {
                    LoseAllGrasps();
                }
                else
                {
                    bodyChunks[0].pos = grasps[0].grabbedChunk.pos;
                    bodyChunks[0].vel = grasps[0].grabbedChunk.vel;

                    burrow += 0.5f;

                    if (Random.value < 0.1)
                    {
                        WaterDrip drip = new(bodyChunks[0].pos, Custom.RNV() * 5, false);
                        room.AddObject(drip);

                        room.PlaySound(SoundID.Slugcat_Eat_Swarmer, bodyChunks[0], false, 1f, 1f);
                    }

                    if (burrow > 5)
                    {
                        BuryIntoChunk(otherCreature.bodyChunks[grasps[0].chunkGrabbed], true);
                    }
                }

                foreach (BodyChunkConnection connection in bodyChunkConnections)
                {
                    connection.active = false;
                }
            }
            else
            {
                burrow = 0;
                foreach (BodyChunkConnection connection in bodyChunkConnections)
                {
                    connection.active = true;
                }
            }

            if (room != null)
            {
                bool pushOutOfTerrain = false;

                Room.Tile tile = room.GetTile(mainBodyChunk.pos);
                IntVector2 tilePos = new(tile.X, tile.Y);

                if (!tile.Solid)
                {
                    lastAirPos = tilePos;
                }
                else if (lastAirPos != null)
                {
                    if (Custom.Dist(room.MiddleOfTile(lastAirPos), mainBodyChunk.pos) > 20)
                    {
                        pushOutOfTerrain = true;

                        for (int i = 0; i < bodyChunks.Length; i++)
                        {
                            bodyChunks[i].HardSetPosition(room.MiddleOfTile(lastAirPos) + Custom.RNV());
                        }
                    }
                }

                if (lastAirPos != null)
                {
                    Vector2 airPos = room.MiddleOfTile(lastAirPos);
                    Create_Square(room, airPos, 2f, 2f, Vec(45), pushOutOfTerrain ? "Red" : "Green", 0);
                }
            }

            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i].vel = Vector2.ClampMagnitude(bodyChunks[i].vel, 100);
            }

            if (!buryOnSpawn)
            {
                AI.Update();
            }
        }
        else
        {
            CollideWithObjects = false;

            foreach (BodyChunkConnection connection in bodyChunkConnections)
            {
                connection.active = false;
            }

            AbstractPhysicalObject attachedObject = abstractPhysicalObject.stuckObjects[0].B;

            if (attachedObject.Room.realizedRoom != null && attachedObject.realizedObject != null)
            {
                Vector2 buryPos = buriedInChunk.owner is Player player ? (player.graphicsModule as PlayerGraphics).drawPositions[buriedInChunk.index, 0] : buriedInChunk.pos;
                foreach (BodyChunk chunk in bodyChunks)
                {
                    chunk.pos = buryPos;
                    chunk.vel = buriedInChunk.vel;
                }
            }
        }

        if (room != null)
        {
            Create_Text(room, firstChunk.pos, "BURIED: " + (buriedInChunk == null).ToString(), "Red", 0);
        }
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);

        if (buriedInChunk == null && grasps[0] == null && otherObject is Creature creature && AI.jumpCooldown < 50 && myChunk == 0 && firstChunk.ContactPoint.ToVector2().magnitude < 0.1f)
        {
            BiteOtherCreature(creature, creature.bodyChunks[otherChunk]);
        }
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
    }

    public void ConnectSegments(int indexA, int indexB)
    {
        Rope rope = indexA < indexB ? connectionRopes[indexA] : connectionRopes[indexB];
        BodyChunk chunk1 = bodyChunks[indexA];
        BodyChunk chunk2 = bodyChunks[indexB];

        rope.Update(chunk1.pos, chunk2.pos);

        float ropeLength = rope.totalLength;
        float segmentLength = Mathf.Clamp(chunk1.rad + chunk2.rad + extraSegmentLength - burrow, 0, 500);


        if (ropeLength > segmentLength)
        {
            Vector2 dirVec = Custom.DirVec(chunk1.pos, chunk2.pos);
            Vector2 newDir = dirVec * (ropeLength - segmentLength) * 0.5f;

            chunk1.pos += newDir;
            chunk1.vel += newDir;
            chunk2.pos -= newDir;
            chunk2.vel -= newDir;
        }
    }
    public void BuryIntoChunk(BodyChunk chunk, bool createAbstractStick)
    {
        buriedInChunk = chunk;

        ParasiteState.creatureAttachedTo = chunk.owner.abstractPhysicalObject.ID;
        ParasiteState.chunk = chunk.index;

        if (createAbstractStick)
        {
            new AbstractParasiteStick(abstractPhysicalObject, buriedInChunk.owner.abstractPhysicalObject, chunk.index, ParasiteState.growth);
        }

        LoseAllGrasps();
    }
    public void BiteOtherCreature(Creature creature, BodyChunk chunk)
    {
        room.PlaySound(SoundID.Spear_Stick_In_Creature, chunk, false, 1f, 1f);

        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            WaterDrip drip = new(bodyChunks[0].pos, Custom.RNV() * 5, false);
            room.AddObject(drip);
        }

        Grab(creature, 0, chunk.index, Grasp.Shareability.NonExclusive, 0f, false, false);

        creature.Violence(bodyChunks[0], Vector2.zero, chunk, null, DamageType.Bite, 0.1f, 50f);
    }

    #region Edible Stuff
    public int BitesLeft { get { return bites; } }
    public int FoodPoints { get { return 1; } }
    public bool Edible
    {
        get
        {
            if (dead)
            { return true; }
            return false;
        }
    }
    public bool AutomaticPickUp { get { return false; } }

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;

        if (!dead)
        { Die(); }

        room.PlaySound(SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos);

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
}

public class ParasiteGraphics : GraphicsModule
{
    public Parasite parasite;

    public TriangleMesh bodyMesh;
    public CircularSprite[] circles;

    public Vector2 rotation, lastRotation;
    public bool wiggle;
    public int wiggleChunkIndex = 0;
    public int wiggleTimer = 0;

    public ParasiteGraphics(PhysicalObject ow) : base(ow, false)
    {
        parasite = ow as Parasite;
        List<BodyPart> list = [];
    }

    public override void Update()
    {
        lastRotation = rotation;

        base.Update();

        float section = 1;
        try
        {
            if (parasite.buriedInChunk == null)
            {

                int nextChunkTime = 5;
                if (parasite.AI.crawlUpStep || parasite.AI.jumpCooldown > 50)
                {
                }

                if (wiggleTimer > nextChunkTime)
                {
                    wiggleChunkIndex++;
                    wiggleTimer = 0;

                    if (wiggleChunkIndex == parasite.bodyChunks.Length)
                    { wiggleChunkIndex = 0; }
                }
                else
                {
                    if (wiggleTimer < 5)
                    {
                        BodyChunk wiggleChunk = parasite.bodyChunks[wiggleChunkIndex];
                        wiggleChunk.vel.y += 1f;
                    }
                    wiggleTimer++;
                }
            }
        }
        catch (Exception e)
        {
            Log_Exception(e, "PARASITE GRAPHICS UPDATE", section);
        }

        rotation = Custom.DirVec(parasite.bodyChunks[1].pos, parasite.bodyChunks[0].pos);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        bodyMesh = TriangleMesh.MakeLongMesh(parasite.bodyChunks.Length, false, false);
        sprites.Add(bodyMesh);

        circles = new CircularSprite[parasite.bodyChunks.Length - 1];
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = new("Futile_White")
            {
                scale = 0.2f
            };
            sprites.Add(circles[i]);
        }

        sLeaser.sprites = sprites.ToArray();

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float section = 0;
        try
        {
            if (parasite.slatedForDeletetion || parasite.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
            else
            {
                if (parasite.buriedInChunk == null)
                {
                    Vector2 headRot = Vector2.Lerp(lastRotation, rotation, timeStacker).normalized;
                    Vector2 perpHeadRot = Custom.PerpendicularVector(headRot);

                    List<Vector2> bodyPositions = [];
                    for (int i = 0; i < parasite.bodyChunks.Length; i++)
                    { bodyPositions.Add(Vector2.Lerp(parasite.bodyChunks[i].lastPos, parasite.bodyChunks[i].pos, timeStacker) - camPos); }

                    List<Vector2> meshPositions = [];

                    Vector2 lastBodyPos = bodyPositions[0];
                    float lastRad = parasite.bodyChunks[0].rad / 2;
                    for (int i = 0; i < bodyPositions.Count; i++)
                    {
                        Vector2 thisBodyPos = bodyPositions[i];

                        Vector2 normalized = (thisBodyPos - lastBodyPos).normalized;
                        Vector2 perpNorm = Custom.PerpendicularVector(normalized);
                        float dist = Vector2.Distance(thisBodyPos, lastBodyPos) / 5f;
                        float rad = parasite.bodyChunks[i].rad / 2;

                        meshPositions.Add(lastBodyPos - perpNorm * (rad + lastRad) * 0.5f + normalized * dist);
                        meshPositions.Add(lastBodyPos + perpNorm * (rad + lastRad) * 0.5f + normalized * dist);
                        meshPositions.Add(thisBodyPos - perpNorm * rad - normalized * dist);
                        meshPositions.Add(thisBodyPos + perpNorm * rad - normalized * dist);

                        lastBodyPos = thisBodyPos;
                        lastRad = rad;
                    }

                    while (meshPositions.Count < bodyMesh.vertices.Length)
                    {
                        meshPositions.Add(bodyPositions[bodyPositions.Count - 1]);
                    }

                    for (int i = 0; i < bodyMesh.vertices.Length; i++)
                    {
                        bodyMesh.MoveVertice(i, meshPositions[i]);
                    }
                    bodyMesh.alpha = 1f;
                }
                else
                {
                    bodyMesh.alpha = 0f;
                }
            }
        }
        catch (Exception e)
        {
            Log_Exception(e, "DRAWSPRITES", section);
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
    }
}

public class ParasiteAI : ArtificialIntelligence, IUseARelationshipTracker
{
    public Parasite parasite;

    public Behavior behavior;
    public Tracker.CreatureRepresentation prey;

    public int moveDir = 1;
    public bool crawlUpStep;
    public int jumpCooldown = 0;
    public Vector2 jumpPos;

    public ParasiteAI(AbstractCreature creature, World world) : base(creature, world)
    {
        parasite = creature.realizedCreature as Parasite;
        parasite.AI = this;

        AddModule(new StandardPather(this, world, creature));
        AddModule(new Tracker(this, 10, 10, -1, 0.35f, 5, 5, 10, false));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new PreyTracker(this, 5, 2f, 3f, 70f, 0.5f));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new UtilityComparer(this));
        pathFinder.stepsPerFrame = 50;

        utilityComparer.AddComparedModule(preyTracker, null, 0.9f, 1.1f);
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);
    }

    public override void Update()
    {
        base.Update();

        float section = 1;
        try
        {
            AIModule highestModule = utilityComparer.HighestUtilityModule();
            float highestModuleAmount = utilityComparer.HighestUtility();

            section = 2;

            if (highestModule != null)
            {
                List<string> lines = [];
                lines.Add(highestModule.GetType().Name);

                section = 2.1f;

                if (highestModule is PreyTracker)
                {
                    behavior = Behavior.Hunt;

                    lines.Add("TRACKED CREATURES: " + tracker.creatures.Count.ToString());
                    if (tracker.creatures.Count > 0)
                    {
                        lines.Add("   FORGET: " + tracker.creatures[0].forgetCounter.ToString());
                    }
                    lines.Add("TRACKED PREY: " + preyTracker.prey.Count.ToString());

                    section = 2.2f;

                    Tracker.CreatureRepresentation prey = preyTracker.MostAttractivePrey;
                    if (prey != null)
                    {
                        this.prey = prey;

                        Vector2 preyPos = prey.VisualContact && prey.representedCreature.realizedCreature != null ? prey.representedCreature.realizedCreature.mainBodyChunk.pos : parasite.room.MiddleOfTile(prey.BestGuessForPosition());

                        if (pathFinder.destination != prey.BestGuessForPosition())
                        {
                            pathFinder.AssignNewDestination(prey.BestGuessForPosition());

                            Create_Square(parasite.room, parasite.room.MiddleOfTile(prey.BestGuessForPosition()), 10f, 10f, Custom.DegToVec(0), "Yellow", 200);
                        }
                    }
                }

                //Create_TextBlock(parasite.room, parasite.firstChunk.pos, lines.ToArray(), "Red", 0);
            }

            MovementUpdate();
        }
        catch (Exception e)
        {
            Log_Exception(e, "PARASITE AI UPDATE", section);
        }
    }

    public void MovementUpdate()
    {
        Room room = parasite.room;
        World world = room.world;
        AImap map = room.aimap;
        Vector2 pos = parasite.bodyChunks[0].pos;
        Vector2 tailPos = parasite.bodyChunks[parasite.bodyChunks.Length - 1].pos;
        Vector2 destination = room.MiddleOfTile(pathFinder.destination);
        IntVector2 intPos = room.GetTilePosition(pos);
        IntVector2 intTailPos = room.GetTilePosition(tailPos);

        WorldCoordinate coordPos = creature.pos;
        Create_Square(room, room.MiddleOfTile(coordPos), 10f, 10f, Vec(45), "Red", 0);

        if (destination != null)
        {
            Create_Square(parasite.room, destination, 10f, 10f, Vec(45), "White", 0);
        }

        bool goToDestination = false;
        float moveSpeed = 2f;

        if (behavior == Behavior.Hunt)
        {
            Tracker.CreatureRepresentation prey = preyTracker.MostAttractivePrey;
            if (prey != null)
            {
                Vector2 preyPos = prey.VisualContact && prey.representedCreature.realizedCreature != null ? prey.representedCreature.realizedCreature.mainBodyChunk.pos : parasite.room.MiddleOfTile(prey.BestGuessForPosition());

                if (Custom.Dist(preyPos, destination) > 100)
                {
                    pathFinder.AssignNewDestination(room.GetWorldCoordinate(preyPos));
                }

                if (Custom.Dist(preyPos, pos) > 50 || !room.VisualContact(preyPos, pos))
                {
                    goToDestination = true;
                }
                else if (parasite.grasps[0] == null && jumpCooldown == 0)
                {
                    jumpCooldown = 100;
                    jumpPos = preyPos;
                }
            }
        }

        if (jumpCooldown > 0)
        {
            Vector2 jumpDir = Custom.DirVec(pos, jumpPos);

            if (jumpCooldown > 50)
            {
                foreach (BodyChunk chunk in parasite.bodyChunks)
                {
                    chunk.vel += Custom.rotateVectorDeg(jumpDir, Random.value < 0.5 ? 90 : -90) * 1;
                }
            }
            else if (jumpCooldown == 50)
            {
                parasite.mainBodyChunk.vel += jumpDir * 50f;
                room.PlaySound(SoundID.Slugcat_Throw_Misc_Inanimate, parasite.mainBodyChunk, false, 1f, 1f);
            }
            jumpCooldown--;
        }
        else if (goToDestination)
        {
            PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(coordPos);
            if (cell.generation == pathFinder.pathGeneration)
            {
                MovementConnection connection1 = (pathFinder as StandardPather).FollowPath(coordPos, false);

                if (connection1 != null)
                {
                    Vector2 pathPos1 = parasite.room.MiddleOfTile(connection1.StartTile);
                    Vector2 pathPos2 = parasite.room.MiddleOfTile(connection1.DestTile);

                    Create_LineBetweenTwoPoints(parasite.room, pathPos1, pathPos2, "Red", 0);

                    Vector2 connectionDir1 = Custom.DirVec(pathPos1, pathPos2);

                    if (Mathf.Abs(connectionDir1.x) > 0.1f)
                    {
                        moveDir = connectionDir1.x > 0.5f ? 1 : -1;
                    }

                    if (parasite.mainBodyChunk.ContactPoint.x == moveDir)
                    {
                        parasite.mainBodyChunk.vel.y += 18f;
                    }
                }
            }

            crawlUpStep = false;
            if (!room.GetTile(intPos).Solid &&
                room.GetTile(intPos).Terrain != Room.Tile.TerrainType.Slope &&
                room.GetTile(new IntVector2(intPos.x, intPos.y - 1)).Solid &&
                room.GetTile(new IntVector2(intPos.x + moveDir, intPos.y)).Solid)
            {
                crawlUpStep = true;
            }
            else if (!room.GetTile(intPos).Solid &&
                room.GetTile(intPos).Terrain != Room.Tile.TerrainType.Slope &&
                room.GetTile(new IntVector2(intTailPos.x, intTailPos.y - 1)).Solid &&
                room.GetTile(new IntVector2(intTailPos.x + moveDir, intTailPos.y)).Solid)
            {
                crawlUpStep = true;
            }

            if (crawlUpStep)
            {
                Create_Square(room, pos, 20f, 20f, Vec(45), "Purple", 0);
                moveSpeed = 5;
            }

            foreach (IntVector2 dir in Custom.eightDirections)
            {
                IntVector2 testPos = room.GetTilePosition(pos) + dir;
                if (room.GetTile(testPos).Solid)
                {
                    parasite.mainBodyChunk.vel.x += moveDir * moveSpeed;
                    break;
                }
            }
        }
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        if (StaticRelationship(otherCreature).type == CreatureTemplate.Relationship.Type.Eats || StaticRelationship(otherCreature).type == CreatureTemplate.Relationship.Type.Attacks)
        {
            return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 5);
        }
        else
        {
            return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, true);
        }
    }

    public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
        {
            return preyTracker;
        }
        return null;
    }

    public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        AbstractCreature creature = dRelation.trackerRep.representedCreature;
        CreatureTemplate.Relationship relationship = StaticRelationship(creature);

        //Debug.Log(creature.creatureTemplate.name + " : " + relationship.type.value);

        return relationship;
    }

    public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return null;
    }

    public class Behavior : ExtEnum<Behavior>
    {
        public Behavior(string value, bool register = false) : base(value, register)
        {
        }

        public static Behavior Wander = new("Wander", true);
        public static Behavior Flee = new("Flee", true);
        public static Behavior Hunt = new("Hunt", true);
    }
}

public class ParasiteState : CreatureState
{
    public EntityID? creatureAttachedTo;
    public int chunk;
    public int growth;
    public ParasiteState(AbstractCreature creature) : base(creature)
    {
        creatureAttachedTo = null;
        chunk = 0;
        growth = 0;
    }
    public override string ToString()
    {
        string dataString = base.ToString();

        if (!creatureAttachedTo.HasValue)
        {
            return dataString;
        }
        else
        {
            dataString += string.Format(CultureInfo.InvariantCulture, "CreatureAttachedTo<cC>{0}<cB>", creatureAttachedTo.Value.ToString());
            dataString += string.Format(CultureInfo.InvariantCulture, "Chunk<cC>{0}<cB>", chunk.ToString());
            dataString += string.Format(CultureInfo.InvariantCulture, "Growth<cC>{0}<cB>", growth.ToString());
        }

        return dataString;
    }
    public override void LoadFromString(string[] s)
    {
        base.LoadFromString(s);

        for (int i = 0; i < s.Length; i++)
        {
            string[] dataString = Regex.Split(s[i], "<cC>");
            if (dataString[0] != null)
            {
                if (dataString[0] == "CreatureAttachedTo")
                {
                    creatureAttachedTo = EntityID.FromString(dataString[1]);
                }
                else if (dataString[0] == "Chunk")
                {
                    chunk = int.Parse(dataString[1]);
                }
                else if (dataString[0] == "Growth")
                {
                    growth = int.Parse(dataString[1]);
                }
            }
        }
    }
}

public class AbstractParasiteStick : AbstractPhysicalObject.AbstractObjectStick
{
    public int chunk;
    public int growth;
    public AbstractParasiteStick(AbstractPhysicalObject parasite, AbstractPhysicalObject otherCreature, int chunk, int growth) : base(parasite, otherCreature)
    {
        this.chunk = chunk;
        this.growth = growth;

        AbstractCreature p = parasite as AbstractCreature;
        AbstractCreature o = otherCreature as AbstractCreature;

        //Debug.Log("");
        //Debug.Log("ABSTRACTPARASITESTICK CREATED. CREATURES: " + p.creatureTemplate.name + " : " + p.ID.ToString() + " - " + o.creatureTemplate.name + " : " + o.ID.ToString());
        //Debug.Log("");
    }

    public override string SaveToString(int roomIndex)
    {
        return string.Concat(
        [
            roomIndex.ToString(),
            "<stkA>paraStk<stkA>",
            A.ID.ToString(),
            "<stkA>",
            B.ID.ToString(),
            "<stkA>",
            chunk.ToString(),
            "<stkA>",
            growth.ToString()
        ]);
    }
}
