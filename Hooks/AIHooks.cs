using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
