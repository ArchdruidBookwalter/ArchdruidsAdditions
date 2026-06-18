using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Policy;
using System.Text.RegularExpressions;
using DevInterface;

using static ArchdruidsAdditions.Objects.PhysicalObjects.Items.LightningFruitVine.LightningFruitBall;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Items;

public class LightningFruit : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public int bites = 3;

    public Vector2 rotation, lastRotation, decoSparkRotation;

    public int power = 1000;
    public int decoSparkPower = 0, decoSparkCooldown = 200;
    public int charge;
    public int lastLightFlash;
    public int lightFlash;
    public bool flashLight;

    public bool shockBiter = false;
    public int shockBiterTimer = 0;

    public bool attachedToVine;

    public Creature lastHolder;

    public LightningFruitVine vine;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return abstractPhysicalObject as AbstractConsumable;
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
        decoSparkRotation = Vector2.up;

        this.charge = charge;

        Random.State state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);

        Color baseFruitColor = charge == 1 ? Custom.HSL2RGB(0.65f, 1f, 0.5f) : Custom.HSL2RGB(0.75f, 1f, 0.5f);
        float baseColorSpread = 0.025f;

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

        stem = new(0.25f);

        Random.state = state;
    }

    #region Physics Stuff

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        lastLightFlash = lightFlash;

        if (power > 0)
        {
            if (!attachedToVine && Random.value < 0.1f)
            {
                power--;
            }

            if (decoSparkPower == 0 && decoSparkCooldown == 0)
            {
                DecorativeSparkle(20);
            }
            else if (decoSparkPower > 0)
            {
                decoSparkRotation = Custom.rotateVectorDeg(decoSparkRotation, Random.Range(-25f, 25f));
                decoSparkPower--;
            }
            else if (decoSparkCooldown > 0)
            {
                decoSparkCooldown--;
            }

            if (Submersion > 0f)
            {
                ReleasePower();
                room.AddObject(new UnderwaterShock(room, this, firstChunk.pos, 14, 200f, 0.1f, lastHolder ?? null, bodyColor));
            }
        }

        if (decoSparkPower > 0)
        {
            decoSparkPower--;
        }

        if (lightFlash > 0)
        {
            lightFlash--;
        }

        if (grabbedBy.Count == 0 && !attachedToVine && firstChunk.vel.magnitude > 1)
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

                    ShockCreature(grasp.grabber);
                }
            }
        }
        else if (attachedToVine && vine != null)
        {
            rotation = Custom.DirVec(firstChunk.pos, vine.segPosList[1]);
        }

        if (camera != null)
        {
            UpdateLighting(camera);
        }

        //Methods.Methods.Create_Text(room, firstChunk.segPos, power.ToString(), "Red", 0);
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

    public void ShockCreature(Creature creature)
    {
        room.PlaySound(SoundID.Centipede_Shock, firstChunk.pos, 1f, 1f);

        DecorativeSparkle(50);

        for (int i = 0; i < Random.Range(1, 4); i++)
        { room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * Random.Range(4f, 15f), bodyColor, null, 8, 14)); }

        creature.Stun(Random.Range(200, 400));
        room.AddObject(new CreatureSpasmer(creature, false, creature.stun));
        creature.LoseAllGrasps();
    }

    public void ReleasePower()
    {
        if (power > 0)
        {
            power = 0;

            DecorativeSparkle(50);

            room.PlaySound(SoundID.Centipede_Shock, firstChunk.pos, 1f, 1f);

            for (int i = 0; i < Random.Range(1, 4); i++)
            { room.AddObject(new Spark(firstChunk.pos, Custom.RNV() * Random.Range(4f, 15f), bodyColor, null, 8, 14)); }
        }
    }

    public void DecorativeSparkle(int time)
    {
        decoSparkPower = time;
        decoSparkCooldown = Random.Range(50, 200);
        lightFlash = 5;
    }

    public void DetachFromVine()
    {
        vine.fruitAttached = false;
        vine = null;

        room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, firstChunk, false, 0.3f, 3f + Random.value / 10f);

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

    #endregion

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

    public FSprite body;

    public LightningFruitVine.LightningFruitStem stem;

    public FSprite backgroundGlow;
    public FSprite foregroundGlow;

    public FSprite decoSpark;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        body = new("LightningFruit1")
        {
            scale = 0.5f
        };
        body.SetPosition(new Vector2(-100, 100));
        body.color = bodyColor;
        sprites.Add(body);

        stem.Initialize(sprites);

        decoSpark = new("Futile_White")
        {
            scale = 1.5f
        };
        decoSpark.SetPosition(new Vector2(-100, 100));
        decoSpark.shader = rCam.room.game.rainWorld.Shaders["LightningBolt"];
        decoSpark.alpha = 1f;
        sprites.Add(decoSpark);

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

        UpdateLighting(rCam);
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

            body.SetPosition(chunkPos);
            body.element = Futile.atlasManager.GetElementWithName("LightningFruit" + (4 - bites).ToString());
            body.rotation = Custom.VecToDeg(rotVec);

            stem.Draw(blackColor, chunkPos + rotVec * 8f, -rotVec);

            float lightFlash = Mathf.Lerp(lastLightFlash, this.lightFlash, timeStacker);

            backgroundGlow.SetPosition(chunkPos);
            backgroundGlow.alpha = (power / 1000f) * 0.5f;
            backgroundGlow.scale = 5f + lightFlash;
            foregroundGlow.SetPosition(chunkPos);
            foregroundGlow.alpha = (power / 1000f) * 0.5f;
            foregroundGlow.scale = 2f + lightFlash / 2;

            if (decoSparkPower > 0)
            {
                decoSpark.SetPosition(chunkPos);
                decoSpark.rotation = Custom.VecToDeg(decoSparkRotation);
                decoSpark.color = Custom.HSL2RGB(1f, 1f, decoSparkPower > 5 ? 0.5f : ((float)decoSparkPower / 10));
            }

            if (camera != rCam)
            {
                UpdateLighting(rCam);
            }

            Color lightColor = Color.Lerp(lastLightColor, this.lightColor, timeStacker);
            float lightExposure = Mathf.Lerp(lastLightExposure, this.lightExposure, timeStacker);
            float colorExposure = Mathf.Lerp(lastColorExposure, this.colorExposure, timeStacker);

            Color baseColor = Color.Lerp(this.bodyColor, lightColor, Mathf.Clamp(Mathf.Min(lightExposure, colorExposure), 0, 0.3f));

            Color bodyColor = Color.Lerp(blackColor, baseColor, lightExposure);
            body.color = blink > 0 ? Color.white : bodyColor;
        }
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer mainContainer)
    {
        mainContainer ??= rCam.ReturnFContainer("Items");
        FContainer lightContainer = rCam.ReturnFContainer("Water");

        mainContainer.AddChild(body);
        mainContainer.AddChild(stem.ballSprite);
        mainContainer.AddChild(stem.leaf1.leafSprite);
        mainContainer.AddChild(stem.leaf2.leafSprite);

        lightContainer.AddChild(backgroundGlow);
        lightContainer.AddChild(foregroundGlow);
        lightContainer.AddChild(decoSpark);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
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
        (lightColor, lightExposure, colorExposure) = TrueLightColorAndExposure(camera.room, camera, firstChunk.pos - camera.pos, power / 1000f);
    }

    #endregion
}

