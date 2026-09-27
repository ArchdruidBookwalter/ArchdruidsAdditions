using HarmonyLib;

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

    internal static void BodyChunk_Update(On.BodyChunk.orig_Update orig, BodyChunk self)
    {
        orig(self);

        if (self.collideWithTerrain)
        {
            if (MiscData.boxHandlers.ContainsKey(self.owner.room))
            {
                if (MiscData.boxHandlers[self.owner.room].TrySnapToCollisionBox(self.pos, self.lastPos, self.TerrainRad, out Vector2 snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint, out CollisionBox closestBox, self))
                {
                    if (contactPoint.y != 0)
                    {
                        if (contactPoint.y < 0 && surfaceDir.y > 0f)
                        {
                            self.pos = snapPos;
                            self.terrainCurveNormal = surfaceDir;

                            float velY = -self.vel.y * surfaceDir.y;
                            if (velY > self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(0, -1), velY, self.lastContactPoint.y > -1);
                            }

                            if (self.terrainCurveNormal.y < TerrainCurve.maxSlideNormalY)
                            {
                                self.contactPoint.y = 0;
                                self.vel -= surfaceDir * Mathf.Min(0f, Vector2.Dot(self.vel, surfaceDir) * (1f + self.owner.bounce * 0.2f));

                                Vector2 newSurfaceDir = new(-surfaceDir.y, surfaceDir.x);
                                self.vel -= Vector2.Dot(self.vel, newSurfaceDir) * Mathf.Clamp01(1f - self.owner.surfaceFriction * 2f) * newSurfaceDir;
                            }
                            else
                            {
                                self.contactPoint.y = -1;
                                float velM = self.vel.magnitude;
                                float vel2 = self.vel.x * -surfaceDir.x / surfaceDir.y;

                                self.vel.y -= vel2;
                                self.vel.y = Mathf.Abs(self.vel.y) * self.owner.bounce;
                                if (self.vel.y < self.owner.gravity || self.vel.y < 1f + 9f * (1f - self.owner.bounce))
                                {
                                    self.vel.y *= 0f;
                                }
                                self.vel.y += vel2;
                                self.vel.x *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                                self.vel = Vector2.ClampMagnitude(self.vel, velM);
                            }

                            if (closestBox != null)
                            { self.vel.x += closestBox.vel.x; }
                        }
                        else if (contactPoint.y > 0)
                        {
                            self.pos.y = snapPos.y;
                            if (self.vel.y > self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(0, 1), Mathf.Abs(self.vel.y), self.lastContactPoint.y < 1);
                            }
                            self.contactPoint.y = 1;
                            self.vel.y = -Mathf.Abs(self.vel.y) * self.owner.bounce;
                            if (Mathf.Abs(self.vel.y) < 1f + 9f * (1f - self.owner.bounce))
                            {
                                self.vel.y *= 0f;
                            }
                            self.vel.x *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                    }

                    if (contactPoint.x != 0)
                    {
                        if (contactPoint.x > 0)
                        {
                            self.pos.x = snapPos.x;
                            if (self.vel.x > self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(1, 0), Mathf.Abs(self.vel.x), self.lastContactPoint.x < 1);
                            }
                            self.contactPoint.x = 1;
                            self.vel.x = -Mathf.Abs(self.vel.x) * self.owner.bounce;
                            if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
                            {
                                self.vel.x = 0f;
                            }
                            self.vel.y *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                        else if (contactPoint.x < 0)
                        {
                            self.pos.x = snapPos.x;
                            if (self.vel.x < -self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(-1, 0), Mathf.Abs(self.vel.x), self.lastContactPoint.x > -1);
                            }
                            self.contactPoint.x = -1;
                            self.vel.x = Mathf.Abs(self.vel.x) * self.owner.bounce;
                            if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
                            {
                                self.vel.x = 0f;
                            }
                            self.vel.y *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                    }
                }

                /*
                Vector2 closestSnapPos = self.pos;
                IntVector2 closestContactPoint = new(0, 0);
                float closestSnapPosDist = float.MaxValue;
                bool pushOutOfBox = false;

                foreach (CollisionBox box in MiscData.boxHandlers[self.owner.room].collisionBoxes)
                {
                    if (box.owner != self.owner)
                    {
                        box.GetSnapPosAndContact(self.pos, self.lastPos, self.TerrainRad, out Vector2 collisionSnapPos, out IntVector2 collisionContactPoint, true);

                        if (collisionContactPoint.x != 0 || collisionContactPoint.y != 0)
                        {
                            float dist = Custom.Dist(collisionSnapPos, self.pos);
                            if (dist < closestSnapPosDist)
                            {
                                closestSnapPos = collisionSnapPos;
                                closestContactPoint = collisionContactPoint;
                                closestSnapPosDist = dist;
                                pushOutOfBox = true;
                            }
                        }
                    }

                    //Create_LineBetweenTwoPoints(self.owner.room, self.pos, self.pos + collisionContactPoint.ToVector2() * 50, 1f, Color.red, 0);
                }

                if (pushOutOfBox)
                {
                    //Create_Square(self.owner.room, closestSnapPos, 5f, 5f, Vec(45), Color.red, 100);
                    //Create_LineBetweenTwoPoints(self.owner.room, self.pos, self.pos + closestContactPoint.ToVector2() * 20f, 2f, Color.red, 100);

                    if (closestContactPoint.y > 0)
                    {
                        self.pos.y = closestSnapPos.y - self.TerrainRad;
                        if (self.vel.y > self.owner.impactTreshhold)
                        {
                            self.owner.TerrainImpact(self.index, new IntVector2(0, 1), Mathf.Abs(self.vel.y), self.lastContactPoint.y < 1);
                        }
                        self.contactPoint.y = 1;
                        self.vel.y = -Mathf.Abs(self.vel.y) * self.owner.bounce;
                        if (Mathf.Abs(self.vel.y) < 1f + 9f * (1f - self.owner.bounce))
                        {
                            self.vel.y *= 0f;
                        }
                        self.vel.x *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                    }
                    else if (closestContactPoint.y < 0 && !standOnBox)
                    {
                        self.pos.y = closestSnapPos.y + self.TerrainRad;
                        if (self.vel.y < self.owner.impactTreshhold)
                        {
                            self.owner.TerrainImpact(self.index, new IntVector2(0, -1), Mathf.Abs(self.vel.y), self.lastContactPoint.y > -1);
                        }
                        self.contactPoint.y = -1;
                        self.vel.y = Mathf.Abs(self.vel.y) * self.owner.bounce;
                        if (self.vel.y < self.owner.gravity || self.vel.y < 1f + 9f * (1f - self.owner.bounce))
                        {
                            self.vel.y *= 0f;
                        }
                        self.vel.x *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                    }

                    if (closestContactPoint.y != -1)
                    {
                        if (closestContactPoint.x > 0)
                        {
                            self.pos.x = closestSnapPos.x - self.TerrainRad;
                            if (self.vel.x > self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(1, 0), Mathf.Abs(self.vel.x), self.lastContactPoint.x < 1);
                            }
                            self.contactPoint.x = 1;
                            self.vel.x = -Mathf.Abs(self.vel.x) * self.owner.bounce;
                            if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
                            {
                                self.vel.x = 0f;
                            }
                            self.vel.y *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                        else if (closestContactPoint.x < 0)
                        {
                            self.pos.x = closestSnapPos.x + self.TerrainRad;
                            if (self.vel.x < -self.owner.impactTreshhold)
                            {
                                self.owner.TerrainImpact(self.index, new IntVector2(-1, 0), Mathf.Abs(self.vel.x), self.lastContactPoint.x > -1);
                            }
                            self.contactPoint.x = -1;
                            self.vel.x = Mathf.Abs(self.vel.x) * self.owner.bounce;
                            if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
                            {
                                self.vel.x = 0f;
                            }
                            self.vel.y *= Mathf.Clamp(self.owner.surfaceFriction * 2f, 0f, 1f);
                        }
                    }

                    //Create_LineBetweenTwoPoints(self.owner.room, self.pos, self.pos + closestContactPoint.ToVector2() * 20f, 2f, Color.red, 0);
                }*/
            }

            //Create_LineBetweenTwoPoints(self.owner.room, self.pos, self.pos + self.ContactPoint.ToVector2() * 50, 2f, Color.red, 0);
        }

        if (self.terrainCurveNormal != null)
        {
            Create_LineBetweenTwoPoints(self.owner.room, self.pos, self.pos + self.terrainCurveNormal * 50, 2f, Color.red, 0);
        }
    }

    internal static bool PhysicalObject_IsTileSolid(On.PhysicalObject.orig_IsTileSolid orig, PhysicalObject self, int bChunk, int relativeX, int relativeY)
    {

        bool baseValue = orig(self, bChunk, relativeX, relativeY);

        if (!baseValue && MiscData.boxHandlers.ContainsKey(self.room))
        {
            BodyChunk chunk = self.bodyChunks[bChunk];
            Vector2 pos = chunk.pos + new Vector2(relativeX * 20f, relativeY * 20f);

            return MiscData.boxHandlers[self.room].PositionInsideBox(pos, 0f, self is Player);
        }

        return baseValue;
    }
}
