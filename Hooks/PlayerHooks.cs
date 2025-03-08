using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        if (obj is  Objects.ParrySword)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp].grabbed is Objects.ParrySword)
        {
            (self.grasps[grasp].grabbed as Objects.ParrySword).Use();
            return;
        }
        orig(self, grasp, eu);
    }
}
