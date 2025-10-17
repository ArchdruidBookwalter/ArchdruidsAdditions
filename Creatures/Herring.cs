using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using static MonoMod.RuntimeDetour.Platforms.DetourNativeMonoPosixPlatform;
using MonoMod;

namespace ArchdruidsAdditions.Creatures;

public class Herring : Creature, IPlayerEdible
{
    public HerringAI AI;

    public Vector2 rotation;
    public Vector2 lastRotation;
    public float dominance;
    public int bites;
    public bool eaten;

    public int BitesLeft { get { return bites; } }
    public int FoodPoints { get { return 1; } }
    public bool Edible { get { return true; } }
    public bool AutomaticPickUp { get { return true; } }

    public Herring(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new(this, 0, default, 1f, 0.05f);
        bodyChunkConnections = [];

        gravity = 0.1f;
        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        collisionLayer = 1;

        GoThroughFloors = true;
        dominance = Random.Range(0f, 20f);
        bites = 3;
        eaten = false;
        rotation = new(1, 0);

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

        lastRotation = rotation;

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
        }
        else
        {
            gravity = 0.9f;
            AI.behavior = HerringAI.Behavior.Flee;
            AI.fleeCounter = 0;
        }

        if (grabbedBy.Count > 0 && !dead)
        {
            firstChunk.vel += Custom.DirVec(grabbedBy[0].grabber.mainBodyChunk.pos, firstChunk.pos) * 10f;
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

        rotation = Vector2.Lerp(rotation.normalized, firstChunk.vel.normalized, 0.3f);
    }
    public override void Grabbed(Grasp grasp)
    {
        if (!dead)
        { room.PlaySound(SoundID.Fly_Caught, firstChunk.pos, 1f, 1.5f); }
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

        if (AI != null && AI.pathFinder.visualizePath)
        {
            foreach (UpdatableAndDeletable updel in newRoom.updateList)
            {
                if (updel is FollowPathVisualizer visualizer && visualizer.pathFinder == AI.pathFinder)
                {
                    visualizer.Destroy();
                }
            }
            newRoom.AddObject(new FollowPathVisualizer(AI.pathFinder));
        }
    }
    public override void Die()
    {
        base.Die();
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
        if (AI.behavior == HerringAI.Behavior.Flee && Random.value > 0.9f)
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
    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;

        if (!dead)
        { Die(); }

        if (bites == 0) { room.PlaySound(SoundID.Slugcat_Final_Bite_Fly, firstChunk.pos); }
        else { room.PlaySound(SoundID.Slugcat_Bite_Fly, firstChunk.pos); }

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
}

public class HerringGraphics : GraphicsModule
{
    public Herring herring;

    public TriangleMesh bodyMech;

    public TailSegment[] tail;

