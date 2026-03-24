using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEngine;
using Menu;
using System.Windows.Forms;

namespace ArchdruidsAdditions.Hooks
{
    public static class MenuHooks
    {
        public static bool mouseCursorVisible = false;
        internal static void MouseCursor_GrafUpdate(On.Menu.MouseCursor.orig_GrafUpdate orig, MouseCursor self, float timeStacker)
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
            /*
            if (Methods.Methods.forceDefaultMouseVisible)
            {
                //Cursor.visible = true;
            }
            else if (Methods.Methods.forceDefaultMouseHidden)
            {
                //Cursor.visible = false;
            }*/
        }

        internal static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);

            if (self.sceneID == MenuScene.SceneID.SleepScreen && self.menu is SleepAndDeathScreen sleepAndDeathScreen)
            {
                //Debug.Log("");
                //Debug.Log("METHOD MENUSCENE_BUILDSCENE WAS CALLED!");
                //Debug.Log("");

                SaveState saveState = sleepAndDeathScreen.myGamePackage.saveState;

                if (Data.SleepScreenData.sleepScreenData.Count == 0)
                {
                    Data.SleepScreenData.sleepScreenData.Add(new Data.SleepScreenData.SleepScreenDataContainer());
                }
                else
                {
                    Data.SleepScreenData.GetScreenData().animationTimer = 0;
                    Data.SleepScreenData.GetScreenData().shownParasite = true;
                }

                bool infected = false;
                int parasiteGrowth = 0;

                if (saveState != null)
                {
                    string[] position = saveState.denPosition.Split('_');

                    foreach (RegionState regionState in saveState.regionStates)
                    {
                        if (regionState != null && regionState.regionName.Contains(position[0]))
                        {
                            foreach (string stick in regionState.savedSticks)
                            {
                                if (stick != null && stick.Contains("paraStk"))
                                {
                                    string[] splitStick = Regex.Split(stick, "<stkA>");

                                    parasiteGrowth = int.Parse(splitStick[5], NumberStyles.Any, CultureInfo.InvariantCulture);

                                    infected = true;
                                }
                            }
                        }
                    }
                }

                //Debug.Log("PARASITE GROWTH: " + parasiteGrowth);
                //Debug.Log("");

                if (infected)
                {
                    Data.SleepScreenData.sleepScreenData[0].infected = true;
                    Data.SleepScreenData.sleepScreenData[0].growth = parasiteGrowth;

                    string newSceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Sleep Screen - Infected";
                    SlugcatStats.Name name = self.menu.manager.currentMainLoop is RainWorldGame game ? game.StoryCharacter : self.menu.manager.rainWorld.progression.PlayingAsSlugcat;

                    if (self.flatMode)
                    {
                        self.AddIllustration(new MenuIllustration(self.menu, self, newSceneFolder, "Sleep - Infected - Flat", new Vector2(23f, 17f), false, true));
                        self.flatIllustrations[self.flatIllustrations.Count - 1].alpha = 0f;
                    }
                    else if (name == SlugcatStats.Name.White
                        || name == SlugcatStats.Name.Yellow
                        || name == SlugcatStats.Name.Red
                        || (ModManager.MSC && (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand
                            || name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer
                            || name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear
                            || name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet
                            || name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint
                            || name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
                        || (ModManager.Watcher && (name == Watcher.WatcherEnums.SlugcatStatsName.Watcher)))
                    {
                        self.AddIllustration(new MenuDepthIllustration(self.menu, self, newSceneFolder, "Sleep - Infected - " + name.value, new Vector2(23f, 17f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
                        self.depthIllustrations[self.depthIllustrations.Count - 1].alpha = 0f;
                    }
                    else
                    {
                        self.AddIllustration(new MenuDepthIllustration(self.menu, self, newSceneFolder, "Sleep - Infected - Default", new Vector2(23f, 17f), 1.7f, MenuDepthIllustration.MenuShader.Normal));
                        self.depthIllustrations[self.depthIllustrations.Count - 1].alpha = 0f;
                    }
                }
                else
                {
                    Data.SleepScreenData.sleepScreenData.Clear();
                }
            }
        }
        internal static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
        {
            orig(self);
        }

        internal static void SleepAndDeathScreen_GetDataFromGame(On.Menu.SleepAndDeathScreen.orig_GetDataFromGame orig, SleepAndDeathScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
        {
            orig(self, package);

            if (self.ID == ProcessManager.ProcessID.SleepScreen && self.starvedLabel == null)
            {
                Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
                if (data != null && data.infected)
                {
                    data.infectedLabel = new(self, self.pages[0], "A parasite has infested your body. Your game has not been saved.", new Vector2(0f, 24f), new Vector2(1366f, 20f), true, null);
                    self.pages[0].subObjects.Add(data.infectedLabel);
                }
            }
        }
        internal static void SleepAndDeathScreen_Update(On.Menu.SleepAndDeathScreen.orig_Update orig, SleepAndDeathScreen self)
        {
            orig(self);

            Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
            if (data != null && data.infected)
            {
                MenuIllustration infectedIllustration2 = null;
                MenuIllustration normalIllustration2 = null;

                foreach (MenuObject obj in self.scene.subObjects)
                {
                    if (obj is MenuIllustration illustration)
                    {
                        string name = illustration.sprite.element.name;

                        if (name.Contains("Infected"))
                        {
                            infectedIllustration2 = illustration;
                        }
                        else if (name.Contains("2") || name.Contains("Screen"))
                        {
                            normalIllustration2 = illustration;
                        }
                    }
                }

                if (infectedIllustration2 != null && normalIllustration2 != null)
                {
                    float stallTime = 20;
                    float fadeTime = 250;
                    float maxTime = 500;

                    if (data.animationTimer > stallTime || data.shownParasite)
                    {
                        Vector2 pos = normalIllustration2.pos;

                        if (self.scene.flatMode)
                        { pos += new Vector2(281, -169); }
                        else
                        {
                            (infectedIllustration2 as MenuDepthIllustration).depth = (normalIllustration2 as MenuDepthIllustration).depth;

                            if (infectedIllustration2.sprite.element.name.Contains("Default"))
                            { pos += new Vector2(60f, 40f); }
                        }

                        infectedIllustration2.pos = pos;

                        float showParasiteAmount = data.shownParasite ? 0.5f : Mathf.Lerp(0, 0.5f, (data.animationTimer - stallTime) / fadeTime);
                        //Debug.Log(data.shownParasite);

                        infectedIllustration2.sprite.MoveInFrontOfOtherNode(normalIllustration2.sprite);
                        infectedIllustration2.alpha = showParasiteAmount;
                    }

                    if (data.animationTimer == stallTime)
                    {
                        self.PlaySound(SoundID.MENU_Enter_Death_Screen, 0f, 1f, 0.8f);
                    }

                    if (data.animationTimer < maxTime)
                    {
                        data.animationTimer++;
                    }
                }
                else
                {
                    data.animationTimer = 300;
                }
            }
        }
        internal static void SleepAndDeathScreen_GrafUpdate(On.Menu.SleepAndDeathScreen.orig_GrafUpdate orig, SleepAndDeathScreen self, float timeStacker)
        {
            orig(self, timeStacker);

            if (self.starvedLabel == null)
            {
                Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
                if (data != null && data.infected && data.infectedLabel != null)
                {
                    data.infectedLabel.label.color = Color.Lerp
                        (
                        Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey),
                        Color.red,
                        0.5f - 0.5f * Mathf.Sin(timeStacker + self.starvedWarningCounter / 30f * 3.1415927f * 2f)
                        );
                    data.infectedLabel.label.alpha = self.StarveLabelAlpha(timeStacker);
                }
            }
        }
        internal static bool SleepAndDeathScreen_AllowFoodMeterTick(Func<SleepAndDeathScreen, bool> orig, SleepAndDeathScreen self)
        {
            Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
            if (data != null && data.infected)
            {
                return data.animationTimer >= 300;
            }

            return orig(self);
        }
        internal static void SleepAndDeathScreen_FoodCountDownDone(On.Menu.SleepAndDeathScreen.orig_FoodCountDownDone orig, SleepAndDeathScreen self)
        {
            orig(self);

            if (self.starvedLabel == null)
            {
                Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
                if (data != null && data.infected)
                {
                    self.starvedWarningCounter = 0;
                }
            }
        }
        internal static void SleepAndDeathScreen_AddSubOjects(On.Menu.SleepAndDeathScreen.orig_AddSubObjects orig, SleepAndDeathScreen self)
        {
            orig(self);

            Data.SleepScreenData.SleepScreenDataContainer data = Data.SleepScreenData.GetScreenData();
            if (data != null && data.infected)
            {
            }
        }
    }
}
