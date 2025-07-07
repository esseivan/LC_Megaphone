using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Megaphone.Scripts;
using UnityEngine;

namespace Megaphone.Patches;

[HarmonyPatch]
public class AudioPatch
{
    public static List<ulong> EnableHighPassIndexes = new List<ulong>();

    public static Dictionary<ulong, float> playersPitchTargets = new Dictionary<ulong, float>();

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
        MyLog.LogDebug($"Player {player.name} connected");
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
            if (voiceChatAudioSource == null)
            {
                MyLog.LogError("voiceChatAudioSource is null...");
            }
            voiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = true;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    private static void PlayerControllerBUpdatePostFix(PlayerControllerB __instance)
    {
        if (__instance == null)
            return;

        ulong clientId = __instance.playerClientId;
        if (!playersPitchTargets.ContainsKey(clientId))
            return;

        float targetPitch = playersPitchTargets[clientId];

        if (targetPitch < 0.5f || targetPitch > 2.0f || targetPitch == 1.0f)
            return;

        MyLog.LogDebug($"Overwriting pitch for {clientId} to {targetPitch}");

        SoundManager.Instance.playerVoicePitchTargets[clientId] = targetPitch;
    }
}
