using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Services.Lobbies.Models;
using UnityEngine.PlayerLoop;

namespace Game.Data
{
    public class LobbyData
    {
        private int _mapIndex;
        private string _relayJoinCode;
        private string _sceneName;

        public int MapIndex
        {
            get => _mapIndex;
            set => _mapIndex = value;
        }

        public string SceneName
        {
            get => _sceneName;
            set => _sceneName = value;
        }

        public string RelayJoinCode
        {
            get => _relayJoinCode;
            set => _relayJoinCode = value;
        }

        public void Initialize(int mapIndex)
        {
            _mapIndex = mapIndex;
        }

        public void Initialize(Dictionary<string, DataObject> lobbyData)
        {
            UpdateState(lobbyData);
        }

        public void UpdateState(Dictionary<string, DataObject> lobbyData)
        {
            if (lobbyData.ContainsKey("MapIndex"))
            {
                MapIndex = Int32.Parse(lobbyData["MapIndex"].Value);
            }

            if (lobbyData.ContainsKey("RelayJoinCode"))
            {
                _relayJoinCode = lobbyData["RelayJoinCode"].Value;
            }

            if (lobbyData.ContainsKey("SceneName"))
            {
                _sceneName = lobbyData["SceneName"].Value;
            }
        }

        public Dictionary<string, string> Serialize()
        {
            return new Dictionary<string, string>()
            {
                { "MapIndex", MapIndex.ToString() },
                { "RelayJoinCode", _relayJoinCode },
                { "SceneName", _sceneName }
            };
        }
    }
}