using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Megaphone;

public class ExampleNetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        MyLog.Logger.LogDebug("OnNetworkSpawn() called");
        LevelEvent = null;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
        Instance = this;

        base.OnNetworkSpawn();
    }

    [ClientRpc]
    public void EventClientRpc(string eventName)
    {
        MyLog.Logger.LogDebug("EventClientRpc() called");
        MyLog.Logger.LogDebug($"LevelEvent is {(LevelEvent == null ? "NULL" : "Not null")}");
        LevelEvent?.Invoke(eventName);
    }

    public static event Action<String> LevelEvent;

    //[ServerRpc(RequireOwnership = false)]
    //public void EventServerRPC(/*parameters here*/)
    //{
    //    // code here
    //}

    public static ExampleNetworkHandler Instance { get; private set; }
}
