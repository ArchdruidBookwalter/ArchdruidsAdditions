using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using Unity.Mathematics;
using UnityEngine;
using RWCustom;
using System.Reflection.Emit;

namespace ArchdruidsAdditions.Hooks;

public static class ScavengerHooks
{
    #region Scavenger
    internal static Vector2 Scavenger_get_HeadLookPoint(Func<Scavenger, Vector2> orig, Scavenger self)
    {
        if (self.animation != null && self.animation is ScavengerHoldBowAnimation)
        {
            return (self.animation as ScavengerHoldBowAnimation).lookPos;
        }
        else if (self.animation != null && self.animation is ScavengerAimBowAnimation)
        {
            return (self.animation as ScavengerAimBowAnimation).lookPos;
        }
        return orig(self);
    }
    internal static Vector2 Scavenger_get_EyesLookPoint(Func<Scavenger, Vector2> orig, Scavenger self)
    {
        if (self.animation != null && self.animation is ScavengerHoldBowAnimation)
        {
            return (self.animation as ScavengerHoldBowAnimation).lookPos;
        }
        else if (self.animation != null && self.animation is ScavengerAimBowAnimation)
        {
            return (self.animation as ScavengerAimBowAnimation).lookPos;
        }
        return orig(self);
    }
    internal static void Scavenger_Throw(On.Scavenger.orig_Throw orig, Scavenger self, Vector2 throwDir)
    {
        orig(self, throwDir);
    }
    internal static void Scavenger_TryThrow(On.Scavenger.orig_TryThrow_BodyChunk_ViolenceType orig, Scavenger self, BodyChunk aimChunk, ScavengerAI.ViolenceType violenceType)
    {
        orig(self, aimChunk, violenceType);
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow)
        {
            if (self.animation != null && self.animation is Scavenger.ThrowChargeAnimation throwAnim)
            {
                self.animation = new ScavengerHoldBowAnimation(self, bow, throwAnim.UseTarget);
            }
        }
    }
    internal static void Scavenger_Update(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        Tracker.CreatureRepresentation focusCreature = self.AI.focusCreature;
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow && focusCreature is not null)
        {
            Vector2 creaturePos = self.room.MiddleOfTile(focusCreature.lastSeenCoord);
            if (focusCreature.representedCreature.realizedCreature is not null && focusCreature.VisualContact)
            { creaturePos = focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos; }

            #region Set Animation
            if ((self.animation is null || self.animation is Scavenger.ThrowChargeAnimation || 
                self.animation is ScavengerHoldBowAnimation || self.animation is ScavengerAimBowAnimation) && 
                self.AI.WantToThrowSpearAtCreature(focusCreature) > 0f)
            {
                if (focusCreature.visualContact && self.AI.CheckHandsForSpear() && self.movMode != Scavenger.MovementMode.Climb &&
                    Custom.Dist(creaturePos, self.mainBodyChunk.pos) > 50f)
                { self.animation = new ScavengerAimBowAnimation(self, bow, creaturePos); }
                else
                { self.animation = new ScavengerHoldBowAnimation(self, bow, creaturePos); }
            }
            #endregion

            if (self.animation is ScavengerHoldBowAnimation holdAnim)
            {
            }

            if (self.animation is ScavengerAimBowAnimation aimAnim)
            {
                self.movMode = Scavenger.MovementMode.StandStill;

                if (Methods.Methods.CheckScavengerInventory(self, typeof(Spear), true) is Spear heldSpear && focusCreature.VisualContact)
                { bow.LoadSpearIntoBow(heldSpear); }

                bow.scavAimCharge++;
            }
            else
            { bow.scavAimCharge = 0; }
        }
        else
        {
            if (self.animation is ScavengerHoldBowAnimation || self.animation is ScavengerAimBowAnimation)
            {
                self.animation = null;
            }
            if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow2)
            {
                bow2.scavAimCharge = 0;
            }
        }

        foreach (Creature.Grasp grasp in self.grasps)
        {
            if (grasp != null && self.grasps.IndexOf(grasp) != 0 && grasp.grabbed is Bow bow3)
            {
                bow3.scavAimCharge = 0;
            }
        }

        /*
        if (self.animation != null)
        { self.room.AddObject(new ColoredShapes.Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, 50f), self.animation.id.ToString(), new(0f, 1f, 0f), 0)); }
        else { self.room.AddObject(new ColoredShapes.Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, 50f), "NULL", new(0f, 1f, 0f), 0)); }
        */

        orig(self, eu);
    }
    internal static void Scavenger_MidRangeUpdate(On.Scavenger.orig_MidRangeUpdate orig, Scavenger self)
    {
        orig(self);
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow)
        {
            if (self.animation != null && self.animation is Scavenger.ThrowChargeAnimation throwAnim)
            {
                self.animation = new ScavengerHoldBowAnimation(self, bow, throwAnim.UseTarget);
            }
        }
    }
    internal static void Scavenger_TryToMeleeCreature(On.Scavenger.orig_TryToMeleeCreature orig, Scavenger self)
    {
        orig(self);
        if (self.grasps[0] is not null && self.grasps[0].grabbed is Bow bow)
        {
            if (self.animation != null && self.animation is Scavenger.ThrowChargeAnimation throwAnim)
            {
                self.animation = new ScavengerHoldBowAnimation(self, bow, throwAnim.UseTarget);
            }
        }
    }
    internal static void Scavenger_PickUpAndPlaceInInventory(On.Scavenger.orig_PickUpAndPlaceInInventory orig, Scavenger self, PhysicalObject obj, bool bypass = false)
    {
        if (obj is Bow)
        {
            foreach (Creature.Grasp grasp in self.grasps)
            {
                if (grasp != null && grasp.grabbed is Bow)
                {
                    return;
                }
            }
        }
        orig(self, obj, bypass);
    }
    internal static void Scavenger_ReleaseGrasp(On.Scavenger.orig_ReleaseGrasp orig, Scavenger self, int grasp)
    {
        if (self.grasps[grasp] != null)
        {
            Debug.Log("");
            Debug.Log("Spear Released Grasp: " + grasp + ", " + self.grasps[grasp].grabbed.GetType());
            Debug.Log("");
        }
        orig(self, grasp);
    }
    #endregion

    #region ScavengerGraphics
    internal static int ScavengerGraphics_ContainerForHeldItem(On.ScavengerGraphics.orig_ContainerForHeldItem orig, ScavengerGraphics self, PhysicalObject obj, int grasp)
    {
        if (obj is Objects.Bow)
        { return 0; }
        return orig(self, obj, grasp);
    }
    internal static void ScavengerGraphics_DrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim)
        {
            if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
            {
                Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                aimAnim.bowHand.mode = Limb.Mode.HuntAbsolutePosition;
                aimAnim.bowHand.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                aimAnim.bowHand.grabPos = null;
            }
            aimAnim.bow.ChangeOverlap(true);
            aimAnim.bow.loadedSpear.ChangeOverlap(true);
        }
        orig(self, sLeaser, rCam, timeStacker, camPosV2);
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim2)
        {
            if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
            {
                Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                aimAnim2.bowHand.mode = Limb.Mode.HuntAbsolutePosition;
                aimAnim2.bowHand.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                aimAnim2.bowHand.grabPos = null;
            }
            aimAnim2.bow.ChangeOverlap(true);
            aimAnim2.bow.loadedSpear.ChangeOverlap(true);
        }
    }
    #endregion

    #region ScavengerHand
    internal static void ScavengerHand_Update(On.ScavengerGraphics.ScavengerHand.orig_Update orig, ScavengerGraphics.ScavengerHand self)
    {
        self.scavenger.room.AddObject(new ColoredShapes.Rectangle(self.scavenger.room, self.pos, 3f, 3f, 45f, new(1f, 0f, 0f), 1));
        self.scavenger.room.AddObject(new ColoredShapes.Rectangle(self.scavenger.room, self.lastPos, 3f, 3f, 45f, new(1f, 1f, 0f), 1));
        self.scavenger.room.AddObject(new ColoredShapes.Rectangle(self.scavenger.room, self.absoluteHuntPos, 3f, 3f, 45f, new(0f, 0f, 1f), 1));
        if (self.grabPos != null)
        { self.scavenger.room.AddObject(new ColoredShapes.Rectangle(self.scavenger.room, self.grabPos.Value, 3f, 3f, 45f, new(0f, 1f, 0f), 1)); }

        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim)
        {
            if (aimAnim.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim.bow.ChangeOverlap(true);
                aimAnim.bow.loadedSpear.ChangeOverlap(true);
            }
        }
        orig(self);
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim2)
        {
            if (aimAnim2.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim2.bow.ChangeOverlap(true);
                aimAnim2.bow.loadedSpear.ChangeOverlap(true);
            }
        }
    }
    internal static void ScavengerHand_DrawSprites(Action<ScavengerGraphics.ScavengerHand, RoomCamera.SpriteLeaser, RoomCamera, float, float2> orig, ScavengerGraphics.ScavengerHand self,
        RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, float2 camPos)
    {
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim)
        {
            if (aimAnim.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim.bow.ChangeOverlap(true);
                aimAnim.bow.loadedSpear.ChangeOverlap(true);
            }
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim2)
        {
            if (aimAnim2.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim2.bow.ChangeOverlap(true);
                aimAnim2.bow.loadedSpear.ChangeOverlap(true);
            }
        }
    }
    internal static void ScavengerHand_StandardLocomotionProcedure(On.ScavengerGraphics.ScavengerHand.orig_StandardLocomotionProcedure orig, ScavengerGraphics.ScavengerHand self)
    {
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim)
        {
            if (aimAnim.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim.bow.ChangeOverlap(true);
                aimAnim.bow.loadedSpear.ChangeOverlap(true);
            }
        }
        orig(self);
        if (self.scavenger.animation != null && self.scavenger.animation is ScavengerAimBowAnimation aimAnim2)
        {
            if (aimAnim2.bowHand == self)
            {
                if (self.scavenger.room != null && self.scavenger.AI.focusCreature != null && self.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(self.scavenger.mainBodyChunk.pos, self.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = self.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    self.grabPos = null;
                }
                aimAnim2.bow.ChangeOverlap(true);
                aimAnim2.bow.loadedSpear.ChangeOverlap(true);
            }
        }
    }
    #endregion

    #region ScavengerAI
    internal static void ScavengerAI_CheckThrow(On.ScavengerAI.orig_CheckThrow orig, ScavengerAI self)
    {
        orig(self);
    }
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
            //Debug.Log("Scavenger Wants " + self.scavengeCandidate.representedItem.type.ToString() + "?");
            if (self.scavengeCandidate.representedItem.type == Enums.AbstractObjectType.Bow)
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
    internal static void ScavengerAI_RetrieveWeapon(On.ScavengerAI.orig_RetrieveWeapon orig, ScavengerAI self)
    {
        orig(self);
        if (self.scavengeCandidate != null && self.scavenger.room != null)
        {
            self.scavenger.room.AddObject(new ColoredShapes.Rectangle(
                self.scavenger.room,
                self.scavenger.room.MiddleOfTile(self.scavengeCandidate.lastSeenCoord),
                10f, 10f, 45f, new(1f, 1f, 0f), 50
                ));
        }
    }
    internal static void ScavengerAI_AttackBehavior(On.ScavengerAI.orig_AttackBehavior orig, ScavengerAI self)
    {
        orig(self);
        foreach (Creature.Grasp grasp in self.scavenger.grasps)
        {
            if (grasp != null && grasp.grabbed is Bow)
            {
                if (!self.CheckHandsForSpear())
                {
                    self.RetrieveWeapon();
                }
                else if (Custom.Dist(self.scavenger.room.MiddleOfTile(self.focusCreature.BestGuessForPosition().Tile), self.scavenger.mainBodyChunk.pos) < 300f)
                {
                    if (self.behavior == ScavengerAI.Behavior.Attack)
                    { self.behavior = ScavengerAI.Behavior.Flee; }
                }
            }
        }
    }
    #endregion

    #region Related Things
    internal static void ScavengerTreasury_ctor(On.ScavengerTreasury.orig_ctor orig, ScavengerTreasury self, Room room, PlacedObject pobj)
    {
        orig(self, room, pobj);
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
    #endregion
}
