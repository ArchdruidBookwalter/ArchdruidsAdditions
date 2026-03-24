using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects.DevObjects;
using Menu;
using Steamworks;
using UnityEngine;
using RWCustom;
using static ArchdruidsAdditions.Methods.Methods;
using static ArchdruidsAdditions.Data.PlayerData;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class GameHooks
{
    public static int debugEffectCooldown = 0;

    #region RainWorldGame Hooks
    internal static void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

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
    internal static AbstractCreature RainWorldGame_SpawnPlayers(On.RainWorldGame.orig_SpawnPlayers_bool_bool_bool_bool_WorldCoordinate orig, RainWorldGame self,
        bool player1, bool player2, bool player3, bool player4, WorldCoordinate location)
    {
        //Debug.Log("");
        //Debug.Log("METHOD RAINWORLDGAME_SPAWNPLAYERS WAS CALLED");
        //Debug.Log("");

        AbstractCreature firstPlayer = orig(self, player1, player2, player3, player4, location);

        //Debug.Log("NUMBER OF PLAYERS: " + self.Players.Count);
        //Debug.Log("");

        return firstPlayer;
    }
    #endregion

    #region PlayerProgression Hooks
    internal static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, SlugcatStats.Name saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
    {
        LogMethodStart("PLAYERPROGRESSION_GETORINITIATESAVESTATE");

        if (self.currentSaveState == null && Data.SaveStateData.saveStateData.ContainsKey(saveStateNumber))
        {
            Data.SaveStateData.SaveStateDataContainer saveData = Data.SaveStateData.saveStateData[saveStateNumber];
            if (saveData.parasiteSaveState != null)
            {
                LogMessage("LOADED PARASITE SAVE STATE!");

                self.currentSaveState = saveData.parasiteSaveState;
                self.currentSaveState.deathPersistentSaveData.winState.ResetLastShownValues();
                saveData.parasiteSaveState = null;
            }
        }

        SaveState newSaveState = orig(self, saveStateNumber, game, setup, saveAsDeathOrQuit);

        LogMethodEnd();

        return newSaveState;
    }
    internal static bool PlayerProgression_SaveWorldStateAndProgression(On.PlayerProgression.orig_SaveWorldStateAndProgression orig, PlayerProgression self, bool malnourished)
    {
        LogMethodStart("PLAYERPROGRESSION_SAVEWORLDSTATEANDPROGRESSION");

        bool output;

        Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.PlayingAsSlugcat];
        if (saveStateData.infected)
        {
            LogMessage("PLAYER IS INFECTED! NOT SAVING STATE TO DISK!");

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

            LogMethodEnd();

            return output;
        }

        output = orig(self, malnourished);

        LogMethodEnd();

        return output;
    }
    internal static void PlayerProgression_ClearOutSaveStateFromMemory(On.PlayerProgression.orig_ClearOutSaveStateFromMemory orig, PlayerProgression self)
    {
        orig(self);

        Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.PlayingAsSlugcat];
        saveStateData.parasiteSaveState = null;
    }
    #endregion
}
