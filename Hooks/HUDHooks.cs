using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using UnityEngine;
using RWCustom;
using HUD;
using static ArchdruidsAdditions.Methods.Methods;

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
            PlayerData.PlayerData.spiceMeters.Add(hud, spiceMeter);
        }
    }
    internal static void FoodMeter_Update(On.HUD.FoodMeter.orig_Update orig, HUD.FoodMeter self)
    {
        orig(self);

        if (self.hud.owner is Player)
        {
            SpiceMeter spiceMeter = PlayerData.PlayerData.spiceMeters[self.hud];
            spiceMeter.Update(self);

            PlayerData.PlayerData.AAPlayerState playerState = PlayerData.PlayerData.playerStates[(self.hud.owner as Player).abstractCreature];
            if (playerState.tooSpicy)
            { self.visibleCounter = 80; }
        }
    }
    internal static void FoodMeter_Draw(On.HUD.FoodMeter.orig_Draw orig, HUD.FoodMeter self, float timeStacker)
    {
        orig(self, timeStacker);
    }
    internal static void FoodMeter_MeterCircle_Draw(On.HUD.FoodMeter.MeterCircle.orig_Draw orig, HUD.FoodMeter.MeterCircle self, float timeStacker)
    {
        orig(self, timeStacker);

        if (self.meter.hud.owner is Player)
        {
            int circleIndex = self.meter.circles.IndexOf(self);
            SpiceMeter spiceMeter = PlayerData.PlayerData.spiceMeters[self.meter.hud];
            SpiceMeter.SpiceCircle spiceCircle = spiceMeter.circles[circleIndex];

            spiceCircle.Draw(self);
        }
    }
}
