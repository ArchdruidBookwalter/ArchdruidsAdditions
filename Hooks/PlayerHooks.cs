using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ArchdruidsAdditions.Creatures;
using ArchdruidsAdditions.Objects;
using IL.Smoke;
using RWCustom;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    #region Player
    internal static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (obj is ScarletFlowerBulb)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Potato)
        {
            if ((obj as Potato).buried)
            {
                if (self.grasps[0] != null && self.grasps[1] != null)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.Drag;
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is ParrySword)
        {
            return Player.ObjectGrabability.BigOneHand;
        }
        if (obj is Bow)
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
        if (obj is ParrySword)
        {
            return true;
        }
        if (obj is Bow)
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
        Potato closestPotato = null;
        float dist;
        float oldDist = float.MaxValue;
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (self.room.physicalObjects[i][j] is Potato thisPotato && thisPotato.buried)
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
        if (obj is Potato potato && potato.buried)
        {
            self.LoseAllGrasps();
            self.Grab(obj, graspUsed, 1, Creature.Grasp.Shareability.CanNotShare, 0.5f, true, false);
        }
        else if (obj is Herring)
        {
            self.Grab(obj, graspUsed, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, false);
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
    static int clearConsoleCooldown = 0;
    internal static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (clearConsoleCooldown > 0)
        { clearConsoleCooldown--; }

        if (self.room != null && self.room.game.devToolsActive)
        {
            if (self.input[0].spec && self.input[0].jmp && clearConsoleCooldown == 0)
            {
                if (File.Exists("consoleLog.txt"))
                {
                    File.Delete("consoleLog.txt");
                }
                clearConsoleCooldown = 100;
                self.room.AddObject(new ColoredShapes.Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, 40f), "Cleared Console!", "Red", 50));
            }
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
            return Vector3.Slerp(bowVec1, bowVec2, Math.Abs((self.graphicsModule as PlayerGraphics).spearDir));
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
    internal static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabbedCreature)
    {
        return orig(self, grabbedCreature);
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
            if (grasp is not null && grasp.grabbed is Potato potato && potato.playerSquint)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }
        }
    }
    #endregion

    #region SlugcatHand
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        orig(self);

        Player player = self.owner.owner as Player;
        Creature.Grasp grasp = player.grasps[self.limbNumber];

        if (grasp != null && grasp.grabbed is Herring herring && !herring.dead)
        {
            self.huntSpeed = Random.value * 5f;
            self.quickness = Random.value * 0.3f;
            self.vel += Custom.RNV() * 1f;
            self.pos += Custom.RNV() * 1f;
            (player.graphicsModule as PlayerGraphics).NudgeDrawPosition(0, Custom.DirVec(player.mainBodyChunk.pos, self.pos) * 1f * Random.value);
            (player.graphicsModule as PlayerGraphics).head.vel += Custom.DirVec(player.mainBodyChunk.pos, self.pos) * 1f * Random.value;
        }
    }
    #endregion
}
