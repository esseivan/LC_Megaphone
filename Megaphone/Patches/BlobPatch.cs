using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Megaphone.Scripts;
using UnityEngine;

namespace Megaphone.Patches;

[HarmonyPatch(typeof(BlobAI))]
public class BlobPatch
{
    [HarmonyPatch(nameof(BlobAI.DetectNoise))]
    [HarmonyPostfix]
    private static void DetectNoisePatch(
        BlobAI __instance,
        Vector3 noisePosition,
        float noiseLoudness,
        int timesPlayedInOneSpot = 0,
        int noiseID = 0
    )
    {
        // Patch so that the blob is also tamed when it hears the siren noise
        if (
            noiseID != MyConfig.SIREN_NOISE_ID
            || Physics.Linecast(
                __instance.transform.position,
                noisePosition,
                StartOfRound.Instance.collidersAndRoomMask
            )
            || (double)Vector3.Distance(__instance.transform.position, noisePosition) >= 12.0
        )
            return;
        __instance.tamedTimer = 2f;
    }
}
