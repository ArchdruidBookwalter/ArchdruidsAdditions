using System;

namespace ArchdruidsAdditions.Hooks;

public static class LightningHooks
{
    internal static void Lightning_ctor(On.Lightning.orig_ctor orig, Lightning self, Room room, float intensity, bool bkgOnly)
    {
        orig(self, room, intensity, bkgOnly);

        try
        {
            string colorString = Plugin.RegionData.ReadRegionData(room.world.region.name, "LightningColor");
            if (colorString != null)
            {
                if (colorString.StartsWith("#"))
                { colorString = colorString.Remove(0, 1); }

                Color newColor = Custom.hexToColor(colorString);
                self.bkgGradient[0] = newColor;
                self.bkgGradient[1] = newColor;
            }
        }
        catch (Exception e)
        {
            Debug.Log("<Archduid's Additions> ---METHOD, \'Lightning_ctor\' EXPERIENCED AN EXCEPTION WHILE TRYING TO GET REGION PROPERTIES DATA VALUE, \'LightningColor\'. IS THE FILE FORMATTED CORRECTLY?---");
            Debug.LogException(e);
        }
    }
}
