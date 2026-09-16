using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;

namespace ArchdruidsAdditions.Hooks;

public static class SaveStateHooks
{
    internal static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string s, RainWorldGame game)
    {
        float section = 0;

        orig(self, s, game);

        /*
        Debug.Log("");
        Debug.Log("STATIC WORLD:");
        for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
        {
            Debug.Log("");
            CreatureTemplate template = StaticWorld.creatureTemplates[i];
            for (int j = 0; j < template.relationships.Length; j++)
            {
                CreatureTemplate.Relationship relationship = template.relationships[j];
                CreatureTemplate otherTemplate = StaticWorld.creatureTemplates[j];

                Debug.Log(template.name + " -> " + otherTemplate.name + " : " + relationship.type + ", " + relationship.intensity);
            }
        } 000
        Debug.Log("");
        */

        bool readData = false;
        PlayerData.AAPlayerState playerState = null;

        try
        {
            for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_START")
                { readData = true; }
                else if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_END")
                { readData = false; }
                else if (readData)
                {
                    section = 1;

                    string[] splitData = Regex.Split(self.unrecognizedSaveStrings[i], "<svB>");

                    if (splitData.Length > 1)
                    {
                        if (splitData[0] == "PLAYER")
                        {
                            section = 2;

                            int playerID = int.Parse(splitData[1]);

                            if (PlayerData.playerStates.Count == 0 || !PlayerData.playerStates.ContainsKey(playerID))
                            {
                                section = 3;

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
                                section = 4;

                                playerState.infected = bool.Parse(splitData[1]);
                            }
                            else if (splitData[0] == "PARASITE")
                            {
                                section = 5;

                                playerState.parasiteID = EntityID.FromString(splitData[1]);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "SAVESTATE_LOADGAME", section);
        }

        if (SaveStateData.saveStateData.Count == 0 || !SaveStateData.saveStateData.ContainsKey(self.saveStateNumber))
        {
            new SaveStateData.SaveStateDataContainer(self.saveStateNumber);
        }
    }
    internal static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        //Debug.Log("");
        //Debug.Log("SAVESTATE_SAVETOSTRING METHOD WAS CALLED!");
        //Debug.Log("");

        if (self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_START") && self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_END"))
        {
            int startIndex = self.unrecognizedSaveStrings.IndexOf("AA_SAVEDATA_START");
            int endIndex = self.unrecognizedSaveStrings.LastIndexOf("AA_SAVEDATA_END") + 1;

            self.unrecognizedSaveStrings.RemoveRange(startIndex, endIndex - startIndex);
        }

        self.unrecognizedSaveStrings.Add("AA_SAVEDATA_START");
        foreach (PlayerData.AAPlayerState playerState in PlayerData.playerStates.Values)
        {
            Dictionary<string, string> playerData = playerState.GetCycleData();

            //Debug.Log("");

            for (int i = 0; i < playerData.Count; i++)
            {
                string[] att = [playerData.ElementAt(i).Key, playerData.ElementAt(i).Value];

                //Debug.Log("SAVED AAPLAYERSTATE VALUE: " + att[0] + ", " + att[1]);

                self.AddUnrecognized(att);
            }
        }
        self.unrecognizedSaveStrings.Add("AA_SAVEDATA_END");

        //Debug.Log("");

        string postSaveString = orig(self);

        return postSaveString;
    }
    internal static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
        //Methods.Methods.LogMethodStart("SAVESTATE_SESSIONENDED");
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

        //Methods.Methods.LogMethodEnd();
    }
    internal static float SaveState_Get_SlowFadeIn(Func<SaveState, float> orig, SaveState self)
    {
        float origValue = orig(self);

        LogMethodStart("SAVESTATE_GET_SLOWFADEIN");
        LogMessage("ORIG VALUE: " + origValue);

        Data.SaveStateData.SaveStateDataContainer saveStateData = Data.SaveStateData.saveStateData[self.progression.PlayingAsSlugcat];
        if (saveStateData.infected)
        {
            LogMessage("PARASITE FOUND!");

            return Mathf.Max(origValue, 4f);
        }

        LogMethodEnd();

        return origValue;
    }
}
