using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArchdruidsAdditions.Objects;
using HUD;
using UnityEngine;
using ArchdruidsAdditions.Objects.HUDObjects;

namespace ArchdruidsAdditions.Data;

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

        public bool infected;
        public EntityID parasiteID;
        public SaveState infectedSaveState;

        public AAPlayerState(AbstractCreature player, PlayerState basePlayerState, int playerIndex)
        {
            this.playerIndex = playerIndex;

            if (player != null && basePlayerState != null)
            {
                RefreshPlayerState(player, basePlayerState);
            }
        }

        public Dictionary<string, string> GetCycleData()
        {
            Dictionary<string, string> saveStrings = [];

            saveStrings.Add("PLAYER", playerIndex.ToString());
            saveStrings.Add("INFECTED", infected.ToString());
            saveStrings.Add("PARASITE", parasiteID.ToString());

            return saveStrings;
        }

        public void ResetCycleOnlyData()
        {
            spiceAmount = 0;
            tooSpicy = false;
            spicyReactTimer = 0;
        }

        public void RefreshPlayerState(AbstractCreature newPlayer, PlayerState newPlayerState)
        {
            player = newPlayer;
            basePlayerState = newPlayerState;

            ResetCycleOnlyData();

            SlugcatStats.Name name = basePlayerState.slugcatCharacter;
            tolerance = Mathf.Max(Mathf.Min(SlugcatStats.SlugcatFoodMeter(name).x - 4, 4), 2);

            if (ModManager.MSC && name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            { tolerance = 4; }

            if (basePlayerState.isPup)
            { tolerance = 1; }
        }

        public void UpdateValue(string key, string value)
        {
            //Debug.Log("AAPLAYERDATA WAS UPDATED! KEY: " + key + ", VALUE: " + value);
        }
    }

    public static AAPlayerState GetPlayerStateFromAbstractCreature(AbstractCreature player)
    {
        foreach (AAPlayerState state in playerStates.Values)
        {
            if (state.player.ID == player.ID)
            { return state; }
        }
        return null;
    }

    public static Dictionary<int, AAPlayerState> playerStates = [];
    public static Dictionary<HUD.HUD, SpiceMeter> spiceMeters = [];
}
