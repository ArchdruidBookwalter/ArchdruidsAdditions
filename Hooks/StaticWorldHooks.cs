using System.Collections.Generic;
using System.Linq;

namespace ArchdruidsAdditions.Hooks;

public static class StaticWorldHooks
{
    internal static void InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
    {
        CreatureTemplate flyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
        CreatureTemplate leechTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech);
        CreatureTemplate eggBugTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.EggBug);
        CreatureTemplate rotTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs);
        CreatureTemplate lizardTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
        CreatureTemplate cicadaTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA);

        List<TileTypeResistance> tileTypeResistances = [];
        List<TileConnectionResistance> tileConnectionResistances = [];

        #region CloudFish
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Air, 1f, PathCost.Legality.Allowed));

        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 10f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));

        CreatureTemplate cloudFish = new CreatureTemplate(Enums.CreatureTemplateType.CloudFish, null, tileTypeResistances, tileConnectionResistances,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
        {
            bodySize = 0.4f,
            canFly = true,
            AI = true,
            requireAImap = true,
            doPreBakedPathing = false,
            smallCreature = false,
            name = "CloudFish",
            preBakedPathingAncestor = flyTemplate,
            relationships = flyTemplate.relationships,
            canSwim = true
        };

        tileTypeResistances.Clear();
        tileConnectionResistances.Clear();
        #endregion

        #region Parasite
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Unallowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Climb, 1f, PathCost.Legality.Unallowed));

        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));

        CreatureTemplate parasite = new(Enums.CreatureTemplateType.Parasite, null, tileTypeResistances, tileConnectionResistances,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f))
        {
            bodySize = 0.1f,
            AI = true,
            requireAImap = true,
            doPreBakedPathing = false,
            smallCreature = true,
            name = "Parasite",
            preBakedPathingAncestor = lizardTemplate,
            visualRadius = 2000,
            socialMemory = true,
            grasps = 1,
            waterRelationship = CreatureTemplate.WaterRelationship.Amphibious,
            waterPathingResistance = 0.1f,
            canSwim = true
        };

        tileTypeResistances.Clear();
        tileConnectionResistances.Clear();
        #endregion

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
            if (name == "Parasite")
            {
                int index = type.entries.IndexOf(name);
                if (StaticWorld.creatureTemplates[index] == null)
                { StaticWorld.creatureTemplates[index] = parasite; }
                else
                { Debug.Log("FAILED TO ADD PARASITE TO STATICWORLD!"); }
            }
        }

        orig();
    }

    internal static void InitStaticWorldRelationships(On.StaticWorld.orig_InitStaticWorldRelationships orig)
    {
        orig();

        CreatureTemplate flyTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly);
        CreatureTemplate cicadaTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA);
        CreatureTemplate smallNeedleTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm);
        CreatureTemplate leechTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Leech);
        CreatureTemplate rotTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs);

        CreatureTemplate.Relationship.Type Ignores = CreatureTemplate.Relationship.Type.Ignores;
        CreatureTemplate.Relationship.Type Uncomfortable = CreatureTemplate.Relationship.Type.Uncomfortable;
        CreatureTemplate.Relationship.Type Afraid = CreatureTemplate.Relationship.Type.Afraid;
        CreatureTemplate.Relationship.Type Attacks = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Antagonizes = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Rivals = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Eats = CreatureTemplate.Relationship.Type.Eats;

        CreatureTemplate cloudFishTemplate = StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.CloudFish);
        CreatureTemplate parasiteTemplate = StaticWorld.GetCreatureTemplate(Enums.CreatureTemplateType.Parasite);
        CreatureTemplate[] newTemplates = [cloudFishTemplate, parasiteTemplate];

        for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
        {
            CreatureTemplate otherCreature = StaticWorld.creatureTemplates[i];

            if (newTemplates.Contains(otherCreature))
            {
                StaticWorld.EstablishRelationship(otherCreature.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, 0f));
            }

            CreatureTemplate.Relationship flyRelationship = otherCreature.relationships[flyTemplate.index];
            CreatureTemplate.Relationship cicadaRelationship = otherCreature.relationships[cicadaTemplate.index];

            #region Cloudfish
            if (otherCreature.type != cloudFishTemplate.type)
            {
                if (flyRelationship.type == Eats || cicadaRelationship.type == Eats)
                {
                    float intensity = Mathf.Clamp(1 / otherCreature.bodySize, 0, 1);

                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Eats, intensity));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Afraid, intensity));
                }
                else if (otherCreature.bodySize > 0.5)
                {
                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Ignores, 0f));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Uncomfortable, 0f));
                }
                else
                {
                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Ignores, 0f));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, 0f));
                }
            }
            #endregion

            #region Parasite
            if (otherCreature.type != parasiteTemplate.type)
            {
                if (otherCreature.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
                {
                    float intensity = Mathf.Clamp(1 / otherCreature.bodySize, 0, 1);

                    StaticWorld.EstablishRelationship(otherCreature.type, parasiteTemplate.type, new CreatureTemplate.Relationship(Eats, intensity));
                    StaticWorld.EstablishRelationship(parasiteTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Afraid, intensity));
                }
                else if (otherCreature.type == CreatureTemplate.Type.Overseer)
                {
                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Ignores, 0f));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, 0f));
                }
                else if (otherCreature.bodySize > 0.5)
                {
                    float intensity = Mathf.Clamp(otherCreature.bodySize / 4, 0, 1);

                    StaticWorld.EstablishRelationship(otherCreature.type, parasiteTemplate.type, new CreatureTemplate.Relationship(Afraid, intensity));
                    StaticWorld.EstablishRelationship(parasiteTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Eats, intensity));
                }
                else
                {
                    StaticWorld.EstablishRelationship(otherCreature.type, parasiteTemplate.type, new CreatureTemplate.Relationship(Uncomfortable, 0f));
                    StaticWorld.EstablishRelationship(parasiteTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, 0f));
                }
            }
            #endregion
        }
    }

    internal static void InitStaticWorldRelationshipsMSC(On.StaticWorld.orig_InitStaticWorldRelationshipsMSC orig)
    {
        orig();

        #region CloudFish - This Creature

        #region Afraid
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
        #endregion

        #endregion

        #region CloudFish - Other Creatures

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

        #region Parasite - This Creature

        #region Eats
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.FireBug,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, DLCSharedEnums.CreatureTemplateType.Yeek,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, DLCSharedEnums.CreatureTemplateType.ZoopLizard,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, DLCSharedEnums.CreatureTemplateType.MirosVulture,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TrainLizard,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
        #endregion

        #endregion

        #region Parasite - Other Creatures

        #region Afraid
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.FireBug, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.Yeek, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ZoopLizard, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.MirosVulture, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        StaticWorld.EstablishRelationship(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, Enums.CreatureTemplateType.Parasite,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
        #endregion

        #endregion

    }

    internal static void InitStaticWorldRelationshipsWatcher(On.StaticWorld.orig_InitStaticWorldRelationshipsWatcher orig)
    {
        orig();

        #region CloudFish - This Creature

        #region Afraid
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, Watcher.WatcherEnums.CreatureTemplateType.SandGrub,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
        StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, Watcher.WatcherEnums.CreatureTemplateType.Loach,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
        #endregion

        #endregion

        #region CloudFish - Other Creatures

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
