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
            relationships = flyTemplate.relationships
        };

        tileTypeResistances.Clear();
        tileConnectionResistances.Clear();
        #endregion

        #region Parasite
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.CurvedFloor, 1f, PathCost.Legality.Allowed));

        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));

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
            grasps = 1
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

        #region CloudFish

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
                    float intensity = Mathf.Clamp(otherCreature.bodySize / 4, 0, 1);

                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Ignores, 0f));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Uncomfortable, 0f));
                }
                else
                {
                    StaticWorld.EstablishRelationship(otherCreature.type, cloudFishTemplate.type, new CreatureTemplate.Relationship(Ignores, 0f));
                    StaticWorld.EstablishRelationship(cloudFishTemplate.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, 0f));
                }
            }

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
        }

        /*
            
            #region This Creature

            #region Ignores
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            #endregion

            #region Uncomfortable
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.Leech,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
            #endregion

            #region Afraid
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.Slugcat,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.LizardTemplate,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.JetFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.CicadaA,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.CicadaB,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.BigSpider,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.DropBug,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));

            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.Vulture,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.BrotherLongLegs,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.DaddyLongLegs,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.MirosBird,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));

            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.PoleMimic,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, CreatureTemplate.Type.TentaclePlant,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            #endregion

            #endregion

            #region Other Creatures

            #region Ignores
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.EggBug, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigNeedleWorm, Enums.CreatureTemplateType.CloudFish,
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
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BlueLizard, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.JetFish, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaB, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigSpider, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.DropBug, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.TentaclePlant, Enums.CreatureTemplateType.CloudFish,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.PoleMimic, Enums.CreatureTemplateType.CloudFish,
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

        */

        #endregion




        #region Parasite

        /*

            #region This Creature

            #region Ignores
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, Enums.CreatureTemplateType.Parasite,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, Enums.CreatureTemplateType.CloudFish,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.Fly,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
                #endregion

                #region Eats
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.Slugcat,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.Scavenger,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.LizardTemplate,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.JetFish,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.CicadaA,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.CicadaB,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.BigSpider,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.DropBug,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));

                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.Vulture,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.MirosBird,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                #endregion

                #region Afraid
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.BrotherLongLegs,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.DaddyLongLegs,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));

                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.PoleMimic,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
                StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.Parasite, CreatureTemplate.Type.TentaclePlant,
                    new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
                #endregion

                #endregion

            #region Other Creatures

            #region Uncomfortable
            StaticWorld.EstablishRelationship(Enums.CreatureTemplateType.CloudFish, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Fly, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Leech, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
            #endregion

            #region Afraid
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Slugcat, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Scavenger, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.LizardTemplate, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BlueLizard, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.JetFish, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaB, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigSpider, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.DropBug, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Vulture, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            #endregion

            #region Eats
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BrotherLongLegs, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.DaddyLongLegs, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));

            StaticWorld.EstablishRelationship(CreatureTemplate.Type.TentaclePlant, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.PoleMimic, Enums.CreatureTemplateType.Parasite,
                new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            #endregion

            #endregion

        */

        #endregion



    }

    public static void EstablishGenericRelationships(CreatureTemplate thisCreature, params CreatureTemplate[] simCreatures)
    {
        CreatureTemplate.Relationship.Type Ignores = CreatureTemplate.Relationship.Type.Ignores;
        CreatureTemplate.Relationship.Type Uncomfortable = CreatureTemplate.Relationship.Type.Uncomfortable;
        CreatureTemplate.Relationship.Type Afraid = CreatureTemplate.Relationship.Type.Afraid;
        CreatureTemplate.Relationship.Type Attacks = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Antagonizes = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Rivals = CreatureTemplate.Relationship.Type.Attacks;
        CreatureTemplate.Relationship.Type Eats = CreatureTemplate.Relationship.Type.Eats;

        for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
        {
            CreatureTemplate otherCreature = StaticWorld.creatureTemplates[i];

            CreatureTemplate.Relationship.Type type = Ignores;
            float intensity = 0f;

            if (otherCreature.type != thisCreature.type)
            {
                float relationshipStrength = 0f;

                foreach (CreatureTemplate simCreature in simCreatures)
                {
                    CreatureTemplate.Relationship simRel = otherCreature.relationships[simCreature.index];

                    if (simRel.type == type)
                    {
                        if (simRel.intensity > intensity)
                        { intensity = simRel.intensity; }
                    }
                    else if (simRel.type == Eats)
                    {
                        relationshipStrength = 1f;
                        intensity = simRel.intensity;
                    }
                    else if (simRel.type == Attacks && relationshipStrength < 1f)
                    {
                        relationshipStrength = 0.75f;
                        intensity = simRel.intensity;
                    }
                    else if (simRel.type == Uncomfortable && relationshipStrength < 0.75f)
                    {
                        relationshipStrength = 0.50f;
                        intensity = simRel.intensity;
                    }
                }
            }

            if (type == Attacks || type == Eats)
            {
                StaticWorld.EstablishRelationship(otherCreature.type, thisCreature.type, new CreatureTemplate.Relationship(type, intensity));
                StaticWorld.EstablishRelationship(thisCreature.type, otherCreature.type, new CreatureTemplate.Relationship(Afraid, intensity));
            }
            else if (type == Uncomfortable || type == Antagonizes || type == Rivals)
            {
                StaticWorld.EstablishRelationship(otherCreature.type, thisCreature.type, new CreatureTemplate.Relationship(type, intensity));
                StaticWorld.EstablishRelationship(thisCreature.type, otherCreature.type, new CreatureTemplate.Relationship(type, intensity));
            }
            else
            {
                StaticWorld.EstablishRelationship(thisCreature.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, intensity));
                StaticWorld.EstablishRelationship(thisCreature.type, otherCreature.type, new CreatureTemplate.Relationship(Ignores, intensity));
            }
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
