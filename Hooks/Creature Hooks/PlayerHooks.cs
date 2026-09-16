using System;
using System.Collections.Generic;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.Decoration;
using ArchdruidsAdditions.Objects.Physical_Objects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;
using static System.Collections.Specialized.BitVector32;
using static ArchdruidsAdditions.Data.PlayerData;

namespace ArchdruidsAdditions.Hooks;

public static class PlayerHooks
{
    #region Player Hooks
    //static int debugCooldown = 0;
    //static bool recordCreatureRelationships = false;
    //static int templateID = 0;

    internal static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        //Create_Square(self.room, self.room.MiddleOfTile(self.firstChunk.pos), 10f, 10f, Vec(0), Color.red, 0);
        //Create_Text(self.room, self.room.MiddleOfTile(self.firstChunk.pos), self.IsTileSolid(0, 0, 0), Color.red, 0);

        //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 20f), self.bodyMode, Color.yellow, 0);
        //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 30f), self.animation, Color.yellow, 0);
        //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 40f), "GOINTOCORRIDORCLIMB: " + self.goIntoCorridorClimb, Color.yellow, 0);

        if (self.room != null && self.room.aimap != null && MiscData.boxHandlers.ContainsKey(self.room))
        {
            CollisionBoxHandler handler = MiscData.boxHandlers[self.room];

            Vector2 mousePos = self.room.MiddleOfTile(new Vector2(Futile.mousePosition.x, Futile.mousePosition.y) + self.room.game.cameras[0].pos);
            IntVector2 newTilePos = self.room.GetTilePosition(mousePos) - self.room.GetTilePosition(self.firstChunk.pos);

            bool solid = self.IsTileSolid(0, newTilePos.x, newTilePos.y);

            Create_Square(self.room, self.room.MiddleOfTile(mousePos), 20f, 20f, Vec(0), Color.red, 0);
            Create_Text(self.room, self.room.MiddleOfTile(mousePos) + new Vector2(0f, 25f), solid, Color.red, 0);
        }


        //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 0f), self.animation.value, Color.yellow, 0);

        //Debug.Log(self.coord.Tile.ToString());

        //Create_Square(self.room, Futile.mousePosition, 5f, 5f, Vec(45), "Red", 0);
        //Create_Text(self.room, Futile.mousePosition + new Vector3(0f, 20f), self.room.GetTilePosition(Futile.mousePosition), "Red", 0);

        /*
        if (debugCooldown > 0)
        { debugCooldown--; }
        else
        {
            if (self.input[0].spec)
            {
                if (self.input[0].thrw)
                {
                    recordCreatureRelationships = true;
                }
            }
        }

        if (recordCreatureRelationships)
        {
            if (templateID == StaticWorld.creatureTemplates.Length)
            {
                recordCreatureRelationships = false;
                templateID = 0;
            }
            else
            {
                Debug.Log("");

                CreatureTemplate creature = StaticWorld.creatureTemplates[templateID];

                Debug.Log("-" + creature.name.ToUpper() + " RELATIONSHIPS: ");

                for (int i = 0; i < creature.relationships.Length; i++)
                {
                    CreatureTemplate otherCreature = StaticWorld.creatureTemplates[i];

                    Debug.Log("   " + otherCreature.name.ToUpper() + " - " + creature.relationships[i].type.value + " : " + creature.relationships[i].intensity);
                }

                templateID++;
            }
            debugCooldown = 200;
        }*/

