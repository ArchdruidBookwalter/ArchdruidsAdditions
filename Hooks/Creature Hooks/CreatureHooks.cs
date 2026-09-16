using System;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.Physical_Objects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using Watcher;

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

    internal static bool BodyPart_OnOtherSideOfTerrain(On.BodyPart.orig_OnOtherSideOfTerrain orig, BodyPart self, Vector2 conPos, float minAffectRadius)
    {
        bool baseValue = orig(self, conPos, minAffectRadius);

        if (!baseValue && MiscData.boxHandlers.ContainsKey(self.owner.owner.room))
        {
            foreach (CollisionBox box in MiscData.boxHandlers[self.owner.owner.room].collisionBoxes)
            {
                if (box.Contains(self.pos, 0f, false))
                {
                    return true;
                }    
            }
        }

        return baseValue;
    }

    internal static void BodyPart_PushOutOfTerrain(On.BodyPart.orig_PushOutOfTerrain orig, BodyPart self, Room room, Vector2 basePoint)
    {
        orig(self, room, basePoint);

        if (Data.MiscData.boxHandlers.ContainsKey(room))
        {
            foreach (CollisionBox box in MiscData.boxHandlers[room].collisionBoxes)
            {
                box.GetSnapPosAndContact(self.pos, self.rad, out Vector2 newPos, out IntVector2 contactPoint, false);

                if (contactPoint.y < 0 || contactPoint.y > 0)
                {
                    self.terrainContact = true;
                    self.pos.y = newPos.y;
                    if (Mathf.Sign(self.vel.y) == contactPoint.y)
                    {
                        self.vel.y = 0f;
                    }
                    self.vel.x *= self.surfaceFric;
                }
                else if (contactPoint.x < 0 || contactPoint.x > 0)
                {
                    self.terrainContact = true;
                    self.pos.x = newPos.x;
                    if (Mathf.Sign(self.vel.x) == contactPoint.x)
                    {
                        self.vel.x = 0f;
                    }
                    self.vel.y *= self.surfaceFric;
                }
            }
        }
        
    }

    internal static void Limb_FindGrip(On.Limb.orig_FindGrip orig, Limb self, Room room, Vector2 attachedPos, Vector2 searchFromPos, float maximumRadiusFromAttachedPos, Vector2 goalPos, int forbiddenXDirs, int forbiddenYDirs, bool behindWalls)
    {
        orig(self, room, attachedPos, searchFromPos, maximumRadiusFromAttachedPos, goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls);

        if (MiscData.boxHandlers.ContainsKey(room))
        {
            Vector2 closestBoxPos = new Vector2(-10000, -10000);
            foreach (CollisionBox box in MiscData.boxHandlers[room].collisionBoxes)
            {
                box.GetSnapPosAndContact(searchFromPos, 0f, out Vector2 closestPoint, out _, false);

                if (Custom.DistNoSqrt(goalPos, closestPoint) < Custom.DistNoSqrt(goalPos, closestBoxPos) && Custom.DistLess(attachedPos, closestPoint, maximumRadiusFromAttachedPos))
                {
                    closestBoxPos = closestPoint;
                }
            }

            if (closestBoxPos.x != -10000 && closestBoxPos.y != -10000)
            {
                if (self.mode != Limb.Mode.HuntAbsolutePosition || Custom.DistNoSqrt(self.absoluteHuntPos, goalPos) > Custom.DistNoSqrt(closestBoxPos, goalPos))
                {
                    self.absoluteHuntPos = closestBoxPos;

                    if (self.mode != Limb.Mode.HuntAbsolutePosition)
                    {
                        self.mode = Limb.Mode.HuntAbsolutePosition;
                        self.GrabbedTerrain();
                    }
                }
            }
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
            foreach (firstChunk chunk in self.bodyChunks)
            {
                Create_Square(self.room, chunk.segPos, chunk.rad * 2f, chunk.rad * 2f, Vector2.up, "Red", 0);
            }
            foreach (PhysicalObject.BodyChunkConnection connection in self.bodyChunkConnections)
            {
                string shellColor = "Red";
                switch (connection.type.value)
                {
                    case "Normal":
                        shellColor = "Red";
                        break;
                    case "Push":
                        shellColor = "Blue";
                        break;
                    case "Pull":
                        shellColor = "Purple";
                        break;
                }

                Create_LineBetweenTwoPoints(self.room, connection.chunk1.segPos, connection.chunk2.segPos, shellColor, 0);
            }
        }*/
    }

    internal static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability sh, float dom, bool ovr, bool pas)
    {
        //LogMethodStart("CREATURE_GRASP");

        bool value = orig(self, obj, graspUsed, chunkGrabbed, sh, dom, ovr, pas);

        //LogMethodEnd();

        return value;
    }

    internal static void Creature_ReleaseGrab(On.Creature.orig_ReleaseGrasp orig, Creature self, int grasp)
    {
        //LogMethodStart("CREATURE_RELEASEGRASP");

        orig(self, grasp);

        //LogMethodEnd();
    }

    internal static void Barnacle_Collide(On.Watcher.Barnacle.orig_Collide orig, Watcher.Barnacle self, PhysicalObject otherObj, int myChunk, int otherChunk)
    {
        if (otherObj is MimicCrab)
        {
            return;
        }

        orig(self, otherObj, myChunk, otherChunk);
    }
}
