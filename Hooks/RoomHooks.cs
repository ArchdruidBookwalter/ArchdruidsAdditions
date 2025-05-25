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
        var firstTimeRealized = self.abstractRoom.firstTimeRealized;

        orig(self);

        if (self.game == null) return;

        var placedObjects = self.roomSettings.placedObjects;
        var session = self.game.session;

        foreach (var pobj in placedObjects)
        {
            if (pobj.active && pobj.type == Enums.PlacedObjectType.ScarletFlower)
            {
                Objects.ScarletFlowerData data = pobj.data as Objects.ScarletFlowerData;

                Objects.ScarletFlowerStem flower = new(pobj, data.rotation);
                self.AddObject(flower);

                if (firstTimeRealized && (session is not StoryGameSession || !(session as StoryGameSession).saveState.ItemConsumed(
                    self.world, false, self.abstractRoom.index, placedObjects.IndexOf(pobj))))
                {
                    var abstractConsumable = new AbstractConsumable(self.world, Enums.AbstractObjectType.ScarletFlowerBulb, null, self.GetWorldCoordinate(pobj.pos),
                        self.game.GetNewID(), self.abstractRoom.index, placedObjects.IndexOf(pobj), data as PlacedObject.ConsumableObjectData)
                    { isConsumed = false };

                    abstractConsumable.realizedObject = new Objects.ScarletFlowerBulb(abstractConsumable, self.world, true, data.rotation, new(1f, 0f, 0f));

                    self.abstractRoom.AddEntity(abstractConsumable);
                }
            }
            if (pobj.active && pobj.type == Enums.PlacedObjectType.Potato)
            {
                Objects.PotatoData data = pobj.data as Objects.PotatoData;

                if (firstTimeRealized && (session is not StoryGameSession || !(session as StoryGameSession).saveState.ItemConsumed(
                    self.world, false, self.abstractRoom.index, placedObjects.IndexOf(pobj))))
                {
                    var abstractConsumable = new AbstractConsumable(self.world, Enums.AbstractObjectType.Potato, null, self.GetWorldCoordinate(pobj.pos), 
                        self.game.GetNewID(), self.abstractRoom.index, placedObjects.IndexOf(pobj), data as PlacedObject.ConsumableObjectData)
                    { isConsumed = false };

                    float hue = UnityEngine.Random.Range(data.minHue, data.maxHue);
                    float sat = UnityEngine.Random.Range(data.minSat, data.maxSat);
                    float val = UnityEngine.Random.Range(data.minVal, data.maxVal);

                    if (val < 0.05f)
                    {
                        val = 0.05f;
                    }

                    Color newColor = Color.HSVToRGB(hue, sat, val);

                    abstractConsumable.realizedObject = new Objects.Potato(abstractConsumable, true, data.rotation, newColor, data.naturalColors);

                    self.abstractRoom.AddEntity(abstractConsumable);
                }
            }
        }
    }
}
