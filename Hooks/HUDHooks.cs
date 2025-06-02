using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchdruidsAdditions.Objects;
using HUD;

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
}
