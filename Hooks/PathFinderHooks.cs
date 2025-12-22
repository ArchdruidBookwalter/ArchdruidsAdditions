using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using ArchdruidsAdditions.Creatures;
using ArchdruidsAdditions.Objects;

namespace ArchdruidsAdditions.Hooks;

public static class PathFinderHooks
{
    internal static void PathFinder_ctor(On.PathFinder.orig_ctor orig, PathFinder self, ArtificialIntelligence AI, World world, AbstractCreature creature)
    {
        if (creature.creatureTemplate.type == CreatureTemplate.Type.CicadaA)
        {
            //self.visualizePath = true;
        }
        orig(self, AI, world, creature);
    }
    internal static void PathFinder_Update(On.PathFinder.orig_Update orig, PathFinder self)
    {
        orig(self);
    }
    internal static void PathFinder_SetDestination(On.PathFinder.orig_SetDestination orig, PathFinder self, WorldCoordinate destination)
    {
        if (self.creatureType.name == "CloudFish")
        {
            Debug.Log("PathFinder_SetDestination Method was called for: CloudFish");
        }
        orig(self, destination);
    }
    internal static void PathFinder_DestinationHasChanged(On.PathFinder.orig_DestinationHasChanged orig, PathFinder self, WorldCoordinate oldDestination, WorldCoordinate newDestination)
    {
        if (self.creatureType.name == "CloudFish")
        {
            Debug.Log("----------------------------------------------------------------");
            Debug.Log("PathFinder_DestinationHasChanged Method was called for: CloudFish");
        }
        orig(self, oldDestination, newDestination);
    }
    internal static void PathFinder_AssignNewDestination(On.PathFinder.orig_AssignNewDestination orig, PathFinder self, WorldCoordinate newDestination)
    {
        if (self.creatureType.name == "CloudFish")
        {
            Debug.Log("PathFinder_AssignNewDestination Method was called for: CloudFish");
        }
        orig(self, newDestination);
    }
    internal static PathCost PathFinder_CoordinateCost(On.PathFinder.orig_CoordinateCost orig, PathFinder self, WorldCoordinate coord)
    {
        if (self.AI is CloudFishAI cloudFishAI)
        {
            //Debug.Log("CLOUDFISH CALLED METHOD: \'PathFinder_CoordinateCost\'" + " - ROOM: " + self.world.GetAbstractRoom(self.room).name);

            Room cloudFishRoom = cloudFishAI.cloudfish.room;
            if (cloudFishRoom != null && cloudFishRoom.BeingViewed)
            {
                //cloudFishRoom.AddObject(new ColoredShapes.Rectangle(cloudFishRoom, cloudFishRoom.MiddleOfTile(coord), 10f, 10f, 45f, "Red", 0));
            }

            if (self.AITileAtWorldCoordinate(coord).acc == AItile.Accessibility.Solid)
            {
                return new PathCost(0f, PathCost.Legality.IllegalTile);
            }
        }
        return orig(self, coord);
    }
    internal static PathCost PathFinder_CheckConnectionCost(On.PathFinder.orig_CheckConnectionCost orig, PathFinder self, PathFinder.PathingCell start, PathFinder.PathingCell goal, MovementConnection connection, bool followingPath)
    {
        if (self.AI is CloudFishAI cloudFishAI)
        {
            Room cloudFishRoom = cloudFishAI.cloudfish.room;
            if (cloudFishRoom != null && cloudFishRoom.BeingViewed && cloudFishRoom.game.devToolsActive)
            {
                //cloudFishRoom.AddObject(new ColoredShapes.Rectangle(cloudFishRoom, cloudFishRoom.MiddleOfTile(start.worldCoordinate), 10f, 10f, 45f, "Green", 0));
                //cloudFishRoom.AddObject(new ColoredShapes.Rectangle(cloudFishRoom, cloudFishRoom.MiddleOfTile(goal.worldCoordinate), 10f, 10f, 45f, "Red", 0));
            }
            //Debug.Log("CLOUDFISH CALLED METHOD: \'PathFinder_CheckConnectionCost\'" + " - ROOM: " + self.world.GetAbstractRoom(self.room).name);
            if (self.AITileAtWorldCoordinate(goal.worldCoordinate).acc == AItile.Accessibility.Solid)
            {
                return new PathCost(0f, PathCost.Legality.IllegalTile);
            }
        }
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
    internal static List<WorldCoordinate> PathFinder_CreatePathForAbstractCreature(On.PathFinder.orig_CreatePathForAbstractreature orig, PathFinder self, WorldCoordinate searchDestination)
    {
        if (self.AI is CloudFishAI cloudFishAI)
        {
            //Debug.Log("CLOUDFISH CALLED: PathFinder_CreatePathForAbstractCreature METHOD");
        }
        return orig(self, searchDestination);
    }

    internal static List<WorldCoordinate> AbstractSpacePathFinder_Path(On.AbstractSpacePathFinder.orig_Path orig, World world, WorldCoordinate start, WorldCoordinate goal, CreatureTemplate creatureType, IOwnAnAbstractSpacePathFinder owner)
    {
        if (creatureType.type == Enums.CreatureTemplateType.CloudFish)
        {
            List<WorldCoordinate> path = orig(world, start, goal, creatureType, owner);
            //Debug.Log("");
            //Debug.Log("CLOUDFISH CALLED \'AbstractSpacePathFinder_Path\' METHOD");
            //Debug.Log("START: " + world.GetAbstractRoom(start.room).name + " - " + start.abstractNode);
            //Debug.Log("GOAL: " + world.GetAbstractRoom(goal.room).name + " - " + goal.abstractNode);
            return path;
        }

        return orig(world, start, goal, creatureType, owner);
    }
    internal static void AbstractSpacePathFinder_AddNode(On.AbstractSpacePathFinder.orig_AddNode orig, WorldCoordinate self, AbstractSpacePathFinder.Node parent, float cost, ref bool[,] alreadyChecked, ref List<AbstractSpacePathFinder.Node> checkNext, ref AbstractSpacePathFinder.Node foundStart, WorldCoordinate start, World world, IOwnAnAbstractSpacePathFinder owner)
    {
        orig(self, parent, cost, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
    }

    internal static void FollowPathVisualizer_Update(On.FollowPathVisualizer.orig_Update orig, FollowPathVisualizer self, bool eu)
    {
        if (self.pathFinder.AI is CloudFishAI fishAI)
        {
            if (!self.pathFinder.visualizePath || !self.pathFinder.world.game.devToolsActive)
            {
                //return;
            }
        }
        orig(self, eu);
    }
    internal static void FollowPathVisualizer_DrawSprites(On.FollowPathVisualizer.orig_DrawSprites orig, FollowPathVisualizer self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self.pathFinder.AI is CloudFishAI fishAI)
        {
            if (!self.pathFinder.visualizePath || !self.pathFinder.world.game.devToolsActive)
            {
                //sLeaser.CleanSpritesAndRemove();
                //return;
            }
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
    }
    internal static MovementConnection StandardPather_FollowPath(On.StandardPather.orig_FollowPath orig, StandardPather self, WorldCoordinate originPos, bool actuallyFollowingThisPath)
    {
        return orig(self, originPos, actuallyFollowingThisPath);
    }
    internal static PathCost StandardPather_HeuristicForCell(On.StandardPather.orig_HeuristicForCell orig, StandardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
    {
        if (self.creatureType.type == Enums.CreatureTemplateType.CloudFish && self.InThisRealizedRoom(cell.worldCoordinate) && (!self.lookingForImpossiblePath || cell.reachable))
        {
            return orig(self, cell, costToGoal);
        }
        return orig(self, cell, costToGoal);
    }
    internal static void AbstractCreatureAI_SetDestination(On.AbstractCreatureAI.orig_SetDestination orig, AbstractCreatureAI self, WorldCoordinate destination)
    {
        if (self.parent.creatureTemplate.name == "CloudFish")
        {
            //Debug.Log("----------------------------------------------------------------");
            //Debug.Log("AbstractCreatureAI_SetDestination Method was called for: CloudFish");
        }
        orig(self, destination);
    }
    internal static void ArtificialIntelligence_SetDestination(On.ArtificialIntelligence.orig_SetDestination orig, ArtificialIntelligence self, WorldCoordinate destination)
    {
        if (self.creature.creatureTemplate.name == "CloudFish")
        {
            //Debug.Log("ArtificalIntelligence_SetDestination Method was called for: CloudFish");
        }
        orig(self, destination);
    }
    internal static int QuickConnectivity_Check(On.QuickConnectivity.orig_Check orig, Room room, CreatureTemplate template, IntVector2 start, IntVector2 goal, int maxGenerations)
    {
        if (template.name == "CloudFish")
        {
            //Debug.Log("QuickConnectivity_Check Method was called for: CloudFish");
        }
        return orig(room, template, start, goal, maxGenerations);
    }
}
