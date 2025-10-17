using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
        public static void CheckIfNull(object value, string valueName)
        {
            if (value is null)
            { Debug.Log(valueName + " is Null!"); }
            else
            { Debug.Log(valueName + " is not Null!"); }
        }
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
                else if (grabber is Scavenger scav)
                {
                    int layer = (scav.graphicsModule as ScavengerGraphics).ContainerForHeldItem(obj, graspUsed);
                    if (layer == 1 || layer == 2)
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items")); }
                    }
                    else if (layer == 0)
                    {
                        for (int i = 0; i < room.game.cameras.Length; i++)
                        { room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Background")); }
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
        public static void CreateLineBetweenTwoPoints(Vector2 point1, Vector2 point2, Room room, string color)
        {
            Vector2 middlePos = Vector2.Lerp(point1, point2, 0.5f);
            float dist = Custom.Dist(point1, point2);
            float rotation = Custom.VecToDeg(Custom.DirVec(point1, point2));
            room.AddObject(new Objects.ColoredShapes.Rectangle(room, middlePos, 0.2f, dist, rotation, color, 0));
        }

        public static void CreateDebugText(BodyChunk chunk, string text)
        { chunk.owner.room.AddObject(new ColoredShapes.Text(chunk.owner.room, chunk.pos, text, "White", 0));}
        public static void CreateDebugText(BodyChunk chunk, string text, string color)
        { chunk.owner.room.AddObject(new ColoredShapes.Text(chunk.owner.room, chunk.pos, text, color, 0)); }
        public static void CreateDebugText(BodyChunk chunk, string text, string color, int line)
        { chunk.owner.room.AddObject(new ColoredShapes.Text(chunk.owner.room, new Vector2(chunk.pos.x, chunk.pos.y + 10 * line), text, color, 0)); }
        public static void CreateDebugText(BodyChunk chunk, string text, string color, int line, int maxLife)
        { chunk.owner.room.AddObject(new ColoredShapes.Text(chunk.owner.room, new Vector2(chunk.pos.x, chunk.pos.y + 10 * line), text, color, maxLife)); }

        public static void CreateDebugSquareAtChunk(BodyChunk chunk)
        { chunk.owner.room.AddObject(new ColoredShapes.Rectangle(chunk.owner.room, chunk.pos, 10f, 10f, 45f, "Yellow", 0)); }
        public static void CreateDebugSquareAtChunk(BodyChunk chunk, string color)
        { chunk.owner.room.AddObject(new ColoredShapes.Rectangle(chunk.owner.room, chunk.pos, 10f, 10f, 45f, color, 0)); }
        public static void CreateDebugSquareAtChunk(BodyChunk chunk, string color, float size)
        { chunk.owner.room.AddObject(new ColoredShapes.Rectangle(chunk.owner.room, chunk.pos, size, size, 45f, color, 0)); }
        public static void CreateDebugSquareAtChunk(BodyChunk chunk, string color, float size, int maxLife)
        { chunk.owner.room.AddObject(new ColoredShapes.Rectangle(chunk.owner.room, chunk.pos, size, size, 45f, color, maxLife)); }

        public static void CreateDebugSquareAtPos(Vector2 pos, Room room)
        { room.AddObject(new ColoredShapes.Rectangle(room, pos, 10f, 10f, 45f, "Yellow", 0)); }
        public static void CreateDebugSquareAtPos(Vector2 pos, Room room, string color)
        { room.AddObject(new ColoredShapes.Rectangle(room, pos, 10f, 10f, 45f, color, 0)); }
        public static void CreateDebugSquareAtPos(Vector2 pos, Room room, string color, float size)
        { room.AddObject(new ColoredShapes.Rectangle(room, pos, size, size, 45f, color, 0)); }
        public static void CreateDebugSquareAtPos(Vector2 pos, Room room, string color, float size, int maxLife)
        { room.AddObject(new ColoredShapes.Rectangle(room, pos, size, size, 45f, color, maxLife)); }


        public static T Check<T>(Expression<Func<T>> expression)
        {
            object o = Expression.Lambda(expression).Compile().DynamicInvoke();

            if (o is T)
            { return (T)o; }

            var memberExp = expression.Body as MemberExpression;
            if (memberExp is null)
            { throw new NullReferenceException("A non-member expression was null."); }
            var message = string.Format
                (
                "Null value detected at {0}.{1}",
                memberExp.Member.DeclaringType.Name,
                memberExp.Member.Name);
            throw new NullReferenceException (message);
        }

        public static bool forceDefaultMouseVisible = false;
        public static bool forceDefaultMouseHidden = false;
        public static bool forceGameMouseVisible = false;
        public static bool forceGameMouseHidden = false;
    }
}
