namespace ArchdruidsAdditions.Hooks;

public static class FLabelHooks
{
    internal static void Redraw(On.FLabel.orig_Redraw orig, FLabel self, bool shouldForceDirty, bool shouldUpdateDepth)
    {
        orig(self, shouldForceDirty, shouldUpdateDepth);
    }
}
