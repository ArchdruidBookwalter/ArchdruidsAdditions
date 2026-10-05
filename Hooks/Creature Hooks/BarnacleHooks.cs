using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watcher;

namespace ArchdruidsAdditions.Hooks;

public static class BarnacleHooks
{
    internal static void Barnacle_Collide(On.Watcher.Barnacle.orig_Collide orig, Watcher.Barnacle self, PhysicalObject otherObj, int myChunk, int otherChunk)
    {
        if (otherObj is MimicCrab)
        {
            return;
        }

        orig(self, otherObj, myChunk, otherChunk);
    }
    internal static void BarnacleAI_SetGroupDiscomfortTick(On.Watcher.BarnacleAI.orig_SetGroupDiscomfortTick orig, BarnacleAI self, float tick)
    {
        if (tick == 0.0016666667f)
        {
            int creatures = 0;
            foreach (AbstractCreature creature in self.realizedCreature.room.abstractRoom.creatures)
            {
                if (creature.realizedCreature != null && Custom.DistLess(creature.realizedCreature.mainBodyChunk.pos, self.realizedCreature.mainBodyChunk.pos, 200) && creature.realizedCreature is not MimicCrab)
                { creatures++; }
            }

            if (creatures == 0)
            {
                return;
            }
        }

        orig(self, tick);
    }
}
