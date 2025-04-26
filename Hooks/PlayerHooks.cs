using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

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
            if (potato.buried)
            {
                if (Custom.Dist(potato.bodyChunks[0].pos, self.mainBodyChunk.pos) < Custom.Dist(potato.bodyChunks[1].pos, self.mainBodyChunk.pos))
                {
                    return Player.ObjectGrabability.CantGrab;
                }
                else
                {
                    return Player.ObjectGrabability.Drag;
                }
            }
            else
            {
                return Player.ObjectGrabability.OneHand;
            }
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

    /*
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig,  SlugcatHand self)
    {
        int grasp = self.limbNumber;
        Player player = self.owner.owner as Player;
        if (player.grasps[grasp] != null && player.grasps[grasp].grabbed is Objects.ParrySword sword && sword.useBool == true && sword.rejectTime == 0f)
        {
            self.mode = Limb.Mode.HuntRelativePosition;
            self.relativeHuntPos = Custom.RotateAroundVector(sword.aimDirection, self.pos, sword.useTime) * 80f;
            self.huntSpeed = 90f;
            self.quickness = 1f;
            self.retract = false;
            self.retractCounter = 0;
            return;
        }
        orig(self);
    }
    */
}
