using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevInterface;
using UnityEngine;
using RWCustom;
using System.Runtime.CompilerServices;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Items;

public class ScarletFlowerBulb : Weapon, IDrawable
{
    #region Variables

    new public Vector2 rotation, lastRotation;

    public LightSource lightSource;

    public Color lightColor;

    public Color redColor = new(1f, 0f, 0f);
    public Color blueColor = new(0f, 0f, 1f);
    public Color blackColor = new(0f, 0f, 0f);

    public bool charged, exploded, frozen;

    private Vector2 homePos;

    #endregion

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return abstractPhysicalObject as AbstractConsumable;
        }
    }

    public ScarletFlowerBulb(AbstractPhysicalObject abstractPhysicalObject, World world, bool frozen, Vector2 rotation, Color color) : base(abstractPhysicalObject, world)
    {
        bodyChunks = [new BodyChunk(this, 0, default, 5f, 0.35f)];
        bodyChunkConnections = [];

        if (frozen)
        {
            collisionLayer = 0;
            CollideWithObjects = false;
            CollideWithSlopes = false;
            CollideWithTerrain = false;
            gravity = 0f;
        }
        else
        {
            collisionLayer = 1;
            CollideWithObjects = true;
            CollideWithSlopes = true;
            CollideWithTerrain = true;
            gravity = 0.9f;
        }

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        this.frozen = frozen;
        this.rotation = rotation;

        double randomNum1 = UnityEngine.Random.Range(0f, 100f);
        double randomNum2 = UnityEngine.Random.Range(0f, 100f);
        float redNum;
        float greenNum;
        float blueNum;
        float alphaNum;
        if (randomNum1 > 90)
        {
            redNum = UnityEngine.Random.Range(0f, 0f);
            greenNum = UnityEngine.Random.Range(0f, 0.3f);
            blueNum = UnityEngine.Random.Range(0f, 0.3f);
        }
        else
        {
            redNum = UnityEngine.Random.Range(0f, 0f);
            greenNum = UnityEngine.Random.Range(0f, 0.1f);
            blueNum = UnityEngine.Random.Range(0f, 0.1f);
        }
        if (randomNum2 > 90)
        {
            alphaNum = UnityEngine.Random.Range(-0.2f, 0.2f);
        }
        else
        {
            alphaNum = 0f;
        }
        lightColor = new(
            color.r + redNum + alphaNum,
            color.g + greenNum + alphaNum,
            color.b + blueNum + alphaNum);
    }

    #region Object Behavior
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            homePos = placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos;
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
            return;
        }
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        #region Behavior
        if (grabbedBy.Count == 0 && !frozen)
        {
            ChangeCollisionLayer(1);
            SetLocalGravity(0.9f);
        }
        lastRotation = rotation;
        firstChunk.collideWithTerrain = grabbedBy.Count == 0;
        if (charged && (firstChunk.ContactPoint.x != 0 || firstChunk.ContactPoint.y != 0))
        {
            Explode();
        }
        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = -Mathf.Abs(rotation.y) + 90;
        }
        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.3f;
        }
        if (!AbstrConsumable.isConsumed && (Vector2.Distance(firstChunk.pos, homePos) > 5f || grabbedBy.Count > 0))
        {
            frozen = false;
            room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, firstChunk, false, 0.8f, 1.6f + UnityEngine.Random.value / 10f);
            AbstrConsumable.Consume();
        }
        rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.3f : 0f) * firstChunk.vel.x).normalized;
        #endregion

        #region Visuals
        if (lightSource == null)
        {
            lightSource = new LightSource(firstChunk.pos, false, lightColor, this);
            lightSource.affectedByPaletteDarkness = 0.5f;
            room.AddObject(lightSource);
        }
        else
        {
            lightSource.setPos = new Vector2?(firstChunk.pos);
            lightSource.setRad = new float?(50f);
            lightSource.setAlpha = new float?(1f);
            if (lightSource.slatedForDeletetion || lightSource.room != room)
            {
                lightSource = null;
            }
        }
        #endregion
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk.pos, 0.35f, 2f);
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
        {
            return false;
        }
        room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk.pos, 0.35f, 2f);
        Explode();
        return base.HitSomething(result, eu);
    }

    public override void HitWall()
    {
        room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk.pos, 0.35f, 2f);
        Explode();
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (speed > 10)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 pos = firstChunk.pos + Custom.RNV() * 5f * UnityEngine.Random.value;
                Vector2 vel = Custom.RNV() * 4f * (1 + UnityEngine.Random.value);
                room.AddObject(new Spark(pos, vel, lightColor, null, 20, 40));
            }
        }
        if (speed > 30)
        {
            Explode();
        }
    }

    public override void PickedUp(Creature upPicker)
    {
        room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, firstChunk);
    }

    public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
    {
        base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
        charged = true;
        Room room = this.room;
        if (room == null)
        {
            return;
        }
        room.PlaySound(SoundID.Slugcat_Throw_Flare_Bomb, firstChunk);
    }

    private void Explode()
    {
        if (!AbstrConsumable.isConsumed)
        {
            AbstrConsumable.Consume();
        }
        if (exploded == false)
        {
            var num = UnityEngine.Random.Range(5, 8);
            Vector2 vector = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);
            Vector2 vector2 = Custom.RNV();

            for (int k = 0; k < num; k++)
            {
                Vector2 pos = firstChunk.pos + Custom.RNV() * 5f * UnityEngine.Random.value;
                Vector2 vel = Custom.RNV() * 4f * (1 + UnityEngine.Random.value);
                room.AddObject(new Spark(pos, vel, lightColor, null, 20, 40));
            }

            for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
            {
                Vector2 creaturePosition = room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos;
                if (Custom.DistLess(firstChunk.pos, creaturePosition, 1600f) && room.abstractRoom.creatures[i].realizedCreature != null &&
                    room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat)
                {
                    room.PlaySound(SoundID.Centipede_Shock, creaturePosition, 0.25f, 1.25f);
                    room.AddObject(new Explosion.ExplosionSmoke(creaturePosition, vector2, 1.1f));
                    room.AddObject(new Explosion.ExplosionLight(creaturePosition, 100f, 1f, 7, new Color(1f, 0f, 0f)));
                    room.abstractRoom.creatures[i].realizedCreature.Stun(200);
                }
            }

            var num2 = UnityEngine.Random.Range(0.2f, 0.3f);

            room.PlaySound(SoundID.Bomb_Explode, firstChunk.pos, 0.75f, 1.25f);

            room.AddObject(new Explosion.ExplosionSmoke(vector, vector2, 1.1f));
            room.AddObject(new Explosion.ExplosionLight(vector, 400f, 1f, 7, lightColor));
            room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, lightColor));
            room.AddObject(new ShockWave(vector, 4000f, 0.05f, 20, true));
        }
        exploded = true;
        AllGraspsLetGoOfThisObject(true);
        abstractPhysicalObject.LoseAllStuckObjects();
        Destroy();
    }
    #endregion

    #region Object Appearance

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite("FlowerBulb", true);
        sLeaser.sprites[1] = new FSprite("FlowerStem", true);

        sLeaser.sprites[2] = new FSprite("Futile_White", true);
        sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];

        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 posVec = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);

        Vector2 stemPos = SpritePosition(timeStacker, -2f);
        Vector2 lightPos = SpritePosition(timeStacker, 8f);

        sLeaser.sprites[0].x = posVec.x - camPos.x;
        sLeaser.sprites[0].y = posVec.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec);

        sLeaser.sprites[1].x = stemPos.x - camPos.x;
        sLeaser.sprites[1].y = stemPos.y - camPos.y;
        sLeaser.sprites[1].rotation = Custom.VecToDeg(rotVec);

        sLeaser.sprites[2].x = lightPos.x - rotVec.x * 3f - camPos.x;
        sLeaser.sprites[2].y = lightPos.y - rotVec.y * 3f - camPos.y;
        sLeaser.sprites[2].scale = 2f;

        if (blink > 0 && UnityEngine.Random.value < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
            sLeaser.sprites[1].color = blinkColor;
            sLeaser.sprites[2].color = lightColor;
        }
        else
        {
            sLeaser.sprites[0].color = lightColor;
            sLeaser.sprites[1].color = blackColor;
            sLeaser.sprites[2].color = lightColor;
        }

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public Vector2 SpritePosition(float timestacker, float position)
    {
        Vector3 vector = Vector3.Slerp(lastRotation, rotation, timestacker);
        Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timestacker) * -4f;
        return Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timestacker) + new Vector2(vector.x, vector.y) * position + new Vector2(vector2.x, vector2.y);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }

        rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
    }

    #endregion

}

