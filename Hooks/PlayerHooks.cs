using System;
using System.Collections.Generic;
using System.Linq;
using static ArchdruidsAdditions.Data.PlayerData;
using static ArchdruidsAdditions.Methods.Methods;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    #region Player Hooks

    static int clearConsoleCooldown = 0;
    static bool playingSounds = false;
    static int soundIndex = 0;
    static int soundTimer = 0;
    static List<string> soundIDs;
    internal static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        AAPlayerState playerState = PlayerData.GetPlayerStateFromAbstractCreature(self.abstractCreature);
        if (playerState.spiceAmount > playerState.tolerance && playerState.tooSpicy == false)
        {
            self.Stun(playerState.spiceAmount * 50);
            playerState.tooSpicy = true;

            playerState.spicyReactTimer = 50;
        }
        else if (playerState.spiceAmount == playerState.tolerance)
        {
            if (playerState.spicyReactTimer == 0 && Random.value < 0.05f)
            {
                playerState.spicyReactTimer = Random.Range(50, 500);
            }
            self.aerobicLevel = Mathf.Max(self.aerobicLevel, 0.9f);
        }

        if (playerState.tooSpicy)
        {
            if (playerState.spiceAmount > 0)
            {
                self.aerobicLevel = 1f;

                if (playerState.spicyReactTimer == 0)
                {
                    playerState.spicyReactTimer = 50;

                    playerState.spiceAmount--;
                }
            }
            else
            {
                playerState.tooSpicy = false;
            }
        }

        if (playerState.spicyReactTimer > 0)
        {
            playerState.spicyReactTimer--;
            self.Blink(2);
            self.Hypothermia = Mathf.Lerp(self.Hypothermia, 0f, 0.01f * playerState.spiceAmount);
        }
    }
    internal static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        bool aimingBow = false;

        foreach (Creature.Grasp grasp in self.grasps)
        {
            if (grasp is not null && grasp.grabbed is not null && grasp.grabbed is Bow bow && bow.aiming)
            {
                aimingBow = true;
            }
        }

        Player.InputPackage package = self.input[0];

        if (aimingBow)
        {
            self.input[0].x = 0; self.input[0].analogueDir.x = 0;
            if (self.bodyMode == Player.BodyModeIndex.Crawl)
            {
                if (self.input[0].y < 0 || self.input[0].analogueDir.y < 0)
                { self.input[0].y = 0; self.input[0].analogueDir.y = 0; }
            }
            else
            {
                self.input[0].y = 0; self.input[0].analogueDir.y = 0;
            }
        }

        orig(self, eu);

        self.input[0] = package;
    }

    internal static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Potato)
        {
            if ((obj as Potato).buried)
            {
                if (self.grasps[0] != null && self.grasps[1] != null)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
                return Player.ObjectGrabability.Drag;
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is ParrySword)
        {
            return Player.ObjectGrabability.BigOneHand;
        }
        if (obj is Bow)
        {
            foreach (Creature.Grasp grasp in self.grasps)
            {
                if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow)
                {
                    return Player.ObjectGrabability.CantGrab;
                }
            }
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is Parasite)
        {
            if ((obj as Parasite).dead)
            {
                return Player.ObjectGrabability.OneHand;
            }
            return Player.ObjectGrabability.CantGrab;
        }
        if (obj is CloudFish || obj is LightningFruit || obj is FirePepper)
        {
            return Player.ObjectGrabability.OneHand;
        }
        return orig(self, obj);
    }
    internal static PhysicalObject Player_PickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorSpears)
    {
        if (favorSpears > 50)
        {
            return orig(self, favorSpears);
        }
        Potato closestPotato = null;
        float dist;
        float oldDist = float.MaxValue;
        for (int i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (self.room.physicalObjects[i][j] is Potato thisPotato && thisPotato.buried)
                {
                    dist = Custom.Dist(thisPotato.bodyChunks[1].pos, self.bodyChunks[0].pos);
                    if (dist < 20f && dist < oldDist)
                    {
                        closestPotato = thisPotato;
                        oldDist = dist;
                    }
                }
            }
        }
        if (closestPotato != null && self.CanIPickThisUp(closestPotato))
        {
            return closestPotato;
        }
        return orig(self, favorSpears);
    }
    internal static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabbedCreature)
    {
        if (grabbedCreature is CloudFish)
        {
            return true;
        }
        return orig(self, grabbedCreature);
    }
    internal static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
    {
        if (obj is Potato potato && potato.buried)
        {
            self.LoseAllGrasps();
            self.Grab(obj, graspUsed, 1, Creature.Grasp.Shareability.CanNotShare, 0.5f, true, false);
        }
        else if (obj is CloudFish)
        {
            self.Grab(obj, graspUsed, 0, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, false);
        }
        else if (obj is Parasite)
        {
            self.Grab(obj, graspUsed, 1, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, false);
        }
        else
        {
            orig(self, obj, graspUsed);
        }
    }

    internal static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject obj)
    {
        if (obj is ScarletFlowerBulb)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static Vector2 Player_GetHeldItemDirection(On.Player.orig_GetHeldItemDirection orig, Player self, int hand)
    {
        Vector2 spearVec1 = Custom.DirVec(self.mainBodyChunk.pos, self.grasps[hand].grabbed.bodyChunks[0].pos) * ((hand == 0) ? (-1f) : 1f);
        if (self.animation != Player.AnimationIndex.HangFromBeam)
        { spearVec1 = Custom.PerpendicularVector(spearVec1); }
        Vector2 spearVec2 = Custom.DegToVec((80f + Mathf.Cos((float)(self.animationFrame + (self.leftFoot ? 9 : 3)) / 12f * 2f * 3.1415927f) * 4f *
            (self.graphicsModule as PlayerGraphics).spearDir) * (self.graphicsModule as PlayerGraphics).spearDir);

        if (self.grasps[hand].grabbed is Bow bow)
        {
            if (self.bodyMode == Player.BodyModeIndex.Crawl)
            {
                Vector2 crawlVec = Custom.PerpendicularVector(orig(self, hand));
                if (crawlVec.y > 0)
                {
                    crawlVec = Custom.rotateVectorDeg(crawlVec, 180);
                }
                return crawlVec;
            }
            else if (self.animation == Player.AnimationIndex.ClimbOnBeam)
            { return Custom.PerpendicularVector(orig(self, hand)); }

            Vector2 bowVec2 = Custom.DirVec(self.bodyChunks[0].pos, self.bodyChunks[1].pos);
            Vector2 bowVec1 = Custom.rotateVectorDeg(bowVec2, (60 + Math.Abs(3 * self.bodyChunks[0].vel.y)) * ((hand == 0) ? 1f : -1f));
            return Vector3.Slerp(bowVec1, bowVec2, Math.Abs((self.graphicsModule as PlayerGraphics).spearDir));
        }
        return orig(self, hand);
    }

    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        if (obj is ParrySword)
        {
            return true;
        }
        if (obj is Bow)
        {
            return true;
        }
        return orig(self, obj);
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp].grabbed is ParrySword sword)
        {
            sword.Use();
            return;
        }
        else if (self.grasps[grasp].grabbed is Bow bow)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null &&
                self.grasps[otherGrasp].grabbed is Spear spear &&
                self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangFromBeam)
            {
                bow.LoadSpearIntoBow(spear);
            }
            return;
        }
        else if (self.grasps[grasp].grabbed is Spear spear2)
        {
            int otherGrasp = grasp == 0 ? 1 : 0;
            if (self.grasps[otherGrasp] is not null &&
                self.grasps[otherGrasp].grabbed is Bow bow2)
            {
                if (self.animation != Player.AnimationIndex.ClimbOnBeam &&
                self.animation != Player.AnimationIndex.HangFromBeam)
                {
                    bow2.LoadSpearIntoBow(spear2);
                }
                return;
            }
        }
        orig(self, grasp, eu);
    }

    internal static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);

        AAPlayerState playerState = PlayerData.GetPlayerStateFromAbstractCreature(self.abstractCreature);

        if (edible is FirePepper)
        {
            playerState.spiceAmount++;
            if (playerState.spiceAmount == 1)
            {
                playerState.spiceAmount = 2;
            }

            playerState.spicyReactTimer += playerState.spiceAmount * 50;
            self.aerobicLevel = Mathf.Min(1f, self.aerobicLevel + (float)playerState.spiceAmount / playerState.tolerance);
        }
        else if (playerState.spiceAmount > 0)
        {
            int baseValue = SlugcatStats.NourishmentOfObjectEaten(self.SlugCatClass, edible);
            int extraValue = baseValue * (playerState.spiceAmount - 1);

            //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 20f), baseValue.ToString(), "Red", 100);

            while (extraValue > 0)
            {
                if (extraValue > 4)
                {
                    self.AddFood(1);
                    extraValue -= 4;
                }
                else
                {
                    self.AddQuarterFood();
                    extraValue--;
                }
            }

            playerState.spiceAmount = 0;
            playerState.spicyReactTimer = 0;
        }
    }
    internal static void Player_AddFood(On.Player.orig_AddFood orig, Player self, int add)
    {
        orig(self, add);
    }

    #endregion

    #region PlayerGraphics Hooks

    internal static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (self.owner.room != null && self.objectLooker != null)
        {
            /*
            List<UpdatableAndDeletable> checkUpdels = [];
            foreach (UpdatableAndDeletable updel in self.owner.room.updateList)
            {
                checkUpdels.Add(updel);
            }

            foreach (UpdatableAndDeletable updel in checkUpdels)
            {
                if (updel is PhysicalObject obj)
                {
                    float interest = self.objectLooker.HowInterestingIsThisObject(obj);
                    Create_Text(self.owner.room, obj.firstChunk.pos, interest.ToString(), "Red", 0);
                }
            }*/

            /*
            if (self.objectLooker.currentMostInteresting != null)
            {
                Create_LineBetweenTwoPoints(self.owner.room, self.owner.firstChunk.pos, self.objectLooker.currentMostInteresting.firstChunk.pos, "Green", 0);
                Create_Square(self.owner.room, self.objectLooker.currentMostInteresting.firstChunk.pos, 10f, 10f, Vec(45), "Green", 0);
            }*/

        }
    }
    internal static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        Player.InputPackage package = self.player.input[0];
        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp != null && grasp.grabbed != null && grasp.grabbed is Bow bow && bow.aiming)
            {
                self.player.input[0].x = 0; self.player.input[0].analogueDir.x = 0;
                if (self.player.input[0].y < 0 || self.player.input[0].analogueDir.y < 0)
                { self.player.input[0].y = 0; self.player.input[0].analogueDir.y = 0; }
            }
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        self.player.input[0] = package;

        AAPlayerState AAPlayerState = PlayerData.GetPlayerStateFromAbstractCreature(self.player.abstractCreature);
        if (AAPlayerState.spicyReactTimer > 0 && sLeaser.sprites[9].element.name.Contains("0"))
        {
            sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
        }

        foreach (Creature.Grasp grasp in self.player.grasps)
        {
            if (grasp is not null && grasp.grabbed is Potato potato && potato.playerSquint)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }
        }
    }
    
    internal static float PlayerObjectLooker_HowInterestingIsThisObject(On.PlayerGraphics.PlayerObjectLooker.orig_HowInterestingIsThisObject orig, PlayerGraphics.PlayerObjectLooker self, PhysicalObject obj)
    {
        float interest = orig(self, obj);

        if (obj is Parasite parasite && parasite.buriedInChunk != null)
        {
            interest = -20;
        }

        return interest;
    }

    #endregion

    #region PlayerState Hooks

    internal static void PlayerState_ctor(On.PlayerState.orig_ctor orig, PlayerState self, AbstractCreature crit, int playerNumber, SlugcatStats.Name slugcatCharacter, bool isGhost)
    {
        orig(self, crit, playerNumber, slugcatCharacter, isGhost);

        //Debug.Log("");
        //Debug.Log("METHOD PLAYERSTATE_CTOR WAS CALLED!");
        //Debug.Log("");

        if (PlayerData.playerStates.Count == 0 || !PlayerData.playerStates.ContainsKey(playerNumber))
        {
            AAPlayerState newPlayerState = new(crit, self, playerNumber);
            playerStates.Add(playerNumber, newPlayerState);

            //Debug.Log("COULDN'T FIND PLAYERSTATE FOR PLAYER: " + playerNumber + ", CREATED NEW ONE.");
        }
        else
        {
            //Debug.Log("PLAYERSTATE WAS FOUND!");
            playerStates[playerNumber].RefreshPlayerState(crit, self);

            if (playerStates[playerNumber].infected)
            {
                foreach (AbstractCreature creature in crit.Room.creatures)
                {
                    if (creature.ID == playerStates[playerNumber].parasiteID && creature.stuckObjects.Count == 0)
                    {
                        //Debug.Log("PARASITE HAD NO STICK, CREATED A NEW ONE!");

                        AbstractParasiteStick newStick = new(creature, crit, playerNumber, 0);
                    }
                }
            }
        }

        //Debug.Log("");
    }

    #endregion

    #region SlugcatHand Hooks
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        orig(self);

        Player player = self.owner.owner as Player;
        Creature.Grasp grasp = player.grasps[self.limbNumber];

        if (grasp != null && grasp.grabbed is CloudFish cloudFish && !cloudFish.dead)
        {
            self.huntSpeed = Random.value * 5f;
            self.quickness = Random.value * 0.3f;
            self.vel += Custom.RNV() * 1f;
            self.pos += Custom.RNV() * 1f;
            (player.graphicsModule as PlayerGraphics).NudgeDrawPosition(0, Custom.DirVec(player.mainBodyChunk.pos, self.pos) * 1f * Random.value);
            (player.graphicsModule as PlayerGraphics).head.vel += Custom.DirVec(player.mainBodyChunk.pos, self.pos) * 1f * Random.value;
        }
    }
    #endregion

    #region SlugcatStats Hook
    
    internal static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name name, IPlayerEdible edible)
    {
        if (edible is FirePepper)
        {
            return 1;
        }
        else if (edible is Parasite)
        {
            return 1;
        }
        return orig(name, edible);
    }

    #endregion
}
