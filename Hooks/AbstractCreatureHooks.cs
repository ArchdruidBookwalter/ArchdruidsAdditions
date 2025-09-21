using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Creatures;

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
            }
        }

        orig.Invoke(self);
    }
}
