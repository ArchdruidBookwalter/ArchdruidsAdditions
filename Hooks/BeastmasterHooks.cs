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
    internal static void BeastMaster_OnRainWorldUpdate(Action<BeastMaster.BeastMaster, On.RainWorld.orig_Update, RainWorld> orig,
        BeastMaster.BeastMaster self, On.RainWorld.orig_Update orig2, RainWorld self2)
    {
        orig(self, orig2, self2);

        /*
        if (self.isMenuOpen)
        { beastMasterMenuOpen = true; }
        else { beastMasterMenuOpen = false; }

        if (self.displayContainer != null && Cursors.container != null)
        { Cursors.container.MoveToFront(); }*/
    }
}