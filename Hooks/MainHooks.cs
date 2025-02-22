using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class MainHooks
{
    internal static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        #region MultiplayerUnlocks
        if (!MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ScarletFlower))
        {
            MultiplayerUnlocks.ItemUnlockList.Add(Enums.SandboxUnlockID.ScarletFlower);
        }
        #endregion

        #region Atlases
        if (!Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ScarletFlowerStem");
        }
        if (!Futile.atlasManager.DoesContainAtlas("ScarletFlowerBulb"))
        {
            Futile.atlasManager.LoadAtlas("atlases/ScarletFlowerBulb");
        }
        #endregion

        Pom.Pom.RegisterCategoryOverride(Enums.PlacedObjectType.ScarletFlower, "Archdruid's Additions");

        UnityEngine.Debug.Log("\"Archdruid's Additions\" HAS SUCCESSFULY HOOKED, \"OnModsInit\" METHOD. =============================================");
    }

    internal static void RainWorld_UnloadResources(On.RainWorld.orig_UnloadResources orig, RainWorld self)
    {
        orig(self);
        if (Futile.atlasManager.DoesContainAtlas("ScarletFlowerStem"))
        {
            Futile.atlasManager.UnloadAtlas("ScarletFlowerStem");
        }
    }

    internal static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        foreach (var mod in newlyDisabledMods)
        {
            if (mod.id == "archdruidbookwalter.archdruidsadditions")
            {
                if (MultiplayerUnlocks.ItemUnlockList.Contains(Enums.SandboxUnlockID.ScarletFlower))
                {
                    MultiplayerUnlocks.ItemUnlockList.Remove(Enums.SandboxUnlockID.ScarletFlower);
                }
                Enums.AbstractObjectType.UnregisterValues();
                Enums.MiscItemType.UnregisterValues();
                Enums.MultiplayerItemType.UnregisterValues();
                Enums.PlacedObjectType.UnregisterValues();
                Enums.SandboxUnlockID.UnregisterValues();
                break;
            }
        }
    }
}
