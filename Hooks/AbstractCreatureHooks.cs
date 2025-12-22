using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Creatures;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArchdruidsAdditions.Hooks;

public static class AbstractCreatureHooks
{
    internal static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
    {
        if (self.Room != null && self.realizedCreature == null)
        {
            if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
            {
                self.realizedCreature = new CloudFish(self, self.world);
                self.InitiateAI();
            }
        }

        orig(self);
    }
    internal static void AbstractCreature_InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
    {
        if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            CloudFishAI newAI = new(self, self.world);
            self.abstractAI.RealAI = newAI;
        }
        orig(self);
    }
    internal static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            self.abstractAI = new CloudFishAbstractAI(world, self);
            if (self.InDen)
            {

            }
        }
    }
    internal static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
    {
        if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            if (self.state.alive && self.Room != null && self.realizedCreature == null)
            {
                CloudFishAbstractAI abstractAI = self.abstractAI as CloudFishAbstractAI;
            }
        }
        orig(self, time);
    }

    internal static void AbstractCreature_InDenUpdate(On.AbstractCreature.orig_InDenUpdate orig, AbstractCreature self, int time)
    {
        if (self.creatureTemplate.type == Enums.CreatureTemplateType.CloudFish)
        {
            if (self.state.alive && self.Room != null && self.realizedCreature == null)
            {
                CloudFishAbstractAI abstractAI = self.abstractAI as CloudFishAbstractAI;
            }
        }
        orig(self, time);
    }
}
