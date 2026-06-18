using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using DevInterface;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Items;

public class AshPepper : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public int bites = 1;
    public Vector2 rotation, lastRotation;
    public Vector2 attachPos, attachRot;
    public float startRot;

    public AshPepperBush.Branch attachedToBranch;
    public int branchIndex;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return abstractPhysicalObject as AbstractConsumable;
        }
    }

    public AshPepper(AbstractPhysicalObject abstractPhysicalObject, AshPepperBush bush, int branchIndex) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, default, 4f, 0.2f);
        bodyChunkConnections = [];

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;
        gravity = 0.9f;
        collisionLayer = 1;

        rotation = Vector2.up;

        bodyColor = Custom.HSL2RGB(0f, 0.8f, 0.3f);
        shineColor = Custom.HSL2RGB(0f, 0.8f, 0.4f);

        if (bush != null)
        {
            if (branchIndex == 0)
            { attachedToBranch = bush.largeBranch; }
            else
            { attachedToBranch = bush.smallBranches[branchIndex - 1]; }

            this.branchIndex = branchIndex;
        }

        startRot = Random.Range(20f, -20f);
    }

    public override void Update(bool eu)
    {
        float section = 0;

        try
        {
            base.Update(eu);
            lastRotation = rotation;

            section = 1;

            if (attachedToBranch != null)
            {
                section = 2;

                UpdateAttachPosAndRot();

                if (grabbedBy.Count > 0 || Custom.Dist(firstChunk.pos, attachPos) > 15)
                {
                    PickOffBranch();
                }
                else
                {
                    section = 2.1f;

                    if (grabbedBy.Count == 0)
                    {
                        section = 2.2f;

                        firstChunk.pos = attachPos;
                        firstChunk.vel *= 0f;

                        rotation = attachRot;
                    }

                    CollideWithObjects = false;
                }
            }
            else
            {
                section = 3;

                if (grabbedBy.Count == 0)
                {
                    section = 3.1f;

                    if (firstChunk.vel.magnitude > 1f)
                    {
                        rotation = Custom.rotateVectorDeg(rotation, 20f * Mathf.Sign(firstChunk.vel.x));
                    }
                }
                else
                {
                    rotation = Vector2.up;
                }
            }

            if (camera != null)
            {
                UpdateLighting(camera);
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "ASHPEPPER_UPDATE", section);
        }
    }

    public void UpdateAttachPosAndRot()
    {
        attachRot = Vec(Vector2.up, attachedToBranch.pepperAngle * attachedToBranch.Rotation.x > 0 ? -1f : 1f);
        attachPos = attachedToBranch.EndPos2 - attachRot * 5f;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;

        if (attachedToBranch != null)
        {
            UpdateAttachPosAndRot();
            bodyChunks[0].HardSetPosition(attachPos);
        }
        else
        {
            bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);

        if (attachedToBranch != null)
        {
            PickOffBranch();
        }
    }

    public void PickOffBranch()
    {
        attachedToBranch.bush.bushVel += Custom.DirVec(attachedToBranch.EndPos2, firstChunk.pos) * 5;

        room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, firstChunk, false, 0.3f, 3f + Random.value / 10f);

        attachedToBranch = null;
        AbstrConsumable.Consume();
        CollideWithObjects = true;
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

    public void ThrowByPlayer()
    {
    }
    #endregion

    #region Graphics Stuff
    public Color bodyColor, shineColor, blackColor;
    public FSprite pepper;

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        pepper = new("AshPepper1")
        {
            scale = 0.8f,
            color = bodyColor
        };
        sprites.Add(pepper);

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
            Vector2 rot = Vector2.Lerp(lastRotation, rotation, timeStacker);

            pepper.SetPosition(chunkPos);
            pepper.rotation = Custom.VecToDeg(rot);
            pepper.element = Futile.atlasManager.GetElementWithName("AshPepper1");

            if (attachedToBranch != null)
            {
                AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
            }
            else
            {
                AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
            }

            if (camera != rCam)
            {
                UpdateLighting(rCam);
            }

            Color lightColor = Color.Lerp(lastLightColor, this.lightColor, timeStacker);
            float lightExposure = Mathf.Lerp(lastLightExposure, this.lightExposure, timeStacker);
            float colorExposure = Mathf.Lerp(lastColorExposure, this.colorExposure, timeStacker);

            Color tintedBodyColor = Color.Lerp(bodyColor, lightColor, colorExposure);
            Color finalBodyColor = Color.Lerp(blackColor, tintedBodyColor, lightExposure);

            pepper.color = blink > 0 ? Color.white : finalBodyColor;
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
        (lightColor, lightExposure, colorExposure) = TrueLightColorAndExposure(camera.room, camera, firstChunk.pos - camera.pos, 0f);
    }
    #endregion
}

