using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;
using UnityEngine;
using RWCustom;
using MonoMod.Cil;

namespace ArchdruidsAdditions.Hooks;

public static class DevtoolsHooks
{
    internal static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type type, PlacedObject pobj)
    {
        if (type == Enums.PlacedObjectType.ScarletFlower)
        {
            if (pobj == null)
            {
                self.RoomSettings.placedObjects.Add(pobj = new(type, null)
                {
                    pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(UnityEngine.Random.value + 360f) * .2f
                });
            }
            var pobjRep = new Objects.ScarletFlowerRepresentation(self.owner, type.ToString() + "_Rep", self, pobj, type.ToString());
            self.tempNodes.Add(pobjRep);
            self.subNodes.Add(pobjRep);
        }
        else
        {
            orig(self, type, pobj);
        }
    }
    internal static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
    {
        if (type == Enums.PlacedObjectType.ScarletFlower)
        {
            return ObjectsPage.DevObjectCategories.Unsorted;
        }
        return orig(self, type);
    }
    internal static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
    {
        if (self.type == Enums.PlacedObjectType.ScarletFlower)
        {
            self.data = new Objects.ScarletFlowerData(self);
        }
        else
        {
            orig(self);
        }
    }
}
