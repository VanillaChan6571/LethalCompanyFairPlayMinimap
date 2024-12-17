// ----------------------------------------------------------------------
// Copyright (c) Tyzeron. All Rights Reserved.
// Licensed under the GNU Affero General Public License, Version 3
// ----------------------------------------------------------------------

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalCompanyMinimap.Component;
using LethalCompanyMinimap.Patches;
using UnityEngine;

namespace LethalCompanyMinimap
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MinimapMod: BaseUnityPlugin
    {
        public const string modGUID = "Minimap-Fair-Play";
        public const string modName = "Minimap (FairPlay Edition)";
        public const string modVersion = "1.2.1";
        public const string modAuthor = "[v69] VanillaChanny + Tyzeron";
        public const string modRepository = "VanillaChan6571/LethalCompanyFairPlayMinimap";

        public static MouseAndKeyboard defaultGuiKey = MouseAndKeyboard.F1;
        public static MouseAndKeyboard defaultToggleMinimapKey = MouseAndKeyboard.F2;
        public static MouseAndKeyboard defaultToggleOverrideKey = MouseAndKeyboard.F3;
        public static MouseAndKeyboard defaultSwitchTargetKey = MouseAndKeyboard.F4;
        public const int defaultMinimapSize = 150;
        public const float defaultXoffset = 0f;
        public const float defaultYoffset = 0f;
        public const float defaultMapZoom = 15.4f;
        public const float defaultBrightness = 5f;

        private static ConfigEntry<MouseAndKeyboard> guiKeyConfig;
        private static ConfigEntry<MouseAndKeyboard> toggleMinimapKeyConfig;
        private static ConfigEntry<MouseAndKeyboard> toggleOverrideKeyConfig;
        private static ConfigEntry<MouseAndKeyboard> switchTargetKeyConfig;
        private static ConfigEntry<bool> enableMinimapConfig;
        private static ConfigEntry<bool> autoRotateConfig;
        private static ConfigEntry<int> minimapSizeConfig;
        private static ConfigEntry<float> minimapXPosConfig;
        private static ConfigEntry<float> minimapYPosConfig;
        private static ConfigEntry<float> minimapZoomConfig;
        private static ConfigEntry<float> brightnessConfig;
        private static ConfigEntry<bool> showLootsConfig;
        private static ConfigEntry<bool> showEnemiesConfig;
        private static ConfigEntry<bool> showTurretConfig;
        private static ConfigEntry<bool> showLandminesConfig;
        private static ConfigEntry<bool> showLivePlayersConfig;
        private static ConfigEntry<bool> showDeadPlayersConfig;
        private static ConfigEntry<bool> showRadarBoostersConfig;
        private static ConfigEntry<bool> showTerminalCodesConfig;
        private static ConfigEntry<bool> showShipArrowConfig;
        private static ConfigEntry<bool> freezePlayerIndexConfig;

        public static MinimapMod Instance;
        public static ManualLogSource mls;
        private readonly Harmony harmony = new Harmony(modGUID);
        public static MinimapGUI minimapGUI;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modName} {modVersion} loaded!");

            // Start the Task of getting the latest version
            _ = VersionChecker.GetLatestVersionAsync();

            // Patching stuff
            harmony.PatchAll(typeof(ManualCameraRendererPatch));
            harmony.PatchAll(typeof(QuickMenuManagerPatch));
            harmony.PatchAll(typeof(ShipTeleporterPatch));
            harmony.PatchAll(typeof(GrabbableObjectPatch));
            harmony.PatchAll(typeof(TerminalAccessibleObjectPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RadarBoosterItemPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof(TimeOfDayPatch));
            harmony.PatchAll(typeof(DeadBodyInfoPatch));
            harmony.PatchAll(typeof(TurretPatch));
            harmony.PatchAll(typeof(LandminePatch));

            // Initialize Minimap Mod Menu GUI
            GameObject minimapGUIObject = new GameObject("MinimapGUI");
            DontDestroyOnLoad(minimapGUIObject);
            minimapGUIObject.hideFlags = HideFlags.HideAndDontSave;
            minimapGUIObject.AddComponent<MinimapGUI>();
            minimapGUI = (MinimapGUI)minimapGUIObject.GetComponent("MinimapGUI");

            // Sync Configuration file and Mod GUI
            SetBindings();
            SyncGUIFromConfigs();
        }

        private void SetBindings()
        {
            guiKeyConfig = Config.Bind("Hotkeys", "Open Mod Menu", defaultGuiKey, "Hotkey to open the Minimap mod menu");
            toggleMinimapKeyConfig = Config.Bind("Hotkeys", "Toggle Minimap", defaultToggleMinimapKey, "Hotkey to toggle the visibility of your Minimap");
            toggleOverrideKeyConfig = Config.Bind("Hotkeys", "Toggle Override Ship", defaultToggleOverrideKey, "Hotkey to toggle the override ship controls");
            switchTargetKeyConfig = Config.Bind("Hotkeys", "Switch Minimap Focus", defaultSwitchTargetKey, "Hotkey to switch the Minimap focus");
            enableMinimapConfig = Config.Bind("Basic Settings", "Enable Minimap", true, "Toggles visibility of your Minimap");
            autoRotateConfig = Config.Bind("Basic Settings", "Auto Rotate", true, "Auto-rotate the Map based on where you are facing");
            minimapSizeConfig = Config.Bind("Basic Settings", "Minimap Size", defaultMinimapSize, "Adjusts the size of your Minimap");
            minimapXPosConfig = Config.Bind("Basic Settings", "X Offset", defaultXoffset, "Shifts the Minimap position horizontally");
            minimapYPosConfig = Config.Bind("Basic Settings", "Y Offset", defaultYoffset, "Shifts the Minimap position vertically");
            minimapZoomConfig = Config.Bind("Basic Settings", "Map Zoom", defaultMapZoom, "Adjust the Map zoom level");
            brightnessConfig = Config.Bind("Basic Settings", "Brightness", defaultBrightness, "Adjust the brightness level");
            showLootsConfig = Config.Bind("Minimap Icons", "Show Loots", false, "Toggles visibility of loots (small triangles) on your Minimap");
            showEnemiesConfig = Config.Bind("Minimap Icons", "Show Enemies", false, "Toggles visibility of enemies (red circles) on your Minimap");
            showTurretConfig = Config.Bind("Turret Icons", "Show Turret", false, "Toggles visibility of Turret on your Minimap");
            showLandminesConfig = Config.Bind("Landmine Icons", "Show Landmine", false, "Toggles visibility of Landmines on your Minimap");
            showLivePlayersConfig = Config.Bind("Minimap Icons", "Show Live Players", true, "Toggles visibility of live players (cyan circles) on your Minimap");
            showDeadPlayersConfig = Config.Bind("Minimap Icons", "Show Dead Players", true, "Toggles visibility of dead players (greyed-out cyan circles) on your Minimap");
            showRadarBoostersConfig = Config.Bind("Minimap Icons", "Show Radar Boosters", true, "Toggles visibility of radar boosters (blue circles) on your Minimap");
            showTerminalCodesConfig = Config.Bind("Minimap Icons", "Show Terminal Codes", false, "Toggles visibility of terminal codes on your Minimap");
            showShipArrowConfig = Config.Bind("Minimap Icons", "Show Ship Arrow", true, "Toggles visibility of the arrow pointing to the Ship");
            freezePlayerIndexConfig = Config.Bind("Advance Settings", "Override Ship Controls", false, "Disables the ability to change the Minimap focus through the ship control panel, allowing Minimap focus changes only through the mod menu");
        }

        public void SyncGUIFromConfigs()
        {
            minimapGUI.guiKey.Key = guiKeyConfig.Value;
            minimapGUI.toggleMinimapKey.Key = toggleMinimapKeyConfig.Value;
            minimapGUI.toggleOverrideKey.Key = toggleOverrideKeyConfig.Value;
            minimapGUI.switchTargetKey.Key = switchTargetKeyConfig.Value;
            minimapGUI.enableMinimap = enableMinimapConfig.Value;
            minimapGUI.autoRotate = autoRotateConfig.Value;
            minimapGUI.minimapSize = minimapSizeConfig.Value;
            minimapGUI.minimapXPos = minimapXPosConfig.Value;
            minimapGUI.minimapYPos = minimapYPosConfig.Value;
            minimapGUI.minimapZoom = minimapZoomConfig.Value;
            minimapGUI.brightness = brightnessConfig.Value;
            minimapGUI.showLoots = false; // Force disable "Show Loots"
            minimapGUI.showEnemies = false; // Force disable "Enemies"
            minimapGUI.showLivePlayers = showLivePlayersConfig.Value;
            minimapGUI.showDeadPlayers = showDeadPlayersConfig.Value;
            minimapGUI.showRadarBoosters = showRadarBoostersConfig.Value;
            minimapGUI.showTerminalCodes = false; // Force disable "TerminalCodes"
            minimapGUI.showShipArrow = showShipArrowConfig.Value;
            minimapGUI.freezePlayerIndex = freezePlayerIndexConfig.Value;
            //Adds Dummmy Nullable Boxes to be true fakely. Hehehe~
            minimapGUI.TheNekoWasHereAsThisIsANullableDummyBox1 = true;
            minimapGUI.TheNekoWasHereAsThisIsANullableDummyBox2 = true;
            minimapGUI.TheNekoWasHereAsThisIsANullableDummyBox3 = true;
            minimapGUI.TheNekoWasHereAsThisIsANullableDummyBox4 = true;
            minimapGUI.TheNekoWasHereAsThisIsANullableDummyBox5 = true;
        }

        public void SyncConfigFromGUI()
        {
            guiKeyConfig.Value = minimapGUI.guiKey.Key;
            toggleMinimapKeyConfig.Value = minimapGUI.toggleMinimapKey.Key;
            toggleOverrideKeyConfig.Value = minimapGUI.toggleOverrideKey.Key;
            switchTargetKeyConfig.Value = minimapGUI.switchTargetKey.Key;
            enableMinimapConfig.Value = minimapGUI.enableMinimap;
            autoRotateConfig.Value = minimapGUI.autoRotate;
            minimapSizeConfig.Value = minimapGUI.minimapSize;
            minimapXPosConfig.Value = minimapGUI.minimapXPos;
            minimapYPosConfig.Value = minimapGUI.minimapYPos;
            minimapZoomConfig.Value = minimapGUI.minimapZoom;
            brightnessConfig.Value = minimapGUI.brightness;
            //showLootsConfig.Value = false; // Removes "Show Loots" from config
            //showEnemiesConfig.Value = false; // Removes "Show Enemies" from config
            //showTurretConfig.Value = false; // Removes "Show Turret" from config
            //showLandminesConfig.Value = false; // Removes "Show Landmine" from config
            showLivePlayersConfig.Value = minimapGUI.showLivePlayers;
            showDeadPlayersConfig.Value = minimapGUI.showDeadPlayers;
            showRadarBoostersConfig.Value = minimapGUI.showRadarBoosters;
            //showTerminalCodesConfig.Value = false; // Removes "Show Terminal Codes" from config
            showShipArrowConfig.Value = minimapGUI.showShipArrow;
            freezePlayerIndexConfig.Value = minimapGUI.freezePlayerIndex;
        }
    }
}
