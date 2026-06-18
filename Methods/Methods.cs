using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ArchdruidsAdditions.Objects.DevObjects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using UnityEngine;

namespace ArchdruidsAdditions.Methods
{
    public static class Methods
    {
        public static int tab = 0;
        public static void LogMethodStart(string methodName)
        {
            string divider = new(' ', tab * 5);

            if (tab == 0)
            { Debug.Log(""); }

            Debug.Log(divider + "METHOD " + methodName.ToUpper() + " WAS CALLED!");
            Debug.Log(divider + "{");

            tab++;
        }
        public static void LogMessage(object message)
        {
            string divider = new(' ', tab * 5);

            Debug.Log(divider + message);
        }
        public static void LogMethodEnd()
        {
            tab--;

            string divider = new(' ', tab * 5);

            Debug.Log(divider + "}");
        }


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
        public static (Color lightColor, float lightExposure, float colorExposure) TrueLightColorAndExposure(Room room, RoomCamera camera, Vector2 pos, float addBrightness)
        {
            float adjustedDarkness = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.3f, 1f, room.Darkness(pos)));

            float lightSourceExposure = 0;
            float lightColorExposure = 0;
            float whiteLightExposure = 0;
            float r = 0;
            float g = 0;
            float b = 0;

            List<FSprite> sprites = [];
            foreach (RoomCamera.SpriteLeaser sLeaser in camera.spriteLeasers)
            {
                foreach (FSprite sprite in sLeaser.sprites)
                {
                    if (sprite.shader.name == "LightSource" || sprite.shader.name == "FlatLight")
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            Vector2 adjPos = pos + camera.pos;

            foreach (FSprite sprite in sprites)
            {
                Vector2 lightPos = sprite.GetPosition();
                float dist = Custom.Dist(pos, lightPos);

                float scale = sprite.width / 2;
                float rad = scale;

                Vector3 lightHSL = Custom.RGB2HSL(sprite.color);

                float lightExposure = Custom.SCurve(Mathf.InverseLerp(rad, 0f, dist), 0.5f);
                float colorExposure = Mathf.Min(lightHSL.y, lightExposure);
                float whiteExposure = Mathf.Min(1f - colorExposure, lightExposure);

                lightSourceExposure = Mathf.Max(lightSourceExposure, lightExposure);
                lightColorExposure = Mathf.Max(lightColorExposure, colorExposure);
                whiteLightExposure = Mathf.Max(whiteLightExposure, whiteExposure);

                r = Mathf.Max(r, sprite.color.r * colorExposure);
                g = Mathf.Max(g, sprite.color.g * colorExposure);
                b = Mathf.Max(b, sprite.color.b * colorExposure);

                if (dist < rad)
                {
                    //Create_LineBetweenTwoPoints(room, adjPos, lightPos + camera.pos, 2f, sprite.color, 1);
                }

                //Create_Square(room, lightPos + camera.pos, scale, scale, Vec(0), sprite.color, 1);
            }

            lightSourceExposure = Mathf.Max(lightSourceExposure, 1f - adjustedDarkness) + addBrightness;
            whiteLightExposure = Mathf.Max(whiteLightExposure, (1f - adjustedDarkness) * 0.5f);
            lightColorExposure -= whiteLightExposure;


            Color lightColor = new(r, g, b);
            //Create_Text(room, adjPos + new Vector2(0f, 80f), "COLOR: " + lightColorExposure, lightColor, 0);

            //Create_Text(room, adjPos + new Vector2(0f, 70f), "H: " + Custom.RGB2HSL(lightColor).x, lightColor, 0);
            //Create_Text(room, adjPos + new Vector2(0f, 60f), "S: " + Custom.RGB2HSL(lightColor).y, lightColor, 0);
            //Create_Text(room, adjPos + new Vector2(0f, 50f), "L: " + Custom.RGB2HSL(lightColor).z, lightColor, 0);

            //Create_Text(room, adjPos + new Vector2(0f, 40f), "WHITE: " + whiteLightExposure, Color.white, 0);
            //Create_Text(room, adjPos + new Vector2(0f, 30f), "LIGHT: " + lightSourceExposure, Color.white, 0);
            //Create_Text(room, adjPos + new Vector2(0f, 20f), "DARKNESS: " + adjustedDarkness, Color.white, 0);

            //Create_Square(room, adjPos, 10f, 10f, Vec(0), "Red", 1);

            return (lightColor, lightSourceExposure, lightColorExposure * 0.5f);
        }

        #region Debug Shapes
        public static bool DebugShapes = false;
        public static bool HideDebugShapes(Room room)
        {
            if (room == null || !DebugShapes || !room.BeingViewed || !room.game.devToolsActive)
            { return true; }

            Player player = room.game.FirstRealizedPlayer;
            if (player != null && player.room == null)
            { return true; }

            if (room.game.lastPauseButton)
            { return true; }

            return false;
        }
        public static void Create_LineBetweenTwoPoints(Room room, Vector2 point1, Vector2 point2, float width, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Line(room, point1, point2, width, color, maxLife));
        }
        public static void Create_LineBetweenTwoPoints(Room room, Vector2 point1, Vector2 point2, float width, Color color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Line(room, point1, point2, width, color, maxLife));
        }
        public static void Create_Text(Room room, Vector2 pos, object text, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Text(room, pos, text == null ? "NULL" : text.ToString(), color, maxLife));
        }
        public static void Create_Text(Room room, Vector2 pos, object text, Color color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Text(room, pos, text == null ? "NULL" : text.ToString(), color, maxLife));
        }
        public static void Create_TextBlock(Room room, Vector2 pos, int dir, string[] lines, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string test = lines[i];
                Create_Text(room, new Vector2(pos.x, pos.y + 20f * i * dir), test, color, maxLife);
            }
        }

        public static void Create_Square(Room room, Vector2 pos, float width, float height, Vector2 rotation, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Rectangle(room, pos, width, height, Custom.VecToDeg(rotation), color, maxLife));
        }
        public static void Create_Square(Room room, Vector2 pos, float width, float height, Vector2 rotation, Color color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Rectangle(room, pos, width, height, Custom.VecToDeg(rotation), color, maxLife));
        }
        public static void Create_LineAndDot(Room room, Vector2 startPos, Vector2 endPos, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.LineAndDot(room, startPos, endPos, color, maxLife));
        }
        public static void Create_Dot(Room room, Vector2 pos, string color, int maxLife)
        {
            if (HideDebugShapes(room))
            { return; }

            room.AddObject(new ColoredShapes.Dot(room, pos, color, maxLife));
        }
        #endregion

        public static Vector2 GetMiddleOfScreen(Room room, RoomCamera camera)
        {
            return room.MiddleOfTile(room.GetTilePosition(camera.pos) + new IntVector2(35, 18));
        }

        public static void Log_Exception(Exception e, string methodName, float section)
        {
            Debug.Log("");
            Debug.Log("EXCEPTION OCCURED IN METHOD: " + methodName + " IN CODE SECTION: " + section);
            Debug.Log("");

            throw e;
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

        public static Vector2 Vec(float degree)
        {
            return Custom.DegToVec(degree);
        }

        public static Vector2 Vec(Vector2 start, float rotate)
        {
            return Custom.rotateVectorDeg(start, rotate);
        }

        public static bool forceDefaultMouseVisible = false;
        public static bool forceDefaultMouseHidden = false;
        public static bool forceGameMouseVisible = false;
        public static bool forceGameMouseHidden = false;
    }
}
