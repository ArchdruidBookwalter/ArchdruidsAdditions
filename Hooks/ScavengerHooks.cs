using System;
using Unity.Mathematics;
using ArchdruidsAdditions.Objects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using static ArchdruidsAdditions.Enums.ScavengerBehavior;
using System.Collections.Generic;
using ArchdruidsAdditions.Data;

namespace ArchdruidsAdditions.Hooks;

public static class ScavengerHooks
{
    #region Scavenger
    internal static void Scavenger_ctor(On.Scavenger.orig_ctor orig, Scavenger self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);

        if (!PlayerData.scavData.ContainsKey(self))
        {
            PlayerData.scavData.Add(self, new PlayerData.ScavData(self));
        }
    }
    internal static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        orig(self, eu);

        if (PlayerData.scavData.ContainsKey(self))
        {
            Bow bow = PlayerData.scavData[self].GetBow();
            if (bow != null && bow.aiming)
            {
                if (self.animation == null || self.animation is not ScavengerAimBowAnimation)
                {
                    self.animation = new ScavengerAimBowAnimation(self, bow, bow.GetAimPos(self));
                }
            }
            else
            {
                if (self.animation is ScavengerAimBowAnimation)
                {
                    self.animation = null;
                }
            }
        }

        //Create_Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, -50f), self.animation, "Blue", 0);
        //Create_Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, -40f), self.AI.behavior, "Yellow", 0);
    }
    internal static void Scavenger_Act(On.Scavenger.orig_Act orig, Scavenger self)
    {
        orig(self);

        /*
        Vector2 pos = self.mainBodyChunk.pos;

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
        }*/
    }
    internal static void Scavenger_TryThrow(On.Scavenger.orig_TryThrow_BodyChunk_ViolenceType orig, Scavenger self, BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType)
    {
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow)
        {
            return;
        }
        orig(self, aimChunk, violenceType);
    }
    internal static void Scavenger_TryToMeleeCreature(On.Scavenger.orig_TryToMeleeCreature orig, Scavenger self)
    {
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow)
        {
            return;
        }
        orig(self);
    }
    internal static bool Scavenger_ArrangeInventory(On.Scavenger.orig_ArrangeInventory orig, Scavenger self)
    {
        if (PlayerData.scavData.ContainsKey(self))
        {
            Bow bow = PlayerData.scavData[self].GetBow();
            if (bow != null)
            {
                self.SwitchGrasps(bow.grabbedBy[0].graspUsed, 0);
                self.PlaceAllGrabbedObjectsInCorrectContainers();
            }
        }
        return orig(self);
    }
    internal static bool Scavenger_WantToLethallyAttack(On.Scavenger.orig_WantToLethallyAttack orig, Scavenger self, Tracker.CreatureRepresentation rep)
    {
        bool origValue = orig(self, rep);

        if (self.AI.behavior == AttackWithBow && rep == self.AI.preyTracker.MostAttractivePrey && self.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal)
        {
            return true;
        }

        return origValue;
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
    public static Bow CheckScavInventoryForBow(Scavenger scav)
    {
        foreach (Creature.Grasp grasp in scav.grasps)
        {
            if (grasp != null && grasp.grabbed is Bow bow)
            {
                return bow;
            }
        }
        return null;
    }
    #endregion

    #region ScavengerAI
    internal static void ScavengerAI_Update(On.ScavengerAI.orig_Update orig, ScavengerAI self)
    {
        orig(self);
    }
    internal static int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInstead, bool wantsSpear = false)
    {
        int baseWeaponScore = orig(self, obj, pickupDropInstead, wantsSpear);

        if (obj is Bow)
        {
            baseWeaponScore = 8;
        }

        return baseWeaponScore;
    }
    internal static int ScavengerAI_CollectScore(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        int baseCollectScore = orig(self, obj, weaponFiltered);

        //Create_Square(obj.room, obj.firstChunk.pos, 10f, 10f, Vec(45), "Purple", 0);
        //Create_Text(obj.room, obj.firstChunk.pos + new Vector2(0f, 15f), baseCollectScore, "Purple", 0);

        if (obj is ScarletFlowerBulb)
        {
            return 3;
        }
        if (obj is Bow)
        {
            return 5;
        }
        else if (obj is Potato || obj is LightningFruit || obj is AshPepper)
        {
            return 2;
        }
        else if (obj is ParrySword)
        {
            return 10;
        }
        else
        {
            return baseCollectScore;
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
        float baseItemScore = orig(self, rep);

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

        return baseItemScore;
    }
    internal static PathCost ScavengerAI_TravelPreference(On.ScavengerAI.orig_TravelPreference orig, ScavengerAI self, MovementConnection coord, PathCost cost)
    {
        return orig(self, coord, cost);
    }
    internal static void ScavengerAI_AttackBehavior(On.ScavengerAI.orig_AttackBehavior orig, ScavengerAI self)
    {
        float section = 0;

        try
        {
            orig(self);

            section = 1;

            if (PlayerData.scavData.ContainsKey(self.scavenger))
            {
                Bow bow = PlayerData.scavData[self.scavenger].GetBow();
                if (bow != null)
                {
                    Spear spear = PlayerData.scavData[self.scavenger].GetSpear();
                    if (spear == null)
                    {
                        self.RetrieveWeapon();
                    }
                    else
                    {
                        if (self.scavenger.movMode != Scavenger.MovementMode.Climb)
                        {
                            if (self.preyTracker.MostAttractivePrey != null && self.preyTracker.MostAttractivePrey.VisualContact)
                            {
                                bow.LoadSpearIntoBow(spear);

                                Debug.Log("LOADED SPEAR INTO BOW");

                                //Create_Square(self.scavenger.room, self.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, 20f, 20f, Vec(45), "Purple", 0);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log_Exception(e, "SCAVENGERAI_ATTACKBEHAVIOR", section);
        }
    }
    internal static void ScavengerAI_DecideBehavior(On.ScavengerAI.orig_DecideBehavior orig, ScavengerAI self)
    {
        orig(self);
    }
    internal static float ScavengerAI_CurrentPlayerAggression(On.ScavengerAI.orig_CurrentPlayerAggression orig, ScavengerAI self, AbstractCreature player)
    {
        float origValue = orig(self, player);

        return origValue;
    }
    internal static bool ScavengerAI_Get_HoldAWeapon(Func<ScavengerAI, bool> orig, ScavengerAI self)
    {
        bool origValue = orig(self);

        if (self.behavior == AttackWithBow)
        { return true; }

        return origValue;
    }
    internal static bool ScavengerAI_Get_NeedAWeapon(Func<ScavengerAI, bool> orig, ScavengerAI self)
    {
        bool origValue = orig(self);

        if (self.behavior == AttackWithBow)
        { return true; }

        return origValue;
    }
    internal static void ScavengerAI_MakeLookHere(On.ScavengerAI.orig_MakeLookHere orig, ScavengerAI self, Vector2 point)
    {
        if (self.behavior == AttackWithBow)
        {
            return;
        }

        orig(self, point);
    }
    internal static void ScavengerAI_SeeThrownWeapon(On.ScavengerAI.orig_SeeThrownWeapon orig, ScavengerAI self, PhysicalObject obj, Creature thrower)
    {
        orig(self, obj, thrower);
    }
    internal static float ScavengerAI_SpearThrowPositionScore(On.ScavengerAI.orig_SpearThrowPositionScore orig, ScavengerAI self, WorldCoordinate testPos, IntVector2 targetPos, ref List<IntVector2> creatureMovementArea)
    {
        float origValue = orig(self, testPos, targetPos, ref creatureMovementArea);

        if (origValue == float.MinValue)
        {
            return origValue;
        }

        if (PlayerData.scavData.ContainsKey(self.scavenger) && PlayerData.scavData[self.scavenger].GetBow() != null)
        {
            if (self.scavenger.room.GetTile(testPos).AnyBeam)
            {
                return float.MinValue;
            }

            //Vector2 pos = self.scavenger.room.MiddleOfTile(testPos);

            //Create_Square(self.scavenger.room, pos, 10f, 10f, Vec(45), "White", 0);
            //Create_Text(self.scavenger.room, pos + new Vector2(0f, 20f), origValue, "White", 0);
            //Create_Square(self.scavenger.room, self.scavenger.room.MiddleOfTile(targetPos), 10f, 10f, Vec(45), "Purple", 0);

            float score = 0f;

            foreach (IntVector2 myPos in self.myMovementArea)
            {
                if (self.scavenger.room.VisualContact(myPos, targetPos) && Custom.DistLess(myPos, targetPos, 400f))
                {
                    foreach (IntVector2 crPos in creatureMovementArea)
                    {
                        if (Mathf.Abs(myPos.x - crPos.x) > 2)
                        {
                            score++;
                        }
                    }
                }
            }

            if (score == 0f)
            { score++; }
            else
            { score += 100f; }

            score *= Custom.LerpMap(testPos.Tile.FloatDist(targetPos), 60f, 90f, 1f, 0f);
            score *= Custom.LerpMap(testPos.Tile.FloatDist(targetPos), 20f, 0f, 1f, 0f);
            score *= 1f - self.threatTracker.ThreatOfArea(testPos, true);
            if (self.scavenger.occupyTile == testPos.Tile)
            {
                score *= 0.1f;
            }

            return score;
        }
        else
        {
            return origValue;
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
        {
            wantToUseBow = false;
        }
        else if (self.world != null && self.world.region != null && self.world.region.name != null)
        {
            string thisRegionHasBows = Plugin.RegionData.ReadRegionData(self.world.region.name, "ScavsHaveBows");
            if ((thisRegionHasBows is null || thisRegionHasBows.Contains("false")) && !Plugin.Options.spawnBowsEverywhere.Value)
            {
                wantToUseBow = false;
            }
        }

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
        AbstractPhysicalObject baseTradeItem = orig(self, main);

        string thisRegionHasBows = Plugin.RegionData.ReadRegionData(self.world.region.name, "ScavsHaveBows");
        if ((thisRegionHasBows is not null && thisRegionHasBows.Contains("true")) || Plugin.Options.spawnBowsEverywhere.Value)
        {
            if (!main && Random.value < 0.2f)
            {
                return new AbstractConsumable(self.world, Enums.AbstractObjectType.Bow, null, self.parent.pos, self.world.game.GetNewID(), -1, -1, null);
            }
        }

        return baseTradeItem;
    }
    #endregion

    #region ScavengerGraphics
    internal static int ScavengerGraphics_ContainerForHeldItem(On.ScavengerGraphics.orig_ContainerForHeldItem orig, ScavengerGraphics self, PhysicalObject obj, int grasp)
    {
        int baseContainer = orig(self, obj, grasp);

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

        return baseContainer;
    }
    internal static float2 ScavengerGraphics_Get_ItemPosition(Func<ScavengerGraphics, int, float2> orig, ScavengerGraphics self, int grasp)
    {
        float2 basePos = orig(self, grasp);

        if (grasp > 0 && self.scavenger.grasps[grasp].grabbed is Bow)
        {
            return self.OnBackSurfacePos(new float2(-self.flip * 0.9f, 0.4f + (grasp - 1) * 0.1f), 1f);
        }

        return basePos;
    }
    internal static float2 ScavengerGraphics_Get_ItemDirection(Func<ScavengerGraphics, int, float2> orig, ScavengerGraphics self, int grasp)
    {
        float2 baseDir = orig(self, grasp);

        if (grasp > 0 && self.scavenger.grasps[grasp].grabbed is Bow)
        {
            return Custom.RotateAroundOrigo(Custom.DegToFloat2(135f - self.flip * (grasp - 1) * 10f + Custom.LerpMap(self.flip, -1f, 1f, -30f, 0f)),
                Custom.VecToDeg(math.lerp(self.bodyAxis, Custom.DirVec(self.drawPositions[self.hipsDrawPos, 1], self.drawPositions[self.chestDrawPos, 1]), 0.75f)));
        }

        return baseDir;
    }
    internal static void ScavengerHand_DrawSprites(Action<ScavengerGraphics.ScavengerHand, RoomCamera.SpriteLeaser, RoomCamera, float, float2> orig,
        ScavengerGraphics.ScavengerHand self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
    {
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
                            //Debug.Log("Naturally Spawned Bow!");
                        }
                    }
                }
            }
        }
    }
    #endregion
}
