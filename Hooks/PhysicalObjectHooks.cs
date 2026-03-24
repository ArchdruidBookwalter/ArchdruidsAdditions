using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArchdruidsAdditions.Methods.Methods;
using UnityEngine;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PhysicalObjectHooks
{
    internal static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {
        if (self is Parasite parasite && parasite.buriedInChunk != null)
        {
            AbstractPhysicalObject otherObject = parasite.abstractPhysicalObject.stuckObjects[0].B;
            if (parasite.room != null && otherObject.Room.realizedRoom != null && parasite.room != otherObject.Room.realizedRoom)
            {
                Debug.Log("TELEPORTED PARASITE TO NEW ROOM.");

                parasite.room.RemoveObject(parasite);
                otherObject.Room.realizedRoom.AddObject(parasite);

                parasite.abstractCreature.Move(otherObject.pos);
            }
        }

        orig(self, eu);

        /*
        if (self.room != null)
        {
            foreach (BodyChunk chunk in self.bodyChunks)
            {
                Create_Square(self.room, chunk.pos, chunk.rad * 2f, chunk.rad * 2f, Vector2.up, "Red", 0);
            }
            foreach (PhysicalObject.BodyChunkConnection connection in self.bodyChunkConnections)
            {
                string color = "Red";
                switch (connection.type.value)
                {
                    case "Normal":
                        color = "Red";
                        break;
                    case "Push":
                        color = "Blue";
                        break;
                    case "Pull":
                        color = "Purple";
                        break;
                }

                Create_LineBetweenTwoPoints(self.room, connection.chunk1.pos, connection.chunk2.pos, color, 0);
            }
        }*/
    }
}
