using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Data;
using GameFramework.Events;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

namespace GameFramework.Manager
{
    public class LobbyManager : Core.Singleton<LobbyManager>
    {
        private Lobby _lobby;
        private Coroutine _heartBeatCoroutine;
        private Coroutine _refreshLobbyCoroutine;

        public string GetLobbyCode()
        {
            return _lobby?.LobbyCode;
        }

        public string GetHostId()
        {
            return _lobby?.HostId;
        }
        public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<String, String> data, Dictionary<String, String> lobbyData)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Data = SerializeLobbyData(lobbyData),
                IsPrivate = isPrivate,
                Player = player
            };
            try
            {
                _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
            }
            catch (Exception)
            {
                return false;
            }

            Debug.Log($" Created lobby with lobby id {_lobby.Id}");

            _heartBeatCoroutine = StartCoroutine(HeartBeatLobbyCoroutine(_lobby.Id, 6f));
            _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            return true;
        }

     


        private IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                Debug.Log("HeartBeat");
                LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

        private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            while (true)
            {
                Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(lobbyId);
                yield return new WaitUntil(() => task.IsCompleted);
                Lobby newLobby = task.Result;
                if (newLobby.LastUpdated > _lobby.LastUpdated)
                {
                    _lobby = newLobby;
                    LobbyEvents.OnLobbyUpdated?.Invoke(_lobby);
                }

                yield return new WaitForSecondsRealtime(waitTimeSeconds);
            }
        }

        private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
            foreach (var (key, value)in data)
            {
                playerData.Add(key, new PlayerDataObject(
                    visibility: PlayerDataObject.VisibilityOptions.Member,
                    value: value));
            }

            return playerData;
        }

        public void OnApplicationQuit()
        {
            if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
            }
        }


        public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));
            options.Player = player;

            try
            {
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
            }
            catch (Exception)
            {
                return false;
            }

            _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
            return true;
        }

        public List<Dictionary<string, PlayerDataObject>> GetPlayersData()
        {
            List<Dictionary<string, PlayerDataObject>> data = new List<Dictionary<string, PlayerDataObject>>();
            foreach (Player player in _lobby.Players)
            {
                data.Add(player.Data);
            }

            return data;
        }

        public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data)
        {
            Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);

            UpdatePlayerOptions options = new UpdatePlayerOptions()
            {
                Data = playerData
            };
            try
            {
                _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, options);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            LobbyEvents.OnLobbyUpdated(_lobby);
            
            return true; 
        }

        public async Task<bool> UpdatePlayerData(string playerId, Dictionary<string, string> data, string allocationId = default, string connectionData = default)
        {
           Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
           UpdatePlayerOptions options = new UpdatePlayerOptions()
           {
               Data = playerData,
               AllocationId = allocationId,
               ConnectionInfo = connectionData
           };

           try
           {
                _lobby = await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, options);
           }
           catch (Exception )
           {
               return false;
           }
           LobbyEvents.OnLobbyUpdated(_lobby);
           
           return true;
        }
        
        private Dictionary<string, DataObject> SerializeLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>();
            foreach (var (key,value) in data)
            {
                lobbyData.Add(key, new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: value));
            }
            return lobbyData;
        }

        public async Task<bool> UpdateLobbyData(Dictionary<string, string> data)
        {
            Dictionary<string, DataObject> lobbyData = SerializeLobbyData(data);

            UpdateLobbyOptions options = new UpdateLobbyOptions()
            {
                Data = lobbyData
            };

            try
            {
                _lobby = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, options); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            LobbyEvents.OnLobbyUpdated(_lobby);
            return true;
        }

        
    }
}