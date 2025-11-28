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
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.SideHighway, 1f, PathCost.Legality.Allowed));

        CreatureTemplate cloudFish = new CreatureTemplate(Enums.CreatureTemplateType.CloudFish, null, tileTypeResistances, tileConnectionResistances,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));

        cloudFish.canFly = true;
        cloudFish.AI = true;
        cloudFish.requireAImap = true;
        cloudFish.doPreBakedPathing = false;
        cloudFish.smallCreature = true;
        cloudFish.name = "CloudFish";

        CreatureTemplate flyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
        cloudFish.preBakedPathingAncestor = flyTemplate;
        cloudFish.relationships = flyTemplate.relationships;

        ExtEnumType type = ExtEnum<CreatureTemplate.Type>.values;
        foreach (string name in type.entries)
        {
            if (name == "CloudFish")
            {
                int index = type.entries.IndexOf(name);
                if (StaticWorld.creatureTemplates[index] == null)
                { StaticWorld.creatureTemplates[index] = cloudFish; }
                else
                { Debug.Log("FAILED TO ADD CLOUDFISH TO STATICWORLD!"); }
            }
        }

        orig();
    }

    internal static void InitStaticWorldRelationships(On.StaticWorld.orig_InitStaticWorldRelationships orig)
    {
        orig();

        #region CloudFish

        #region Ignores
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.EggBug, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaB, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.LanternMouse, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.Slugcat, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.Leech, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));

        StaticWorld.EstablishRelationship(CreatureTemplate.Type.LizardTemplate, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.BlueLizard, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.JetFish, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigSpider, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.DropBug, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));

        StaticWorld.EstablishRelationship(CreatureTemplate.Type.RedLizard, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.Vulture, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        StaticWorld.EstablishRelationship(CreatureTemplate.Type.MirosBird, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        #endregion

        #endregion
    }

    internal static void InitStaticWorldRelationshipsMSC(On.StaticWorld.orig_InitStaticWorldRelationshipsMSC orig)
    {
        orig();

        #region CloudFish

        #region Ignores
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.FireBug, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        #endregion

        #endregion

    }

    internal static void InitStaticWorldRelationshipsWatcher(On.StaticWorld.orig_InitStaticWorldRelationshipsWatcher orig)
    {
        orig();

        #region CloudFish

        #region Ignores
        #endregion

        #region Eats
        StaticWorld.EstablishRelationship(Watcher.WatcherEnums.CreatureTemplateType.SandGrub, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 2f));
        StaticWorld.EstablishRelationship(Watcher.WatcherEnums.CreatureTemplateType.Loach, Enums.CreatureTemplateType.CloudFish,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
        #endregion

        #endregion

    }
}
