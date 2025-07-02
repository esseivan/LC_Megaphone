using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Megaphone.Scripts;
using UnityEngine;
using static ES3Spreadsheet;

namespace Megaphone.Patches;

[HarmonyPatch]
public class AudioPatch
{
    public static List<ulong> EnableHighPassIndexes = new List<ulong>();

    public static void EnableHighpass(ulong index, bool on)
    {
        if (on)
            EnableHighPassIndexes.Add(index);
        else if (EnableHighPassIndexes.Contains(index))
            EnableHighPassIndexes.Remove(index);
    }

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
        // Register new connected user
        PlayerControllerB player = __instance.allPlayerScripts[assignedPlayerObjectId];
        MyLog.Logger.LogDebug($"Player {player.name} connected");
        AudioMod.RegisterNewPlayer(player);
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.UpdatePlayerVoiceEffects))]
    [HarmonyPostfix]
    private static void UpdatePlayerVoiceEffectsPostfix(StartOfRound __instance)
    {
        // Ensure high pass filter stays enabled
        // No need to disable, it will be done automatically
        foreach (ulong i in EnableHighPassIndexes)
        {
            AudioSource voiceChatAudioSource = __instance
                .allPlayerScripts[i]
                .currentVoiceChatAudioSource;
            voiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = true;
        }
    }
}
