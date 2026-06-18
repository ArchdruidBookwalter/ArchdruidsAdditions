using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PhysicalObjectHooks
{
    internal static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {
        if (self is Parasite parasite && parasite.buriedInChunk != null && parasite.abstractPhysicalObject.stuckObjects.Count > 0)
        {
            AbstractPhysicalObject otherObject = parasite.abstractPhysicalObject.stuckObjects[0].B;
            if (parasite.room != null && otherObject.Room.realizedRoom != null && parasite.room != otherObject.Room.realizedRoom)
            {
                parasite.room.RemoveObject(parasite);
                otherObject.Room.realizedRoom.AddObject(parasite);

                parasite.abstractCreature.Move(otherObject.pos);
            }
        }

        orig(self, eu);
    }
}
