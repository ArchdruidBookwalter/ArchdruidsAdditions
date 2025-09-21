using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using UnityEngine.Rendering;

namespace ArchdruidsAdditions.Creatures;

public class Herring : Creature
{
    public HerringAI AI;

    public float dominance;

    public Herring(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new(this, 0, default, 5f, 0.05f);
        bodyChunkConnections = [];

        gravity = 0f;
        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        collisionLayer = 1;

        dominance = UnityEngine.Random.Range(0f, 20f);

        AI = new(abstractCreature, world);
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        AI.wantMovePos = firstChunk.pos;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (Consious)
        {
            Act();
            gravity = 0f;
            if (grabbedBy.Count > 0)
            {
                firstChunk.vel += Custom.RNV() * 10f;
            }
        }
        else
        {
            gravity = 0.9f;
            AI.behavior = HerringAI.Behavior.Flee;
            AI.fleeCounter = 0;
        }
    }
    public override void Die()
    {
        base.Die();
        room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, firstChunk.pos, 1f, 1.5f);
        foreach (HerringAI.TrackedObject obj in AI.trackedCreatures)
        {
            if (obj.obj is Herring herring && Custom.DistLess(firstChunk.pos, herring.firstChunk.pos, 1000))
            {
                herring.AI.behavior = HerringAI.Behavior.Flee;
                herring.AI.fleeCounter = 0;
            }
        }
    }
    public void Act()
    {
        AI.Update();
        if (AI.behavior == HerringAI.Behavior.Flee && UnityEngine.Random.value > 0.9f)
        {
            room.PlaySound(SoundID.Bat_Afraid_Flying_Sounds, firstChunk.pos, 1f, 1f);
        }
    }

    public override void InitiateGraphicsModule()
    {
        if (graphicsModule == null)
        {
            graphicsModule = new HerringGraphics(this);
        }
    }
}

public class HerringGraphics : GraphicsModule
{
    Herring Herring
    {
        get { return owner as Herring; }
    }
    public HerringGraphics(PhysicalObject ow) : base(ow, false)
    {

    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("pixel", true);

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 chunkPos = Vector2.Lerp(Herring.firstChunk.lastPos, Herring.firstChunk.pos, timeStacker);

        sLeaser.sprites[0].SetPosition(chunkPos - camPos);
        sLeaser.sprites[0].scale = 10f;
        sLeaser.sprites[0].color = new Color(1f, 0f, 0f);

        if (Herring.slatedForDeletetion || Herring.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
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
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
    }
}

public class HerringAI : ArtificialIntelligence
{
    public Herring Herring
    {
        get
        {
            return creature.realizedCreature as Herring;
        }
    }

    public Vector2 wantMovePos;
    public Vector2 wantMoveDir;
    public int fleeCounter;
    public float speed;

    public Behavior behavior;
    public List<TrackedObject> trackedCreatures;
    public List<TrackedObject> trackedObjects;
    public Herring leader;

    public Vector2 lastDangerPos;
    public Vector2? DangerPos
    {
        get
        {
            List<Vector2> dangerPositions = [];

            foreach (TrackedObject obj in trackedCreatures)
            {
                if (obj.threat > 0 && Custom.DistLess(Herring.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 1000))
                {
                    dangerPositions.Add((obj.obj as Creature).mainBodyChunk.pos);
                }
            }
            foreach (TrackedObject obj in trackedObjects)
            {
                if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown)
                {
                    dangerPositions.Add(weapon.firstChunk.pos);
                }
            }

            if (dangerPositions.Count == 0)
            { return null; }

            float averageX = 0;
            float averageY = 0;
            foreach (Vector2 dangerPos in dangerPositions)
            {
                averageX += dangerPos.x;
                averageY += dangerPos.y;
            }
            averageX /= dangerPositions.Count;
            averageY /= dangerPositions.Count;

            return new Vector2 (averageX, averageY);
        }
    }

