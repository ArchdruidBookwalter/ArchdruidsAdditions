using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using DevInterface;
using System.Security.Policy;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ArchdruidsAdditions.Objects;

public class Potato : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public bool buried;
    public Vector2 rotation, lastRotation, startRotation;
    public Color rootColor;
    public Color blackColor = new(0f, 0f, 0f);
    public int bites = 3;
    public Vector2 homePos;

    public float stemLength = UnityEngine.Random.Range(10f, 15f);
    public float elasticity = 0.8f;
    public bool randomFlip1 = UnityEngine.Random.Range(-1f, 1f) < 0 ? true : false;
    public bool randomFlip2 = UnityEngine.Random.Range(-1f, 1f) < 0 ? true : false;

    public bool playerSquint;
    public ChunkDynamicSoundLoop soundLoop;

    public AbstractConsumable AbstrConsumable
    {
        get
        {
            return this.abstractPhysicalObject as AbstractConsumable; 
        }
    }

    public Potato(AbstractPhysicalObject abstractPhysicalObject, bool buried, Vector2 rotation, Color color, bool defaultColor) : base(abstractPhysicalObject)
    {
        bodyChunks = new BodyChunk[2];
        bodyChunks[0] = new BodyChunk(this, 0, default, 6f, 0.2f);
        bodyChunks[1] = new BodyChunk(this, 0, default, 3f, 0.1f);
        bodyChunkConnections = new BodyChunkConnection[1];
        bodyChunkConnections[0] = new BodyChunkConnection(bodyChunks[0], bodyChunks[1], stemLength, BodyChunkConnection.Type.Normal, elasticity, -1f);

        if (buried)
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

        bodyChunks[1].collideWithObjects = false;

        airFriction = 0.999f;
        bounce = 0.2f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 1.2f;

        this.buried = buried;
        startRotation = rotation.normalized;

        if (defaultColor)
        {
            float randomNum = UnityEngine.Random.Range(0f, 100f);
            float hue;
            float val;

            if (randomNum > 90f)
            {
                hue = 90f;
                val = 80f;
            }
            else if (randomNum > 50f)
            {
                hue = 10f;
                val = 80f;
            }
            else
            {
                hue = 5f;
                val = 80f;
            }

            color = UnityEngine.Random.ColorHSV(
                (hue - 2) / 100, (hue + 2) / 100, 
                0.8f, 0.8f, 
                (val - 2) / 100, (val + 2) / 100);
        }
        rootColor = color;

        soundLoop = new(bodyChunks[1])
        {
            sound = SoundID.Tentacle_Plant_Move_LOOP,
            Volume = 0f,
            Pitch = 1f
        };

        playerSquint = false;
    }

    #region Behavior

    public float soundTimer = 0f;
    public int soundIndex = 0;
    public float soundSpeed = 0f;

    public int pollinated;
    public int depollinateTimer;

    public override void Update(bool eu)
    {
        base.Update(eu);
        soundLoop.Update();
        lastRotation = rotation;

        /*
        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player && room.game.devToolsActive)
        {
            int input = player.input[0].y;
            if (input != 0)
            {
                if (soundTimer == 0f)
                {
                    if (input == 1)
                    {
                        soundIndex++;
                        if (soundIndex > SoundID.values.entries.Count - 1)
                        {
                            soundIndex = 0;
                        }
                    }
                    else
                    {
                        soundIndex--;
                        if (soundIndex < 0)
                        {
                            soundIndex = SoundID.values.entries.Count - 1;
                        }
                    }
                    Debug.Log("Index: " + soundIndex.ToString());
                    string name = SoundID.values.entries[soundIndex];
                    SoundID sound = new SoundID(name);
                    if (!name.Contains("LOOP"))
                    {
                        room.PlaySound(sound, player.mainBodyChunk, false, 1f, 1f);
                        room.AddObject(new ExplosionSpikes(room, player.mainBodyChunk.pos, 50, 10f, 5f, 10f, 10f, new(0f, 1f, 0f)));
                    }
                    else
                    {
                        room.AddObject(new ExplosionSpikes(room, player.mainBodyChunk.pos, 50, 10f, 5f, 10f, 10f, new(0f, 1f, 0f)));
                    }
                    Debug.Log(sound.ToString());
                    soundTimer = 20f;
                    if (soundSpeed < 10f)
                    {
                        soundSpeed += 1f;
                    }
                }
                if (soundTimer > 0)
                {
                    soundTimer -= soundSpeed;
                }
                if (soundTimer < 0)
                {
                    soundTimer = 0;
                }
            }
            else
            {
                soundTimer = 0;
                soundSpeed = 0;
            }
        }*/

        if (firstChunk.ContactPoint.y < 0)
        {
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x = firstChunk.vel.x * 0.3f;
        }

        if (buried)
        {
            ChangeCollision(false, false);
            gravity = 0f;
            bodyChunks[0].vel = Vector2.zero;
            bodyChunks[0].HardSetPosition(homePos);

            Color whiteColor = new(1f, 1f, 1f);
            Color blackColor = new(0.01f, 0.01f, 0.01f);

            float dist = Custom.Dist(bodyChunks[0].pos, bodyChunks[1].pos);
            if (dist > stemLength + 60f)
            {
                buried = false;
                room.PlaySound(SoundID.Spear_Stick_In_Wall, firstChunk, false, 1f, 1f);
                bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos) * 20f + startRotation * 10f;
                AbstrConsumable.Consume();
                bodyChunkConnections[0].elasticity = elasticity;
                soundLoop.Volume = 0f;
                playerSquint = false;
                AllGraspsLetGoOfThisObject(true);
                for (int i = 0; i < UnityEngine.Random.Range(3f, 6f); i++)
                {
                    float speed = UnityEngine.Random.Range(5f, 10f);
                    room.AddObject(new WaterDrip(bodyChunks[0].pos + startRotation * 10, startRotation * speed + Custom.RNV() * speed, false)); 
                }
            }
            else if (dist > stemLength + 20f)
            {
                bodyChunkConnections[0].elasticity *= 0.99f;
                soundLoop.Volume = dist/60f;
                playerSquint = true;
            }
            else if (dist > stemLength + 10f)
            {
                bodyChunkConnections[0].elasticity *= 0.99f;
                soundLoop.Volume = 0f;
                playerSquint = false;
            }
            else
            {
                bodyChunkConnections[0].elasticity = elasticity;
                soundLoop.Volume = 0f;
                playerSquint = false;
            }
            

            if (grabbedBy.Count == 0)
            {
                bodyChunks[1].vel = startRotation * 2;
            }
            else
            {
                bodyChunks[1].HardSetPosition(bodyChunks[1].pos);
                if (grabbedBy[0].grabbedChunk == bodyChunks[0])
                {
                    AllGraspsLetGoOfThisObject(true);
                }
            }
        }
        else if (!buried && grabbedBy.Count > 0)
        {
            ChangeCollision(false, false);
            if (grabbedBy[0].grabber is Player player2)
            {
                if (grabbedBy[0].graspUsed == 0)
                {
                    if (player2.mainBodyChunk.vel.x < -1)
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        {
                            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Background"));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        {
                            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Items"));
                        }
                    }
                }
                else
                {
                    if (player2.mainBodyChunk.vel.x > 1)
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        {
                            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Background"));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        {
                            room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Items"));
                        }
                    }
                }
                bodyChunks[1].vel.y += 10f;
            }
            gravity = 0.9f;
            bodyChunkConnections[0].elasticity = elasticity;
            soundLoop.Volume = 0f;
            playerSquint = false;
        }
        else
        {
            for (int i = 0; i < room.game.cameras.Length; i++)
            {
                room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer("Items"));
            }
            ChangeCollision(true, true);
            gravity = 0.9f;
            bodyChunkConnections[0].elasticity = elasticity;
            soundLoop.Volume = 0;
            playerSquint = false;
        }

        if (pollinated >= 500)
        {
            depollinateTimer++;
        }

        if (depollinateTimer >= 1000)
        {
            depollinateTimer = 0;
            pollinated = 0;
        }

        rotation = Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos);
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            bodyChunks[0].HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
            bodyChunks[1].HardSetPosition(bodyChunks[0].pos + new Vector2(0f, stemLength));
            homePos = bodyChunks[0].pos;
            return;
        }
        bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        bodyChunks[1].HardSetPosition(bodyChunks[0].pos + new Vector2(0f, stemLength));
        homePos = bodyChunks[0].pos;
    }

    public override void Grabbed(Creature.Grasp grasp)
    {
        base.Grabbed(grasp);
        Debug.Log("Picked Up Potato");
    }

    public void ChangeCollision(bool chunk1collide, bool chunk2collide)
    {
        if (chunk1collide)
        {
            bodyChunks[0].collideWithObjects = true;
            bodyChunks[0].collideWithSlopes = true;
            bodyChunks[0].collideWithTerrain = true;
        }
        else
        {
            bodyChunks[0].collideWithObjects = false;
            bodyChunks[0].collideWithSlopes = false;
            bodyChunks[0].collideWithTerrain = false;
        }
        if (chunk2collide)
        {
            bodyChunks[1].collideWithObjects = false;
            bodyChunks[1].collideWithSlopes = true;
            bodyChunks[1].collideWithTerrain = true;
        }
        else
        {
            bodyChunks[1].collideWithObjects = false;
            bodyChunks[1].collideWithSlopes = false;
            bodyChunks[1].collideWithTerrain = false;
        }
    }

    #region Edible Stuff
    public int BitesLeft
    {
        get { return bites; }
    }

    public int FoodPoints
    {
        get { return 1; }
    }

    public bool Edible
    {
        get 
        { 
            if (buried)
            {  return false; }
            return true; 
        }
    }

    public bool AutomaticPickUp
    {
        get
        {
            if (buried)
            { return false; }
            return true;
        }
    }

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

    #endregion

    #region Visuals
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
        sLeaser.sprites = new FSprite[4];

        sLeaser.sprites[0] = new FSprite("Potato1", true);
        sLeaser.sprites[1] = new FSprite("PotatoStem1", true);
        sLeaser.sprites[2] = new FSprite("PotatoStem2", true);
        sLeaser.sprites[3] = new FSprite("DangleFruit0A", true);

        AddToContainer(sLeaser, rCam, null);
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
        Vector2 rootPosVec = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker) + rotVec * -5f;
        Vector2 leavesPosVec = Vector2.Lerp(bodyChunks[1].lastPos, bodyChunks[1].pos, timeStacker) + rotVec * 5f;

        sLeaser.sprites[0].x = rootPosVec.x - camPos.x;
        sLeaser.sprites[0].y = rootPosVec.y - camPos.y;
        sLeaser.sprites[0].rotation = Custom.VecToDeg(rotVec) - 30;

        sLeaser.sprites[1].x = leavesPosVec.x - camPos.x;
        sLeaser.sprites[1].y = leavesPosVec.y - camPos.y;
        sLeaser.sprites[1].rotation = Custom.VecToDeg(rotVec) + 90;

        Vector2 rootTopPosVec = rootPosVec + rotVec * 10f;
        Vector2 leavesBottomPosVec = leavesPosVec - rotVec * 2f;
        Vector2 flowerPos = leavesPosVec + rotVec * 5f;

        sLeaser.sprites[2].x = Vector2.Lerp(rootTopPosVec, leavesBottomPosVec, 0.5f).x - camPos.x;
        sLeaser.sprites[2].y = Vector2.Lerp(rootTopPosVec, leavesBottomPosVec, 0.5f).y - camPos.y;
        sLeaser.sprites[2].width = Custom.Dist(rootTopPosVec, leavesBottomPosVec);
        sLeaser.sprites[2].rotation = Custom.VecToDeg(rotVec) + 90;

        sLeaser.sprites[3].x = flowerPos.x - camPos.x;
        sLeaser.sprites[3].y = flowerPos.y - camPos.y;
        sLeaser.sprites[3].rotation = Custom.VecToDeg(rotVec);
        sLeaser.sprites[3].scale = 0.5f;

        if (randomFlip1)
        {
            sLeaser.sprites[1].scaleY = -1;
        }
        if (randomFlip2)
        {
            sLeaser.sprites[2].scaleY = -1;
        }

        if (buried)
        {
            sLeaser.sprites[0].alpha = 0;
        }
        else
        {
            sLeaser.sprites[0].alpha = 1;
        }

        if (bites > 0)
        {
            sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("Potato" + (4 - bites).ToString());
        }

        if (blink > 0 && UnityEngine.Random.value < 0.5f)
        {
            sLeaser.sprites[0].color = blinkColor;
            sLeaser.sprites[1].color = blinkColor;
            sLeaser.sprites[2].color = blinkColor;
            sLeaser.sprites[3].color = blinkColor;
        }
        else
        {
            sLeaser.sprites[0].color = rootColor;
            sLeaser.sprites[1].color = blackColor;
            sLeaser.sprites[2].color = blackColor;
            sLeaser.sprites[3].color = new(0.8f, 0.8f, 0.8f);
        }

        sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[0]);
        sLeaser.sprites[3].MoveBehindOtherNode(sLeaser.sprites[2]);

        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}

public class PotatoData : PlacedObject.ConsumableObjectData
{
    new public Vector2 panelPos;
    public Vector2 rotation;
    new public int minRegen;
    new public int maxRegen;

    public PotatoData(PlacedObject owner) : base(owner)
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
        string text = this.BaseSaveString();
        text = SaveState.SetCustomData(this, text);
        return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", unrecognizedAttributes);
    }
}

public class PotatoRepresentation : ConsumableRepresentation
{
    public PotatoData data;
    public Handle rotationHandle;
    //public ConsumableControlPanel controlPanel;

    public PotatoRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pobj, string name) :
        base(owner, IDstring, parentNode, pobj, name)
    {
        data = pobj.data as PotatoData;
        rotationHandle = new(owner, "Rotation_Handle", this, data.rotation);
        controlPanel = new(owner, "Potato_Panel", this, data.panelPos, "Consumable: Potato");

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
        (pObj.data as PotatoData).rotation = rotationHandle.pos;

        MoveSprite(2, absPos);
        fSprites[2].scaleY = controlPanel.pos.magnitude;
        fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
        (pObj.data as PotatoData).panelPos = controlPanel.pos;

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


