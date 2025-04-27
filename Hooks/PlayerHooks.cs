using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
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
            Objects.Potato potato = obj as Objects.Potato;
            float dist1 = Custom.Dist(potato.bodyChunks[0].pos, self.bodyChunks[0].pos);
            float dist2 = Custom.Dist(potato.bodyChunks[1].pos, self.bodyChunks[0].pos);

            if (potato.buried)
            {
                if (dist1 <= dist2)
                {
                    self.pickUpCandidate = null;
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
        return orig(self, obj);
    }
    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        if (obj is Objects.ParrySword)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i] == null)
            {
                UnityEngine.Debug.Log("Grasp " + i + ": NULL");
            }
            else
            {
                UnityEngine.Debug.Log("Grasp " + i + ":" + self.grasps[i].grabbed.ToString());
            }
        }
        if (self.grasps[grasp].grabbed is Objects.ParrySword sword)
        {
            sword.Use();

            /*
            if (self.grasps[0 == grasp ? 1 : 0].grabbed is Objects.ParrySword sword2)
            {
                sword2.Use();
            }*/

            return;
        }
        orig(self, grasp, eu);
    }
    internal static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser,
        RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp is not null && grasp.grabbed is Objects.Potato potato && potato.playerSquint)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }
        }
    }
}
