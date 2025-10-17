using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class StaticWorldHooks
{
    internal static void InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
    {
        List<CreatureTemplate> creatureList = [];
        List<TileTypeResistance> tileTypeResistances = [];
        List<TileConnectionResistance> tileConnectionResistances = [];

        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));

        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));

        CreatureTemplate herring = new CreatureTemplate(Enums.CreatureTemplateType.Herring, null, tileTypeResistances, tileConnectionResistances,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));

        herring.canFly = true;
        herring.AI = true;
        herring.requireAImap = true;
        herring.doPreBakedPathing = false;
        herring.smallCreature = true;
        herring.name = "Herring";

        CreatureTemplate flyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
        herring.preBakedPathingAncestor = flyTemplate;
        herring.relationships = flyTemplate.relationships;

        ExtEnumType type = ExtEnum<CreatureTemplate.Type>.values;
        foreach (string name in type.entries)
        {
            if (name == "Herring")
            {
                int index = type.entries.IndexOf(name);
                if (StaticWorld.creatureTemplates[index] == null)
                { StaticWorld.creatureTemplates[index] = herring; }
                else
                { Debug.Log("FAILED TO ADD HERRING TO STATICWORLD!"); }
            }
        }

        orig();
    }

    internal static void InitStaticWorldRelationships(On.StaticWorld.orig_InitStaticWorldRelationships orig)
    {
        orig();

        #region Herring

        #region Ignores
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Herring, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.EggBug, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.MirosBird, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.Slugcat, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaB, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.JetFish, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.LanternMouse, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        #endregion

        #endregion
    }

    internal static void InitStaticWorldRelationshipsMSC(On.StaticWorld.orig_InitStaticWorldRelationshipsMSC orig)
    {
        orig();

        #region Herring

        #region Ignores
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.FireBug, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        #endregion

        #endregion

    }

    internal static void InitStaticWorldRelationshipsWatcher(On.StaticWorld.orig_InitStaticWorldRelationshipsWatcher orig)
    {
        orig();

        #region Herring

        #region Ignores
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(Watcher.WatcherEnums.CreatureTemplateType.SandGrub, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
        StaticWorld.EstablishRelationship(Watcher.WatcherEnums.CreatureTemplateType.Loach, Enums.CreatureTemplateType.Herring,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
        #endregion

        #endregion

    }
}