        AAPlayerState playerState = GetPlayerState(self.abstractCreature.ID.number);
        if (playerState != null)
        {
            #region Spice

            if (playerState.spiceAmount > 0)
            {
                int spiceTolerance = AAPlayerState.SlugcatSpiceTolerance(self.SlugCatClass, self.isSlugpup);
                bool reactToSpice = AAPlayerState.SlugcatReactsToSpice(self.SlugCatClass);

                if (playerState.spiceAmount > spiceTolerance || playerState.tooSpicy)
                {
                    if (!playerState.tooSpicy)
                    {
                        self.Stun(playerState.spiceAmount * 50);
                        playerState.tooSpicy = true;

                        playerState.spicyReactTimer = 50;
                    }
                    else
                    {
                        if (playerState.spicyReactTimer <= 0)
                        {
                            playerState.spicyReactTimer = 50;
                            playerState.spiceAmount--;
                        }
                    }

                    self.aerobicLevel = 1f;
                }
                else if (self.FoodInStomach == self.MaxFoodInStomach)
                {
                    if (playerState.spicyReactTimer <= 0)
                    {
                        playerState.spicyReactTimer = 50;
                        playerState.spiceAmount--;
                    }
                }
                else if (reactToSpice && playerState.spiceAmount == spiceTolerance)
                {
                    if (playerState.spicyReactTimer == 0 && Random.value < 0.05f)
                    {
                        playerState.spicyReactTimer = Random.Range(50, 500);
                    }
                    self.aerobicLevel = Mathf.Max(self.aerobicLevel, 0.9f);
                }

                if (playerState.spicyReactTimer > 0)
                {
                    playerState.spicyReactTimer--;
                    self.Blink(2);
                    self.Hypothermia = Mathf.Lerp(self.Hypothermia, 0f, 0.01f * playerState.spiceAmount);
                }

                //Create_Text(self.room, self.firstChunk.pos, playerState.spiceAmount, "Red", 0);
                //Create_Text(self.room, self.firstChunk.pos + new Vector2(0f, 20f), playerState.spicyReactTimer, "Red", 0);
            }
            else
            {
                playerState.spicyReactTimer = 0;
                playerState.tooSpicy = false;
            }

            #endregion

            if (playerState.parasiteStick != null)
            {
                if (!playerState.infected)
                {
                    playerState.infected = true;
                    playerState.previousMalnourishment1 = self.slugcatStats.malnourished;
                    playerState.previousMalnourishment2 = self.slugcatStats.malnourishedByCreature;
                }

                self.SetMalnourished(false, true);
            }
            else if (playerState.infected)
            {
                playerState.infected = false;
                playerState.parasiteKillCounter = 0;
            }
        }
    }
    internal static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
    }
    internal static void Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
    {
        bool slideOnBox = false;

        orig(self);
    }
    internal static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);

        if (TextBeingInputted)
        {
            self.input[0].jmp = false;
            self.input[0].pckp = false;
            self.input[0].thrw = false;
            self.input[0].spec = false;
            self.input[0].x = 0;
            self.input[0].y = 0;
            self.input[0].analogueDir *= 0f;
        }
        else
        {
            bool aimingBow = false;
            foreach (Creature.Grasp grasp in self.grasps)
            {
                if (grasp is not null && grasp.grabbed is not null && grasp.grabbed is Bow bow)
                {
                    bow.basePackage = self.input[0];

                    if (bow.aiming)
                    {
                        aimingBow = true;
                    }
                }
            }

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
        }
    }
    internal static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
    {
        orig(self, newRoom);
    }

    internal static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability baseGrabability = orig(self, obj);

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
        if (obj is CloudFish || obj is LightningFruit || obj is AshPepper)
        {
            return Player.ObjectGrabability.OneHand;
        }
        if (obj is MimicCrab crab)
        {
            if (crab.ShellStick != null)
            { return Player.ObjectGrabability.CantGrab; }
            else
            { return Player.ObjectGrabability.OneHand; }
        }

        return baseGrabability;
    }
    internal static PhysicalObject Player_PickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorSpears)
    {
        PhysicalObject baseCanidate = orig(self, favorSpears);

        if (favorSpears > 50)
        {
            return baseCanidate;
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

        return baseCanidate;
    }
    internal static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabbedCreature)
    {
        bool baseLegality = orig(self, grabbedCreature);

        if (grabbedCreature is CloudFish)
        {
            return true;
        }

        return baseLegality;
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
        bool baseSwallowability = orig(self, obj);

        if (obj is ScarletFlowerBulb)
        {
            return true;
        }

        return baseSwallowability;
    }
    internal static Vector2 Player_GetHeldItemDirection(On.Player.orig_GetHeldItemDirection orig, Player self, int hand)
    {
        Vector2 baseItemDir = orig(self, hand);

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

        return baseItemDir;
    }

    internal static bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig,  Player self, PhysicalObject obj)
    {
        bool baseThrowability = orig(self, obj);

        if (obj is ParrySword)
        {
            return true;
        }
        if (obj is Bow)
        {
            return true;
        }

        return baseThrowability;
    }
    internal static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        PhysicalObject obj = self.grasps[grasp].grabbed;

        if (obj is ParrySword sword)
        {
            sword.Use();
            return;
        }
        else if (obj is Bow bow)
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
        else if (obj is Spear spear2)
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
        int foodPips = 0;
        AAPlayerState playerState = GetPlayerState(self.abstractCreature.ID.number);
        if (playerState != null)
        {
            playerState.recordFoodPips = true;
            foodPips = playerState.foodPipsConsumed;
        }

        orig(self, edible);

        if (playerState != null)
        {
            playerState.recordFoodPips = false;
            foodPips = playerState.foodPipsConsumed - foodPips;

            if (edible is AshPepper)
            {
                playerState.spiceAmount++;
                if (playerState.spiceAmount == 1)
                {
                    playerState.spiceAmount = 2;
                }

                int tolerance = AAPlayerState.SlugcatSpiceTolerance(self.SlugCatClass, self.isSlugpup);
                if (AAPlayerState.SlugcatReactsToSpice(self.SlugCatClass) && playerState.spiceAmount > Math.Max(tolerance - 3, 0))
                {
                    playerState.spicyReactTimer += playerState.spiceAmount * 50;
                    self.aerobicLevel = Mathf.Min(1f, self.aerobicLevel + (float)playerState.spiceAmount / tolerance);
                }
            }
            else if (playerState.spiceAmount > 0)
            {
                int extraFood = foodPips * (playerState.spiceAmount - 1);

                while (extraFood > 0)
                {
                    if (extraFood > 4)
                    {
                        self.AddFood(1);
                        extraFood -= 4;
                    }
                    else
                    {
                        self.AddQuarterFood();
                        extraFood--;
                    }
                }

                playerState.spiceAmount = 0;
                playerState.spicyReactTimer = 0;
            }
        }
    }
    internal static void Player_AddFood(On.Player.orig_AddFood orig, Player self, int add)
    {
        AAPlayerState playerState = PlayerData.GetPlayerState(self.abstractCreature.ID.number);
        if (playerState != null && playerState.recordFoodPips)
        {
            playerState.foodPipsConsumed += 4 * add;

            //Debug.Log("RECORDED FOOD PIPS: " + add);
        }

        orig(self, add);
    }
    internal static void Player_AddQuarterFood(On.Player.orig_AddQuarterFood orig, Player self)
    {
        bool recordFoodPips = false;

        AAPlayerState playerState = PlayerData.GetPlayerState(self.abstractCreature.ID.number);
        if (playerState != null && playerState.recordFoodPips && self.redsIllness == null && self.FoodInStomach < self.MaxFoodInStomach)
        {
            playerState.foodPipsConsumed += 1;

            recordFoodPips = true;
            playerState.recordFoodPips = false;

            //Debug.Log("RECORDED QUAD FOOD PIP");
        }

        orig(self);

        if (recordFoodPips)
        {
            playerState.recordFoodPips = true;
        }
    }
    #endregion



    #region PlayerGraphics Hooks
    internal static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        float section = 0;

        try
        {
            //Debug.Log("<ArchdruidsAdditions> METHOD PLAYERGRAPHICS_UPDATE WAS CALLED! 1");

            orig(self);

            //Debug.Log("<ArchdruidsAdditions> METHOD PLAYERGRAPHICS_UPDATE WAS CALLED! 2");

            section = 1;

            AAPlayerState playerState = GetPlayerState(self.player.abstractCreature.ID.number);
            if (playerState != null)
            {
                if (playerState.parasiteStick != null && playerState.parasiteStick.A != null)
                {
                    playerState.parasiteMalnourishment = Mathf.Min(1f, playerState.parasiteMalnourishment + 0.0001f);
                    self.malnourished = Mathf.Max(self.malnourished, playerState.parasiteMalnourishment);
                }
                else
                {
                    playerState.parasiteMalnourishment = Mathf.Max(0f, playerState.parasiteMalnourishment - 0.0001f);
                    self.malnourished = Mathf.Max(self.malnourished, playerState.parasiteMalnourishment);
                }
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "PLAYERGRAPHICS_UPDATE", section);
        }

        //Create_Text(self.player.room, self.player.mainBodyChunk.pos, self.malnourished, "Red", 0);
    }
    internal static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float section = 0;

        try
        {
            section = 1;

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
            AAPlayerState AAPlayerState = GetPlayerState(self.player.abstractCreature.ID.number);
            bool changeSprites = !sLeaser.deleteMeNextFrame && !rCam.room.game.DEBUGMODE;

            if (changeSprites)
            {
                if (AAPlayerState != null && AAPlayerState.parasiteMalnourishment > 0)
                {
                    self.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
                }
            }

            section = 2;

            //Debug.Log("<ArchdruidsAdditions> METHOD PLAYERGRAPHICS_DRAWSPRITES WAS CALLED! 1");

            orig(self, sLeaser, rCam, timeStacker, camPos);

            //Debug.Log("<ArchdruidsAdditions> METHOD PLAYERGRAPHICS_DRAWSPRITES WAS CALLED! 2");

            section = 3;

            if (changeSprites)
            {
                self.player.input[0] = package;

                if (AAPlayerState != null && AAPlayerState.spiceAmount > 0 && AAPlayerState.spicyReactTimer > 0 && sLeaser.sprites[9].element.name.Contains("0"))
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
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "PLAYERGRAPHICS_DRAWSPRITES", section);
        }
    }
    internal static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject obj)
    {
        float section = 0;

        try
        {
            orig(self, obj);

            section = 1;

            AAPlayerState playerState = PlayerData.GetPlayerState(self.player.abstractCreature.ID.number);
            if (playerState != null && playerState.parasiteStick != null)
            {
                playerState.parasiteMalnourishment = 1f;
                self.malnourished = 1f;
            }
            else
            {
                playerState.parasiteMalnourishment = 0f;
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "PLAYERGRAPHICS_CTOR", section);
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

        if (playerStates.ContainsKey(crit.ID.number))
        {
            playerStates[crit.ID.number].RefreshPlayerState(crit, self);

            if (playerStates[crit.ID.number].infected)
            {
                foreach (AbstractCreature creature in crit.Room.creatures)
                {
                    if (creature.ID == playerStates[crit.ID.number].parasiteID && creature.stuckObjects.Count == 0)
                    {
                        new AbstractParasiteStick(creature, crit, playerNumber, 0);
                    }
                }
            }
        }
        else
        {
            AAPlayerState newPlayerState = new(crit, self);
            playerStates.Add(crit.ID.number, newPlayerState);
        }
    }
    #endregion



    #region SlugcatHand Hooks
    internal static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
        float section = 0;

        try
        {
            orig(self);

            section = 1;

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
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "SLUGCATHAND_UPDATE", section);
        }
    }
    #endregion



    #region SlugcatStats Hook
    internal static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name name, IPlayerEdible edible)
    {
        int baseNourishment = orig(name, edible);

        if (edible is AshPepper)
        {
            return 1;
        }
        else if (edible is Parasite)
        {
            return 1;
        }

        return baseNourishment;
    }
    #endregion
}
