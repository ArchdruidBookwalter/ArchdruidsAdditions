using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

public static class SpearHooks
{
    /*internal static void Spear_Thrown(On.Spear.orig_Thrown orig, Spear self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 thrownDir, float frc, bool eu)
    {
        Debug.Log("Spear Was Thrown!");
        foreach (UpdatableAndDeletable obj in self.room.updateList)
        {
            if (obj is Trackers.ThrowTracker tracker && tracker.weapon == self)
            {
            }
        }
        orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, thrownDir, frc, eu);
    }
    internal static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj != null)
        {
            self.room.AddObject(new ColoredShapes.Rectangle(self.room, self.firstChunk.pos, 10f, 10f, 45f, new(1f, 0f, 0f), 100));
            Debug.Log(result.obj.ToString());
        }
        return orig(self, result, eu);
    }*/
    internal static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self != null && !self.slatedForDeletetion)
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[i])
                {
                    if (obj is Bow bow && bow.loadedSpear is not null && bow.loadedSpear == self)
                    {
                        self.rotation = bow.rotation;
                        self.firstChunk.pos = bow.stringPos + bow.rotation * 25f;
                    }
                }
            }
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
    }
}
