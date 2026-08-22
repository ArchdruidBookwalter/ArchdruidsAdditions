using System.Collections.Generic;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PhysicalObjectHooks
{
    internal static void PhysicalObject_Update(On.PhysicalObject.orig_Update orig, PhysicalObject self, bool eu)
    {
        orig(self, eu);
    }

    internal static void Mushroom_DrawSprites(On.Mushroom.orig_DrawSprites orig, Mushroom self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
    }
}
