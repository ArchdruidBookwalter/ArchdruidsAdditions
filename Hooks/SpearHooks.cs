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

        if (self is not null && self.room is not null)
        {
            Trackers.ThrowTracker tracker = Methods.Methods.GetTracker(self, self.room) as Trackers.ThrowTracker;
            if (tracker != null)
            {
                //self.room.AddObject(new ColoredShapes.Rectangle(self.room, Vector2.Lerp(self.firstChunk.lastPos, self.firstChunk.pos, timeStacker), 1f, 1f, 45f, new(0f, 0f, 1f), 200));
                if (self.firstChunk.vel.magnitude > 1000f)
                {
                    self.firstChunk.vel = self.firstChunk.vel.normalized * 50f;
                    self.rotation = self.firstChunk.vel.normalized;
                }
                //Debug.Log("Spear Velocity: " + self.firstChunk.vel.magnitude);
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
            //Debug.Log("WARNING: StuckInWall was NULL!");
        }

        /*
        switch (self.mode.value)
        {
            case "Carried":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "Carried", new(0f, 1f, 0f), 0));
                break;
            case "Free":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "Free", new(0f, 1f, 0f), 0));
                break;
            case "Frozen":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "Frozen", new(0f, 0f, 1f), 0));
                break;
            case "OnBack":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "OnBack", new(0f, 1f, 0f), 0));
                break;
            case "StuckInCreature":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "StuckInCreature", new(1f, 1f, 0f), 0));
                break;
            case "StuckInWall":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "StuckInWall", new(1f, 1f, 0f), 0));
                break;
            case "Thrown":
                self.room.AddObject(new ColoredShapes.Text(self.room, self.firstChunk.pos, "Thrown", new(1f, 0f, 0f), 0));
                break;
        }*/

        orig(self, eu);
    }
    internal static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj != null)
        {
            //self.room.AddObject(new ColoredShapes.Rectangle(self.room, result.collisionPoint, 2f, 2f, 45f, new(1f, 1f, 0f), 200));
            //Debug.Log("");
            //Debug.Log("Spear Hit Something?");
            //Debug.Log("Object: " + result.chunk.owner.ToString());
            //Debug.Log("Velocity: " + self.firstChunk.vel.magnitude);
            //Debug.Log("");
        }
        return orig(self, result, eu);
    }
}
