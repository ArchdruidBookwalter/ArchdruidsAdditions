using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ArchdruidsAdditions.Data;
using DevInterface;
using SplashWater;
using Watcher;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

#region Parasite Creature

public class Parasite : InsectoidCreature, IPlayerEdible, IHaveInjectedPoisonColor
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
    public bool stuckOnTerrain;

    public IntVector2 lastAirTile;

    public int bites;
    public bool eaten;

    public bool buryOnSpawn;
    public int burySpawnAttempts;
    public int burrowCounter;
    public int maxBurrowCounter = 200;

    public BodyChunk buriedInChunk;
    public float relativeBuryAng;
    public float relativeBuryDist;
    public int hurtTimer;

    public bool splat;
    public int splatNoiseCooldown;

    public Color InjectedPoisonColor
    {   
        get { return (graphicsModule as ParasiteGraphics).poisonColor; }
        set { (graphicsModule as ParasiteGraphics).poisonColor = value; } 
    }
    public int dieCounter;
    public bool poisoned;
    public bool shocked;
    public float poisonTime;

    public Color color;

    public BodyChunk TailChunk
    {
        get
        {
            return bodyChunks[bodyChunks.Length - 1];
        }
    }

    public BodyChunk HeadChunk
    {
        get
        {
            return bodyChunks[0];
        }
    }

    public AbstractParasiteStick ParasiteStick
    {
        get
        {
            if (parasiteStick != null)
            {
                return parasiteStick;
            }
            else
            {
                if (abstractPhysicalObject.stuckObjects.Count > 0)
                {
                    foreach (AbstractPhysicalObject.AbstractObjectStick stick in abstractPhysicalObject.stuckObjects)
                    {
                        if (stick is AbstractParasiteStick parasiteStick)
                        {
                            this.parasiteStick = parasiteStick;
                            return parasiteStick;
                        }
                    }
                }
                return null;
            }
        }
    }
    public AbstractParasiteStick parasiteStick;

    public Parasite(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        extraSegmentLength = 2f;

        int length = 5;

        List<BodyChunk> newBodyChunks = [];
        for (int i = 0; i < length; i++)
        {
            newBodyChunks.Add(new(this, 0, default, Mathf.Lerp(2f, 0f, (float)i/(length + 1)), 0.05f));
        }
        bodyChunks = [.. newBodyChunks];

        List<BodyChunkConnection> newBodyChunkConnections = [];
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            for (int j = i + 1; j < bodyChunks.Length; j++)
            {
                BodyChunk chunk1 = bodyChunks[i];
                BodyChunk chunk2 = bodyChunks[j];
                newBodyChunkConnections.Add(new(chunk1, chunk2, chunk1.rad + chunk2.rad + extraSegmentLength, BodyChunkConnection.Type.Push, 1f, -1f));
            }
        }
        bodyChunkConnections = [.. newBodyChunkConnections];

        gravity = 0.9f;
        airFriction = 0.999f;
        bounce = 0.2f;
        waterFriction = 0.96f;
        buoyancy = 0.9f;
        collisionLayer = 1;
        surfaceFriction = 0.4f;

        bites = 3;

        if (ParasiteState.creatureAttachedTo.HasValue)
        {
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

        AI.destination = HeadChunk.pos;

        lastAirTile = room.GetTilePosition(HeadChunk.pos);
    }
    public override void Update(bool eu)
    {
        base.Update(eu);

        float section = 1;

        try
        {

            section = 2;

            if (buryOnSpawn)
            { TryBuryOnSpawn(); }

            if (buriedInChunk == null)
            {
                section = 3;

                if (Consious && !buryOnSpawn)
                {
                    if (grasps[0] != null)
                    {
                        TryBurrowInPrey(grasps[0]);
                    }

                    AI.Update();
                }

                if (grasps[0] != null)
                {
                    CollideWithObjects = false;
                    CollideWithTerrain = false;

                    foreach (BodyChunkConnection connection in bodyChunkConnections)
                    {
                        connection.active = false;
                    }

                    BodyChunk grabbedChunk = grasps[0].grabbedChunk;

                    Vector2 buryDir = Custom.rotateVectorDeg(grabbedChunk.Rotation, relativeBuryAng);
                    Vector2 buryPos = GetBuryPos(grabbedChunk) + buryDir * relativeBuryDist;
                    Vector2 dirVec = Custom.DirVec(HeadChunk.pos, buryPos);

                    float dist1 = Custom.Dist(HeadChunk.pos, buryPos);
                    float mass1 = HeadChunk.mass / (HeadChunk.mass + grabbedChunk.mass);
                    HeadChunk.pos += dirVec * dist1 * (1f - mass1);
                    HeadChunk.vel += dirVec * dist1 * (1f - mass1);

                    Vector2 neckPos = buryPos + buryDir;
                    Vector2 dirVec2 = Custom.DirVec(bodyChunks[1].pos, neckPos);
                    float dist2 = Custom.Dist(bodyChunks[1].pos, neckPos);
                    float mass2 = bodyChunks[1].mass / (bodyChunks[1].mass + grabbedChunk.mass);
                    bodyChunks[1].pos += dirVec2 * dist2 * (1f - mass2);
                    bodyChunks[1].vel += dirVec2 * dist2 * (1f - mass2);
                }
                else
                {
                    CollideWithObjects = true;
                    CollideWithTerrain = true;

                    foreach (BodyChunkConnection connection in bodyChunkConnections)
                    {
                        connection.active = true;
                    }

                    if (AI.jumping)
                    {
                        TailChunk.vel *= 0.5f;
                    }

                    BodyChunk randomChunk = RandomChunk;
                    if ((randomChunk.ContactPoint.x != 0 || randomChunk.ContactPoint.y != 0) && randomChunk.vel.magnitude > 5f)
                    {
                        if (splatNoiseCooldown == 0)
                        {
                            MakeSplatNoise();

                            splatNoiseCooldown = Random.Range(20, 50);
                        }
                    }
                }

                #region Physics

                stuckOnTerrain = false;

                if (enteringShortCut == null)
                {
                    for (int i = 0; i < connectionRopes.Length - 1; i++)
                    {
                        ConnectSegments(i, i + 1, false);
                    }
                    for (int i = connectionRopes.Length - 1; i >= 0; i--)
                    {
                        ConnectSegments(i, i + 1, true);
                    }

                    foreach (BodyChunkConnection connection in bodyChunkConnections)
                    {
                        if (Custom.Dist(connection.chunk1.pos, connection.chunk2.pos) > connection.distance * 50)
                        {
                            //Create_LineBetweenTwoPoints(room, connection.chunk1.pos, connection.chunk2.pos, 5f, "Red", 0);
                            connection.chunk1.pos = connection.chunk2.pos + Custom.DirVec(connection.chunk2.pos, connection.chunk1.pos) * connection.distance * 50;
                        }
                    }
                }

                IntVector2 headPos = room.GetTilePosition(HeadChunk.pos);
                if (headPos != lastAirTile)
                {
                    if (room.GetTile(headPos).Solid || (Custom.Dist(room.MiddleOfTile(headPos), room.MiddleOfTile(lastAirTile)) > 30 && !room.VisualContact(headPos, lastAirTile)))
                    {
                        Vector2 aimDir = Vector2.zero;
                        for (int i = 0; i < 10; i++)
                        {
                            aimDir = Custom.DirVec(room.MiddleOfTile(lastAirTile), room.MiddleOfTile(RandomChunk.pos));
                            IntVector2 checkDir = Random.value > 0.5 ? new(Math.Sign(aimDir.x), 0) : new(0, Math.Sign(aimDir.y));
                            IntVector2 checkPos = lastAirTile + checkDir;

                            if (room.GetTile(lastAirTile + checkDir).Solid)
                            {
                                break;
                            }
                            else if (room.VisualContact(lastAirTile, checkPos))
                            {
                                lastAirTile = checkPos;
                            }
                        }

                        foreach (BodyChunk chunk in bodyChunks)
                        {
                            chunk.pos = room.MiddleOfTile(lastAirTile) + Custom.RNV() * 5f;
                            chunk.vel += -aimDir;
                        }
                    }
                    else
                    {
                        lastAirTile = headPos;
                    }
                }

                #endregion
            }
            else
            {
                section = 4;

                if (ParasiteStick == null)
                {
                    BurrowOutOfCreature();
                    return;
                }
                else if (dieCounter > 50)
                {
                    BurrowOutAndDie(shocked);
                    return;
                }

                if (dead)
                {
                    dieCounter++;
                }

                section = 5;

                CollideWithObjects = false;
                CollideWithTerrain = false;

                foreach (BodyChunkConnection connection in bodyChunkConnections)
                { connection.active = false; }

                section = 6;

                AbstractCreature attachedCreature = ParasiteStick.OtherCreature;
                if (attachedCreature.realizedCreature != null)
                {
                    if (attachedCreature.realizedCreature.injectedPoison > 0f)
                    {
                        float poisonChange = 0.01f;
                        attachedCreature.realizedCreature.injectedPoison = Mathf.Max(attachedCreature.realizedCreature.injectedPoison - poisonChange, 0f);

                        if (attachedCreature.realizedCreature is IHaveInjectedPoisonColor crWPoisonColor)
                        {
                            InjectPoison(poisonChange, crWPoisonColor.InjectedPoisonColor);
                            injectedPoison = Mathf.Min(injectedPoison, 0.9f);
                        }
                        else
                        {
                            injectedPoison = Mathf.Min(injectedPoison + poisonChange, 0.9f);
                        }
                    }

                    if (attachedCreature.realizedCreature.Stunned)
                    {
                        foreach (UpdatableAndDeletable updel in room.updateList)
                        {
                            if (updel is CreatureSpasmer spasmer && spasmer.crit == attachedCreature.realizedCreature)
                            {
                                shocked = true;
                                dieCounter++;
                            }
                        }
                    }

                    if (room != null && attachedCreature.realizedCreature is Player && PlayerData.playerStates.ContainsKey(attachedCreature.ID.number))
                    {
                        PlayerData.AAPlayerState playerState = PlayerData.playerStates[attachedCreature.ID.number];

                        if (hurtTimer > 0)
                        {
                            if (hurtTimer == 1)
                            { HurtPlayer(false); }

                            hurtTimer--;
                        }
                        else
                        {
                            if (ParasiteStick.growth >= 6)
                            {
                                if (playerState.parasiteKillCounter < 3)
                                {
                                    playerState.parasiteKillCounter++;

                                    hurtTimer = Random.Range(1000 / playerState.parasiteKillCounter, 1500 / playerState.parasiteKillCounter);
                                }

                            }
                        }

                        //Create_Text(room, HeadChunk.pos, playerState.parasiteKillCounter, "Red", 0);

                        //Create_Text(room, HeadChunk.pos + new Vector2(0f, 10f), hurtTimer, "Red", 0);
                    }

                    if (dieCounter > 0)
                    {
                        attachedCreature.realizedCreature.Stun(50);
                    }

                    Vector2 buryPos = buriedInChunk.pos;

                    if (attachedCreature.realizedObject is Player player)
                    { buryPos = (player.graphicsModule as PlayerGraphics).drawPositions[buriedInChunk.index, 0]; }

                    foreach (BodyChunk chunk in bodyChunks)
                    {
                        chunk.pos = buryPos;
                        chunk.vel = buriedInChunk.vel;
                    }
                }

                section = 7;

                if (attachedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    Data.PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(attachedCreature.ID.number);
                    if (playerState != null)
                    {
                        if (playerState.tooSpicy)
                        {
                            InjectPoison(0.01f, Custom.HSL2RGB(0f, 0.8f, 0.3f));
                            injectedPoison = Mathf.Min(injectedPoison, 0.9f);
                        }
                    }
                }

                section = 8;

                if (injectedPoison >= 0.9f)
                {
                    dieCounter++;
                }

                IntVector2 headPos = room.GetTilePosition(HeadChunk.pos);
                if (headPos != lastAirTile)
                { lastAirTile = headPos; }
            }

            section = 9;

            if (splatNoiseCooldown > 0)
            { splatNoiseCooldown--; }

            /*
            if (ParasiteStick != null)
            {
                Create_Text(room, HeadChunk.pos, ParasiteStick.growth, "Red", 0);
            }*/

        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "PARASITE_UPDATE", section);

            throw e;
        }
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);

        if (myChunk == 0 && AI.jumping && buriedInChunk == null && grasps[0] == null)
        {
            if (otherObject is Creature creature && creature is not Parasite && !creature.dead)
            {
                foreach (Grasp grasp in creature.grabbedBy)
                {
                    if (grasp.grabber is Parasite)
                    {
                        goto End;
                    }
                }

                foreach (AbstractPhysicalObject.AbstractObjectStick stick in creature.abstractCreature.stuckObjects)
                {
                    if (stick is AbstractParasiteStick)
                    {
                        goto End;
                    }
                }

                BiteOtherCreature(creature, creature.bodyChunks[otherChunk]);
            }
        }

        End:;

        if (splatNoiseCooldown == 0)
        {
            MakeSplatNoise();
        }
        splatNoiseCooldown = Random.Range(20, 50);
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
    }
    public override void Destroy()
    {
        base.Destroy();
    }

    public void TryBuryOnSpawn()
    {
        AbstractCreature stuckInCreature = null;
        if (abstractPhysicalObject.stuckObjects.Count == 0)
        {
            List<AbstractCreature> creatures = room.abstractRoom.creatures;
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].ID == ParasiteState.creatureAttachedTo.Value && creatures[i].realizedCreature != null)
                {
                    stuckInCreature = creatures[i];
                    BuryIntoChunk(creatures[i].realizedCreature.bodyChunks[ParasiteState.chunk], true);
                    buryOnSpawn = false;
                }
            }
        }
        else
        {
            AbstractParasiteStick abstractStick = abstractPhysicalObject.stuckObjects[0] as AbstractParasiteStick;
            AbstractPhysicalObject stuckObj = abstractStick.B;
            if (stuckObj.Room == room.abstractRoom && stuckObj.realizedObject != null)
            {
                stuckInCreature = stuckObj as AbstractCreature;
                BuryIntoChunk(stuckObj.realizedObject.bodyChunks[abstractStick.chunk], false);
                buryOnSpawn = false;
            }
        }

        if (stuckInCreature != null)
        {
            Data.PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(stuckInCreature.ID.number);
            if (playerState != null)
            {
                playerState.parasiteMalnourishment = 1f;
                if (stuckInCreature.realizedCreature.graphicsModule is PlayerGraphics pGraphics)
                {
                    pGraphics.malnourished = 1f;
                }
            }
        }

        burySpawnAttempts++;
        if (burySpawnAttempts >= 40)
        {
            buryOnSpawn = false;
        }
    }
    public void ConnectSegments(int indexA, int indexB, bool unBug)
    {
        Rope rope = indexA < indexB ? connectionRopes[indexA] : connectionRopes[indexB];

        BodyChunk chunk1 = bodyChunks[indexA];
        BodyChunk chunk2 = bodyChunks[indexB];

        rope.Update(chunk1.pos, chunk2.pos);

        float ropeLength = rope.totalLength;
        float segmentLength = Mathf.Max(0, chunk1.rad + chunk2.rad - ((float)burrowCounter / 50));

        if (ropeLength > segmentLength)
        {
            float mass = chunk1.mass / (chunk1.mass + chunk2.mass);
            float pullStrength = (ropeLength - segmentLength) * 0.8f;
            Vector2 dirVec1 = Custom.DirVec(chunk1.pos, rope.AConnect);
            Vector2 dirVec2 = Custom.DirVec(chunk2.pos, rope.BConnect);

            chunk1.pos += dirVec1 * pullStrength * mass;
            chunk1.vel += dirVec1 * pullStrength * mass;
            chunk2.pos += dirVec2 * pullStrength * (1f - mass);
            chunk2.vel += dirVec2 * pullStrength * (1f - mass);

            if (!room.VisualContact(chunk1.pos, chunk2.pos) && ropeLength > 20)
            {
                //Create_LineBetweenTwoPoints(room, chunk1.pos, chunk2.pos, 1f, "Red", 0);
                stuckOnTerrain = true;
            }
        }
    }
    public bool CheckCollision()
    {
        if (!room.VisualContact(HeadChunk.pos, TailChunk.pos))
        {
            return true;
        }

        foreach (BodyChunk chunk in bodyChunks)
        {
            if (room.GetTile(chunk.pos).Solid)
            {
                return true;
            }
        }

        return false;
    }
    public void TryBurrowInPrey(Grasp grasp)
    {
        if (grasp.grabbed is not Creature creature || creature.dead)
        {
            ReleaseGrasp(0);
            return;
        }
        else if (grasp.grabbed.abstractPhysicalObject.stuckObjects.Count > 0)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in grasp.grabbed.abstractPhysicalObject.stuckObjects)
            {
                if (stick is AbstractParasiteStick paraStick && paraStick.Parasite != abstractCreature)
                {
                    ReleaseGrasp(0);
                    return;
                }
            }
        }

        if (burrowCounter == 0 && creature.abstractCreature.creatureTemplate.bodySize < 3)
        {
            creature.Stun(maxBurrowCounter + 50);
        }

        burrowCounter++;

        if (burrowCounter > maxBurrowCounter)
        {
            for (int i = 0; i < Random.Range(3, 5); i++)
            {
                WaterDrip drip = new(HeadChunk.pos, Custom.RNV() * 5, false);
                room.AddObject(drip);
            }

            BuryIntoChunk(grasp.grabbedChunk, true);

            room.PlaySound(SoundID.Slugcat_Eat_Meat_A, HeadChunk, false, 1f, 1f);
            room.PlaySound(SoundID.Spear_Stick_In_Creature, HeadChunk, false, 1f, 0.8f);
        }
        else if (burrowCounter % 20 == 0)
        {
            WaterDrip drip = new(HeadChunk.pos, Custom.RNV() * 5, false);
            room.AddObject(drip);

            room.PlaySound(SoundID.Slugcat_Eat_Meat_A, HeadChunk, false, 1f, 1f);
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

        if (!buryOnSpawn && ParasiteState.growth == 0)
        {
            HurtPlayer(true);
        }
        else if (ParasiteState.growth < 6)
        {
            hurtTimer = Random.Range(1000, 1500);
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

        Vector2 buryPos = GetBuryPos(chunk);
        relativeBuryAng = Custom.Angle(chunk.Rotation, Custom.DirVec(buryPos, HeadChunk.pos));
        relativeBuryDist = Mathf.Min(chunk.rad / 2, Custom.Dist(buryPos, HeadChunk.pos));

        creature.Violence(bodyChunks[0], Vector2.zero, chunk, null, DamageType.Bite, 0.1f, 50f);
    }
    public Vector2 GetBuryPos(BodyChunk buriedInChunk)
    {
        Vector2 buryPos;
        if (buriedInChunk.owner.graphicsModule is PlayerGraphics pGraphics)
        {
            buryPos = Vector2.Lerp(pGraphics.drawPositions[0, 0], pGraphics.drawPositions[1, 0], 0.5f);
        }
        else
        {
            buryPos = buriedInChunk.pos;
        }

        if (buriedInChunk.owner.evenUpdate != evenUpdate)
        {
            buryPos += buriedInChunk.vel;
        }

        return buryPos;
    }
    public void HurtPlayer(bool sharp)
    {
        if (buriedInChunk != null && buriedInChunk.owner is Creature player)
        {
            Data.PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.abstractCreature.ID.number);
            if (playerState != null)
            {
                bool stun = sharp || ParasiteStick.growth >= 6;
                bool fatal = playerState.parasiteKillCounter >= 3;

                playerState.parasiteIllnessEffect = new(room, player.abstractCreature, abstractCreature, sharp ? 100 : 500, fatal ? 500 : 0, 500, stun, fatal);
            }
        }
    }
    public void BurrowOutAndDie(bool shocked)
    {
        //Debug.Log("PARASITE WAS POISONED!");

        BurrowOutOfCreature();

        Die();

        if (shocked)
        {
            room.AddObject(new CreatureSpasmer(this, true, Random.Range (200, 400)));
        }
    }
    public void BurrowOutOfCreature()
    {
        LoseAllGrasps();
        abstractPhysicalObject.LoseAllStuckObjects();
        ParasiteState.creatureAttachedTo = null;

        Vector2 buryPos = GetBuryPos(buriedInChunk);
        foreach (BodyChunk chunk in bodyChunks)
        {
            chunk.pos = buryPos;
            chunk.vel *= 0f;
        }
        buriedInChunk = null;

        burrowCounter = 0;
        dieCounter = 0;

        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            WaterDrip drip = new(bodyChunks[0].pos, Custom.RNV() * 5, false);
            room.AddObject(drip);
        }

        room.PlaySound(SoundID.Spear_Stick_In_Creature, HeadChunk, false, 1f, 1f);
    }
    public void MakeSplatNoise()
    {
        room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, firstChunk.pos, 0.4f, 1f);
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
    public FSprite bigEye;
    public FSprite smallEye;
    public FSprite tooth1;
    public FSprite tooth2;

    public Vector2 rotation, lastRotation;
    public bool wiggle;
    public int wiggleChunkIndex = 0;
    public int wiggleTimer = 0;

    public Color bodyColor, blackColor, poisonColor;
    public float segmentBulge = 0f;

    public ParasiteGraphics(PhysicalObject ow) : base(ow, false)
    {
        parasite = ow as Parasite;
        List<BodyPart> list = [];

        Random.State state = Random.state;
        Random.InitState(parasite.abstractCreature.ID.number);

        bodyColor = Custom.HSL2RGB(0.1f, Random.Range(0.1f, 0.2f), Random.Range(0.6f, 0.9f));

        Random.state = state;
    }

    public override void Update()
    {
        lastRotation = rotation;

        base.Update();

        float section = 1;
        try
        {
        }
        catch (Exception e)
        {
            Log_Exception(e, "PARASITE GRAPHICS UPDATE", section);
        }

        rotation = Custom.DirVec(parasite.bodyChunks[1].pos, parasite.bodyChunks[0].pos);

        if (camera != null)
        {
            UpdateLighting(camera);
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        bodyMesh = TriangleMesh.MakeLongMesh(parasite.bodyChunks.Length + 1, false, false);
        sprites.Add(bodyMesh);

        bigEye = new("JetFishEyeB", true)
        {
            scale = 0.5f
        };
        sprites.Add(bigEye);

        smallEye = new("JetFishEyeB", true)
        {
            scale = 0.25f
        };
        sprites.Add(smallEye);

        //Debug.Log(parasite.AI.burrowCounter);

        tooth1 = new("SpiderLeg0B", true)
        {
            anchorY = 0.1f,
            scaleX = -0.5f,
            scaleY = 0.2f
        };
        sprites.Add(tooth1);

        tooth2 = new("SpiderLeg0B", true)
        {
            anchorY = 0.1f,
            scaleX = 0.5f,
            scaleY = 0.2f,
        };
        sprites.Add(tooth2);

        sLeaser.sprites = [.. sprites];

        AddToContainer(sLeaser, rCam, null);

        UpdateLighting(rCam);
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

                    Vector2 lastBodyPos = bodyPositions[0] + headRot * 5;
                    Vector2 thisBodyPos = bodyPositions[0];
                    Vector2 nextBodyPos = bodyPositions[1];

                    Vector2 frontPos = Vector2.Lerp(thisBodyPos, lastBodyPos, 0.5f);
                    Vector2 backPos = Vector2.Lerp(thisBodyPos, nextBodyPos, 0.5f);

                    Vector2 frontRot = headRot;
                    Vector2 backRot = Custom.DirVec(nextBodyPos, thisBodyPos);

                    float thisRad = parasite.bodyChunks[0].rad;
                    float lastRad = thisRad;

                    for (int i = 0; i < bodyPositions.Count; i++)
                    {
                        if (i == 0)
                        {
                            Vector2 frontPos1 = thisBodyPos - perpHeadRot * thisRad + headRot * thisRad;
                            Vector2 frontPos2 = thisBodyPos + perpHeadRot * thisRad + headRot * thisRad;

                            Vector2 thisRot = Vector2.Lerp(frontRot, backRot, 0.5f).normalized;
                            Vector2 perpRot = Custom.PerpendicularVector(thisRot);
                            Vector2 sidePos1 = thisBodyPos - perpRot * (thisRad + segmentBulge);
                            Vector2 sidePos2 = thisBodyPos + perpRot * (thisRad + segmentBulge);

                            meshPositions.Add(frontPos1);
                            meshPositions.Add(frontPos2);
                            meshPositions.Add(sidePos1);
                            meshPositions.Add(sidePos2);
                        }
                        else if (i < bodyPositions.Count - 1)
                        {
                            lastBodyPos = thisBodyPos;
                            thisBodyPos = nextBodyPos;
                            nextBodyPos = bodyPositions[i + 1];

                            frontPos = backPos;
                            backPos = Vector2.Lerp(thisBodyPos, nextBodyPos, 0.5f);

                            frontRot = backRot;
                            backRot = Custom.DirVec(nextBodyPos, thisBodyPos);

                            lastRad = thisRad;
                            thisRad = parasite.bodyChunks[i].rad;

                            Vector2 perpFrontRot = Custom.PerpendicularVector(frontRot);
                            Vector2 frontPos1 = frontPos - perpFrontRot * (thisRad - segmentBulge);
                            Vector2 frontPos2 = frontPos + perpFrontRot * (thisRad - segmentBulge);

                            Vector2 thisRot = Vector2.Lerp(frontRot, backRot, 0.5f).normalized;
                            Vector2 perpRot = Custom.PerpendicularVector(thisRot);
                            Vector2 sidePos1 = thisBodyPos - perpRot * (thisRad + segmentBulge);
                            Vector2 sidePos2 = thisBodyPos + perpRot * (thisRad + segmentBulge);

                            meshPositions.Add(frontPos1);
                            meshPositions.Add(frontPos2);
                            meshPositions.Add(sidePos1);
                            meshPositions.Add(sidePos2);
                        }
                        else
                        {
                            lastBodyPos = thisBodyPos;
                            thisBodyPos = nextBodyPos;

                            frontPos = backPos;
                            frontRot = backRot;

                            lastRad = thisRad;
                            thisRad = parasite.bodyChunks[i].rad;

                            Vector2 perpFrontRot = Custom.PerpendicularVector(frontRot);
                            Vector2 frontPos1 = frontPos - perpFrontRot * (thisRad - segmentBulge);
                            Vector2 frontPos2 = frontPos + perpFrontRot * (thisRad - segmentBulge);

                            Vector2 perpRot = Custom.PerpendicularVector(frontRot);
                            Vector2 sidePos1 = thisBodyPos - perpRot * (thisRad + segmentBulge);
                            Vector2 sidePos2 = thisBodyPos + perpRot * (thisRad + segmentBulge);
                            Vector2 tailPos = thisBodyPos - frontRot * thisRad;

                            meshPositions.Add(frontPos1);
                            meshPositions.Add(frontPos2);
                            meshPositions.Add(sidePos1);
                            meshPositions.Add(sidePos2);
                            meshPositions.Add(tailPos);
                        }
                    }

                    int bury = parasite.burrowCounter;
                    int maxBury = parasite.maxBurrowCounter;
                    bool hideEyes = bury > maxBury / 2;

                    Vector2 lastMeshPos = meshPositions[meshPositions.Count - 1];
                    while (meshPositions.Count < bodyMesh.vertices.Length)
                    {
                        meshPositions.Add(lastMeshPos);
                    }

                    for (int i = 0; i < bodyMesh.vertices.Length; i++)
                    {
                        bodyMesh.MoveVertice(i, meshPositions[i]);
                    }
                    bodyMesh.alpha = 1f;

                    bigEye.SetPosition(bodyPositions[0]);
                    bigEye.alpha = hideEyes ? 0f : 1f;
                    bigEye.color = blackColor;

                    Vector2 headSegRot = Custom.DirVec(bodyPositions[1], bodyPositions[0]);

                    smallEye.SetPosition(bodyPositions[0] + headSegRot * -5f);
                    smallEye.alpha = hideEyes ? 0f : 1f;
                    smallEye.color = blackColor;

                    tooth1.SetPosition(meshPositions[0]);
                    tooth1.rotation = Custom.VecToDeg(headSegRot);
                    tooth1.color = blackColor;
                    tooth1.alpha = hideEyes ? 0f : 1f;

                    tooth2.SetPosition(meshPositions[1]);
                    tooth2.rotation = Custom.VecToDeg(headSegRot);
                    tooth2.color = blackColor;
                    tooth2.alpha = hideEyes ? 0f : 1f;

                    if (camera == null)
                    { UpdateLighting(rCam); }

                    Color lightColor = Color.Lerp(lastLightColor, this.lightColor, timeStacker);
                    float lightExposure = Mathf.Lerp(lastLightExposure, this.lightExposure, timeStacker);
                    float colorExposure = Mathf.Lerp(lastColorExposure, this.colorExposure, timeStacker);

                    Color tintedBodyColor = Color.Lerp(this.bodyColor, lightColor, colorExposure);
                    Color finalBodyColor = Color.Lerp(blackColor, tintedBodyColor, lightExposure);

                    if (parasite.injectedPoison > 0)
                    {
                        bodyMesh.color = Color.Lerp(finalBodyColor, poisonColor, 0.5f);
                        tooth1.color = Color.Lerp(blackColor, poisonColor, 0.5f);
                        tooth2.color = Color.Lerp(blackColor, poisonColor, 0.5f);
                    }
                    else
                    {
                        bodyMesh.color = finalBodyColor;
                        tooth1.color = blackColor;
                        tooth2.color = blackColor;
                    }

                    bigEye.color = blackColor;
                    smallEye.color = blackColor;
                }
                else
                {
                    bodyMesh.alpha = 0f;
                    bigEye.alpha = 0f;
                    smallEye.alpha = 0f;
                    tooth1.alpha = 0f;
                    tooth2.alpha = 0f;
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

        blackColor = palette.blackColor;
    }

    public RoomCamera camera;
    public Color lightColor, lastLightColor;
    public float lightExposure, lastLightExposure;
    public float colorExposure, lastColorExposure;

    public void UpdateLighting(RoomCamera rCam)
    {
        camera = rCam;

        lastLightColor = lightColor;
        lastLightExposure = lightExposure;
        lastColorExposure = colorExposure;
        (lightColor, lightExposure, colorExposure) = TrueLightColorAndExposure(camera.room, camera, parasite.firstChunk.pos - camera.pos, 0f);
    }
}

