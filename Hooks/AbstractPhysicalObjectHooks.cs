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
                self.realizedObject = new Objects.ScarletFlowerBulb(self, self.world, false, Custom.RNV(), new(1f, 0f, 0f));
                UnityEngine.Debug.Log("SPAWNED \"Scarlet Flower Bulb\" OBJECT IN DEFAULT STATE");
            }
            if (self.type == Enums.AbstractObjectType.ParrySword)
            {
                self.realizedObject = new Objects.ParrySword(self, self.world, new(0f, 1f, 0.5f));
                UnityEngine.Debug.Log("SPAWNED \"Parry Spear\" OBJECT IN DEFAULT STATE");
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
