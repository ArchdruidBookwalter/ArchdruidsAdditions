using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using MonoMod.RuntimeDetour;
using UnityEngine;
using System.ComponentModel.Composition.Hosting;

namespace ArchdruidsAdditions.Hooks;

public static class MainHooks
{
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    static bool oneShot;
    public static bool beastMasterActive = false;
    public static bool mouseDragActive = false;
    internal static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        #region MultiplayerUnlocks
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.Bow))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.Bow);
        }
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ScarletFlowerBulb))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.ScarletFlowerBulb);
        }
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ParrySword))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.ParrySword);
        }
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.Potato))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.Potato);
        }
        #endregion

        #region Atlases
        if (!Futile.atlasManager.DoesContainAtlas("Bow"))
        {
            Futile.atlasManager.LoadAtlas("atlases/Bow");
        }
        if (!Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ScarletFlowerStem");
        }
        if (!Futile.atlasManager.DoesContainAtlas("ScarletFlowerBulb"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ScarletFlowerBulb");
        }
        if (!Futile.atlasManager.DoesContainAtlas("ParrySword"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ParrySword");
        }
        if (!Futile.atlasManager.DoesContainAtlas("Potato"))
        {
            Futile.atlasManager.LoadAtlas("atlases/Potato");
        }
        #endregion

        if (oneShot == false)
        {
            Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.ScarletFlower, "Archdruid's Additions");
            Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.Potato, "Archdruid's Additions");
            oneShot = true;
        }

        UnityEngine.Debug.Log("\"Archdruid's Additions\" HAS SUCCESSFULY HOOKED, \"OnModsInit\" METHOD. =============================================");
    }
    internal static void RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        if (Futile.atlasManager.DoesContainAtlas("Bow"))
        {
            Futile.atlasManager.UnloadAtlas("Bow");
        }
        if (Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
        {
            Futile.atlasManager.UnloadAtlas("ScarletFlowerStem");
        }
        if (Futile.atlasManager.DoesContainAtlas("ScarletFlowerBulb"))
        {
            Futile.atlasManager.UnloadAtlas("ScarletFlowerBulb");
        }
        if (Futile.atlasManager.DoesContainAtlas("ParrySword"))
        {
            Futile.atlasManager.UnloadAtlas("ParrySword");
        }
        if (Futile.atlasManager.DoesContainAtlas("Potato"))
        {
            Futile.atlasManager.UnloadAtlas("Potato");
        }
    }
    internal static void RainWorld_OnModsEnabled(On.RainWorld.orig_OnModsEnabled orig, RainWorld self, ModManager.Mod[] newlyEnabledMods)
    {
        orig(self, newlyEnabledMods); 
    }
    internal static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == "archdruidbookwalter.archdruidsadditions")
            {
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.Bow))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.Bow);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ScarletFlowerBulb))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.ScarletFlowerBulb);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ParrySword))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.ParrySword);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.Potato))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.Potato);
                }
                Enums.AbstractObjectType.UnregisterValues();
                Enums.MiscItemType.UnregisterValues();
                Enums.MultiplayerItemType.UnregisterValues();
                Enums.PlacedObjectType.UnregisterValues();
                Enums.SandboxUnlockID.UnregisterValues();
                break;
            }
            if (mod.id == "fyre.BeastMaster")
            {
                
            }
        }
    }
    internal static void RainWorld_PostModsInIt(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        Debug.Log("");
        foreach (var mod in ModManager.ActiveMods)
        {
            //Debug.Log(mod.id);
            if (mod.id == "fyre.BeastMaster")
            {
                if (Methods.Methods.BeastmasterDependency.modPresent)
                {
                    Methods.Methods.BeastmasterDependency.CreateHook("DrawSprites");
                    Methods.Methods.BeastmasterDependency.CreateHook("RainWorldOnUpdate");
                    beastMasterActive = true;
                }
            }
            if (mod.id == "maxi-mol.mousedrag")
            {
                mouseDragActive = true;
            }
        }
        Debug.Log("");
        Debug.Log("BeastMaster detected: " + Methods.Methods.BeastmasterDependency.modPresent);
        Debug.Log("");
    }
}
