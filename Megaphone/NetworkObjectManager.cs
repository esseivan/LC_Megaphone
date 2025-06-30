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

        networkPrefab = (GameObject)
            Plugin.Assets_network.LoadAsset("Assets/ExampleNetworkHandler.prefab");
        networkPrefab.AddComponent<ExampleNetworkHandler>();

        NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        MyLog.Logger.LogError("ExampleNetworkHandler successfully added");
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
            MyLog.Logger.LogError("NetworkObject successfully spawned");
        }
    }

    static GameObject networkPrefab;
}
