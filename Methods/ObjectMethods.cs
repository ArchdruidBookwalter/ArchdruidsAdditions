using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Methods
{
    public class ObjectMethods
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
    }
}
