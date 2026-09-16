using System;
using ArchdruidsAdditions.Data;
using ArchdruidsAdditions.Objects.PhysicalObjects.Creatures;
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

    internal static void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float deltaTime)
    {
        if (self is RainWorldGame game)
        {
            if (PlayerData.DisableDevToolsForRainWorldGame)
            {
                //Debug.Log("RESTORED DEVTOOLS");
                game.devToolsActive = PlayerData.DevToolsOn;

                PlayerData.DisableDevToolsForRainWorldGame = false;
            }

            foreach (AbstractCreature player in game.session.Players)
            {
                Data.PlayerData.AAPlayerState playerState = PlayerData.GetPlayerState(player.ID.number);

                foreach (AbstractPhysicalObject.AbstractObjectStick stick in player.stuckObjects)
                {
                    if (stick is AbstractParasiteStick)
                    {
                        if (playerState != null && playerState.parasiteIllnessEffect != null && playerState.parasiteIllnessEffect.intensity > 0)
                        {
                            float newFPS = Mathf.Lerp(40f, 15f, playerState.parasiteIllnessEffect.intensity / 10000);

                            self.framesPerSecond = Math.Min(self.framesPerSecond, Mathf.RoundToInt(newFPS));
                        }
                        break;
                    }    
                }
            }
        }

        orig(self, deltaTime);
    }
}
