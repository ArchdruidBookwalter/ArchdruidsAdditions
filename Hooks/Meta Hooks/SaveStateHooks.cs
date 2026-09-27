using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArchdruidsAdditions.Hooks;

public static class SaveStateHooks
{
    internal static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string s, RainWorldGame game)
    {
        float section = 0;

        LogMethodStart("SAVESTATE_LOADGAME");

        try
        {

            orig(self, s, game);

            section = 0.1f;


            if (SaveStateData.AASaveDataContainer != null)
            {
                LogMessage("AASAVEDATACONTAINER WAS NOT NULL!");
            }

            bool readData = false;
            PlayerData.AAPlayerState playerState = null;
            SaveStateData.SaveStateDataContainer currentSaveDataContainer = null;

            foreach (string modKey in SaveStateData.saveStateDataContainers.Keys)
            {
                SaveStateData.SaveStateDataContainer container = SaveStateData.saveStateDataContainers[modKey];
                if (!container.campaignDataValues.ContainsKey(self.saveStateNumber))
                {
                    container.InitalizeForCampaign(self.saveStateNumber);
                }
            }

            section = 0.2f;

            LogMessage("LOADING SAVE STRING: ");

            if (self.unrecognizedSaveStrings != null)
            {
                for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                {
                    LogMessage(self.unrecognizedSaveStrings[i]);

                    if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_START")
                    { readData = true; }
                    else if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_END")
                    { readData = false; }
                    else if (readData)
                    {
                        section = 1;

                        if (self.unrecognizedSaveStrings[i].Contains("MODDATA_"))
                        {
                            section = 1.1f;

                            string modName = self.unrecognizedSaveStrings[i].Remove(0, 8);
                            if (SaveStateData.saveStateDataContainers.ContainsKey(modName))
                            { currentSaveDataContainer = SaveStateData.saveStateDataContainers[modName]; }
                            else
                            { currentSaveDataContainer = null; }
                        }
                        else
                        {
                            section = 1.2f;

                            string[] splitData = Regex.Split(self.unrecognizedSaveStrings[i], "<svB>");

                            section = 1.3f;

                            if (splitData.Length > 1)
                            {
                                if (currentSaveDataContainer != null)
                                {
                                    section = 1.4f;

                                    SaveStateData.SaveStateDataContainer.CampaignDataContainer campaignDataContainer = currentSaveDataContainer.campaignDataValues[self.saveStateNumber];
                                    if (campaignDataContainer.HasValue(splitData[0]))
                                    {
                                        SaveStateData.SaveStateDataContainer.DataValue value = campaignDataContainer.GetValue(splitData[0]);
                                        campaignDataContainer.SetValue(splitData[0], SaveStateData.SaveStateDataContainer.DataValue.GenericDataValueFromString(splitData[1], value.valueType));
                                    }
                                }
                                else
                                {
                                    section = 2f;

                                    if (splitData[0] == "PLAYER")
                                    {
                                        section = 2.1f;

                                        int playerID = int.Parse(splitData[1]);

                                        if (PlayerData.playerStates.Count == 0 || !PlayerData.playerStates.ContainsKey(playerID))
                                        {
                                            section = 2.2f;

                                            PlayerData.AAPlayerState newPlayerState = new(null, null);
                                            PlayerData.playerStates.Add(playerID, newPlayerState);

                                            playerState = newPlayerState;
                                        }
                                        else
                                        {
                                            playerState = PlayerData.playerStates[playerID];
                                        }
                                    }
                                    else if (playerState != null)
                                    {
                                        if (splitData[0] == "INFECTED")
                                        {
                                            section = 2.3f;

                                            playerState.infected = bool.Parse(splitData[1]);
                                        }
                                        else if (splitData[0] == "PARASITE")
                                        {
                                            section = 2.4f;

                                            playerState.parasiteID = EntityID.FromString(splitData[1]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            section = 3;

            /*
            LogMessage("");
            LogMessage("RECORDED SAVE STATE DATA:");

            foreach (string modNameKey in SaveStateData.saveStateDataContainers.Keys)
            {
                section = 3.1f;

                SaveStateData.SaveStateDataContainer container = SaveStateData.saveStateDataContainers[modNameKey];
                LogMessage("   " + modNameKey + ": ");

                section = 3.2f;

                SaveStateData.SaveStateDataContainer.CampaignDataContainer campaignContainer = container.campaignDataValues[self.saveStateNumber];
                foreach (string valueKey in container.baseValues.Keys)
                {
                    section = 3.3f;

                    SaveStateData.SaveStateDataContainer.DataValue value = campaignContainer.GetValue(valueKey);

                    if (value != null)
                    {
                        LogMessage("      " + valueKey.ToUpper() + ": " + value.value.ToString());
                    }
                    else
                    {
                        LogMessage("      " + valueKey.ToUpper() + ": NULL");
                    }
                }
            }*/
        }
        catch (Exception e)
        {
            LogMethodEnd();

            Methods.Methods.Log_Exception(e, "SAVESTATE_LOADGAME", section);
        }

        LogMethodEnd();
    }
    internal static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        float section = 0;

        try
        {
            LogMethodStart("SAVESTATE_SAVETOSTRING");

            section = 1;

            if (self.unrecognizedSaveStrings != null)
            {
                if (self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_START") && self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_END"))
                {
                    int startIndex = self.unrecognizedSaveStrings.IndexOf("AA_SAVEDATA_START");
                    int endIndex = self.unrecognizedSaveStrings.LastIndexOf("AA_SAVEDATA_END") + 1;

                    self.unrecognizedSaveStrings.RemoveRange(startIndex, endIndex - startIndex);
                }

                self.unrecognizedSaveStrings.Add("AA_SAVEDATA_START");

                section = 2;

                foreach (PlayerData.AAPlayerState playerState in PlayerData.playerStates.Values)
                {
                    Dictionary<string, string> playerData = playerState.GetCycleData();

                    for (int i = 0; i < playerData.Count; i++)
                    {
                        string[] att = [playerData.ElementAt(i).Key, playerData.ElementAt(i).Value];
                        self.AddUnrecognized(att);
                    }
                }

                LogMessage("ADDED PLAYERDATA TO UNRECOGNIZEDSAVESTRINGS");

                section = 3;

                foreach (string modName in SaveStateData.saveStateDataContainers.Keys)
                {
                    self.unrecognizedSaveStrings.Add("MODDATA_" + modName);

                    SaveStateData.SaveStateDataContainer modContainer = SaveStateData.saveStateDataContainers[modName];

                    if (!modContainer.campaignDataValues.ContainsKey(self.saveStateNumber))
                    {
                        modContainer.InitializeValuesForCampaign(self.saveStateNumber);
                    }

                    if (modContainer.campaignDataValues.ContainsKey(self.saveStateNumber))
                    {
                        foreach (SaveStateData.SaveStateDataContainer.DataValue value in modContainer.campaignDataValues[self.saveStateNumber].values)
                        {
                            if (value.value != null)
                            {
                                string[] att = [value.name, value.value.ToString()];
                                self.AddUnrecognized(att);
                            }
                        }
                    }
                }

                LogMessage("ADDED SAVESTATEDATA TO UNRECOGNIZEDSAVESTRINGS");

                section = 4;

                self.unrecognizedSaveStrings.Add("AA_SAVEDATA_END");
            }

            section = 5;

            LogMessage("SAVESTRING: ");
            foreach (string saveString in self.unrecognizedSaveStrings)
            {
                LogMessage(saveString);
            }

            string postSaveString = orig(self);

            LogMethodEnd();

            return postSaveString;
        }
        catch (Exception e)
        {
            Log_Exception(e, "SAVESTATE_SAVETOSTRING", section);

            string postSaveString = orig(self);

            LogMethodEnd();

            return postSaveString;
        }
    }
    internal static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
        Methods.Methods.LogMethodStart("SAVESTATE_SESSIONENDED");
        //Methods.Methods.LogMessage("SURVIVED: " + survived + ", " + "MALNOURISHED: " + newMalnourished);

        if (survived)
        {
            List<AbstractCreature> survivingPlayers = [];

            foreach (AbstractCreature creature in game.Players[0].Room.creatures)
            {
                if (creature.state.alive && game.Players.Contains(creature))
                {
                    survivingPlayers.Add(creature);
                }

                if (creature.state is ParasiteState parasiteState && parasiteState.creatureAttachedTo.HasValue)
                {
                    parasiteState.growth++;
                }
            }
        }

        PlayerData.scavData.Clear();

        orig(self, game, survived, newMalnourished);

        Methods.Methods.LogMethodEnd();
    }
    internal static float SaveState_Get_SlowFadeIn(Func<SaveState, float> orig, SaveState self)
    {
        float origValue = orig(self);

        LogMethodStart("SAVESTATE_GET_SLOWFADEIN");
        LogMessage("ORIG VALUE: " + origValue);

        SaveStateData.SaveStateDataContainer container = SaveStateData.AASaveDataContainer;
        if (container != null && container.GetBool("INFECTED", self.progression.PlayingAsSlugcat))
        {
            LogMessage("PARASITE FOUND!");

            return Mathf.Max(origValue, 4f);
        }

        LogMethodEnd();

        return origValue;
    }
}
