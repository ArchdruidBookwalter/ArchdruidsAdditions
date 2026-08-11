using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

public class MimicCrab : Creature, IPlayerEdible
{
    public MimicCrabAI AI;

    public int stepCooldown;

    public bool Swimming
    {
        get
        { return Submersion > 0.3f; }
    }

    public int attachCounter;
    public PhysicalObject AttachedToObject
    {
        get
        {
            if (ShellStick != null && ShellStick.Shell.realizedObject != null)
            {
                return ShellStick.Shell.realizedObject;
            }

            return null;
        }
    }
    public AbstractCrabShellStick ShellStick
    {
        get
        {
            if (shellStick != null && abstractCreature.stuckObjects.Contains(shellStick))
            { return shellStick; }

            foreach (AbstractPhysicalObject.AbstractObjectStick objectStick in abstractCreature.stuckObjects)
            {
                if (objectStick is AbstractCrabShellStick shellStick)
                {
                    this.shellStick = shellStick;
                    return shellStick;
                }
            }

            return null;
        }
    }
    public AbstractCrabShellStick shellStick;
    public int hideInShellCounter, maxHideCounter = 200;
    public int shellStuckCounter;
    public bool AttachedToFruitVine
    {
        get
        {
            if (AttachedToObject != null && AttachedToObject.abstractPhysicalObject is AbstractConsumable absCon && !absCon.isConsumed)
            { return true; }
            return false;
        }
    }

    public Vector2 bodyRot, lastBodyRot;
    public float roll, lastRoll;
    public float touchingGround;

    public bool Hidden
    { get { return hideInShellCounter == maxHideCounter; } }

    public bool Footing
    {
        get
        {
            if (grabbedBy.Count > 0 || Hidden || Stunned)
            { return false; }

            if (AttachedToObject != null)
            {
                if (AttachedToObject.grabbedBy.Count > 0)
                { return false; }
                if (AttachedToObject is Weapon weapon && weapon.mode == Weapon.Mode.Thrown)
                { return false; }
            }

            if (footing >= 5)
            { return true; }

            return false;
        }
    }
    public int footing;

    public int bites;

    new public MimicCrabGraphics graphicsModule
    {
        get
        {
            return base.graphicsModule as MimicCrabGraphics;
        }
    }

    public MimicCrab(AbstractCreature creature, World world) : base(creature, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new(this, 0, default, 5f, 0.5f);
        bodyChunkConnections = [];

        gravity = 0.9f;
        airFriction = 0.999f;
        bounce = 0.2f;
        waterFriction = 0.96f;
        buoyancy = 0.9f;
        collisionLayer = 1;
        surfaceFriction = 0.4f;

        bites = 6;
    }

    public override void Update(bool eu)
    {
        float section = 0;

        try
        {
            //LogMethodStart("MIMICCRAB_UPDATE");

            base.Update(eu);

            section = 1;

            lastBodyRot = bodyRot;
            lastRoll = roll;

            if (Consious)
            {
                if (room != null)
                {
                    section = 1.1f;

                    AI.Update();

                    section = 1.2f;

                    if (grasps[0] != null && grasps[0].grabbed is PhysicalObject obj && obj is not Creature && obj.room != null)
                    {
                        bool releaseGrasp = false;
                        if (obj.grabbedBy.Count > 1)
                        { releaseGrasp = true; }
                        else
                        {
                            foreach (AbstractPhysicalObject.AbstractObjectStick stick in obj.abstractPhysicalObject.stuckObjects)
                            {
                                if (stick is AbstractCrabShellStick || obj.grabbedBy.Count > 1)
                                { releaseGrasp = true; }
                            }
                        }

                        if (releaseGrasp)
                        { ReleaseGrasp(0); }
                        else
                        {
                            Vector2 shellRot = Custom.rotateVectorDeg(bodyRot, 20f);

                            float attachDist = firstChunk.rad + obj.firstChunk.rad;
                            Vector2 shellAttachPos = firstChunk.pos + shellRot * attachDist;
                            Vector2 bodyAttachPos = obj.firstChunk.pos - shellRot * attachDist;

                            Vector2 dirVec2 = Custom.DirVec(firstChunk.pos, bodyAttachPos);
                            float dist2 = Custom.Dist(firstChunk.pos, bodyAttachPos);
                            Vector2 dirVec3 = Custom.DirVec(obj.firstChunk.pos, shellAttachPos);
                            float dist3 = Custom.Dist(obj.firstChunk.pos, shellAttachPos);

                            firstChunk.vel += dirVec2 * dist2 * 0.5f;
                            firstChunk.pos += dirVec2 * dist2 * 0.5f;
                            obj.firstChunk.vel += dirVec3 * dist3 * 0.5f;
                            obj.firstChunk.pos += dirVec3 * dist3 * 0.5f;

                            if (obj is CrabShell shell)
                            {
                                shell.ChangeRotation(Custom.PerpendicularVector(shellRot), 0.9f);
                                shell.roll = roll;
                            }

                            if (AI.ValueOfObject(obj.abstractPhysicalObject.type) > 0f)
                            {
                                attachCounter++;

                                if (attachCounter > 80)
                                {
                                    new AbstractCrabShellStick(abstractCreature, obj.abstractPhysicalObject);

                                    if (obj is CrabShell shell2)
                                    { shell2.crab = this; }

                                    room.PlaySound(SoundID.Vulture_Mask_Pick_Up, firstChunk, false, 1f, 0.9f);
                                }
                            }
                        }
                    }
                    else
                    {
                        attachCounter = Mathf.Max(0, attachCounter--);
                    }

                    section = 1.3f;

                    if (room.aimap.TileAccessibleToCreature(firstChunk.pos, Template) || room.aimap.getAItile(firstChunk.pos).acc == AItile.Accessibility.Solid)
                    {
                        if (footing < 20)
                        { footing++; }
                    }
                    else
                    { footing = 0; }
                }
            }
            else
            {
                section = 1.4f;

                footing = 0;
            }

            section = 2;

            if (room != null)
            {
                roll = Mathf.Lerp(roll, Mathf.Lerp(0.4f, 0.6f, touchingGround) + (graphicsModule.wobbleAmount * 0.1f), 0.9f);

                if (AttachedToObject != null && AttachedToObject.room != null && graphicsModule != null)
                {
                    section = 1.21f;

                    ReleaseGrasp(0);

                    Vector2 shellRot = Custom.DegToVec(Custom.VecToDeg(bodyRot) + 20f + graphicsModule.wobbleAmount * 10f);

                    if (AttachedToObject is CrabShell shell)
                    {
                        if (!Hidden)
                        {
                            shell.ChangeRotation(Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, shell.firstChunk.pos)), 0.9f);
                            shell.roll = roll;
                        }

                        if (shell.mode == Weapon.Mode.Thrown)
                        { Stun(10); }
                    }

                    float attachDist = 10f * (Hidden ? 0 : Mathf.InverseLerp(0f, 0.5f, roll));
                    Vector2 shellAttachPos = firstChunk.pos + shellRot * attachDist;
                    Vector2 bodyAttachPos = AttachedToObject.firstChunk.pos - shellRot * attachDist;

                    Vector2 dirVec2 = Custom.DirVec(firstChunk.pos, bodyAttachPos);
                    float dist2 = Custom.Dist(firstChunk.pos, bodyAttachPos);
                    Vector2 dirVec3 = Custom.DirVec(AttachedToObject.firstChunk.pos, shellAttachPos);
                    float dist3 = Custom.Dist(AttachedToObject.firstChunk.pos, shellAttachPos);

                    firstChunk.vel += dirVec2 * dist2 * 0.5f;
                    firstChunk.pos += dirVec2 * dist2 * 0.5f;

                    section = 1.22f;

                    if (AttachedToObject.grabbedBy.Count == 0)
                    {
                        AttachedToObject.firstChunk.vel += dirVec3 * dist3 * 0.5f;
                        AttachedToObject.firstChunk.pos += dirVec3 * dist3 * 0.5f;
                    }
                }

                if (room.aimap.getTerrainProximity(firstChunk.pos) < 2)
                { touchingGround = Mathf.Lerp(touchingGround, 1f, 0.1f); }
                else
                { touchingGround = Mathf.Lerp(touchingGround, 0f, 0.1f); }

                if (Hidden)
                { bodyRot = Vector3.Slerp(bodyRot, Vector2.up, 0.1f); }
                else if (touchingGround > 0.5f)
                {
                    IntVector2 startPos = room.GetTilePosition(firstChunk.pos);
                    IntVector2 bestDir = new(0, 1);
                    int bestDirTerrainProximity = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        IntVector2 testPos = startPos + Custom.eightDirectionsDiagonalsLast[i];
                        int terrainProximity = room.aimap.getTerrainProximity(testPos);

                        if (terrainProximity > bestDirTerrainProximity)
                        {
                            bestDir = Custom.eightDirectionsDiagonalsLast[i];
                            bestDirTerrainProximity = terrainProximity;
                        }
                    }

                    bodyRot = Vector3.Slerp(bodyRot, bestDir.ToVector2().normalized, 0.1f).normalized;
                }
                else if (firstChunk.vel.magnitude > 1f)
                { bodyRot = Vector3.Slerp(bodyRot, firstChunk.vel.normalized, 0.1f); }
            }

            if (grabbedBy.Count > 0 || AttachedToObject != null)
            {
                CollideWithObjects = false;
            }
            else
            {
                CollideWithObjects = true;
            }

            //LogMethodEnd();

            //Create_LineBetweenTwoPoints(room, firstChunk.creaturePos, firstChunk.creaturePos + moveDir * 40f, 1f, "Red", 0);
        }
        catch (Exception e)
        {
            Log_Exception(e, "MIMICCRAB_UPDATE", section);
        }
    }
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);

        //Create_Square(room, firstChunk.creaturePos, 10f, 10f, Vector2.up, "Green", 100);
    }
    public override void InitiateGraphicsModule()
    {
        base.graphicsModule ??= new MimicCrabGraphics(this);
    }

    #region Edible Stuff
    public int BitesLeft { get { return bites; } }
    public int FoodPoints { get { return 1; } }
    public bool Edible { get { return AttachedToObject == null; } }
    public bool AutomaticPickUp { get { return false; } }

    public void BitByPlayer(Grasp grasp, bool eu)
    {
        bites--;

        if (!dead)
        { Die(); }

        room.PlaySound(SoundID.Slugcat_Bite_Swarmer, firstChunk.pos);

        if (grasp.grabber is Player player)
        { firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(player.bodyChunks[0].pos, player.bodyChunks[1].pos, 0.1f)); }
        else
        { firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos); }

        if (bites < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }
    public void ThrowByPlayer()
    { }
    #endregion
}

