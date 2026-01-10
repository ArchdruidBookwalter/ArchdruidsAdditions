using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace ArchdruidsAdditions.Hooks;

public static class OverWorldHooks
{
    internal static void OverWorld_ctor(On.OverWorld.orig_ctor orig, OverWorld self, RainWorldGame game)
    {
        if (!game.IsArenaSession)
        {
            Debug.Log("ARCHDRUIDS ADDITIONS GOT REGION DATA");
            Plugin.RegionData = new(game, game.TimelinePoint);
        }

        orig(self, game);
    }
}
