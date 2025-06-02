using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cursor = System.Windows.Forms.Cursor;
using UnityEngine;

namespace ArchdruidsAdditions.Hooks;

public static class ProcessHooks
{
    public static bool forceDefaultMouseVisible = false;
    public static bool forceDefaultMouseHidden = false;
    public static bool forceGameMouseVisible = false;
    public static bool forceGameMouseHidden = false;
    internal static void ProcessManager_Update(On.ProcessManager.orig_Update orig, ProcessManager self, float deltaTime)
    {
        orig(self, deltaTime);
        if (forceDefaultMouseVisible)
        { Cursor.Show(); }
        else
        { Cursor.Hide(); }
    }
}
