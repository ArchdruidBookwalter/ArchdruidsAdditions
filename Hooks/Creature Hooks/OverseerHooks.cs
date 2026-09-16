using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class OverseerHooks
{
    internal static void OverseerTutorialBehavior_Update(On.OverseerTutorialBehavior.orig_Update orig, OverseerTutorialBehavior self)
    {
        if (ModManager.MMF && MMF.cfgExtraTutorials.Value == false)
        {
            return;
        }
        orig(self);
    }
}
