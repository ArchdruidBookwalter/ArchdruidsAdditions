using System;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using static ArchdruidsAdditions.Data.PlayerData;

namespace ArchdruidsAdditions.Hooks;

public static class WeaponHooks
{
    internal static void Spear_DrawSprites(On.Spear.orig_DrawSprites orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (spearsLoadedInBows.ContainsKey(self))
        {
            Bow bow = spearsLoadedInBows[self];

            float aimCharge = Mathf.Clamp(Mathf.Lerp(bow.lastAimCharge, bow.aimCharge, timeStacker) / bow.shootThreshold, 0f, 1f);
            Vector2 bowPos = Vector2.Lerp(bow.firstChunk.lastPos, bow.firstChunk.pos, timeStacker);
            Vector2 bowRot = Vector2.Lerp(bow.lastRotation, bow.rotation, timeStacker);

            Vector2 spearPos = bowPos - (bowRot * aimCharge * 20f) + bowRot * 17f;

            self.rotation = bowRot;
            self.lastRotation = bowRot;
            self.firstChunk.pos = spearPos;
            self.firstChunk.lastPos = spearPos;
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (self.grabbedBy.Count > 0)
        {
            foreach (Creature.Grasp grasp in self.grabbedBy[0].grabber.grasps)
            {
                if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow)
                {
                    if (bow.loadedSpear != null && bow.loadedSpear == self)
                    {
                        self.ChangeOverlap(true);
                    }
                }
            }
        }
    }
    internal static void Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);

        if (spearsShotByBows.ContainsKey(self))
        {
            if (self.mode != Weapon.Mode.Thrown || self.slatedForDeletetion)
            {
                spearsShotByBows.Remove(self);
            }
        }

        if (spearsLoadedInBows.ContainsKey(self))
        {
            if (!spearsLoadedInBows[self].aiming)
            {
                spearsLoadedInBows.Remove(self);
            }
        }
    }
    internal static Vector2 ElectricSpear_ZapperAttachPos(On.MoreSlugcats.ElectricSpear.orig_ZapperAttachPos orig, MoreSlugcats.ElectricSpear self, float timeStacker, int node)
    {
        Vector2 basePos = orig(self, timeStacker, node);

        if (self.mode == Weapon.Mode.StuckInCreature)
        {
            Vector2 rotation = Vector3.Slerp(self.lastRotation, self.rotation, timeStacker);
            return orig(self, timeStacker, node) - rotation * 20f;
        }

        return basePos;
    }
    internal static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
    {
        if (ModManager.MSC && self.Spear_NeedleCanFeed())
        {
            Player player = self.thrownBy as Player;

            bool recordFoodToRecords = self.room.game.IsStorySession && self.room.game.GetStorySession.playerSessionRecords != null;
            if (result.obj is AshPepper pepper)
            {
                self.Spear_NeedleDisconnect();
                self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.firstChunk.pos);

                for (int i = 0; i < Random.Range(2, 5); i++)
                {
                    self.room.AddObject(new WaterDrip(pepper.firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Random.value * 4f, false));
                }

                player.ObjectEaten(pepper);
                (player.graphicsModule as PlayerGraphics).LookAtObject(self);
                pepper.Destroy();

                if (recordFoodToRecords)
                { self.room.game.GetStorySession.playerSessionRecords[((self.thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(result.obj); }

                self.ChangeMode(Weapon.Mode.Free);

                return true;
            }
            else
            {
                PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.abstractCreature.ID.number);
                if (playerState != null)
                {
                    playerState.recordFoodPips = true;
                    int foodPips = playerState.foodPipsConsumed;

                    bool value;
                    if (result.obj is Parasite || result.obj is CloudFish)
                    {
                        if (result.obj is Creature creature && !creature.dead && creature.SpearStick(self, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, self.firstChunk.vel))
                        {
                            if (creature is Parasite)
                            {
                                recordFoodToRecords = false;
                            }
                            else if (creature is CloudFish)
                            {
                                player.AddQuarterFood();
                                player.AddQuarterFood();
                            }

                            self.Spear_NeedleDisconnect();
                            self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.firstChunk.pos);
                            self.LodgeInCreature(result, eu);

                            if (recordFoodToRecords)
                            { self.room.game.GetStorySession.playerSessionRecords[(player.abstractCreature.state as PlayerState).playerNumber].AddEat(creature); }

                            if (self.abstractPhysicalObject.world.game.IsArenaSession && self.abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0)
                            { self.abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(player, creature); }

                            value = true;
                        }
                        else
                        {
                            value = orig(self, result, eu);
                        }
                    }
                    else
                    {
                        value = orig(self, result, eu);
                    }

                    playerState.recordFoodPips = false;

                    if (value && recordFoodToRecords)
                    {
                        foodPips = playerState.foodPipsConsumed - foodPips;
                        Debug.Log("SPEARMASTER ATE FOOD: " + foodPips);

                        if (playerState.spiceAmount > 0)
                        {
                            int extraFood = foodPips * (playerState.spiceAmount - 1);

                            while (extraFood > 0)
                            {
                                if (extraFood > 4)
                                {
                                    player.AddFood(1);
                                    extraFood -= 4;
                                }
                                else
                                {
                                    player.AddQuarterFood();
                                    extraFood--;
                                }
                            }

                            playerState.spiceAmount = 0;
                            playerState.spicyReactTimer = 0;
                        }
                    }

                    return value;
                }
            }
        }

        return orig(self, result, eu);
    }

    internal static void Weapon_Thrown(On.Weapon.orig_Thrown orig, Weapon self, Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 thrownDir, float frc, bool eu)
    {
        if (self is Spear spear && spearsShotByBows.ContainsKey(spear))
        {
            SpearShotByBow spearData = spearsShotByBows[spear];

            spear.Shoot(thrownBy, thrownPos, spearData.shootDir.normalized, frc, eu);
            spear.rotation = spearData.shootDir;
            spear.changeDirCounter = 0;
            spear.alwaysStickInWalls = true;
            spear.doNotTumbleAtLowSpeed = true;

            if (Mathf.Abs(spear.rotation.x) > Mathf.Abs(spear.rotation.y))
            { spear.throwDir = new IntVector2(Math.Sign(spear.rotation.x), 0); }
            else
            { spear.throwDir = new IntVector2(0, Math.Sign(spear.rotation.y)); }

            if (self.room != null && self.room.locusts != null)
            {
                self.room.locusts.WeaponThrown(self, thrownBy, thrownPos, thrownDir);
            }
        }
        else
        {
            orig(self, thrownBy, thrownPos, firstFrameTraceFromPos, thrownDir, frc, eu);
        }
    }
    internal static void Weapon_Update(On.Weapon.orig_Update orig, Weapon self, bool eu)
    {
        orig(self, eu);

        if (self is Spear spear)
        {
            if (spearsShotByBows.ContainsKey(spear))
            {
                if (spear.mode != Weapon.Mode.Thrown)
                {
                    spearsShotByBows.Remove(spear);
                }
                else
                {
                    if (spear.firstChunk.ContactPoint.x != 0 || spear.firstChunk.ContactPoint.y != 0)
                    {
                        if (Mathf.Abs(Custom.Angle(spear.firstChunk.ContactPoint.ToVector2(), spear.rotation)) > 10)
                        { spear.HitWall(); }
                        else
                        { spear.doNotTumbleAtLowSpeed = false; }
                    }
                    else if (spear.firstChunk.vel.magnitude > 10f)
                    {
                        spear.rotation = spear.firstChunk.vel.normalized;

                        if (Mathf.Abs(spear.rotation.x) > Mathf.Abs(spear.rotation.y))
                        { spear.throwDir = new IntVector2(Math.Sign(spear.rotation.x), 0); }
                        else
                        { spear.throwDir = new IntVector2(0, Math.Sign(spear.rotation.y)); }
                    }
                }
            }

            if (spear.mode == Spear.Mode.StuckInCreature && spear.abstractSpear.poison > 0f)
            {
                foreach (AbstractPhysicalObject.AbstractObjectStick stick in spear.stuckInChunk.owner.abstractPhysicalObject.stuckObjects)
                {
                    if (stick is AbstractParasiteStick paraStick)
                    {
                        (paraStick.Parasite.realizedObject as Parasite).InjectedPoisonColor = new Color(spear.abstractSpear.poisonHue, 1f, 0.5f);
                    }
                }
            }
        }
    }
}
