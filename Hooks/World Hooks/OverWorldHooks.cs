using Debug = UnityEngine.Debug;

namespace ArchdruidsAdditions.Hooks;

public static class OverWorldHooks
{
    internal static void OverWorld_ctor(On.OverWorld.orig_ctor orig, OverWorld self, RainWorldGame game)
    {
        if (!game.IsArenaSession)
        {
            Debug.Log("<Archduid's Additions> TRIED TO GET REGION DATA");

            Plugin.RegionData = new(game, game.TimelinePoint);

            if (Plugin.RegionData != null)
            {
                Debug.Log("<Archduid's Additions> GOT REGION DATA");
            }
        }

        orig(self, game);
    }

    internal static void OverWorld_Update(On.OverWorld.orig_Update orig, OverWorld self)
    {
        //LogMethodStart("OVERWORLD_UPDATE");

        orig(self);

        //LogMethodEnd();
    }
}
