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
using DevInterface;
using ArchdruidsAdditions.Creatures;
using System.Runtime.InteropServices;

namespace ArchdruidsAdditions.Objects;

public class LightningFruit : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public int bites = 3;

    public Vector2 rotation, lastRotation, lightningRotation;

    public int power = 1000;
    public int lightningPower = 0;
    public int charge;

    public bool shockBiter = false;
    public int shockBiterTimer = 0;

    public bool attachedToVine;

    public Creature lastHolder;

    public LightningFruitVine vine;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return this.abstractPhysicalObject as AbstractConsumable;
        }
    }

    public LightningFruit(AbstractPhysicalObject abstractPhysicalObject, int charge) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, default, 6f, 0.2f);
        bodyChunkConnections = [];

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        gravity = 0.9f;
        collisionLayer = 1;

        bodyColor = Custom.HSL2RGB(0.65f, 1f, 0.5f);
        shineColor = Color.Lerp(bodyColor, Color.white, 0.3f);

        rotation = Vector2.up;
        lightningRotation = Vector2.up;

        this.charge = charge;

        Random.State state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);

        Color baseFruitColor = charge == 1 ? Custom.HSL2RGB(0.65f, 1f, 0.5f) : Custom.HSL2RGB(0f, 1f, 0.5f);
        float baseColorSpread = 0.05f;

        try
        {
            string regionColorString = Plugin.RegionData.ReadRegionData(abstractPhysicalObject.world.region.name, charge == 1 ? "LightningFruitColorA" : "LightningFruitColorB");
            if (regionColorString != null)
            {
                if (regionColorString.StartsWith("#"))
                {
                    regionColorString = regionColorString.Remove(0, 1);
                }
                baseFruitColor = Custom.hexToColor(regionColorString);
            }
            string colorSpread = Plugin.RegionData.ReadRegionData(abstractPhysicalObject.world.region.name, "LightningFruitColorSpread");
            if (colorSpread != null)
            {
                baseColorSpread = float.Parse(colorSpread);
            }
        }
        catch (Exception e)
        {
            Debug.Log("---OBJECT \'LIGHTNINGFRUIT\' EXPERIENCED AN EXCEPTION WHILE TRYING TO GET REGION PROPERTIES DATA. IS THE FILE FORMATTED CORRECTLY?---");
            Debug.LogException(e);
        }

        Vector3 HSLFruitColor = Custom.RGB2HSL(baseFruitColor);
        float spreadedHue = Random.Range(HSLFruitColor.x - baseColorSpread, HSLFruitColor.x + baseColorSpread);

        bodyColor = Custom.HSL2RGB(spreadedHue, HSLFruitColor.y, HSLFruitColor.z);
        shineColor = Color.Lerp(bodyColor, Color.white, 0.3f);

        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        if (power > 0)
        {
            if (!attachedToVine && Random.value < 0.1f)
            {
                power--;
            }

            if (Random.value < power / 10000f)
            {
                DecorativeSparkle(5);
            }

            if (Submersion > 0f)
            {
                ReleasePower();
                room.AddObject(new UnderwaterShock(room, this, firstChunk.pos, 14, 200f, 0.1f, lastHolder ?? null, bodyColor));
            }
        }

        if (lightningPower > 0)
        {
            lightningPower--;
        }

        if (grabbedBy.Count == 0 && firstChunk.vel.magnitude > 1)
        {
            rotation = Custom.rotateVectorDeg(rotation, firstChunk.vel.magnitude);
        }
        else if (grabbedBy.Count > 0)
        {
            Creature.Grasp grasp = grabbedBy[0];
            lastHolder = grasp.grabber;
            rotation = Vector2.up;

            if (shockBiter)
            {
                shockBiterTimer++;
                if (shockBiterTimer > 2)
                {
                    shockBiter = false;
                    shockBiterTimer = 0;
                    room.PlaySound(SoundID.Centipede_Shock, firstChunk.pos, 1f, 1f);

                    DecorativeSparkle(5);

                    for (int i = 0; i < Random.Range(1, 4); i++)
                    { room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * Random.Range(4f, 15f), bodyColor, null, 8, 14)); }

                    grasp.grabber.Stun(Random.Range(200, 400));
                    room.AddObject(new CreatureSpasmer(grasp.grabber, false, grasp.grabber.stun));
                    grasp.grabber.LoseAllGrasps();
                }
            }
        }

        //Methods.Methods.Create_Text(room, firstChunk.pos, power.ToString(), "Red", 0);
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (otherObject is LightningFruit otherFruit && otherFruit.charge != charge)
        {
            if (power > 0 && otherFruit.power > 0)
            {
                ReleasePower();
                otherFruit.ReleasePower();
            }
        }
    }

    public override void Destroy()
    {
        base.Destroy();

        if (vine != null)
        { vine.Destroy(); }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        if (attachedToVine && vine != null)
        {
            vine.releaseCounter = 1;
        }
    }

    public void ReleasePower()
    {
        if (power > 0)
        {
            power = 0;

            DecorativeSparkle(20);

            room.PlaySound(SoundID.Centipede_Shock, firstChunk.pos, 1f, 1f);

            for (int i = 0; i < Random.Range(1, 4); i++)
            { room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * Random.Range(4f, 15f), bodyColor, null, 8, 14)); }
        }
    }

    public void DecorativeSparkle(int time)
    {
        lightningRotation = Custom.rotateVectorDeg(lightningRotation, Random.Range(-25f, 25f));
        lightningPower = time;
    }

    public void DetachFromVine()
    {
        vine.fruitAttached = false;
        vine = null;

        attachedToVine = false;

        AbstrConsumable.Consume();
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;

        if (!AbstrConsumable.isConsumed)
        {
            bodyChunks[0].HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);

            vine = new(room, this);
            room.AddObject(vine);
            attachedToVine = true;
        }
        else
        {
            bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        }
    }

    public void ThrowByPlayer()
    {
    }

    #region Consumable Stuff
    public int BitesLeft
    { get { return bites; } }

    public int FoodPoints
    { get { return 1; } }

    public bool Edible
    { get { return true; } }

    public bool AutomaticPickUp
    { get { return true; } }

    public void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        if (power > 0)
        {
            shockBiter = true;
        }
        else
        {
            bites--;

            if (bites == 0) { room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk.pos); }
            else { room.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, firstChunk.pos); }

            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }
    }
    #endregion

    #region Graphics Stuff
    public Color bodyColor, shineColor, blackColor;
    public CircularSprite body;
    public CircularSprite shine;
    public FSprite backgroundGlow;
    public FSprite foregroundGlow;
    public FSprite lightning;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        body = new("SphericalFruit1")
        {
            scale = 0.5f
        };
        body.SetPosition(new Vector2(-100, 100));
        body.color = bodyColor;
        sprites.Add(body);

        shine = new("Futile_White")
        {
            scale = 0.35f
        };
        shine.SetPosition(new Vector2(-100, 100));
        shine.color = shineColor;
        sprites.Add(shine);

        lightning = new("Futile_White")
        {
            scale = 1.5f
        };
        lightning.SetPosition(new Vector2(-100, 100));
        lightning.shader = rCam.room.game.rainWorld.Shaders["LightningBolt"];
        lightning.alpha = 1f;
        sprites.Add(lightning);

        backgroundGlow = new("Futile_White")
        {
            scale = 5f
        };
        backgroundGlow.SetPosition(new Vector2(-100, 100));
        backgroundGlow.color = bodyColor;
        backgroundGlow.shader = rCam.room.game.rainWorld.Shaders["LightSource"];
        sprites.Add(backgroundGlow);

        foregroundGlow = new("Futile_White")
        {
            scale = 2f
        };
        foregroundGlow.SetPosition(new Vector2(-100, 100));
        foregroundGlow.color = bodyColor;
        foregroundGlow.shader = rCam.room.game.rainWorld.Shaders["FlatLight"];
        sprites.Add(foregroundGlow);

        sLeaser.sprites = sprites.ToArray();

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
            Vector2 chunkPos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
            Vector2 rotVec = Vector2.Lerp(lastRotation, rotation, timeStacker);

            Vector2 shinePos = chunkPos + Custom.DegToVec(-45) * 2f;

            body.SetPosition(chunkPos);
            body.element = Futile.atlasManager.GetElementWithName("SphericalFruit" + (4 - bites).ToString());

            shine.SetPosition(shinePos);
            if (bites == 1)
            { shine.isVisible = false; }

            backgroundGlow.SetPosition(chunkPos);
            backgroundGlow.alpha = power / 1000f;
            foregroundGlow.SetPosition(chunkPos);
            foregroundGlow.alpha = power / 3000f;

            lightning.SetPosition(chunkPos);
            lightning.rotation = Custom.VecToDeg(lightningRotation);
            lightning.color = Custom.HSL2RGB(1f, 1f, lightningPower / 5f);

            (Color lightColor, float lightExposure, float colorExposure) = TrueLightColorAndExposure(room, chunkPos + camPos, power / 1000f);
            Color baseColor = Color.Lerp(this.bodyColor, lightColor, Mathf.Clamp(Mathf.Min(lightExposure, colorExposure), 0, 0.3f));

            Vector3 vecBodyColor = Custom.RGB2HSL(baseColor);
            Color bodyColor = Color.Lerp(blackColor, baseColor, lightExposure);
            Color shineColor = Color.Lerp(blackColor, Custom.HSL2RGB(vecBodyColor.x, vecBodyColor.y, Mathf.Clamp(vecBodyColor.z + (0.1f * (1 - vecBodyColor.z)), 0f, 1f)), lightExposure);

            body.color = bodyColor;
            shine.color = shineColor;

            if (blink > 0)
            {
                body.color = Color.white;
                shine.color = Color.white;
            }
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }

        rCam.ReturnFContainer("Water").AddChild(backgroundGlow);
        rCam.ReturnFContainer("Water").AddChild(foregroundGlow);
        rCam.ReturnFContainer("Water").AddChild(lightning);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}

