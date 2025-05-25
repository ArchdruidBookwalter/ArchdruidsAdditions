using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class GameHooks
{
    internal static void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
    {
        orig(self, dt);
        if (self.devToolsActive)
        {
            if (self.framesPerSecond == 10)
            {
                self.framesPerSecond = 1;
            }
        }
    }
}
