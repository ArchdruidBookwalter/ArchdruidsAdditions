using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Newtonsoft.Json.Linq;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

public static class PathFinderHooks
{
    internal static void PathFinder_ctor(On.PathFinder.orig_ctor orig, PathFinder self, ArtificialIntelligence AI, World world, AbstractCreature creature)
    {
        if (creature.creatureTemplate.type == CreatureTemplate.Type.CicadaA)
        {
            self.visualizePath = true;
        }
        orig(self, AI, world, creature);
    }
    internal static void PathFinder_Update(On.PathFinder.orig_Update orig, PathFinder self)
    {
        orig(self);
    }
    internal static PathCost PathFinder_CheckConnectionCost(On.PathFinder.orig_CheckConnectionCost orig, PathFinder self, PathFinder.PathingCell start, PathFinder.PathingCell goal, MovementConnection connection, bool followingPath)
    {
        /*
        if (self.creature != null)
        {
            Debug.Log(self.creature.creatureTemplate.type.value);
        }
        Room room = self.realizedRoom;
        CheckIfNull(room, "ROOM");
        */
        return orig(self, start, goal, connection, followingPath);
    }
    internal static void FollowPathVisualizer_Update(On.FollowPathVisualizer.orig_Update orig, FollowPathVisualizer self, bool eu)
    {
        //self.room.AddObject(new Objects.ColoredShapes.Rectangle(self.room, self.creature.mainBodyChunk.pos, 10f, 10f, 45f, "Blue", 0));
        //Debug.Log(self.creature.abstractCreature.creatureTemplate.type.value);

        orig(self, eu);
    }
    internal static List<WorldCoordinate> AbstractSpacePathFinder_Path(On.AbstractSpacePathFinder.orig_Path orig,
        World world, WorldCoordinate start, WorldCoordinate goal, CreatureTemplate creatureType, IOwnAnAbstractSpacePathFinder owner)
    {
        if (creatureType.type == Enums.CreatureTemplateType.Herring)
        {
            /*
            AbstractRoom room1 = world.GetAbstractRoom(start);
            if (room1.realizedRoom != null)
            {
                room1.realizedRoom.AddObject(new Objects.ColoredShapes.Rectangle(room1.realizedRoom, room1.realizedRoom.MiddleOfTile(start), 5f, 5f, 45f, "Red", 10));
            }

            AbstractRoom room2 = world.GetAbstractRoom(goal);
            if (room2.realizedRoom != null)
            {
                room2.realizedRoom.AddObject(new Objects.ColoredShapes.Rectangle(room2.realizedRoom, room2.realizedRoom.MiddleOfTile(goal), 5f, 5f, 45f, "Green", 10));
            }

            /*
            Debug.Log("Start Node: " + start.abstractNode);
            Debug.Log("Goal Node: " + goal.abstractNode);
            Debug.Log("Creature PreBakedPathingIndex: " + creatureType.prebakedPathingIndex);
            Debug.Log("Connectivity: " + (float)room1.ConnectivityCost(start.abstractNode, goal.abstractNode, creatureType));

            /*
            Debug.Log("");
            Debug.Log("------------");
            Debug.Log("METHOD: \'AbstractSpacePathFinder_Path\' WAS CALLED.");
            Debug.Log("");
            Debug.Log("Creature Type: " + creatureType.type.value);
            Debug.Log("Start Coordinate: " + start + " Type: " + world.GetNode(start).type.value);
            Debug.Log("End Coordinate: " + goal + " Type: " + world.GetNode(start).type.value);
            Debug.Log("");
            Debug.Log("Creature Mapped Node Types: ");

            ExtEnumType type = ExtEnum<AbstractRoomNode.Type>.values;
            for (int i = 0; i < creatureType.mappedNodeTypes.Length; i++)
            {
                Debug.Log(i + " - " + type.GetEntry(i) + ": " + creatureType.mappedNodeTypes[i]);
            }

            Debug.Log("------------");
            Debug.Log("");
            */
        }

        return orig(world, start, goal, creatureType, owner);
    }
    internal static void AbstractSpacePathFinder_AddNode(On.AbstractSpacePathFinder.orig_AddNode orig, WorldCoordinate self,
        AbstractSpacePathFinder.Node parent, float cost, ref bool[,] alreadyChecked, ref List<AbstractSpacePathFinder.Node> checkNext,
        ref AbstractSpacePathFinder.Node foundStart, WorldCoordinate start, World world, IOwnAnAbstractSpacePathFinder owner)
    {
        /*
        Debug.Log("");
        Debug.Log("------------");
        Debug.Log("METHOD: \'AbstractSpacePathFinder_Path\' WAS CALLED.");
        Debug.Log("------------");
        Debug.Log("");
        */
        orig(self, parent, cost, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
    }
    internal static MovementConnection StandardPather_FollowPath(On.StandardPather.orig_FollowPath orig, StandardPather self, WorldCoordinate originPos, bool actuallyFollowingThisPath)
    {
        return orig(self, originPos, actuallyFollowingThisPath);
    }
    internal static PathCost StandardPather_HeuristicForCell(On.StandardPather.orig_HeuristicForCell orig, StandardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
    {
        if (self.creatureType.type == Enums.CreatureTemplateType.Herring && self.InThisRealizedRoom(cell.worldCoordinate) && (!self.lookingForImpossiblePath || cell.reachable))
        {
            return orig(self, cell, costToGoal);
        }
        return orig(self, cell, costToGoal);
    }
    internal static void AbstractCreatureAI_SetDestination(On.AbstractCreatureAI.orig_SetDestination orig, AbstractCreatureAI self, WorldCoordinate destination)
    {
        if (self.parent.creatureTemplate.name == "Herring")
        {
            Debug.Log("----------------------------------------------------------------");
            Debug.Log("AbstractCreatureAI_SetDestination Method was called for: Herring");
        }
        orig(self, destination);
    }
    internal static void ArtificialIntelligence_SetDestination(On.ArtificialIntelligence.orig_SetDestination orig, ArtificialIntelligence self, WorldCoordinate destination)
    {
        if (self.creature.creatureTemplate.name == "Herring")
        {
            Debug.Log("ArtificalIntelligence_SetDestination Method was called for: Herring");
        }
        orig(self, destination);
    }
    internal static void PathFinder_SetDestination(On.PathFinder.orig_SetDestination orig, PathFinder self, WorldCoordinate destination)
    {
        if (self.creatureType.name == "Herring")
        {
            Debug.Log("PathFinder_SetDestination Method was called for: Herring");
        }
        orig(self, destination);
    }

    internal static void PathFinder_DestinationHasChanged(On.PathFinder.orig_DestinationHasChanged orig, PathFinder self, WorldCoordinate oldDestination, WorldCoordinate newDestination)
    {
        if (self.creatureType.name == "Herring")
        {
            Debug.Log("----------------------------------------------------------------");
            Debug.Log("PathFinder_DestinationHasChanged Method was called for: Herring");
        }
        orig(self, oldDestination, newDestination);
    }

    internal static void PathFinder_AssignNewDestination(On.PathFinder.orig_AssignNewDestination orig, PathFinder self, WorldCoordinate newDestination)
    {
        if (self.creatureType.name == "Herring")
        {
            Debug.Log("PathFinder_AssignNewDestination Method was called for: Herring");
        }
        orig(self, newDestination);
    }
    internal static int QuickConnectivity_Check(On.QuickConnectivity.orig_Check orig, Room room, CreatureTemplate template, IntVector2 start, IntVector2 goal, int maxGenerations)
    {
        if (template.name == "Herring")
        {
            Debug.Log("QuickConnectivity_Check Method was called for: Herring");
        }
        return orig(room, template, start, goal, maxGenerations);
    }
}