public class MimicCrabGraphics : GraphicsModule
{
    public MimicCrab crab;

    public Leg[] legs;

    public FSprite bodySprite;
    public TriangleMesh shellMesh;

    public Color blackColor;
    public Color bodyColor;
    public bool albino;

    //public Vector2 bodyRot, lastBodyRot;
    //public float touchingGround;

    public float roll, lastRoll;
    public float wobbleAmount, lastWobbleAmount;
    public int wobbleTimer;

    public int grabbedObjectSLeaserIndex;

    public MimicCrabGraphics(PhysicalObject ow) : base(ow, true)
    {
        crab = ow as MimicCrab;

        List<BodyPart> parts = [];

        legs = new Leg[6];
        for (int i = 0; i < legs.Length; i++)
        {
            legs[i] = new Leg(this, i);
            parts.Add(legs[i].limb);
        }

        bodyParts = [..parts];

        albino = false;
    }

    public override void Update()
    {
        base.Update();

        for (int i = 0; i < legs.Length; i++)
        {
            if (i >= crab.bites)
            {
                legs[i].hide = true;
            }

            legs[i].Update(GetLegAttachPos(crab.firstChunk.pos, i), GetLegGoalPos(i));
        }

        if (camera != null)
        {
            UpdateLighting(camera);

            grabbedObjectSLeaserIndex = -1;

            if (crab.AttachedToObject != null)
            {
                foreach (RoomCamera.SpriteLeaser sLeaser in camera.spriteLeasers)
                {
                    if (sLeaser.drawableObject == crab.AttachedToObject)
                    {
                        grabbedObjectSLeaserIndex = camera.spriteLeasers.IndexOf(sLeaser);
                        break;
                    }
                }

                grabbedObjectSLeaserIndex = -1;
            }
            else if (crab.grasps[0] != null)
            {
                foreach (RoomCamera.SpriteLeaser sLeaser in camera.spriteLeasers)
                {
                    if (sLeaser.drawableObject == crab.grasps[0].grabbed)
                    {
                        grabbedObjectSLeaserIndex = camera.spriteLeasers.IndexOf(sLeaser);
                        break;
                    }
                }
            }
        }

        lastWobbleAmount = wobbleAmount;

        if (crab.Footing && crab.AI.moving && !crab.Hidden)
        {
            float wobbleLength = 10f;

            if (wobbleTimer > wobbleLength * 2f)
            { wobbleTimer = 0; }
            else
            { wobbleTimer++; }

            wobbleAmount = (Mathf.PingPong(wobbleTimer, wobbleLength) - wobbleLength) / wobbleLength;
        }

        //Create_LineBetweenTwoPoints(otherCrab.room, otherCrab.firstChunk.creaturePos, otherCrab.firstChunk.creaturePos + otherCrab.bodyRot * 40f, 1f, "Red", 0);
        //Create_Square(otherCrab.room, otherCrab.firstChunk.creaturePos, otherCrab.firstChunk.rad * 2f, otherCrab.firstChunk.rad * 2f, Vec(45), "Red", 0);
        //Create_Text(otherCrab.room, otherCrab.firstChunk.creaturePos, wobbleAmount, "Red", 0);
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        bodySprite = new FSprite("Futile_White", true)
        {
            shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"],
            alpha = 0.5f,
            scale = crab.firstChunk.rad / 8f
        };
        sprites.Add(bodySprite);

        shellMesh = TriangleMesh.MakeLongMesh(1, false, false);
        sprites.Add(shellMesh);

        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].InitSprites(sprites);
        }

        sLeaser.sprites = [.. sprites];

        sLeaser.containers = new FContainer[2];
        sLeaser.containers[0] = new FContainer();
        sLeaser.containers[1] = new FContainer();

        AddToContainer(sLeaser, rCam, null);

        UpdateLighting(rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float section = 0;
        try
        {
            if (crab.slatedForDeletetion || crab.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
            else
            {
                Vector2 bodyPos;
                Vector2 bodyRot = Vector2.Lerp(crab.lastBodyRot, crab.bodyRot, timeStacker);

                if (crab.AttachedToObject == null && crab.grasps[0] == null)
                {
                    ReleaseAllInternallyContainedSprites();
                }

                if (crab.AttachedToObject != null && !crab.AttachedToObject.slatedForDeletetion)
                {
                    Vector2 shellPos = Vector2.Lerp(crab.AttachedToObject.firstChunk.lastPos, crab.AttachedToObject.firstChunk.pos, timeStacker) - camPos;
                    float roll = Mathf.Lerp(crab.lastRoll, crab.roll, timeStacker);

                    if (crab.AttachedToObject is CrabShell shell)
                    {
                        AddObjectToInternalContainer(shell, 1);
                        shell.shellShape.UpdateSpriteLayers(shell.shellShape.z);
                    }

                    if (crab.Hidden)
                    { bodyPos = shellPos; }
                    else
                    { bodyPos = shellPos - bodyRot * 10f * Mathf.InverseLerp(0f, 0.5f, roll); }
                }
                else
                {
                    bodyPos = Vector2.Lerp(crab.firstChunk.lastPos, crab.firstChunk.pos, timeStacker) - camPos;

                    shellMesh.alpha = 0f;
                }

                bodySprite.SetPosition(bodyPos);
                for (int i = 0; i < legs.Length; i++)
                {
                    Vector2 legAttachPos = GetLegAttachPos(bodyPos, i);

                    legs[i].DrawSprites(legAttachPos, timeStacker, camPos, blackColor);
                }

                if (camera == null)
                { UpdateLighting(rCam); }

                Color lightColor = Color.Lerp(lastLightColor, this.lightColor, timeStacker);
                float lightExposure = Mathf.Lerp(lastLightExposure, this.lightExposure, timeStacker);
                float colorExposure = Mathf.Lerp(lastColorExposure, this.colorExposure, timeStacker);

                Color tintedBodyColor = Color.Lerp(bodyColor, lightColor, colorExposure);
                Color finalBodyColor = Color.Lerp(blackColor, tintedBodyColor, lightExposure);

                bodySprite.color = blackColor;

                if (crab.grasps[0] != null && crab.grasps[0].grabbed is IDrawable iDrawable2)
                {
                    AddObjectToInternalContainer(iDrawable2, 1);

                    if (crab.grasps[0].grabbed is CrabShell shell2)
                    { shell2.shellShape.UpdateSpriteLayers(shell2.shellShape.z); }
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

        newContainer.AddChild(sLeaser.containers[0]);
        newContainer.AddChild(sLeaser.containers[1]);

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            sLeaser.containers[0].AddChild(fsprite);
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);

        blackColor = palette.blackColor;

        if (albino == false)
        { bodyColor = blackColor; }

        UpdateLighting(rCam);
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
        (lightColor, lightExposure, colorExposure) = TrueLightColorAndExposure(camera.room, camera, crab.firstChunk.pos - camera.pos, 0f);
    }

    public Vector2 GetLegAttachPos(Vector2 bodyPos, int index)
    {
        if (index < 3)
        {
            Vector2 leftSide = bodyPos + Custom.DegToVec(Custom.VecToDeg(crab.bodyRot) + 45 * (index + 1)) * crab.firstChunk.rad;
            Vector2 rightSide = bodyPos + Custom.DegToVec(Custom.VecToDeg(crab.bodyRot) - 45 * (index + 1)) * crab.firstChunk.rad;

            return Vector2.Lerp(leftSide, rightSide, crab.touchingGround);
        }
        else
        {
            Vector2 leftSide = bodyPos + Custom.DegToVec(Custom.VecToDeg(crab.bodyRot) + 45 * (index - 2)) * crab.firstChunk.rad;
            Vector2 rightSide = bodyPos + Custom.DegToVec(Custom.VecToDeg(crab.bodyRot) - 45 * (index - 2)) * crab.firstChunk.rad;

            return Vector2.Lerp(rightSide, leftSide, crab.touchingGround);
        }
    }
    public Vector2 GetLegGoalPos(int index)
    {
        if (crab.grasps[0] != null)
        {
            return crab.grasps[0].grabbed.firstChunk.pos;
        }
        else
        {
            Vector2 attachPos = GetLegAttachPos(crab.firstChunk.pos, index);

            Vector2 legDir = Custom.DirVec(crab.firstChunk.pos, attachPos);

            return attachPos + legDir * legs[index].length * 0.5f;
        }
    }

    public class Leg
    {
        public MimicCrab crab;
        public MimicCrabGraphics graphics;

        public Limb limb;
        public FSprite sprite1, sprite2;

        public float length;
        public float flip, lastFlip;

        public bool stepSound;

        public bool forceGrab;

        public SoundID stepSoundID;

        public bool hide;
        public bool pullIntoShell;

        public Leg(MimicCrabGraphics graphics, int index)
        {
            crab = graphics.crab;
            this.graphics = graphics;

            limb = new Limb(graphics, graphics.crab.firstChunk, index, 1f, 0.5f, 0.9f, 7f, 0.8f)
            {
                mode = Limb.Mode.Dangle
            };

            length = 25f;

            if (ModManager.Watcher)
            { stepSoundID = Watcher.WatcherEnums.WatcherSoundID.Barnacle_Step; }
            else
            { stepSoundID = SoundID.Lizard_BlueWhite_Foot_Release; }
        }

        public void Update(Vector2 attachPos, Vector2 goalPos)
        {
            if (crab.hideInShellCounter == crab.maxHideCounter)
            {
                pullIntoShell = true;
            }

            if (hide)
            {
                limb.mode = Limb.Mode.Dangle;
            }
            else
            {
                limb.Update();
                limb.ConnectToPoint(attachPos, length, false, 0f, crab.firstChunk.vel, 0.1f, 0f);

                lastFlip = flip;

                if (crab.Consious)
                {
                    if (crab.Hidden)
                    {
                        limb.mode = Limb.Mode.Dangle;
                        limb.vel += Custom.DirVec(limb.pos, attachPos) * Custom.Dist(limb.pos, attachPos) * 0.5f;
                    }
                    else if (!crab.AI.moving)
                    {
                        if (!limb.reachedSnapPosition || !Custom.DistLess(limb.pos, goalPos, 2f))
                        { limb.FindGrip(crab.room, attachPos, goalPos, length * 1.5f, goalPos, -2, -2, true); }
                    }
                    else
                    {
                        if (!limb.reachedSnapPosition || !Custom.DistLess(limb.pos, limb.absoluteHuntPos, length * 0.5f))
                        { limb.FindGrip(crab.room, attachPos, goalPos, length * 1.5f, goalPos, -2, -2, true); stepSound = false; }
                    }

                    if (limb.reachedSnapPosition && !stepSound)
                    {
                        crab.room.PlaySound(stepSoundID, crab.firstChunk);
                        stepSound = true;
                    }
                }
                else
                {
                    limb.mode = Limb.Mode.Dangle;
                    limb.vel.y -= crab.gravity;
                }

                flip = Mathf.Lerp(flip, Custom.Angle(Custom.DirVec(crab.firstChunk.pos, limb.pos), crab.bodyRot) > 0 ? -1f : 1f, 0.1f) * Mathf.Lerp(0.5f, 1f, crab.touchingGround);
            }

            //Create_Text(otherCrab.room, limb.creaturePos + Custom.DirVec(otherCrab.firstChunk.creaturePos, limb.creaturePos) * 50f, string.Format("{0:N2}", flip), "Green", 0);
            Create_Square(crab.room, attachPos, 2f, 2f, Vec(0), "Green", 0);
            Create_Square(crab.room, limb.absoluteHuntPos, 2f, 2f, Vec(0), "Blue", 0);
            Create_Square(crab.room, goalPos, 2f, 2f, Vec(0), "Green", 0);
        }

        public void InitSprites(List<FSprite> sprites)
        {
            sprite1 = new FSprite("CentipedeLegA", true)
            { anchorY = 0.1f };
            sprite2 = new FSprite("CentipedeLegB", true)
            { anchorY = 0.1f };

            sprites.Add(sprite1);
            sprites.Add(sprite2);
        }

        public void DrawSprites(Vector2 attachPos, float timeStacker, Vector2 camPos, Color blackColor)
        {
            if (hide)
            {
                sprite1.alpha = 0f;
                sprite2.alpha = 0f;
            }
            else
            {
                Vector2 limbPos = Vector2.Lerp(limb.lastPos, limb.pos, timeStacker) - camPos;

                float flip = Mathf.Lerp(lastFlip, this.flip, timeStacker);

                float segmentLength = crab.Hidden ? 0f : length / 2f;

                Vector2 elbowPos = Custom.InverseKinematic(attachPos, limbPos, segmentLength, segmentLength, flip);

                sprite1.SetPosition(attachPos);
                sprite1.rotation = Custom.AimFromOneVectorToAnother(attachPos, elbowPos);
                sprite1.scaleY = Custom.Dist(attachPos, elbowPos) / 27f;
                sprite1.scaleX = -1f * Mathf.Sign(flip);
                sprite1.color = blackColor;

                sprite2.SetPosition(elbowPos);
                sprite2.rotation = Custom.AimFromOneVectorToAnother(elbowPos, limbPos);
                sprite2.scaleY = Custom.Dist(elbowPos, limbPos) / 25f;
                sprite2.scaleX = -1f * Mathf.Sign(flip);
                sprite2.color = blackColor;
            }
        }
    }
}

public class MimicCrabAI : ArtificialIntelligence, IUseARelationshipTracker, IUseItemTracker
{
    public MimicCrab crab;
    public Behavior behavior;
    public bool movingToDestination;
    public ItemTracker.ItemRepresentation shellCanidate;
    public Tracker.CreatureRepresentation leaderCanidate;
    public int invalidLeaderTime;
    public bool fleeing, moving;
    public int fleeCounter;
    public WorldCoordinate lastDangerCoord;
    public WorldCoordinate hideCoord;

    public MimicCrabAI(AbstractCreature creature, World world) : base(creature, world)
    {
        crab = creature.realizedCreature as MimicCrab;
        crab.AI = this;

        AddModule(new StandardPather(this, world, creature));
        AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10, false));
        AddModule(new ItemTracker(this, 10, 10, -1, -1, true));
        AddModule(new RelationshipTracker(this, tracker));
        AddModule(new ThreatTracker(this, 10));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new UtilityComparer(this));

        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);

        pathFinder.stepsPerFrame = 20;

        //pathFinder.visualize = true;
        //pathFinder.Reset(creature.Room.realizedRoom);
    }


    public override void Update()
    {
        float section = 0;

        try
        {
            //LogMethodStart("MIMICCRABAI");

            base.Update();

            section = 1;

            UpdateBehavior();

            section = 2;

            UpdateMovement();

            //LogMethodEnd();
        }
        catch (Exception e)
        {
            Log_Exception(e, "MIMICCRABAI_UPDATE", section);
        }
    }
    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        if (threatTracker != null && threatTracker.Utility() > 0f && threatTracker.mostThreateningCreature != null && threatTracker.mostThreateningCreature.VisualContact)
        {
            float tileThreat = threatTracker.ThreatOfTile(coord.destinationCoord, false);

            Create_Text(crab.room, crab.room.MiddleOfTile(coord.destinationCoord), tileThreat.ToString("0"), "Red", 0);

            cost += new PathCost(tileThreat, PathCost.Legality.Allowed);
        }

        return cost;
    }
    public void UpdateBehavior()
    {
        float section = 0;

        try
        {
            //LogMethodStart("MIMICCRABAI_UPDATEBEHAVIOR");

            Room room = crab.room;

            AIModule highestModule = utilityComparer.HighestUtilityModule();

            if (highestModule is ThreatTracker threatTracker && threatTracker.mostThreateningCreature != null)
            {
                lastDangerCoord = threatTracker.mostThreateningCreature.BestGuessForPosition();

                Vector2 dangerPos = room.MiddleOfTile(lastDangerCoord);

                if (crab.AttachedToObject == null || Custom.DistLess(crab.firstChunk.pos, dangerPos, 50f))
                {
                    behavior = Behavior.Flee;

                    //pathFinder.RestartPathFinding();
                }
            }

            section = 1;

            if (behavior == Behavior.Flee)
            {
                hideCoord = default;

                //Create_LineBetweenTwoPoints(room, crab.firstChunk.pos, room.MiddleOfTile(lastDangerCoord), 1f, "Red", 0);

                if (highestModule is ThreatTracker threatTracker2 && threatTracker2.mostThreateningCreature != null && threatTracker2.mostThreateningCreature.VisualContact)
                { fleeCounter = 0; }
                else
                { fleeCounter++; }

                if (fleeCounter > 200)
                {
                    behavior = Behavior.Hide;
                }
            }
            else
            {
                fleeCounter = 0;

                if (highestModule is RainTracker rainTracker)
                {
                    behavior = Behavior.ReturnToDen;
                }
                else
                {
                    behavior = Behavior.Hide;
                }
            }

            section = 2;

            if (behavior == Behavior.Hide && crab.AttachedToObject != null && (crab.firstChunk.vel.magnitude < 1f || crab.hideInShellCounter == 200) && (Custom.DistLess(crab.firstChunk.pos, room.MiddleOfTile(pathFinder.destination), 50f) || crab.AttachedToFruitVine))
            {
                if (crab.hideInShellCounter < 200)
                { crab.hideInShellCounter++; }
            }
            else
            {
                crab.hideInShellCounter = 0;
            }

            //Create_Text(room, crab.firstChunk.pos, behavior.value, "Yellow", 0);

            //LogMethodEnd();
        }
        catch (Exception e)
        {
            Log_Exception(e, "MIMICCRABAI_UPDATEBEHAVIOR", section);
        }
    }
    public void UpdateMovement()
    {
        float section = 0;

        try
        {
            //LogMethodStart("MIMICCRABAI_UPDATEMOVEMENT");

            Room room = crab.room;
            Vector2 pos = crab.firstChunk.pos;
            Vector2 destination = room.MiddleOfTile(pathFinder.destination);

            PathFinder.PathingCell cell = pathFinder.PathingCellAtWorldCoordinate(crab.coord);
            if (!cell.reachable || !cell.possibleToGetBackFrom)
            { pathFinder.OutOfElement(); }

            bool goToDestination = false;
            bool goToObject = false;
            bool fleeFromEnemy = false;
            Vector2 moveDir = Vector2.zero;

            section = 1;

            if (behavior == Behavior.Flee && threatTracker.mostThreateningCreature != null && 
                threatTracker.mostThreateningCreature.VisualContact && cell.generation != pathFinder.pathGeneration)
            {
                fleeFromEnemy = true;

                Vector2 enemyDir = Custom.DirVec(pos, room.MiddleOfTile(lastDangerCoord.Tile.x, lastDangerCoord.Tile.y - 2));

                moveDir.x = -Mathf.Sign(enemyDir.x);
                if (crab.IsTileSolid(0, -Math.Sign(enemyDir.x), 0))
                { moveDir.y = -Mathf.Sign(enemyDir.y); }
            }

            section = 2;

            if (crab.grasps[0] == null && crab.AttachedToObject == null)
            {
                section = 2.1f;

                UpdateShellCanidate();

                if (shellCanidate != null && shellCanidate.representedItem.realizedObject != null)
                {
                    WorldCoordinate itemCoord = shellCanidate.BestGuessForPosition();

                    Vector2 objPos = room.MiddleOfTile(itemCoord);
                    if (!Custom.DistLess(destination, objPos, 20))
                    {
                        pathFinder.AssignNewDestination(itemCoord);
                    }

                    if (Custom.DistLess(pos, objPos, 20f))
                    {
                        crab.Grab(shellCanidate.representedItem.realizedObject, 0, 0, Creature.Grasp.Shareability.NonExclusive, 1000f, true, false);

                        room.PlaySound(SoundID.Vulture_Mask_Pick_Up, crab.firstChunk);
                    }
                    else
                    {
                        goToObject = true;
                        goToDestination = true;
                    }
                }
            }

            if (!goToObject && crab.grasps[0] == null)
            {
                if (behavior == Behavior.Flee)
                {
                    section = 2.2f;

                    Vector2 dangerPos = room.MiddleOfTile(lastDangerCoord);

                    if (!goToObject)
                    {
                        if (Custom.DistLess(dangerPos, destination, 400))
                        {
                            WorldCoordinate bestHideCoord = crab.coord;
                            float bestScore = float.MinValue;
                            for (int i = 0; i < 20; i++)
                            {
                                IntVector2 randomTile = room.RandomTile();

                                for (int j = 0; j < 20; j++)
                                {
                                    IntVector2 testTile = new(randomTile.x, randomTile.y - j);
                                    WorldCoordinate testCoord = room.GetWorldCoordinate(testTile);
                                    if (pathFinder.CoordinateViable(testCoord))
                                    {
                                        float testScore = -pathFinder.CoordinateCost(testCoord).resistance * 0.5f;

                                        testScore += Custom.Dist(room.MiddleOfTile(testTile), dangerPos);

                                        if (testScore > bestScore)
                                        {
                                            bestHideCoord = testCoord;
                                            bestScore = testScore;
                                        }
                                    }
                                }
                            }

                            if (bestHideCoord != null)
                            {
                                pathFinder.AssignNewDestination(bestHideCoord);
                            }
                        }

                        if (!Custom.DistLess(pos, destination, 100f))
                        {
                            goToDestination = true;
                        }
                    }
                }
                else if (behavior == Behavior.Hide)
                {
                    section = 2.31f;

                    if (!crab.AttachedToFruitVine)
                    {
                        UpdateLeaderCanidate();

                        section = 2.32f;

                        if (!pathFinder.CoordinateViable(hideCoord) || Custom.WorldCoordFloatDist(crab.coord, hideCoord) > 40)
                        {
                            WorldCoordinate bestHideCoord = crab.coord;
                            float bestScore = float.MinValue;
                            for (int i = 0; i < 20; i++)
                            {
                                IntVector2 randomTile = new(crab.coord.x + Random.Range(-20, 21), crab.coord.y + Random.Range(-20, 21));

                                for (int j = 0; j < 20; j++)
                                {
                                    IntVector2 testTile = new(randomTile.x, randomTile.y - j);
                                    WorldCoordinate testCoord = room.GetWorldCoordinate(testTile);
                                    if (room.aimap.getAItile(testCoord).acc == AItile.Accessibility.Floor && room.aimap.getAItile(new IntVector2(testCoord.x, testCoord.y - 1)).acc == AItile.Accessibility.Solid)
                                    {
                                        float testScore = -pathFinder.CoordinateCost(testCoord).resistance;

                                        if (leaderCanidate != null && Custom.DistLess(room.MiddleOfTile(testTile), room.MiddleOfTile(leaderCanidate.BestGuessForPosition()), 100f))
                                        { testScore += 1000f; }

                                        if (testScore > bestScore)
                                        {
                                            bestHideCoord = testCoord;
                                            bestScore = testScore;
                                        }
                                    }
                                }
                            }

                            hideCoord = bestHideCoord;
                        }

                        section = 2.33f;

                        if (pathFinder.CoordinateViable(hideCoord))
                        {
                            Vector2 hidePos = room.MiddleOfTile(hideCoord);

                            if (!Custom.DistLess(destination, hidePos, 20f))
                            {
                                pathFinder.AssignNewDestination(hideCoord);
                            }

                            if (!Custom.DistLess(pos, hidePos, 50) || !crab.IsTileSolid(0, 0, -1))
                            {
                                goToDestination = true;
                            }
                        }
                    }
                }
            }

            section = 3;

            if (goToDestination && !fleeFromEnemy)
            {
                PathFinder.PathingCell cell2 = pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(pos));
                if (cell2.generation == pathFinder.pathGeneration)
                {
                    MovementConnection connect1 = (pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(pos), false);
                    MovementConnection connect2 = (pathFinder as StandardPather).FollowPath(connect1.destinationCoord, false);
                    MovementConnection connect3 = (pathFinder as StandardPather).FollowPath(connect2.destinationCoord, false);

                    if (connect1 != default && connect1.StartTile != connect2.DestTile && VisualContact(room.MiddleOfTile(connect1.DestTile), 10f))
                    {
                        moveDir = Custom.DirVec(pos, room.MiddleOfTile(connect1.DestTile));
                    }

                    //Create_LineBetweenTwoPoints(room, room.MiddleOfTile(connect1.StartTile), room.MiddleOfTile(connect1.DestTile), 1f, "Green", 0);
                    //Create_LineBetweenTwoPoints(room, room.MiddleOfTile(connect2.StartTile), room.MiddleOfTile(connect2.DestTile), 1f, "Yellow", 0);
                    //Create_LineBetweenTwoPoints(room, room.MiddleOfTile(connect3.StartTile), room.MiddleOfTile(connect3.DestTile), 1f, "Red", 0);
                }
            }

            section = 4;

            moving = false;

            if (crab.Footing)
            {
                if (moveDir.magnitude > 0.5f)
                {
                    moving = true;

                    float x = moveDir.x;
                    float y = moveDir.y;

                    float xSpeed = 1f;
                    float ySpeed = 1f;

                    if (behavior == Behavior.Flee)
                    { xSpeed *= 3f; }

                    crab.firstChunk.vel.x += moveDir.x * xSpeed;
                    crab.firstChunk.vel.y += moveDir.y * ySpeed;

                    if (room.aimap.getAItile(crab.coord).acc != AItile.Accessibility.Floor)
                    { crab.firstChunk.vel *= 0.8f; }
                }

                crab.firstChunk.vel *= 0.8f;
                crab.firstChunk.vel.y += crab.gravity * (1f - crab.Submersion);

                if (crab.AttachedToObject != null)
                {
                    PhysicalObject shell = crab.AttachedToObject;

                    shell.firstChunk.vel *= 0.8f;
                    shell.firstChunk.vel.y += shell.gravity * (1f - shell.Submersion);

                    if (crab.AttachedToFruitVine)
                    {
                        crab.firstChunk.vel += moveDir * 20f;
                    }
                }
            }

            section = 5;

            Vector2 newDestination = room.MiddleOfTile(pathFinder.destination);
            //Create_LineAndDot(room, pos, newDestination, "Yellow", 0);

            //LogMethodEnd();
        }
        catch (Exception e)
        {
            Log_Exception(e, "MIMICCRABAI_UPDATEMOVEMENT", section);
        }
    }

    public void UpdateShellCanidate()
    {
        if (shellCanidate != null && (shellCanidate.representedItem.slatedForDeletion || ValueOfObject(shellCanidate) < 0))
        { shellCanidate = null; }

        foreach (ItemTracker.ItemRepresentation item in itemTracker.items)
        {
            float score = ValueOfObject(item);

            if (item.representedItem.realizedObject != null)
            { Create_Text(item.representedItem.realizedObject.room, item.representedItem.realizedObject.firstChunk.pos, score, "Red", 0); }

            if (score >= 0 && (shellCanidate is null || score > ValueOfObject(shellCanidate)))
            { shellCanidate = item; }
        }
    }
    public void UpdateLeaderCanidate()
    {
        float section = 0f;

        try
        {

            if (leaderCanidate != null)
            {
                section = 1;

                Tracker.CreatureRepresentation newLeader = SearchForLeader();
                if (newLeader != null)
                {
                    float leaderScore = LeadershipScore(leaderCanidate);

                    if (newLeader.representedCreature == leaderCanidate.representedCreature || leaderScore < 0)
                    { leaderCanidate = newLeader; }
                }
            }
            else
            {
                section = 2;

                leaderCanidate = SearchForLeader();
            }
        }
        catch (Exception e)
        {
            Log_Exception(e, "MIMICCRABAI_UPDATELEADERCANIDATE", 0);
        }
    }
    public float ValueOfObject(ItemTracker.ItemRepresentation obj)
    {
        if (!pathFinder.CoordinateViable(obj.representedItem.pos))
        {
            return -200;
        }
        if (obj.representedItem.Room != crab.room.abstractRoom)
        {
            return 0f;
        }
        else
        {
            float score = 10f;
            if (obj.representedItem.realizedObject != null && obj.representedItem.realizedObject.grabbedBy.Count > 0)
            {
                return -100;
            }

            foreach (AbstractPhysicalObject.AbstractObjectStick stick in obj.representedItem.stuckObjects)
            {
                if (stick is AbstractCrabShellStick)
                {
                    return -50;
                }
            }

            score += ValueOfObject(obj.representedItem.type);

            return score;
        }
    }
    public float ValueOfObject(AbstractPhysicalObject.AbstractObjectType type)
    {
        if (type == Enums.AbstractObjectType.CrabShell)
        { return 10f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.Rock)
        { return 5f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
        { return 5f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.Lantern)
        { return 4f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
        { return 4f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
        { return 3f; }
        else if (type == AbstractPhysicalObject.AbstractObjectType.WaterNut)
        { return 3f; }
        return 0f;
    }

    public Tracker.CreatureRepresentation SearchForLeader()
    {
        Tracker.CreatureRepresentation bestRep = null;
        float bestScore = float.MinValue;
        foreach (Tracker.CreatureRepresentation rep in tracker.creatures)
        {
            float score = LeadershipScore(rep);
            if (score > 0 && score > bestScore)
            {
                bestScore = score;
                bestRep = rep;
            }

            /*
            if (rep.representedCreature.realizedCreature != null)
            { Create_Text(rep.representedCreature.realizedCreature.room, Vector2.Lerp(crab.firstChunk.creaturePos, rep.representedCreature.realizedCreature.mainBodyChunk.creaturePos, 0.5f), score, "Red", 0); }*/
        }

        if (bestRep != null)
        { return bestRep; }

        return null;
    }
    public float LeadershipScore(Tracker.CreatureRepresentation rep)
    {
        if (rep.representedCreature.realizedCreature != null)
        {
            if (rep.representedCreature.slatedForDeletion || 
                rep.representedCreature.realizedCreature.dead || 
                rep.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Pack ||
                (ModManager.Watcher && rep.representedCreature.realizedCreature is Watcher.Barnacle barnacle && !barnacle.hasShell) ||
                (rep.representedCreature.realizedCreature is MimicCrab otherCrab && otherCrab.AttachedToObject == null))
            { return -10f; }
        }

        float score = Mathf.InverseLerp(20f, 0f, Custom.WorldCoordFloatDist(rep.BestGuessForPosition(), crab.coord)) * 10f;

        if (rep.representedCreature.creatureTemplate.type == Enums.CreatureTemplateType.MimicCrab)
        { score += 100f; }
        else if (ModManager.Watcher && (rep.representedCreature.creatureTemplate.type == Watcher.WatcherEnums.CreatureTemplateType.Barnacle))
        { score += 200f; }

        return score;
    }

    public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return new RelationshipTracker.TrackedCreatureState();
    }
    public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable || relationship.type == CreatureTemplate.Relationship.Type.Afraid)
        {
            return threatTracker;
        }
        return null;
    }
    public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        AbstractCreature creature = dRelation.trackerRep.representedCreature;
        CreatureTemplate.Relationship staticRelationship = StaticRelationship(creature);

        return staticRelationship;
    }

    public bool TrackItem(AbstractPhysicalObject obj)
    {
        return ValueOfObject(obj.type) > 0;
    }

    public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
    {
    }

    public class Behavior : ExtEnum<Behavior>
    {
        public Behavior(string value, bool register = false) : base(value, register)
        {
        }

        public static Behavior Hide = new("Hide", true);
        public static Behavior Flee = new("Flee", true);
        public static Behavior ReturnToDen = new("ReturnToDen", true);
        public static Behavior LookForShell = new("LookForShell", true);
        public static Behavior FollowLeader = new("FollowLeader", true);
    }
}

