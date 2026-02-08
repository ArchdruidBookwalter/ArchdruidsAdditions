using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArchdruidsAdditions.Objects;
using HUD;
using UnityEngine;

namespace ArchdruidsAdditions.PlayerData;

public static class PlayerData
{
    public class AAPlayerState
    {
        public AbstractCreature player;
        public PlayerState basePlayerState;
        public int playerIndex;

        public int spiceAmount;
        public int tolerance;
        public bool tooSpicy;

        public int spicyReactTimer;

        public AAPlayerState(AbstractCreature player, PlayerState basePlayerState, int playerIndex)
        {
            this.player = player;
            this.basePlayerState = basePlayerState;
            this.playerIndex = playerIndex;

            spiceAmount = 0;
            tooSpicy = false;
            spicyReactTimer = 0;

            SlugcatStats.Name name = basePlayerState.slugcatCharacter;
            tolerance = Mathf.Max(Mathf.Min(SlugcatStats.SlugcatFoodMeter(name).x - 4, 4), 2);

            if (ModManager.MSC && name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            { tolerance = 4; }

            if (basePlayerState.isPup)
            { tolerance = 1; }
        }
        
        public Dictionary<string, string> GetCycleData()
        {
            Dictionary<string, string> saveStrings = [];

            saveStrings.Add("PLAYER", playerIndex.ToString());

            return saveStrings;
        }

        public void ResetCycleOnlyData()
        {
            spiceAmount = 0;
            tooSpicy = false;
            spicyReactTimer = 0;
        }

        public void UpdateValue(string key, string value)
        {
            //Debug.Log("AAPLAYERDATA WAS UPDATED! KEY: " + key + ", VALUE: " + value);
        }
    }

    public static AAPlayerState GetPlayerStateFromIndex(int playerIndex)
    {
        foreach (AAPlayerState state in playerStates.Values)
        {
            if (state.playerIndex == playerIndex)
            { return state; }
        }
        return null;
    }

    public static Dictionary<AbstractCreature, AAPlayerState> playerStates = [];
    public static Dictionary<HUD.HUD, SpiceMeter> spiceMeters = [];
}
