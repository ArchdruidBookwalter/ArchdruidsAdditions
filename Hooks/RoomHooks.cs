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

                    float hue;
                    float sat;
                    float val;

                    if (data.maxHue == 1)
                    { hue = UnityEngine.Random.Range(0f, 1f); }
                    else
                    { hue = data.maxHue; }

                    if (data.maxSat == 1)
                    { sat = UnityEngine.Random.Range(0f, 1f); }
                    else
                    { sat = data.maxSat; }

                    if (data.maxVal == 1)
                    { val = UnityEngine.Random.Range(0.05f, 1f); }
                    else
                    { val = data.maxVal; }

                    Color newColor = Color.HSVToRGB(hue, sat, val);

                    abstractConsumable.realizedObject = new Objects.Potato(abstractConsumable, true, data.rotation, newColor, false);

                    self.abstractRoom.AddEntity(abstractConsumable);
                }
            }
        }
    }
}
