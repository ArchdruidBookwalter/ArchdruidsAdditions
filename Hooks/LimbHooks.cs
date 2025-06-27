using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class LimbHooks
{
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        orig(self);
        /*
        if (self.mode == Limb.Mode.HuntAbsolutePosition || self.mode == Limb.Mode.HuntRelativePosition)
        {
            foreach (UpdatableAndDeletable obj in self.owner.owner.room.updateList)
            {
                if (obj is Trackers.LimbTracker tracker && tracker.limb == self)
                {
                    self.mode = Limb.Mode.HuntAbsolutePosition;
                    self.absoluteHuntPos = tracker.huntPos;
                    self.pos = self.absoluteHuntPos;
                    self.vel = Vector2.zero;
                    break;
                }
            }
        }*/
    }
}
