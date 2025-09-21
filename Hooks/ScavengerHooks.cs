using System;
using ArchdruidsAdditions.Objects;
using Unity.Mathematics;
using UnityEngine;
using RWCustom;

using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace ArchdruidsAdditions.Hooks;

public static class ScavengerHooks
{
    #region Scavenger
    internal static void Scavenger_Act(On.Scavenger.orig_Act orig, Scavenger self)
    {
        orig(self);

        int scavAimChargeThreshold = 50;
        CreatureTemplate.Type type = self.abstractCreature.creatureTemplate.type;
        if (ModManager.DLCShared && (type == DLCSharedEnums.CreatureTemplateType.ScavengerElite || self.King))
        { scavAimChargeThreshold = 30; }

        bool wantToAimBow = false;
        Bow bow = CheckScavengerInventory_Obj(self, typeof(Bow), false) as Bow;
        if (self.room != null && bow is not null && self.AI.focusCreature != null && self.AI.WantToThrowSpearAtCreature(self.AI.focusCreature) > 0f)
        {
            self.lookPoint = self.room.MiddleOfTile(self.AI.focusCreature.BestGuessForPosition());

            if (bow.scavWithinRange && Custom.ManhattanDistance(self.AI.pathFinder.destination, self.abstractCreature.pos) > 5)
            {
                bow.scavWithinRange = false;
            }
            else if (Custom.ManhattanDistance(self.AI.pathFinder.destination, self.abstractCreature.pos) < 2)
            {
                bow.scavWithinRange = true;
            }

            Spear spear = CheckScavengerInventory_Obj(self, typeof(Spear), true) as Spear;
            if (spear is null)
            {
                self.AI.RetrieveWeapon();
            }
            else if (self.AI.focusCreature.VisualContact)
            {
                if (self.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    self.lookPoint = self.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
                }
                if (bow.grabbedBy[0].graspUsed == 0 &&
                    self.AI.IsThrowPathClearFromFriends(self.lookPoint, 50f) &&
                    self.movMode != Scavenger.MovementMode.Climb &&
                    bow.scavWithinRange)
                {
                    self.animation = new ScavengerAimBowAnimation(self, bow, self.lookPoint);
                    wantToAimBow = true;
                    if (bow.loadedSpear == null)
                    {
                        bow.LoadSpearIntoBow(spear);
                        self.PlaceAllGrabbedObjectsInCorrectContainers();
                    }
                    if (self.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
                    {
                        bow.scavAimCharge++;
                        if (bow.scavAimCharge > scavAimChargeThreshold)
                        {
                            wantToAimBow = false;
                        }
                    }
                    self.mainBodyChunk.vel.y += 1.1f;
                    self.mainBodyChunk.vel.x *= 0.5f;
                }
            }
        }
        
        if (!wantToAimBow && self.animation is ScavengerAimBowAnimation)
        {
            self.animation = null;
            if (bow is not null)
            {
                if (bow.scavAimCharge < scavAimChargeThreshold)
                {
                    bow.UnloadSpearFromBow();
                    self.PlaceAllGrabbedObjectsInCorrectContainers();
                }
                bow.scavAimCharge = 0;
            }
        }
    }
    internal static void Scavenger_TryThrow(On.Scavenger.orig_TryThrow_BodyChunk_ViolenceType orig, Scavenger self, BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType)
    {
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow)
        {
            return;
        }
        orig(self, aimChunk, violenceType);
    }
    internal static void Scavenger_TryToMeleeCreature(On.Scavenger.orig_TryToMeleeCreature orig, Scavenger self)
    {
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow)
        {
            return;
        }
        orig(self);
    }
    internal static bool Scavenger_ArrangeInventory(On.Scavenger.orig_ArrangeInventory orig, Scavenger self)
    {
        if (self.animation is ScavengerAimBowAnimation aimAnim)
        {
            self.SwitchGrasps(aimAnim.bow.grabbedBy[0].graspUsed, 0);
            self.PlaceAllGrabbedObjectsInCorrectContainers();
            return false;
        }
        return orig(self);
    }
    public static PhysicalObject CheckScavengerInventory_Obj(Scavenger scav, Type searchType, bool includeAllSpearTypes)
    {
        foreach (Creature.Grasp grasp in scav.grasps)
        {
            if (grasp != null && grasp.grabbed != null)
            {
                if (grasp.grabbed.GetType() == searchType)
                {
                    return grasp.grabbed;
                }
                if (includeAllSpearTypes && searchType == typeof(Spear))
                {
                    if (grasp.grabbed.GetType().BaseType == typeof(Spear))
                    {
                        return grasp.grabbed;
                    }
                }
            }
        }
        return null;
    }
    public static bool CheckScavengerInventory_Bool(Scavenger scav, Type searchType, bool includeAllSpearTypes)
    {
        foreach (Creature.Grasp grasp in scav.grasps)
        {
            if (grasp != null && grasp.grabbed != null)
            {
                if (grasp.grabbed.GetType() == searchType)
                {
                    return true;
                }
                if (includeAllSpearTypes && searchType == typeof(Spear))
                {
                    if (grasp.grabbed.GetType().BaseType == typeof(Spear))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion


    #region ScavengerAI
    internal static int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInstead, bool wantsSpear = false)
    {
        if (obj is Bow)
        {
            return 8;
        }
        return orig(self, obj, pickupDropInstead, wantsSpear);
    }
    internal static int ScavengerAI_CollectScore(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        if (obj is Objects.ScarletFlowerBulb)
        {
            return 3;
        }
        if (obj is Objects.Bow)
        {
            return 5;
        }
        else if (obj is Objects.Potato)
        {
            return 2;
        }
        else if (obj is Objects.ParrySword)
        {
            return 10;
        }
        else
        {
            return orig(self, obj, weaponFiltered);
        }
    }
    internal static void ScavengerAI_CheckForScavengeItems(On.ScavengerAI.orig_CheckForScavangeItems orig, ScavengerAI self, bool conservativeBias)
    {
        orig(self, conservativeBias);
        if (self.scavengeCandidate != null)
        {
            if (self.scavengeCandidate.representedItem.type != AbstractPhysicalObject.AbstractObjectType.Spear)
            {
                foreach (Creature.Grasp grasp in self.scavenger.grasps)
                {
                    if (grasp != null && grasp.grabbed is Bow)
                    {
                        self.scavengeCandidate = null;
                    }
                }
            }
        }
    }
    internal static float ScavengerAI_PickUpItemScore(On.ScavengerAI.orig_PickUpItemScore orig, ScavengerAI self, ItemTracker.ItemRepresentation rep)
    {
        if (rep.representedItem.type == Enums.AbstractObjectType.Bow)
        {
            foreach (Creature.Grasp grasp in self.scavenger.grasps)
            {
                if (grasp != null && grasp.grabbed is Bow)
                {
                    return -10000;
                }
            }
        }
        return orig(self, rep);
    }
    internal static PathCost ScavengerAI_TravelPreference(On.ScavengerAI.orig_TravelPreference orig, ScavengerAI self, MovementConnection coord, PathCost cost)
    {
        foreach (Creature.Grasp grasp in self.scavenger.grasps)
        {
            if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow && bow.aiming)
            {
                cost.legality = PathCost.Legality.Unallowed;
                return cost;
            }
        }
        return orig(self, coord, cost);
    }
    internal static void ScavengerAI_AttackBehavior(On.ScavengerAI.orig_AttackBehavior orig, ScavengerAI self)
    {
        Bow bow = CheckScavengerInventory_Obj(self.scavenger, typeof(Bow), false) as Bow;
        if (bow != null && self.preyTracker.MostAttractivePrey != null)
        {
            Spear spear = CheckScavengerInventory_Obj(self.scavenger, typeof(Spear), true) as Spear;
            if (spear is null)
            {
                self.RetrieveWeapon();
            }
            else
            {
                self.focusCreature = self.preyTracker.MostAttractivePrey;

                Room room = self.scavenger.room;
                WorldCoordinate preyPos = self.focusCreature.lastSeenCoord;
                WorldCoordinate scavPos = self.creature.pos;

                if (Random.value < 0.5f)
                {
                    IntVector2 newStandPos;
                    if (Random.value < 0.5f)
                    { newStandPos = new(scavPos.x + Random.Range(-20, 20), scavPos.y); }
                    else
                    { newStandPos = new(preyPos.x + Random.Range(-20, 20), preyPos.y); }
                    int sign = Random.value < 0.5f ? -1 : 1;
                    for (int i = 0; i < 40; i++)
                    {
                        //Methods.Methods.CreateDebugSquareAtPos(self.scavenger.room.MiddleOfTile(newStandPos), self.scavenger.room, "White", 10f, 0);
                        if (room.GetTile(newStandPos).Terrain == Room.Tile.TerrainType.Air &&
                            room.GetTile(new IntVector2(newStandPos.x, newStandPos.y - 1)).Solid &&
                            (room.GetTile(new IntVector2(newStandPos.x - 1, newStandPos.y - 1)).Solid ||
                            room.GetTile(new IntVector2(newStandPos.x + 1, newStandPos.y - 1)).Solid) &&
                            room.VisualContact(room.GetWorldCoordinate(newStandPos), preyPos) &&
                            Custom.ManhattanDistance(room.GetWorldCoordinate(newStandPos), preyPos) > 10f)
                        {
                            if (!bow.scavStandPos.HasValue || Custom.ManhattanDistance(bow.scavStandPos.Value, scavPos) > Custom.ManhattanDistance(room.GetWorldCoordinate(newStandPos), scavPos) || !room.VisualContact(bow.scavStandPos.Value, preyPos))
                            {
                                bow.scavStandPos = room.GetWorldCoordinate(newStandPos);
                            }
                            break;
                        }
                        newStandPos.y += sign;
                    }
                }

                if (!bow.scavStandPos.HasValue)
                { bow.scavStandPos = scavPos; }

                self.creature.abstractAI.SetDestination(bow.scavStandPos.Value);

                //Methods.Methods.CreateDebugSquareAtPos(self.scavenger.room.MiddleOfTile(bow.scavStandPos.Value), self.scavenger.room, "Yellow", 10f, 0);
                //Methods.Methods.CreateDebugSquareAtPos(self.scavenger.room.MiddleOfTile(self.focusCreature.BestGuessForPosition()), self.scavenger.room, "Red");
            }
        }
        else
        {
            orig(self);
        }
    }
    #endregion

    #region ScavengerAbstractAI
    internal static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        orig(self);

        CreatureTemplate.Type type = self.parent.creatureTemplate.type;
        bool wantToUseBow = true;
        if (ModManager.Watcher && type == Watcher.WatcherEnums.CreatureTemplateType.ScavengerDisciple || type == Watcher.WatcherEnums.CreatureTemplateType.ScavengerTemplar)
        { wantToUseBow = false; }

        string thisRegionHasBows = Plugin.RegionData.ReadRegionData(self.world.region.name, "ScavsHaveBows");
        if ((thisRegionHasBows is null || thisRegionHasBows.Contains("false")) && !Plugin.Options.spawnBowsEverywhere.Value)
        { wantToUseBow = false; }

        if (wantToUseBow && Random.value < 0.2f)
        {
            AbstractPhysicalObject.CreatureGripStick zeroGrip = null;
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.parent.stuckObjects)
            {
                if (stick is AbstractPhysicalObject.CreatureGripStick grip && grip.grasp == 0)
                {
                    zeroGrip = grip;
                }
            }

            if (zeroGrip != null)
            { self.DropAndDestroy(zeroGrip); }

            AbstractPhysicalObject bow = new(self.world, Enums.AbstractObjectType.Bow, null, self.parent.pos, self.world.game.GetNewID());
            self.world.GetAbstractRoom(self.parent.pos).AddEntity(bow);
            new AbstractPhysicalObject.CreatureGripStick(self.parent, bow, 0, true);
        }
    }
    internal static void ScavengerAbstractAI_ReGearInDen(On.ScavengerAbstractAI.orig_ReGearInDen orig, ScavengerAbstractAI self)
    {
        orig(self);

        CreatureTemplate.Type type = self.parent.creatureTemplate.type;
        bool wantToUseBow = true;
        if (ModManager.Watcher && type == Watcher.WatcherEnums.CreatureTemplateType.ScavengerDisciple || type == Watcher.WatcherEnums.CreatureTemplateType.ScavengerTemplar)
        { wantToUseBow = false; }

        string thisRegionHasBows = Plugin.RegionData.ReadRegionData(self.world.region.name, "ScavsHaveBows");
        if ((thisRegionHasBows is null || thisRegionHasBows.Contains("false")) && !Plugin.Options.spawnBowsEverywhere.Value)
        { wantToUseBow = false; }

        if (wantToUseBow && Random.value < 0.2f)
        {
            AbstractPhysicalObject.CreatureGripStick zeroGrip = null;
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.parent.stuckObjects)
            {
                if (stick is AbstractPhysicalObject.CreatureGripStick grip && grip.grasp == 0)
                {
                    zeroGrip = grip;
                }
            }

            if (zeroGrip != null)
            { self.DropAndDestroy(zeroGrip); }

            AbstractPhysicalObject bow = new(self.world, Enums.AbstractObjectType.Bow, null, self.parent.pos, self.world.game.GetNewID());
            self.world.GetAbstractRoom(self.parent.pos).AddEntity(bow);
            new AbstractPhysicalObject.CreatureGripStick(self.parent, bow, 0, true);
        }
    }
    internal static void ScavengerabstractAI_UpdateMissionAppropriateGear(On.ScavengerAbstractAI.orig_UpdateMissionAppropriateGear orig, ScavengerAbstractAI self)
    {
        self.missionAppropriateGear = false;
        if (self.squad != null && self.squad.missionType != ScavengerAbstractAI.ScavengerSquad.MissionID.None)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.parent.stuckObjects)
            {
                if (stick is AbstractPhysicalObject.CreatureGripStick grip && grip.A.type == Enums.AbstractObjectType.Bow)
                {
                    self.missionAppropriateGear = true;
                    return;
                }
            }
        }
        orig(self);
    }
    internal static AbstractPhysicalObject ScavengerAbstractAI_TradeItem(On.ScavengerAbstractAI.orig_TradeItem orig, ScavengerAbstractAI self, bool main)
    {
        string thisRegionHasBows = Plugin.RegionData.ReadRegionData(self.world.region.name, "ScavsHaveBows");
        if ((thisRegionHasBows is not null && thisRegionHasBows.Contains("true")) || Plugin.Options.spawnBowsEverywhere.Value)
        {
            if (!main && Random.value < 0.2f)
            {
                return new AbstractConsumable(self.world, Enums.AbstractObjectType.Bow, null, self.parent.pos, self.world.game.GetNewID(), -1, -1, null);
            }
        }
        return orig(self, main);
    }
    #endregion

    #region ScavengerGraphics
    internal static int ScavengerGraphics_ContainerForHeldItem(On.ScavengerGraphics.orig_ContainerForHeldItem orig, ScavengerGraphics self, PhysicalObject obj, int grasp)
    {
        if (obj is Bow bow)
        {
            if (bow.aiming)
            { return 2; }
            return 0;
        }
        if (obj is Spear)
        {
            foreach (Creature.Grasp grasp2 in self.scavenger.grasps)
            {
                if (grasp2 != null && grasp2.grabbed != null && grasp2.grabbed is Bow bow2 && bow2.loadedSpear != null && bow2.loadedSpear == obj)
                {
                    return 2;
                }
            }
        }
        return orig(self, obj, grasp);
    }
    internal static float2 ScavengerGraphics_ItemPosition(Func<ScavengerGraphics, int, float2> orig, ScavengerGraphics self, int grasp)
    {
        if (grasp > 0 && self.scavenger.grasps[grasp].grabbed is Bow)
        {
            return self.OnBackSurfacePos(new float2(-self.flip * 0.9f, 0.4f + (grasp - 1) * 0.1f), 1f);
        }
        return orig(self, grasp);
    }
    internal static float2 ScavengerGraphics_ItemDirection(Func<ScavengerGraphics, int, float2> orig, ScavengerGraphics self, int grasp)
    {
        if (grasp > 0 && self.scavenger.grasps[grasp].grabbed is Bow)
        {
            return Custom.RotateAroundOrigo(Custom.DegToFloat2(135f - self.flip * (grasp - 1) * 10f + Custom.LerpMap(self.flip, -1f, 1f, -30f, 0f)),
                Custom.VecToDeg(math.lerp(self.bodyAxis, Custom.DirVec(self.drawPositions[self.hipsDrawPos, 1], self.drawPositions[self.chestDrawPos, 1]), 0.75f)));
        }
        return orig(self, grasp);
    }
    internal static void ScavengerHand_DrawSprites(Action<ScavengerGraphics.ScavengerHand, RoomCamera.SpriteLeaser, RoomCamera, float, float2> orig,
        ScavengerGraphics.ScavengerHand self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
    {
        if (self.scavenger.room != null)
        {
            //self.scavenger.room.AddObject(new ColoredShapes.SmallRectangle(self.scavenger.room, self.absoluteHuntPos, self.retract ? "Green" : "Red", 10));
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
    }
    #endregion

    #region Related Things
    internal static void ScavengerTreasury_ctor(On.ScavengerTreasury.orig_ctor orig, ScavengerTreasury self, Room room, PlacedObject pobj)
    {
        orig(self, room, pobj);

        string thisRegionHasBows = Plugin.RegionData.ReadRegionData(room.world.region.name, "ScavsHaveBows");
        if ((thisRegionHasBows is not null && thisRegionHasBows.Contains("true")) || Plugin.Options.spawnBowsEverywhere.Value)
        {
            if (room.abstractRoom.firstTimeRealized)
            {
                for (int i = 0; i < self.tiles.Count; i++)
                {
                    if (UnityEngine.Random.value < Mathf.InverseLerp(self.Rad, self.Rad / 5f, Vector2.Distance(room.MiddleOfTile(self.tiles[i]), pobj.pos)))
                    {
                        AbstractPhysicalObject newObj = null;
                        if (UnityEngine.Random.value < 0.2f)
                        {
                            newObj = new(room.world, Enums.AbstractObjectType.Bow, null, room.GetWorldCoordinate(self.tiles[i]), room.game.GetNewID());
                        }
                        if (newObj is not null)
                        {
                            self.property.Add(newObj);
                            room.abstractRoom.entities.Add(newObj);
                            Debug.Log("Naturally Spawned Bow!");
                        }
                    }
                }
            }
        }
    }
    #endregion
}
