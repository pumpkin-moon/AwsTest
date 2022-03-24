using Unity.Netcode;
using UnityEngine;

#if UNITY_SERVER
using Network;
#endif

public class TestServer : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;
    
#if UNITY_SERVER
    private NetworkManagerCallbackLogger logger;

    private void Start()
    {
        logger = new NetworkManagerCallbackLogger(networkManager);
        networkManager.StartServer();
    }

    private void OnApplicationQuit()
    {
        networkManager.Shutdown();
    }
#endif
}
