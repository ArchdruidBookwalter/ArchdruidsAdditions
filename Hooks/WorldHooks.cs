using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class WorldHooks
{
    internal static CreatureTemplate.Type WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
    {
        if (s == "CloudFish" || s == "cloudfish" || s == "cloud fish" || s == "Cloud Fish")
        { return Enums.CreatureTemplateType.CloudFish; }
        return orig(s);
    }
}
