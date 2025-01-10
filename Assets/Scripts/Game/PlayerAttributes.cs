using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PlayerAttributes : NetworkBehaviour
    {
        public Image healthBar;
        public NetworkVariable<float> healthPoints = new NetworkVariable<float>(100f);
        
        private void Update()
        {
            healthBar.fillAmount = healthPoints.Value / 100f;
        }

        private void OnCollisionEnter(Collision collider)
        {
            if (collider.gameObject.CompareTag("projectile"))
            {
                healthPoints.Value -= 10;
            }
        }
    }
}