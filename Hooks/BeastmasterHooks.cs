using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class BeastmasterHooks
{
    public static bool oneShot = true;
    public static bool beastMasterMenuOpen;
    public static FContainer beastMasterContainer;
    
    internal static void BeastMaster_DrawSprites(Action<BeastMaster.BeastMaster, Vector2> orig,
        BeastMaster.BeastMaster self, Vector2 centerPoint)
    {
        orig(self, centerPoint);

        if (self.isMenuOpen)
        {
            beastMasterMenuOpen = true;
            beastMasterContainer = self.displayContainer;
        }
        else
        {
            beastMasterMenuOpen = false;
            beastMasterContainer = null;
        }
    }
    internal static void BeastMaster_OnRainWorldUpdate(Action<BeastMaster.BeastMaster, On.RainWorld.orig_Update, RainWorld> orig,
        BeastMaster.BeastMaster self, On.RainWorld.orig_Update orig2, RainWorld self2)
    {
        orig(self, orig2, self2);
        if (beastMasterContainer != null && Cursors.container != null)
        {
            Cursors.container.MoveToFront();
        }
    }
}