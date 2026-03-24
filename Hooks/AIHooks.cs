using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArchdruidsAdditions.Objects;
using static ArchdruidsAdditions.Methods.Methods;
using RWCustom;
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
        if (temp.type == Enums.CreatureTemplateType.CloudFish)
        {
            PathCost cost = orig(self, pos, temp);
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

        return orig(self, pos, temp);
    }

    internal static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
    {
        orig(self);

        AIModule highestModule = self.utilityComparer.HighestUtilityModule();
        float highestModuleAmount = self.utilityComparer.HighestUtility();

        if (highestModule != null)
        {
            List<string> lines = [];
            lines.Add(highestModule.GetType().Name);

            if (highestModule is PreyTracker)
            {
                lines.Add("PREY: " + self.preyTracker.prey.Count.ToString());
                lines.Add("VIS RAD: " + self.creature.creatureTemplate.visualRadius.ToString());
            }
            Create_TextBlock(self.lizard.room, self.lizard.firstChunk.pos, 1, lines.ToArray(), "Red", 0);
        }
    }
}
