using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;
using HarmonyLib;
using System.Windows.Forms.VisualStyles;

namespace ArchdruidsAdditions.Configuration
{
    public class PluginOptions : OptionInterface
    {
        public PluginOptions()
        {
            aimBowControls = config.Bind<string>
            ("AimBowControls", "Mouse",
                new ConfigurableInfo
                (
                    "Preferred controls type to be used when aiming a bow.",
                    new ConfigAcceptableList<string>(["Mouse", "Directional Inputs"]),
                    "",
                    []    
                )
            );
            spawnBowsEverywhere = config.Bind<bool>
            ("SpawnBowsEverywhere", false,
                new ConfigurableInfo
                (
                    "Enable this to make bows spawn in all regions."
                )
            );
            useDefaultMouseCursor = config.Bind<bool>
            ("UseDefaultMouseCursor", false,
                new ConfigurableInfo
                (
                    "Enabling this might help if the mouse cursor bugs out and becomes invisible or unusable. The game should be restarted after changing this setting."
                )
            );
        }
        public readonly Configurable<string> aimBowControls;
        public readonly Configurable<bool> spawnBowsEverywhere;
        public readonly Configurable<bool> useDefaultMouseCursor;

        public override void Initialize()
        {
            try
            {
                OpTab optionsTab = new(this, "Options");
                Tabs = [optionsTab];

                Vector2 startPos = new(300f, 520f);
                optionsTab.AddItems(
                [
                    new OpLabel(new Vector2(startPos.x - 150f, startPos.y), new Vector2(300f, 30f), "~ Archdruid's Additions : Options ~", FLabelAlignment.Center, true)
                    {
                        verticalAlignment = OpLabel.LabelVAlignment.Center,
                    }
                ]);
                optionsTab.AddItems(
                [
                    new OpLabel(startPos + new Vector2(-305f, -52f), new Vector2(300f, 30f), "Bow Controls:", FLabelAlignment.Right, false)
                    {
                        verticalAlignment = OpLabel.LabelVAlignment.Center,
                        description = aimBowControls.info.description,
                    },
                    new OpComboBox(aimBowControls, startPos + new Vector2(5f, -50f), 140f)
                    {
                    }
                ]);
                optionsTab.AddItems(
                [
                    new OpLabel(startPos + new Vector2(-305f, -82f), new Vector2(300f, 30f), "Spawn Bows Everywhere:", FLabelAlignment.Right, false)
                    {
                        verticalAlignment = OpLabel.LabelVAlignment.Center,
                        description = spawnBowsEverywhere.info.description,
                    },
                    new OpCheckBox(spawnBowsEverywhere, startPos + new Vector2(5f, -80f))
                    {
                    }
                ]);
                optionsTab.AddItems(
                [
                    new OpLabel(startPos + new Vector2(-305f, -112f), new Vector2(300f, 30f), "Use Default Mouse Cursor:", FLabelAlignment.Right, false)
                    {
                        verticalAlignment = OpLabel.LabelVAlignment.Center,
                        description = useDefaultMouseCursor.info.description,
                    },
                    new OpCheckBox(useDefaultMouseCursor, startPos + new Vector2(5f, -110f))
                    {
                    }
                ]);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex); 
            }
        }
        public void AddPoint(OpTab tab, Vector2 pos)
        {
            Color color = new(1f, 0f, 0f);
            tab.AddItems(new UIelement[]
            {
                new OpImage(pos, "pixel")
                {
                    color = color
                }
            });
        }
    }
}
