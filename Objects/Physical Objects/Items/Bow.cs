using System;
using ArchdruidsAdditions.Data;

namespace ArchdruidsAdditions.Objects.PhysicalObjects.Items;

public class Bow : Weapon, IDrawable
{
    new public Vector2 rotation, lastRotation;
    public float bowLength = 60f;
    public float bowWidth = 10f;
    public string lastLayer;

    public Spear loadedSpear;

    public Vector2 stringPos, lastStringPos;
    public Vector2 aimDirection = new(0, 1);
    public bool aiming;
    public int lastDirection = 1;

    public int lastAimCharge, aimCharge, shootThreshold = 25, maxAimCharge = 100;

    public Vector2 camPos;
    public Vector2? cursorPos = null;
    public Player.InputPackage? basePackage = null;

    Color blackColor = new(0f, 0f, 0f);

    new public ChunkDynamicSoundLoop soundLoop;

    public Creature Wielder
    {
        get
        {
            if (grabbedBy.Count > 0)
            { return grabbedBy[0].grabber; }
            return null;
        }
    }

    public Bow(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
    {
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0, 0), 5f, 0.1f);

        bodyChunkConnections = [];

        collisionLayer = 2;
        CollideWithObjects = true;
        CollideWithSlopes = true;
        CollideWithTerrain = true;
        gravity = 0.9f;

        airFriction = 0.999f;
        bounce = 0.3f;
        surfaceFriction = 0.2f;
        waterFriction = 0.92f;
        buoyancy = 0.2f;

        rotation = new(0f, 1f);
        lastRotation = rotation;
        stringPos = firstChunk.pos;
        lastStringPos = stringPos;

        lastLayer = "Items";

