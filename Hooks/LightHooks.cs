using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;
using RWCustom;

namespace ArchdruidsAdditions.Hooks;

public static class LightHooks
{
    internal static void Redlight_Update(On.Redlight.orig_Update orig, Redlight self, bool eu)
    {
        orig(self, eu);
    }

    internal static void LightSource_Update(On.LightSource.orig_Update orig, LightSource self, bool eu)
    {
        orig(self, eu);
        //Methods.Methods.Create_Square(self.room, self.pos, self.Rad * 2f, self.Rad * 2f, Custom.DegToVec(45), self.color, 0);
    }

    internal static void LightSource_DrawSprites(On.LightSource.orig_DrawSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.room != null)
        {
            float rad = sLeaser.sprites[0].width;
            //Methods.Methods.Create_Square(self.room, self.pos, rad * 2f, rad * 2f, Vector2.up, self.color, 0);
        }
    }
}
