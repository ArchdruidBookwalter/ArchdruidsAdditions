using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class SlugpupHooks
{
    internal static SlugNPCAI.Food SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is Objects.Potato)
        {
            return SlugNPCAI.Food.DangleFruit;
        }
        return orig(self, food);
    }
}
