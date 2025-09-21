using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class OverWorldHooks
{
    internal static void OverWorld_ctor(On.OverWorld.orig_ctor orig, OverWorld self, RainWorldGame game)
    {
        Plugin.RegionData = new(game, game.TimelinePoint);

        orig(self, game);
    }
}
