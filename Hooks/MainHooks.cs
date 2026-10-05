using System;
using System.IO;
using System.Reflection;

namespace ArchdruidsAdditions.Hooks;

public static class MainHooks
{
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    public static bool beastMasterActive = false;
    public static bool mouseDragActive = false;
    internal static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (!Plugin.loggedStrings)
        {
            foreach (string logString in Plugin.logStrings)
            {
                Debug.Log(logString);
            }
            foreach (Exception e in Plugin.logExceptions)
            {
                Debug.LogException(e);
            }
            Plugin.loggedStrings = true;
        }

        SaveStateData.AASaveDataContainer ??= new();

        Debug.Log("<Archduid's Additions> LOADED METHOD: ON_MODS_INIT");

        #region Atlases

        #region Items
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
        if (!Futile.atlasManager.DoesContainAtlas("SphericalFruit"))
        {
            Futile.atlasManager.LoadAtlas("atlases/SphericalFruit");
        }
        if (!Futile.atlasManager.DoesContainAtlas("LightningFruit"))
        {
            Futile.atlasManager.LoadAtlas("atlases/LightningFruit");
        }
        if (!Futile.atlasManager.DoesContainAtlas("AshPepper"))
        {
            Futile.atlasManager.LoadAtlas("atlases/AshPepper");
        }
        if (!Futile.atlasManager.DoesContainAtlas("ParasiteEgg"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ParasiteEgg");
        }
        if (!Futile.atlasManager.DoesContainAtlas("SnailShell"))
        {
            Futile.atlasManager.LoadAtlas("atlases/SnailShell");
        }
        #endregion

        #region Creatures
        if (!Futile.atlasManager.DoesContainAtlas("CloudFish"))
        {
            Futile.atlasManager.LoadAtlas("atlases/CloudFish");
        }
        if (!Futile.atlasManager.DoesContainAtlas("Parasite"))
        {
            Futile.atlasManager.LoadAtlas("atlases/Parasite");
        }
        if (!Futile.atlasManager.DoesContainAtlas("MimicCrab"))
        {
            Futile.atlasManager.LoadAtlas("atlases/MimicCrab");
        }
        #endregion

        #region Level Sprites

        for (int i = 2; i < 6; i++)
        {
            for (int j = 1; j < 4; j++)
            {
                string spriteName = "Crate" + i + "x" + i + "_" + j;

                Texture2D texture = new(1, 1);
                string filePath = AssetManager.ResolveFilePath("atlases" + Path.DirectorySeparatorChar + "leveltextures" + Path.DirectorySeparatorChar + spriteName + ".png");
                AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + filePath, true, true);
                HeavyTexturesCache.LoadAndCacheAtlasFromTexture(spriteName, texture, false);
            }
        }

        #endregion

        #region Other Stuff
        if (!Futile.atlasManager.DoesContainAtlas("Arc"))
        {
            Futile.atlasManager.LoadAtlas("atlases/Arc");
        }
        if (!Futile.atlasManager.DoesContainAtlas("StoneHead"))
        {
            Futile.atlasManager.LoadAtlas("atlases/leveltextures/StoneHead");
        }
        if (!Futile.atlasManager.DoesContainAtlas("BowlSymbol"))
        {
            Futile.atlasManager.LoadAtlas("atlases/BowlSymbol");
        }
        #endregion

        #endregion

        MachineConnector.SetRegisteredOI(Plugin.PLUGIN_GUID, Plugin.Options);

        AAEnums.RegisterAllEnums();

        foreach (MultiplayerUnlocks.SandboxUnlockID type in SandboxUnlockID.values)
        {
            if (!MultiplayerUnlocks.ItemUnlockList.Contains(type))
            { MultiplayerUnlocks.ItemUnlockList.Add(type); }
        }

