using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class BeastmasterHooks
{
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    public static bool Enabled
    {
        get
        {
            foreach (var mod in ModManager.ActiveMods)
            {
                if (mod.id == "fyre.BeastMaster")
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static bool createdHooks = false;
    public static void CreateBeastmasterHooks()
    {
        if (createdHooks || !Enabled)
        {
            return;
        }
        else
        {
            try { new Hook(typeof(BeastMaster.BeastMaster).GetMethod("RainWorldOnUpdate", ALL_FLAGS), BeastmasterHooks.BeastMaster_OnRainWorldUpdate); }
            catch (Exception ex)
            {
                Debug.Log("");
                Debug.Log("Couldn't find Beastmaster Update Method?");
                Debug.Log("");
                Debug.LogException(ex);
            }
            createdHooks = true;
        }
    }

    public static bool beastMasterMenuOpen;
    internal static void BeastMaster_OnRainWorldUpdate(Action<BeastMaster.BeastMaster, On.RainWorld.orig_Update, RainWorld> orig,
        BeastMaster.BeastMaster self, On.RainWorld.orig_Update orig2, RainWorld self2)
    {
        orig(self, orig2, self2);

        if (self.isMenuOpen)
        { beastMasterMenuOpen = true; }
        else { beastMasterMenuOpen = false; }

        if (self.displayContainer != null && Cursors.container != null)
        { Cursors.container.MoveToFront(); }
    }
}