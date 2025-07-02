using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Megaphone.Scripts;

namespace Megaphone.Patches;

[HarmonyPatch]
public class AudioPatch
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
    [HarmonyPostfix]
    private static void ConnectClientToPlayerObjectPatch(
        StartOfRound __instance,
        ulong clientId,
        int connectedPlayers,
        ulong[] connectedPlayerIdsOrdered,
        int assignedPlayerObjectId,
        int serverMoneyAmount,
        int levelID,
        int profitQuota,
        int timeUntilDeadline,
        int quotaFulfilled,
        int randomSeed,
        bool isChallenge
    )
    {
        PlayerControllerB player = __instance.allPlayerScripts[assignedPlayerObjectId];
        MyLog.Logger.LogDebug($"Player {player.name} connected");
        AudioMod.RegisterNewPlayer(player);
    }
}
