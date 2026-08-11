using System.Collections.Generic;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PhysicalObjectHooks
{
    internal static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {
        /*
        if (self.abstractPhysicalObject.stuckObjects.Count > 0)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.abstractPhysicalObject.stuckObjects)
            {
                if (stick is AbstractParasiteStick paraStick && self.abstractPhysicalObject != paraStick.Parasite)
                {
                    if (paraStick.Parasite.Room != self.abstractPhysicalObject.Room)
                    {
                        Debug.Log("PARASITE IS IN WRONG ROOM!");
                    }
                }
                else if (stick is AbstractCrabShellStick shellStick && self.abstractPhysicalObject != shellStick.Crab)
                {
                    if (shellStick.Crab.Room != self.abstractPhysicalObject.Room)
                    {
                        Debug.Log("CRAB IS IN WRONG ROOM!");
                    }
                }
            }
        }*/

        /*
        foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.abstractPhysicalObject.stuckObjects)
        {
            PhysicalObject obj1 = stick.A.realizedObject;
            PhysicalObject obj2 = stick.B.realizedObject;

            if (obj1 != null && obj2 != null)
            {
                Create_LineBetweenTwoPoints(self.room, obj1.firstChunk.pos, obj2.firstChunk.pos, 2f, "Yellow", 0);
            }
        }*/

        /*
        if (self is CrabShell)
        {
            List<string> strings = [];
            strings.Add((obj is AbstractCreature) ? (obj as AbstractCreature).creatureTemplate.type.value : obj.type.value);
            strings.Add("STUCK OBJECTS:");
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in obj.stuckObjects)
            {
                strings.Add("   " +
                ((stick.A is AbstractCreature) ? (stick.A as AbstractCreature).creatureTemplate.type.value : stick.A.type.value) +
                " - " +
                ((stick.B is AbstractCreature) ? (stick.B as AbstractCreature).creatureTemplate.type.value : stick.B.type.value));
            }

            Create_TextBlock(self.room, self.firstChunk.pos + new Vector2(50f, 50f), -1, [.. strings], "Red", 0);
        }*/

        /*
        if (obj.stuckObjects.Count > 0)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.abstractPhysicalObject.stuckObjects)
            {
                if (stick is AbstractParasiteStick paraStick)
                {
                    Debug.Log("PARASITE STICK DETECTED FOR OBJECTS: ");

                    Debug.Log("   " +
                    ((stick.A is AbstractCreature) ? (stick.A as AbstractCreature).creatureTemplate.type : stick.A.type) +
                    " - " +
                    ((stick.B is AbstractCreature) ? (stick.B as AbstractCreature).creatureTemplate.type : stick.B.type));
                }
                else if (stick is AbstractParasiteEggStick paraEggStick)
                {
                    Debug.Log("PARASITE EGG STICK DETECTED FOR OBJECTS: ");

                    Debug.Log("   " +
                    ((stick.A is AbstractCreature) ? (stick.A as AbstractCreature).creatureTemplate.type : stick.A.type) +
                    " - " +
                    ((stick.B is AbstractCreature) ? (stick.B as AbstractCreature).creatureTemplate.type : stick.B.type));
                }
                else if (stick is AbstractCrabShellStick shellStick)
                {
                    Debug.Log("CRAB SHELL STICK DETECTED FOR OBJECTS: ");

                    Debug.Log("   " +
                    ((stick.A is AbstractCreature) ? (stick.A as AbstractCreature).creatureTemplate.type : stick.A.type) +
                    " - " +
                    ((stick.B is AbstractCreature) ? (stick.B as AbstractCreature).creatureTemplate.type : stick.B.type));
                }
            }
        }*/

        /*
        if (self is Parasite parasite && parasite.buriedInChunk != null && parasite.abstractPhysicalObject.stuckObjects.Count > 0)
        {
            AbstractPhysicalObject otherObject = parasite.abstractPhysicalObject.stuckObjects[0].B;
            if (parasite.room != null && otherObject.Room.realizedRoom != null && parasite.room != otherObject.Room.realizedRoom)
            {
                Debug.Log("PARASITE IS IN WRONG ROOM!");

                parasite.room.RemoveObject(parasite);
                otherObject.Room.realizedRoom.AddObject(parasite);

                parasite.abstractCreature.Move(otherObject.pos);
            }
        }
        else if (self is MimicCrab crab && crab.AttachedToObject != null && crab.abstractPhysicalObject.stuckObjects.Count > 0)
        {
            AbstractPhysicalObject otherObject = crab.shellStick.Shell;
            if (crab.room != null && otherObject.Room.realizedRoom != null && crab.room != otherObject.Room.realizedRoom)
            {
                Debug.Log("CRAB IS IN WRONG ROOM!");

                crab.room.RemoveObject(crab);
                otherObject.Room.realizedRoom.AddObject(crab);

                crab.abstractCreature.Move(otherObject.pos);
            }
        }*/

        orig(self, eu);
    }
}
