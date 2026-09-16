using System.Collections.Generic;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PathFinderHooks
{
    internal static void PathFinder_ctor(On.PathFinder.orig_ctor orig, PathFinder self, ArtificialIntelligence AI, World world, AbstractCreature creature)
    {
        /*
        if (creature.creatureTemplate.type == Enums.CreatureTemplateType.Parasite)
        {
            self.visualize = true;
            self.visualizePath = true;
        }
        */

        orig(self, AI, world, creature);
    }
    internal static void PathFinder_Update(On.PathFinder.orig_Update orig, PathFinder self)
    {
        orig(self);

        if (self.visualizePath && self.realizedRoom != null)
        {
            bool foundVisualizer = false;
            foreach (UpdatableAndDeletable updel in self.realizedRoom.updateList)
            {
                if (updel is FollowPathVisualizer visualizer && visualizer.pathFinder == self)
                {
                    foundVisualizer = true;
                    break;
                }
            }

            if (!foundVisualizer)
            {
                self.Reset(self.realizedRoom);
            }
        }
    }
    internal static void PathFinder_SetDestination(On.PathFinder.orig_SetDestination orig, PathFinder self, WorldCoordinate destination)
    {
        orig(self, destination);
    }
    internal static void PathFinder_DestinationHasChanged(On.PathFinder.orig_DestinationHasChanged orig, PathFinder self, WorldCoordinate oldDestination, WorldCoordinate newDestination)
    {
        orig(self, oldDestination, newDestination);
    }
    internal static void PathFinder_AssignNewDestination(On.PathFinder.orig_AssignNewDestination orig, PathFinder self, WorldCoordinate newDestination)
    {
        orig(self, newDestination);
    }
    internal static PathCost PathFinder_CoordinateCost(On.PathFinder.orig_CoordinateCost orig, PathFinder self, WorldCoordinate coord)
    {
        PathCost cost = orig(self, coord);
        if (self.AI is CloudFishAI cloudFishAI)
        {
            if (self.AITileAtWorldCoordinate(coord).acc == AItile.Accessibility.Solid)
            {
                return new PathCost(0f, PathCost.Legality.IllegalTile);
            }
        }
        return cost;
    }
    internal static PathCost PathFinder_CheckConnectionCost(On.PathFinder.orig_CheckConnectionCost orig, PathFinder self, PathFinder.PathingCell start, PathFinder.PathingCell goal, MovementConnection connection, bool followingPath)
    {
        PathCost cost = orig(self, start, goal, connection, followingPath);
        if (self.AI is CloudFishAI cloudFishAI)
        {
            if (self.AITileAtWorldCoordinate(goal.worldCoordinate).acc == AItile.Accessibility.Solid)
            {
                return new PathCost(0f, PathCost.Legality.IllegalTile);
            }
        }
        return cost;
    }
    internal static List<WorldCoordinate> PathFinder_CreatePathForAbstractCreature(On.PathFinder.orig_CreatePathForAbstractreature orig, PathFinder self, WorldCoordinate searchDestination)
    {
        return orig(self, searchDestination);
    }

    internal static List<WorldCoordinate> AbstractSpacePathFinder_Path(On.AbstractSpacePathFinder.orig_Path orig, World world, WorldCoordinate start, WorldCoordinate goal, CreatureTemplate creatureType, IOwnAnAbstractSpacePathFinder owner)
    {
        return orig(world, start, goal, creatureType, owner);
    }
    internal static void AbstractSpacePathFinder_AddNode(On.AbstractSpacePathFinder.orig_AddNode orig, WorldCoordinate self, AbstractSpacePathFinder.Node parent, float cost, ref bool[,] alreadyChecked, ref List<AbstractSpacePathFinder.Node> checkNext, ref AbstractSpacePathFinder.Node foundStart, WorldCoordinate start, World world, IOwnAnAbstractSpacePathFinder owner)
    {
        orig(self, parent, cost, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
    }

    internal static void FollowPathVisualizer_Update(On.FollowPathVisualizer.orig_Update orig, FollowPathVisualizer self, bool eu)
    {
        orig(self, eu);
    }
    internal static void FollowPathVisualizer_DrawSprites(On.FollowPathVisualizer.orig_DrawSprites orig, FollowPathVisualizer self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
    }
    internal static MovementConnection StandardPather_FollowPath(On.StandardPather.orig_FollowPath orig, StandardPather self, WorldCoordinate originPos, bool actuallyFollowingThisPath)
    {
        return orig(self, originPos, actuallyFollowingThisPath);
    }
    internal static PathCost StandardPather_HeuristicForCell(On.StandardPather.orig_HeuristicForCell orig, StandardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
    {
        return orig(self, cell, costToGoal);
    }
    internal static void AbstractCreatureAI_SetDestination(On.AbstractCreatureAI.orig_SetDestination orig, AbstractCreatureAI self, WorldCoordinate destination)
    {
        orig(self, destination);
    }
    internal static void ArtificialIntelligence_SetDestination(On.ArtificialIntelligence.orig_SetDestination orig, ArtificialIntelligence self, WorldCoordinate destination)
    {
        orig(self, destination);
    }
    internal static int QuickConnectivity_Check(On.QuickConnectivity.orig_Check orig, Room room, CreatureTemplate template, IntVector2 start, IntVector2 goal, int maxGenerations)
    {
        return orig(room, template, start, goal, maxGenerations);
    }
}
