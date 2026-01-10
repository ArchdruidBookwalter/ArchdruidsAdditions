using Cursor = System.Windows.Forms.Cursor;
using Debug = UnityEngine.Debug;

namespace ArchdruidsAdditions.Hooks;

public static class ProcessHooks
{
    internal static void ProcessManager_Update(On.ProcessManager.orig_Update orig, ProcessManager self, float deltaTime)
    {
        orig(self, deltaTime);

        if (!Plugin.Options.useDefaultMouseCursor.Value)
        {
            Cursor.Hide();
        }
        else
        {
            Cursor.Show();
        }
    }
}
