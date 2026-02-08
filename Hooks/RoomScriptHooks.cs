using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreSlugcats;

namespace ArchdruidsAdditions.Hooks;

public static class RoomScriptHooks
{
    internal static void RoomSpecificScript_SU_CO4StartUp_Update(On.RoomSpecificScript.SU_C04StartUp.orig_Update orig, RoomSpecificScript.SU_C04StartUp self, bool eu)
    {
        orig(self, eu);
        if (ModManager.MMF && MMF.cfgExtraTutorials.Value == false)
        {
            self.showControlsCounter = 0;
        }
    }
}
