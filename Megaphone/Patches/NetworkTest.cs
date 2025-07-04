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
        MyLog.Logger.LogDebug("SubscribeToHandler() called");
        ExampleNetworkHandler.LevelEvent += ReceivedEventFromServer;
    }

    [
        HarmonyPostfix,
        HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))
    ]
    static void UnsubscribeFromHandler()
    {
        MyLog.Logger.LogDebug("UnsubscribeFromHandler() called");
        ExampleNetworkHandler.LevelEvent -= ReceivedEventFromServer;
    }

    static void ReceivedEventFromServer(string eventName)
    {
        // Event Code Here
        MyLog.Logger.LogDebug("Successfully received RPC");
    }

    public static void SendEventToClients(string eventName)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            return;

        MyLog.Logger.LogDebug("Sending RPC...");
        ExampleNetworkHandler.Instance.EventClientRpc(eventName);
    }
}
