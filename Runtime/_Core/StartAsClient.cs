using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
namespace ImxCoreSockets
{
    public class StartAsClient : MonoBehaviour
    {
        [Tooltip("Select start as a Server or Client")]
        [Header("Start as")]
        MainManager.SelectType starting;

        [SerializeField] ClientServerSelector clientServerSelector;

        [SerializeField] private string StreamingAssetFileName = "baseurl.txt";

        public string ipKey;
        public string ScreenName;

        public UnityEvent OnConnect;
        public UnityEvent OnConnectionFailed;

        public bool isConnected = false;
        public bool isReconnecting = false;
        public bool IsConnected
        {
            get => isConnected;
            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnConnect?.Invoke();
                }
            }
        }
        public bool IsReconnecting
        {
            get => isReconnecting;
            set
            {
                if (isReconnecting != value)
                {
                    isReconnecting = value;
                    OnConnectionFailed?.Invoke();
                }
            }
        }

        private void ClientConnected()
        {
            IsConnected = true;
            IsReconnecting = false;
        }

        private void ClientDisConnected()
        {
            IsConnected = false;
            IsReconnecting = true;
        }

        private void OnEnable()
        {
            TCP_ClientController.OnConnect += ClientConnected;
            TCP_ClientController.OnServerDisconnected += ClientDisConnected;
        }

        private void OnDestroy()
        {
            TCP_ClientController.OnConnect -= ClientConnected;
            TCP_ClientController.OnServerDisconnected -= ClientDisConnected;
        }

        private void Awake()
        {
            Seperate_Screen_Url();
        }

        public void Start()
        {
            ClientSelection();
        }

        private void ClientSelection()
        {
            starting = clientServerSelector.selectedType;
            if (starting == MainManager.SelectType.Client)
            {
                if (!string.IsNullOrEmpty(ipKey))
                {
                    Debug.Log("IP from base URL");
                    PlayerPrefs.SetString(nameof(ipKey), ipKey);
                    clientServerSelector.ProcessStart();
                }
                else
                {
                    Debug.LogError("Failed to load IP from file. Please check the file or provide a valid IP.");
                }
            }
        }

        private void Seperate_Screen_Url()
        {
            string gotData = GetBaseURL(StreamingAssetFileName);

            if (gotData.Contains("#"))
            {
                string[] splitData = gotData.Split("#");
                ipKey = splitData[0];
                ScreenName = splitData[1].ToUpper();
            }
            else
                ipKey = gotData;
            ScreenName = ScreenName.Trim();
        }

        public string GetBaseURL(string fileName)
        {
            string fullpath;
            fullpath = Path.Combine(Application.streamingAssetsPath, fileName);
            string dataToLoad = "";
            if (File.Exists(fullpath))
            {
                try
                {
                    using FileStream stream = new FileStream(fullpath, FileMode.Open);
                    using StreamReader reader = new StreamReader(stream);
                    dataToLoad = reader.ReadToEnd().Trim();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error Occured. Path not Exist " + fullpath + "\n" + e);
                }
            }
            Debug.Log("Base URL: " + dataToLoad);
            return dataToLoad;
        }
    }
}