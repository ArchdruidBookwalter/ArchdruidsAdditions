using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class ScavengerHooks
{
    internal static int CollectScore(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        if (obj is Objects.ScarletFlowerBulb)
        {
            return 3;
        }
        else if (obj is Objects.Potato)
        {
            return 2;
        }
        else if (obj is Objects.ParrySword)
        {
            return 10;
        }
        else
        {
            return orig(self, obj, weaponFiltered);
        }
    }
}
