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
                        Vector2 spearPos = stringPos + bow.rotation * 22f;
                        Vector2 lastSpearPos = lastStringPos + bow.lastRotation * 22f;
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
    }
    internal static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        if (self.addPoles && self.stuckInWall is null)
        {
            self.stuckInWall = self.room.MiddleOfTile(self.firstChunk.pos);
            Debug.Log("WARNING: StuckInWall was NULL!");
        }

        foreach (UpdatableAndDeletable updel in self.room.updateList)
        {
            if (updel is Trackers.ThrowTracker tracker && tracker.weapon == self)
            {

            }
        }

        orig(self, eu);

        foreach (UpdatableAndDeletable updel in self.room.updateList)
        {
            if (updel is Trackers.ThrowTracker tracker && tracker.weapon == self)
            {
            }
        }
    }
}
