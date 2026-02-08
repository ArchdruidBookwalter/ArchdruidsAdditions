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
using static System.Collections.Specialized.BitVector32;

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
        public static (Color lightColor, float lightExposure, float colorExposure) TrueLightColorAndExposure(Room room, Vector2 pos, float addBrightness)
        {
            float roomDarkness = Mathf.Clamp((room.Darkness(pos) - 0.3f) * 1.6f, 0f, 1f);

            float lightSourceExposure = 0;
            float lightColorExposure = 0;
            float r = 0;
            float g = 0;
            float b = 0;
            foreach (LightSource light in room.lightSources)
            {
                CircularSprite newSprite = new(light.ElementName, 64);
                float trueLightRad = light.rad * (newSprite.width / 2f) * 0.125f;

                if (Custom.DistLess(pos, light.pos, trueLightRad))
                {
                    float thisLightExposure = Mathf.Lerp(1, 0, Custom.Dist(pos, light.pos) / trueLightRad) * 2;
                    lightSourceExposure = Mathf.Max(lightSourceExposure, thisLightExposure);

                    float thisColorExposure = Custom.RGB2HSL(light.color).y * thisLightExposure * 2;
                    lightColorExposure = Mathf.Max(lightColorExposure, thisColorExposure);

                    r = Mathf.Max(r, light.color.r * Mathf.Min(thisLightExposure, thisColorExposure));
                    g = Mathf.Max(g, light.color.g * Mathf.Min(thisLightExposure, thisColorExposure));
                    b = Mathf.Max(b, light.color.b * Mathf.Min(thisLightExposure, thisColorExposure));
                }
            }

            Color lightColor = new(r, g, b);

            float trueBrightness = Mathf.Clamp(1 - (roomDarkness - lightSourceExposure - addBrightness), 0f, 1f);

            return (lightColor, trueBrightness, lightSourceExposure);
        }

        #region Debug Shapes
        public static bool DebugShapes = true;
        public static void Create_LineBetweenTwoPoints(Room room, Vector2 point1, Vector2 point2, string color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Vector2 middlePos = Vector2.Lerp(point1, point2, 0.5f);
            float dist = Custom.Dist(point1, point2);
            Vector2 rotation = Custom.DirVec(point1, point2);
            Create_Square(room, middlePos, 0.2f, dist, rotation, color, maxLife);
        }
        public static void Create_LineBetweenTwoPoints(Room room, Vector2 point1, Vector2 point2, Color color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Vector2 middlePos = Vector2.Lerp(point1, point2, 0.5f);
            float dist = Custom.Dist(point1, point2);
            Vector2 rotation = Custom.DirVec(point1, point2);
            Create_Square(room, middlePos, 0.2f, dist, rotation, color, maxLife);
        }
        public static void Create_Text(Room room, Vector2 pos, string text, string color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return; }

            room.AddObject(new ColoredShapes.Text(room, pos, text, color, maxLife));
        }
        public static void Create_Text(Room room, Vector2 pos, string text, Color color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return; }

            room.AddObject(new ColoredShapes.Text(room, pos, text, color, maxLife));
        }
        public static void Create_TextBlock(Room room, Vector2 bottomPos, string[] lines, string color, int maxLife)
        {
            if (room == null || !DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return; }

            for (int i = 0; i < lines.Length - 1; i++)
            {
                string test = lines[lines.Length - 1 - i];
                Create_Text(room, new Vector2(bottomPos.x, bottomPos.y + 20f * i), test, color, maxLife);
            }
        }

        public static void Create_Square(Room room, Vector2 pos, float width, float height, Vector2 rotation, string color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return; }

            room.AddObject(new ColoredShapes.Rectangle(room, pos, width, height, Custom.VecToDeg(rotation), color, maxLife));
        }
        public static void Create_Square(Room room, Vector2 pos, float width, float height, Vector2 rotation, Color color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return; }

            room.AddObject(new ColoredShapes.Rectangle(room, pos, width, height, Custom.VecToDeg(rotation), color, maxLife));
        }
        public static void Create_LineAndDot(Room room, Vector2 startPos, Vector2 endPos, string color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            room.AddObject(new ColoredShapes.LineAndDot(room, startPos, endPos, color, maxLife));
        }
        public static void Create_Dot(Room room, Vector2 pos, string color, int maxLife)
        {
            if (!DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return; }

            room.AddObject(new ColoredShapes.Dot(room, pos, color, maxLife));
        }
        #endregion

        public static void Log_Exception(Exception e, string methodName, float section)
        {
            Debug.Log("");
            Debug.Log("EXCEPTION OCCURED IN METHOD: " + methodName + " IN CODE SECTION: " + section);
            Debug.Log("");
            Debug.LogException(e);
        }
        public static void Log_Coordinate(World world, WorldCoordinate coord)
        {
            Debug.Log(world.GetAbstractRoom(coord).name + ", " + coord.x + ", " + coord.y + ", " + coord.abstractNode);
        }

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
