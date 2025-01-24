using System;
using System.Collections.Generic;
using Game.Data;
using GameFramework.Events;
using Unity.VisualScripting;
using UnityEngine;
using LobbyEvents = Game.Events.LobbyEvents;

namespace Game
{
    public class LobbySpawner:MonoBehaviour
    {
        [SerializeField]private List<LobbyPlayer> _players;

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }
        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }
        private void OnLobbyUpdated()
        {
            List<LobbyPlayerData> playerDatas = GameLobbyManager.Instance.GetPlayers();
Debug.Log("Player data count:- " + playerDatas.Count);
            for (int i = 0; i < playerDatas.Count; i++)
            {
                LobbyPlayerData data = playerDatas[i];
                _players[i].SetData(data);
            }
        }
    }
    
}