using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWDebug = UnityEngine.Debug;

namespace ArchdruidsAdditions.Hooks;

public static class StaticWorldHooks
{
    internal static void InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
    {
        List<CreatureTemplate> creatureList = [];
        List<TileTypeResistance> tileTypeResistances = [];
        List<TileConnectionResistance> tileConnectionResistances = [];

        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.OffScreen, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1f, PathCost.Legality.Allowed));
        tileTypeResistances.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2.5f, PathCost.Legality.Allowed));

        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Standard, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OpenDiagonal, 3f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 3f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.SemiDiagonalReach, 2f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToWater, 20f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 1.5f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.NPCTransportation, 25f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.OffScreenMovement, 1f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.BetweenRooms, 10f, PathCost.Legality.Allowed));
        tileConnectionResistances.Add(new TileConnectionResistance(MovementConnection.MovementType.Slope, 1.5f, PathCost.Legality.Allowed));

        CreatureTemplate herring = new CreatureTemplate(Enums.CreatureTemplateType.Herring, null, tileTypeResistances, tileConnectionResistances,
            new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));

        ExtEnumType type = ExtEnum<CreatureTemplate.Type>.values;
        foreach (string name in type.entries)
        {
            if (name == "Herring")
            {
                int index = type.entries.IndexOf(name);
                if (StaticWorld.creatureTemplates[index] == null)
                { StaticWorld.creatureTemplates[index] = herring; }
                else
                { RWDebug.Log("FAILED TO ADD HERRING TO STATICWORLD!"); }
            }
        }

        orig();
    }
}