        foreach (PlacedObject.Type type in PlacedObjectType.values)
        {
            if (type != null)
            {
                try
                { Pom.Pom.RegisterCategoryOverride(type, "Archdruid's Additions"); }
                catch
                { }
            }
        }

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath(string.Concat(["Shaders", Path.DirectorySeparatorChar.ToString(), "rainworldaashaders",])));
            self.Shaders.Add("ArchAdds.CustomVectorCircle", FShader.CreateShader("ArchAdds.CustomVectorCircle", bundle.LoadAsset<Shader>("Assets/Shaders/CustomVectorCircle.shader")));
            Data.MiscData.CircleFade = Shader.PropertyToID("_CircleFade");
        }
        catch
        { }
    }
    internal static void RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);

        #region Object Sprites
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
        if (Futile.atlasManager.DoesContainAtlas("SphericalFruit"))
        {
            Futile.atlasManager.UnloadAtlas("SphericalFruit");
        }
        if (Futile.atlasManager.DoesContainAtlas("LightningFruit"))
        {
            Futile.atlasManager.UnloadAtlas("LightningFruit");
        }
        if (Futile.atlasManager.DoesContainAtlas("AshPepper"))
        {
            Futile.atlasManager.UnloadAtlas("AshPepper");
        }
        if (Futile.atlasManager.DoesContainAtlas("ParasiteEgg"))
        {
            Futile.atlasManager.UnloadAtlas("ParasiteEgg");
        }
        if (Futile.atlasManager.DoesContainAtlas("SnailShell"))
        {
            Futile.atlasManager.UnloadAtlas("SnailShell");
        }
        #endregion

        #region Creature Sprites
        if (Futile.atlasManager.DoesContainAtlas("CloudFish"))
        {
            Futile.atlasManager.UnloadAtlas("CloudFish");
        }
        if (Futile.atlasManager.DoesContainAtlas("Parasite"))
        {
            Futile.atlasManager.UnloadAtlas("Parasite");
        }
        if (Futile.atlasManager.DoesContainAtlas("MimicCrab"))
        {
            Futile.atlasManager.UnloadAtlas("MimicCrab");
        }
        #endregion

        #region Level Sprites
        if (Futile.atlasManager.DoesContainAtlas("Bowl"))
        {
            Futile.atlasManager.UnloadAtlas("Bowl");
        }

        for (int i = 2; i < 6; i++)
        {
            for (int j = 1; j < 4; j++)
            {
                string spriteName = "Crate" + i + "x" + i + "_" + j;

                if (Futile.atlasManager.DoesContainAtlas(spriteName))
                {
                    Futile.atlasManager.UnloadAtlas(spriteName);
                }
            }
        }
        #endregion

        #region Other Sprites
        if (Futile.atlasManager.DoesContainAtlas("Arc"))
        {
            Futile.atlasManager.UnloadAtlas("Arc");
        }
        if (Futile.atlasManager.DoesContainAtlas("StoneHead"))
        {
            Futile.atlasManager.UnloadAtlas("StoneHead");
        }
        if (Futile.atlasManager.DoesContainAtlas("BowlSymbol"))
        {
            Futile.atlasManager.UnloadAtlas("BowlSymbol");
        }
        #endregion
    }
    internal static void RainWorld_OnModsEnabled(On.RainWorld.orig_OnModsEnabled orig, RainWorld self, ModManager.Mod[] newlyEnabledMods)
    {
        orig(self, newlyEnabledMods);

        Debug.Log("<Archduid's Additions> LOADED METHOD: ON_MODS_ENABLED");

        foreach (var mod in newlyEnabledMods)
        {
            if (mod.id == "archdruidbookwalter.archdruidsadditions")
            {
                AAEnums.RegisterAllEnums();
                foreach (PlacedObject.Type type in PlacedObjectType.values)
                {
                    try
                    { Pom.Pom.RegisterCategoryOverride(type, "Archdruid's Additions"); }
                    catch
                    { }
                }

                break;
            }
        }
    }
    internal static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        Debug.Log("<Archduid's Additions> LOADED METHOD: ON_MODS_DISABLED");

        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == "archdruidbookwalter.archdruidsadditions")
            {
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Bow))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.Bow);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.ScarletFlowerBulb))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.ScarletFlowerBulb);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.ParrySword))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.ParrySword);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.Potato))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.Potato);
                }
                if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.LightningFruit))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.LightningFruit);
                }
                AAEnums.UnregisterAllEnums();
                break;
            }
        }
    }
    internal static void RainWorld_PostModsInIt(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        Debug.Log("<Archduid's Additions> TRIED TO LOAD METHOD: POST_MODS_INIT");

        orig(self);

        Debug.Log("<Archduid's Additions> SUCCESSFULLY LOADED METHOD: POST_MODS_INIT");

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
