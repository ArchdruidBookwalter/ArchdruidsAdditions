using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using HUD;

using Random = UnityEngine.Random;
using static ArchdruidsAdditions.Methods.Methods;
using static ArchdruidsAdditions.Objects.SpiceMeter;

namespace ArchdruidsAdditions.Objects;

public class FirePepper : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public int bites = 1;
    public Vector2 rotation, lastRotation;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return this.abstractPhysicalObject as AbstractConsumable;
        }
    }

    public FirePepper(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
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
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        lastRotation = rotation;

        if (grabbedBy.Count == 0 && firstChunk.vel.magnitude > 1f)
        {
            rotation = Custom.rotateVectorDeg(rotation, firstChunk.vel.magnitude);
        }
        else if (grabbedBy.Count > 0)
        {
            rotation = Vector2.up;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        room = placeRoom;

        if (!AbstrConsumable.isConsumed)
        {
            bodyChunks[0].HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
        }
        else
        {
            bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        }
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

        pepper = new("FirePepper1")
        {
            scale = 0.8f,
            color = bodyColor
        };
        sprites.Add(pepper);

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
            Vector2 rot = Vector2.Lerp(lastRotation, rotation, timeStacker);

            pepper.SetPosition(chunkPos);
            pepper.rotation = Custom.VecToDeg(rot);
            pepper.element = Futile.atlasManager.GetElementWithName("FirePepper1");
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
    #endregion
}

public class SpiceMeter
{
    public PlayerData.PlayerData.AAPlayerState playerState;
    public FoodMeter foodMeter;
    public Player player;

    public SpiceCircle[] circles;
    public int pulseCounter = 0;
    public float pulseAmount = 0;
    public float pulseLength = 0;

    public int foodPips = 0;
    public int maxPips = 0;
    public int spicePips = 0;

    public int activeSpicePipes = 0;

    public SpiceMeter(HUD.HUD hud, int maxFood)
    {
        player = hud.owner as Player;
        playerState = PlayerData.PlayerData.playerStates[player.abstractCreature];

        circles = new SpiceCircle[maxFood];
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = new(hud, this);
        }
    }

    public void Update(FoodMeter foodMeter)
    {
        this.foodMeter = foodMeter;

        foodPips = player.FoodInStomach;
        maxPips = player.MaxFoodInStomach;
        spicePips = playerState.spiceAmount;

        for (int i = 0; i < circles.Count(); i++)
        {
            FoodMeter.MeterCircle foodCircle = foodMeter.circles[i];
            SpiceMeter.SpiceCircle spiceCircle = circles[i];

            if (foodPips >= maxPips - 1 && spicePips > 0)
            {
                spiceCircle.hide = true;
            }
            else
            {
                if (i < foodPips && foodCircle.foodPlopped)
                {
                    spiceCircle.hide = true;
                }
                else if ((spicePips > 0 && i < foodPips + spicePips) || (i < foodPips && !spiceCircle.hide))
                {
                    spiceCircle.hide = false;
                }
                else
                {
                    spiceCircle.hide = true;
                }
            }

            if (spiceCircle.hide)
            {
                if (spiceCircle.fade > 0f)
                { spiceCircle.fade -= 0.1f; }
                else if (spiceCircle.fade != 0f)
                { spiceCircle.fade = 0f; }
            }
            else
            {
                if (spiceCircle.fade < 1f)
                { spiceCircle.fade += 0.1f; }
                else if (spiceCircle.fade != 1f)
                { spiceCircle.fade = 1f; }
            }
        }

        pulseLength = 50;
        pulseCounter++;
        if (pulseCounter > pulseLength * 2)
        {
            pulseCounter = 1;
        }
        pulseAmount = Mathf.PingPong(pulseCounter, pulseLength);
    }

    public class SpiceCircle : HUDCircle
    {
        public SpiceMeter spiceMeter;
        public FoodMeter foodMeter;
        public FoodMeter.MeterCircle foodCircle;
        public bool hide;
        public SpiceCircle(HUD.HUD hud, SpiceMeter spiceMeter) : base(hud, HUDCircle.SnapToGraphic.FoodCircleA, hud.fContainers[1], 0)
        {
            this.spiceMeter = spiceMeter;

            hide = true;
        }

        public void Draw(FoodMeter.MeterCircle foodCircle)
        {
            FSprite foodSprite = foodCircle.circles[0].sprite;
            FSprite spiceSprite = sprite;

            float pulse = spiceMeter.pulseAmount / spiceMeter.pulseLength / 10f;

            spiceSprite.x = foodSprite.x;
            spiceSprite.y = foodSprite.y;
            spiceSprite.shader = foodSprite.shader;
            spiceSprite.alpha = Mathf.Min(foodSprite.alpha, fade);
            spiceSprite.scale = foodSprite.scale + pulse + 0.2f;
            spiceSprite.isVisible = foodSprite.isVisible;

            Vector2 pos = foodSprite.GetPosition();
            spiceSprite.x = pos.x + 0.5f;
            spiceSprite.y = pos.y;

            if (spiceSprite.shader == basicShader)
            {
                spiceSprite.element = Futile.atlasManager.GetElementWithName(snapGraphic.ToString());
                spiceSprite.color = new Color(1f, 0f, 0f);
            }
            else
            {
                spiceSprite.element = Futile.atlasManager.GetElementWithName("Futile_White");
                spiceSprite.color = new Color(1f / 255f, foodSprite.color.g, foodSprite.color.b);
            }

            spiceSprite.MoveBehindOtherNode(foodSprite);
        }
    }
}