public class LightningFruitVine : UpdatableAndDeletable, IDrawable
{
    public Color blackColor;
    public Color fruitColor;
    public LightningFruit fruit;
    public Vector2 fruitStemPos;
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

    public LightningFruitBall[] balls;
    public LightningFruitStem stem1;
    public LightningFruitStem stem2;

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

        BaseInitializeVine();

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

        Color baseFruitColor = charge == 1 ? Custom.HSL2RGB(0.65f, 1f, 0.5f) : Custom.HSL2RGB(0.75f, 1f, 0.5f);
        float baseColorSpread = 0.025f;

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

        BaseInitializeVine();

        Random.state = state;
    }
    public void BaseInitializeVine()
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

        List<LightningFruitBall> newballs = [];
        for (int i = decorative ? 0 : 2; i < numOfSegments - 1; i++)
        {
            for (int j = 0; j < Random.Range(0, 5); j++)
            {
                LightningFruitBall newBall = new
                (
                    this, 
                    i, 
                    Random.value, 
                    0.25f, 
                    Random.value < 0.1f,
                    Random.value < 0.5 ? Random.Range(1, 3) : 0
                );
                newballs.Add(newBall);
            }
        }
        balls = [.. newballs];

        stem1 = new(0.25f);
        stem2 = new(0.25f);
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

        foreach (LightningFruitBall ball in balls)
        {
            ball.Initialize(sprites, room.game.rainWorld);
        }

        stem1.Initialize(sprites);

        stem2.Initialize(sprites);

        sLeaser.sprites = [.. sprites];

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
            List<Vector2> segPositions = [];

            Vector2 lastSegmentPos = Vector2.Lerp(segLastPosList[0], segPosList[0], timeStacker) - camPos;
            for (int i = 0; i < segPosList.Length; i++)
            {
                Vector2 segmentPos;

                if (i == StartIndex && fruitAttached)
                {
                    Vector2 fruitPos = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker) - camPos;
                    Vector2 nextSegmentPos = Vector2.Lerp(segLastPosList[1], segPosList[1], timeStacker) - camPos;

                    segmentPos = fruitPos + Custom.DirVec(fruitPos, nextSegmentPos) * 8f;
                    lastSegmentPos = segmentPos;
                }
                else
                {
                    segmentPos = Vector2.Lerp(segLastPosList[i], segPosList[i], timeStacker) - camPos;
                }

                Vector2 segmentRot = Custom.DirVec(segmentPos, lastSegmentPos);
                Vector2 perpRot = Custom.PerpendicularVector(segmentRot);

                segPositions.Add(segmentPos);

                float vineRad = 1f;

                vineMesh.MoveVertice(i * 4, lastSegmentPos - perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 1, lastSegmentPos + perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 2, segmentPos - perpRot * vineRad);
                vineMesh.MoveVertice(i * 4 + 3, segmentPos + perpRot * vineRad);
                vineMesh.color = blackColor;

                lastSegmentPos = segmentPos;
            }



            foreach (LightningFruitBall ball in balls)
            {
                ball.Draw
                (
                    camPos,
                    Vector2.Lerp(segLastPosList[ball.segIndex], segPosList[ball.segIndex], timeStacker) - camPos,
                    Vector2.Lerp(segLastPosList[ball.segIndex + 1], segPosList[ball.segIndex + 1], timeStacker) - camPos,
                    blackColor
                );
            }

            //Vector2 stem1Rot = Custom.DirVec(segPositions[1], segPositions[0]);
            //stem1.Draw(blackColor, segPositions[0], stem1Rot);

            //Vector2 stem2Rot = Custom.DirVec(segPositions[segPositions.Count - 2], segPositions[segPositions.Count - 1]);
            //stem2.Draw(blackColor, segPositions[segPositions.Count - 1], stem2Rot);
        }
    }
    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer mainContainer)
    {
        mainContainer ??= rCam.ReturnFContainer("Background");
        FContainer lightContainer = rCam.ReturnFContainer("Water");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();

            if (fsprite.shader == rCam.room.game.rainWorld.Shaders["FlatLight"])
            {
                lightContainer.AddChild(fsprite);
            }
            else
            {
                mainContainer.AddChild(fsprite);
            }
        }
    }
    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    public class LightningFruitBall
    {
        public LightningFruitVine vine;

        public CircularSprite ballSprite;
        public FSprite glow;
        public LightningFruitLeaf[] leaves;
        public int segIndex;
        public float segPos;
        public float size;

        public Vector2 pos;
        public Vector2 rot;

        public Color color;
        public bool colored;

        public LightningFruitBall(LightningFruitVine vine, int segIndex, float pos, float size, bool colored, int leafCount)
        {
            this.vine = vine;
            this.segIndex = segIndex;
            this.segPos = pos;
            this.size = size;
            this.colored = colored;

            if (colored)
            { color = vine.fruitColor; }

            List<LightningFruitLeaf> newLeaves = [];
            for (int i = 0; i < leafCount; i++)
            {
                LightningFruitLeaf newLeaf = new(Random.value < 0.5 ? Random.Range(0, 90) : Random.Range(180, 270));
                newLeaves.Add(newLeaf);
            }
            leaves = [.. newLeaves];
        }
        public void Initialize(List<FSprite> newSprites, RainWorld rainWorld)
        {
            ballSprite = new("Futile_White")
            {
                scale = size,
            };

            newSprites.Add(ballSprite);

            foreach (LightningFruitLeaf leaf in leaves)
            {
                leaf.Initialize(newSprites);
            }

            if (colored)
            {
                glow = new("Futile_White")
                {
                    scale = size + 1,
                    shader = rainWorld.Shaders["FlatLight"]
                };

                newSprites.Add(glow);
            }
        }
        public void Draw(Vector2 camPos, Vector2 segPos1, Vector2 segPos2, Color blackColor)
        {
            pos = Vector2.Lerp(segPos1, segPos2, segPos);
            rot = Custom.PerpendicularVector(Custom.DirVec(segPos1, segPos2));

            ballSprite.SetPosition(pos);

            if (colored)
            {
                ballSprite.color = color;

                if (vine.room.GetTile(pos + camPos).Solid)
                {
                    glow.alpha = 0f;
                }
                else
                {
                    glow.SetPosition(pos);
                    glow.color = color;
                    glow.alpha = 0.5f;
                }
            }
            else
            {
                ballSprite.color = blackColor;
            }

            foreach (LightningFruitLeaf leaf in leaves)
            {
                leaf.Draw(blackColor, pos, rot);
                leaf.leafSprite.MoveBehindOtherNode(ballSprite);
            }
        }
        public class LightningFruitLeaf
        {
            public FSprite leafSprite;
            public int leafSpriteNumber;
            public float floatRotation;

            public LightningFruitLeaf(float startRotation)
            {
                floatRotation = startRotation;  

                leafSpriteNumber = Random.Range(0, 5);
            }

            public void Initialize(List<FSprite> newSprites)
            {
                leafSprite = new FSprite("Leaf" + leafSpriteNumber.ToString(), false)
                {
                    scale = 0.6f,
                    anchorY = 0.9f
                };

                newSprites.Add(leafSprite);
            }
            public void Draw(Color blackColor, Vector2 pos, Vector2 rot)
            {
                leafSprite.SetPosition(pos);
                leafSprite.rotation = Custom.VecToDeg(rot) + floatRotation;
                leafSprite.color = blackColor;
            }
        }
    }
    public class LightningFruitStem
    {
        public CircularSprite ballSprite;
        public LightningFruitLeaf leaf1;
        public LightningFruitLeaf leaf2;
        public float size;

        public LightningFruitStem(float size)
        {
            this.size = size;

            leaf1 = new(135);
            leaf2 = new(-135);
        }

        public void Initialize(List<FSprite> newSprites)
        {
            ballSprite = new("Futile_White")
            {
                scale = size,
            };

            newSprites.Add(ballSprite);

            leaf1.Initialize(newSprites);
            leaf2.Initialize(newSprites);
        }

        public void Draw(Color blackColor, Vector2 pos, Vector2 rot)
        {
            ballSprite.SetPosition(pos);
            ballSprite.color = blackColor;

            leaf1.Draw(blackColor, pos, rot);
            leaf2.Draw(blackColor, pos, rot);
        }
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
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class LightningFruitRepresentation : ConsumableRepresentation
{
    public LightningFruitData data;
    new public FruitControlPanel controlPanel;
    public FSprite line;

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

        line = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(line);
        owner.placedObjectsContainer.AddChild(line);
    }

    public override void Refresh()
    {
        base.Refresh();

        MoveSprite(fSprites.IndexOf(line), absPos);
        line.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        line.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as LightningFruitData).panelPos = controlPanel.pos;

        data.minRegen = (pObj.data as PlacedObject.ConsumableObjectData).minRegen;
        data.maxRegen = (pObj.data as PlacedObject.ConsumableObjectData).maxRegen;

        data.charge = (pObj.data as LightningFruitData).charge;
    }
}