    public HerringAI(AbstractCreature creature, World world) : base(creature, world)
    {
        this.creature = creature;
        wantMovePos = Herring.firstChunk.pos;
        speed = 1f;

        lastDangerPos = wantMovePos;

        behavior = Behavior.Wander;

        trackedCreatures = [];
        trackedObjects = [];
    }
    public override void Update()
    {
        base.Update();

        if (Herring != null)
        {
            if (UnityEngine.Random.value < 0.5)
            {
                UpdateTrackedObjects();
            }
            UpdateBehavior();
            UpdateMovement();
        }
    }
    public void UpdateTrackedObjects()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (PhysicalObject obj in Herring.room.physicalObjects[i])
            {
                if (obj != null && obj != Herring && Herring.room.VisualContact(Herring.firstChunk.pos, obj.firstChunk.pos))
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
                            if (newCreature is Herring || newCreature is Fly || newCreature is Overseer)
                            { threat = 0; }

                            trackedCreatures.Add(new TrackedObject(Herring, newCreature, 250, threat));
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
                        { trackedObjects.Add(new TrackedObject(Herring, obj, 100, 0)); }
                    }
                }
            }
        }
        
        for (int i = 0; i < trackedCreatures.Count - 1; i++)
        {
            Creature creature = trackedCreatures[i].obj as Creature;
            //Herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(Herring.room, creature.mainBodyChunk.pos, "Yellow", 10));
            if (Herring.room.VisualContact(Herring.firstChunk.pos, creature.mainBodyChunk.pos))
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
            if (Herring.room.VisualContact(Herring.firstChunk.pos, trackedObjects[i].obj.firstChunk.pos))
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
    public void UpdateMovement()
    {
        if (behavior == Behavior.Flee || behavior == Behavior.Wander)
        {
            Vector2 fleeDir = Custom.rotateVectorDeg(Herring.firstChunk.vel.normalized, UnityEngine.Random.Range(-1f, 1f));

            if (DangerPos.HasValue)
            {
                //fleeDir = Custom.DirVec(DangerPos.Value, Herring.firstChunk.pos);
                //Herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(Herring.room, DangerPos.Value, "Red", 100));
            }

            Vector2 fleePos = Herring.firstChunk.pos + fleeDir * 40f;
            if (!ValidIdlePos(fleePos))
            {
                Vector2 testPos1 = Herring.firstChunk.pos + Custom.PerpendicularVector(fleeDir) * 40f;
                Vector2 testPos2 = Herring.firstChunk.pos + Custom.PerpendicularVector(fleeDir) * -40f;
                if (ValidIdlePos(testPos1))
                {
                    fleePos = testPos1;
                }
                else if (ValidIdlePos(testPos2))
                {
                    fleePos = testPos2;
                }
                else
                {
                    fleePos = Herring.firstChunk.pos - fleeDir * 40f;
                }
            }

            wantMovePos = fleePos;
            speed = 2f;

            if (behavior == Behavior.Flee)
            {
                speed = 3f;
            }
        }
        else if (behavior == Behavior.Follow)
        {
            wantMovePos = leader.firstChunk.pos + Custom.RNV() * 40f;
            speed = 2f;
        }
        wantMoveDir = Custom.DirVec(Herring.firstChunk.pos, wantMovePos);
        Herring.firstChunk.vel *= 0.8f;
        Herring.firstChunk.vel += wantMoveDir * speed;

        if (Herring.firstChunk.ContactPoint.ToVector2().magnitude > 0.1f)
        {
            Herring.firstChunk.vel -= Herring.firstChunk.ContactPoint.ToVector2();
        }
        //Herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(Herring.room, wantMovePos, color, 0));
    }
    public void UpdateBehavior()
    {
        foreach (TrackedObject obj in trackedCreatures)
        {
            if (obj.threat > 0 && Custom.DistLess(Herring.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 20 * obj.threat))
            {
                behavior = Behavior.Flee;
                Herring.firstChunk.vel += Custom.DirVec(DangerPos.Value, Herring.firstChunk.pos) * 1f;
                fleeCounter = 0;
            }
            if (obj.obj is Herring herring)
            {
                if (herring.Consious && herring.dominance > Herring.dominance && (leader == null || leader.dominance > herring.dominance))
                {
                    leader = herring;
                }
            }
        }
        foreach (TrackedObject obj in trackedObjects)
        {
            if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(Herring.firstChunk.pos, weapon.firstChunk.pos, 50))
            {
                behavior = Behavior.Flee;
                fleeCounter = 0;
            }
        }

        if (behavior == Behavior.Flee)
        {
            fleeCounter++;
            if (fleeCounter > 100)
            {
                behavior = Behavior.Wander;
            }
        }

        if (behavior == Behavior.Wander)
        {
            if (leader != null)
            {
                behavior = Behavior.Follow;
            }
        }

        if (behavior == Behavior.Follow)
        {
            if (leader == null || !Herring.room.VisualContact(leader.firstChunk.pos, Herring.firstChunk.pos))
            {
                behavior = Behavior.Wander;
            }
            if (!leader.Consious)
            {
                behavior = Behavior.Flee;
                fleeCounter = 0;
            }
        }

        Herring.room.AddObject(new Objects.ColoredShapes.Text(Herring.room, Herring.firstChunk.pos, behavior.value, "Yellow", 0));
    }
    public bool ValidIdlePos(Vector2 testPos)
    {
        Room.Tile tile = Herring.room.GetTile(testPos);
        int terrainProximity = Herring.room.aimap.getTerrainProximity(testPos);

        if (tile.Solid || terrainProximity < 2 || !Herring.room.VisualContact(testPos, Herring.firstChunk.pos))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public class Behavior : ExtEnum<Behavior>
    {
        public Behavior(string value, bool register = false) : base(value, register)
        {
        }

        public static Behavior Wander = new Behavior("Wander", true);
        public static Behavior Flee = new Behavior("Flee", true);
        public static Behavior Follow = new Behavior("Follow", true);
    }
    public class TrackedObject
    {
        public Herring herring;
        public PhysicalObject obj;
        public int forgetTimer;
        public float threat;
        public TrackedObject(Herring herring, PhysicalObject obj, int forgetTimer, float threat)
        {
            this.herring = herring;
            this.obj = obj;
            this.forgetTimer = forgetTimer;
            this.threat = threat;
        }
    }
}
