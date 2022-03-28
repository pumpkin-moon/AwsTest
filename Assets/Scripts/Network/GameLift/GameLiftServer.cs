using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Aws.GameLift;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

namespace Network.GameLift
{
    public class GameLiftServer : IDisposable
    {
        public static event ProcessParameters.OnStartGameSessionDelegate GameSessionStarted;
        public static event ProcessParameters.OnUpdateGameSessionDelegate GameSessionUpdated;
        public static event ProcessParameters.OnProcessTerminateDelegate ProcessTerminated;
        public static event ProcessParameters.OnHealthCheckDelegate HealthChecked;

        private readonly int listeningPort;
        private readonly List<string> logPaths;
        
        private readonly Dictionary<ulong, string> playerSessions = new Dictionary<ulong, string>();
        private readonly NetworkManager networkManager;

        public GameLiftServer(NetworkManager networkManager, List<string> logPaths)
        {
            this.logPaths = logPaths;
            this.networkManager = networkManager;
            
            listeningPort = ((UNetTransport) networkManager.NetworkConfig.NetworkTransport).ConnectPort;
            Subscribe();
        }

        private void Subscribe()
        {
            networkManager.ConnectionApprovalCallback += OnConnectionApproval;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void Unsubscribe()
        {
            networkManager.ConnectionApprovalCallback -= OnConnectionApproval;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        private void OnConnectionApproval(byte[] payload, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
        {
            string playerSession = Encoding.UTF8.GetString(payload);
            if (AcceptPlayerSession(playerSession))
            {
                Debug.Log($"Player {playerSession} connected");
                playerSessions.Add(clientId, playerSession);
            }

            callback(false, null, true, null, null);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (RemovePlayerSession(playerSessions[clientId]))
            {
                Debug.Log($"Player {playerSessions[clientId]} disconnected");
                playerSessions.Remove(clientId);
            }
        }

        private static void AssertOutcome(GenericOutcome outcome, [CallerMemberName] string prefix = null,
            Action onSuccess = null, Action onFailure = null)
        {
            if (outcome.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke();
                Debug.LogError($"{prefix} failure: {outcome.Error}");
            }
        }

        private static void LogCaller([CallerMemberName] string caller = null)
        {
            Debug.Log(caller);
        }

        public void InitSDK()
        {
            LogCaller();
            
            GenericOutcome initSDKOutcome = GameLiftServerAPI.InitSDK();
            AssertOutcome(initSDKOutcome, onSuccess: ProcessReady);
        }

        private void ProcessReady()
        {
            LogCaller();
            
            LogParameters logParameters = new LogParameters(logPaths);

            ProcessParameters processParameters = new ProcessParameters(OnStartGameSession, OnUpdateGameSession,
                OnProcessTerminate, OnHealthCheck, listeningPort, logParameters);

            GenericOutcome outcome = GameLiftServerAPI.ProcessReady(processParameters);
            AssertOutcome(outcome);
        }

        private void OnStartGameSession(GameSession gameSession)
        {
            LogCaller();
            
            GenericOutcome outcome = GameLiftServerAPI.ActivateGameSession();
            AssertOutcome(outcome, onSuccess: () => GameSessionStarted?.Invoke(gameSession));
        }

        private void OnUpdateGameSession(UpdateGameSession updateGameSession)
        {
            LogCaller();
            
            GameSessionUpdated?.Invoke(updateGameSession);
        }

        private void OnProcessTerminate()
        {
            LogCaller();
            
            ProcessTerminated?.Invoke();

            GenericOutcome outcome = GameLiftServerAPI.ProcessEnding();
            AssertOutcome(outcome, onSuccess: () =>
            {
                Debug.Log("Game session terminated.");
                Application.Quit();
            });
        }

        private bool OnHealthCheck()
        {
            LogCaller();
            
            bool? v = HealthChecked?.Invoke();
            return !v.HasValue || v.Value;
        }

        private bool AcceptPlayerSession(string playerSessionId)
        {
            LogCaller();
            
            GenericOutcome outcome = GameLiftServerAPI.AcceptPlayerSession(playerSessionId);
            AssertOutcome(outcome);

            return outcome.Success;
        }

        private bool RemovePlayerSession(string playerSessionId)
        {
            LogCaller();
            
            GenericOutcome outcome = GameLiftServerAPI.RemovePlayerSession(playerSessionId);
            AssertOutcome(outcome);

            return outcome.Success;
        }

        public void Dispose()
        {
            Unsubscribe();
            OnProcessTerminate();
        }
    }
}
