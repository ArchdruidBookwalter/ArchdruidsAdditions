using System.Collections.Generic;
using ArchdruidsAdditions.Objects.HUDObjects;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
using ArchdruidsAdditions.Objects.PhysicalObjects.Items;

namespace ArchdruidsAdditions.Data;

public static class PlayerData
{
    public static bool DisableDevToolsForRainWorldGame = false;
    public static bool DevToolsOn = false;
    public static bool TextBeingInputted = false;
    public static bool DisableLogs = false;

    public class AAPlayerState
    {
        public AbstractCreature player;
        public PlayerState basePlayerState;

        public bool recordFoodPips;
        public int foodPipsConsumed;
        public int quarterFoodPipsConsumed;
        public int spiceAmount;
        public bool tooSpicy;

        public int spicyReactTimer;

        public bool infected;
        public EntityID parasiteID;
        public SaveState infectedSaveState;
        public ParasiteIllnessEffect parasiteIllnessEffect;
        public float parasiteMalnourishment;
        public bool previousMalnourishment1;
        public bool previousMalnourishment2;
        public int parasiteKillCounter;

        public AbstractParasiteStick parasiteStick
        {
            get
            {
                foreach (AbstractPhysicalObject.AbstractObjectStick stick in player.stuckObjects)
                {
                    if (stick is AbstractParasiteStick paraStick)
                    {
                        return paraStick;
                    }
                }
                return null;
            }
        }

        public AAPlayerState(AbstractCreature player, PlayerState basePlayerState)
        {
            if (player != null && basePlayerState != null)
            {
                RefreshPlayerState(player, basePlayerState);
            }
        }

        public Dictionary<string, string> GetCycleData()
        {
            Dictionary<string, string> saveStrings = [];

            saveStrings.Add("PLAYER", player.ID.number.ToString());
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

            if (parasiteStick != null)
            {
                parasiteMalnourishment = 1f;
            }
        }

        public void UpdateValue(string key, string value)
        {
            //Debug.Log("AAPLAYERDATA WAS UPDATED! KEY: " + key + ", VALUE: " + value);
        }

        public static bool SlugcatReactsToSpice(SlugcatStats.Name name)
        {
            if (ModManager.MSC && name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear)
            { return false; }
            return true;
        }

        public static int SlugcatSpiceTolerance(SlugcatStats.Name name, bool isPup)
        {
            if (isPup)
            { return 1; }

            if (ModManager.MSC && name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            { return 4; }

            return Mathf.Max(Mathf.Min(SlugcatStats.SlugcatFoodMeter(name).x - 4, 4), 2);
        }
    }
    public static AAPlayerState GetPlayerState(int id)
    {
        if (playerStates.ContainsKey(id)) return playerStates[id];
        return null;
    }

    public class SpearShotByBow
    {
        public Room room;
        public Spear spear;
        public Vector2 shootDir;
        public Vector2 lastVel;
        public int tickCounter;

        public SpearShotByBow(Room room, Spear spear, Vector2 shootDir)
        {
            this.room = room;
            this.spear = spear;
            this.shootDir = shootDir;
        }
    }

    public class SpearLoadedInBow
    {
        public Room room;
        public Spear spear;
        public Bow bow;

        public SpearLoadedInBow(Room room, Spear spear, Bow bow)
        {
            this.room = room;
            this.spear = spear;
            this.bow = bow;
        }
    }

    public class ScavData
    {
        public Scavenger scav;

        public Bow GetBow()
        {
            foreach (Creature.Grasp grasp in scav.grasps)
            {
                if (grasp != null && grasp.grabbed is Bow bow)
                {
                    return bow;
                }
            }
            return null;
        }

        public Spear GetSpear()
        {
            foreach (Creature.Grasp grasp in scav.grasps)
            {
                if (grasp != null && grasp.grabbed is Spear spear)
                {
                    return spear;
                }
            }
            return null;
        }

        public ScavData(Scavenger scav)
        {
            this.scav = scav;
        }
    }

    public static Dictionary<Spear, SpearShotByBow> spearsShotByBows = [];
    public static Dictionary<Scavenger, ScavData> scavData = [];
    public static Dictionary<Spear, Bow> spearsLoadedInBows = [];
    public static Dictionary<int, AAPlayerState> playerStates = [];
    public static Dictionary<HUD.HUD, SpiceMeter> spiceMeters = [];
}
