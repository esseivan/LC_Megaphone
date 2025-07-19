using System;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using HarmonyLib;
using Megaphone.Scripts;
using UnityEngine;

namespace Megaphone.Patches;

[HarmonyPatch(typeof(PufferAI))]
public class PufferPatch
{
    [HarmonyPatch(nameof(PufferAI.DetectNoise))]
    [HarmonyPostfix]
    private static void DetectNoisePatch(
        PufferAI __instance,
        Vector3 noisePosition,
        float noiseLoudness,
        int timesPlayedInOneSpot = 0,
        int noiseID = 0
    )
    {
        if (noiseID != MyConfig.SIREN_NOISE_ID)
            return;
        // Special case for siren noise ; Can be heard from further away
        float num = Vector3.Distance(noisePosition, __instance.transform.position);
        if (num > 25.0f)
            return;
        if (
            Physics.Linecast(
                __instance.eye.position,
                noisePosition,
                StartOfRound.Instance.collidersAndRoomMaskAndDefault,
                QueryTriggerInteraction.Ignore
            )
        )
            noiseLoudness /= 2f;
        if (
            (noiseLoudness / num) <= 0.045f
            || (__instance.timeSinceLookingAtNoise <= 4.0f) // Reduced from 5.0
        )
            return;
        __instance.timeSinceLookingAtNoise = 0.0f;
        __instance.lookAtNoise = noisePosition;
    }
}
