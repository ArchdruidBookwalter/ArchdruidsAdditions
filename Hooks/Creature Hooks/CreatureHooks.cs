using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class CreatureHooks
{
    #region Generic Body Part Hooks

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
            if (MiscData.boxHandlers[self.owner.owner.room].PositionInsideBox(self.pos, 0f, false))
            {
                return true;
            }
        }

        return baseValue;
    }
    internal static void BodyPart_PushOutOfTerrain(On.BodyPart.orig_PushOutOfTerrain orig, BodyPart self, Room room, Vector2 basePoint)
    {
        orig(self, room, basePoint);

        if (Data.MiscData.boxHandlers.ContainsKey(room))
        {
            if (MiscData.boxHandlers[room].TrySnapToCollisionBox(self.pos, self.lastPos, self.rad, out Vector2 snapPos, out _, out IntVector2 contactPoint, out CollisionBox box, null))
            {
                if (contactPoint.y != 0)
                {
                    self.terrainContact = true;
                    self.pos.y = snapPos.y;
                    if (Mathf.Sign(self.vel.y) == contactPoint.y)
                    {
                        self.vel.y = 0f;
                    }
                    self.vel.x *= self.surfaceFric;
                    
                    if (box != null)
                    { self.vel.x += box.vel.x; }
                }
                else if (contactPoint.x != 0)
                {
                    self.terrainContact = true;
                    self.pos.x = snapPos.x;
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
        Limb.Mode mode = self.mode;

        orig(self, room, attachedPos, searchFromPos, maximumRadiusFromAttachedPos, goalPos, forbiddenXDirs, forbiddenYDirs, behindWalls);

        Vector2 newGoalPos = attachedPos + Custom.DirVec(attachedPos, goalPos) * Mathf.Min(Custom.Dist(attachedPos, goalPos), maximumRadiusFromAttachedPos);
        if (MiscData.boxHandlers.ContainsKey(room) && MiscData.boxHandlers[room].TryGetSurfacePos(newGoalPos, self.rad, out Vector2 surfacePos, out Vector2 surfaceDir, out float surfacePosDist))
        {
            if (Custom.DistNoSqrt(goalPos, surfacePos) < Custom.DistNoSqrt(goalPos, self.absoluteHuntPos) && Custom.DistLess(surfacePos, attachedPos, maximumRadiusFromAttachedPos))
            {
                if (mode != Limb.Mode.HuntAbsolutePosition)
                { self.GrabbedTerrain(); }

                self.mode = Limb.Mode.HuntAbsolutePosition;
                self.absoluteHuntPos = surfacePos;
            }
        }

        Create_Square(room, attachedPos, 3f, 3f, Vec(45), Color.yellow, 0);
        Create_Square(room, newGoalPos, 3f, 3f, Vec(45), Color.yellow, 0);
        Create_LineBetweenTwoPoints(room, attachedPos, newGoalPos, 1f, Color.yellow, 0);

        if (mode == Limb.Mode.HuntAbsolutePosition)
        {
            Create_Square(room, attachedPos, 3f, 3f, Vec(45), Color.red, 0);
            Create_Square(room, self.absoluteHuntPos, 3f, 3f, Vec(45), Color.red, 0);
            Create_LineBetweenTwoPoints(room, attachedPos, self.absoluteHuntPos, 1f, Color.red, 0);
        }
    }
    
    #endregion

    //

    #region Generic Creature Hooks

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

    #endregion

    //

    internal static void OverseerTutorialBehavior_Update(On.OverseerTutorialBehavior.orig_Update orig, OverseerTutorialBehavior self)
    {
        if (ModManager.MMF && MMF.cfgExtraTutorials.Value == false)
        {
            return;
        }
        orig(self);
    }

    internal static void DaddyTentacle_StickToTerrain(On.DaddyTentacle.orig_StickToTerrain orig, DaddyTentacle self, Tentacle.TentacleChunk chunk)
    {
        int preSoundTimer = self.chunksStickSounds[chunk.tentacleIndex];

        bool snapToChunk = false;
        Vector2 snapPos = Vector2.zero;
        if (MiscData.boxHandlers.ContainsKey(self.owner.room) && (self.floatGrabDest == null || Custom.DistLess(chunk.pos, self.floatGrabDest.Value, 200f)))
        {
            if (MiscData.boxHandlers.ContainsKey(self.owner.room) && MiscData.boxHandlers[self.owner.room].TrySnapToCollisionBox(chunk.pos, chunk.lastPos, chunk.rad, out snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint, out CollisionBox box))
            {
                snapToChunk = true;
                self.chunksStickSounds[chunk.tentacleIndex] = 0;
            }
        }

        orig(self, chunk);

        if (snapToChunk)
        {
            chunk.vel.x += (snapPos.x - chunk.pos.x) * 0.1f;
            chunk.vel.y *= 0.9f;
            chunk.vel.y += (snapPos.y - chunk.pos.y) * 0.1f;
            chunk.vel.x *= 0.9f;

            self.chunksStickSounds[chunk.tentacleIndex] = preSoundTimer;

            if (self.chunksStickSounds[chunk.tentacleIndex] > 10)
            { self.owner.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, chunk.pos, Mathf.InverseLerp(self.tChunks.Length / 2, self.tChunks.Length - 1, chunk.tentacleIndex), 1f, self.owner.abstractPhysicalObject); }
            if (self.chunksStickSounds[chunk.tentacleIndex] > 0)
            { self.chunksStickSounds[chunk.tentacleIndex] = 0; }
            else
            { self.chunksStickSounds[chunk.tentacleIndex]--; }
        }
    }

    internal static void IL_VultureGraphics_ctor(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.GotoNext(MoveType.Before,
            x => x.MatchCallOrCallvirt(typeof(UnityEngine.Random), "get_value"),
            x => x.MatchLdcR4(0.5f),
            x => x.MatchBlt(out _),
            x => x.MatchLdcR4(8),
            x => x.MatchLdcR4(15),
            x => x.MatchCallOrCallvirt(typeof(UnityEngine.Random), "get_value"),
            x => x.MatchCallOrCallvirt(typeof(UnityEngine.Mathf), nameof(Mathf.Lerp)),
            x => x.MatchBr(out _),
            x => x.MatchLdcR4(40),
            x => x.MatchStloc(1)
            );

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((VultureGraphics self) =>
        {
            if (self.IsMiros)
            {
                self.feathersPerWing = 0;
            }
        });
    }
}