public class CrabShell : Weapon
{
    public float roll, lastRoll;
    new public Vector2 rotation, lastRotation;

    public Color shellColor, blackColor;

    public bool madeClinkSound;
    public int clinkSoundCooldown;
    public SoundID clinkSound;

    public ShellSprite shellShape;

    public bool charged;

    public MimicCrab crab;

    public AbstractCrabShellStick ShellStick
    {
        get
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick objectStick in abstractPhysicalObject.stuckObjects)
            {
                if (objectStick is AbstractCrabShellStick shellStick)
                {
                    this.shellStick = shellStick;
                    return shellStick;
                }
            }

            return null;
        }
    }
    public AbstractCrabShellStick shellStick;

    public CrabShell(AbstractPhysicalObject absObj, World world) : base(absObj, world)
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

        int shape = Random.Range(0, 3);
        switch (shape)
        {
            case 0: shellShape = new SnailShell(this); break;
            default: shellShape = new BarnacleCone(this); break;
        }

        rotation = Custom.RNV();
        roll = Random.Range(-2f, 2f);

        shellColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.04f, 0.1f, 0.1f), Custom.ClampedRandomVariation(0.5f, 0.2f, 0.3f), Custom.ClampedRandomVariation(0.75f, 0.2f, 1.5f));

        clinkSound = ModManager.Watcher ? Watcher.WatcherEnums.WatcherSoundID.Barnacle_Shell_Clink : SoundID.Snail_Warning_Click;
    }

    public override void Update(bool eu)
    {
        bool stopAbsStickDeactivation = false;
        if (ShellStick != null)
        { MiscData.stopAbsStkDeactivation = true; stopAbsStickDeactivation = true; }

        base.Update(eu);

        if (stopAbsStickDeactivation)
        { MiscData.stopAbsStkDeactivation = false; }

        lastRoll = roll;
        lastRotation = rotation;

        if (grabbedBy.Count == 0 && firstChunk.vel.magnitude > 5f)
        {
            if (roll > 2)
            { roll = -2; lastRoll = roll; }
            else
            { roll += 0.05f; }

            rotation = Custom.rotateVectorDeg(rotation, 1f * firstChunk.vel.magnitude);
        }

        if (firstChunk.ContactPoint.x != 0 || firstChunk.ContactPoint.y != 0)
        {
            if (!madeClinkSound)
            {
                madeClinkSound = true;
                room.PlaySound(clinkSound, firstChunk);
            }
            clinkSoundCooldown = 20;
        }
        else if (madeClinkSound)
        {
            if (clinkSoundCooldown > 0)
            { clinkSoundCooldown--; }
            else
            { madeClinkSound = false; }
        }

        if (camera != null)
        {
            UpdateLighting(camera);
        }

        if (crab != null && (crab.dead || crab.slatedForDeletetion))
        { crab = null; }
    }
    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);

        if (charged || firstChunk.vel.magnitude > 20f)
        {
            Explode();

            if (otherObject is Creature creature)
            {
                room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk.pos);
                creature.Stun(100);
            }
        }
        else if (otherObject.bodyChunks[otherChunk].vel.magnitude > 20f)
        {
            Explode();
        }
    }
    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);

        Explode();
    }
    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
    {
        base.HitByExplosion(hitFac, explosion, hitChunk);

        Explode();
    }
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.chunk == null)
        { return false; }

        if (result.obj is Creature)
        { room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk.pos); }

        Explode();

        return base.HitSomething(result, eu);
    }
    public override void HitWall()
    {
        Explode();
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (charged || speed > 20)
        {
            Explode();
        }
    }
    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);

        charged = true;
        room.PlaySound(SoundID.Slugcat_Throw_Rock, firstChunk);
    }

    public void Explode()
    {
        for (int i = 0; i < Random.Range(5, 6); i++)
        {
            string spriteName = "RootBall1";
            int randomNumber = Random.Range(0, 4);
            switch (randomNumber)
            {
                case 1:
                    spriteName = "KrakenShield0";
                    break;
                case 2:
                    spriteName = "Cicada5body";
                    break;
                case 3:
                    spriteName = "Cicada1body";
                    break;
            }

            CentipedeShell shell = new(firstChunk.pos, Custom.RNV() * Random.Range(5f, 10f), shellColor, Random.Range(0.4f, 0.5f), Random.Range(0.4f, 0.5f), spriteName)
            {
                impactSound = clinkSound
            };

            room.AddObject(shell);
        }

        if (ModManager.Watcher)
        { room.PlaySound(Watcher.WatcherEnums.WatcherSoundID.Barnacle_Shell_Crack, firstChunk.pos, 0.8f, 2f); }
        else
        { room.PlaySound(clinkSound, firstChunk.pos); }
       
        crab?.Stun(100);

        Destroy();
    }
    public void ChangeRotation(Vector2 newRot, float lerp)
    {
        rotation = Vector3.Slerp(rotation, newRot, lerp);
    }
    public void ChangeRoll(float newRoll, float lerp)
    {
        roll = Mathf.Lerp(roll, newRoll, lerp);
    }

    #region Graphics
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        List<FSprite> sprites = [];

        shellShape.InitSprites(sprites);

        sLeaser.sprites = [..sprites];

        AddToContainer(sLeaser, rCam, null);

        UpdateLighting(rCam);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
            Vector2 rot = Vector2.Lerp(lastRotation, rotation, timeStacker).normalized;
            float roll = Mathf.Lerp(lastRoll, this.roll, timeStacker);

            Quaternion quaternion = Quaternion.AngleAxis(Vector2.SignedAngle(Vector2.right, rot), Vector3.forward) * Quaternion.AngleAxis(roll * 90f + 90f, Vector3.right);

            if (camera != rCam)
            {
                UpdateLighting(rCam);
            }

            Color lightColor = Color.Lerp(lastLightColor, this.lightColor, timeStacker);
            float lightExposure = Mathf.Lerp(lastLightExposure, this.lightExposure, timeStacker);
            float colorExposure = Mathf.Lerp(lastColorExposure, this.colorExposure, timeStacker);

            Color tintedBodyColor = Color.Lerp(shellColor, lightColor, colorExposure);
            Color finalBodyColor = blink > 0 ? Color.white : Color.Lerp(blackColor, Color.Lerp(blackColor, tintedBodyColor, lightExposure), 0.5f);

            shellShape.DrawSprites(pos, rot, quaternion, blackColor, finalBodyColor, lightExposure);
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        UpdateLighting(rCam);

        blackColor = palette.blackColor;
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

    public class ShellSprite
    {
        public CrabShell shell;

        public float z;

        public ShellSprite(CrabShell shell)
        {
            this.shell = shell;
        }

        public virtual void InitSprites(List<FSprite> sprites)
        { }

        public virtual void DrawSprites(Vector2 pos, Vector2 rot, Quaternion quaternion, Color blackColor, Color shellColor, float lightExposure)
        { }

        public virtual void UpdateSpriteLayers(float z)
        { this.z = z; }
    }

    public class BarnacleCone : ShellSprite
    {
        public TriangleMesh shellMesh;
        public FSprite[] circles;

        public float coneBaseRad, coneTipRad, coneLength;

        public BarnacleCone(CrabShell shell) : base(shell)
        {
            coneBaseRad = Random.Range(5f, 7f);
            coneTipRad = coneBaseRad * Random.Range(0.6f, 0.7f);
            coneLength = Random.Range(15f, 20f);
        }

        public override void InitSprites(List<FSprite> sprites)
        {
            base.InitSprites(sprites);

            shellMesh = new TriangleMesh("RainWorld_White", [new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(2, 1, 3)], false, true);

            circles = new FSprite[4];
            circles[0] = new FSprite("Circle20", false);
            circles[1] = new FSprite("Circle20", false);
            circles[2] = new FSprite("Circle20", false);
            circles[3] = new FSprite("Circle20", false);

            sprites.Add(shellMesh);
            sprites.Add(circles[0]);
            sprites.Add(circles[1]);
            sprites.Add(circles[2]);
            sprites.Add(circles[3]);
        }

        public override void DrawSprites(Vector2 pos, Vector2 rot, Quaternion quaternion, Color blackColor, Color shellColor, float lightExposure)
        {
            Vector3 basePos = quaternion * new Vector3(0, -coneLength / 2f, 0);
            Vector3 tipPos = quaternion * new Vector3(0, coneLength / 2f, 0);

            Vector2 circle0Pos = pos + (Vector2)basePos;
            Vector2 circle1Pos = pos + (Vector2)tipPos;

            Vector2 coneDir = Custom.DirVec(circle0Pos, circle1Pos);
            float coneAngle = Custom.VecToDeg(coneDir);

            float x = tipPos.normalized.x;
            float y = tipPos.normalized.y;
            float z = tipPos.normalized.z;

            circles[0].SetPosition(circle0Pos);
            circles[0].rotation = coneAngle;
            circles[0].scaleX = coneBaseRad / 10f;
            circles[0].scaleY = coneBaseRad / 10f * Mathf.Abs(z);

            circles[1].SetPosition(circle0Pos);
            circles[1].rotation = coneAngle;
            circles[1].scaleX = coneBaseRad / 20f;
            circles[1].scaleY = coneBaseRad / 20f * Mathf.Abs(z);

            circles[2].SetPosition(circle1Pos);
            circles[2].rotation = coneAngle;
            circles[2].scaleX = coneTipRad / 10f;
            circles[2].scaleY = coneTipRad / 10f * Mathf.Abs(z);

            circles[3].SetPosition(circle1Pos);
            circles[3].rotation = coneAngle;
            circles[3].scaleX = coneTipRad / 20f;
            circles[3].scaleY = coneTipRad / 20f * Mathf.Abs(z);

            for (int i = 0; i < shellMesh.vertices.Length; i += 2)
            {
                float lerp = (float)i / (shellMesh.vertices.Length - 2);

                Vector2 conePos = Vector2.Lerp(circle0Pos, circle1Pos, lerp);
                Vector2 perpConeDir = Custom.PerpendicularVector(coneDir);
                float rad = Mathf.Lerp(coneBaseRad, coneTipRad, Mathf.Pow(lerp, 1.5f));

                shellMesh.MoveVertice(i, conePos + perpConeDir * rad);
                shellMesh.MoveVertice(i + 1, conePos - perpConeDir * rad);
            }

            UpdateSpriteLayers(z);

            circles[0].color = shellColor;
            circles[1].color = blackColor;
            circles[2].color = shellColor;
            circles[3].color = blackColor;
            shellMesh.color = shellColor;
        }

        public override void UpdateSpriteLayers(float z)
        {
            base.UpdateSpriteLayers(z);

            if (z > 0)
            {
                circles[0].MoveBehindOtherNode(circles[2]);
                circles[1].MoveBehindOtherNode(circles[0]);

                circles[3].MoveInFrontOfOtherNode(circles[2]);
            }
            else
            {
                circles[2].MoveBehindOtherNode(circles[0]);
                circles[3].MoveBehindOtherNode(circles[2]);

                circles[1].MoveInFrontOfOtherNode(circles[0]);
            }
        }
    }

    public class SnailShell : ShellSprite
    {
        public TriangleMesh shellMesh, mouthMesh;
        public FSprite[] circles;

        public float mouthRad, sideRad;

        public SnailShell(CrabShell shell) : base(shell)
        {
            mouthRad = Random.Range(5f, 7f);
            sideRad = Random.Range(7.5f, 10f);
        }

        public override void InitSprites(List<FSprite> sprites)
        {
            base.InitSprites(sprites);

            shellMesh = new TriangleMesh("RainWorld_White", [new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(2, 1, 3)], false, true);
            mouthMesh = new TriangleMesh("RainWorld_White", [new TriangleMesh.Triangle(0, 1, 2), new TriangleMesh.Triangle(2, 1, 3)], false, true);

            circles = new FSprite[6];
            circles[0] = new FSprite("Circle20", false);
            circles[1] = new FSprite("Circle20", false);
            circles[2] = new FSprite("Circle20", false);
            circles[3] = new FSprite("Circle20", false);

            Debug.Log("CIRCLE20 DATA: " + circles[0].element.sourcePixelSize);

            sprites.Add(shellMesh);
            sprites.Add(mouthMesh);
            sprites.Add(circles[0]);
            sprites.Add(circles[1]);
            sprites.Add(circles[2]);
            sprites.Add(circles[3]);
        }

        public override void DrawSprites(Vector2 pos, Vector2 rot, Quaternion quaternion, Color blackColor, Color shellColor, float lightExposure)
        {
            Vector3 basePos1 = quaternion * new Vector3(0, -sideRad, 0);
            Vector3 tipPos1 = quaternion * new Vector3(0, sideRad, 0);
            Vector3 side1Pos1 = quaternion * new Vector3(0, 0, mouthRad);
            Vector3 side2Pos1 = quaternion * new Vector3(0, 0, -mouthRad);
            Vector3 mouth1Pos1 = quaternion * new Vector3(sideRad - mouthRad, -sideRad, 0);
            Vector3 mouth2Pos1 = quaternion * new Vector3(sideRad - mouthRad, 0, 0);

            Vector2 basePos2 = pos + (Vector2)basePos1;
            Vector2 tipPos2 = pos + (Vector2)tipPos1;
            Vector2 side1Pos2 = pos + (Vector2)side1Pos1;
            Vector2 side2Pos2 = pos + (Vector2)side2Pos1;
            Vector2 mouth1Pos2 = pos + (Vector2)mouth1Pos1;
            Vector2 mouth2Pos2 = pos + (Vector2)mouth2Pos1;

            Vector2 coneDir = Custom.DirVec(basePos2, tipPos2);
            float coneAngle = Custom.VecToDeg(coneDir);

            float baseZ = basePos1.normalized.z;
            float side1Z = side1Pos1.normalized.z;

            circles[0].SetPosition(side1Pos2);
            circles[0].rotation = coneAngle;
            circles[0].scaleX = sideRad / 10f;
            circles[0].scaleY = sideRad / 10f * Mathf.Abs(side1Z);

            circles[1].SetPosition(side2Pos2);
            circles[1].rotation = coneAngle;
            circles[1].scaleX = sideRad / 10f;
            circles[1].scaleY = sideRad / 10f * Mathf.Abs(side1Z);

            circles[2].SetPosition(mouth1Pos2);
            circles[2].rotation = coneAngle;
            circles[2].scaleX = mouthRad / 10f;
            circles[2].scaleY = mouthRad / 10f * Mathf.Abs(baseZ);

            circles[3].SetPosition(mouth1Pos2);
            circles[3].rotation = coneAngle;
            circles[3].scaleX = mouthRad / 20f;
            circles[3].scaleY = mouthRad / 20f * Mathf.Abs(baseZ);

            Vector2 perpTubeDir1 = Custom.PerpendicularVector(Custom.DirVec(side1Pos2, side2Pos2));
            for (int i = 0; i < shellMesh.vertices.Length; i += 2)
            {
                float lerp = (float)i / (shellMesh.vertices.Length - 2);

                Vector2 tubePos = Vector2.Lerp(side1Pos2, side2Pos2, lerp);
                float rad = sideRad;

                shellMesh.MoveVertice(i, tubePos + perpTubeDir1 * rad);
                shellMesh.MoveVertice(i + 1, tubePos - perpTubeDir1 * rad);
            }

            Vector2 perpTubeDir2 = Custom.PerpendicularVector(Custom.DirVec(mouth1Pos2, mouth2Pos2));
            for (int i = 0; i < mouthMesh.vertices.Length; i += 2)
            {
                float lerp = (float)i / (mouthMesh.vertices.Length - 2);

                Vector2 tubePos = Vector2.Lerp(mouth1Pos2, mouth2Pos2, lerp);
                float rad = mouthRad;

                mouthMesh.MoveVertice(i, tubePos + perpTubeDir2 * rad);
                mouthMesh.MoveVertice(i + 1, tubePos - perpTubeDir2 * rad);
            }

            UpdateSpriteLayers(side1Z);

            Color lightColor = Color.Lerp(shellColor, Color.white, lightExposure * 0.25f);

            Color meshColor = Color.Lerp(shellColor, lightColor, Mathf.Abs(baseZ));
            Color newShellColor = Color.Lerp(shellColor, lightColor, 1f - Mathf.Abs(baseZ));

            shellMesh.color = meshColor;
            mouthMesh.color = meshColor;
            circles[0].color = side1Z < 0 ? meshColor : newShellColor;
            circles[1].color = side1Z >= 0 ? meshColor : newShellColor;
            circles[2].color = meshColor;
            circles[3].color = baseZ < 0 ? meshColor : blackColor;

            circles[0].element = side1Z < 0 ? Futile.atlasManager.GetElementWithName("Circle20") : Futile.atlasManager.GetElementWithName("SnailShell");
            circles[1].element = side1Z >= 0 ? Futile.atlasManager.GetElementWithName("Circle20") : Futile.atlasManager.GetElementWithName("SnailShell");
        }

        public override void UpdateSpriteLayers(float z)
        {
            base.UpdateSpriteLayers(z);

            if (z < 0)
            {
                circles[0].MoveToBack();
                circles[1].MoveToFront();
            }
            else
            {
                circles[0].MoveToFront();
                circles[1].MoveToBack();
            }
        }
    }
}

public class AbstractCrabShellStick : AbstractPhysicalObject.AbstractObjectStick
{
    public AbstractCreature Crab
    {
        get { return A as AbstractCreature; }
    }
    public AbstractPhysicalObject Shell
    {
        get { return B as AbstractPhysicalObject; }
    }
    public AbstractCrabShellStick(AbstractPhysicalObject crab, AbstractPhysicalObject shell) : base(crab, shell)
    {
    }

    public override string SaveToString(int roomIndex)
    {
        return string.Concat(
        [
            roomIndex.ToString(),
            "<stkA>crabStk<stkA>",
            A.ID.ToString(),
            "<stkA>",
            B.ID.ToString()
        ]);
    }
}