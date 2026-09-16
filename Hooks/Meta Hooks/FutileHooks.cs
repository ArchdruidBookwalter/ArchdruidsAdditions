using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Data;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class FutileHooks
{
    internal static void FLabel_Redraw(On.FLabel.orig_Redraw orig, FLabel self, bool shouldForceDirty, bool shouldUpdateDepth)
    {
        orig(self, shouldForceDirty, shouldUpdateDepth);
    }

    internal static void FFacetRenderLayer_UpdateMeshProperties(On.FFacetRenderLayer.orig_UpdateMeshProperties orig, FFacetRenderLayer self)
    {
        orig(self);
        if (self is FCustomRenderLayer layer)
        { layer.OverrideUpdateMeshProperties(); }
    }
}
