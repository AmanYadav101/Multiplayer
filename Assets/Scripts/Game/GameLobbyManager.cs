using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Data;
using GameFramework.Events;
using GameFramework.Manager;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using LobbyPlayerData = Game.Data.LobbyPlayerData;

namespace Game
{
    public class GameLobbyManager : GameFramework.Core.Singleton<GameLobbyManager>
    {

        private List<LobbyPlayerData> _lobbyPlayerData = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        private LobbyData _lobbyData;

        private int _maxNumberOfPlayers = 4;

        private bool _inGame = false;
        public bool IsHost => _localLobbyPlayerData.Id == LobbyManager.Instance.GetHostId();
        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        public string GetLobbyCode()
        {
            return LobbyManager.Instance.GetLobbyCode();
        }

        public async Task<bool> CreateLobby()
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, MainMenuController.playerName);
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(0);
            bool succeeded = await LobbyManager.Instance.CreateLobby(_maxNumberOfPlayers, true, _localLobbyPlayerData.Serialize(), _lobbyData.Serialize());
            return succeeded;
        }


        public async Task<bool> JoinLobby(string code)
        {
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, MainMenuController.playerName);
            bool succeeded = await LobbyManager.Instance.JoinLobby(code, _localLobbyPlayerData.Serialize());

            return succeeded;
        }

        private async void OnLobbyUpdated(Lobby lobby)
        {
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayersData();
            _lobbyPlayerData.Clear();

            int numberOfPlayerReady = 0;
            
            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.IsReady)
                {
                    numberOfPlayerReady++;
                }

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;
                }

                _lobbyPlayerData.Add(lobbyPlayerData);
                Debug.Log("Player data:- " + data);
            }
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(lobby.Data);
            Events.LobbyEvents.OnLobbyUpdated?.Invoke();

            if (numberOfPlayerReady == lobby.Players.Count)
            {
                Events.LobbyEvents.OnLobbyReady?.Invoke();
            }

            if (_lobbyData.RelayJoinCode != default && !_inGame)
            {
                await joinRelayServer(_lobbyData.RelayJoinCode);
                SceneManager.LoadSceneAsync(_lobbyData.SceneName);
            }
            
        }

      

        public List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayerData;
        }

        public async Task<bool> SetPlayerReady()
        {
            _localLobbyPlayerData.IsReady = true;
            return await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize());
        }

        public int GetMapIndex()
        {
            return _lobbyData.MapIndex;
        }

        public async Task<bool> SetSelectedMap(int currentMapIndex, string sceneName)
        {
            _lobbyData.MapIndex = currentMapIndex;
            _lobbyData.SceneName = sceneName;
            return await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize()); 
        }

        public async Task StartGame()
        {
            string relayJoinCode = await RelayManager.Instance.CreateRelay(_maxNumberOfPlayers);
            _inGame = true;
            _lobbyData.RelayJoinCode = relayJoinCode;
            await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());

            string allocationId = RelayManager.Instance.GetAllocationId();
            string connectionData = RelayManager.Instance.GetConnectionData();
            await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);
            
            SceneManager.LoadSceneAsync(_lobbyData.SceneName);
        }
        
        private async Task<bool> joinRelayServer(string relayJoinCode)
        {
            _inGame = true;
            await RelayManager.Instance.JoinRelay(relayJoinCode);
            string allocationId = RelayManager.Instance.GetAllocationId();
            string connectionData = RelayManager.Instance.GetConnectionData();
            await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

            return true;
        }
        
        
    }
}