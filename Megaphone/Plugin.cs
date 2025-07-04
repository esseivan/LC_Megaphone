using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using Megaphone.Items;
using Megaphone.Patches;
using Megaphone.Scripts;
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
public class Plugin : BaseUnityPlugin
{
    public const string ASSET_PATH_MEGAPHONE_ITEM = "Assets/Megaphone/MegaphoneItem.asset";
    public const string ASSET_PATH_MEGAPHONE_TNODE =
        "Assets/Megaphone/iTerminalNodeMegaphone.asset";
    public const string ASSET_PATH_NET_HANDLER = "Assets/Network/ExampleNetworkHandler.prefab";

    public static Plugin Instance { get; private set; } = null!;
    internal static new ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony Harmony { get; set; }

    public static ConfigEntry<bool> configCanBuy;
    public static ConfigEntry<bool> configIsScrap;
    public static ConfigEntry<int> configRarity;
    public static ConfigEntry<int> configPrice;
    public static ConfigEntry<float> configHearDistance;

    public static AssetBundle Assets;
    public static AssetBundle Assets_network;

    private void SetupConfigBinds()
    {
        configCanBuy = Config.Bind(
            "Item",
            "CanBuy",
            true,
            "Can the item be bought from the terminal"
        );

        configIsScrap = Config.Bind("Item", "IsScrap", true, "Can the item spawn in interiors");

        configRarity = Config.Bind(
            "Item",
            "Rarity",
            100,
            "Rarity of the object. 0 is never, 100 is often"
        );

        configPrice = Config.Bind("Item", "Price", 15, "Buy cost of the item");

        configHearDistance = Config.Bind(
            "Audio",
            "Hearing distance modifier",
            2.0f,
            "Change the distance multiplier the voices can be heard from when talking in 'loud mode' (switch with Q)."
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

        CreateItems();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    private static void CreateItems()
    {
        int iPrice = configPrice.Value;
        int iRarity = configRarity.Value;
        Item megaphoneItem = Assets.LoadAsset<Item>(ASSET_PATH_MEGAPHONE_ITEM);
        Logger.LogDebug($"Found item {megaphoneItem.itemName}");
        GrabbableObject script = megaphoneItem.spawnPrefab.AddComponent<MegaphoneItem>();
        Logger.LogDebug($"Found script {script}");
        script.grabbable = true;
        script.isInFactory = true;
        script.itemProperties = megaphoneItem;
        script.grabbableToEnemies = true;

        MyLog.Logger.LogDebug($"Found item '{megaphoneItem.itemName}'");

        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(megaphoneItem.spawnPrefab);

        if (configIsScrap.Value)
        {
            LethalLib.Modules.Items.RegisterScrap(
                megaphoneItem,
                iRarity,
                LethalLib.Modules.Levels.LevelTypes.All
            );
        }

        if (configCanBuy.Value)
        {
            TerminalNode iTerminalNode = Assets.LoadAsset<TerminalNode>(ASSET_PATH_MEGAPHONE_TNODE);
            LethalLib.Modules.Items.RegisterShopItem(
                megaphoneItem,
                null,
                null,
                iTerminalNode,
                iPrice
            );
        }
        else
        {
            LethalLib.Modules.Items.RegisterItem(megaphoneItem);
        }
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
        //AddCommand(
        //    "ron",
        //    new CommandInfo
        //    {
        //        Category = "other",
        //        Description = "Enable robot voices",
        //        DisplayTextSupplier = AudioMod.EnableRobotVoice,
        //    }
        //);

        //AddCommand(
        //    "roff",
        //    new CommandInfo
        //    {
        //        Category = "other",
        //        Description = "Disable robot voices",
        //        DisplayTextSupplier = AudioMod.DisableRobotVoice,
        //    }
        //);
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
