using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using HUD;
using static ArchdruidsAdditions.Methods.Methods;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.HUDObjects;
using Menu;

namespace ArchdruidsAdditions.Hooks;

public static class HUDHooks
{
    internal static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera camera)
    {
        orig(self, camera);
        self.AddPart(new Cursors.PointCursor(self, null));
    }
    internal static void HUD_InitMultiplayerHud(On.HUD.HUD.orig_InitMultiplayerHud orig, HUD.HUD self, ArenaGameSession session)
    {
        orig(self, session);
        List<PlayerSpecificMultiplayerHud> multiHuds = [];
        foreach (HudPart part in self.parts)
        {
            if (part is PlayerSpecificMultiplayerHud multiHud)
            {
                multiHuds.Add(multiHud);
            }
        }
        foreach (PlayerSpecificMultiplayerHud multiHud in multiHuds)
        {
            self.AddPart(new Cursors.PointCursor(self, multiHud));
        }
    }
    internal static void HUD_InitSleepHud(On.HUD.HUD.orig_InitSleepHud orig, HUD.HUD self, SleepAndDeathScreen screen, Map.MapData mapData, SlugcatStats stats)
    {
        orig(self, screen, mapData, stats);

        if (screen.ID == ProcessManager.ProcessID.SleepScreen)
        {
            Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
            if (data != null && data.infected)
            {
                if (!data.shownParasite)
                {
                    screen.forceWatchAnimation = true;
                }

                data.parasiteGrowthMeter = new(self)
                {
                    points = data.growth
                };
                self.AddPart(data.parasiteGrowthMeter);
            }
        }
    }
    internal static void HUD_Update(On.HUD.HUD.orig_Update orig, HUD.HUD self)
    {
        orig(self);
        if (self.owner.GetOwnerType() == HUD.HUD.OwnerType.Player)
        {
            bool hasPointer = false;
            foreach (HudPart part in self.parts)
            {
                if (part is Cursors.PointCursor)
                { hasPointer = true; }
            }
            if (!hasPointer)
            {
                self.AddPart(new Cursors.PointCursor(self, null));
            }
        }
    }
    internal static void FoodMeter_ctor(On.HUD.FoodMeter.orig_ctor orig, FoodMeter self, HUD.HUD hud, int maxFood, int survivalLimit, Player associatedPup, int pupNumber)
    {
        orig(self, hud, maxFood, survivalLimit, associatedPup, pupNumber);

        if (hud.owner is Player && associatedPup == null)
        {
            SpiceMeter spiceMeter = new(hud, maxFood);
            PlayerData.spiceMeters.Add(hud, spiceMeter);
        }
    }
    internal static void FoodMeter_Update(On.HUD.FoodMeter.orig_Update orig, HUD.FoodMeter self)
    {
        orig(self);

        if (self.hud.owner is Player)
        {
            SpiceMeter spiceMeter = PlayerData.spiceMeters[self.hud];
            spiceMeter.Update(self);

            PlayerData.AAPlayerState playerState = PlayerData.GetPlayerStateFromAbstractCreature((self.hud.owner as Player).abstractCreature);
            if (playerState.tooSpicy)
            { self.visibleCounter = 80; }
        }

        if (self.hud.owner is SleepAndDeathScreen sleepScreen && sleepScreen.AllowFoodMeterTick && !sleepScreen.karmaLadder.startedAnimating)
        {
            /*
            Debug.Log("");
            Debug.Log("METHOD FOODMETER_UPDATE WAS CALLED!");

            foreach (FoodMeter.MeterCircle circle in self.circles)
            {
                int index = self.circles.IndexOf(circle);
                HUDCircle circle1 = circle.circles[0];
                HUDCircle circle2 = circle.circles[1];

                string circleIndexString = "CIRCLE " + index + ": ";
                string radsString = string.Format("RADS: {0:N2}-{1:N2}", circle1.rad, circle2.rad);
                string fadesString = string.Format("FADES: {0:N2}-{1:N2}", circle1.fade, circle2.fade);
                string flopped = string.Format("PLOPPED: {0}", circle.plopped);

                Debug.Log(string.Format("{0, -20}{1, -20}{2, -20}{3, -20}", circleIndexString, radsString, fadesString, flopped));
            }*/
        }
    }
    internal static void FoodMeter_Draw(On.HUD.FoodMeter.orig_Draw orig, HUD.FoodMeter self, float timeStacker)
    {
        orig(self, timeStacker);
    }

    internal static void FoodMeter_MeterCircle_Draw(On.HUD.FoodMeter.MeterCircle.orig_Draw orig, HUD.FoodMeter.MeterCircle self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.meter.hud.owner is SleepAndDeathScreen sleepAndDeathScreen)
        {

        }

        if (self.meter.hud.owner is Player)
        {
            int circleIndex = self.meter.circles.IndexOf(self);
            SpiceMeter spiceMeter = PlayerData.spiceMeters[self.meter.hud];
            SpiceMeter.SpiceCircle spiceCircle = spiceMeter.circles[circleIndex];

            spiceCircle.Draw(self);
        }
    }
}
