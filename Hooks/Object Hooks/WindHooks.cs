using static Watcher.PearlContent;

namespace ArchdruidsAdditions.Hooks;

public static class WindHooks
{
    internal static void WindRect_Update(On.WindRect.orig_Update orig, WindRect self, bool eu)
    {
        orig(self, eu);

        float vel = self.Data.velocity * (80f + 10f * Mathf.Sin(self.room.game.clock / 40f * 2f));
        float vel2 = self.Data.velocity * (10f + 10f * Mathf.Sign(self.room.game.clock * 10f));

        if (MiscData.ropeObjects.ContainsKey(self.room))
        {
            foreach (RopeObject rope in Data.MiscData.ropeObjects[self.room])
            {
                foreach (RopeObject.RopeSegment segment in rope.ropeSegments)
                {
                    if (self.WindAffectsPoint(segment.pos) && !segment.Stuck)
                    {
                        float newVel = Mathf.Min(vel * Mathf.Max(segment.owner.windAffect, 0.2f), 50f);
                        float newVel2 = Mathf.Min(vel2 * Mathf.Max(segment.owner.windAffect, 0.2f) * 0.1f, 50f);

                        segment.vel.x = self.ApplyWind(segment.vel.x, newVel, 0.02f);
                        segment.vel.y = self.ApplyWind(segment.vel.y, Mathf.Abs(newVel) * 0.2f, 0.1f);
                        segment.vel += Custom.RNV() * newVel2 * Mathf.Max(segment.owner.windAffect, 0.2f) * 0.5f;
                    }
                }
            }
        }
    }

    internal static void Sandstorm_AffectObjects(On.Watcher.Sandstorm.orig_AffectObjects orig, Watcher.Sandstorm self, float amount)
    {
        orig(self, amount);

        if (MiscData.ropeObjects.ContainsKey(self.room))
        {
            Vector2 dir = Custom.RotateAroundOrigo(self.globalWindDir, Random.Range(-10f, 10f));
            float baseVel1 = amount * 10f * (80f + 10f * Mathf.Sin(self.room.game.clock / 40f * 2f));
            float baseVel2 = amount * 10f * (10f + 30f * Mathf.Sign(self.room.game.clock * 10f));

            foreach (RopeObject rope in Data.MiscData.ropeObjects[self.room])
            {
                foreach (RopeObject.RopeSegment segment in rope.ropeSegments)
                {
                    float a = 1;
                    if (self.surfaceMask != null)
                    {
                        IntVector2 tilePos = self.room.GetTilePosition(segment.pos);
                        a = self.surfaceMask.GetPixel(Mathf.Clamp(tilePos.x, 0, self.room.TileWidth - 1), Mathf.Clamp(tilePos.y, 0, self.room.TileHeight - 1)).a;
                    }
                    float pixelIntensity = Mathf.Lerp(a, 1f, self.GlobalIntensity);

                    float newVel = Mathf.Min(baseVel1 * Mathf.Max(segment.owner.windAffect, 0.2f) * pixelIntensity, 50f);
                    float newVel2 = Mathf.Min(baseVel2 * Mathf.Max(segment.owner.windAffect, 0.2f) * 0.1f * pixelIntensity, 50f);

                    segment.vel.x = ApplyWind(segment.vel.x, newVel, 0.02f);
                    segment.vel.y = ApplyWind(segment.vel.y, Mathf.Abs(newVel) * 0.2f, 0.1f);
                    segment.vel += Custom.RNV() * newVel2 * Mathf.Max(segment.owner.windAffect, 0.2f) * 0.5f;
                }
            }
        }
    }

    public static float ApplyWind(float baseVel, float newVel, float force)
    {
        if (Mathf.Abs(baseVel) < Mathf.Abs(newVel) || Mathf.Sign(baseVel) != Mathf.Sign(newVel))
        { return Mathf.Lerp(baseVel, newVel, force); }
        return baseVel;
    }
}
