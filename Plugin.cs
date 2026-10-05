global using ArchdruidsAdditions.Data;
global using ArchdruidsAdditions.Objects;
global using RWCustom;
global using UnityEngine;
global using static ArchdruidsAdditions.Methods.Methods;
global using Color = UnityEngine.Color;
global using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using EffExt;
using Menu;
using MonoMod.RuntimeDetour;
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

    public static Configuration.PluginOptions Options;
    public static RegionData RegionData;

    public static List<string> logStrings = [];
    public static List<Exception> logExceptions = [];
    public static bool loggedStrings = false;

    public Plugin()
    {
        try { Options = new Configuration.PluginOptions(); }
        catch (Exception ex) { Debug.LogException(ex); }
    }

    public void OnEnable()
    {
        EffectDefinitionBuilder builder1 = new("ForceRoomEnergy");
        builder1.SetEffectInitializer(LightRodPowerEffect.EffectSpawner);
        builder1.AddFloatField("DriftStrength", 0f, 0.1f, 0f, 0f, "DriftStrength");
        builder1.AddIntField("DriftMode", 0, 2, 0, "DriftMode");
        builder1.AddFloatField("ResetChance", 0f, 100f, 0f, 0f, "ResetChance");
        builder1.AddFloatField("ResetCooldown", 0f, 2000f, 0f, 0f, "ResetCD");
        builder1.SetCategory("AAEffects");
        builder1.Register();

        EffectDefinitionBuilder builder2 = new("RandomShells");
        builder2.SetEffectInitializer(RandomShells.EffectSpawner);
        builder2.SetCategory("AAEffects");
        builder2.Register();

        //

        #region Main Hooks
        On.RainWorld.OnModsInit += Hooks.MainHooks.RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += Hooks.MainHooks.RainWorld_UnloadResources;
        On.RainWorld.OnModsEnabled += Hooks.MainHooks.RainWorld_OnModsEnabled;
        On.RainWorld.OnModsDisabled += Hooks.MainHooks.RainWorld_OnModsDisabled;
        On.RainWorld.PostModsInit += Hooks.MainHooks.RainWorld_PostModsInIt;
        #endregion

        int section = 0;
        try
        {

            //

            section = 1;
            #region Abstract Hooks

            #region AbstractPhysicalObject Hooks
            On.AbstractPhysicalObject.Update += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Update;
            On.AbstractPhysicalObject.Realize += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Realize;
            On.AbstractPhysicalObject.Abstractize += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Abstractize;
            On.AbstractPhysicalObject.AddConnected += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_AddConnected;
            On.AbstractPhysicalObject.LoseAllStuckObjects += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_LoseAllStuckObjects;
            On.AbstractPhysicalObject.Destroy += Hooks.AbstractPhysicalObjectHooks.AbstractPhysicalObject_Destroy;
            On.AbstractPhysicalObject.AbstractObjectStick.FromString += Hooks.AbstractPhysicalObjectHooks.AbstractObjectStick_FromString;
            On.AbstractPhysicalObject.AbstractObjectStick.Deactivate += Hooks.AbstractPhysicalObjectHooks.AbstractObjectStick_Deactivate;
            On.AbstractConsumable.IsTypeConsumable += Hooks.AbstractPhysicalObjectHooks.AbstractConsumable_IsTypeConsumable;
            #endregion

            #region AbstractCreature Hooks
            On.AbstractCreature.Realize += Hooks.AbstractCreatureHooks.AbstractCreature_Realize;
            On.AbstractCreature.InitiateAI += Hooks.AbstractCreatureHooks.AbstractCreature_InitiateAI;
            On.AbstractCreature.ctor += Hooks.AbstractCreatureHooks.AbstractCreature_ctor;
            On.AbstractCreature.InDenUpdate += Hooks.AbstractCreatureHooks.AbstractCreature_InDenUpdate;
            On.AbstractCreature.Update += Hooks.AbstractCreatureHooks.AbstractCreature_Update;
            On.AbstractCreature.ChangeRooms += Hooks.AbstractCreatureHooks.AbstractCreature_ChangeRooms;
            On.AbstractCreature.DropCarriedObject += Hooks.AbstractCreatureHooks.AbstractCreature_DropCarriedObject;
            On.AbstractCreature.setCustomFlags += Hooks.AbstractCreatureHooks.AbstractCreature_SetCustomFlags;
            #endregion

            #region AbstractRoom Hooks
            On.AbstractRoom.ConnectivityCost += Hooks.AbstractRoomHooks.AbstractRoom_ConnectivityCost;
            On.AbstractRoomNode.ConnectionCost += Hooks.AbstractRoomHooks.AbstractRoomNode_ConnectionCost;
            #endregion

            #endregion

            //

            section = 2;
            #region Creature Hooks

            #region AI Hooks
            On.ArtificialIntelligence.SetDestination += Hooks.AIHooks.ArtificialIntelligence_SetDestination;
            On.RoomPreprocessor.DecompressStringToAImaps += Hooks.AIHooks.RoomPreprocessor_DecompressStringToAImaps;
            On.AImap.TileCostForCreature_WorldCoordinate_CreatureTemplate += Hooks.AIHooks.AImap_TileCostForCreature;
            On.AImapper.FindAccessibilityOfCurrentTile += Hooks.AIHooks.Aimapper_FindAccessibilityOfCurrentTile;
            On.RelationshipTracker.DynamicRelationship.Update += Hooks.AIHooks.DynamicRelationship_Update;
            On.ArtificialIntelligence.Update += Hooks.AIHooks.ArtificialIntelligence_Update;
            On.VultureAI.Update += Hooks.AIHooks.VultureAI_Update;
            On.VultureAI.OnlyHurtDontGrab += Hooks.AIHooks.VultureAI_OnlyHurtDontGrab;
            On.VultureAI.DoIWantToBiteCreature += Hooks.AIHooks.VultureAI_DoIWantToBiteCreature;
            On.MirosBirdAI.DoIWantToBiteCreature += Hooks.AIHooks.MirosBirdAI_DoIWantToBiteCreature;
            #endregion

            #region Barnacle Hooks
            On.Watcher.Barnacle.Collide += Hooks.BarnacleHooks.Barnacle_Collide;
            On.Watcher.BarnacleAI.SetGroupDiscomfortTick += Hooks.BarnacleHooks.BarnacleAI_SetGroupDiscomfortTick;
            #endregion

            #region Creature Hooks
            On.TailSegment.ctor += Hooks.CreatureHooks.TailSegment_ctor;
            On.BodyPart.OnOtherSideOfTerrain += Hooks.CreatureHooks.BodyPart_OnOtherSideOfTerrain;
            On.BodyPart.PushOutOfTerrain += Hooks.CreatureHooks.BodyPart_PushOutOfTerrain;
            On.Limb.FindGrip += Hooks.CreatureHooks.Limb_FindGrip;
            On.Creature.ctor += Hooks.CreatureHooks.Creature_ctor;
            On.Creature.Update += Hooks.CreatureHooks.Creature_Update;
            On.Creature.Grab += Hooks.CreatureHooks.Creature_Grab;
            On.Creature.ReleaseGrasp += Hooks.CreatureHooks.Creature_ReleaseGrab;
            On.OverseerTutorialBehavior.Update += Hooks.CreatureHooks.OverseerTutorialBehavior_Update;
            On.DaddyTentacle.StickToTerrain += Hooks.CreatureHooks.DaddyTentacle_StickToTerrain;

            try
            {
                IL.VultureGraphics.ctor += Hooks.CreatureHooks.IL_VultureGraphics_ctor;
            }
            catch (Exception e)
            {
                logStrings.Add("<ArchdruidsAdditions> FAILED TO ATTACH VULTUREGRAPHICS_CTOR ILHOOK!");
                logExceptions.Add(e);
            }
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

            #region Lizard Hooks
            On.Lizard.Act += Hooks.LizardHooks.Lizard_Act;
            On.LizardAI.Update += Hooks.LizardHooks.LizardAI_Update;
            On.LizardLimb.Update += Hooks.LizardHooks.LizardLimb_Update;

            try
            {
                IL.Lizard.FollowConnection += Hooks.LizardHooks.IL_Lizard_FollowConnection;
            }
            catch (Exception e)
            {
                logStrings.Add("<ArchdruidsAdditions> FAILED TO ATTACH LIZARD_FOLLOWCONNECTION ILHOOK!");
                logExceptions.Add(e);
            }
            #endregion

            #region PathFinderHooks
            On.PathFinder.ctor += Hooks.PathFinderHooks.PathFinder_ctor;
            On.PathFinder.Update += Hooks.PathFinderHooks.PathFinder_Update;
            On.PathFinder.CoordinateCost += Hooks.PathFinderHooks.PathFinder_CoordinateCost;
            On.PathFinder.CheckConnectionCost += Hooks.PathFinderHooks.PathFinder_CheckConnectionCost;
            On.PathFinder.CreatePathForAbstractreature += Hooks.PathFinderHooks.PathFinder_CreatePathForAbstractCreature;

            On.AbstractSpacePathFinder.Path += Hooks.PathFinderHooks.AbstractSpacePathFinder_Path;
            On.AbstractSpacePathFinder.AddNode += Hooks.PathFinderHooks.AbstractSpacePathFinder_AddNode;

            On.FollowPathVisualizer.Update += Hooks.PathFinderHooks.FollowPathVisualizer_Update;
            On.FollowPathVisualizer.DrawSprites += Hooks.PathFinderHooks.FollowPathVisualizer_DrawSprites;

            On.QuickConnectivity.Check += Hooks.PathFinderHooks.QuickConnectivity_Check;
            #endregion

            #region Player Hooks
            On.Player.Update += Hooks.PlayerHooks.Player_Update;
            On.Player.checkInput += Hooks.PlayerHooks.Player_checkInput;
            On.Player.Grabability += Hooks.PlayerHooks.Player_Grabability;
            On.Player.PickupCandidate += Hooks.PlayerHooks.Player_PickupCandidate;
            On.Player.IsCreatureLegalToHoldWithoutStun += Hooks.PlayerHooks.Player_IsCreatureLegalToHoldWithoutStun;
            On.Player.SlugcatGrab += Hooks.PlayerHooks.Player_SlugcatGrab;
            On.Player.CanBeSwallowed += Hooks.PlayerHooks.Player_CanBeSwallowed;
            On.Player.GetHeldItemDirection += Hooks.PlayerHooks.Player_GetHeldItemDirection;
            On.Player.IsObjectThrowable += Hooks.PlayerHooks.Player_IsObjectThrowable;
            On.Player.ThrowObject += Hooks.PlayerHooks.Player_ThrowObject;
            On.Player.ObjectEaten += Hooks.PlayerHooks.Player_ObjectEaten;
            On.Player.AddFood += Hooks.PlayerHooks.Player_AddFood;
            On.Player.AddQuarterFood += Hooks.PlayerHooks.Player_AddQuarterFood;
            On.PlayerGraphics.Update += Hooks.PlayerHooks.PlayerGraphics_Update;
            On.PlayerGraphics.DrawSprites += Hooks.PlayerHooks.PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ctor += Hooks.PlayerHooks.PlayerGraphics_ctor;
            On.PlayerGraphics.PlayerObjectLooker.HowInterestingIsThisObject += Hooks.PlayerHooks.PlayerObjectLooker_HowInterestingIsThisObject;
            On.PlayerState.ctor += Hooks.PlayerHooks.PlayerState_ctor;
            On.SlugcatHand.Update += Hooks.PlayerHooks.SlugcatHand_Update;
            On.SlugcatStats.NourishmentOfObjectEaten += Hooks.PlayerHooks.SlugcatStats_NourishmentOfObjectEaten;
            #endregion

            #region Scavenger Hooks
            On.Scavenger.ctor += Hooks.ScavengerHooks.Scavenger_ctor;
            On.Scavenger.Update += Hooks.ScavengerHooks.Scavenger_Update;
            On.Scavenger.Act += Hooks.ScavengerHooks.Scavenger_Act;
            On.Scavenger.TryThrow_BodyChunk_ViolenceType += Hooks.ScavengerHooks.Scavenger_TryThrow;
            On.Scavenger.TryToMeleeCreature += Hooks.ScavengerHooks.Scavenger_TryToMeleeCreature;
            On.Scavenger.ArrangeInventory += Hooks.ScavengerHooks.Scavenger_ArrangeInventory;
            On.Scavenger.WantToLethallyAttack += Hooks.ScavengerHooks.Scavenger_WantToLethallyAttack;
            new Hook(typeof(Scavenger).GetMethod("get_HeadLookPoint"), Hooks.ScavengerHooks.Scavenger_Get_HeadLookPoint);
            new Hook(typeof(Scavenger).GetMethod("get_EyesLookPoint"), Hooks.ScavengerHooks.Scavenger_Get_EyesLookPoint);

            On.ScavengerGraphics.ContainerForHeldItem += Hooks.ScavengerHooks.ScavengerGraphics_ContainerForHeldItem;
            new Hook(typeof(ScavengerGraphics).GetMethod("ItemPosition"), Hooks.ScavengerHooks.ScavengerGraphics_Get_ItemPosition);
            new Hook(typeof(ScavengerGraphics).GetMethod("ItemDirection"), Hooks.ScavengerHooks.ScavengerGraphics_Get_ItemDirection);

            Type[] types =
            [
                typeof(RoomCamera.SpriteLeaser),
        typeof(RoomCamera),
        typeof(float),
        typeof(float2),
    ];
            new Hook(typeof(ScavengerGraphics.ScavengerHand).GetMethod("DrawSprites", types), Hooks.ScavengerHooks.ScavengerHand_DrawSprites);

            On.ScavengerAI.Update += Hooks.ScavengerHooks.ScavengerAI_Update;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += Hooks.ScavengerHooks.ScavengerAI_CollectScore;
            On.ScavengerAI.WeaponScore += Hooks.ScavengerHooks.ScavengerAI_WeaponScore;
            On.ScavengerAI.CheckForScavangeItems += Hooks.ScavengerHooks.ScavengerAI_CheckForScavengeItems;
            On.ScavengerAI.PickUpItemScore += Hooks.ScavengerHooks.ScavengerAI_PickUpItemScore;
            On.ScavengerAI.TravelPreference += Hooks.ScavengerHooks.ScavengerAI_TravelPreference;
            On.ScavengerAI.AttackBehavior += Hooks.ScavengerHooks.ScavengerAI_AttackBehavior;
            On.ScavengerAI.DecideBehavior += Hooks.ScavengerHooks.ScavengerAI_DecideBehavior;
            On.ScavengerAI.CurrentPlayerAggression += Hooks.ScavengerHooks.ScavengerAI_CurrentPlayerAggression;
            new Hook(typeof(ScavengerAI).GetMethod("get_HoldWeapon"), Hooks.ScavengerHooks.ScavengerAI_Get_HoldAWeapon);
            new Hook(typeof(ScavengerAI).GetMethod("get_NeedAWeapon"), Hooks.ScavengerHooks.ScavengerAI_Get_NeedAWeapon);
            On.ScavengerAI.MakeLookHere += Hooks.ScavengerHooks.ScavengerAI_MakeLookHere;
            On.ScavengerAI.SeeThrownWeapon += Hooks.ScavengerHooks.ScavengerAI_SeeThrownWeapon;
            On.ScavengerAI.SpearThrowPositionScore += Hooks.ScavengerHooks.ScavengerAI_SpearThrowPositionScore;
            On.ScavengerAbstractAI.InitGearUp += Hooks.ScavengerHooks.ScavengerAbstractAI_InitGearUp;
            On.ScavengerAbstractAI.ReGearInDen += Hooks.ScavengerHooks.ScavengerAbstractAI_ReGearInDen;
            On.ScavengerAbstractAI.UpdateMissionAppropriateGear += Hooks.ScavengerHooks.ScavengerabstractAI_UpdateMissionAppropriateGear;
            On.ScavengerAbstractAI.TradeItem += Hooks.ScavengerHooks.ScavengerAbstractAI_TradeItem;
            On.ScavengerTreasury.ctor += Hooks.ScavengerHooks.ScavengerTreasury_ctor;
            #endregion

            #region Slugpup Hooks
            On.MoreSlugcats.SlugNPCAI.GetFoodType += Hooks.SlugpupHooks.SlugNPCAI_GetFoodType;
            On.MoreSlugcats.SlugNPCAI.WantsToEatThis += Hooks.SlugpupHooks.SlugNPCAI_WantsToEatThis;
            #endregion

            #region StaticWorld Hooks
            On.StaticWorld.InitCustomTemplates += Hooks.StaticWorldHooks.InitCustomTemplates;
            On.StaticWorld.InitStaticWorldRelationships += Hooks.StaticWorldHooks.InitStaticWorldRelationships;
            On.StaticWorld.InitStaticWorldRelationshipsMSC += Hooks.StaticWorldHooks.InitStaticWorldRelationshipsMSC;
            On.StaticWorld.InitStaticWorldRelationshipsWatcher += Hooks.StaticWorldHooks.InitStaticWorldRelationshipsWatcher;
            #endregion

            #endregion

            //

            section = 3;
            #region Meta Hooks

            #region Devtools Hooks
            On.DevInterface.ObjectsPage.CreateObjRep += Hooks.DevtoolsHooks.ObjectsPage_CreateObjRep;
            On.DevInterface.Panel.CopyToClipboard += Hooks.DevtoolsHooks.Panel_CopyToClipboard;
            On.DevInterface.Panel.PasteFromClipboard += Hooks.DevtoolsHooks.Panel_PasteFromClipboard;
            On.PlacedObject.GenerateEmptyData += Hooks.DevtoolsHooks.PlacedObject_GenerateEmptyData;
            On.DevInterface.MapPage.CreatureVis.CritString += Hooks.DevtoolsHooks.MapPage_CreatureVis_CritString;
            On.DevInterface.MapPage.CreatureVis.CritCol += Hooks.DevtoolsHooks.MapPage_CreatureVis_CritCol;
            On.DevInterface.Handle.Update += Hooks.DevtoolsHooks.Handle_Update;
            #endregion

            #region Futile Hooks
            On.FFacetRenderLayer.UpdateMeshProperties += Hooks.FutileHooks.FFacetRenderLayer_UpdateMeshProperties;
            #endregion

            #region Game Hooks
            On.RainWorldGame.Update += Hooks.GameHooks.RainWorldGame_Update;
            On.RainWorldGame.CommunicateWithUpcomingProcess += Hooks.GameHooks.RainWorldGame_CommunicateWithUpcomingProcess;
            On.RainWorldGame.Win += Hooks.GameHooks.RainWorldGame_Win;
            On.RainWorldGame.SpawnPlayers_bool_bool_bool_bool_WorldCoordinate += Hooks.GameHooks.RainWorldGame_SpawnPlayers;
            On.RainWorldGame.RawUpdate += Hooks.GameHooks.RainWorldGame_RawUpdate;

            On.PlayerProgression.GetOrInitiateSaveState += Hooks.GameHooks.PlayerProgression_GetOrInitiateSaveState;
            On.PlayerProgression.SaveWorldStateAndProgression += Hooks.GameHooks.PlayerProgression_SaveWorldStateAndProgression;
            On.PlayerProgression.ClearOutSaveStateFromMemory += Hooks.GameHooks.PlayerProgression_ClearOutSaveStateFromMemory;
            On.PlayerProgression.DeleteSaveFile += Hooks.GameHooks.PlayerProgression_DeleteSaveFile;
            On.PlayerProgression.WipeSaveState += Hooks.GameHooks.PlayerProgression_WipeSaveState;
            On.PlayerProgression.Revert += Hooks.GameHooks.PlayerProgression_Revert;

            On.MoreSlugcats.SpeedRunTimer.GetTimerTickIncrement += Hooks.GameHooks.SpeedRunTimer_GetTimerTickIncrement;
            #endregion

            #region HUD Hooks
            On.HUD.HUD.InitSinglePlayerHud += Hooks.HUDHooks.HUD_InitSinglePlayerHud;
            On.HUD.HUD.InitMultiplayerHud += Hooks.HUDHooks.HUD_InitMultiplayerHud;
            On.HUD.HUD.InitSleepHud += Hooks.HUDHooks.HUD_InitSleepHud;
            On.HUD.HUD.Update += Hooks.HUDHooks.HUD_Update;
            On.HUD.FoodMeter.ctor += Hooks.HUDHooks.FoodMeter_ctor;
            On.HUD.FoodMeter.Update += Hooks.HUDHooks.FoodMeter_Update;
            On.HUD.FoodMeter.SleepUpdate += Hooks.HUDHooks.FoodMeter_SleepUpdate;
            On.HUD.FoodMeter.MoveSurvivalLimit += Hooks.HUDHooks.FoodMeter_MoveSurvivalLimit;
            On.HUD.FoodMeter.Draw += Hooks.HUDHooks.FoodMeter_Draw;
            On.HUD.FoodMeter.MeterCircle.Draw += Hooks.HUDHooks.FoodMeter_MeterCircle_Draw;
            On.ItemSymbol.SpriteNameForItem += Hooks.SymbolHooks.ItemSymbol_SpriteNameForItem;
            On.ItemSymbol.ColorForItem += Hooks.SymbolHooks.ItemSymbol_ColorForItem;
            On.CreatureSymbol.SpriteNameOfCreature += Hooks.SymbolHooks.CreatureSymbol_SpriteNameOfCreature;
            On.CreatureSymbol.ColorOfCreature += Hooks.SymbolHooks.CreatureSymbol_ColorOfCreature;
            #endregion

            #region Menu Hooks
            On.Menu.MouseCursor.GrafUpdate += Hooks.MenuHooks.MouseCursor_GrafUpdate;
            On.MainLoopProcess.GrafUpdate += Hooks.MenuHooks.MainLoopProcess_GrafUpdate;

            On.Menu.MenuScene.BuildScene += Hooks.MenuHooks.MenuScene_BuildScene;
            On.Menu.MenuScene.Update += Hooks.MenuHooks.MenuScene_Update;

            On.Menu.SleepAndDeathScreen.GetDataFromGame += Hooks.MenuHooks.SleepAndDeathScreen_GetDataFromGame;
            On.Menu.SleepAndDeathScreen.Update += Hooks.MenuHooks.SleepAndDeathScreen_Update;
            On.Menu.SleepAndDeathScreen.GrafUpdate += Hooks.MenuHooks.SleepAndDeathScreen_GrafUpdate;
            On.Menu.SleepAndDeathScreen.AddSubObjects += Hooks.MenuHooks.SleepAndDeathScreen_AddSubOjects;
            new Hook(typeof(SleepAndDeathScreen).GetMethod("get_AllowFoodMeterTick"), Hooks.MenuHooks.SleepAndDeathScreen_Get_AllowFoodMeterTick);
            On.Menu.SleepAndDeathScreen.FoodCountDownDone += Hooks.MenuHooks.SleepAndDeathScreen_FoodCountDownDone;
            #endregion

            #region SaveState Hooks
            On.SaveState.LoadGame += Hooks.SaveStateHooks.SaveState_LoadGame;
            On.SaveState.SaveToString += Hooks.SaveStateHooks.SaveState_SaveToString;
            On.SaveState.SessionEnded += Hooks.SaveStateHooks.SaveState_SessionEnded;
            new Hook(typeof(SaveState).GetMethod("get_SlowFadeIn"), Hooks.SaveStateHooks.SaveState_Get_SlowFadeIn);
            #endregion

            #region Symbol Hooks
            On.ItemSymbol.SpriteNameForItem += Hooks.SymbolHooks.ItemSymbol_SpriteNameForItem;
            On.ItemSymbol.ColorForItem += Hooks.SymbolHooks.ItemSymbol_ColorForItem;
            On.CreatureSymbol.SpriteNameOfCreature += Hooks.SymbolHooks.CreatureSymbol_SpriteNameOfCreature;
            On.CreatureSymbol.ColorOfCreature += Hooks.SymbolHooks.CreatureSymbol_ColorOfCreature;
            #endregion

            #region Process Hooks
            On.ProcessManager.Update += Hooks.ProcessHooks.ProcessManager_Update;
            On.ProcessManager.PostSwitchMainProcess += Hooks.ProcessHooks.ProcessManager_PostSwitchMainProcess;
            On.MainLoopProcess.RawUpdate += Hooks.ProcessHooks.MainLoopProcess_RawUpdate;
            #endregion

            #endregion

            //

            section = 4;
            #region Object Hooks

            #region Light Hooks
            On.Redlight.Update += Hooks.LightHooks.Redlight_Update;
            On.LightSource.Update += Hooks.LightHooks.LightSource_Update;
            On.LightSource.DrawSprites += Hooks.LightHooks.LightSource_DrawSprites;
            #endregion

            #region Lightning Hooks
            On.Lightning.ctor += Hooks.LightningHooks.Lightning_ctor;
            #endregion

            #region PhysicalObject Hooks
            On.PhysicalObject.Update += Hooks.PhysicalObjectHooks.PhysicalObject_Update;
            On.PhysicalObject.IsTileSolid += Hooks.PhysicalObjectHooks.PhysicalObject_IsTileSolid;
            On.Mushroom.DrawSprites += Hooks.PhysicalObjectHooks.Mushroom_DrawSprites;
            On.BodyChunk.Update += Hooks.PhysicalObjectHooks.BodyChunk_Update;
            #endregion

            #region Misc Hooks
            On.ShelterDoor.DoorClosed += Hooks.ShelterHooks.ShelterDoor_DoorClosed;
            #endregion

            #region SharedPhysics Hooks
            On.SharedPhysics.VerticalCollision += Hooks.SharedPhysicsHooks.SharedPhysics_TerrainCollisionData_VerticalCollision;
            #endregion

            #region Weapon Hooks
            On.Spear.DrawSprites += Hooks.WeaponHooks.Spear_DrawSprites;
            On.Spear.Update += Hooks.WeaponHooks.Spear_Update;
            On.Spear.HitSomething += Hooks.WeaponHooks.Spear_HitSomething;
            On.MoreSlugcats.ElectricSpear.ZapperAttachPos += Hooks.WeaponHooks.ElectricSpear_ZapperAttachPos;
            On.Weapon.Thrown += Hooks.WeaponHooks.Weapon_Thrown;
            On.Weapon.Update += Hooks.WeaponHooks.Weapon_Update;
            #endregion

            #region Wind Hooks
            On.WindRect.Update += Hooks.WindHooks.WindRect_Update;
            On.Watcher.Sandstorm.AffectObjects += Hooks.WindHooks.Sandstorm_AffectObjects;
            #endregion

            #endregion

            //

            section = 5;
            #region World Hooks

            #region Room Hooks
            On.Room.Loaded += Hooks.RoomHooks.Room_Loaded;
            On.Room.Update += Hooks.RoomHooks.Room_Update;
            On.Room.HasAnySolid_int_int += Hooks.RoomHooks.Room_HasAnySolid;
            new Hook(typeof(Room).GetMethod("get_ElectricPower"), Hooks.RoomHooks.Room_Get_ElectricPower);
            On.RoomSettings.LoadPlacedObjects_StringArray_Timeline += Hooks.RoomHooks.RoomSettings_LoadPlacedObjects;
            On.RoomCamera.ChangeRoom += Hooks.RoomHooks.RoomCamera_ChangeRoom;
            #endregion

            #region RoomSpecificScript Hooks
            On.RoomSpecificScript.SU_C04StartUp.Update += Hooks.RoomScriptHooks.RoomSpecificScript_SU_CO4StartUp_Update;
            On.RoomSpecificScript.SU_A43SuperJumpOnly.Update += Hooks.RoomScriptHooks.RoomSpecificScript_SU_A43SuperJumpOnly_Update;
            #endregion

            #region Shortcut Hooks
            On.ShortcutHandler.Update += Hooks.ShortcutHooks.ShortcutHandler_Update;
            #endregion

            #region Misc Hooks
            On.OverWorld.ctor += Hooks.OverWorldHooks.OverWorld_ctor;
            On.OverWorld.Update += Hooks.OverWorldHooks.OverWorld_Update;
            On.WorldLoader.CreatureTypeFromString += Hooks.WorldHooks.WorldLoader_CreatureTypeFromString;
            #endregion

            #endregion

            //

        }
        catch (Exception e)
        {
            logStrings.Add("<ArchdruidsAdditions> FAILED TO ATTACH HOOKS WHILE INITALIZING PLUGIN! SECTION: " + section);
            logExceptions.Add(e);
        }
    }
}