using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects.Decoration;

namespace ArchdruidsAdditions.Hooks;

public static class WindHooks
{
    internal static void WindRect_Update(On.WindRect.orig_Update orig, WindRect self, bool eu)
    {
        orig(self, eu);

        float vel = self.Data.velocity * (80f + 10f * Mathf.Sin(self.room.game.clock / 40f * 2f));
        float vel2 = self.Data.velocity * (10f + 30f * Mathf.Sign(self.room.game.clock * 10f));

        foreach (RopeObject rope in Data.MiscData.ropeObjects)
        {
            foreach (RopeObject.RopeSegment segment in rope.ropeSegments)
            {
                if (self.WindAffectsPoint(segment.pos) && !segment.Stuck)
                {
                    float newVel = vel * (1f / segment.mass);

                    segment.vel.x = self.ApplyWind(segment.vel.x, newVel, 0.02f);
                    segment.vel.y = self.ApplyWind(segment.vel.y, Mathf.Abs(newVel * 0.5f), 0.02f);
                    segment.vel += Custom.RNV() * vel2 * (1f / (segment.mass * 5)) * 0.3f;
                }
            }
        }
    }
}
