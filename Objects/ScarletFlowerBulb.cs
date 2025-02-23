using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL;
using UnityEngine;
using RWCustom;
using On;

namespace ArchdruidsAdditions.Objects;

public class ScarletFlowerBulb : Weapon, IDrawable
{

    #region Variables
    private static float Rand => UnityEngine.Random.value;

    new public Vector2 rotation;
    new public Vector2 lastRotation;
    new public Vector2? setRotation;

    public LightSource lightSource;

    public Color lightColor = new Color(1f, 0f, 0f);

    private bool charged;
    private bool exploded;
    private bool frozen;

    private readonly float rotationOffset;
    #endregion

    private AbstractConsumable Consumable
    {
        get
        {
            return abstractPhysicalObject as AbstractConsumable;
        }
    }

    public ScarletFlowerBulb(AbstractPhysicalObject abstr, World world) : base(abstr, world)
    {
        bodyChunks = new[]
        {
            new BodyChunk(this, 0, default, 5f, 0.35f)
        };
        bodyChunkConnections = Array.Empty<BodyChunkConnection>();

        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        collisionLayer = 1;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        rotationOffset = Rand * 30 - 15;
    }

    #region Object Behavior

    private void Explode()
    {
        if (exploded == false)
        {
            var num = UnityEngine.Random.Range(5, 8);
            Vector2 vector = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);
            Vector2 vector2 = Custom.RNV();

            for (int k = 0; k < num; k++)
            {
                Vector2 pos = firstChunk.pos + Custom.RNV() * 5f * Rand;
                Vector2 vel = Custom.RNV() * 4f * (1 + Rand);
                room.AddObject(new Spark(pos, vel, lightColor, null, 20, 40));
            }

            for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
            {
                Vector2 creaturePosition = room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos;
                if (Custom.DistLess(firstChunk.pos, creaturePosition, 1600f) && room.abstractRoom.creatures[i].realizedCreature != null &&
                    room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Slugcat)
                {
                    room.PlaySound(SoundID.Centipede_Shock, creaturePosition, 0.75f, 1.25f);
                    room.AddObject(new Explosion.ExplosionSmoke(creaturePosition, vector2, 1.1f));
                    room.AddObject(new Explosion.ExplosionLight(creaturePosition, 100f, 1f, 7, new Color(1f, 0f, 0f)));
                    room.abstractRoom.creatures[i].realizedCreature.Stun(200);
                }
            }

            var num2 = UnityEngine.Random.Range(0.2f, 0.3f);

            room.PlaySound(SoundID.Bomb_Explode, firstChunk.pos, 0.75f, 1.25f);
            room.PlaySound(SoundID.Bomb_Explode, firstChunk.pos, 0.75f, num2);
            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, firstChunk.pos, 0.75f, num2);

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

    public override void Update(bool eu)
    {
        base.Update(eu);

        #region Behavior
        if (grabbedBy.Count == 0)
        {
            ChangeCollisionLayer(1);
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
            frozen = false;
        }
        if (!frozen)
        if (setRotation != null)
        {
            rotation = setRotation.Value;
            setRotation = null;
        }
        rotation = (rotation - Custom.PerpendicularVector(rotation) * (firstChunk.ContactPoint.y < 0 ? 0.15f : 0.05f) * firstChunk.vel.x).normalized;
        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.8f;
        }
        if (abstractPhysicalObject is AbstractConsumable && grabbedBy.Count > 0 && !(abstractPhysicalObject as AbstractConsumable).isConsumed)

        {
            (abstractPhysicalObject as AbstractConsumable).Consume();
        }
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
                Vector2 pos = firstChunk.pos + Custom.RNV() * 5f * Rand;
                Vector2 vel = Custom.RNV() * 4f * (1 + Rand);
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

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        rotation = Custom.RNV();
        lastRotation = rotation;
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
        sLeaser.sprites[0].color = lightColor;
        sLeaser.sprites[2].color = lightColor;
        sLeaser.sprites[1].color = palette.blackColor;
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
