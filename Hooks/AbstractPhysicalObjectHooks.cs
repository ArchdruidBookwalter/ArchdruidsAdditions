using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractPhysicalObjectHooks
{
    internal static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig.Invoke(self);
        if (self.realizedObject is null)
        {
            if (self.type == Enums.AbstractObjectType.ScarletFlowerBulb)
            {
                self.realizedObject = new Objects.ScarletFlowerBulb(self, self.world, false, Custom.RNV());
                UnityEngine.Debug.Log("SPAWNED \"Scarlet Flower Bulb\" OBJECT IN DEFAULT STATE");
            }
        }
    }

    internal static bool AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type)
    {
        if (type == Enums.AbstractObjectType.ScarletFlowerBulb)
        {
            return true;
        }
        return orig(type);
    }
}
