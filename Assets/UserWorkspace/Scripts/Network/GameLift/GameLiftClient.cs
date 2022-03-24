using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.Runtime;
using Helpers;
using Unity.Netcode;

namespace Network.GameLift
{
    public class GameLiftClient
    {
        private static AmazonGameLiftClient gameLiftClient;
        
        private static string clientGuid;
        private static string clientFleetId;

        public GameLiftClient(string fleetId, AWSCredentials credentials)
        {
            clientGuid = Prefs.Hash;
            clientFleetId = fleetId;

            AmazonGameLiftConfig config = new AmazonGameLiftConfig
            {
                ServiceURL = "http://localhost:8080"
            };

            gameLiftClient = new AmazonGameLiftClient(credentials, config);
        }
        
        public static async Task<GameSession> CreateGameSessionAsync(CreateGameSessionRequest request, CancellationToken ct = default)
        {
            request.FleetId = clientFleetId;
            request.CreatorId = clientGuid;

            CreateGameSessionResponse response = await gameLiftClient.CreateGameSessionAsync(request, ct);
            return response.GameSession;
        }
        
        public static async Task<List<GameSession>> SearchGameSessionsAsync(SearchGameSessionsRequest request, CancellationToken ct = default)
        {
            request.FleetId = clientFleetId;

            SearchGameSessionsResponse response = await gameLiftClient.SearchGameSessionsAsync(request, ct);
            return response.GameSessions;
        }

        public static async Task<PlayerSession> CreatePlayerSessionAsync(NetworkConfig config, GameSession session, CancellationToken ct = default)
        {
            CreatePlayerSessionRequest request = new CreatePlayerSessionRequest
            {
                GameSessionId = session.GameSessionId,
                PlayerId = clientGuid
            };

            CreatePlayerSessionResponse response = await gameLiftClient.CreatePlayerSessionAsync(request, ct);
            PlayerSession playerSession = response.PlayerSession;

            config.ConnectionData = Encoding.UTF8.GetBytes(playerSession.PlayerSessionId);
            
            return playerSession;
        }
    }
}