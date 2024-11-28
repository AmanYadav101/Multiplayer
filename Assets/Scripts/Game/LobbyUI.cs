using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Events;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Image _mapImage;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private TextMeshProUGUI _mapName;
        [SerializeField] private MapSelectionData _mapSelectionData;

        private int _currentMapIndex = 0;

        private void OnEnable()
        {
            _readyButton.onClick.AddListener(OnReadyPressed);
            if (GameLobbyManager.Instance.IsHost)
            {
                _leftButton.onClick.AddListener(OnLeftButtonClicked);
                _rightButton.onClick.AddListener(OnRightButtonClicked);
                _startButton.onClick.AddListener(OnstartButtonClicked);
                Events.LobbyEvents.OnLobbyReady += OnLobbyReady;
            }

            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }


        private void OnDisable()
        {
            _readyButton.onClick.RemoveAllListeners();
            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();
            _startButton.onClick.RemoveAllListeners();

            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
            Events.LobbyEvents.OnLobbyReady -= OnLobbyReady;
        }

        async void Start()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.Instance.GetLobbyCode()}";

            if (!GameLobbyManager.Instance.IsHost)
            {
                _leftButton.gameObject.SetActive(false);
                _rightButton.gameObject.SetActive(false);
            }
            else
            {
              await GameLobbyManager.Instance.SetSelectedMap(_currentMapIndex,
                    _mapSelectionData.maps[_currentMapIndex].SceneName);
            }
        }

        private async void OnLeftButtonClicked()
        {
            if (_currentMapIndex - 1 > 0)
            {
                _currentMapIndex--;
            }
            else
            {
                _currentMapIndex = 0;
            }

            UpdateMap();
            await GameLobbyManager.Instance.SetSelectedMap(_currentMapIndex,
                _mapSelectionData.maps[_currentMapIndex].SceneName);
        }

        private async void OnRightButtonClicked()
        {
            if (_currentMapIndex + 1 < _mapSelectionData.maps.Count - 1)
            {
                _currentMapIndex++;
            }
            else
            {
                _currentMapIndex = _mapSelectionData.maps.Count - 1;
            }

            UpdateMap();
             await GameLobbyManager.Instance.SetSelectedMap(_currentMapIndex,
                _mapSelectionData.maps[_currentMapIndex].SceneName);
        }

        private void UpdateMap()
        {
            _mapImage.sprite = _mapSelectionData.maps[_currentMapIndex].MapThumbnail;
            _mapName.text = _mapSelectionData.maps[_currentMapIndex].MapName;
        }

        private async void OnReadyPressed()
        {
            bool succeed = await GameLobbyManager.Instance.SetPlayerReady();
            if (succeed)
            {
                _readyButton.gameObject.SetActive(false);
            }
        }

        private void OnLobbyUpdated()
        {
            _currentMapIndex = GameLobbyManager.Instance.GetMapIndex();
            UpdateMap();
        }

        private void OnLobbyReady()
        {
            _startButton.gameObject.SetActive(true);
        }

        private async void OnstartButtonClicked()
        {
            await GameLobbyManager.Instance.StartGame();
        }
    }
}