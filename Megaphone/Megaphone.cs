using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using Megaphone.Patches;
using Newtonsoft.Json;
using TerminalApi;
using TerminalApi.Classes;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace Megaphone;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("atomic.terminalapi", MinimumDependencyVersion: "1.5.0")]
[BepInDependency(LethalLib.Plugin.ModGUID)]
//[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
//[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class Megaphone : BaseUnityPlugin
{
    public static Megaphone Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony Harmony { get; set; }

    public static ConfigEntry<string> configGreeting;
    public static ConfigEntry<bool> configDisplayGreeting;

    public static AssetBundle Assets;
    public static AssetBundle Assets_network;

    private void SetupConfigBinds()
    {
        configGreeting = Config.Bind(
            "General", // The section under which the option is shown
            "GreetingText", // The key of the configuration option in the configuration file
            "Hello, world!", // The default value
            "A greeting text to show when the game is launched"
        ); // Description of the option to show in the config file

        configDisplayGreeting = Config.Bind(
            "General.Toggles",
            "DisplayGreeting",
            true,
            "Whether or not to show the greeting text"
        );
    }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        SetupConfigBinds();

        LoadAssets();

        Patch();

        CreateTerminalCommands();

        if (configDisplayGreeting.Value)
            Logger.LogDebug(configGreeting.Value);

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void LoadAssets()
    {
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        Assets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "assets/esn_megaphone"));
        if (Assets == null)
        {
            Logger.LogError("Failed to load custom assets.");
            return;
        }

        Assets_network = AssetBundle.LoadFromFile(
            Path.Combine(sAssemblyLocation, "assets/esn_network")
        );
        if (Assets == null)
        {
            Logger.LogError("Failed to load network assets.");
            return;
        }

        Logger.LogDebug($"Assets loaded");
        string[] names = Assets.GetAllAssetNames();
        foreach (string name in names)
        {
            Logger.LogDebug($"{name}");
        }
        names = Assets_network.GetAllAssetNames();
        foreach (string name in names)
        {
            Logger.LogDebug($"{name}");
        }
    }

    private static void CreateTerminalCommands()
    {
        AddCommand(
            "ron",
            new CommandInfo
            {
                Category = "other",
                Description = "Enable robot voices",
                DisplayTextSupplier = Commands.EnableRobotVoice,
            }
        );

        AddCommand(
            "roff",
            new CommandInfo
            {
                Category = "other",
                Description = "Disable robot voices",
                DisplayTextSupplier = Commands.DisableRobotVoice,
            }
        );
    }

    private static void NetcodePatcher()
    {
        Logger.LogDebug("Patching netcode...");

        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
            );
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(
                    typeof(RuntimeInitializeOnLoadMethodAttribute),
                    false
                );
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
        Logger.LogDebug("Patching netcode... complete");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();
        NetcodePatcher();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
