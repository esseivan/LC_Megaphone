using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Megaphone;

public class ExampleNetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        MyLog.Logger.LogError("OnNetworkSpawn() called");
        LevelEvent = null;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
        Instance = this;

        base.OnNetworkSpawn();
    }

    [ClientRpc]
    public void EventClientRpc(string eventName)
    {
        MyLog.Logger.LogError("EventClientRpc() called");
        MyLog.Logger.LogError($"LevelEvent is {(LevelEvent == null ? "NULL" : "Not null")}");
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
