using System.Collections.Generic;

namespace ArchdruidsAdditions.Hooks;

public static class SharedPhysicsHooks
{
    internal static SharedPhysics.TerrainCollisionData SharedPhysics_TerrainCollisionData_VerticalCollision(On.SharedPhysics.orig_VerticalCollision orig, Room room, SharedPhysics.TerrainCollisionData origData)
    {
        SharedPhysics.TerrainCollisionData baseData = orig(room, origData);

        if (MiscData.boxHandlers.ContainsKey(room))
        {
            baseData = MiscData.boxHandlers[room].CheckCollisionForBoxes(baseData);
        }

        return baseData;
    }
}
