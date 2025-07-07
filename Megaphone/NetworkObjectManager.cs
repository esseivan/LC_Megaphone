using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Megaphone;

[HarmonyPatch]
public class NetworkObjectManager
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    public static void Init()
    {
        if (networkPrefab != null)
            return;

        networkPrefab = (GameObject)Plugin.Assets_network.LoadAsset(Plugin.ASSET_PATH_NET_HANDLER);
        //networkPrefab.AddComponent<ExampleNetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        MyLog.LogDebug("ExampleNetworkHandler successfully added");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
    static void SpawnNetworkHandler()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            var networkHandlerHost = Object.Instantiate(
                networkPrefab,
                Vector3.zero,
                Quaternion.identity
            );
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            MyLog.LogDebug("NetworkObject successfully spawned");
        }
    }

    static GameObject networkPrefab;
}
