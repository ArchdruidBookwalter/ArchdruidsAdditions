using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class SaveStateHooks
{
    internal static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string s, RainWorldGame game)
    {
        orig(self, s, game);

        Debug.Log("");
        Debug.Log("SAVESTATE_LOADGAME METHOD WAS CALLED!");
        Debug.Log("");

        bool readData = false;
        PlayerData.PlayerData.AAPlayerState playerState = null;

        for (int i = 0; i < self.unrecognizedSaveStrings.Count; i++)
        {
            if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_START")
            {
                readData = true;
            }
            else if (self.unrecognizedSaveStrings[i] == "AA_SAVEDATA_END")
            {
                readData = false;
            }

            if (readData)
            {
                //Debug.Log("");
                string[] data = Regex.Split(self.unrecognizedSaveStrings[i], "<svB>");

                if (data[0] == "PLAYER")
                {
                    int playerIndex = int.Parse(data[1]);
                    playerState = PlayerData.PlayerData.GetPlayerStateFromIndex(playerIndex);
                    playerState?.ResetCycleOnlyData();
                }
                else
                {
                    playerState?.UpdateValue(data[0], data[1]);
                }

                //Debug.Log("");
            }
        }
    }
    internal static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
        Debug.Log("");
        Debug.Log("SAVESTATE_SAVETOSTRING METHOD WAS CALLED!");
        Debug.Log("");

        if (self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_START") && self.unrecognizedSaveStrings.Contains("AA_SAVEDATA_END"))
        {
            int startIndex = self.unrecognizedSaveStrings.IndexOf("AA_SAVEDATA_START");
            int endIndex = self.unrecognizedSaveStrings.LastIndexOf("AA_SAVEDATA_END") + 1;

            self.unrecognizedSaveStrings.RemoveRange(startIndex, endIndex - startIndex);
        }

        self.unrecognizedSaveStrings.Add("AA_SAVEDATA_START");
        foreach (PlayerData.PlayerData.AAPlayerState playerState in PlayerData.PlayerData.playerStates.Values)
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
}
