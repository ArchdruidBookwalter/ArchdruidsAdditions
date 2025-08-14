using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Steamworks;
using ArchdruidsAdditions.Objects;

namespace ArchdruidsAdditions.Hooks;

public static class WeaponHooks
{
    internal static void Weapon_Thrown(On.Weapon.orig_Thrown orig, Weapon self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 thrownDir, float frc, bool eu)
    {
        Trackers.ThrowTracker tracker = Methods.Methods.GetTracker(self, self.room) as Trackers.ThrowTracker;
        if (tracker != null)
        {
            self.Shoot(tracker.thrownBy, tracker.startPos, tracker.shootDir, tracker.force, tracker.eu);
            return;
        }
        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, thrownDir, frc, eu); 
    }
    internal static void Weapon_HitWall(On.Weapon.orig_HitWall orig, Weapon self)
    {
        orig(self);
    }
    internal static void Weapon_Update(On.Weapon.orig_Update orig, Weapon self, bool eu)
    {
        orig(self, eu);

        Trackers.ThrowTracker tracker = Methods.Methods.GetTracker(self, self.room) as Trackers.ThrowTracker;
        if (tracker != null && tracker.weapon == self)
        {
            if (self is Spear spear)
            {
                if (spear.mode != Weapon.Mode.Thrown || spear is null || spear.room is null)
                {
                    Debug.Log("");
                    Debug.Log("Tracker was Destroyed. Reason:");
                    Debug.Log(spear.mode);
                    if (spear is null) { Debug.Log("Spear was Null"); }
                    else if (spear.room is null) { Debug.Log("Room was Null"); }
                    Debug.Log("");
                    tracker.Destroy();
                }
                else
                {
                    if (tracker.firstTick)
                    {
                        spear.firstChunk.vel = tracker.shootDir * 60f;
                        tracker.firstTick = false;
                    }

                    spear.rotation = spear.firstChunk.vel.normalized;
                    spear.alwaysStickInWalls = true;
                    spear.doNotTumbleAtLowSpeed = true;

                    float rot; 
                    float thr; 
                    IntVector2 contactPoint = spear.firstChunk.contactPoint;
                    if (contactPoint.x != 0 || contactPoint.y != 0)
                    {
                        spear.firstChunk.vel = tracker.lastVel;
                        spear.rotation = spear.firstChunk.vel.normalized;
                        rot = Custom.VecToDeg(spear.rotation);
                        thr = Custom.VecToDeg(spear.throwDir.ToVector2());
                        if (spear.firstChunk.vel.magnitude > 30 && spear.throwDir == spear.firstChunk.ContactPoint && Mathf.Abs(Mathf.Abs(rot) - Mathf.Abs(thr)) < 10)
                        {
                            //Debug.Log("Spear Stuck!");
                        }
                        else if (tracker.tickCount > 0)
                        {
                            spear.alwaysStickInWalls = false;
                            if (contactPoint.y != 0 && Math.Abs(spear.firstChunk.vel.normalized.y) < 0.1f)
                            { spear.ChangeMode(Weapon.Mode.Free); }
                            else
                            { spear.HitWall(); }
                            //Debug.Log("Spear Bounced!");
                        }
                    }
                    else
                    {
                        tracker.lastVel = spear.firstChunk.vel;
                        if (Mathf.Abs(spear.rotation.x) > Mathf.Abs(spear.rotation.y))
                        { spear.throwDir = new IntVector2(Math.Sign(spear.rotation.x), 0); }
                        else
                        { spear.throwDir = new IntVector2(0, Math.Sign(spear.rotation.y)); }
                    }
                    if (tracker.tickCount < 5)
                    { tracker.tickCount++; }
                }
            }
        }

        if (self is Spear)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[i])
                {
                    if (obj is not null && obj is Bow bow && bow.loadedSpear is not null && bow.loadedSpear == self)
                    {

                        Vector2 stringPos = bow.firstChunk.pos - bow.rotation * (bow.bowWidth / 2) - bow.rotation * bow.aimCharge;
                        Vector2 lastStringPos = bow.firstChunk.lastPos - bow.lastRotation * (bow.bowWidth / 2) - bow.lastRotation * bow.aimCharge;
                        Vector2 spearPos = stringPos + bow.rotation * 22f;
                        Vector2 lastSpearPos = lastStringPos + bow.lastRotation * 22f;
                        try
                        {
                            self.rotation = bow.rotation;
                            self.lastRotation = bow.lastRotation;
                        }
                        catch (Exception e) { Debug.LogError("Loaded Spear DrawSprites experienced an Error!"); Debug.LogException(e); }
                    }
                }
            }
        }
    }
}