public class ParasiteAI : ArtificialIntelligence, IUseARelationshipTracker
{
    public Parasite parasite;

    public Behavior behavior;
    public Tracker.CreatureRepresentation prey;

    public Vector2 destination;
    public Vector2 moveDir;
    public float speed;
    public bool touchingGround;

    public bool jumping;
    public Vector2 jumpPos;
    public int jumpCooldown = 0;
    public int jumpPoint = 40;
    public int maxCooldown = 80;
    public int shortcutCooldown;

    public ParasiteAI(AbstractCreature creature, World world) : base(creature, world)
    {
        parasite = creature.realizedCreature as Parasite;
        parasite.AI = this;

        AddModule(new StandardPather(this, world, creature));
        AddModule(new Tracker(this, 2, 10, 50, 0.35f, 5, 5, 10, false));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new PreyTracker(this, 5, 2f, 3f, 70f, 0.5f));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new UtilityComparer(this));
        pathFinder.stepsPerFrame = 50;

        utilityComparer.AddComparedModule(preyTracker, null, 0.9f, 1.1f);
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);

        jumping = false;
    }

    public override void Update()
    {
        base.Update();

        float section = 1;
        try
        {
            section = 2;

            BehaviorUpdate();

            section = 3;

            MovementUpdate();

            touchingGround = false;
            foreach (BodyChunk chunk in parasite.bodyChunks)
            {
                if (chunk.ContactPoint.x != 0 || chunk.ContactPoint.y != 0)
                {
                    touchingGround = true;
                    break;
                }
            }

            if (jumping)
            {
                if (jumpCooldown < jumpPoint - 20 && !touchingGround || jumpCooldown == 0)
                {
                    jumping = false;
                }
            }

            //Create_Text(parasite.room, parasite.firstChunk.pos, jumping, "Red", 0);

            if (shortcutCooldown > 0)
            {
                shortcutCooldown--;
            }
        }
        catch (Exception e)
        {
            Log_Exception(e, "PARASITE AI UPDATE", section);
        }
    }
    public void BehaviorUpdate()
    {
        if (parasite.burrowCounter > 0)
        {
            behavior = Behavior.Burrow;
        }
        else
        {
            AIModule highestModule = utilityComparer.HighestUtilityModule();

            if (highestModule != null)
            {
                if (highestModule is PreyTracker preyTracker && preyTracker.MostAttractivePrey != null)
                {
                    behavior = Behavior.Hunt;
                }
                else
                {
                    behavior = Behavior.Wander;
                }
            }
            else
            {
                behavior = Behavior.Wander;
            }
        }
    }
    public void MovementUpdate()
    {
        Room room = parasite.room;
        World world = room.world;
        AImap map = room.aimap;
        Vector2 pos = parasite.HeadChunk.pos;
        Vector2 tailPos = parasite.TailChunk.pos;
        //Vector2 destination = room.MiddleOfTile(pathFinder.destination);
        IntVector2 intPos = room.GetTilePosition(pos);
        IntVector2 intTailPos = room.GetTilePosition(tailPos);

        WorldCoordinate coordPos = creature.pos;

        //bool followPath = false;
        bool swimming = parasite.HeadChunk.submersion > 0.2;

        //Create_Text(parasite.room, pos, behavior, "Yellow", 0);

        Vector2 swimDir = Vector2.zero;
        moveDir = swimDir;

        //Create_Square(room, destination, 10f, 10f, Vec(45), "Red", 0);

        if (behavior == Behavior.Wander)
        {
            if (Custom.DistLess(pos, destination, 50) || (!swimming && Mathf.Abs(pos.x - destination.x) < 50))
            {
                for (int i = 0; i < 20; i++)
                {
                    IntVector2 randomPos = new(intPos.x + Random.Range(-50, 50), intPos.y + Random.Range(-50, 50));
                    if (map.getTerrainProximity(randomPos) > 3 && !map.getAItile(randomPos).AnyWater)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            IntVector2 testPos = randomPos + new IntVector2(0, -j + 5);
                            if (map.getAItile(testPos).acc == AItile.Accessibility.Floor || map.getAItile(testPos).AnyWater)
                            {
                                destination = room.MiddleOfTile(testPos);
                            }
                        }
                    }
                }
            }

            swimDir = Custom.DirVec(parasite.mainBodyChunk.pos, destination);
            moveDir.x = Mathf.Sign(swimDir.x);
            moveDir.y = Mathf.Sign(swimDir.y);

            speed = 1;
        }
        else if (behavior == Behavior.Hunt)
        {
            Tracker.CreatureRepresentation rep = preyTracker.MostAttractivePrey;
            if (rep != null)
            {
                destination = room.MiddleOfTile(rep.BestGuessForPosition());
                //Create_Square(room, destination, 20f, 20f, Vec(45), "Red", 0);

                if ((rep.VisualContact && Custom.DistLess(pos, destination, 50)) || (!swimming && Mathf.Abs(pos.x - destination.x) < 50))
                {
                    TryJump(destination);
                }
                else
                {
                    swimDir = Custom.DirVec(parasite.mainBodyChunk.pos, destination);
                    moveDir.x = Mathf.Sign(swimDir.x);
                    moveDir.y = Mathf.Sign(swimDir.y);
                }
            }

            speed = 2;
        }

        bool goUpForAir = false;
        if (swimming && parasite.lungs < 0.5)
        {
            goUpForAir = true;

            parasite.HeadChunk.vel += Vector2.up * (2f + (1 - parasite.lungs)) * 0.5f;
        }

        if (jumpCooldown == 0 && moveDir.magnitude > 0.1)
        {
            if (swimming)
            {
                if (!goUpForAir)
                {
                    parasite.HeadChunk.vel += swimDir * speed * 0.5f;
                }

                for (int i = 1; i < parasite.bodyChunks.Length - 1; i++)
                {
                    parasite.WeightedPush(i + 1, i, -parasite.HeadChunk.vel.normalized, 2f);
                }

                //Create_Square(room, pos, 20f, 20f, Vec(45), "Blue", 0);
            }
            else if (map.getAItile(intPos).acc == AItile.Accessibility.Floor)
            {
                //Create_Square(room, pos, 40f, 40f, Vec(45), "Purple", 0);

                parasite.mainBodyChunk.vel.x += moveDir.x * (touchingGround ? speed : speed * 0.1f);
                if ((room.GetTile(intPos).Terrain != Room.Tile.TerrainType.Slope && room.GetTile(intPos.x + (int)moveDir.x, intPos.y).Solid) ||
                    (room.GetTile(intTailPos).Terrain != Room.Tile.TerrainType.Slope && room.GetTile(intTailPos.x + (int)moveDir.x, intTailPos.y).Solid))
                {
                    if (room.GetTile(intPos.x + (int)moveDir.x, intPos.y + 1).Solid)
                    {
                        TryJump(room.MiddleOfTile(intPos.x, intPos.y + 2));
                    }
                    else
                    {
                        parasite.mainBodyChunk.vel.y += 6f;
                        parasite.mainBodyChunk.vel.x += 2f * moveDir.x;
                    }
                }

                if (Random.value < 0.05)
                {
                    parasite.RandomChunk.vel.y += 5f;
                }
            }

            ShortcutData scdata = room.shortcutData(intPos);
            if (scdata.shortCutType == ShortcutData.Type.Normal || scdata.shortCutType == ShortcutData.Type.RoomExit)
            {
                shortcutCooldown = 200;
                parasite.enteringShortCut = intPos;
                parasite.NPCTransportationDestination = scdata.destinationCoord;
            }
        }

        if (jumpCooldown > 0)
        {
            Vector2 jumpDir = Custom.DirVec(pos, jumpPos);

            if (jumpCooldown > jumpPoint)
            {
                parasite.RandomChunk.vel += Custom.RNV() * 3f;
                parasite.HeadChunk.vel += jumpDir * 0.5f;
                parasite.TailChunk.vel -= jumpDir * 0.5f;
            }
            else if (jumpCooldown == jumpPoint)
            {
                jumping = true;
                parasite.HeadChunk.vel += jumpDir * 50f;
                parasite.TailChunk.vel += jumpDir * 10f;
                room.PlaySound(SoundID.Big_Spider_Jump, parasite.mainBodyChunk, false, 0.3f, 2f);
            }

            jumpCooldown--;
        }
    }
    public void TryJump(Vector2 jumpPos)
    {
        if (parasite.grasps[0] == null && jumpCooldown == 0 && !jumping)
        {
            Vector2 bodyDir = Custom.DirVec(parasite.HeadChunk.pos, parasite.TailChunk.pos);
            Vector2 jumpDir = Custom.DirVec(parasite.HeadChunk.pos, jumpPos);

            if (jumpDir.y < 0.5f && Vector2.Angle(bodyDir, jumpDir) < 45)
            {
                parasite.HeadChunk.vel.y += 1;
            }
            else
            {
                jumpCooldown = maxCooldown;
                this.jumpPos = jumpPos;
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
    public override float CurrentPlayerAggression(AbstractCreature player)
    {
        if (parasite.buriedInChunk != null && parasite.buriedInChunk.owner.abstractPhysicalObject == player)
        {
            return 0f;
        }

        return base.CurrentPlayerAggression(player);
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
        CreatureTemplate.Relationship staticRelationship = StaticRelationship(creature);

        if (staticRelationship.type == CreatureTemplate.Relationship.Type.Eats)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in creature.stuckObjects)
            {
                if (stick is AbstractParasiteStick paraStick && paraStick.Parasite != parasite.abstractCreature)
                {
                    if (creature.realizedCreature != null && creature.realizedCreature.room != null)
                    {
                        //Create_Square(player.realizedCreature.room, player.realizedCreature.mainBodyChunk.segPos, 20f, 20f, Vec(45), "Red", 0);
                    }

                    return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
                }
            }

            if (creature.state.dead)
            {
                if (creature.realizedCreature != null && creature.realizedCreature.room != null)
                {
                    //Create_Square(player.realizedCreature.room, player.realizedCreature.mainBodyChunk.segPos, 20f, 20f, Vec(45), "Red", 0);
                }

                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
            }

            if (creature.realizedCreature != null && creature.realizedCreature.room != null)
            {
                //Create_Square(player.realizedCreature.room, player.realizedCreature.mainBodyChunk.segPos, 20f, 20f, Vec(45), "Green", 0);
            }
        }

        //Debug.Log(player.creatureTemplate.name + " : " + staticRelationship.type.value);

        return staticRelationship;
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
        public static Behavior Hunt = new("Hunt", true);
        public static Behavior Burrow = new("Burrow", true);
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
    public AbstractCreature Parasite
    {
        get { return A as AbstractCreature; }
    }
    public AbstractCreature OtherCreature
    {
        get { return B as AbstractCreature; }
    }
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

public class ParasiteIllnessEffect : CosmeticSprite
{
    public AbstractCreature player;
    public AbstractCreature parasite;
    public AbstractParasiteStick ParasiteStick
    {
        get
        {
            if (parasiteStick != null)
            { return parasiteStick; }

            foreach (AbstractPhysicalObject.AbstractObjectStick stick in parasite.stuckObjects)
            { if (stick is AbstractParasiteStick parasiteStick)
                {
                    this.parasiteStick = parasiteStick;
                    return parasiteStick;
                }
            }

            return null;
        }
    }
    public AbstractParasiteStick parasiteStick;
    public HUD.HUD hud;

    public FSprite sprite;
    public DisembodiedDynamicSoundLoop soundLoop;
    public bool stun, fatal, killedPlayer;

    public int timer, startTime, middleTime, fadeTime;

    public float intensity;

    new Vector2 pos;
    new Vector2 lastPos;

    Vector2 lastPlayerPos;

    float timeStacker;

    public ParasiteIllnessEffect(Room room, AbstractCreature player, AbstractCreature parasite, int startTime, int middleTime, int fadeTime, bool stun, bool fatal)
    {
        this.room = room;
        this.player = player;
        this.parasite = parasite;

        this.startTime = startTime;
        this.middleTime = middleTime;
        this.fadeTime = fadeTime;
        this.stun = stun;
        this.fatal = fatal;

        lastPlayerPos = GetPos();
        pos = lastPlayerPos;
        lastPos = pos;

        room.AddObject(this);
    }

    public override void Update(bool eu)
    {
        lastPos = pos;

        base.Update(eu);

        if (ParasiteStick == null)
        {
            Destroy();
            return;
        }

        #region Visuals

        RoomCamera roomCamera = player.world.game.cameras[0];
        if (roomCamera.room != room)
        {
            NewRoom(player.Room.realizedRoom);
        }

        lastPlayerPos = GetPos();
        pos = lastPlayerPos;

        timer++;

        if (timer < startTime)
        {
            timer++;

            intensity = Custom.SCurve((float)timer / startTime, 1f);
        }
        else if (timer < startTime + middleTime)
        {
            timer++;
        }
        else if (timer < startTime + middleTime + fadeTime)
        {
            if (fatal && player.state.alive)
            {
                KillPlayer();
            }

            timer++;

            intensity = Custom.SCurve(1f - ((float)(timer - startTime - middleTime) / fadeTime), 1f);
        }
        else
        {
            Destroy();
            return;
        }

        //Create_Text(room, pos + new Vector2(0f, 30f), intensity, "Red", 0);

        //Create_Text(room, pos + new Vector2(0f, 40f), timer, "Blue", 0);

        if (intensity > 0)
        {
            if (player.realizedCreature != null && player.state.alive && PlayerData.playerStates.ContainsKey(player.ID.number))
            {
                PlayerData.AAPlayerState state = PlayerData.playerStates[this.player.ID.number];
                Player player = this.player.realizedCreature as Player;


                player.aerobicLevel = Mathf.Max(player.aerobicLevel, intensity * ((stun || fatal) ? 2f : 1f));
                player.Blink(100);

                if (stun)
                {
                    player.Stun(50 + state.parasiteKillCounter * 20);
                }
            }

            soundLoop ??= new DisembodiedDynamicSoundLoop(this)
            {
                sound = SoundID.Reds_Illness_LOOP,
                VolumeGroup = 1
            };

            soundLoop.Update();
            soundLoop.Volume = intensity;
        }

        #endregion
    }
    public override void Destroy()
    {
        base.Destroy();
        if (soundLoop != null && soundLoop.emitter != null)
        {
            soundLoop.emitter.slatedForDeletetion = true;
        }

        PlayerData.AAPlayerState state = PlayerData.GetPlayerState(player.ID.number);
        if (state != null)
        {
            if (state.parasiteIllnessEffect == this)
            {
                state.parasiteIllnessEffect = null;
            }
        }
    }
    public void KillPlayer()
    {
        if (player.realizedCreature != null)
        {
            Player realizedPlayer = player.realizedCreature as Player;

            AbstractPhysicalObject newEgg = new(room.world, Enums.AbstractObjectType.ParasiteEgg, null, room.GetWorldCoordinate(realizedPlayer.mainBodyChunk.pos), room.game.GetNewID())
            {
                unrecognizedAttributes = ["GROW_ON_STARTUP"]
            };
            room.abstractRoom.AddEntity(newEgg);
            newEgg.RealizeInRoom();

            new AbstractParasiteEggStick(newEgg, player);

            InfectedCorpse corpse = new(player, [newEgg], true);
            room.AddObject(corpse);

            (newEgg.realizedObject as ParasiteEgg).StickInCorpse(corpse);

            realizedPlayer.Die();
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);

        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("Futile_White", true)
        {
            shader = rCam.game.rainWorld.Shaders["RedsIllness"]
        };

        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("GrabShaders"));
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        this.timeStacker = timeStacker;

        if (!slatedForDeletetion)
        {
            Vector2 pos = Vector2.Lerp(lastPos, this.pos, timeStacker);
            float fade = intensity * 0.5f;
            float scale = rCam.sSize.x;

            sprite = sLeaser.sprites[0];

            if (fade < 0)
            { sprite.isVisible = false; return; }

            sprite.isVisible = true;
            sprite.SetPosition(pos - camPos);
            sprite.scale = scale;
            sprite.color = new Color(fade * 10, fade, 0f, 0f);
        }
    }

    public Vector2 GetPos()
    {
        Vector2 pos;
        Creature player = this.player.realizedCreature;

        if (player != null)
        {
            if (player.inShortcut && player.abstractCreature.Room == room.abstractRoom)
            {
                pos = player.inShortcutVessel.SmoothPos(timeStacker);
            }
            else if (player != null)
            {
                pos = player.mainBodyChunk.pos;
            }
            else
            {
                pos = lastPlayerPos;
            }
        }
        else
        {
            pos = lastPlayerPos;
        }

        return pos;
    }
    public void NewRoom(Room newRoom)
    {
        room?.RemoveObject(this);
        room = newRoom;
        newRoom.AddObject(this);

        lastPlayerPos = GetPos();
        pos = lastPlayerPos;
        lastPos = pos;
    }
}

#endregion


#region Parasite Egg

public class ParasiteEgg : PhysicalObject, IDrawable
{
    public InfectedCorpse stuckInCorpse;
    public int chunkIndex;

    public Creature StuckInCreature
    {
        get
        {
            if (stuckInCorpse == null || stuckInCorpse.deadCreature == null || stuckInCorpse.deadCreature.realizedCreature == null)
            { return null; }

            return stuckInCorpse.deadCreature.realizedCreature;
        }
    }
    public BodyChunk StuckInChunk
    {
        get
        {
            if (StuckInCreature == null)
            { return null; }

            return StuckInCreature.bodyChunks[chunkIndex];
        }
    }

    public Vector2 ChunkRot
    {
        get
        {
            Vector2 chunkRot = Vector2.up;

            if (StuckInCreature != null)
            {
                if (chunkIndex == 0)
                {
                    chunkRot = Custom.DirVec(StuckInChunk.pos, StuckInCreature.bodyChunks[chunkIndex + 1].pos);
                }
                else
                {
                    chunkRot = Custom.DirVec(StuckInCreature.bodyChunks[chunkIndex - 1].pos, StuckInChunk.pos);
                }
            }

            return chunkRot;
        }
    }

    public Vector2 rot, lastRot;
    public float stickDeg;

    public int pulseTimer;
    public float pulseVel;
    public float pulse;

    public int hatchCounter = 0;
    public int initCounter = 0;
    public float hatchGrowth = 0;

    public int growCounter;
    public int growDelayTime;
    public int maxGrowth = 100;

    public float twitch;

    public bool hatched;

    public ChunkDynamicSoundLoop soundLoop;

    public List<Creature> frightenedCreatures = [];

    public ParasiteEgg(AbstractPhysicalObject abstractPhysicalObject, bool growOnInit) : base(abstractPhysicalObject)
    {
        Random.State state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.number);

        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0, 0), 8f, 1f);
        bodyChunkConnections = [];

        stickDeg = Random.Range(-30f, 30f);

        rot = Custom.rotateVectorDeg(Vector2.up, stickDeg);
        lastRot = rot;

        shellColor = Custom.HSL2RGB(0.1f, Random.Range(0.1f, 0.2f), Random.Range(0.6f, 0.9f));
        slimeColor = Custom.HSL2RGB(Random.Range(0.2f, 0.22f), Random.Range(0.4f, 0.6f), Random.Range(0.4f, 0.6f));

        soundLoop = new(firstChunk)
        {
            sound = SoundID.Bro_Digestion_LOOP,
            Volume = 0f
        };

        pulseTimer = Random.Range(0, 300);
        pulse = 0f;
        pulseVel = 1f;

        chunkIndex = -1;

        Random.state = state;

        CollideWithObjects = true;
        CollideWithTerrain = false;

        collisionLayer = 0;

        growCounter = growOnInit ? 0 : maxGrowth;
        if (growOnInit)
        {
            growDelayTime = Random.Range(10, 50);
        }
    }

    #region Physics Stuff

    public override void Update(bool eu)
    {
        float section = 0;

        try
        {
            section = 1f;

            base.Update(eu);

            lastRot = rot;

            if (growDelayTime > 0)
            {
                growDelayTime--;
                if (growDelayTime == 0)
                {
                    PopOutOfCorpse();
                }
            }
            else if (growCounter < maxGrowth)
            { growCounter++; }

            if (pulseTimer < 300)
            { pulseTimer++; }
            else
            { pulseTimer = 0; }

            pulse = (Mathf.Cos(2 * Mathf.PI * ((float)pulseTimer / 300)) + 1) * 0.1f;

            section = 2;

            if (room != null)
            {
                bool hatch = false;
                foreach (List<PhysicalObject> objList in room.physicalObjects)
                {
                    section = 2.1f;
                    foreach (PhysicalObject obj in objList)
                    {
                        section = 2.2f;
                        if (obj is Creature creature && !creature.dead && StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.Parasite).relationships[creature.Template.index].type == CreatureTemplate.Relationship.Type.Eats)
                        {
                            section = 2.3f;
                            foreach (BodyChunk chunk in obj.bodyChunks)
                            {
                                if (Custom.DistLess(chunk.pos, firstChunk.pos, 200))
                                {
                                    hatch = true;
                                    goto End;
                                }
                            }

                            section = 2.4f;
                            if (creature.abstractCreature != null && creature.abstractCreature.abstractAI != null && creature.abstractCreature.abstractAI.RealAI != null)
                            {
                                section = 2.5f;
                                ArtificialIntelligence intelligence = creature.abstractCreature.abstractAI.RealAI;
                                if (intelligence.threatTracker != null && intelligence.VisualContact(room.GetWorldCoordinate(firstChunk.pos), 10))
                                {
                                    section = 2.6f;

                                    if (!frightenedCreatures.Contains(creature))
                                    {
                                        section = 2.7f;

                                        intelligence.threatTracker.AddThreatPoint(null, room.GetWorldCoordinate(firstChunk.pos), 700f);

                                        section = 2.8f;

                                        frightenedCreatures.Add(creature);
                                    }
                                }
                            }
                        }
                    }
                }
                End:;

                section = 3;

                if (hatch)
                {
                    if (hatchCounter > 100)
                    {
                        Hatch(false);
                    }
                    else
                    {
                        hatchCounter++;
                    }
                }
                else
                {
                    if (hatchCounter > 0)
                    {
                        hatchCounter--;
                    }
                }
                hatchGrowth = hatchCounter * 0.001f;

                section = 4;

                if (hatchCounter > 0 || growCounter < maxGrowth)
                {
                    twitch = Mathf.Clamp(twitch + (Random.value < 0.5 ? 0.05f : -0.05f), 10f, -10f);
                }
                else
                {
                    twitch = 0;
                }

                section = 5;

                if (StuckInCreature != null)
                {
                    Vector2 perpChunkRot = Custom.PerpendicularVector(ChunkRot);
                    if (perpChunkRot.y < 0)
                    { perpChunkRot.y *= -1; }
                    rot = Custom.rotateVectorDeg(perpChunkRot, stickDeg + (twitch * ((float)hatchCounter / 100)));

                    if (StuckInCreature is Player player)
                    {
                        PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
                        firstChunk.pos = Vector2.Lerp(graphics.drawPositions[0, 0], graphics.drawPositions[1, 0], 0.5f) + rot * StuckInChunk.rad;
                    }
                    else
                    {
                        firstChunk.pos = StuckInChunk.pos + rot * StuckInChunk.rad;
                    }
                }
                else
                {
                    rot = Custom.rotateVectorDeg(Vector2.up, stickDeg + (twitch * ((float)hatchCounter / 100)));
                }
            }

            section = 6;

            soundLoop.Volume = Mathf.Max((float)hatchCounter / 100, (growDelayTime == 0 && growCounter < maxGrowth) ? 0.5f : 0f);
            soundLoop.Update();
        }
        catch (Exception e)
        {
            Log_Exception(e, "PARASITEEGG_UPDATE", section);
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);

        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);

        Hatch(false);
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);

        Hatch(true);
    }

    public void StickInCorpse(InfectedCorpse infectedCorpse)
    {
        Random.State state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.number);

        stuckInCorpse = infectedCorpse;

        bool foundSpace = false;
        for (int i = 0; i < 10; i++)
        {
            int chunkIndex = Random.Range(1, StuckInCreature.bodyChunks.Length);

            bool spaceTaken = false;
            foreach (AbstractPhysicalObject egg in stuckInCorpse.eggs)
            {
                if ((egg.realizedObject as ParasiteEgg).chunkIndex == chunkIndex)
                {
                    spaceTaken = true;
                    break;
                }
            }

            if (!spaceTaken)
            {
                this.chunkIndex = chunkIndex;
                foundSpace = true;
            }
        }

        if (!foundSpace)
        {
            for (int i = 0; i < StuckInCreature.bodyChunks.Length; i++)
            {
                bool spaceTaken = false;
                foreach (AbstractPhysicalObject egg in stuckInCorpse.eggs)
                {
                    if ((egg.realizedObject as ParasiteEgg).chunkIndex == i)
                    {
                        spaceTaken = true;
                        break;
                    }
                }

                if (!spaceTaken)
                {
                    chunkIndex = i;
                    foundSpace = true;
                }
            }
        }

        if (!foundSpace)
        {
            Destroy();
        }

        Random.state = state;
    }

    public void Hatch(bool explosion)
    {
        float section = 0;
        try
        {
            if (hatched)
            {
                return;
            }

            section = 1;

            EggWaterJet waterJet = new(room, firstChunk.pos, 5);
            room.AddObject(waterJet);

            section = 2;

            room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk.pos, 1f, 1f);
            room.PlaySound(SoundID.Splashing_Water_Into_Water_Surface, firstChunk.pos, 1f, 1f);

            section = 3;

            for (int i = 0; i < Random.Range(5, 7); i++)
            {
                PuffBallSkin piece = new(firstChunk.pos + Custom.RNV() * Random.Range(5, 0), Custom.RNV() * Random.Range(1f, 5f), eggSprite.color, eggSprite.color);
                room.AddObject(piece);
            }

            section = 4;

            for (int i = 0; i < Random.Range(3, 4); i++)
            {
                AbstractCreature parasite = new(room.world, StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.Parasite), null, abstractPhysicalObject.pos, room.game.GetNewID());
                room.abstractRoom.AddEntity(parasite);
                parasite.Realize();

                Parasite realizedParasite = parasite.realizedCreature as Parasite;
                realizedParasite.PlaceInRoom(room);
                realizedParasite.firstChunk.vel += Custom.RNV() * 10f;

                if (explosion)
                {
                    realizedParasite.Die();

                    room.AddObject(new Smoke.Smolder(room, realizedParasite.firstChunk.pos, realizedParasite.firstChunk, null));
                }
            }

            section = 5;

            hatched = true;

            if (!slatedForDeletetion)
            {
                Destroy();
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "EGG_EXPLODE", section);
        }
    }

    public void PopOutOfCorpse()
    {
        room.PlaySound(SoundID.Spear_Stick_In_Creature, firstChunk.pos, 1f, 1f);
        room.PlaySound(SoundID.Splashing_Water_Into_Water_Surface, firstChunk.pos, 1f, 1f);

        for (int i = 0; i < Random.Range(3, 5); i++)
        {
            WaterDrip drip = new(firstChunk.pos, Custom.RNV() * Random.Range(1f, 5f), false);
            room.AddObject(drip);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
    }

    #endregion

    #region Graphics Stuff
    public FSprite eggSprite;
    public FSprite mudSprite;
    public Color shellColor;
    public Color slimeColor;
    public Color blackColor;

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        eggSprite = new FSprite("BodyA", true)
        {
            scale = 1f,
            color = shellColor
        };
        sprites.Add(eggSprite);

        mudSprite = MudUtils.MakeMudSprite(rCam, eggSprite);
        sprites.Add(mudSprite);
        
        sLeaser.sprites = [..sprites];

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
            Vector2 rot = Vector2.Lerp(lastRot, this.rot, timeStacker);

            float growthScale = growDelayTime > 0 ? 0f : (float)growCounter / maxGrowth;

            eggSprite.SetPosition(pos);
            eggSprite.rotation = Custom.VecToDeg(rot) + 180;
            eggSprite.scale = (1f + pulse + hatchGrowth) * growthScale * 0.8f;

            mudSprite.SetPosition(pos);
            mudSprite.rotation = eggSprite.rotation;
            mudSprite.scale = eggSprite.scale;

            if (mudSprite is MudUtils.MudOverlaySprite overlaySprite)
            {
                FAtlasElement element = eggSprite.element;
                float width = element.sourceRect.width;
                float height = element.sourceRect.height;

                Vector2[] vertices = overlaySprite.vertices;
                vertices[0] = new Vector2(-width / 2, height / 2);
                vertices[1] = new Vector2(width / 2, height / 2);
                vertices[2] = new Vector2(width / 2, -height / 2);
                vertices[3] = new Vector2(-width / 2, -height / 2);

                overlaySprite.packedColor = MudUtils.PackColor(slimeColor);
                overlaySprite.alpha = Mathf.Lerp(0.2f, 1f, hatchCounter / 100f);
            }

            eggSprite.color = Color.Lerp(shellColor, blackColor, room.Darkness(pos));
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    #endregion

    public class EggWaterJet : UpdatableAndDeletable
    {
        public Vector2 pos;

        public int maxTime;
        public int timer;

        public WaterJet waterJet;

        public EggWaterJet(Room room, Vector2 pos, int maxTime)
        {
            this.maxTime = maxTime;
            this.pos = pos;

            waterJet = new(room);
        }
        public override void Update(bool eu)
        {
            base.Update(eu);

            if (timer > maxTime)
            {
                Destroy();
                return;
            }
            else
            {
                timer++;
            }

            waterJet.NewParticle(pos, Custom.rotateVectorDeg(Vector2.up, Random.Range(45, -45)) * 10, 5f, 5f);
        }
    }
}

