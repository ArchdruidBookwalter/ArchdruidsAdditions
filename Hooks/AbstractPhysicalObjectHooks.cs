using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;

using ArchdruidsAdditions.Objects;
using ArchdruidsAdditions.Enums;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractPhysicalObjectHooks
{
    internal static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig.Invoke(self);
        if (self.realizedObject is null)
        {
            if (self.type == AbstractObjectType.Bow)
            {
                self.realizedObject = new Bow(self, self.world);
            }
            else if (self.type == AbstractObjectType.ScarletFlowerBulb)
            {
                self.realizedObject = new ScarletFlowerBulb(self, self.world, false, Custom.RNV(), new(1f, 0f, 0f));
            }
            else if (self.type == AbstractObjectType.ParrySword)
            {
                self.realizedObject = new ParrySword(self, self.world, new(1f, 0.79f, 0.3f));
            }
            else if (self.type == AbstractObjectType.Potato)
            {
                float hue = Random.Range(0f, 1f);
                float sat = Random.Range(0f, 1f);
                float val = Random.Range(0.05f, 1f);
                self.realizedObject = new Potato(self, false, new(0f, 1f), Color.HSVToRGB(hue, sat, val), true);
                (self.realizedObject as Potato).bodyChunks[1].vel += Custom.RNV();
            }
            else if (self.type == AbstractObjectType.LightningFruit)
            {
                int charge;
                int power;

                if (self.unrecognizedAttributes != null && self.unrecognizedAttributes.Count() > 0)
                {
                    charge = int.Parse(self.unrecognizedAttributes[0]);
                    power = int.Parse(self.unrecognizedAttributes[1]);
                }
                else
                {
                    charge = Random.value > 0.5f ? 1 : -1;
                    power = 1000;
                }

                self.realizedObject = new LightningFruit(self, charge)
                { power = power };
            }
            else if (self.type == AbstractObjectType.FirePepper)
            {
                self.realizedObject = new FirePepper(self);
            }
        }
    }

    internal static void AbstractPhysicalObject_Abstractize(On.AbstractPhysicalObject.orig_Abstractize orig, AbstractPhysicalObject self, WorldCoordinate coord)
    {
        if (self.realizedObject is LightningFruit fruit)
        {
            if (self.unrecognizedAttributes == null)
            {
                self.unrecognizedAttributes = new string[2];
            }
            self.unrecognizedAttributes[0] = fruit.charge.ToString();
            self.unrecognizedAttributes[1] = fruit.power.ToString();
        }
        orig(self, coord);
    }

    internal static bool AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type)
    {
        if (type == AbstractObjectType.ScarletFlowerBulb ||
            type == AbstractObjectType.Potato ||
            type == AbstractObjectType.LightningFruit ||
            type == AbstractObjectType.FirePepper)
        {
            return true;
        }
        return orig(type);
    }
}
