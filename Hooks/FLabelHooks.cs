using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class FLabelHooks
{
    internal static void Redraw(On.FLabel.orig_Redraw orig, FLabel self, bool shouldForceDirty, bool shouldUpdateDepth)
    {
        if (self.text.Contains("MIGRATING"))
        {
        }
        orig(self, shouldForceDirty, shouldUpdateDepth);
    }
}