public class DecoVineData : PlacedObject.ResizableObjectData
{
    new public Vector2 handlePos;
    public Vector2 panelPos;
    public int charge;
    public float elasticity;

    public DecoVineData(PlacedObject owner) : base(owner)
    {
        handlePos = new Vector2(0f, 100f);
        panelPos = new Vector2(0f, 100f);
        charge = 1;
        elasticity = 1;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", new object[]
        {
            handlePos.x,
            handlePos.y,
            panelPos.x,
            panelPos.y,
            charge,
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
            elasticity = Mathf.Min(float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture), 10f); failIndex++;
            unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
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
                elasticity = 1f;
            }
            if (failIndex < 7)
            {
                unrecognizedAttributes = [];
            }

            Debug.LogException(e);
        }
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class DecoVineRepresentation : ResizeableObjectRepresentation
{
    public DecoVineData data;
    public Handle handle;
    public DecoVineControlPanel controlPanel;
    public FSprite line;
    public DecoVineRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name, false)
    {
        data = pobj.data as DecoVineData;
        controlPanel = new(owner, "Lightning_Fruit_Panel", this, data.panelPos, new Vector2(250f, 45f), "Decorative Lightning Vine");

        handle = subNodes[0] as Handle;
        handle.pos = data.handlePos;

        subNodes.Add(controlPanel);

        line = new FSprite("pixel") { anchorY = 0f };
        fSprites.Add(line);
        owner.placedObjectsContainer.AddChild(line);
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

            subNodes.Add(chargeButton = new(owner, "Charge_Button", this, new Vector2(5f, 5f), 240, "CHARGE: " + data.charge));
            subNodes.Add(elasticitySlider = new(owner, "Elasticity_Slider", this, new Vector2(5f, 25f), "ELASTICITY"));
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
            if (sender.IDstring == "Charge_Button")
            {
                data.charge = -data.charge;
            }
        }
        public override void Refresh()
        {
            data = (parentNode as DecoVineRepresentation).data;
            chargeButton.Text = "CHARGE: " + data.charge;

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
        data.elasticity = (pObj.data as DecoVineData).elasticity;

        MoveSprite(fSprites.IndexOf(line), absPos);
        line.scaleY = controlPanel.collapsed ? 0f : controlPanel.pos.magnitude;
        line.rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
    }
}