    public HerringGraphics(PhysicalObject ow) : base(ow, false)
    {
        herring = ow as Herring;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        TriangleMesh mesh = TriangleMesh.MakeLongMesh(2, true, false);
        sLeaser.sprites[0] = mesh;

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 chunkPos = Vector2.Lerp(herring.firstChunk.lastPos, herring.firstChunk.pos, timeStacker) - camPos;
        Vector2 rotation = Vector3.Slerp(herring.lastRotation, herring.rotation, timeStacker);

        Vector2 bodyPos0 = chunkPos + rotation * 5f + Custom.PerpendicularVector(rotation) * 2f;
        Vector2 bodyPos1 = chunkPos + rotation * 5f - Custom.PerpendicularVector(rotation) * 2f;
        Vector2 bodyPos2 = chunkPos + Custom.PerpendicularVector(rotation) * 4f;
        Vector2 bodyPos3 = chunkPos - Custom.PerpendicularVector(rotation) * 4f;
        Vector2 bodyPos4 = chunkPos - rotation * 10f + Custom.PerpendicularVector(rotation) * 3f;
        Vector2 bodyPos5 = chunkPos - rotation * 10f - Custom.PerpendicularVector(rotation) * 3f;
        Vector2 bodyPos6 = chunkPos - rotation * 15f;

        bodyMech = sLeaser.sprites[0] as TriangleMesh;
        bodyMech.MoveVertice(0, bodyPos0);
        bodyMech.MoveVertice(1, bodyPos1);
        bodyMech.MoveVertice(2, bodyPos2);
        bodyMech.MoveVertice(3, bodyPos3);
        bodyMech.MoveVertice(4, bodyPos4);
        bodyMech.MoveVertice(5, bodyPos5);
        bodyMech.MoveVertice(6, bodyPos6);
        bodyMech.color = new Color(1f, 0f, 0f);
        bodyMech.alpha = 1f;

        if (herring.slatedForDeletetion || herring.room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
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

public class HerringAI : ArtificialIntelligence
{
    public Herring herring;

    public Vector2 moveDir;
    public int fleeCounter;
    public float speed;
    public float leaveRoomDesire;

    public bool pathfinding;

    public Behavior behavior;
    public List<TrackedObject> trackedCreatures;
    public List<TrackedObject> trackedObjects;
    public Herring leader;

    public Vector2? DangerPos
    {
        get
        {
            List<Vector2> dangerPositions = [];

            foreach (TrackedObject obj in trackedCreatures)
            {
                if (obj.threat > 0 && Custom.DistLess(herring.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 200))
                {
                    dangerPositions.Add((obj.obj as Creature).mainBodyChunk.pos);
                }
            }

            foreach (TrackedObject obj in trackedObjects)
            {
                if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(herring.firstChunk.pos, weapon.firstChunk.pos, 200))
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
    public Vector2 lastDangerPos;

    public HerringAI(AbstractCreature creature, World world) : base(creature, world)
    {
        herring = creature.realizedCreature as Herring;
        herring.AI = this;
        
        AddModule(new CicadaPather(this, world, creature));
        pathFinder.stepsPerFrame = 80;
        pathFinder.accessibilityStepsPerFrame = 60;
        //pathFinder.visualizePath = true;

        speed = 1f;
        moveDir = new Vector2(1f, 0f);
        lastDangerPos = new Vector2(0, 0);

        behavior = Behavior.Wander;

        trackedCreatures = [];
        trackedObjects = [];

        /*
        foreach (CreatureTemplate template in StaticWorld.creatureTemplates)
        {
            CreatureTemplate.Relationship relationship = herring.Template.CreatureRelationship(template);
            Debug.Log("Name: " + template.name);
            Debug.Log("Relationship: " + relationship.type);
            Debug.Log("Intensity: " + relationship.intensity);
            Debug.Log("Size: " + template.bodySize);
            Debug.Log("Scaryness: " + template.scaryness);
            Debug.Log("");
        }*/
    }
    public override void Update()
    {
        base.Update();

        if (herring != null)
        {
            if (herring.room != null)
            {
                if (Random.value < 0.5)
                {
                    UpdateTrackedObjects();
                }
                UpdateBehavior();
                UpdateMovement();
            }
        }
    }
    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
    }
    public void UpdateTrackedObjects()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (PhysicalObject obj in herring.room.physicalObjects[i])
            {
                if (obj != null && obj != herring && herring.room.VisualContact(herring.firstChunk.pos, obj.firstChunk.pos))
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
                            { threat = 10f + newCreature.Template.bodySize / 2f + relationship.intensity * 20f; }

                            trackedCreatures.Add(new TrackedObject(herring, newCreature, 250, threat));
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
                        { trackedObjects.Add(new TrackedObject(herring, obj, 100, 0)); }
                    }
                }
            }
        }

        for (int i = 0; i < trackedCreatures.Count - 1; i++)
        {
            Creature creature = trackedCreatures[i].obj as Creature;
            //Herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(Herring.room, creature.mainBodyChunk.pos, "Yellow", 10));
            if (herring.room.VisualContact(herring.firstChunk.pos, creature.mainBodyChunk.pos))
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
            if (herring.room.VisualContact(herring.firstChunk.pos, trackedObjects[i].obj.firstChunk.pos))
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
        AImap map = herring.room.aimap;
        CreatureTemplate template = herring.Template;

        Vector2 destination = herring.room.MiddleOfTile(pathFinder.destination);
        Vector2 pos = herring.firstChunk.pos;
        IntVector2 intPos = herring.room.GetTilePosition(pos);
        WorldCoordinate coordPos = creature.pos;

        pathfinding = false;

        if (behavior == Behavior.Wander)
        {
            speed = 2f;

            if (Custom.DistLess(pos, destination, 80))
            {
                IntVector2 bestPos = intPos;
                for (int i = 0; i < 10; i++)
                {
                    IntVector2 checkTile1 = herring.room.RandomTile();
                    for (int j = 0; j < 20; j++)
                    {
                        IntVector2 checkTile2 = new (Random.Range(checkTile1.x - 20, checkTile1.x + 21), Random.Range(checkTile1.y - 20, checkTile1.y + 21));
                        if (pathFinder.CoordinateReachable(herring.room.GetWorldCoordinate(checkTile2)) &&
                            Custom.ManhattanDistance(checkTile2, intPos) > Custom.ManhattanDistance(bestPos, intPos) &&
                            (map.getTerrainProximity(checkTile2) > 5 || map.getTerrainProximity(checkTile2) > map.getTerrainProximity(bestPos)))
                        {
                            bestPos = checkTile2;
                        }
                    }
                }
                if (pathFinder.CoordinateReachable(herring.room.GetWorldCoordinate(bestPos)))
                {
                    pathFinder.AssignNewDestination(herring.room.GetWorldCoordinate(bestPos));
                }
            }

            pathfinding = true;
        }
        else if (behavior == Behavior.Follow)
        {
            if (herring.room.VisualContact(pos, leader.firstChunk.pos))
            {
                speed = Mathf.Clamp(Custom.Dist(pos, leader.firstChunk.pos) / 20, 0f, 2f);
                moveDir = Custom.DirVec(pos, leader.firstChunk.pos);

                if (Custom.DistLess(pos, leader.firstChunk.pos, 20))
                {
                    speed = 0f;
                }
            }
            else
            {
                speed = 3f;
                if (pathFinder.CoordinateReachable(leader.coord))
                {
                    pathFinder.AssignNewDestination(leader.coord);
                }
                pathfinding = true;
            }

            if (leader.enteringShortCut.HasValue)
            {
                herring.room.AddObject(new Objects.ColoredShapes.Rectangle(herring.room, destination, 10f, 10f, 50f, "Green", 0));
                speed = 3f;
                if (pathFinder.CoordinateReachable(leader.NPCTransportationDestination))
                {
                    pathFinder.AssignNewDestination(leader.NPCTransportationDestination);
                }
                pathfinding = true;
            }
        }
        else if (behavior == Behavior.Flee)
        {
            speed = Mathf.Clamp(20 / (Custom.Dist(pos, lastDangerPos) / 30), 3f, 5f);

            if (map.getTerrainProximity(pos) > 200)
            {
                moveDir = Custom.rotateVectorDeg(moveDir, Random.value > 0.5f ? 2f : -2f);
                for (int i = 0; i < 30; i++)
                {
                    Vector2 checkPos = pos + moveDir * 60f;
                    if ((map.getTerrainProximity(checkPos) > 2 || Custom.DistLess(pos, lastDangerPos, 100)) && herring.room.VisualContact(pos, checkPos))
                    {
                        break;
                    }
                    else
                    {
                        moveDir = Custom.rotateVectorDeg(moveDir, 3);
                    }
                }
            }
            else
            {
                if (herring.room.VisualContact(pos, lastDangerPos) && Custom.DistLess(pos, lastDangerPos, 80f) && map.getTerrainProximity(pos) > 5)
                {
                    moveDir = (moveDir + Custom.DirVec(lastDangerPos, pos)).normalized;
                }

                if (Custom.DistLess(pos, destination, 80f))
                {
                    IntVector2 bestPos = intPos;

                    if (herring.room.VisualContact(pos, lastDangerPos))
                    {
                        IntVector2 fleePos = herring.room.GetTilePosition(herring.room.MiddleOfTile(intPos) + Custom.DirVec(pos, lastDangerPos) * 40f);
                        for (int j = 0; j < 20; j++)
                        {
                            IntVector2 checkTile = new(Random.Range(intPos.x - 2, intPos.x + 3), Random.Range(intPos.y - 2, intPos.y + 3));
                            WorldCoordinate coordOfTile = herring.room.GetWorldCoordinate(checkTile);
                            if (pathFinder.CoordinateReachable(coordOfTile) &&
                                Custom.ManhattanDistance(checkTile, fleePos) > Custom.ManhattanDistance(bestPos, fleePos))
                            {
                                bestPos = checkTile;
                            }
                        }
                    }
                    else
                    {
                        IntVector2 fleePos = herring.room.GetTilePosition(herring.room.MiddleOfTile(intPos) - moveDir * 40f);
                        for (int j = 0; j < 20; j++)
                        {
                            IntVector2 checkTile = new(Random.Range(intPos.x - 2, intPos.x + 3), Random.Range(intPos.y - 2, intPos.y + 3));
                            WorldCoordinate coordOfTile = herring.room.GetWorldCoordinate(checkTile);
                            if (pathFinder.CoordinateReachable(coordOfTile) &&
                                Custom.ManhattanDistance(checkTile, fleePos) > Custom.ManhattanDistance(bestPos, fleePos))
                            {
                                bestPos = checkTile;
                            }
                        }
                    }

                    if (pathFinder.CoordinateReachable(herring.room.GetWorldCoordinate(bestPos)))
                    {
                        pathFinder.AssignNewDestination(herring.room.GetWorldCoordinate(bestPos));
                    }
                }

                pathfinding = true;
            }
        }

        if (pathfinding)
        {
            MovementConnection connection1 = (pathFinder as CicadaPather).FollowPath(coordPos, false);
            MovementConnection connection2 = (pathFinder as CicadaPather).FollowPath(connection1.destinationCoord, false);
            MovementConnection connection3 = (pathFinder as CicadaPather).FollowPath(connection2.destinationCoord, false);

            Vector2 wantMoveDir;
            if (herring.room.VisualContact(pos, herring.room.MiddleOfTile(connection3.destinationCoord)) && !map.getAItile(pos).narrowSpace && !map.getAItile(connection1.destinationCoord).narrowSpace && !map.getAItile(connection2.destinationCoord).narrowSpace)
            {
                wantMoveDir = Custom.DirVec(pos, herring.room.MiddleOfTile(connection3.destinationCoord));
                moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.1f);
            }
            else
            {
                speed = 2f;
                wantMoveDir = Custom.DirVec(pos, herring.room.MiddleOfTile(connection1.destinationCoord));
                moveDir = Vector2.Lerp(moveDir, wantMoveDir, 0.8f);
            }

            if (connection1.type == MovementConnection.MovementType.ShortCut)
            {
                herring.enteringShortCut = connection1.StartTile;
                herring.NPCTransportationDestination = connection1.destinationCoord;
            }

            //herring.room.AddObject(new Objects.ColoredShapes.Rectangle(herring.room, destination, 10f, 10f, 45f, "Yellow", 0));
        }
        else if(herring.firstChunk.ContactPoint.ToVector2().magnitude > 0.1f)
        {
            moveDir = moveDir - herring.firstChunk.ContactPoint.ToVector2() * 2f;
        }

        herring.firstChunk.vel *= 0.8f;
        herring.firstChunk.vel += moveDir * speed;

        //herring.room.AddObject(new Objects.ColoredShapes.Rectangle(herring.room, herring.firstChunk.pos + moveDir * 25f, 1f, 50f, Custom.VecToDeg(moveDir), "Green", 0));
    }
    public void UpdateBehavior()
    {
        foreach (TrackedObject obj in trackedCreatures)
        {
            if (obj.threat > 0 && Custom.DistLess(herring.firstChunk.pos, (obj.obj as Creature).mainBodyChunk.pos, 8 * obj.threat))
            {
                behavior = Behavior.Flee;
                fleeCounter = 0;
                pathFinder.AssignNewDestination(herring.coord);
            }
            if (obj.obj is Herring herring2)
            {
                if (behavior != Behavior.Flee && herring2.Consious && herring2.dominance > herring.dominance && (leader == null || leader.dominance > herring2.dominance))
                {
                    leader = herring2;
                }
            }
        }
        foreach (TrackedObject obj in trackedObjects)
        {
            if (obj.obj is Weapon weapon && weapon.mode == Weapon.Mode.Thrown && Custom.DistLess(herring.firstChunk.pos, weapon.firstChunk.pos, 50))
            {
                behavior = Behavior.Flee;
                fleeCounter = 0;
                pathFinder.AssignNewDestination(herring.coord);
            }
        }

        if (behavior == Behavior.Flee)
        {
            leader = null;
            fleeCounter++;
            if (fleeCounter > 100)
            {
                behavior = Behavior.Wander;
            }
            if (DangerPos.HasValue && DangerPos.Value != null)
            {
                lastDangerPos = DangerPos.Value;
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
            if (leader == null || !Custom.DistLess(herring.mainBodyChunk.pos, leader.mainBodyChunk.pos, 200))
            {
                behavior = Behavior.Wander;
            }
            if (!leader.Consious || leader.grabbedBy.Count > 0)
            {
                behavior = Behavior.Flee;
                fleeCounter = 0;
                pathFinder.AssignNewDestination(herring.coord);
            }
        }

        //herring.room.AddObject(new Objects.ColoredShapes.Text(herring.room, herring.firstChunk.pos, behavior.value, "Yellow", 0));
    }
    public void TryToLeaveRoom()
    {
        ShortcutData[] shortcutDatas = herring.room.shortcuts;
        AImap map = herring.room.aimap;

        foreach (ShortcutData shortcutData in shortcutDatas)
        {
            Vector2 shortcutPos = herring.room.MiddleOfTile(shortcutData.StartTile);
            if (map.TileAccessibleToCreature(shortcutData.StartTile, herring.Template))
            {
                if (shortcutData.shortCutType == ShortcutData.Type.RoomExit)
                {
                    herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(herring.room, shortcutPos, "Red", 0));
                }
                else
                {
                    herring.room.AddObject(new Objects.ColoredShapes.SmallRectangle(herring.room, shortcutPos, "White", 0));
                }
            }
        }
    }
    public void MoveWithPathFinding(WorldCoordinate? newDestination)
    {
        pathfinding = true;

        if (newDestination.HasValue)
        {
            if (pathFinder.CoordinateReachable(newDestination.Value))
            {
                pathFinder.AssignNewDestination(newDestination.Value);
            }
        }
        else
        {
            AImap map = herring.room.aimap;

            Vector2 destination = herring.room.MiddleOfTile(pathFinder.destination);
            Vector2 pos = herring.firstChunk.pos;
            Vector2 lookPos = pos + herring.firstChunk.vel.normalized * 120f;

            if (Custom.Dist(pos, destination) < 80)
            {
                IntVector2 checkPos = herring.room.GetTilePosition(lookPos);
                IntVector2 bestPos = checkPos;

                for (int i = 0; i < 20; i++)
                {
                    IntVector2 newPos = new(checkPos.x + Random.Range(-20, 21), checkPos.y + Random.Range(-20, 21));
                    if (pathFinder.CoordinateReachable(herring.room.GetWorldCoordinate(newPos)) && map.getTerrainProximity(newPos) > 3)
                    {
                        bestPos = newPos;
                    }
                }
                if (pathFinder.CoordinateReachable(herring.room.GetWorldCoordinate(bestPos)))
                {
                    pathFinder.AssignNewDestination(herring.room.GetWorldCoordinate(bestPos));
                }
            }
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
    public class TrackedEntrance
    {
        public Herring herring;
        public Room room;
        public IntVector2 position;
        public string type;
        public TrackedEntrance(Herring herring, Room room, IntVector2 position, string type)
        {
            this.herring = herring;
            this.room = room;
            this.position = position;
            this.type = type;
        }
    }
}

public class HerringPather : StandardPather
{
    public HerringPather(ArtificialIntelligence AI, World world, AbstractCreature creature) : base(AI, world, creature)
    {

    }

    public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
    {
        PathCost cost = base.CheckConnectionCost(start, goal, connection, followingPath);

        int terrainProximity = realizedRoom.aimap.getTerrainProximity(connection.destinationCoord);

        if (terrainProximity < 5)
        {
            int resistance = 5 - terrainProximity;
            cost.resistance += 10 * resistance;
        }

        return cost;
    }
}
