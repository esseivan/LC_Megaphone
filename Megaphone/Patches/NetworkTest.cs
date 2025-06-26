using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Unity.Netcode;

namespace Megaphone.Patches;

[HarmonyPatch]
internal class NetworkTest
{
    [HarmonyPostfix, HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewFloor))]
    static void SubscribeToHandler()
    {
        MyLog.Logger.LogError("SubscribeToHandler() called");
        ExampleNetworkHandler.LevelEvent += ReceivedEventFromServer;
    }

    [
        HarmonyPostfix,
        HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))
    ]
    static void UnsubscribeFromHandler()
    {
        MyLog.Logger.LogError("UnsubscribeFromHandler() called");
        ExampleNetworkHandler.LevelEvent -= ReceivedEventFromServer;
    }

    static void ReceivedEventFromServer(string eventName)
    {
        // Event Code Here
        MyLog.Logger.LogError("Successfully received RPC");
    }

    public static void SendEventToClients(string eventName)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            return;

        MyLog.Logger.LogError("Sending RPC...");
        ExampleNetworkHandler.Instance.EventClientRpc(eventName);
    }
}
