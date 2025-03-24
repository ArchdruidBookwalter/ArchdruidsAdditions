using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class SlugNPCHooks
{
    internal static void SlugNPCAI_AteFood(On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is Objects.Potato)
        {
        }
        orig(self, food);
    }
}
