using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Creatures;
using ArchdruidsAdditions.Objects;
using IL.Smoke;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    #region Player
    internal static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (obj is Objects.ScarletFlowerBulb)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Objects.Potato)
        {
            if ((obj as Objects.Potato).buried)
            {
                if (self.grasps[0] != null && self.grasps[1] != null)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.Drag;
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is Objects.ParrySword)
        {
            return Player.ObjectGrabability.BigOneHand;
        }
        if (obj is Objects.Bow)
        {
            foreach (Creature.Grasp grasp in self.grasps)
            {
                if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is Herring)
        {
            return Player.ObjectGrabability.OneHand;
        }
        return orig(self, obj);
    }
    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        if (obj is Objects.ParrySword)
        {
            return true;
        }
        if (obj is Objects.Bow)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static PhysicalObject Player_PickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorSpears)
    {
        if (favorSpears > 50)
        {
            return orig(self, favorSpears);
        }
        Objects.Potato closestPotato = null;
        float dist;
        float oldDist = float.MaxValue;
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (self.room.physicalObjects[i][j] is Objects.Potato thisPotato && thisPotato.buried)
                {
                    dist = Custom.Dist(thisPotato.bodyChunks[1].pos, self.bodyChunks[0].pos);
                    if (dist < 20f && dist < oldDist)
                    {
                        closestPotato = thisPotato;
                        oldDist = dist;
                    }
                }
            }
        }
        if (closestPotato != null && self.CanIPickThisUp(closestPotato))
        {
            return closestPotato;
        }
        return orig(self, favorSpears);
    }
    internal static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
        if (obj is Objects.Potato potato && potato.buried)
        {
            self.LoseAllGrasps();
            self.Grab(obj, graspUsed, 1, Creature.Grasp.Shareability.CanNotShare, 0.5f, true, false);
        }
        else
        {
            orig(self, obj, graspUsed);
        }
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp].grabbed is ParrySword sword)
        {
            sword.Use();
            return;
        }
        else if (self.grasps[grasp].grabbed is Bow bow)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null &&
                self.grasps[otherGrasp].grabbed is Spear spear &&
                self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangFromBeam)
            {
                bow.LoadSpearIntoBow(spear);
            }
            return;
        }
        else if (self.grasps[grasp].grabbed is Spear spear2)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null &&
                self.grasps[otherGrasp].grabbed is Bow bow2)
            {
                if (self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangFromBeam)
                {
                    bow2.LoadSpearIntoBow(spear2);
                }
                return;
            }
        }
        orig(self, grasp, eu);
    }
    internal static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.room != null && self.room.game.devToolsActive)
        {
            //Methods.Methods.CreateDebugText(self.mainBodyChunk, self.room.ElectricPower.ToString(), "Red");
        }
    }
    internal static Vector2 Player_GetHeldItemDirection(On.Player.orig_GetHeldItemDirection orig, Player self, int hand)
    {
        Vector2 spearVec1 = Custom.DirVec(self.mainBodyChunk.pos, self.grasps[hand].grabbed.bodyChunks[0].pos) * ((hand == 0) ? (-1f) : 1f);
        if (self.animation != Player.AnimationIndex.HangFromBeam)
        { spearVec1 = Custom.PerpendicularVector(spearVec1); }
        Vector2 spearVec2 = Custom.DegToVec((80f + Mathf.Cos((float)(self.animationFrame + (self.leftFoot ? 9 : 3)) / 12f * 2f * 3.1415927f) * 4f *
            (self.graphicsModule as PlayerGraphics).spearDir) * (self.graphicsModule as PlayerGraphics).spearDir);

        if (self.grasps[hand].grabbed is Bow bow)
        {
            if (self.bodyMode == Player.BodyModeIndex.Crawl)
            {
                Vector2 crawlVec = Custom.PerpendicularVector(orig(self, hand));
                if (crawlVec.y > 0)
                {
                    crawlVec = Custom.rotateVectorDeg(crawlVec, 180);
                }
                return crawlVec;
            }
            else if (self.animation == Player.AnimationIndex.ClimbOnBeam)
            { return Custom.PerpendicularVector(orig(self, hand)); }

            Vector2 bowVec2 = Custom.DirVec(self.bodyChunks[0].pos, self.bodyChunks[1].pos);
            Vector2 bowVec1 = Custom.rotateVectorDeg(bowVec2, (60 + Math.Abs(3 * self.bodyChunks[0].vel.y)) * ((hand == 0) ? 1f : -1f));
            if (bow.getRotation)
            {
                //self.room.AddObject(new ColoredShapes.Rectangle(self.room, self.grasps[hand].grabbed.firstChunk.pos + bowVec1 * 20f, 0.2f, 40f, Custom.VecToDeg(bowVec1), new(1f, 0f, 0f), 0));
                //self.room.AddObject(new ColoredShapes.Rectangle(self.room, self.grasps[hand].grabbed.firstChunk.pos + bowVec2 * 20f, 0.2f, 40f, Custom.VecToDeg(bowVec2), new(0f, 1f, 0f), 0));
            }
            return Vector3.Slerp(bowVec1, bowVec2, Math.Abs((self.graphicsModule as PlayerGraphics).spearDir));
        }
        else if (self.grasps[hand].grabbed is Spear spear)
        {
            //self.room.AddObject(new ColoredShapes.Rectangle(self.room, spear.firstChunk.pos + spearVec1 * 20f, 0.2f, 40f, Custom.VecToDeg(spearVec1), new(1f, 0f, 0f), 0));
            //self.room.AddObject(new ColoredShapes.Rectangle(self.room, spear.firstChunk.pos + spearVec2 * 20f, 0.2f, 40f, Custom.VecToDeg(spearVec2), new(0f, 1f, 0f), 0));
        }
        return orig(self, hand);
    }
    internal static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        bool aimingBow = false;

        foreach (Creature.Grasp grasp in self.grasps)
        {
            if (grasp is not null && grasp.grabbed is not null && grasp.grabbed is Bow bow && bow.aiming)
            {
                aimingBow = true;
            }
        }

        Player.InputPackage package = self.input[0];

        if (aimingBow)
        {
            self.input[0].x = 0; self.input[0].analogueDir.x = 0;
            if (self.bodyMode == Player.BodyModeIndex.Crawl)
            {
                if (self.input[0].y < 0 || self.input[0].analogueDir.y < 0)
                { self.input[0].y = 0; self.input[0].analogueDir.y = 0; }
            }
            else
            {
                self.input[0].y = 0; self.input[0].analogueDir.y = 0;
            }
        }

        orig(self, eu);

        self.input[0] = package;
    }
    public static Color GetColor(int index)
    {
        if (index == 0)
        {
            return new Color(1f, 0f, 0f);
        }
        else if (index == 1)
        {
            return new Color(1f, 0.5f, 0f);
        }
        else if (index == 2)
        {
            return new Color(1f, 1f, 0f);
        }
        else if (index == 3)
        {
            return new Color(0.5f, 1f, 0f);
        }
        else if (index == 4)
        {
            return new Color(0f, 1f, 0f);
        }
        else if (index == 5)
        {
            return new Color(0f, 1f, 0.5f);
        }
        else if (index == 6)
        {
            return new Color(0f, 1f, 1f);
        }
        else if (index == 7)
        {
            return new Color(0f, 0.5f, 1f);
        }
        else if (index == 8)
        {
            return new Color(0f, 0f, 1f);
        }
        else
        {
            int newIndex = index - 9;
            return GetColor(newIndex);
        }
    }
    #endregion

    #region PlayerGraphics
    internal static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Player.InputPackage package = self.player.input[0];
        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow && bow.aiming)
            {
                self.player.input[0].x = 0; self.player.input[0].analogueDir.x = 0;
                if (self.player.input[0].y < 0 || self.player.input[0].analogueDir.y < 0)
                { self.player.input[0].y = 0; self.player.input[0].analogueDir.y = 0; }
            }
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        self.player.input[0] = package;

        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp is not null && grasp.grabbed is Objects.Potato potato && potato.playerSquint)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }
        }
    }
    #endregion
}
