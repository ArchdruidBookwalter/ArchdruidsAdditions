using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ArchdruidsAdditions.Objects;
using UnityEngine;
using RWCustom;
using HarmonyLib;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using EffExt;
using On;

namespace ArchdruidsAdditions.Hooks;

public static class SpearHooks
{
    internal static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self is not null && self.room is not null && !self.slatedForDeletetion)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[i])
                {
                    if (obj is not null && obj is Bow bow && bow.loadedSpear is not null && bow.loadedSpear == self)
                    {
                        Vector2 stringPos = bow.firstChunk.pos - bow.rotation * (bow.bowWidth / 2) - bow.rotation * bow.aimCharge;
                        Vector2 lastStringPos = bow.firstChunk.lastPos - bow.lastRotation * (bow.bowWidth / 2) - bow.lastRotation * bow.aimCharge;
                        Vector2 spearPos = stringPos + bow.rotation * 21f;
                        Vector2 lastSpearPos = lastStringPos + bow.lastRotation * 21f;
                        try
                        {
                            self.rotation = bow.rotation;
                            self.lastRotation = bow.lastRotation;
                            self.firstChunk.pos = spearPos;
                            self.firstChunk.lastPos = lastSpearPos;
                        }
                        catch (Exception e) { Debug.LogError("Loaded Spear DrawSprites experienced an Error!"); Debug.LogException(e); }
                    }
                }
            }
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.grabbedBy.Count > 0)
        {
            foreach (Creature.Grasp grasp in self.grabbedBy[0].grabber.grasps)
            {
                if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow)
                {
                    if (bow.loadedSpear != null && bow.loadedSpear == self)
                    {
                        self.ChangeOverlap(true);
                    }
                }
            }
        }
    }
    internal static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        if (self.addPoles && self.stuckInWall is null)
        {
            self.addPoles = false;
            self.HitWall();
            self.firstChunk.vel = -self.firstChunk.vel;
            Debug.Log("WARNING: StuckInWall was NULL!");
        }
        if (self.firstChunk.ContactPoint.y != 0 && self.mode == Weapon.Mode.Thrown)
        {
            self.ChangeMode(Weapon.Mode.Free);
        }
        orig(self, eu);
    }
    internal static Vector2 ElectricSpear_ZapperAttachPos(On.MoreSlugcats.ElectricSpear.orig_ZapperAttachPos orig, MoreSlugcats.ElectricSpear self, float timeStacker, int node)
    {
        if (self.mode == Weapon.Mode.StuckInCreature)
        {
            Vector2 rotation = Vector3.Slerp(self.lastRotation, self.rotation, timeStacker);
            return orig(self, timeStacker, node) - rotation * 20f;
        }
        return orig(self, timeStacker, node);
    }
}
