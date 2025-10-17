using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Creatures;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractCreatureHooks
{
    internal static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
    {
        if (self.Room != null && self.realizedCreature == null)
        {
            if (self.creatureTemplate.type == Enums.CreatureTemplateType.Herring)
            {
                self.realizedCreature = new Herring(self, self.world);
                self.InitiateAI();
            }
        }

        orig(self);
    }
    internal static void AbstractCreature_InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
    {
        if (self.creatureTemplate.type == Enums.CreatureTemplateType.Herring)
        {
            /*
            Debug.Log("");
            Debug.Log("AbstractCreature_InitiateAI Method was called by Herring.");
            Debug.Log("");
            */
            self.abstractAI.RealAI = new HerringAI(self, self.world);
        }
        orig(self);
    }
}
