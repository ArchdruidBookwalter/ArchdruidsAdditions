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
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Unity.Mathematics;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ArchdruidsAdditions;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("fyre.BeastMaster", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "archdruidbookwalter.archdruidsadditions";
    public const string PLUGIN_NAME = "ArchdruidsAdditions";
    public const string PLUGIN_VERSION = "1.0.0";

    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    public void OnEnable()
    {
        #region AbstractPhysicalObject Hooks
        On.AbstractPhysicalObject.Realize += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Realize;
        On.AbstractConsumable.IsTypeConsumable += Hooks.AbstractPhysicalObjectHooks.AbstractConsumable_IsTypeConsumable;
        #endregion

        #region Devtools Hooks
        On.DevInterface.ObjectsPage.CreateObjRep += Hooks.DevtoolsHooks.ObjectsPage_CreateObjRep;
        On.DevInterface.Panel.CopyToClipboard += Hooks.DevtoolsHooks.Panel_CopyToClipboard;
        On.DevInterface.Panel.PasteFromClipboard += Hooks.DevtoolsHooks.Panel_PasteFromClipboard;
        On.PlacedObject.GenerateEmptyData += Hooks.DevtoolsHooks.PlacedObject_GenerateEmptyData;
        #endregion

        #region HUDHooks
        On.HUD.HUD.InitSinglePlayerHud += Hooks.HUDHooks.HUD_InitSinglePlayerHud;
        On.HUD.HUD.InitMultiplayerHud += Hooks.HUDHooks.HUD_InitMultiplayerHud;
        #endregion

        #region Insect Hooks
        On.MiniFly.Update += Hooks.InsectHooks.MiniFly_Update;
        On.RedSwarmer.Update += Hooks.InsectHooks.RedSwarmer_Update;
        #endregion

        #region Item Symbol Hooks
        On.ItemSymbol.SpriteNameForItem += Hooks.ItemSymbolHooks.ItemSymbol_SpriteNameForItem;
        On.ItemSymbol.ColorForItem += Hooks.ItemSymbolHooks.ItemSymbol_ColorForItem;
        #endregion

        #region Iterator Hooks
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += Hooks.IteratorHooks.On_MoonConversation_AddEvents;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += Hooks.IteratorHooks.On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        On.SLOracleBehavior.Update += Hooks.IteratorHooks.On_SLOracleBehavior_Update;
        #endregion

        #region Limb Hooks
        On.SlugcatHand.Update += Hooks.LimbHooks.SlugcatHand_Update;
        On.Limb.FindGrip += Hooks.LimbHooks.Limb_FindGrip;
        #endregion

        #region Main Hooks
        On.RainWorld.OnModsInit += Hooks.MainHooks.RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += Hooks.MainHooks.RainWorld_UnloadResources;
        On.RainWorld.OnModsEnabled += Hooks.MainHooks.RainWorld_OnModsEnabled;
        On.RainWorld.OnModsDisabled += Hooks.MainHooks.RainWorld_OnModsDisabled;
        On.RainWorld.PostModsInit += Hooks.MainHooks.RainWorld_PostModsInIt;
        #endregion

        #region Menu Hooks
        On.Menu.MouseCursor.GrafUpdate += Hooks.MenuHooks.MouseCursor_GrafUpdate;
        On.MainLoopProcess.GrafUpdate += Hooks.MenuHooks.MainLoopProcess_GrafUpdate;
        #endregion

        #region Player
        On.Player.GetHeldItemDirection += Hooks.PlayerHooks.Player_GetHeldItemDirection;
        On.Player.Grabability += Hooks.PlayerHooks.Player_Grabability;
        On.Player.IsObjectThrowable += Hooks.PlayerHooks.Player_IsObjectThrowable;
        On.Player.PickupCandidate += Hooks.PlayerHooks.Player_PickupCandidate;
        On.Player.SlugcatGrab += Hooks.PlayerHooks.Player_SlugcatGrab;
        On.Player.ThrowObject += Hooks.PlayerHooks.Player_ThrowObject;
        //On.Player.Update += Hooks.PlayerHooks.Player_Update;
        On.PlayerGraphics.DrawSprites += Hooks.PlayerHooks.PlayerGraphics_DrawSprites;
        #endregion

        #region Process Hooks
        On.ProcessManager.Update += Hooks.ProcessHooks.ProcessManager_Update;
        #endregion

        #region Rain World Hooks
        //On.RainWorld.Update += Hooks.RainWorldHooks.RainWorld_Update;
        #endregion

        #region Room Hooks
        On.Room.Loaded += Hooks.RoomHooks.Room_Loaded;
        #endregion

        #region Scavenger Hooks
        new Hook(typeof(Scavenger).GetMethod("get_HeadLookPoint"), Hooks.ScavengerHooks.Scavenger_get_HeadLookPoint);
        new Hook(typeof(Scavenger).GetMethod("get_EyesLookPoint"), Hooks.ScavengerHooks.Scavenger_get_EyesLookPoint);
        On.Scavenger.Update += Hooks.ScavengerHooks.Scavenger_Update;
        On.Scavenger.MidRangeUpdate += Hooks.ScavengerHooks.Scavenger_MidRangeUpdate;
        On.Scavenger.Throw += Hooks.ScavengerHooks.Scavenger_Throw;
        On.Scavenger.TryToMeleeCreature += Hooks.ScavengerHooks.Scavenger_TryToMeleeCreature;
        On.Scavenger.TryThrow_BodyChunk_ViolenceType += Hooks.ScavengerHooks.Scavenger_TryThrow;
        On.Scavenger.PickUpAndPlaceInInventory += Hooks.ScavengerHooks.Scavenger_PickUpAndPlaceInInventory;
        On.Scavenger.ReleaseGrasp += Hooks.ScavengerHooks.Scavenger_ReleaseGrasp;
        On.ScavengerGraphics.ContainerForHeldItem += Hooks.ScavengerHooks.ScavengerGraphics_ContainerForHeldItem;
        On.ScavengerGraphics.DrawSprites += Hooks.ScavengerHooks.ScavengerGraphics_DrawSprites;
        On.ScavengerGraphics.ScavengerHand.Update += Hooks.ScavengerHooks.ScavengerHand_Update;
        On.ScavengerGraphics.ScavengerHand.StandardLocomotionProcedure += Hooks.ScavengerHooks.ScavengerHand_StandardLocomotionProcedure;
        On.ScavengerAI.CheckThrow += Hooks.ScavengerHooks.ScavengerAI_CheckThrow;
        On.ScavengerAI.CollectScore_PhysicalObject_bool += Hooks.ScavengerHooks.ScavengerAI_CollectScore;
        On.ScavengerAI.WeaponScore += Hooks.ScavengerHooks.ScavengerAI_WeaponScore;
        On.ScavengerAI.CheckForScavangeItems += Hooks.ScavengerHooks.ScavengerAI_CheckForScavengeItems;
        On.ScavengerAI.PickUpItemScore += Hooks.ScavengerHooks.ScavengerAI_PickUpItemScore;
        On.ScavengerAI.RetrieveWeapon += Hooks.ScavengerHooks.ScavengerAI_RetrieveWeapon;
        On.ScavengerAI.AttackBehavior += Hooks.ScavengerHooks.ScavengerAI_AttackBehavior;
        On.ScavengerTreasury.ctor += Hooks.ScavengerHooks.ScavengerTreasury_ctor;
        #endregion

        #region Slugpup Hooks
        On.MoreSlugcats.SlugNPCAI.GetFoodType += Hooks.SlugpupHooks.SlugNPCAI_GetFoodType;
        #endregion

        #region Spear Hooks
        On.Spear.DrawSprites += Hooks.SpearHooks.Spear_DrawSprites;
        On.Spear.Update += Hooks.SpearHooks.Spear_Update;
        On.Spear.HitSomething += Hooks.SpearHooks.Spear_HitSomething;
        //On.Spear.Thrown += Hooks.SpearHooks.Spear_Thrown;
        #endregion

        #region Weapon Hooks
        On.Weapon.Thrown += Hooks.WeaponHooks.Weapon_Thrown;
        On.Weapon.HitWall += Hooks.WeaponHooks.Weapon_HitWall;
        On.Weapon.Update += Hooks.WeaponHooks.Weapon_Update;
        #endregion

        On.Player.Update += Hooks.PlayerHooks.Player_Update;

        Type[] types = [typeof(RoomCamera.SpriteLeaser), typeof(RoomCamera), typeof(float), typeof(float2)];
        new Hook(typeof(ScavengerGraphics.ScavengerHand).GetMethod("DrawSprites", types), Hooks.ScavengerHooks.ScavengerHand_DrawSprites);
    }
}