public class InfectedCorpse : UpdatableAndDeletable
{
    public AbstractCreature deadCreature;
    public List<AbstractPhysicalObject> eggs;
    public Color slimeColor;
    public int slime;

    public InfectedCorpse(AbstractCreature deadCreature, List<AbstractPhysicalObject> eggs, bool startup)
    {
        Random.State state = Random.state;
        Random.InitState(deadCreature.ID.number);

        this.deadCreature = deadCreature;
        this.eggs = eggs;
        
        slime = startup ? 0 : 500;

        foreach (AbstractPhysicalObject egg in eggs)
        {
            if (egg.realizedObject != null)
            {
                ParasiteEgg realizedEgg = egg.realizedObject as ParasiteEgg;
                realizedEgg.stuckInCorpse = this;
            }
        }

        slimeColor = Custom.HSL2RGB(Random.Range(0.2f, 0.22f), Random.Range(0.4f, 0.6f), Random.Range(0.4f, 0.6f));

        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (deadCreature.Room.realizedRoom != null && deadCreature.Room.realizedRoom != room)
        {
            room = deadCreature.Room.realizedRoom;
        }

        if (deadCreature != null && deadCreature.slatedForDeletion)
        {
            foreach (AbstractPhysicalObject egg in eggs)
            {
                if (egg.realizedObject != null)
                {
                    (egg.realizedObject as ParasiteEgg).Destroy();
                }
            }
        }

        deadCreature.realizedCreature.mudColor = slimeColor;
        deadCreature.realizedCreature.muddy = slime;

        if (slime < 500)
        { slime += 10; }
    }
}

