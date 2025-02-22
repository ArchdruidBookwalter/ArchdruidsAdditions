using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractPhysicalObjectHooks
{
    internal static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig.Invoke(self);
        bool flag = self.type == Enums.AbstractObjectType.ScarletFlowerBulb;
        if (flag)
        {
            self.realizedObject = new Objects.ScarletFlowerBulb(self, self.world);
        }
    }
}
