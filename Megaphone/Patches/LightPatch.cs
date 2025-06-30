using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;

namespace Megaphone.Patches;

[HarmonyPatch(typeof(ShipLights))]
public class LightPatch
{
    [HarmonyPatch(nameof(ShipLights.ToggleShipLights))]
    [HarmonyPrefix]
    private static bool ToggleShipLightsPostfix(ShipLights __instance)
    {
        Item MyCustomItem = Megaphone.Assets.LoadAsset<Item>(Megaphone.ASSET_PATH_MEGAPHONE_ITEM);
        GameObject gameObject = Object.Instantiate<GameObject>(
            MyCustomItem.spawnPrefab,
            __instance.transform.position,
            Quaternion.identity,
            StartOfRound.Instance.propsContainer
        );
        gameObject.GetComponent<GrabbableObject>().fallTime = 0.0f;
        gameObject.GetComponent<NetworkObject>().Spawn();
        gameObject.GetComponent<NetworkObject>().TrySetParent(gameObject.transform.parent);

        return false;

        //MyLog.Logger.LogDebug("ToggleShipLightsPostfix() called");
        //MyLog.Logger.LogDebug($"Lights are {isOn}");
        //ManualCameraRenderer obj = UnityEngine.Object.FindObjectOfType<ManualCameraRenderer>();

        //Terminal terminal = UnityEngine.Object.FindAnyObjectByType<Terminal>();

        //if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        //{
        //    int credits = 500;
        //    terminal.useCreditsCooldown = true;
        //    terminal.groupCredits = credits;
        //    terminal.SyncGroupCreditsServerRpc(
        //        terminal.groupCredits,
        //        terminal.numberOfItemsInDropship
        //    );
        //}

        //NetworkTest.SendEventToClients("light");

        //if (obj is not null && obj.HasMethod("SwitchScreenOn"))
        //{
        //    MyLog.Logger.LogDebug($"TVon is {obj.isScreenOn}");
        //    //obj.SwitchScreenOn(isOn);
        //    //obj.syncingSwitchScreen = true;
        //    //obj.SwitchScreenOnServerRpc(isOn);
        //}
        //else
        //{
        //    MyLog.Logger.LogDebug($"Could not find 'ManualCameraRenderer' : {obj?.ToString()}");
        //}
    }
}
