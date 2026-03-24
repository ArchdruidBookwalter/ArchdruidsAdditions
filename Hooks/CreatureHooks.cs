using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ArchdruidsAdditions.Methods.Methods;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class CreatureHooks
{
    internal static void TailSegment_ctor(On.TailSegment.orig_ctor orig, TailSegment self,
        GraphicsModule module, float radius, float connectionRadius, TailSegment connectedSegment, float surfaceFriction, float airFriction, float affectPrevious, bool pullInPreviousPos)
    {
        if (module is CloudFishGraphics || module is ParasiteGraphics)
        {
            self.owner = module;
            self.rad = radius;
            self.connectionRad = connectionRadius;
            self.connectedSegment = connectedSegment;
            self.surfaceFric = surfaceFriction;
            self.airFriction = airFriction;
            self.affectPrevious = affectPrevious;
            self.pullInPreviousPosition = pullInPreviousPos;
            self.connectedPoint = null;
            self.Reset(module.owner.firstChunk.pos);
        }
        else
        {
            orig(self, module, radius, connectionRadius, connectedSegment, surfaceFriction, airFriction, affectPrevious, pullInPreviousPos);
        }
    }

    internal static void CreatureState_LoadFromString(On.CreatureState.orig_LoadFromString orig, CreatureState self, string[] s)
    {
        orig(self, s);
    }

    internal static void Creature_ctor(On.Creature.orig_ctor orig, Creature self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);

        /*
        CreatureTemplate thisCreatureTemplate = creature.creatureTemplate;

        Debug.Log("");
        Debug.Log(creature.creatureTemplate.name.ToUpper() + " RELATIONSHIPS: ");

        Debug.Log("|Creature Name                 |My Relationship              |This Creature's Relationship |");

        for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
        {
            CreatureTemplate otherCreatureTemplate = StaticWorld.creatureTemplates[i];

            CreatureTemplate.Relationship otherCreaturesRelationship = otherCreatureTemplate.relationships[thisCreatureTemplate.index];
            CreatureTemplate.Relationship thisCreaturesRelationship = thisCreatureTemplate.relationships[otherCreatureTemplate.index];

            string templateName = string.Format("{0,-30}", otherCreatureTemplate.name);
            string otherRel = string.Format("{0,-21} {1,7:N1}", otherCreaturesRelationship.type.value, otherCreaturesRelationship.intensity);
            string thisRel = string.Format("{0,-21} {1,7:N1}", thisCreaturesRelationship.type.value, thisCreaturesRelationship.intensity);

            Debug.Log("|" + templateName + "|" + thisRel + "|" + otherRel + "|");
        }

        Debug.Log("");
        */
    }

    internal static void Creature_Update(On.Creature.orig_Update orig, Creature self, bool eu)
    {
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
