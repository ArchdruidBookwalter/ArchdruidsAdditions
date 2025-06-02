using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class RainWorldHooks
{
    internal static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        orig(self);
        if (Objects.Cursors.container != null)
        {
            Objects.Cursors.container.MoveToFront();
        }
    }
}
