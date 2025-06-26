using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace Megaphone.Patches;

[HarmonyPatch(typeof(ShipLights))]
public class LightPatch
{
    [HarmonyPatch(nameof(ShipLights.ToggleShipLights))]
    [HarmonyPostfix]
    private static void ToggleShipLightsPostfix(ShipLights __instance)
    {
        bool isOn = __instance.areLightsOn;

        MyLog.Logger.LogDebug("ToggleShipLightsPostfix() called");
        MyLog.Logger.LogDebug($"Lights are {isOn}");
        ManualCameraRenderer obj = UnityEngine.Object.FindObjectOfType<ManualCameraRenderer>();

        Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

        if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            int credits = 500;
            terminal.useCreditsCooldown = true;
            terminal.groupCredits = credits;
            terminal.SyncGroupCreditsServerRpc(
                terminal.groupCredits,
                terminal.numberOfItemsInDropship
            );
        }

        NetworkTest.SendEventToClients("light");

        if (obj is not null && obj.HasMethod("SwitchScreenOn"))
        {
            MyLog.Logger.LogDebug($"TVon is {obj.isScreenOn}");
            //obj.SwitchScreenOn(isOn);
            //obj.syncingSwitchScreen = true;
            //obj.SwitchScreenOnServerRpc(isOn);
        }
        else
        {
            MyLog.Logger.LogDebug($"Could not find 'ManualCameraRenderer' : {obj?.ToString()}");
        }
    }
}
