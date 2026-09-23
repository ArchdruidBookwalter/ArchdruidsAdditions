using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchdruidsAdditions.Hooks;

public static class ShortcutHooks
{
    internal static void ShortcutHandler_Update(On.ShortcutHandler.orig_Update orig, ShortcutHandler self)
    {
        //LogMethodStart("ShortcutHandler_Update");

        orig(self);

        //LogMethodEnd();
    }
}
