using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArchdruidsAdditions.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class CreatureHooks
{
    internal static void TailSegment_ctor(On.TailSegment.orig_ctor orig, TailSegment self,
        GraphicsModule module, float radius, float connectionRadius, TailSegment connectedSegment, float surfaceFriction, float airFriction, float affectPrevious, bool pullInPreviousPos)
    {
        if (module is CloudFishGraphics cloudFishModule)
        {
            self.owner = cloudFishModule;
            self.rad = radius;
            self.connectionRad = connectionRadius;
            self.connectedSegment = connectedSegment;
            self.surfaceFric = surfaceFriction;
            self.airFriction = airFriction;
            self.affectPrevious = affectPrevious;
            self.pullInPreviousPosition = pullInPreviousPos;
            self.connectedPoint = null;
            self.Reset(cloudFishModule.cloudFish.firstChunk.pos);
        }
        else
        {
            orig(self, module, radius, connectionRadius, connectedSegment, surfaceFriction, airFriction, affectPrevious, pullInPreviousPos);
        }
    }

    internal static void CreatureState_LoadFromString(On.CreatureState.orig_LoadFromString orig, CreatureState self, string[] s)
    {
        orig(self, s);
        Debug.Log("------------------------");
        Debug.Log("CREATURE: " + self.creature.creatureTemplate.name);
        foreach (string st in s)
        {
            Debug.Log(st);
        }
        Debug.Log("------------------------");
    }
}
