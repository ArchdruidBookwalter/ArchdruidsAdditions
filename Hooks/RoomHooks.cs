using System;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Decoration;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using ArchdruidsAdditions.Objects.RoomEffects;

namespace ArchdruidsAdditions.Hooks;

public static class RoomHooks
{
    internal static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        //Methods.Methods.LogMethodStart("ROOM_LOADED");

        var firstTimeRealized = self.abstractRoom.firstTimeRealized;

        orig(self);

        if (self.game == null) return;

        var placedObjects = self.roomSettings.placedObjects;
        var session = self.game.session;

        foreach (var pObj in placedObjects)
        {
            if (pObj.active)
            {
                if (pObj.type == Enums.PlacedObjectType.ScarletFlower)
                {
                    ScarletFlowerData data = pObj.data as ScarletFlowerData;

                    ScarletFlower flower = new(pObj, data.rotation);
                    self.AddObject(flower);

                    if (firstTimeRealized && (session is not StoryGameSession || !(session as StoryGameSession).saveState.ItemConsumed(
                        self.world, false, self.abstractRoom.index, placedObjects.IndexOf(pObj))))
                    {
                        var abstractConsumable = new AbstractConsumable(self.world, Enums.AbstractObjectType.ScarletFlowerBulb, null, self.GetWorldCoordinate(pObj.pos),
                            self.game.GetNewID(), self.abstractRoom.index, placedObjects.IndexOf(pObj), data)
                        { isConsumed = false };

                        abstractConsumable.realizedObject = new ScarletFlowerBulb(abstractConsumable, self.world, true, data.rotation, new(1f, 0f, 0f));

                        self.abstractRoom.AddEntity(abstractConsumable);
                    }
                }
                else if (pObj.type == Enums.PlacedObjectType.Potato)
                {
                    PotatoData data = pObj.data as PotatoData;

                    if (firstTimeRealized && (session is not StoryGameSession || !(session as StoryGameSession).saveState.ItemConsumed(
                        self.world, false, self.abstractRoom.index, placedObjects.IndexOf(pObj))))
                    {
                        var abstractConsumable = new AbstractConsumable(self.world, Enums.AbstractObjectType.Potato, null, self.GetWorldCoordinate(pObj.pos),
                            self.game.GetNewID(), self.abstractRoom.index, placedObjects.IndexOf(pObj), data)
                        { isConsumed = false };

                        float hue = UnityEngine.Random.Range(data.minHue, data.maxHue);
                        float sat = UnityEngine.Random.Range(data.minSat, data.maxSat);
                        float val = UnityEngine.Random.Range(data.minVal, data.maxVal);

                        if (val < 0.05f)
                        {
                            val = 0.05f;
                        }

                        Color newColor = Color.HSVToRGB(hue, sat, val);

                        abstractConsumable.realizedObject = new Potato(abstractConsumable, true, data.rotation, newColor, data.naturalColors);

                        self.abstractRoom.AddEntity(abstractConsumable);
                    }
                }
                else if (pObj.type == Enums.PlacedObjectType.LightningFruit)
                {
                    LightningFruitData data = pObj.data as LightningFruitData;

                    if (firstTimeRealized && (session is not StoryGameSession ||
                        !(session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, placedObjects.IndexOf(pObj))))
                    {
                        var abstractConsumable = new AbstractConsumable(self.world, Enums.AbstractObjectType.LightningFruit, null, self.GetWorldCoordinate(pObj.pos),
                            self.game.GetNewID(), self.abstractRoom.index, placedObjects.IndexOf(pObj), data)
                        { isConsumed = false };

                        abstractConsumable.realizedObject = new LightningFruit(abstractConsumable, data.charge);

                        self.abstractRoom.AddEntity(abstractConsumable);
                    }
                }
                else if (pObj.type == Enums.PlacedObjectType.DecoLightningVine)
                {
                    DecoVineData data = pObj.data as DecoVineData;

                    float segmentLength = 20f;
                    float ropeLength = Custom.Dist(pObj.pos, pObj.pos + data.handlePos) * Mathf.Lerp(0.5f, 2f, Mathf.InverseLerp(0f, 10f, data.elasticity));

                    int segments = Mathf.RoundToInt(ropeLength / segmentLength);

                    LightningFruitVine vine = new(self, segments, segmentLength, pObj.pos, pObj.pos + data.handlePos, data.charge, Mathf.RoundToInt(pObj.pos.x * 100 + pObj.pos.y * 100));
                    self.AddObject(vine);
                }
                else if (pObj.type == Enums.PlacedObjectType.AshPepperBush)
                {
                    AshPepperBushData data = pObj.data as AshPepperBushData;
                    int pObjIndex = placedObjects.IndexOf(pObj);

                    int numOfPeppers = Random.Range(data.minPeppers, data.maxPeppers + 1);

                    AshPepperBush bush = new(pObj, self, numOfPeppers, Mathf.RoundToInt(pObj.pos.x * 100 + pObj.pos.y * 100));

                    if (firstTimeRealized && (session is not StoryGameSession ||
                        !(session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, pObjIndex)))
                    {
                        if (Random.value > 0.5)
                        {
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 0);
                        }

                        if (Random.value > 0.5)
                        {
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 1);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 4);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 2);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 3);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 0);
                        }
                        else
                        {
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 2);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 3);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 1);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 4);
                            bush.PopulateBranch(self, pObj, pObjIndex, data, 0);
                        }
                    }

                    self.AddObject(bush);
                }
                else if (pObj.type == Enums.PlacedObjectType.InfectedCorpse)
                {
                    if (firstTimeRealized)
                    {
                        InfectedCorpseData data = pObj.data as InfectedCorpseData;

                        if (Random.value < data.spawnChance && data.creatureIndex >= 1)
                        {
                            CreatureTemplate template = StaticWorld.creatureTemplates[data.creatureIndex];

                            if (template.type != CreatureTemplate.Type.StandardGroundCreature && template.type != CreatureTemplate.Type.LizardTemplate)
                            {
                                AbstractCreature newCreature = new(self.world, template, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID());
                                self.abstractRoom.AddEntity(newCreature);

                                int numOfEggs = Random.Range(Mathf.Max(1, (int)template.bodySize / 2), (int)template.bodySize);
                                for (int i = 0; i < numOfEggs; i++)
                                {
                                    AbstractPhysicalObject newEgg = new(self.world, Enums.AbstractObjectType.ParasiteEgg, null, self.GetWorldCoordinate(pObj.pos), self.game.GetNewID());
                                    self.abstractRoom.AddEntity(newEgg);

                                    new AbstractParasiteEggStick(newEgg, newCreature);
                                }
                            }
                        }
                    }
                }
                else if (pObj.type == Enums.PlacedObjectType.CrabShellCircle)
                {
                    if (firstTimeRealized)
                    {
                        CrabShellCircleData data = pObj.data as CrabShellCircleData;

                        int shells = Mathf.RoundToInt(data.handlePos.magnitude * data.density);

                        IntVector2 centerTile = self.GetTilePosition(pObj.pos);
                        int radius = Mathf.RoundToInt(data.handlePos.magnitude / 20f);

                        for (int i = -radius; i < radius; i++)
                        {
                            if (Random.value < data.density)
                            {
                                IntVector2 startTile = new(centerTile.x + i, centerTile.y + Random.Range(-radius, radius + 1));
                                for (int j = startTile.y; j > 0; j--)
                                {
                                    IntVector2 testTile = new(startTile.x, j);
                                    if (Custom.DistLess(pObj.pos, self.MiddleOfTile(testTile), data.handlePos.magnitude) && !self.GetTile(testTile).Solid && self.GetTile(testTile.x, testTile.y - 1).Solid)
                                    {
                                        AbstractPhysicalObject newShell = new(self.world, Enums.AbstractObjectType.CrabShell, null, self.GetWorldCoordinate(testTile), self.game.GetNewID());
                                        self.abstractRoom.AddEntity(newShell);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (pObj.type == Enums.PlacedObjectType.RopeObject)
                {
                    if (firstTimeRealized)
                    {
                        PlacedObject.ResizableObjectData data = pObj.data as PlacedObject.ResizableObjectData;

                        float segmentLength = 20f;
                        float ropeLength = Custom.Dist(pObj.pos, pObj.pos + data.handlePos);

                        int segments = Mathf.RoundToInt(ropeLength / segmentLength);

                        HangingRope rope = new(segments, segmentLength, pObj.pos, pObj.pos + data.handlePos);
                        self.AddObject(rope);

                        RopeObject.RopeSegment segment = rope.ropeSegments[rope.EndIndex];

                        DangleFruit.AbstractDangleFruit fruit = new(self.world, null, self.GetWorldCoordinate(segment.pos), self.world.game.GetNewID(), -1, -1, false, null);
                        self.abstractRoom.AddEntity(fruit);
                        fruit.RealizeInRoom();

                        segment.AttachObject(new RopeObject.RopeAttachedObject(segment, fruit.realizedObject, 0));
                    }
                }
            }
        }

        foreach (RoomSettings.RoomEffect effect in self.roomSettings.effects)
        {
            if (effect.type.value == "RandomShells")
            {
                RandomShells.ActuallySpawnEffect(self, effect, firstTimeRealized);
            }
        }
    }
    internal static float Room_Get_ElectricPower(Func<Room, float> orig, Room self)
    {
        float basePower = orig(self);

        foreach (RoomSettings.RoomEffect effect in self.roomSettings.effects)
        {
            if (effect.type.value == "ForceRoomEnergy")
            {
                foreach (UpdatableAndDeletable updel in self.updateList)
                {
                    if (updel is LightRodPowerEffect.LightRodPower power)
                    {
                        return power.currentPower;
                    }
                }
            }
        }

        return basePower;
    }
    internal static void RoomSettings_LoadPlacedObjects(On.RoomSettings.orig_LoadPlacedObjects_StringArray_Timeline orig, RoomSettings self, string[] s, SlugcatStats.Timeline timeline)
    {
        try
        {
            orig(self, s, timeline);
        }
        catch (Exception e)
        {
            Debug.Log("<Archduid's Additions> EXPERIENCED ONE OR MORE EXCEPTIONS WHILE LOADING PLACED OBJECTS IN ROOM: " + self.room.abstractRoom.name);
        }
    }
    internal static void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int pos)
    {
        orig(self, newRoom, pos);

        if (self.followAbstractCreature != null)
        {
            Data.PlayerData.AAPlayerState playerState = Data.PlayerData.GetPlayerState(self.followAbstractCreature.ID.number);
            if (playerState != null && playerState.parasiteIllnessEffect != null)
            {
                playerState.parasiteIllnessEffect.NewRoom(newRoom);
            }
        }
    }
}