public class LightningFruitVine : UpdatableAndDeletable, IDrawable
{
    public Color blackColor;
    public Color fruitColor;
    public LightningFruit fruit;
    public bool fruitAttached;
    public int releaseCounter;

    public Vector2[] segPosList;
    public Vector2[] segLastPosList;
    public Vector2[] segVelList;

    public Vector2 stuckPos1;
    public Vector2 stuckPos2;
    public float vineLength;
    public float segmentLength;
    public int numOfSegments;
    public bool decorative;
    public float elasticity;

    public TriangleMesh vineMesh;

    public CircularSprite[] rustBalls;
    public float[] rustBallPosList;
    public bool[] rustBallColored;

    public FSprite[] exposedWires;
    public FSprite[] wireGlows;
    public float[] wireDegs;

    public int StartIndex
    {
        get
        { return 0; }
    }
    public int EndIndex
    {
        get
        { return segPosList.Length - 1; }
    }

    public LightningFruitVine(Room room, LightningFruit fruit)
    {
        this.room = room;
        this.fruit = fruit;

        elasticity = 0f;

        stuckPos2 = fruit.firstChunk.pos;
        IntVector2 startCheckPos = room.GetTilePosition(stuckPos2);
        for (int i = startCheckPos.y; i < room.TileHeight + 10; i++)
        {
            IntVector2 checkPos = new(startCheckPos.x, i);
            stuckPos1 = new(stuckPos2.x, 10f + i * 20f);
            if (room.GetTile(checkPos).Solid)
            {
                break;
            }
        }

        decorative = false;
        fruitAttached = true;

        fruitColor = fruit.bodyColor;

        Random.State state = Random.state;
        Random.InitState(fruit.abstractPhysicalObject.ID.number);

        InitializeVine();

        Random.state = state;
    }
    public LightningFruitVine(Room room, Vector2 startPos, Vector2 endPos, int charge, float elasticity, int seed)
    {
        this.room = room;
        this.elasticity = elasticity;

        stuckPos1 = startPos;
        stuckPos2 = endPos;

        decorative = true;
        fruitAttached = false;

        Random.State state = Random.state;
        Random.InitState(seed);

        Color baseFruitColor = charge == 1 ? Custom.HSL2RGB(0.65f, 1f, 0.5f) : Custom.HSL2RGB(0f, 1f, 0.5f);
        float baseColorSpread = 0.05f;

        int section = 0;
        try
        {
            string regionColorString = Plugin.RegionData.ReadRegionData(room.world.region.name, charge == 1 ? "LightningFruitColorA" : "LightningFruitColorB");
            if (regionColorString != null)
            {
                section = 1;
                if (regionColorString.StartsWith("#"))
                {
                    regionColorString = regionColorString.Remove(0, 1);
                }
                baseFruitColor = Custom.hexToColor(regionColorString);
            }
            string colorSpread = Plugin.RegionData.ReadRegionData(room.world.region.name, "LightningFruitColorSpread");
            if (colorSpread != null)
            {
                section = 2;
                baseColorSpread = float.Parse(colorSpread);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Exception occured when trying to get the region color data for Lightning Fruit in section: " + section + "! Is the region properties file formatted correctly? Check the Steam Workshop page for more information.");
            Debug.LogException(e);
        }

        Vector3 HSLFruitColor = Custom.RGB2HSL(baseFruitColor);
        float spreadedHue = Random.Range(HSLFruitColor.x - baseColorSpread, HSLFruitColor.x + baseColorSpread);

        fruitColor = Custom.HSL2RGB(spreadedHue, HSLFruitColor.y, HSLFruitColor.z);

        InitializeVine();

        Random.state = state;
    }
    public void InitializeVine()
    {
        float vineLength = Custom.Dist(stuckPos1, stuckPos2) + elasticity;
        this.vineLength = vineLength;
        int numOfSegments = Math.Max(Mathf.RoundToInt(vineLength / 15f), 2);
        this.numOfSegments = numOfSegments;

        segmentLength = vineLength / Mathf.Pow(numOfSegments, 1.1f);

        segPosList = new Vector2[numOfSegments];
        segLastPosList = new Vector2[numOfSegments];
        segVelList = new Vector2[numOfSegments];

        segPosList[StartIndex] = stuckPos1;
        segPosList[EndIndex] = stuckPos2;

        if (decorative)
        {
            float elasticity = this.elasticity * 100f;

            Vector2 midPos = Vector2.Lerp(segPosList[StartIndex], segPosList[EndIndex], 0.5f);
            Vector2 minHeightPos = midPos + Vector2.down * elasticity;

            Vector2 bezierBaseDir = Custom.DirVec(segPosList[StartIndex], segPosList[EndIndex]);
            float lowestStartPos = Mathf.Min(segPosList[StartIndex].y, segPosList[EndIndex].y);

            Vector2 bezierFinalDir = Vector2.Lerp(bezierBaseDir, new Vector2(bezierBaseDir.x, 0f).normalized, elasticity / Mathf.Clamp(midPos.y - lowestStartPos, 0f, 1000f));
            float bezierStretch = vineLength / 6;

            for (int i = 1; i < segPosList.Length - 1; i++)
            {
                float pos = (float)i / segPosList.Length;
                Vector2 newSegPos = Custom.Bezier(segPosList[StartIndex], minHeightPos - bezierFinalDir * bezierStretch, segPosList[EndIndex], minHeightPos + bezierFinalDir * bezierStretch, pos);

                segLastPosList[i] = segPosList[i];
                segPosList[i] = newSegPos;
            }
        }
        else
        {
            for (int i = 1; i < segPosList.Length - 1; i++)
            {
                float pos = (float)i / segPosList.Length;
                Vector2 firstRopePos = Vector2.Lerp(segPosList[EndIndex], segPosList[StartIndex], pos);
                segPosList[i] = firstRopePos;
            }
        }

        for (int i = 0; i < segLastPosList.Length; i++)
        { segLastPosList[i] = segPosList[i]; }

        for (int i = 0; i < segVelList.Length; i++)
        { segVelList[i] = Vector2.zero; }

        int numOfColoredBalls = 0;
        rustBalls = new CircularSprite[numOfSegments];
        rustBallPosList = new float[rustBalls.Length];
        rustBallColored = new bool[rustBalls.Length];
        for (int i = 1; i < rustBallPosList.Length; i++)
        {
            rustBallPosList[i] = Random.Range(0, vineLength);
            rustBallColored[i] = Random.value < 0.2f;

            if (rustBallColored[i])
            {
                numOfColoredBalls++;
            }
        }
        exposedWires = new FSprite[numOfColoredBalls * 2];
        wireGlows = new FSprite[numOfColoredBalls];
        wireDegs = new float[exposedWires.Length];
        for (int i = 0; i < wireDegs.Length; i++)
        { wireDegs[i] = Random.Range(0f, 360f); }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (!decorative)
        {
            for (int i = 0; i < segPosList.Length; i++)
            {
                segLastPosList[i] = segPosList[i];
                segPosList[i] += segVelList[i];
                segVelList[i] *= 0.99f;
                segVelList[i].y -= 0.9f;
            }

            ConnectWallSegments();
            for (int i = 0; i < segPosList.Length; i++)
            {
                if (i == StartIndex)
                {
                    ConnectSegments(i, i + 1);
                }
                else if (i == EndIndex)
                {
                    ConnectSegments(i, i - 1);
                }
                else
                {
                    ConnectSegments(i, i + 1);
                    ConnectSegments(i, i - 1);
                }
            }
            ConnectWallSegments();

            if (fruitAttached)
            {
                if (releaseCounter > 0)
                {
                    releaseCounter++;
                    if (releaseCounter > 30)
                    {
                        fruit.DetachFromVine();
                    }
                }
                if (Custom.Dist(segPosList[StartIndex + 1], fruit.firstChunk.pos) > segmentLength)
                {
                    float pullDist = Vector2.Distance(segPosList[StartIndex + 1], fruit.firstChunk.pos);
                    Vector2 pullDir = Custom.DirVec(segPosList[StartIndex + 1], fruit.firstChunk.pos) * (pullDist - segmentLength);

                    if (pullDist > segmentLength * 2)
                    {
                        fruit.DetachFromVine();
                    }
                    else
                    {
                        segPosList[StartIndex] += pullDir * 0.75f;
                        segVelList[StartIndex] += pullDir * 0.75f;
                        fruit.firstChunk.vel *= 0.95f;
                        fruit.firstChunk.vel -= pullDir * 0.25f;
                    }
                }
            }
        }
    }
    public void ConnectSegments(int A, int B)
    {
        Vector2 dirVec = Custom.DirVec(segPosList[A], segPosList[B]);
        float dist = Custom.Dist(segPosList[A], segPosList[B]);
        float dist2 = Mathf.InverseLerp(0f, segmentLength, dist);

        Vector2 newDir = dirVec * (dist - segmentLength) * 0.5f;

        segPosList[A] += newDir;
        segVelList[A] += newDir;
        segPosList[B] -= newDir;
        segVelList[B] -= newDir;
    }
    public void ConnectWallSegments()
    {
        if (fruitAttached)
        {
            segPosList[StartIndex] = fruit.firstChunk.pos;
            segVelList[StartIndex] *= 0f;
        }
        else if (decorative)
        {
            segPosList[StartIndex] = stuckPos2;
            segVelList[StartIndex] *= 0f;
        }

        segPosList[EndIndex] = stuckPos1;
        segVelList[EndIndex] *= 0f;
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        blackColor = rCam.currentPalette.blackColor;

        List<FSprite> sprites = [];

        vineMesh = TriangleMesh.MakeLongMesh(numOfSegments, false, false);
        vineMesh.color = rCam.currentPalette.blackColor;
        sprites.Add(vineMesh);

        int wireIndex = 0;
        int glowIndex = 0;
        for (int i = 0; i < rustBalls.Length; i++)
        {
            if (rustBallColored[i])
            {
                FSprite wire1 = new("deerEyeA2")
                { color = fruitColor };

                FSprite wire2 = new("deerEyeA2")
                { color = fruitColor };


                exposedWires[wireIndex] = wire1;
                exposedWires[wireIndex + 1] = wire2;
                sprites.Add(exposedWires[wireIndex]);
                sprites.Add(exposedWires[wireIndex + 1]);

                wireIndex += 2;
            }

            CircularSprite rustBall = new("SphericalFruit1")
            {
                color = rustBallColored[i] ? fruitColor : blackColor,
                scale = 0.15f,
            };
            rustBalls[i] = rustBall;
            sprites.Add(rustBalls[i]);

            if (rustBallColored[i])
            {
                FSprite wireGlow = new("Futile_White")
                {
                    color = fruitColor,
                    shader = rCam.room.game.rainWorld.Shaders["FlatLight"],
                    alpha = 0.1f,
                    scale = 2f
                };
                wireGlows[glowIndex] = wireGlow;
                sprites.Add(wireGlows[glowIndex]);

                glowIndex++;
            }
        }

        sLeaser.sprites = sprites.ToArray();

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
            Vector2 lastSegmentPos = Vector2.Lerp(segLastPosList[0], segPosList[0], timeStacker) - camPos;
            for (int i = 0; i < segPosList.Length; i++)
            {
                Vector2 segmentPos = Vector2.Lerp(segLastPosList[i], segPosList[i], timeStacker) - camPos;
                if (i == StartIndex && fruitAttached)
                {
                    segmentPos = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker) - camPos;
                    lastSegmentPos = segmentPos;
                }

                Vector2 segmentRot = Custom.DirVec(segmentPos, lastSegmentPos);
                Vector2 perpRot = Custom.PerpendicularVector(segmentRot);

                float vineRad = 1f;

                vineMesh.MoveVertice(i * 4, lastSegmentPos - perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 1, lastSegmentPos + perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 2, segmentPos - perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 3, segmentPos + perpRot * vineRad);
                vineMesh.color = blackColor;

                lastSegmentPos = segmentPos;
            }

            int exposedWireIndex = 0;
            int wireGlowIndex = 0;
            for (int i = 0; i < rustBalls.Length; i++)
            {
                CircularSprite ball = rustBalls[i];
                float ballPosFloat = rustBallPosList[i];

                int segIndex1 = 0;
                int segIndex2 = 0;
                float segPosFloat1 = 0;
                float segPosFloat2 = 0;

                for (int j = 0; j < segPosList.Length - 1; j++)
                {
                    float checkPosFloat1 = (float)j / (segPosList.Length - 1) * vineLength;
                    float checkPosFloat2 = (float)(j + 1) / (segPosList.Length - 1) * vineLength;

                    if (ballPosFloat >= checkPosFloat1 && ballPosFloat <= checkPosFloat2)
                    {
                        segIndex1 = j;
                        segIndex2 = j + 1;
                        segPosFloat1 = checkPosFloat1;
                        segPosFloat2 = checkPosFloat2;

                        break;
                    }
                }

                Vector2 segPos1 = Vector2.Lerp(segLastPosList[segIndex1], segPosList[segIndex1], timeStacker) - camPos;
                Vector2 segPos2 = Vector2.Lerp(segLastPosList[segIndex2], segPosList[segIndex2], timeStacker) - camPos;
                Vector2 perpSegRot = Custom.PerpendicularVector(Custom.DirVec(segPos2, segPos1));

                float inSegBallPos = (segPosFloat2 - segPosFloat1) / ballPosFloat;
                Vector2 ballPos = Vector2.Lerp(segPos1, segPos2, inSegBallPos);

                ball.SetPosition(ballPos);
                ball.color = rustBallColored[i] ? fruitColor : blackColor;

                if (rustBallColored[i])
                {
                    FSprite wire1 = exposedWires[exposedWireIndex];
                    FSprite wire2 = exposedWires[exposedWireIndex + 1];
                    FSprite wireGlow = wireGlows[wireGlowIndex];
                    wire1.SetPosition(ballPos);
                    wire2.SetPosition(ballPos);
                    wireGlow.SetPosition(ballPos);
                    wire1.color = fruitColor;
                    wire2.color = fruitColor;
                    wireGlow.color = fruitColor;

                    Vector2 segmentRot = Custom.DirVec(segPos1, segPos2);
                    Vector2 wireRot1 = Custom.rotateVectorDeg(segmentRot, wireDegs[exposedWireIndex]);
                    Vector2 wireRot2 = Custom.rotateVectorDeg(segmentRot, wireDegs[exposedWireIndex + 1]);

                    wire1.rotation = Custom.VecToDeg(wireRot1);
                    wire2.rotation = Custom.VecToDeg(wireRot2);

                    exposedWireIndex += 2;
                    wireGlowIndex++;
                }
            }
        }
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Background");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }

        foreach (FSprite glow in wireGlows)
        {
            rCam.ReturnFContainer("Water").AddChild(glow);
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
}

public class LightningFruitData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    new public int minRegen;
    new public int maxRegen;
    public int charge;

    public LightningFruitData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        minRegen = 2;
        maxRegen = 3;
        charge = 1;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}", new object[]
        {
            panelPos.x,
            panelPos.y,
            minRegen,
            maxRegen,
            charge
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        charge = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
    }

    public override string ToString()
    {
        string text = this.BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class LightningFruitRepresentation : ConsumableRepresentation
{
    public LightningFruitData data;
    new public FruitControlPanel controlPanel;

    public class FruitControlPanel : ConsumableControlPanel, IDevUISignals
    {
        public LightningFruitData data;
        public Button chargeButton;
        public FruitControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) :
            base(owner, IDstring, parentNode, pos, name)
        {
            data = (parentNode as LightningFruitRepresentation).data;
            size = new Vector2(250f, 65f);

            subNodes.Add(chargeButton = new(owner, "Charge_Button", this, new Vector2(5f, 45f), 240f, "CHARGE: " + data.charge));
        }
        public override void Refresh()
        {
            data = (parentNode as LightningFruitRepresentation).data;
            chargeButton.Text = "CHARGE: " + data.charge;
            base.Refresh();
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "Charge_Button")
            {
                data.charge = -data.charge;
            }
        }
    }

    public LightningFruitRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as LightningFruitData;
        controlPanel = new(owner, "Lightning_Fruit_Panel", this, data.panelPos, "Consumable: Lightning Fruit");

        (pObj.data as PlacedObject.ConsumableObjectData).minRegen = data.minRegen;
        (pObj.data as PlacedObject.ConsumableObjectData).maxRegen = data.maxRegen;

        subNodes[0].ClearSprites();
        subNodes.RemoveAt(0);

        subNodes.Add(controlPanel);
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        owner.placedObjectsContainer.AddChild(fSprites[1]);
        owner.placedObjectsContainer.AddChild(fSprites[2]);
    }

    public override void Refresh()
    {
        MoveSprite(1, absPos);
        fSprites[1].scaleY = controlPanel.pos.magnitude;
        fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as LightningFruitData).panelPos = controlPanel.pos;

        data.minRegen = (pObj.data as PlacedObject.ConsumableObjectData).minRegen;
        data.maxRegen = (pObj.data as PlacedObject.ConsumableObjectData).maxRegen;

        data.charge = (pObj.data as LightningFruitData).charge;

        base.Refresh();
    }
}

