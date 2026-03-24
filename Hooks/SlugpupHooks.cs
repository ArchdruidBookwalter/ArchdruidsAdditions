using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class SlugpupHooks
{
    internal static SlugNPCAI.Food SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is Potato)
        {
            return SlugNPCAI.Food.DangleFruit;
        }
        else if (food is LightningFruit)
        {
            return SlugNPCAI.Food.Neuron;
        }
        else if (food is FirePepper)
        {
            return SlugNPCAI.Food.FireEgg;
        }
        return orig(self, food);
    }
    internal static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is LightningFruit fruit && fruit.power > 0)
        {
            return false;
        }
        else if (food is FirePepper)
        {
            return false;
        }

        return orig(self, food);    
    }
}
