using System;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkManagerCallbackLogger : IDisposable
    {
        private readonly NetworkManager networkManager;
        
        public NetworkManagerCallbackLogger(NetworkManager networkManager)
        {
            this.networkManager = networkManager;
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            networkManager.OnServerStarted += OnServerStarted;
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void UnregisterCallbacks()
        {
            networkManager.OnServerStarted -= OnServerStarted;
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong obj)
        {
            Debug.Log($"{nameof(OnClientDisconnected)}: {obj}");
        }

        private void OnClientConnected(ulong obj)
        {
            Debug.Log($"{nameof(OnClientConnected)}: {obj}");
        }

        private void OnServerStarted()
        {
            Debug.Log(nameof(OnServerStarted));
        }

        public void Dispose()
        {
            UnregisterCallbacks();
        }
    }
}