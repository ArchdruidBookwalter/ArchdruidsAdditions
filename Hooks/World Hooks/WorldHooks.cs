namespace ArchdruidsAdditions.Hooks;

public static class WorldHooks
{
    internal static CreatureTemplate.Type WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
    {
        CreatureTemplate.Type baseType = orig(s);

        if (s == "CloudFish" || s == "cloudfish" || s == "cloud fish" || s == "Cloud Fish")
        {
            return CreatureTemplateType.CloudFish;
        }

        if (s == "MimicCrab" || s == "mimiccrab" || s == "mimic crab" || s == "Mimic Crab")
        {
            return CreatureTemplateType.MimicCrab;
        }

        return baseType;
    }
}