public class AbstractParasiteEggStick : AbstractPhysicalObject.AbstractObjectStick
{

    public AbstractParasiteEggStick(AbstractPhysicalObject egg, AbstractPhysicalObject corpse) : base(egg, corpse)
    {
    }

    public AbstractPhysicalObject Egg
    {
        get { return A; }
    }
    public AbstractPhysicalObject Corpse
    {
        get { return B; }
    }

    public override string SaveToString(int roomIndex)
    {
        return string.Concat(
        [
            roomIndex.ToString(),
            "<stkA>paraEggStk<stkA>",
            A.ID.ToString(),
            "<stkA>",
            B.ID.ToString()
        ]);
    }
}

public class InfectedCorpseData : PlacedObject.Data
{
    public Vector2 panelPos;
    public float spawnChance;
    public int creatureIndex;

    public InfectedCorpseData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        spawnChance = 1;
        creatureIndex = 0;
    }

    protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}", new object[]
        {
            panelPos.x,
            panelPos.y,
            spawnChance,
            creatureIndex
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        spawnChance = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        creatureIndex = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4);
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class InfectedCorpseRepresentation : DevInterface.PlacedObjectRepresentation
{
    public InfectedCorpseData Data
    {
        get
        {
            return pObj.data as InfectedCorpseData;
        }
    }

    public InfectedCorpsePanel controlPanel;

    public InfectedCorpseRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        controlPanel = new(owner, "InfectedCorpse_Panel", this, Data.panelPos, new Vector2(250f, 45f), "Infected Corpse");

        subNodes.Add(controlPanel);
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        owner.placedObjectsContainer.AddChild(fSprites[1]);
    }

    public override void Refresh()
    {
        MoveSprite(1, absPos);
        fSprites[1].scaleY = controlPanel.pos.magnitude;
        fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);

        base.Refresh();
    }

    public class InfectedCorpsePanel : Panel
    {
        public DevObjects.KeyboardInput creatureNameInput;
        public InfectedCorpseSlider spawnChanceSlider;

        public InfectedCorpseData Data
        {
            get
            {
                return (parentNode as InfectedCorpseRepresentation).Data;
            }
        }

        public InfectedCorpsePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string name) :
            base(owner, IDstring, parentNode, pos, size, name)
        {
            creatureNameInput = new DevObjects.KeyboardInput(owner, "Creature", this, new Vector2(5f, 5f), "Creature:");
            subNodes.Add(creatureNameInput);

            spawnChanceSlider = new InfectedCorpseSlider(owner, "SpawnChance", this, new Vector2(5f, 25f), "Spawn Chance: ", false, 110f, 32f);
            subNodes.Add(spawnChanceSlider);

            if (Data.creatureIndex < 0)
            {
                creatureNameInput.inputBox.input = "INPUT NAME";
            }
            else
            {
                creatureNameInput.inputBox.input = StaticWorld.creatureTemplates[Data.creatureIndex].type.value;
            }

            CheckValidity();
        }

        public override void Refresh()
        {
            Data.panelPos = pos;

            if (creatureNameInput.inputBox.clicked)
            {
                CheckValidity();
            }

            base.Refresh();
        }

        public void CheckValidity()
        {
            bool devTools = ModManager.DevTools;
            ModManager.DevTools = false;
            CreatureTemplate.Type creatureType = WorldLoader.CreatureTypeFromString(creatureNameInput.Value);
            ModManager.DevTools = devTools;

            if 
                (creatureType != null && creatureType.index >= 0
                && creatureType != CreatureTemplate.Type.StandardGroundCreature
                && creatureType != CreatureTemplate.Type.LizardTemplate
                )
            {
                creatureNameInput.inputBox.validString = true;
                Data.creatureIndex = creatureType.index;
            }
            else
            {
                creatureNameInput.inputBox.validString = false;
            }
        }

        public class InfectedCorpseSlider : DevObjects.CustomSlider
        {
            public InfectedCorpseData Data
            {
                get
                {
                    return (parentNode.parentNode as InfectedCorpseRepresentation).Data;
                }
            }

            public InfectedCorpseSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, bool inheritButton, float titleWidth, float numberWidth)
            : base(owner, IDstring, parentNode, pos, title, inheritButton, titleWidth, numberWidth)
            {

            }

            public override void NubDragged(float nubPos)
            {
                base.NubDragged(nubPos);

                Data.spawnChance = nubPos;
            }

            public override void Refresh()
            {
                base.Refresh();

                NumberText = string.Format("{0:N0}%", Data.spawnChance * 100);

                RefreshNubPos(Data.spawnChance);
            }
        }
    }
}

#endregion
