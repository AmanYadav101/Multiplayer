using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Screens References")] [SerializeField]
        private GameObject _mainScreen;

        [SerializeField] private GameObject _joinScreen;
        [SerializeField] private GameObject _nameScreen;

        [Header("Button References")] [SerializeField]
        private Button _hostButton;

        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button _nameButton;
        [SerializeField] private Button _okayNameButton;

        [Header("Text References")] [SerializeField]
        private TextMeshProUGUI _codeText;

        [SerializeField] private TextMeshProUGUI _nameText;

       
        public static string playerName { get; private set; }
        

        // Start is called before the first frame update
        void OnEnable()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
            _submitButton.onClick.AddListener(OnSubmitCodeClicked);
            _nameButton.onClick.AddListener(OnNameButtonClicked);
            _okayNameButton.onClick.AddListener(OnOkayNameButtonClicked);
        }


        void OnDisable()
        {
            _hostButton?.onClick.RemoveListener(OnHostClicked);
            _joinButton?.onClick.RemoveListener(OnJoinClicked);
            _submitButton?.onClick.RemoveListener(OnSubmitCodeClicked);
            _nameButton?.onClick.RemoveAllListeners();
            _okayNameButton?.onClick.RemoveAllListeners();
        }
        

        private async void OnHostClicked()
        {
            bool succeeded = await GameLobbyManager.Instance.CreateLobby();
            if (succeeded)
            {
                SceneManager.LoadSceneAsync("Lobby");
            }
        }

        private void OnJoinClicked()
        {
            Debug.Log("Join");
            _mainScreen.SetActive(false);
            _joinScreen.SetActive(true);
        }

        private async void OnSubmitCodeClicked()
        {
            string code = _codeText.text;
            code = code.Substring(0, code.Length - 1);

            bool succeeded = await GameLobbyManager.Instance.JoinLobby(code);
            if (succeeded)
            {
                SceneManager.LoadSceneAsync("Lobby");
            }
        }

        private void OnNameButtonClicked()
        {
            _mainScreen.SetActive(false);
            _nameScreen.SetActive(true);
        }

        private void OnOkayNameButtonClicked()
        {
            playerName = _nameText.text;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            
            Debug.Log(playerName);
            
            _mainScreen.SetActive(true);
            _nameScreen.SetActive(false);
        }
    }
}