using System.Collections.Generic;
using Amazon.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace Network.GameLift
{
    public class GameLiftInitializer : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager networkManager;

        [SerializeField]
        private List<string> logPaths;
        
#if UNITY_SERVER
        private GameLiftServer gameLiftServer;

        private void Awake()
        {
            gameLiftServer = new GameLiftServer(networkManager, logPaths);
            gameLiftServer.InitSDK();
        }

        private void OnApplicationQuit()
        {
            gameLiftServer.Dispose();
        }
#else
        private GameLiftClient gameLiftClient;
        
        private void Awake()
        {
            const string fleet = "fleet-123";
            gameLiftClient = new GameLiftClient(fleet, new AnonymousAWSCredentials());
        }
#endif
    }
}