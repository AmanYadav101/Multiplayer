using System;
using Game.Data;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Game
{
    public class LobbyPlayer:MonoBehaviour
    {
        [SerializeField] private TextMeshPro _playerName;
        [SerializeField] private Renderer _isReadyRenderer;
        
        private MaterialPropertyBlock _propertyBlock;
        private LobbyPlayerData _data;

        private void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.GamerTag;

            if (_data.IsReady)
            {
                if (_isReadyRenderer != null)
                {
                    _isReadyRenderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor("_BaseColor", Color.green);
                    _isReadyRenderer.SetPropertyBlock(_propertyBlock);
                }
            }
            
            gameObject.SetActive(true);

            
        }
    }
}