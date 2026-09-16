using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class SlugpupHooks
{
    internal static SlugNPCAI.Food SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
    {
        SlugNPCAI.Food baseType = orig(self, food);

        if (food is Potato)
        {
            return SlugNPCAI.Food.DangleFruit;
        }
        else if (food is LightningFruit)
        {
            return SlugNPCAI.Food.Neuron;
        }
        else if (food is AshPepper)
        {
            return SlugNPCAI.Food.FireEgg;
        }
        else if (food is CloudFish)
        {
            return SlugNPCAI.Food.VultureGrub;
        }

        return baseType;
    }
    internal static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject food)
    {
        bool baseWant = orig(self, food);

        if (food is LightningFruit fruit && fruit.power > 0)
        {
            return false;
        }
        else if (food is AshPepper)
        {
            return false;
        }
        else if (food is Parasite)
        {
            return false;
        }

        return baseWant;    
    }
}
