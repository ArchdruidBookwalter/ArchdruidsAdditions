using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractPhysicalObjectHooks
{
    internal static void AbstractPhysicalObject_Update(On.AbstractPhysicalObject.orig_Update orig, AbstractPhysicalObject self, int time)
    {
        orig(self, time);
    }
    internal static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        //Debug.Log("");
        //Debug.Log("OBJECT \'" + self.type.ToString() + "\' TRIED TO REALIZE IN ROOM");

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
            else if (self.type == AbstractObjectType.CrabShell)
            {
                self.realizedObject = new CrabShell(self, self.world);
            }
            else if (self.type == AbstractObjectType.BigChandelier)
            {
                self.realizedObject = new HangingPlatform(self.Room.realizedRoom, self, 200f, "Chandelier");
            }
            else if (self.type == AbstractObjectType.LootCrate)
            {
                int size = 2;
                int damage = 1;

                if (self.unrecognizedAttributes != null && self.unrecognizedAttributes.Count() > 0)
                {
                    size = int.Parse(self.unrecognizedAttributes[0]);
                    damage = int.Parse(self.unrecognizedAttributes[1]);
                }

                self.realizedObject = new LootCrate(self, self.Room.realizedRoom, size, damage);
            }
        }

        //if (self.realizedObject != null)
        //{ Debug.Log("OBJECT \'" + self.type.ToString() + "\' WAS REALIZED"); }
    }
    internal static void AbstractPhysicalObject_Abstractize(On.AbstractPhysicalObject.orig_Abstractize orig, AbstractPhysicalObject self, WorldCoordinate coord)
    {
        if (self.realizedObject is LightningFruit fruit)
        {
            self.unrecognizedAttributes ??= new string[2];
            self.unrecognizedAttributes[0] = fruit.charge.ToString();
            self.unrecognizedAttributes[1] = fruit.power.ToString();
        }
        if (self.realizedObject is LootCrate crate)
        {
            self.unrecognizedAttributes ??= new string[2];
            self.unrecognizedAttributes[0] = crate.size.ToString();
            self.unrecognizedAttributes[1] = crate.damage.ToString();
        }

        /*
        if (self is not AbstractCreature)
        {
            Debug.Log("OBJECT \'" + self.type.ToString() + "\' ABSTRACTIZED");
        }
        else
        {
            Debug.Log("CREATURE \'" + (self as AbstractCreature).creatureTemplate.type.ToString() + "\' ABSTRACTIZED");
        }*/

        orig(self, coord);
    }
    internal static void AbstractPhysicalObject_AddConnected(On.AbstractPhysicalObject.orig_AddConnected orig, AbstractPhysicalObject self, ref List<AbstractPhysicalObject> list)
    {
        orig(self, ref list);

        /*
        Debug.Log("METHOD ABSTRACTPHYSICALOBJECT_ADDCONNECTED WAS CALLED BY: " + self.type.value);
        Debug.Log("   ITEMS: ");
        foreach (AbstractPhysicalObject item in list)
        {
            Debug.Log("   " + item.type.value.ToUpper());
        }*/
    }
    internal static void AbstractPhysicalObject_LoseAllStuckObjects(On.AbstractPhysicalObject.orig_LoseAllStuckObjects orig, AbstractPhysicalObject self)
    {
        orig(self);
    }
    internal static void AbstractPhysicalObject_Destroy(On.AbstractPhysicalObject.orig_Destroy orig, AbstractPhysicalObject self)
    {
        orig(self);
    }

    internal static bool AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type)
    {
        bool ret = orig(type);

        if (type == AbstractObjectType.ScarletFlowerBulb ||
            type == AbstractObjectType.Potato ||
            type == AbstractObjectType.LightningFruit ||
            type == AbstractObjectType.AshPepper || 
            type == AbstractObjectType.LootCrate)
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
    internal static void AbstractObjectStick_Deactivate(On.AbstractPhysicalObject.AbstractObjectStick.orig_Deactivate orig, AbstractPhysicalObject.AbstractObjectStick self)
    {
        if (MiscData.stopAbsStkDeactivation)
        { return; }

        if (self is AbstractCrabShellStick shellStick)
        {
            //Debug.Log("SHELLSTICK WAS DEACTIVATED!");

            if (shellStick.Crab.realizedCreature != null && shellStick.Crab.realizedCreature is MimicCrab crab)
            { crab.shellStick = null; }
            if (shellStick.Shell.realizedObject != null && shellStick.Shell.realizedObject is CrabShell shell)
            { shell.shellStick = null; }
        }

        orig(self);
    }
}
