using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL.Menu;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks
{
    public static class MenuHooks
    {
        public static bool mouseCursorVisible = false;
        internal static void MouseCursor_GrafUpdate(On.Menu.MouseCursor.orig_GrafUpdate orig, Menu.MouseCursor self, float timeStacker)
        {
            orig(self, timeStacker);
            if (self.menu is Menu.TutorialControlsPage)
            {
                self.fade = 0f;
            }
            else
            {
                Methods.Methods.forceDefaultMouseHidden = true;
            }
        }
        internal static void MainLoopProcess_GrafUpdate(On.MainLoopProcess.orig_GrafUpdate orig, MainLoopProcess self, float timeStacker)
        {
            orig(self, timeStacker);
            if (Methods.Methods.forceDefaultMouseVisible)
            {
                //Cursor.visible = true;
            }
            else if (Methods.Methods.forceDefaultMouseHidden)
            {
                //Cursor.visible = false;
            }
        }
    }
}
