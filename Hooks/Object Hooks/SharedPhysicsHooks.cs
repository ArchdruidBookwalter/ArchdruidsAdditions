using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.Physical_Objects;

namespace ArchdruidsAdditions.Hooks;

public static class SharedPhysicsHooks
{
    internal static SharedPhysics.TerrainCollisionData SharedPhysics_TerrainCollisionData_VerticalCollision(On.SharedPhysics.orig_VerticalCollision orig, Room room, SharedPhysics.TerrainCollisionData origData)
    {
        SharedPhysics.TerrainCollisionData baseData = orig(room, origData);

        if (MiscData.boxHandlers.ContainsKey(room))
        {
            List<CollisionBox> boxes = MiscData.boxHandlers[room].collisionBoxes;
            foreach (CollisionBox box in boxes)
            {
                baseData = box.UpdateCollisionData(baseData);
            }
        }

        return baseData;
    }
}
