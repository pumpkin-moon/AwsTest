using System.Reflection;
using System.Threading.Tasks;
using Amazon.GameLift.Model;
using Network.GameLift;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class TestClient : MonoBehaviour
{
    [SerializeField]
    private NetworkManager networkManager;
    
#if !UNITY_SERVER
    private async void Start()
    {
        PlayerSession session = await CreateGame();
        LogSession(session);
        
        UNetTransport transport = (UNetTransport) networkManager.NetworkConfig.NetworkTransport;
        transport.ConnectPort = session.Port;
        transport.ConnectAddress = session.IpAddress;
        
        networkManager.StartClient();
    }

    private void LogSession(PlayerSession ps)
    {
        foreach (PropertyInfo propertyInfo in typeof(PlayerSession).GetProperties())
        {
            Debug.Log($"{propertyInfo.Name}: {propertyInfo.GetValue(ps)}");
        }
    }

    private async Task<PlayerSession> CreateGame()
    {
        CreateGameSessionRequest createRequest = new CreateGameSessionRequest
        {
            MaximumPlayerSessionCount = 2
        };
        
        GameSession gameSession = await GameLiftClient.CreateGameSessionAsync(createRequest);
        PlayerSession playerSession = await GameLiftClient.CreatePlayerSessionAsync(networkManager.NetworkConfig, gameSession);

        return playerSession;
    } 

    private void OnApplicationQuit()
    {
        networkManager.Shutdown();
    }
#endif
}