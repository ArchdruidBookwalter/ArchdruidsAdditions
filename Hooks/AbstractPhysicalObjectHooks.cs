using System.Linq;
using System.Globalization;
using ArchdruidsAdditions.Enums;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractPhysicalObjectHooks
{
    internal static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);

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
            else if (self.type == AbstractObjectType.AshPepper)
            {
                self.realizedObject = new AshPepper(self, null, 0);
            }
            else if (self.type == AbstractObjectType.ParasiteEgg)
            {
                bool growOnStartup = false;
                if (self.unrecognizedAttributes != null && self.unrecognizedAttributes.Contains("GROW_ON_STARTUP"))
                {
                    growOnStartup = true;
                    self.unrecognizedAttributes = null;
                }

                self.realizedObject = new ParasiteEgg(self, growOnStartup);
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
        bool ret = orig(type);

        if (type == AbstractObjectType.ScarletFlowerBulb ||
            type == AbstractObjectType.Potato ||
            type == AbstractObjectType.LightningFruit ||
            type == AbstractObjectType.AshPepper)
        {
            return true;
        }

        return ret;
    }

    internal static void AbstractObjectStick_FromString(On.AbstractPhysicalObject.AbstractObjectStick.orig_FromString orig, string[] splitString, AbstractRoom room)
    {
        if (splitString.Length > 1)
        {
            if (splitString[1] == "paraStk")
            {
                //Debug.Log("");
                //Debug.Log("METHOD ABSTRACTOBJECTSTICK_FROMSTRING WAS CALLED FOR PARASITESTICK");
                //Debug.Log("");

                EntityID ID1 = EntityID.FromString(splitString[2]);
                EntityID ID2 = EntityID.FromString(splitString[3]);
                AbstractPhysicalObject obj1 = null;
                AbstractPhysicalObject obj2 = null;

                for (int i = 0; i < room.entities.Count; i++)
                {
                    AbstractWorldEntity entity = room.entities[i];

                    if (entity is AbstractPhysicalObject obj)
                    {
                        if (obj.ID == ID1)
                        { obj1 = obj; }
                        else if (obj.ID == ID2)
                        { obj2 = obj; }
                    }
                }

                if (obj1 != null && obj2 != null)
                {
                    int chunk = int.Parse(splitString[4], NumberStyles.Any, CultureInfo.InvariantCulture);
                    int growth = int.Parse(splitString[5], NumberStyles.Any, CultureInfo.InvariantCulture);

                    new AbstractParasiteStick(obj1, obj2, chunk, growth).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splitString, 6);
                    return;
                }
            }
            else if (splitString[1] == "paraEggStk")
            {
                EntityID ID1 = EntityID.FromString(splitString[2]);
                EntityID ID2 = EntityID.FromString(splitString[3]);
                AbstractPhysicalObject obj1 = null;
                AbstractPhysicalObject obj2 = null;

                for (int i = 0; i < room.entities.Count; i++)
                {
                    AbstractWorldEntity entity = room.entities[i];

                    if (entity is AbstractPhysicalObject obj)
                    {
                        if (obj.ID == ID1)
                        { obj1 = obj; }
                        else if (obj.ID == ID2)
                        { obj2 = obj; }
                    }
                }

                if (obj1 != null && obj2 != null)
                {
                    new AbstractParasiteEggStick(obj1, obj2).unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(splitString, 4);
                    return;
                }
            }
            else
            {
                orig(splitString, room);
            }
        }
        else
        {
            orig(splitString, room);
        }
    }
}