        soundLoop = new ChunkDynamicSoundLoop(firstChunk);
        soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
        soundLoop.Volume = 0f;
        soundLoop.Pitch = 0.5f;
    }

    #region Behavior
    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
    }
    public bool getRotation = false;
    public override void Update(bool eu)
    {
        base.Update(eu);

        lastAimCharge = aimCharge;
        lastRotation = rotation;
        lastStringPos = stringPos;

        soundLoop.Update();

        rotationSpeed = Mathf.Clamp(rotationSpeed, -10f, 10f);

        if (loadedSpear is not null)
        {
            if (loadedSpear.grabbedBy.Count == 0)
            {
                UnloadSpearFromBow();
            }
        }

        if (Wielder != null)
        {
            CollideWithObjects = false;
            CollideWithTerrain = false;

            if (Wielder is Player && basePackage.HasValue)
            {
                if (basePackage.Value.x != 0)
                { lastDirection = basePackage.Value.x; }
            }

            if (aiming)
            {
                rotation = Custom.DirVec(Wielder.mainBodyChunk.pos, GetAimPos(Wielder));

                if (aimCharge < maxAimCharge)
                {
                    aimCharge++;
                }

                if (aimCharge >= shootThreshold)
                {
                    soundLoop.Volume = 0f;
                }
                else if (aimCharge > 1)
                {
                    soundLoop.Volume = 1f;
                }

                if (Wielder is Player player)
                {
                    Player.InputPackage input = basePackage.Value;
                    PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
                    SlugcatHand arrowHand = graphics.hands[loadedSpear.grabbedBy[0].graspUsed];
                    SlugcatHand bowHand = graphics.hands[grabbedBy[0].graspUsed];

                    if (Plugin.Options.aimBowControls.Value == "Directional Inputs")
                    {
                        Vector2 dir = input.analogueDir.normalized;
                        cursorPos += dir * (input.pckp ? 2f : 10f);
                    }

                    if (aimCharge < maxAimCharge && player.animation == Player.AnimationIndex.Flip)
                    {
                        aimCharge += 2;
                    }

                    bool isPlayerBusy =
                        player.animation == Player.AnimationIndex.ClimbOnBeam ||
                        player.animation == Player.AnimationIndex.HangFromBeam ||
                        player.animation == Player.AnimationIndex.HangUnderVerticalBeam;

                    if (input.thrw && !isPlayerBusy)
                    {
                        if (aimCharge > 1)
                        {
                            graphics.LookAtPoint(GetAimPos(player), 10f);
                        }
                    }
                    else
                    {
                        Shoot(eu);

                        arrowHand.pushOutOfTerrain = true;
                        bowHand.pushOutOfTerrain = true;
                    }
                }
                else if (Wielder is Scavenger scav)
                {
                    ScavengerGraphics graphics = scav.graphicsModule as ScavengerGraphics;
                    ScavengerGraphics.ScavengerHand bowHand = graphics.hands[0];
                    ScavengerGraphics.ScavengerHand arrowHand = graphics.hands[1];

                    if (scav.AI.preyTracker.MostAttractivePrey != null)
                    {
                        if (aimCharge >= maxAimCharge)
                        {
                            if (scav.AI.preyTracker.MostAttractivePrey.VisualContact && scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
                            {
                                Shoot(eu);
                            }
                            else
                            {
                                UnloadSpearFromBow();
                            }
                        }
                    }
                    else
                    {
                        UnloadSpearFromBow();
                    }
                }
            }
            else
            {
                UnloadSpearFromBow();

                if (Wielder is Player player)
                {
                    getRotation = true;
                    rotation = player.GetHeldItemDirection(grabbedBy[0].graspUsed);
                    getRotation = false;
                }
                else if (Wielder is Scavenger scav && grabbedBy[0].graspUsed > 0 && scav.graphicsModule != null)
                {
                    rotation = (scav.graphicsModule as ScavengerGraphics).ItemDirection(grabbedBy[0].graspUsed);
                }
                else
                {
                    rotation = Custom.DirVec(Wielder.mainBodyChunk.pos, firstChunk.pos);
                }
            }
        }
        else
        {
            CollideWithObjects = true;
            CollideWithTerrain = true;

            UnloadSpearFromBow();
        }

        float clampedAimCharge = Mathf.Clamp((float)aimCharge / shootThreshold, 0f, 1f);
        float bowWidth = 10f + clampedAimCharge * 5f;
        stringPos = firstChunk.pos - (rotation * bowWidth) - (rotation * clampedAimCharge * 20f);
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }
    public override void PickedUp(Creature upPicker)
    {
        base.PickedUp(upPicker);
    }
    public override void HitByWeapon(Weapon weapon)
    {
    }
    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        return false;
    }

    public void LoadSpearIntoBow(Spear spear)
    {
        if (aiming == false)
        {
            room.PlaySound(SoundID.Slugcat_Pick_Up_Rock, firstChunk.pos, 1f, 1f);
        }

        Debug.Log("LOADED SPEAR INTO BOW!");

        aiming = true;
        loadedSpear = spear;

        if (!PlayerData.spearsLoadedInBows.ContainsKey(spear))
        {
            PlayerData.spearsLoadedInBows.Add(spear, this);
        }
    }
    public void UnloadSpearFromBow()
    {
        aiming = false;
        aimCharge = 0;
        soundLoop.Volume = 0f;

        if (loadedSpear != null)
        {
            if (PlayerData.spearsLoadedInBows.ContainsKey(loadedSpear))
            { PlayerData.spearsLoadedInBows.Remove(loadedSpear); }
        }

        if (Wielder != null && Wielder is Player player)
        {
            if (Plugin.Options.aimBowControls.Value == "Mouse")
            {
                cursorPos = new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + camPos;
            }
            else
            {
                cursorPos = player.mainBodyChunk.pos + new Vector2(lastDirection * 50f, 0f);
            }
        }

        loadedSpear = null;
    }
    public void Shoot(bool eu)
    {
        if (aimCharge >= shootThreshold && loadedSpear != null)
        {
            if (PlayerData.spearsShotByBows.ContainsKey(loadedSpear))
            { PlayerData.spearsShotByBows.Remove(loadedSpear); }
            PlayerData.spearsShotByBows.Add(loadedSpear, new PlayerData.SpearShotByBow(loadedSpear.room, loadedSpear, aimDirection.normalized));

            bool bonus = false;
            if (Wielder != null && Wielder is Player player && player.animation == Player.AnimationIndex.Flip)
            {
                bonus = true;

                loadedSpear.spearDamageBonus = 2f;

                for (int i = 0; i < Random.Range(5, 8); i++)
                {
                    room.AddObject(new Spark(loadedSpear.firstChunk.pos, Custom.rotateVectorDeg(-aimDirection, Random.Range(-20f, 20f)) * 40f, new Color(1f, 1f, 1f), null, 4, 8));
                }

                room.PlaySound(SoundID.Slugcat_Throw_Spear, firstChunk, false, 1f, 1.5f);
            }

            loadedSpear.Thrown(grabbedBy[0].grabber, firstChunk.pos, firstChunk.pos + aimDirection * 10f, new IntVector2(0, 0), bonus ? 1.5f : 1f, eu);
            loadedSpear.AllGraspsLetGoOfThisObject(true);
        }

        UnloadSpearFromBow();
    }
    public Vector2 GetAimPos(Creature grabber)
    {
        if (grabber is Player player)
        {
            if (basePackage.Value.x != 0)
            { lastDirection = basePackage.Value.x; }

            if (Plugin.Options.aimBowControls.Value == "Mouse")
            {
                return new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + camPos;
            }
            else if (Plugin.Options.aimBowControls.Value == "Directional Inputs")
            {
                return cursorPos.Value;
            }
        }
        else if (grabber is Scavenger scav && scav.AI.preyTracker.MostAttractivePrey != null)
        {
            Tracker.CreatureRepresentation rep = scav.AI.preyTracker.MostAttractivePrey;
            if (rep.representedCreature.realizedCreature != null && rep.VisualContact)
            {
                return rep.representedCreature.realizedCreature.mainBodyChunk.pos;
            }
            else
            {
                return scav.room.MiddleOfTile(rep.BestGuessForPosition());
            }
        }
        return Vector2.zero;
    }
    #endregion

    #region Appearance
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[3];

        sLeaser.sprites[0] = new FSprite("Bow", true);
        sLeaser.sprites[1] = new FSprite("pixel", true);
        sLeaser.sprites[2] = new FSprite("pixel", true);

        AddToContainer(sLeaser, rCam, null);
    }
    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        newContainer ??= rCam.ReturnFContainer(lastLayer);

        foreach (FSprite fsprite in sLeaser.sprites)
        {
            fsprite.RemoveFromContainer();
            newContainer.AddChild(fsprite);
        }
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
        else
        {
            this.camPos = camPos;

            Vector2 handlePos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 rotVec = Vector3.Slerp(lastRotation, rotation, timeStacker);
            float aimCharge = Mathf.Clamp(Mathf.Lerp(lastAimCharge, this.aimCharge, timeStacker) / shootThreshold, 0f, 1f);

            float bowWidth = 10f + aimCharge * 2f;
            float bowLength = 60f - aimCharge * 4f;

            Vector2 handleEnd1Pos = handlePos + Custom.rotateVectorDeg(rotVec, 90) * (bowLength / 2 - 2f) - rotVec * (bowWidth - 3f);
            Vector2 handleEnd2Pos = handlePos + Custom.rotateVectorDeg(rotVec, -90) * (bowLength / 2 - 2f) - rotVec * (bowWidth - 3f);
            Vector2 bowMiddlePos = handlePos - rotVec * (bowWidth / 2 - 2f);
            Vector2 baseStringPos = Vector2.Lerp(handleEnd1Pos, handleEnd2Pos, 0.5f);
            Vector2 stringPos = baseStringPos - rotVec * aimCharge * 20f;

            if (aiming && Wielder != null && loadedSpear != null)
            {
                ChangeOverlap(true);
                loadedSpear.ChangeOverlap(true);

                if (Wielder is Player player)
                {
                    aimDirection = Custom.DirVec(player.mainBodyChunk.pos, GetAimPos(player));

                    PlayerGraphics graphics = player.graphicsModule as PlayerGraphics;
                    SlugcatHand arrowHand = graphics.hands[loadedSpear.grabbedBy[0].graspUsed];
                    SlugcatHand bowHand = graphics.hands[grabbedBy[0].graspUsed];

                    if (rotVec.x < 0)
                    { arrowHand.pos = stringPos + Custom.PerpendicularVector(rotVec) * 2f; }
                    else { arrowHand.pos = stringPos - Custom.PerpendicularVector(rotVec) * 2f; }

                    arrowHand.reachingForObject = true;
                    arrowHand.absoluteHuntPos = stringPos;
                    arrowHand.huntSpeed = 100f;
                    arrowHand.quickness = 1f;
                    arrowHand.pushOutOfTerrain = false;

                    bowHand.reachingForObject = true;
                    bowHand.absoluteHuntPos = player.mainBodyChunk.pos + aimDirection * 100f;
                    bowHand.huntSpeed = 100f;
                    bowHand.quickness = 1f;
                    bowHand.pushOutOfTerrain = false;
                }
                else if (Wielder is Scavenger scav)
                {
                    Vector2 aimPos = scav.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
                    Vector2 scavPos = scav.mainBodyChunk.pos;
                    float distance = Custom.Dist(new Vector2(aimPos.x, 0), new Vector2(scavPos.x, 0));
                    aimDirection = Custom.DirVec(scav.mainBodyChunk.pos, new Vector2(aimPos.x, aimPos.y + distance * 0.05f));

                    ScavengerGraphics graphics = scav.graphicsModule as ScavengerGraphics;
                    ScavengerGraphics.ScavengerHand bowHand = graphics.hands[0];
                    ScavengerGraphics.ScavengerHand arrowHand = graphics.hands[1];

                    arrowHand.pos = stringPos;
                    arrowHand.absoluteHuntPos = stringPos;
                    arrowHand.huntSpeed = 100f;
                    arrowHand.quickness = 1f;
                    arrowHand.pushOutOfTerrain = false;
                    arrowHand.mode = Limb.Mode.HuntAbsolutePosition;

                    bowHand.absoluteHuntPos = scav.mainBodyChunk.pos + aimDirection * 100f;
                    bowHand.huntSpeed = 100f;
                    bowHand.quickness = 1f;
                    bowHand.pushOutOfTerrain = false;
                    bowHand.mode = Limb.Mode.HuntAbsolutePosition;
                }
            }

            sLeaser.sprites[0].x = bowMiddlePos.x - camPos.x;
            sLeaser.sprites[0].y = bowMiddlePos.y - camPos.y;
            sLeaser.sprites[0].rotation = Custom.VecToDeg(Custom.rotateVectorDeg(rotVec, -90));
            sLeaser.sprites[0].scale = 50f;
            sLeaser.sprites[0].height = bowLength;
            sLeaser.sprites[0].width = bowWidth;

            sLeaser.sprites[1].SetPosition(Vector2.Lerp(handleEnd1Pos, stringPos, 0.5f) - camPos);
            sLeaser.sprites[1].height = Custom.Dist(stringPos, handleEnd1Pos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(Custom.DirVec(handleEnd1Pos, stringPos));

            sLeaser.sprites[2].SetPosition(Vector2.Lerp(handleEnd2Pos, stringPos, 0.5f) - camPos);
            sLeaser.sprites[2].height = Custom.Dist(stringPos, handleEnd2Pos);
            sLeaser.sprites[2].rotation = Custom.VecToDeg(Custom.DirVec(handleEnd2Pos, stringPos));

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (blink > 0 && Random.value < 0.5f)
                { sLeaser.sprites[i].color = blinkColor; }
                else
                { sLeaser.sprites[i].color = blackColor; }
            }
        }
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        blackColor = palette.blackColor;
    }
    #endregion
}
