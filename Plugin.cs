using System;
using BepInEx;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using EffExt;

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

    public static Configuration.PluginOptions Options;
    public static RegionData.RegionData RegionData;

    public Plugin()
    {
        try { Options = new Configuration.PluginOptions(); }
        catch (Exception ex) { Debug.LogException(ex); }
    }


    public void OnEnable()
    {
        EffectDefinitionBuilder builder = new EffectDefinitionBuilder("ForceRoomEnergy");
        builder.SetEffectInitializer(Effects.LightRodPowerEffect.EffectSpawner);
        builder.AddFloatField("DriftStrength", 0f, 0.1f, 0f, 0f, "DriftStrength");
        builder.AddIntField("DriftMode", 0, 2, 0, "DriftMode");
        builder.AddFloatField("ResetChance", 0f, 100f, 0f, 0f, "ResetChance");
        builder.AddFloatField("ResetCooldown", 0f, 2000f, 0f, 0f, "ResetCD");
        builder.SetCategory("AAEffects");
        builder.Register();

        #region AbstractPhysicalObject Hooks
        On.AbstractPhysicalObject.Realize += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Realize;
        On.AbstractConsumable.IsTypeConsumable += Hooks.AbstractPhysicalObjectHooks.AbstractConsumable_IsTypeConsumable;
        #endregion

        #region AbstractCreature Hooks
        On.AbstractCreature.Realize += Hooks.AbstractCreatureHooks.AbstractCreature_Realize;
        On.AbstractCreature.InitiateAI += Hooks.AbstractCreatureHooks.AbstractCreature_InitiateAI;
        On.AbstractCreature.ctor += Hooks.AbstractCreatureHooks.AbstractCreature_ctor;
        On.AbstractCreature.InDenUpdate += Hooks.AbstractCreatureHooks.AbstractCreature_InDenUpdate;
        On.AbstractCreature.Update += Hooks.AbstractCreatureHooks.AbstractCreature_Update;
        #endregion

        #region AbstractRoom Hooks
        On.AbstractRoom.ConnectivityCost += Hooks.AbstractRoomHooks.AbstractRoom_ConnectivityCost;
        #endregion

        #region AI Hooks
        On.ArtificialIntelligence.SetDestination += Hooks.AIHooks.ArtificialIntelligence_SetDestination;
        On.RoomPreprocessor.DecompressStringToAImaps += Hooks.AIHooks.RoomPreprocessor_DecompressStringToAImaps;
        On.AImap.TileCostForCreature_WorldCoordinate_CreatureTemplate += Hooks.AIHooks.AImap_TileCostForCreature;
        #endregion

        #region Creature
        On.Creature.Update += Hooks.CreatureHooks.Creature_Update;
        On.TailSegment.ctor += Hooks.CreatureHooks.TailSegment_ctor;
        On.TailSegment.Update += Hooks.CreatureHooks.TailSegment_Update;
        #endregion

        #region Devtools Hooks
        On.DevInterface.ObjectsPage.CreateObjRep += Hooks.DevtoolsHooks.ObjectsPage_CreateObjRep;
        On.DevInterface.Panel.CopyToClipboard += Hooks.DevtoolsHooks.Panel_CopyToClipboard;
        On.DevInterface.Panel.PasteFromClipboard += Hooks.DevtoolsHooks.Panel_PasteFromClipboard;
        On.PlacedObject.GenerateEmptyData += Hooks.DevtoolsHooks.PlacedObject_GenerateEmptyData;
        On.DevInterface.MapPage.CreatureVis.CritString += Hooks.DevtoolsHooks.DevInterface_MapPage_CreatureVis_CritString;
        On.DevInterface.MapPage.CreatureVis.CritCol += Hooks.DevtoolsHooks.DevInterface_MapPage_CreatureVis_CritCol;
        #endregion

        #region FLabel Hooks
        On.FLabel.Redraw += Hooks.FLabelHooks.Redraw;
        #endregion

        #region HUDHooks
        On.HUD.HUD.InitSinglePlayerHud += Hooks.HUDHooks.HUD_InitSinglePlayerHud;
        On.HUD.HUD.InitMultiplayerHud += Hooks.HUDHooks.HUD_InitMultiplayerHud;
        On.HUD.HUD.Update += Hooks.HUDHooks.HUD_Update;
        #endregion

        #region Insect Hooks
        On.MiniFly.Update += Hooks.InsectHooks.MiniFly_Update;
        On.RedSwarmer.Update += Hooks.InsectHooks.RedSwarmer_Update;
        #endregion

        #region Iterator Hooks
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += Hooks.IteratorHooks.On_MoonConversation_AddEvents;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += Hooks.IteratorHooks.On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        On.SLOracleBehavior.Update += Hooks.IteratorHooks.On_SLOracleBehavior_Update;
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

        #region OverWorld Hooks
        On.OverWorld.ctor += Hooks.OverWorldHooks.OverWorld_ctor;
        #endregion

        #region PathFinderHooks
        On.PathFinder.ctor += Hooks.PathFinderHooks.PathFinder_ctor;
        On.PathFinder.CoordinateCost += Hooks.PathFinderHooks.PathFinder_CoordinateCost;
        On.PathFinder.CheckConnectionCost += Hooks.PathFinderHooks.PathFinder_CheckConnectionCost;
        On.PathFinder.CreatePathForAbstractreature += Hooks.PathFinderHooks.PathFinder_CreatePathForAbstractCreature;

        On.AbstractSpacePathFinder.Path += Hooks.PathFinderHooks.AbstractSpacePathFinder_Path;
        On.AbstractSpacePathFinder.AddNode += Hooks.PathFinderHooks.AbstractSpacePathFinder_AddNode;

        On.FollowPathVisualizer.Update += Hooks.PathFinderHooks.FollowPathVisualizer_Update;
        On.FollowPathVisualizer.DrawSprites += Hooks.PathFinderHooks.FollowPathVisualizer_DrawSprites;

        On.QuickConnectivity.Check += Hooks.PathFinderHooks.QuickConnectivity_Check;
        #endregion

        #region Player
        On.Player.GetHeldItemDirection += Hooks.PlayerHooks.Player_GetHeldItemDirection;
        On.Player.Grabability += Hooks.PlayerHooks.Player_Grabability;
        On.Player.IsObjectThrowable += Hooks.PlayerHooks.Player_IsObjectThrowable;
        On.Player.PickupCandidate += Hooks.PlayerHooks.Player_PickupCandidate;
        On.Player.SlugcatGrab += Hooks.PlayerHooks.Player_SlugcatGrab;
        On.Player.ThrowObject += Hooks.PlayerHooks.Player_ThrowObject;
        On.Player.MovementUpdate += Hooks.PlayerHooks.Player_MovementUpdate;
        On.Player.Update += Hooks.PlayerHooks.Player_Update;
        On.Player.IsCreatureLegalToHoldWithoutStun += Hooks.PlayerHooks.Player_IsCreatureLegalToHoldWithoutStun;
        On.PlayerGraphics.DrawSprites += Hooks.PlayerHooks.PlayerGraphics_DrawSprites;
        On.SlugcatHand.Update += Hooks.PlayerHooks.SlugcatHand_Update;
        #endregion

        #region Process Hooks
        On.ProcessManager.Update += Hooks.ProcessHooks.ProcessManager_Update;
        #endregion

        #region Room Hooks
        On.Room.Loaded += Hooks.RoomHooks.Room_Loaded;
        new Hook(typeof(Room).GetMethod("get_ElectricPower"), Hooks.RoomHooks.Room_ElectricPower);
        #endregion

        #region Scavenger Hooks
        On.Scavenger.Act += Hooks.ScavengerHooks.Scavenger_Act;
        On.Scavenger.TryThrow_BodyChunk_ViolenceType += Hooks.ScavengerHooks.Scavenger_TryThrow;
        On.Scavenger.TryToMeleeCreature += Hooks.ScavengerHooks.Scavenger_TryToMeleeCreature;
        On.Scavenger.ArrangeInventory += Hooks.ScavengerHooks.Scavenger_ArrangeInventory;
        On.ScavengerGraphics.ContainerForHeldItem += Hooks.ScavengerHooks.ScavengerGraphics_ContainerForHeldItem;
        new Hook(typeof(ScavengerGraphics).GetMethod("ItemPosition"), Hooks.ScavengerHooks.ScavengerGraphics_ItemPosition);
        new Hook(typeof(ScavengerGraphics).GetMethod("ItemDirection"), Hooks.ScavengerHooks.ScavengerGraphics_ItemDirection);

        Type[] types =
        [
            typeof(RoomCamera.SpriteLeaser),
            typeof(RoomCamera),
            typeof(float),
            typeof(float2),
        ];
        new Hook(typeof(ScavengerGraphics.ScavengerHand).GetMethod("DrawSprites", types), Hooks.ScavengerHooks.ScavengerHand_DrawSprites);

        On.ScavengerAI.CollectScore_PhysicalObject_bool += Hooks.ScavengerHooks.ScavengerAI_CollectScore;
        On.ScavengerAI.WeaponScore += Hooks.ScavengerHooks.ScavengerAI_WeaponScore;
        On.ScavengerAI.CheckForScavangeItems += Hooks.ScavengerHooks.ScavengerAI_CheckForScavengeItems;
        On.ScavengerAI.PickUpItemScore += Hooks.ScavengerHooks.ScavengerAI_PickUpItemScore;
        On.ScavengerAI.TravelPreference += Hooks.ScavengerHooks.ScavengerAI_TravelPreference;
        On.ScavengerAI.AttackBehavior += Hooks.ScavengerHooks.ScavengerAI_AttackBehavior;
        On.ScavengerAbstractAI.InitGearUp += Hooks.ScavengerHooks.ScavengerAbstractAI_InitGearUp;
        On.ScavengerAbstractAI.ReGearInDen += Hooks.ScavengerHooks.ScavengerAbstractAI_ReGearInDen;
        On.ScavengerAbstractAI.UpdateMissionAppropriateGear += Hooks.ScavengerHooks.ScavengerabstractAI_UpdateMissionAppropriateGear;
        On.ScavengerAbstractAI.TradeItem += Hooks.ScavengerHooks.ScavengerAbstractAI_TradeItem;
        On.ScavengerTreasury.ctor += Hooks.ScavengerHooks.ScavengerTreasury_ctor;
        #endregion

        #region Slugpup Hooks
        On.MoreSlugcats.SlugNPCAI.GetFoodType += Hooks.SlugpupHooks.SlugNPCAI_GetFoodType;
        #endregion

        #region Spear Hooks
        On.Spear.DrawSprites += Hooks.SpearHooks.Spear_DrawSprites;
        On.Spear.Update += Hooks.SpearHooks.Spear_Update;
        On.MoreSlugcats.ElectricSpear.ZapperAttachPos += Hooks.SpearHooks.ElectricSpear_ZapperAttachPos;
        #endregion

        #region Symbol Hooks
        On.ItemSymbol.SpriteNameForItem += Hooks.SymbolHooks.ItemSymbol_SpriteNameForItem;
        On.ItemSymbol.ColorForItem += Hooks.SymbolHooks.ItemSymbol_ColorForItem;
        On.CreatureSymbol.SpriteNameOfCreature += Hooks.SymbolHooks.CreatureSymbol_SpriteNameOfCreature;
        On.CreatureSymbol.ColorOfCreature += Hooks.SymbolHooks.CreatureSymbol_ColorOfCreature;
        #endregion

        #region Weapon Hooks
        On.Weapon.Thrown += Hooks.WeaponHooks.Weapon_Thrown;
        On.Weapon.HitWall += Hooks.WeaponHooks.Weapon_HitWall;
        On.Weapon.Update += Hooks.WeaponHooks.Weapon_Update;
        #endregion

        #region World Hooks
        On.WorldLoader.CreatureTypeFromString += Hooks.WorldHooks.WorldLoader_CreatureTypeFromString;
        On.StaticWorld.InitCustomTemplates += Hooks.StaticWorldHooks.InitCustomTemplates;
        On.StaticWorld.InitStaticWorldRelationships += Hooks.StaticWorldHooks.InitStaticWorldRelationships;
        On.StaticWorld.InitStaticWorldRelationshipsMSC += Hooks.StaticWorldHooks.InitStaticWorldRelationshipsMSC;
        On.StaticWorld.InitStaticWorldRelationshipsWatcher += Hooks.StaticWorldHooks.InitStaticWorldRelationshipsWatcher;
        #endregion

        //On.Player.Update += Hooks.PlayerHooks.Player_Update;
    }

    private PathCost PathFinder_CheckConnectionCost(On.PathFinder.orig_CheckConnectionCost orig, PathFinder self, PathFinder.PathingCell start, PathFinder.PathingCell goal, MovementConnection connection, bool followingPath)
    {
        throw new NotImplementedException();
    }
}