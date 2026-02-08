using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class MainHooks
{
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

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
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.LightningFruit))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.LightningFruit);
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
        if (!Futile.atlasManager.DoesContainAtlas("CloudFish"))
        {
            Futile.atlasManager.LoadAtlas("atlases/CloudFish");
        }
        if (!Futile.atlasManager.DoesContainAtlas("SphericalFruit"))
        {
            Futile.atlasManager.LoadAtlas("atlases/SphericalFruit");
        }
        if (!Futile.atlasManager.DoesContainAtlas("FirePepper"))
        {
            Futile.atlasManager.LoadAtlas("atlases/FirePepper");
        }
        if (!Futile.atlasManager.DoesContainAtlas("Arc"))
        {
            Futile.atlasManager.LoadAtlas("atlases/Arc");
        }
        #endregion

        MachineConnector.SetRegisteredOI(Plugin.PLUGIN_GUID, Plugin.Options);

        Debug.Log("ARCHDRUID'S ADDITIONS LOADED METHOD: ON_MODS_INIT");

        Enums.AAEnums.RegisterAllEnums();

        try
        { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.ScarletFlower, "Archdruid's Additions"); }
        catch
        { Debug.Log("ScarletFlower has already been placed in correct Devtools category."); }
        try
        { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.Potato, "Archdruid's Additions"); }
        catch
        { Debug.Log("Potato has already been placed in correct Devtools category."); }
        try
        { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.LightningFruit, "Archdruid's Additions"); }
        catch
        { Debug.Log("LightningFruit has already been placed in correct Devtools category."); }
        try
        { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.DecoLightningVine, "Archdruid's Additions"); }
        catch
        { Debug.Log("LightningVine has already been placed in correct Devtools category."); }
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
        if (Futile.atlasManager.DoesContainAtlas("CloudFish"))
        {
            Futile.atlasManager.UnloadAtlas("CloudFish");
        }
        if (Futile.atlasManager.DoesContainAtlas("SphericalFruit"))
        {
            Futile.atlasManager.UnloadAtlas("SphericalFruit");
        }
        if (Futile.atlasManager.DoesContainAtlas("FirePepper"))
        {
            Futile.atlasManager.UnloadAtlas("FirePepper");
        }
        if (Futile.atlasManager.DoesContainAtlas("Arc"))
        {
            Futile.atlasManager.UnloadAtlas("Arc");
        }
    }
    internal static void RainWorld_OnModsEnabled(On.RainWorld.orig_OnModsEnabled orig, RainWorld self, ModManager.Mod[] newlyEnabledMods)
    {
        orig(self, newlyEnabledMods);

        Debug.Log("ARCHDRUID'S ADDITIONS LOADED METHOD: ON_MODS_ENABLED");

        foreach (var mod in newlyEnabledMods)
        {
            if (mod.id == "archdruidbookwalter.archdruidsadditions")
            {
                Enums.AAEnums.RegisterAllEnums();

                try
                { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.ScarletFlower, "Archdruid's Additions"); }
                catch
                { Debug.Log("ScarletFlower has already been placed in correct Devtools category."); }
                try
                { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.Potato, "Archdruid's Additions"); }
                catch
                { Debug.Log("Potato has already been placed in correct Devtools category."); }
                try
                { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.LightningFruit, "Archdruid's Additions"); }
                catch
                { Debug.Log("LightningFruit has already been placed in correct Devtools category."); }
                try
                { Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.DecoLightningVine, "Archdruid's Additions"); }
                catch
                { Debug.Log("LightningVine has already been placed in correct Devtools category."); }

                break;
            }
        }
    }
    internal static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        Debug.Log("ARCHDRUID'S ADDITIONS LOADED METHOD: ON_MODS_DISABLED");

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
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.LightningFruit))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.LightningFruit);
                }
                Enums.AAEnums.UnregisterAllEnums();
                break;
            }
        }
    }
    internal static void RainWorld_PostModsInIt(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        Debug.Log("ARCHDRUID'S ADDITIONS TRIED TO LOAD METHOD: POST_MODS_INIT");

        orig(self);

        Debug.Log("ARCHDRUID'S ADDITIONS SUCCESSFULLY LOADED METHOD: POST_MODS_INIT");

        foreach (var mod in ModManager.ActiveMods)
        {
            if (mod.id == "fyre.BeastMaster")
            {
                BeastmasterHooks.CreateBeastmasterHooks();
                beastMasterActive = true;
            }
            if (mod.id == "maxi-mol.mousedrag")
            {
                mouseDragActive = true;
            }
        }
    }
}
