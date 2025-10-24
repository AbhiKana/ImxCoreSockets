using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ImxCoreSockets
{
    public class ConnectViaInput : MonoBehaviour
    {
        [Tooltip("Select start as a Server or Client")]
        [Header("Start as")]
        MainManager.SelectType starting;

        public ClientServerSelector clientServerSelector;

        private bool isConnectedToServer;
        public bool IsConnectedToServer
        {
            get { return isConnectedToServer; }
            set
            {
                isConnectedToServer = value;
                Debug.Log("Connection status changed: " + value);
            }
        }

        //Drop ClientInputCanvas panel into ClientManagerwithInput Prefab
        //Change your Canvas UI according to your story board 
        //Reference the IP connection Gamobject into the Input Panel variable in this script through Unity Editor 
        [SerializeField] GameObject InputPanel;

        public GameObject panel => InputPanel;

        public string ipKey;
        public bool IsConnected = false;
        public bool IsReconnecting = false;
        public bool CheckMaxAttempts = false;

        public UnityEvent OnConnectToserver;
        public UnityEvent OnServerDisconnect;
        public UnityEvent OnServerNotFound;

        public void Awake()
        {
            if (InputPanel != null)
            {
                InputPanel.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    Debug.Log("Auto Click");
                    SetIP(InputPanel.GetComponentInChildren<TMP_InputField>());
                    clientServerSelector.ProcessStart();
                });
            }
        }

        private void Start()
        {
            TCP_ClientController.OnMessageReceived += ReconnectToServer;
            ConnectToServer();
        }

        [ContextMenu("Reconnect")]
        public void ReInitialize()
        {
            clientServerSelector.StartApp();
        }


        public void SetIP(TMP_InputField inputText)
        {
            if (string.IsNullOrEmpty(inputText.text.Trim()) || string.IsNullOrWhiteSpace(inputText.text.Trim()) || !Regex.IsMatch(inputText.text, @"([0-9]{1,3})[.]([0-9]{1,3})[.]([0-9]{1,3})[.]([0-9]{1,3})"))
            {
                inputText.text = "Enter Correct IP";
                return;
            }

            if (!IsConnected)
            {
                ipKey = inputText.text.Trim();
                PlayerPrefs.SetString(nameof(ipKey), ipKey);
                Debug.Log("Server not found");
                IsConnectedToServer = false;
                OnServerNotFound?.Invoke();
            }
        }

        private void GetIPFrom_InputField()
        {
            if (PlayerPrefs.HasKey(nameof(ipKey)))
            {
                ipKey = PlayerPrefs.GetString(nameof(ipKey));
                InputPanel.GetComponentInChildren<TMP_InputField>().text = ipKey.Trim();
                clientServerSelector.ProcessStart();
            }
            else
            {
                InputPanel.SetActive(true);
                InputPanel.GetComponentInChildren<TMP_InputField>().text = string.Empty;
            }
        }

        private void ConnectToServer()
        {
            starting = clientServerSelector.selectedType;
            if (starting == MainManager.SelectType.Client)
            {
                if (InputPanel != null)
                {
                    GetIPFrom_InputField();
                }
            }
            TCP_ClientController.OnConnect += ClientConnected;
        }

        void ReconnectToServer(string msg)
        {
            if (msg.Contains("Server Disconnected"))
            {
                TCP_ClientController.OnServerDisconnected?.Invoke();
                OnServerDisconnect?.Invoke();
                IsConnected = false;
                IsConnectedToServer = false;
                IsReconnecting = true;
                Debug.Log("Please, Reconnect to server");
                //InvokeRepeating(nameof(InitializeClient), 3f, 3f);
            }
        }

        public void ClientConnected()
        {
            UpdateConnectionStatus();
            OnConnectToserver?.Invoke();
        }

        private void UpdateConnectionStatus()
        {
            isConnectedToServer = IsConnected = true;
            if (IsReconnecting)
            {
                IsReconnecting = false;
            }
        }

        public void EnableInputField(bool val)
        {
            InputPanel.SetActive(val);
        }

        public void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
