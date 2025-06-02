using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Steamworks;

namespace ArchdruidsAdditions.Hooks;

public static class WeaponHooks
{
    internal static void Weapon_Thrown(On.Weapon.orig_Thrown orig, Weapon self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 thrownDir, float frc, bool eu)
    {
        foreach (UpdatableAndDeletable obj in self.room.updateList)
        {
            if (obj is Trackers.ThrowTracker tracker && tracker.weapon == self)
            {
                //Debug.Log("Tracker Detected");
                self.Shoot(tracker.thrownBy, tracker.startPos, tracker.shootDir, tracker.force, tracker.eu);
                return;
            }
        }
        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, thrownDir, frc, eu); 
    }
    internal static void Weapon_HitWall(On.Weapon.orig_HitWall orig, Weapon self)
    {
        orig(self);
        if (self is Spear spear)
        {
            foreach (UpdatableAndDeletable obj in self.room.updateList)
            {
                if (obj is Trackers.ThrowTracker tracker && tracker.weapon == self)
                {
                    //Debug.Log("Spear Hit Wall!");
                }
            }
        }
    }
}
