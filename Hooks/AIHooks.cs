using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ArchdruidsAdditions.Objects;

namespace ArchdruidsAdditions.Hooks;

public static class AIHooks
{
    internal static void ArtificialIntelligence_SetDestination(On.ArtificialIntelligence.orig_SetDestination orig, ArtificialIntelligence self, WorldCoordinate destination)
    {
        if (self is ScavengerAI scavAI)
        {
            foreach (Creature.Grasp grasp in scavAI.scavenger.grasps)
            {
                if (grasp != null && grasp.grabbed is not null && grasp.grabbed is Bow bow && bow.aiming)
                {
                    //return;
                }
            }
        }
        orig(self, destination);
    }
    internal static CreatureSpecificAImap[] RoomPreprocessor_DecompressStringToAImaps(On.RoomPreprocessor.orig_DecompressStringToAImaps orig, string s, AImap map)
    {
        /*
        UnityEngine.Debug.Log("");
        UnityEngine.Debug.Log(StaticWorld.preBakedPathingCreatures.Length);
        UnityEngine.Debug.Log("");*/
        return orig(s, map);
    }
    internal static PathCost AImap_TileCostForCreature(On.AImap.orig_TileCostForCreature_WorldCoordinate_CreatureTemplate orig, AImap self, WorldCoordinate pos, CreatureTemplate temp)
    {
        if (temp.type == Enums.CreatureTemplateType.Herring)
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

        return orig(self, pos, temp);
    }
}
