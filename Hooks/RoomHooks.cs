using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class RoomHooks
{
    internal static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        if (self.game == null) return;

        foreach (var pobj in self.roomSettings.placedObjects)
        {
            if (pobj.active && pobj.type == Enums.PlacedObjectType.ScarletFlower)
            {
                Objects.ScarletFlowerData data = pobj.data as Objects.ScarletFlowerData;
                Objects.ScarletFlower flower = new(pobj, data.rotation);
                self.AddObject(flower);
            }
        }
    }
}
