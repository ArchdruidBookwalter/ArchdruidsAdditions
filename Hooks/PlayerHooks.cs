using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL.Smoke;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    #region Player
    internal static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (obj is Objects.ScarletFlowerBulb)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Objects.Potato)
        {
            if ((obj as Objects.Potato).buried)
            {
                if (self.grasps[0] != null && self.grasps[1] != null)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.Drag;
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is Objects.ParrySword)
        {
            return Player.ObjectGrabability.BigOneHand;
        }
        if (obj is Objects.Bow)
        {
            return Player.ObjectGrabability.OneHand;
        }
        return orig(self, obj);
    }
    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        if (obj is Objects.ParrySword)
        {
            return true;
        }
        if (obj is Objects.Bow)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static PhysicalObject Player_PickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorSpears)
    {
        if (favorSpears > 50)
        {
            return orig(self, favorSpears);
        }
        Objects.Potato closestPotato = null;
        float dist;
        float oldDist = float.MaxValue;
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (self.room.physicalObjects[i][j] is Objects.Potato thisPotato && thisPotato.buried)
                {
                    dist = Custom.Dist(thisPotato.bodyChunks[1].pos, self.bodyChunks[0].pos);
                    if (dist < 20f && dist < oldDist)
                    {
                        closestPotato = thisPotato;
                        oldDist = dist;
                    }
                }
            }
        }
        if (closestPotato != null && self.CanIPickThisUp(closestPotato))
        {
            return closestPotato;
        }
        return orig(self, favorSpears);
    }
    internal static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
        if (obj is Objects.Potato potato && potato.buried)
        {
            self.LoseAllGrasps();
            self.Grab(obj, graspUsed, 1, Creature.Grasp.Shareability.CanNotShare, 0.5f, true, false);
        }
        else
        {
            orig(self, obj, graspUsed);
        }
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp].grabbed is Objects.ParrySword sword)
        {
            sword.Use();
            return;
        }
        else if (self.grasps[grasp].grabbed is Objects.Bow bow)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null && self.grasps[otherGrasp].grabbed is Spear spear)
            {
                bow.LoadSpearIntoBow(spear);
            }
            return;
        }
        else if (self.grasps[grasp].grabbed is Spear spear2)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null && self.grasps[otherGrasp].grabbed is Objects.Bow bow2)
            {
                bow2.LoadSpearIntoBow(spear2);
                return;
            }
        }
        orig(self, grasp, eu);
    }
    internal static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        if (self.room != null && self.room.game.devToolsActive)
        {
            if (self.input[0].y > 0)
            {
                Room room = self.room;
                RoomCamera camera = room.game.cameras[0];

                /*
                var spriteLeasers = camera.spriteLeasers;
                for (int i = 0; i < spriteLeasers.Count(); i++)
                {
                    var sLeaser = spriteLeasers[i];

                    if (sLeaser.drawableObject is GraphicsModule || sLeaser.drawableObject is Objects.Potato || sLeaser.drawableObject is CosmeticInsect)
                    {
                        for (int j = 0; j < sLeaser.sprites.Count(); j++)
                        {
                            var sprite = sLeaser.sprites[j];

                            if (sprite.isVisible && numOfSprites < 50 && Custom.Dist(sprite.GetPosition() + camera.pos, self.mainBodyChunk.pos) < 500f)
                            {
                                Color color = GetColor(j);

                                Rect rect = sprite.GetTextureRectRelativeToContainer();
                                room.AddObject(new Objects.ColoredShapes.Rectangle(room, rect.center + camera.pos, rect.width, rect.height, sprite.rotation, color, 1));
                                numOfSprites++;
                            }
                        }
                    }
                }*/

                for (int i = 0; i < 3; i++)
                {
                    foreach (PhysicalObject obj in room.physicalObjects[i])
                    {
                        foreach (BodyChunk chunk in obj.bodyChunks)
                        {
                            float radius = chunk.rad * 2;

                            if (radius < 2f)
                            { radius = 2f; }

                            float hue = (float)obj.bodyChunks.IndexOf(chunk) / obj.bodyChunks.Length;
                            Color color = UnityEngine.Random.ColorHSV(hue, hue, 1f, 1f, 1f, 1f);
                            room.AddObject(new Objects.ColoredShapes.Rectangle(room, chunk.pos, radius, radius, 0f, color, 1));
                        }
                        foreach (PhysicalObject.BodyChunkConnection chain in obj.bodyChunkConnections)
                        {
                            room.AddObject(new Objects.ColoredShapes.Rectangle(room, Vector2.Lerp(chain.chunk1.pos, chain.chunk2.pos, 0.5f), 0.1f, chain.distance, Custom.VecToDeg(Custom.DirVec(chain.chunk1.pos, chain.chunk2.pos)), new(0f, 0f, 1f), 1));
                        }
                        if (obj is Spear)
                        {
                            Vector2 vector = obj.room.MiddleOfTile(obj.firstChunk.pos);
                            room.AddObject(new Objects.ColoredShapes.Rectangle(room, vector, 5f, 5f, 45f, new(0f, 1f, 0f), 1));
                        }
                    }
                }
            }
        }
    }

    public static Color GetColor(int index)
    {
        if (index == 0)
        {
            return new Color(1f, 0f, 0f);
        }
        else if (index == 1)
        {
            return new Color(1f, 0.5f, 0f);
        }
        else if (index == 2)
        {
            return new Color(1f, 1f, 0f);
        }
        else if (index == 3)
        {
            return new Color(0.5f, 1f, 0f);
        }
        else if (index == 4)
        {
            return new Color(0f, 1f, 0f);
        }
        else if (index == 5)
        {
            return new Color(0f, 1f, 0.5f);
        }
        else if (index == 6)
        {
            return new Color(0f, 1f, 1f);
        }
        else if (index == 7)
        {
            return new Color(0f, 0.5f, 1f);
        }
        else if (index == 8)
        {
            return new Color(0f, 0f, 1f);
        }
        else
        {
            int newIndex = index - 9;
            return GetColor(newIndex);
        }
    }
    #endregion

    #region PlayerGraphics
    internal static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        //self.owner.room.AddObject(new Objects.ColoredShapes.Rectangle(self.owner.room, new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + rCam.pos, 1f, 1f, 45f, new Color(1f, 1f, 0f), 1));

        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp is not null && grasp.grabbed is Objects.Potato potato && potato.playerSquint)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }
        }
    }
    #endregion
}
