using System;
using System.Globalization;
using BepInEx;
using DevInterface;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection.Emit;

namespace ArchdruidsAdditions;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "archdruidbookwalter.archdruidsadditions";
    public const string PLUGIN_NAME = "ArchdruidsAdditions";
    public const string PLUGIN_VERSION = "1.0.0";

    public void OnEnable()
    {
        #region AbstractPhysicalObject Hooks
        On.AbstractPhysicalObject.Realize += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Realize;
        On.AbstractConsumable.IsTypeConsumable += Hooks.AbstractPhysicalObjectHooks.AbstractConsumable_IsTypeConsumable;
        #endregion

        #region Devtools Hooks
        On.DevInterface.ObjectsPage.CreateObjRep += Hooks.DevtoolsHooks.ObjectsPage_CreateObjRep;
        On.PlacedObject.GenerateEmptyData += Hooks.DevtoolsHooks.PlacedObject_GenerateEmptyData;
        #endregion

        #region Item Symbol Hooks
        On.ItemSymbol.SpriteNameForItem += Hooks.ItemSymbolHooks.ItemSymbol_SpriteNameForItem;
        On.ItemSymbol.ColorForItem += Hooks.ItemSymbolHooks.ItemSymbol_ColorForItem;
        #endregion

        #region Iterator Hooks
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += Hooks.IteratorHooks.On_MoonConversation_AddEvents;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += Hooks.IteratorHooks.On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        #endregion

        #region Main Hooks
        On.RainWorld.OnModsInit += Hooks.MainHooks.RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += Hooks.MainHooks.RainWorld_UnloadResources;
        On.RainWorld.OnModsEnabled += Hooks.MainHooks.RainWorld_OnModsEnabled;
        On.RainWorld.OnModsDisabled += Hooks.MainHooks.RainWorld_OnModsDisabled;
        #endregion

        #region Player
        On.Player.IsObjectThrowable += Hooks.PlayerHooks.Player_IsObjectThrowable;
        On.Player.ThrowObject += Hooks.PlayerHooks.Player_ThrowObject;
        #endregion

        #region Room Hooks
        On.Room.Loaded += Hooks.RoomHooks.Room_Loaded;
        #endregion
    }
}