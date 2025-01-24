using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class Init : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {
            await UnityServices.InitializeAsync();

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                AuthenticationService.Instance.ClearSessionToken(); //Use this so that new player id can be allocated  to each build or player. if this is not used, every player will have same player id and will not be able to join the lobby.
                
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); //Used for authentication using apple, fb, google etc. Anonymous is tied to machine, so if we uninstall the game, all the data will be lost.

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    string username = PlayerPrefs.GetString("username");
                    if (username == "")
                    {
                        username = "Player";
                        PlayerPrefs.SetString("username", username);
                    }

                    SceneManager.LoadSceneAsync("MainMenu");
                }
            }
        }

        private void OnSignedIn()
        {
            Debug.Log($"PlayerId: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"Token: {AuthenticationService.Instance.AccessToken}");
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}