public class ScarletFlower : UpdatableAndDeletable, IDrawable
{
    private PlacedObject pobj;
    private Vector2 rotation;

    public AbstractConsumable consumable;

    public ScarletFlower(PlacedObject pobj, Vector2 rotation)
    {
        this.pobj = pobj;
        this.rotation = rotation;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (newContainer == null)
        {
            newContainer = rCam.ReturnFContainer("Items");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            newContainer.AddChild(sLeaser.sprites[i]);
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];

        sLeaser.sprites[0] = new FSprite("ScarletFlowerStem", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        sLeaser.sprites[0].SetPosition(pobj.pos - camPos);
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotation);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[0].color = palette.blackColor;
    }
}

public class ScarletFlowerData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    public Vector2 rotation;
    new public int minRegen;
    new public int maxRegen;

    public ScarletFlowerData(PlacedObject owner) : base(owner)
    {
        panelPos = new Vector2(0f, 100f);
        rotation = new Vector2(0f, 100f);
        minRegen = 2;
        maxRegen = 3;
    }

    new protected string BaseSaveString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}", new object[]
        {
            panelPos.x,
            panelPos.y,
            minRegen,
            maxRegen,
            rotation.x,
            rotation.y,
        });
    }

    public override void FromString(string s)
    {
        string[] array = Regex.Split(s, "~");
        panelPos.x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
        panelPos.y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
        minRegen = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
        maxRegen = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
        rotation.x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
        rotation.y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
        unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
    }

    public override string ToString()
    {
        string text = BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class ScarletFlowerRepresentation : ConsumableRepresentation
{
    public ScarletFlowerData data;
    public Handle rotationHandle;
    //public ConsumableControlPanel controlPanel;

    public ScarletFlowerRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as ScarletFlowerData;
        rotationHandle = new(owner, "Rotation_Handle", this, data.rotation);
        controlPanel = new(owner, "ScarletFlower_Panel", this, data.panelPos, "Consumable: Scarlet Flower");

        subNodes[0].ClearSprites();
        subNodes.RemoveAt(0);

        subNodes.Add(rotationHandle);
        subNodes.Add(controlPanel);
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        fSprites.Add(new FSprite("pixel") { anchorY = 0f });
        owner.placedObjectsContainer.AddChild(fSprites[1]);
        owner.placedObjectsContainer.AddChild(fSprites[2]);
    }

    public override void Refresh()
    {
        base.Refresh();

        MoveSprite(1, absPos);
        fSprites[1].scaleY = rotationHandle.pos.magnitude;
        fSprites[1].rotation = Custom.AimFromOneVectorToAnother(absPos, rotationHandle.absPos);
        (pObj.data as ScarletFlowerData).rotation = rotationHandle.pos;

        MoveSprite(2, absPos);
        fSprites[2].scaleY = controlPanel.pos.magnitude;
        fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as ScarletFlowerData).panelPos = controlPanel.pos;

        /*
        UnityEngine.Debug.Log("");
        for (int i = 0; i < subNodes.Count; i++)
        {
            UnityEngine.Debug.Log(i + ": " + subNodes[i]);
        }
        UnityEngine.Debug.Log("");
        */
    }
}
