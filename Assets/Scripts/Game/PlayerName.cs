using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class PlayerName : NetworkBehaviour
    {
        [SerializeField] private TextMeshPro playerName;

        private NetworkVariable<FixedString64Bytes> _networkPlayerName = new NetworkVariable<FixedString64Bytes>();

        private void Start()
        {
            if (IsOwner)
            {
                var playerNameText = PlayerPrefs.GetString("PlayerName", "Player");
                SetPlayerNameServerRPC(playerNameText);
            }
        }

        private void OnEnable()
        {
            _networkPlayerName.OnValueChanged += OnValueChanged;
        }

        public override void OnDestroy()
        {
            _networkPlayerName.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
        {
            playerName.SetText(newvalue.ToString());
        }

        [ServerRpc]
        private void SetPlayerNameServerRPC(FixedString64Bytes newName)
        {
            _networkPlayerName.Value = newName;
        }
        
    }
}