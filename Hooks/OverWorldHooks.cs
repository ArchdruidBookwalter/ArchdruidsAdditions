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

                /*
                foreach (string[] regionData in Plugin.RegionData.regionDataList)
                {
                    Debug.Log("");
                    foreach (string regionName in regionData)
                    {
                        if (regionName != null)
                        {
                            Debug.Log(regionName);
                        }
                    }
                }*/
            }
        }

        orig(self, game);
    }
}
