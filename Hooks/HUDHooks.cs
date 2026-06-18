using System;
using System.Collections.Generic;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.HUDObjects;
using HUD;
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
        //LogMethodStart("HUD_INITSLEEPHUD");

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

        //LogMethodEnd();
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
        //Methods.Methods.LogMethodStart("FOODMETER_CTOR");

        orig(self, hud, maxFood, survivalLimit, associatedPup, pupNumber);

        //Methods.Methods.LogMessage("SURVIVAL LIMIT: " + survivalLimit);

        if (hud.owner is Player player && associatedPup == null)
        {
            SpiceMeter spiceMeter = new(hud, maxFood);
            PlayerData.spiceMeters.Add(hud, spiceMeter);
        }

        //Methods.Methods.LogMethodEnd();
    }
    internal static void FoodMeter_Update(On.HUD.FoodMeter.orig_Update orig, HUD.FoodMeter self)
    {
        orig(self);

        float section = 1;

        try
        {
            if (self.hud.owner is Player player)
            {
                section = 2;

                SpiceMeter spiceMeter = PlayerData.spiceMeters[self.hud];

                section = 3;

                spiceMeter?.Update(self);

                section = 4;

                PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState((self.hud.owner as Player).abstractCreature.ID.number);
                if (playerState != null && playerState.tooSpicy)
                { self.visibleCounter = 80; }
            }
        }
        catch (Exception e)
        {
            Methods.Methods.Log_Exception(e, "FOODMETER_UPDATE", section);
        }
    }
    internal static void FoodMeter_SleepUpdate(On.HUD.FoodMeter.orig_SleepUpdate orig, HUD.FoodMeter self)
    {
        //LogMethodStart("FOODMETER_SLEEPUPDATE");

        //LogMessage("MAX FOOD: " + self.maxFood);
        //LogMessage("SURVIVAL LIMIT: " + self.survivalLimit);

        orig(self);

        //LogMethodEnd();
    }
    internal static void FoodMeter_MoveSurvivalLimit(On.HUD.FoodMeter.orig_MoveSurvivalLimit orig, HUD.FoodMeter self, float to, bool smooth)
    {
        orig(self, to, smooth);
    }
    internal static void FoodMeter_Draw(On.HUD.FoodMeter.orig_Draw orig, HUD.FoodMeter self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.hud.owner is Player player)
        {
            SpiceMeter spiceMeter = PlayerData.spiceMeters[self.hud];
            spiceMeter?.Draw(self, timeStacker);
        }
    }
    internal static void FoodMeter_MeterCircle_Draw(On.HUD.FoodMeter.MeterCircle.orig_Draw orig, HUD.FoodMeter.MeterCircle self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.meter.hud.owner is Player)
        {
            SpiceMeter spiceMeter = PlayerData.spiceMeters[self.meter.hud];
            if (spiceMeter != null)
            {
                int index = self.meter.circles.IndexOf(self);
                SpiceMeter.SpiceMeterCircle spiceCircle = spiceMeter.circles[index];
                spiceCircle.Draw(self.circles[0], timeStacker);
            }
        }
    }
}
