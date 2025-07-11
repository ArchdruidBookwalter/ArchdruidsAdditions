using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class LimbHooks
{
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        orig(self);
    }
    internal static void Limb_FindGrip(On.Limb.orig_FindGrip orig, Limb self, Room room, Vector2 attachedPos, Vector2 searchFromPos,
        float maxRadius, Vector2 goalPos, int forbiddenXDirs, int forbiddenYDirs, bool behindWalls)
    {
        orig(self, room, attachedPos, searchFromPos, maxRadius, goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls);
        if (self is ScavengerGraphics.ScavengerHand scavHand && scavHand.scavenger.animation != null && scavHand.scavenger.animation is ScavengerAimBowAnimation aimAnim)
        {
            if (aimAnim.bowHand == scavHand)
            {
                if (scavHand.scavenger.room != null && scavHand.scavenger.AI.focusCreature != null && scavHand.scavenger.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    Vector2 aimDir = Custom.DirVec(scavHand.scavenger.mainBodyChunk.pos, scavHand.scavenger.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                    scavHand.mode = Limb.Mode.HuntAbsolutePosition;
                    scavHand.absoluteHuntPos = scavHand.scavenger.mainBodyChunk.pos + aimDir * 40f;
                    scavHand.grabPos = null;
                }
                aimAnim.bow.ChangeOverlap(true);
                aimAnim.bow.loadedSpear.ChangeOverlap(true);
            }
        }
    }
}
