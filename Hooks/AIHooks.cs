using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class AIHooks
{
    internal static void ArtificialIntelligence_SetDestination(On.ArtificialIntelligence.orig_SetDestination orig, ArtificialIntelligence self, WorldCoordinate destination)
    {
        orig(self, destination);
    }
    internal static CreatureSpecificAImap[] RoomPreprocessor_DecompressStringToAImaps(On.RoomPreprocessor.orig_DecompressStringToAImaps orig, string s, AImap map)
    {
        return orig(s, map);
    }
    internal static PathCost AImap_TileCostForCreature(On.AImap.orig_TileCostForCreature_WorldCoordinate_CreatureTemplate orig, AImap self, WorldCoordinate pos, CreatureTemplate temp)
    {
        PathCost cost = orig(self, pos, temp);

        if (temp.type == Enums.CreatureTemplateType.CloudFish)
        {
            int terrainProximity = self.getTerrainProximity(pos);

            if (terrainProximity < 5)
            {
                int resistance = 5 - terrainProximity;
                cost.resistance += 10 * resistance;
            }

            return cost;
        }

        if (self.getAItile(pos).acc == AItile.Accessibility.Solid)
        {
            return new PathCost(0f, PathCost.Legality.IllegalTile);
        }

        return cost;
    }
    internal static void DynamicRelationship_Update(On.RelationshipTracker.DynamicRelationship.orig_Update orig, RelationshipTracker.DynamicRelationship self)
    {
        orig(self);

        CreatureTemplate.Relationship parasiteRelationship = self.rt.AI.creature.creatureTemplate.relationships[StaticWorld.GetCreatureTemplate("Parasite").index];

        if (parasiteRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
        {
            bool creatureIsParasite = false;
            bool shouldBeAfraid = false;

            AbstractCreature repCreature = self.trackerRep.representedCreature;

            if (repCreature.creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
            {
                creatureIsParasite = true;
                if (repCreature.realizedCreature != null && (repCreature.realizedCreature as Parasite).buriedInChunk == null)
                {
                    shouldBeAfraid = true;
                }
            }
            else if (repCreature.creatureTemplate.type != Enums.CreatureTemplateType.Parasite && self.trackerRep.representedCreature.stuckObjects.Count > 0)
            {
                foreach (AbstractPhysicalObject.AbstractObjectStick stick in self.trackerRep.representedCreature.stuckObjects)
                {
                    if (stick is AbstractParasiteStick || stick is AbstractParasiteEggStick)
                    {
                        shouldBeAfraid = true;
                    }
                }
            }

            if (shouldBeAfraid)
            {
                if (self.currentRelationship != parasiteRelationship)
                {
                    self.rt.SortCreatureIntoModule(self, parasiteRelationship);

                    self.trackerRep.priority = self.currentRelationship.intensity * self.trackedByModuleWeigth;
                    self.currentRelationship = parasiteRelationship;
                }
            }
            else if (creatureIsParasite)
            {
                CreatureTemplate.Relationship ignore = new(CreatureTemplate.Relationship.Type.Ignores, 0f);

                if (self.currentRelationship != ignore)
                {
                    self.rt.SortCreatureIntoModule(self, parasiteRelationship);

                    self.trackerRep.priority = self.currentRelationship.intensity * self.trackedByModuleWeigth;
                    self.currentRelationship = parasiteRelationship;
                }
            }

            if (repCreature.realizedCreature != null)
            {
                Room room = repCreature.realizedCreature.room;
                Vector2 pos = repCreature.realizedCreature.mainBodyChunk.pos;

                if (shouldBeAfraid)
                {
                    //Create_Square(room, repCreature.realizedCreature.mainBodyChunk.pos, 20f, 20f, Vec(45), "Red", 0);
                }
                else if (creatureIsParasite)
                {
                    //Create_Square(room, repCreature.realizedCreature.mainBodyChunk.pos, 20f, 20f, Vec(45), "Green", 0);
                }
            }
        }
    }

    internal static void ArtificialIntelligence_Update(On.ArtificialIntelligence.orig_Update orig, ArtificialIntelligence self)
    {
        orig(self);
    }
    internal static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
    {
        orig(self);
    }
    internal static void VultureAI_Update(On.VultureAI.orig_Update orig, VultureAI self)
    {
        orig(self);

        if (self.vulture.room != null && self.relationshipTracker != null)
        {
            foreach (RelationshipTracker.DynamicRelationship relationship in self.relationshipTracker.relationships)
            {
                if (relationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    Vector2 bestGuessForPosition = self.vulture.room.MiddleOfTile(relationship.trackerRep.BestGuessForPosition());
                    self.disencouraged += 1f / Custom.Dist(self.vulture.mainBodyChunk.pos, bestGuessForPosition) * relationship.currentRelationship.intensity * 100;

                    if (self.disencouraged > 10)
                    {
                        self.behavior = VultureAI.Behavior.Disencouraged;
                    }
                }
            }
        }
    }
    internal static bool VultureAI_OnlyHurtDontGrab(On.VultureAI.orig_OnlyHurtDontGrab orig, VultureAI self, PhysicalObject testObj)
    {
        bool baseGrab = orig(self, testObj);

        if (testObj is Creature creature)
        {
            Tracker.CreatureRepresentation creatureRep = self.tracker.RepresentationForCreature(creature.abstractCreature, false);
            if (creatureRep != null && creatureRep.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Afraid)
            {
                return true;
            }
        }

        return baseGrab;
    }
    internal static bool VultureAI_DoIWantToBiteCreature(On.VultureAI.orig_DoIWantToBiteCreature orig, VultureAI self, AbstractCreature creature)
    {
        bool baseResult = orig(self, creature);

        if (creature.stuckObjects.Count > 0)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in creature.stuckObjects)
            {
                if (stick is AbstractParasiteStick || stick is AbstractParasiteEggStick)
                {
                    return false;
                }
            }
        }

        return baseResult;
    }
    internal static bool MirosBirdAI_DoIWantToBiteCreature(On.MirosBirdAI.orig_DoIWantToBiteCreature orig, MirosBirdAI self, AbstractCreature creature)
    {
        bool baseResult = orig(self, creature); 

        if (creature.stuckObjects.Count > 0)
        {
            foreach (AbstractPhysicalObject.AbstractObjectStick stick in creature.stuckObjects)
            {
                if (stick is AbstractParasiteStick || stick is AbstractParasiteEggStick)
                {
                    return false;
                }
            }
        }

        return baseResult;
    }
}
