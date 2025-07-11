using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Hooks;
using ArchdruidsAdditions.Objects;
using MonoMod.RuntimeDetour;
using RWCustom;
using Steamworks;
using Unity.Mathematics;
using UnityEngine;

namespace ArchdruidsAdditions.Methods
{
    public static class Methods
    {
        public static void ChangeItemSpriteLayer(PlayerCarryableItem obj, Creature grabber, int graspUsed)
        {
            Room room = obj.room;
            if (obj is IDrawable drawableObj)
            {
                if (obj is Bow bow && bow.aiming)
                {
                    for (int i = 0; i < room.game.cameras.Length; i++)
                    { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items")); }
                }
                else if (grabber is Player player)
                {
                    if (graspUsed == 0)
                    {
                        if ((player.graphicsModule as PlayerGraphics).spearDir < 0)
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Background")); }
                        }
                        else
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items")); }
                        }
                    }
                    else
                    {
                        if ((player.graphicsModule as PlayerGraphics).spearDir > 0)
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Background")); }
                        }
                        else
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items")); }
                        }
                    }
                }
            }
        }
        public static bool IsPlayerCrawling(Player player)
        {
            if (player.bodyMode == Player.BodyModeIndex.Crawl || player.bodyMode == Player.BodyModeIndex.CorridorClimb)
            { return true; }
            return false;
        }
        public static float PT(float side1, float side2)
        {
            return (float)Math.Sqrt(side1 * side1 + side2 * side2);
        }
        public static bool CanSpearStick(Spear spear)
        {
            Vector2 chunkPos = spear.firstChunk.pos;
            Vector2 startPos = spear.thrownPos;
            Room room = spear.room;

            float spearRot = Math.Abs(spear.rotation.GetAngle());

            if ((spearRot < 170f && spearRot > 100f) || (spearRot < 80f && spearRot > 10f))
            { return false; }

            if (Custom.DistLess(startPos, chunkPos, 560f * Mathf.Max(1f, spear.spearDamageBonus))
                && room.GetTile(chunkPos).Terrain == Room.Tile.TerrainType.Air
                && room.GetTile(chunkPos + spear.rotation * 20f).Terrain == Room.Tile.TerrainType.Solid)
            {
                for (int i = 0; i < 3; i++)
                {
                    foreach (PhysicalObject obj in room.physicalObjects[i])
                    {
                        if (obj is Spear otherSpear && otherSpear.mode == Weapon.Mode.StuckInWall && Custom.DistLess(spear.firstChunk.pos, otherSpear.firstChunk.pos, 20f))
                        { return false; }
                    }
                }

                foreach (PlacedObject obj in room.roomSettings.placedObjects)
                {
                    if (obj.type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(spear.firstChunk.pos, obj.pos, (obj.data as PlacedObject.ResizableObjectData).Rad))
                    { return false; }
                }

                if (room.abstractRoom.shelter && room.shelterDoor != null && (room.shelterDoor.IsClosing || room.shelterDoor.IsOpening))
                { return false; }
            }
            else
            { return false; }

            return true;
        }
        public static UpdatableAndDeletable GetTracker(UpdatableAndDeletable trackedUpdel, Room room)
        {
            foreach (UpdatableAndDeletable updel in room.updateList)
            {
                if (trackedUpdel is Weapon weapon && updel is Trackers.ThrowTracker tracker && tracker.weapon == weapon)
                {
                    return tracker;
                }
            }
            return null;
        }
        public static void CreateLineBetweenTwoPoints(Vector2 point1, Vector2 point2, Room room, Color color)
        {
            Vector2 middlePos = Vector2.Lerp(point1, point2, 0.5f);
            float dist = Custom.Dist(point1, point2);
            float rotation = Custom.VecToDeg(Custom.DirVec(point1, point2));
            room.AddObject(new Objects.ColoredShapes.Rectangle(room, middlePos, 0.2f, dist, rotation, color, 0));
        }
        public static PhysicalObject CheckScavengerInventory(Scavenger scav, Type searchType, bool includeAllSpearTypes)
        {
            //Debug.Log(searchType);
            int foundItemCount;
            for (int i = 0; i < scav.grasps.Count(); i++)
            {
                if (scav.grasps[i] is not null)
                {
                    //Debug.Log(i + ": " + scav.grasps[i].grabbed.GetType());
                    if (scav.grasps[i].grabbed.GetType() == searchType)
                    {
                        //Debug.Log("Found Type!");
                        return scav.grasps[i].grabbed;
                    }
                    if (includeAllSpearTypes && searchType == typeof(Spear))
                    {
                        if (scav.grasps[i].grabbed.GetType().BaseType == typeof(Spear))
                        {
                            //Debug.Log("Found Type!");
                            return scav.grasps[i].grabbed;
                        }
                    }
                }
                else
                { //Debug.Log(i + ": EMPTY");
                }
            }
            //Debug.Log("");
            return null;
        }

        public static bool forceDefaultMouseVisible = false;
        public static bool forceDefaultMouseHidden = false;
        public static bool forceGameMouseVisible = false;
        public static bool forceGameMouseHidden = false;
    }
}