public class AshPepperBush : CosmeticSprite
{
    public PlacedObject pObj;
    public Vector2 bushDir;

    public Vector2 bushVel;

    public Branch largeBranch;
    public Branch[] smallBranches;

    public Color blackColor;

    public Vector2 stemPos, lastStemPos;
    public Vector2 rootPos;

    public AshPepper[] attachedPeppers;
    public int peppers;
    public int maxPeppers;

    public AshPepperBush(PlacedObject pObj, Room room, int maxPeppers, int seed)
    {
        this.pObj = pObj;
        this.maxPeppers = maxPeppers;

        Random.State state = Random.state;
        Random.InitState(seed);

        bushDir = Custom.rotateVectorDeg(Vector2.up, Random.Range(-10f, 10f));

        largeBranch = new(this, 4, 5f, 3f, bushDir, 0, false);
        List<Branch> smallBranches = [];
        for (int i = 1; i < largeBranch.Length - 1; i++)
        {
            Vector2 segmentDir = Custom.DirVec(largeBranch.segmentPositions[i, 0], largeBranch.segmentPositions[i + 1, 0]);
            for (int j = 0; j < 2; j++)
            {
                Vector2 branchDir = Custom.rotateVectorDeg(segmentDir, j == 0 ? Random.Range(62, 58) : -Random.Range(62, 58));
                smallBranches.Add(new Branch(this, 3, 3f, 1f, branchDir, i, true));
            }
        }
        this.smallBranches = [.. smallBranches];

        attachedPeppers = new AshPepper[this.smallBranches.Length + 1];

        IntVector2 startSearchPos = room.GetTilePosition(pObj.pos);
        for (int i = startSearchPos.y; i > 0; i--)
        {
            if (room.GetTile(startSearchPos.x, i).Solid)
            {
                rootPos = new(pObj.pos.x, room.MiddleOfTile(startSearchPos.x, i).y + Random.Range(2f, 8f));
                break;
            }
        }

        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        bool pulling = false;
        foreach (AshPepper pepper in attachedPeppers)
        {
            if (pepper != null && pepper.grabbedBy.Count > 0 && pepper.attachedToBranch != null)
            {
                bushVel = Custom.DirVec(pepper.attachedToBranch.EndPos2, pepper.firstChunk.pos) * Custom.Dist(pepper.attachedToBranch.EndPos2, pepper.firstChunk.pos);
                pulling = true;
                break;
            }
        }

        Vector2 stemPos1 = rootPos;
        Vector2 stemPos2 = rootPos + largeBranch.dir * 40 + bushVel;

        largeBranch.Update(stemPos1, stemPos2);

        for (int i = 0; i < smallBranches.Length; i++)
        {
            Vector2 branchPos1 = largeBranch.segmentPositions[smallBranches[i].attachIndex, 0];
            Vector2 branchPos2 = branchPos1 + smallBranches[i].dir * Mathf.Lerp(10f, 5f, ((float)smallBranches[i].attachIndex - 1) / (largeBranch.Length - 2));

            smallBranches[i].Update(branchPos1, branchPos2);
        }

        if (!pulling)
        {
            if (bushVel.magnitude > 0.05f)
            {
                bushVel *= 0.5f;
            }
            else
            {
                bushVel *= 0;
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        largeBranch.Initialize(sprites);

        foreach (Branch branch in smallBranches)
        {
            branch.Initialize(sprites);
        }

        sLeaser.sprites = [..sprites];

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        largeBranch.Draw(timeStacker, camPos);

        foreach (Branch branch in smallBranches)
        {
            branch.Draw(timeStacker, camPos);
        }
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer mainContainer)
    {
        mainContainer ??= rCam.ReturnFContainer("Background");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();

            mainContainer.AddChild(fsprite);
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        blackColor = palette.blackColor;
    }

    public class Branch
    {
        public AshPepperBush bush;

        public TriangleMesh branch;
        public Leaf[] leaves;

        public int numOfPositions;
        public float branchWidth;
        public float[] randomSegmentDeviation;
        public float pepperAngle;

        public Vector2[,] segmentPositions;
        public Vector2 EndPos1 { get { return segmentPositions[0, 0]; } }
        public Vector2 EndPos2 { get { return segmentPositions[segmentPositions.GetLength(0) - 1, 0]; } }

        public Vector2 dir;
        public int attachIndex;
        public bool sideBranch;

        public int Length
        {
            get
            {
                return segmentPositions.GetLength(0);
            }
        }

        public Vector2 Rotation
        {
            get
            {
                return Custom.DirVec(EndPos1, EndPos2);
            }
        }

        public Branch(AshPepperBush bush, int length, float width, float maxDev, Vector2 dir, int attachIndex, bool sideBranch)
        {
            this.bush = bush;
            this.dir = dir;
            this.attachIndex = attachIndex;
            this.sideBranch = sideBranch;

            numOfPositions = Mathf.Max(length, 2);
            branchWidth = width;

            List<Leaf> leaves = [];
            leaves.Add(new (90));
            leaves.Add(new (-90));
            this.leaves = [..leaves];

            segmentPositions = new Vector2[numOfPositions, 2];
            randomSegmentDeviation = new float[numOfPositions];

            float devDir = Random.value > 0.5 ? -1 : 1;
            for (int i = 0; i < randomSegmentDeviation.Length; i++)
            {
                if (i == 0 || i == randomSegmentDeviation.Length - 1)
                {
                    randomSegmentDeviation[i] = 0;
                }
                else
                {
                    randomSegmentDeviation[i] = Random.Range(maxDev, 1f) * devDir;
                    devDir = -devDir;
                }
            }

            pepperAngle = Random.Range(3f, 5f);
        }

        public void Update(Vector2 newEndPos1, Vector2 newEndPos2)
        {
            //Debug.Log(segmentPositions.GetLength(0));

            Vector2 perpDir = Custom.PerpendicularVector(Rotation);

            for (int i = 0; i < segmentPositions.GetLength(0); i++)
            {
                segmentPositions[i, 1] = segmentPositions[i, 0];

                if (i == 0)
                {
                    segmentPositions[i, 0] = newEndPos1;
                }
                else if (i < segmentPositions.GetLength(0) - 1)
                {
                    float floatPos = (float)i / (segmentPositions.GetLength(0) - 1);

                    Vector2 segmentPos = Vector2.Lerp(EndPos1, EndPos2, floatPos) + perpDir * randomSegmentDeviation[i];
                    segmentPositions[i, 0] = segmentPos;
                }
                else
                {
                    segmentPositions[i, 0] = newEndPos2;
                }
            }
        }

        public void Initialize(List<FSprite> newSprites)
        {
            branch = TriangleMesh.MakeLongMesh(numOfPositions * 2, true, false);
            newSprites.Add(branch);

            foreach (Leaf leaf in leaves)
            {
                leaf.Initialize(newSprites);
            }
        }

        public void Draw(float timeStacker, Vector2 camPos)
        {
            List<Vector2> branchPositions = [];
            List<Vector2> meshPositions = [];

            for (int i = 0; i < segmentPositions.GetLength(0); i++)
            {
                Vector2 pos = Vector2.Lerp(segmentPositions[i, 1], segmentPositions[i, 0], timeStacker) - camPos;
                branchPositions.Add(pos);

                //Create_Square(bush.room, pos, 2f, 2f, Vec(45), "Yellow", 0);
            }

            for (int i = 0; i < branchPositions.Count; i++)
            {
                Vector2 pos = branchPositions[i];

                Vector2 segmentDir = Vector2.up;
                if ((sideBranch || i > 0) && i < branchPositions.Count - 1)
                {
                    Vector2 nextPos = branchPositions[i + 1];

                    segmentDir = Custom.DirVec(pos, nextPos);
                }
                else if (i == branchPositions.Count - 1)
                {
                    Vector2 lastPos = branchPositions[i - 1];

                    segmentDir = Custom.DirVec(lastPos, pos);
                }
                Vector2 perpDir = Custom.PerpendicularVector(segmentDir);

                float segmentWidth = Mathf.Pow(Mathf.Lerp(1f, 0.5f, (float)i / (branchPositions.Count - 1)), 2) * branchWidth;

                Vector2 pos1 = pos + perpDir * (segmentWidth / 2);
                Vector2 pos2 = pos - perpDir * (segmentWidth / 2);

                meshPositions.Add(pos1);
                meshPositions.Add(pos2);
            }

            int verticeIndex = 0;
            for (int i = 0; i < branch.vertices.Length; i++)
            {
                Vector2 verticePosition = meshPositions[verticeIndex];
                branch.MoveVertice(i, verticePosition);

                if (verticeIndex < meshPositions.Count - 1)
                {
                    verticeIndex++;
                }
            }
            branch.color = bush.blackColor;

            Vector2 leafPos = branchPositions[branchPositions.Count - 1];
            Vector2 leafDir = Custom.DirVec(branchPositions[0], branchPositions[branchPositions.Count - 1]);
            leaves[0].Draw(bush.blackColor, leafPos, leafDir);
            leaves[1].Draw(bush.blackColor, leafPos, leafDir);
        }
    }
    public class Leaf
    {
        public FSprite leafSprite;
        public int leafSpriteNumber;
        public float floatRotation;

        public Leaf(float startRotation)
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

    public void PopulateBranch(Room room, PlacedObject pObj, int pObjIndex, AshPepperBushData data, int branch)
    {
        if (peppers >= maxPeppers)
        { return; }

        if (attachedPeppers[branch] == null)
        {
            AbstractConsumable abstractConsumable = new(room.world, Enums.AbstractObjectType.AshPepper, null, room.GetWorldCoordinate(pObj.pos),
            room.game.GetNewID(), room.abstractRoom.index, pObjIndex, data)
            { isConsumed = false };

            AshPepper newAshPepper = new(abstractConsumable, this, branch);
            abstractConsumable.realizedObject = newAshPepper;
            attachedPeppers[branch] = newAshPepper;
            room.abstractRoom.AddEntity(abstractConsumable);

            peppers++;
        }
    }
}

public class AshPepperBushData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    new public int minRegen;
    new public int maxRegen;

    public int minPeppers;
    public int maxPeppers;

    public AshPepperBushData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        minRegen = 2;
        maxRegen = 3;
        minPeppers = 2;
        maxPeppers = 3;
    }
    new protected string BaseSaveString()
    {
        Debug.Log("ASHPEPPERBUSH DATA SAVED");

        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", new object[]
        {
            panelPos.x,
            panelPos.y,
            minRegen,
            maxRegen,
            minPeppers,
            maxPeppers,
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        minPeppers = int.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxPeppers = int.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class AshPepperBushRepresentation : ConsumableRepresentation
{
    public AshPepperBushData Data
    {
        get
        {
            return pObj.data as AshPepperBushData;
        }
    }


    new public AshPepperBushControlPanel controlPanel;
    public FSprite line;

    public AshPepperBushRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        controlPanel = new(owner, "AshPepperBush_Panel", this, Data.panelPos, "Consumable: Ash Pepper");

        (pObj.data as PlacedObject.ConsumableObjectData).minRegen = Data.minRegen;
        (pObj.data as PlacedObject.ConsumableObjectData).maxRegen = Data.maxRegen;

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
        (pObj.data as AshPepperBushData).panelPos = controlPanel.pos;

        Data.minRegen = (pObj.data as PlacedObject.ConsumableObjectData).minRegen;
        Data.maxRegen = (pObj.data as PlacedObject.ConsumableObjectData).maxRegen;
    }

    public class AshPepperBushControlPanel : ConsumableControlPanel, IDevUISignals
    {
        public AshPepperBushData Data
        {
            get
            {
                return (parentNode as AshPepperBushRepresentation).Data;
            }
        }

        public AshPepperBushControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) :
            base(owner, IDstring, parentNode, pos, name)
        {
            size = new Vector2(250f, 85f);

            subNodes.Add(new AshPepperBushSlider(owner, "Min_Peppers", this, new Vector2(5f, 45f), "Min Peppers"));
            subNodes.Add(new AshPepperBushSlider(owner, "Max_Peppers", this, new Vector2(5f, 65f), "Max Peppers"));
        }

        public override void Refresh()
        {
            Data.panelPos = pos;

            base.Refresh();
        }
        public void Signal(DevUISignalType type, DevUINode sender, string message)
        {
        }

        public class AshPepperBushSlider : Slider
        {
            public AshPepperBushData Data
            {
                get
                {
                    return (parentNode as AshPepperBushControlPanel).Data;
                }
            }

            public AshPepperBushSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
            : base(owner, IDstring, parentNode, pos, title, false, 110f)
            {
            }

            public override void Refresh()
            {
                base.Refresh();

                float newNubPos = 0;

                if (IDstring == "Max_Peppers")
                {
                    newNubPos = (Data.maxPeppers - 1) / 4f;

                    NumberText = Data.maxPeppers.ToString();
                }
                else if (IDstring == "Min_Peppers")
                {
                    newNubPos = (Data.minPeppers - 1) / 4f;

                    NumberText = Data.minPeppers.ToString();
                }

                RefreshNubPos(newNubPos);
            }

            public override void NubDragged(float nubPos)
            {
                if (IDstring == "Max_Peppers")
                {
                    Data.maxPeppers = Math.Max((int)(nubPos * 4) + 1, Data.minPeppers);
                }
                else if (IDstring == "Min_Peppers")
                {
                    Data.minPeppers = Math.Min((int)(nubPos * 4) + 1, Data.maxPeppers);
                }

                parentNode.parentNode.Refresh();
                Refresh();
            }
        }
    }
}

public class AbstractMultiConsumable : AbstractConsumable
{
    public PhysicalObject[] realizedObjects;

    public AbstractMultiConsumable(
        World world,
        AbstractPhysicalObject.AbstractObjectType type,
        PhysicalObject[] realizedObjects,
        WorldCoordinate pos, EntityID ID,
        int originRoom, int placedObjectIndex,
        PlacedObject.ConsumableObjectData consumableData) : base(world, type, null, pos, ID, originRoom, placedObjectIndex, consumableData)
    {
        this.realizedObjects = realizedObjects;
    }
}