public class DecoVineData : PlacedObject.ResizableObjectData
{
    new public Vector2 handlePos;
    public Vector2 panelPos;
    public int charge;
    public int seed;
    public float elasticity;

    public DecoVineData(PlacedObject owner) : base(owner)
    {
        handlePos = new Vector2(0f, 100f);
        panelPos = new Vector2(0f, 100f);
        charge = 1;
        seed = Random.Range(1000, 10000);
        elasticity = 1;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}", new object[]
        {
            handlePos.x,
            handlePos.y,
            panelPos.x,
            panelPos.y,
            charge,
            seed,
            elasticity
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");

        int failIndex = 0;
        try
        {
            handlePos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            handlePos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            panelPos.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            panelPos.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            charge = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            seed = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            elasticity = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture); failIndex++;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 7);
        }
        catch (Exception e)
        {
            Debug.Log("---PLACEDOBJECT \'DECORATIVEVINE\' EXPERIENCED AN EXCEPTION WHILE TRYING TO LOAD DATA. FAILURE OCCURED AT DATA INDEX: " + failIndex);

            if (failIndex < 1)
            {
                handlePos.x = 0f;
            }
            if (failIndex < 2)
            {
                handlePos.y = 100f;
            }
            if (failIndex < 3)
            {
                panelPos.x = 0f;
            }
            if (failIndex < 4)
            {
                panelPos.y = 100f;
            }
            if (failIndex < 5)
            {
                charge = 1;
            }
            if (failIndex < 6)
            {
                seed = Random.Range(1000, 10000);
            }
            if (failIndex < 7)
            {
                elasticity = 1f;
            }
            if (failIndex < 8)
            {
                unrecognizedAttributes = [];
            }

            Debug.LogException(e);
        }
    }

    public override string ToString()
    {
        string text = this.BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class DecoVineRepresentation : ResizeableObjectRepresentation
{
    public DecoVineData data;
    public Handle handle;
    public DecoVineControlPanel controlPanel;
    public DecoVineRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name, false)
    {
        data = pobj.data as DecoVineData;
        controlPanel = new(owner, "Lightning_Fruit_Panel", this, data.panelPos, new Vector2(250f, 45f), "Decorative Lightning Vine");

        handle = subNodes[0] as Handle;
        handle.pos = data.handlePos;

        subNodes.Add(controlPanel);
    }

    public class DecoVineControlPanel : Panel, IDevUISignals
    {
        public DecoVineData data;
        public Button chargeButton;
        public Button newSeedButton;
        public ElasticitySlider elasticitySlider;
        public DecoVineControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string name) :
            base(owner, IDstring, parentNode, pos, size, name)
        {
            data = (parentNode as DecoVineRepresentation).data;

            subNodes.Add(chargeButton = new(owner, "Charge_Button", this, new Vector2(5f, 5f), 117.5f, "CHARGE: " + data.charge));
            subNodes.Add(newSeedButton = new(owner, "New_Seed_Button", this, new Vector2(127.5f, 5f), 117.5f, "SEED: " + data.seed));
            subNodes.Add(elasticitySlider = new(owner, "Elasticity_Slider", this, new Vector2(5f, 25f), "ELASTICITY"));
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "Charge_Button")
            {
                data.charge = -data.charge;
            }
            if (sender.IDstring == "New_Seed_Button")
            {
                data.seed = Random.Range(1000, 10000);
            }
        }
        public override void Refresh()
        {
            data = (parentNode as DecoVineRepresentation).data;
            chargeButton.Text = "CHARGE: " + data.charge;
            newSeedButton.Text = "SEED: " + data.seed;

            base.Refresh();
        }
        public class ElasticitySlider : Slider
        {
            public DecoVineData data;
            public ElasticitySlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
                data = (parentNode as DecoVineControlPanel).data;
            }
            public override void Refresh()
            {
                base.Refresh();
                string newText = data.elasticity.ToString();

                if (newText.Length > 3)
                {
                    newText = newText.Substring(0, 3);
                }

                NumberText = newText;
                RefreshNubPos(data.elasticity / 10);
            }
            public override void NubDragged(float nubPos)
            {
                data.elasticity = nubPos * 10;
                parentNode.parentNode.Refresh();
                Refresh();
            }
        }
    }

    public override void Refresh()
    {
        base.Refresh();

        (pObj.data as DecoVineData).panelPos = controlPanel.pos;
        (pObj.data as DecoVineData).handlePos = handle.pos;
        (pObj.data as DecoVineData).elasticity = controlPanel.elasticitySlider.data.elasticity;

        data.charge = (pObj.data as DecoVineData).charge;
        data.seed = (pObj.data as DecoVineData).seed;
        data.elasticity = (pObj.data as DecoVineData).elasticity;
    }
}
