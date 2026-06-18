using System;
using System.Collections.Generic;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.DevObjects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class GameHooks
{
    public static int debugEffectCooldown = 0;

    #region RainWorldGame Hooks
    internal static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        if (PlayerData.TextBeingInputted)
        {
            self.lastPauseButton = true; self.lastRestartButton = true;
        }

        orig(self);

        if (PlayerData.TextBeingInputted)
        {
            self.lastPauseButton = true; self.lastRestartButton = true;
        }

        if (!self.GamePaused && self.devToolsActive)
        {
            if (self.Players.Count > 0 && self.Players[0].realizedCreature != null && !self.Players[0].realizedCreature.dead && debugEffectCooldown == 0)
            {
                Player player = self.Players[0].realizedCreature as Player;
                Player.InputPackage input = player.input[0];

                if (player.room != null && input.jmp && input.thrw && input.pckp && input.spec)
                {
                    debugEffectCooldown = 200;

                    DebugShapes = !DebugShapes;

                    if (DebugShapes)
                    {
                        player.room.AddObject(new ColoredShapes.Text(player.room, player.firstChunk.pos += new Vector2(0f, 50f), "Debug Shapes Enabled!", "Green", 200));
                    }
                    else
                    {
                        player.room.AddObject(new ColoredShapes.Text(player.room, player.firstChunk.pos += new Vector2(0f, 50f), "Debug Shapes Disabled!", "Red", 200));
                    }
                }
            }

            if (debugEffectCooldown > 0)
            {
                debugEffectCooldown--;
            }
        }
    }
    internal static void RainWorldGame_CommunicateWithUpcomingProcess(On.RainWorldGame.orig_CommunicateWithUpcomingProcess orig, RainWorldGame self, MainLoopProcess nextProcess)
    {
        orig(self, nextProcess);
    }
    internal static void RainWorldGame_Win(On.RainWorldGame.orig_Win orig, RainWorldGame self, bool malnourished, bool fromWarpPoint)
    {
        //LogMethodStart("RAINWORLDGAME_WIN");

        //LogMessage("MALNOURISHED: " + malnourished);

        if (self.manager.upcomingProcess == null)
        {
            bool playerInfected = false;

            List<EntityID> infectedCreatures = [];
            List<EntityID> alivePlayers = [];

            AbstractCreature player = self.FirstAlivePlayer;
            if (player != null)
            {
                foreach (AbstractCreature creature in player.Room.creatures)
                {
                    if (creature.state.alive)
                    {
                        if (self.Players.Contains(creature))
                        {
                            alivePlayers.Add(creature.ID);
                        }
                        else if (creature.state is ParasiteState parasiteState && parasiteState.creatureAttachedTo != null)
                        {
                            infectedCreatures.Add(parasiteState.creatureAttachedTo.Value);
                        }
                    }
                }
            }

            foreach (EntityID id in infectedCreatures)
            {
                if (alivePlayers.Contains(id))
                {
                    playerInfected = true;
                    break;
                }
            }

            Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.StoryCharacter];
            if (playerInfected)
            {
                //LogMessage("PLAYER IS INFECTED! SAVESTATE SHOULD NOT BE SAVED");
                self.rainWorld.saveBackedUp = true;

                saveStateData.infected = true;
            }
            else
            {
                saveStateData.infected = false;
            }
        }

        orig(self, malnourished, fromWarpPoint);

        //LogMethodEnd();
    }
    internal static void RainWorldGame_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
    {
        PlayerData.DevToolsOn = self.devToolsActive;

        if (PlayerData.TextBeingInputted && PlayerData.DevToolsOn)
        {
            //Debug.Log("PLAYER IS TYPING WHILE DEVTOOLS ARE ON, TEMPORARILY DISABLING DEVTOOLS");

            PlayerData.DisableDevToolsForRainWorldGame = true;

            self.debugGraphDrawer?.Update();
            self.devUI?.Update();

            self.devToolsActive = false;
        }

        orig(self, dt);
    }
    internal static AbstractCreature RainWorldGame_SpawnPlayers(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self,
        bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
    {
        return orig(self, player1, player2, player3, player4, location);
    }
    #endregion

    #region PlayerProgression Hooks
    internal static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
    {
        //LogMethodStart("PLAYERPROGRESSION_GETORINITIATESAVESTATE");

        if (self.currentSaveState == null && Data.SaveStateData.saveStateData.ContainsKey(saveStateNumber))
        {
            Data.SaveStateData.SaveStateDataContainer saveData = Data.SaveStateData.saveStateData[saveStateNumber];
            if (saveData.parasiteSaveState != null)
            {
                //LogMessage("LOADED PARASITE SAVE STATE!");

                self.currentSaveState = saveData.parasiteSaveState;
                self.currentSaveState.deathPersistentSaveData.winState.ResetLastShownValues();
                saveData.parasiteSaveState = null;
            }
        }

        SaveState newSaveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);

        //LogMethodEnd();

        return newSaveState;
    }
    internal static bool PlayerProgression_SaveWorldStateAndProgression(On.PlayerProgression.orig_SaveWorldStateAndProgression orig, PlayerProgression self, bool malnourished)
    {
        //LogMethodStart("PLAYERPROGRESSION_SAVEWORLDSTATEANDPROGRESSION");

        bool output;

        Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.PlayingAsSlugcat];
        if (saveStateData.infected)
        {
            //LogMessage("PLAYER IS INFECTED! NOT SAVING STATE TO DISK!");

            output = self.SaveProgressionAndDeathPersistentDataOfCurrentState(true, false);

            if (ModManager.MMF && self.currentSaveState != null)
            {
                for (int i = 0; i < self.currentSaveState.objectTrackers.Count; i++)
                {
                    self.currentSaveState.objectTrackers[i].UninitializeTracker();
                }
            }

            if (ModManager.MMF || ModManager.Watcher)
            {
                self.BackupRegionStatePreservation();
            }

            saveStateData.parasiteSaveState = self.currentSaveState;
            self.currentSaveState = null;

            //LogMethodEnd();

            return output;
        }

        output = orig(self, malnourished);

        //LogMethodEnd();

        return output;
    }
    internal static void PlayerProgression_ClearOutSaveStateFromMemory(On.PlayerProgression.orig_ClearOutSaveStateFromMemory orig, PlayerProgression self)
    {
        orig(self);

        if (Data.SaveStateData.saveStateData.ContainsKey(self.PlayingAsSlugcat))
        {
            Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.PlayingAsSlugcat];
            saveStateData.parasiteSaveState = null;
        }
    }
    #endregion

    #region SpeedRunTimer Hooks
    internal static double SpeedRunTimer_GetTimerTickIncrement(On.MoreSlugcats.SpeedRunTimer.orig_GetTimerTickIncrement orig, RainWorldGame game, double input)
    {
        double baseInc = orig(game, input);

        foreach (AbstractCreature player in game.session.Players)
        {
            Data.PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.ID.number);
            if (playerState != null && playerState.parasiteIllnessEffect != null)
            {
                float newFPS = Mathf.Lerp(40f, 15f, playerState.parasiteIllnessEffect.intensity / 1000);
                double newInc = 1.0 / (double)newFPS;

                return Math.Min(baseInc, newInc);
            }
        }

        return baseInc;
    }
    #endregion
}
