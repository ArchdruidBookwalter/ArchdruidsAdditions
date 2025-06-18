using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Hooks;
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
                if (grabber is Player player)
                {
                    if (graspUsed == 0)
                    {
                        if (player.mainBodyChunk.vel.x < -1)
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            {
                                room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Background"));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            {
                                room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items"));
                            }
                        }
                    }
                    else
                    {
                        if (player.mainBodyChunk.vel.x > 1)
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            {
                                room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Background"));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < room.game.cameras.Length; i++)
                            {
                                room.game.cameras[i].MoveObjectToContainer(drawableObj, room.game.cameras[i].ReturnFContainer("Items"));
                            }
                        }
                    }
                }
            }
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

        public static class BeastmasterDependency
        {
            private static bool? _modPresent;
            public static bool modPresent
            {
                get
                {
                    if (_modPresent == null)
                    { _modPresent = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("fyre.BeastMaster"); }
                    return (bool)_modPresent;
                }
            }
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public static void CreateHook(String methodName)
            {
                if (methodName == "DrawSprites")
                {
                    try
                    {
                        //new Hook(typeof(BeastMaster.BeastMaster).GetMethod("DrawSprites"), BeastmasterHooks.BeastMaster_DrawSprites);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                else if (methodName == "RainWorldOnUpdate")
                {
                    try
                    {
                        //new Hook(typeof(BeastMaster.BeastMaster).GetMethod("RainWorldOnUpdate"), BeastmasterHooks.BeastMaster_OnRainWorldUpdate);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public static bool forceDefaultMouseVisible = false;
        public static bool forceDefaultMouseHidden = false;
        public static bool forceGameMouseVisible = false;
        public static bool forceGameMouseHidden = false;
    }
}
