using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Menu;

using ArchdruidsAdditions.Objects.HUDObjects;

namespace ArchdruidsAdditions.Data;

public static class SleepScreenData
{
    public static List<SleepScreenDataContainer> sleepScreenData = [];
    public class SleepScreenDataContainer()
    {
        public bool infected = true;
        public bool shownParasite = false;
        public int animationTimer = 0;
        public int growth = 0;
        public MenuMicrophone.MenuSoundLoop soundLoop;
        public ParasiteGrowthMeter parasiteGrowthMeter;
        public MenuLabel infectedLabel;
    }
    public static SleepScreenDataContainer GetScreenData()
    {
        if (sleepScreenData.Count > 0)
        {
            return sleepScreenData[0];
        }
        return null;
    }
}
