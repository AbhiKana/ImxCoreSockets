using UnityEngine;
using System.Net.Sockets;
using System.Net;
using UnityEngine.Events;

namespace ImxCoreSockets
{
    public class StartAsServer : MonoBehaviour
    {
        [Tooltip("Select start as a Server or Client")]
        [Header("Start as")]
        MainManager.SelectType starting;

        [SerializeField] ClientServerSelector cliSelector;

        //Drop ServerIPCanvas Prefab into ServerManager Prefab
        //Change your Canvas UI according to your story board 
        //Reference the IP Address Text Gamobject into the IP address Panel varliable in this script through Unity Editor 
        [SerializeField] TMPro.TMP_Text ipAddress;

        public UnityEvent<bool> OnClientConnect;

        private void OnEnable()
        {
            TCP_ServerController.OnClientConnected += ClientStatus;
        }

        private void OnDisable()
        {
            TCP_ServerController.OnClientConnected -= ClientStatus;
        }

        public void ClientStatus(bool val, int count)
        {
            OnClientConnect?.Invoke(val);
            Debug.Log("<color=" + (count > 0 ? "green" : "red") + ">Connect Client: </color>" + val + " Count: " + count);

            ipAddress.text = "Client Connected: " + count;
            if (ipAddress != null && count == 0)
                ipAddress.text = GetIPAddress();
        }

        private void Start()
        {
            Initialize();
        }

        //Called on connect to CMS server event
        public void Initialize()
        {
            starting = cliSelector.selectedType;
            if (starting == MainManager.SelectType.Server)
            {
                if (ipAddress != null)
                    ipAddress.text = GetIPAddress();
                cliSelector.ProcessStart();
            }
        }

        public void OnServerConnect(bool value)
        {
            if (ipAddress != null)
                ipAddress.gameObject.SetActive(value);

            // To be added Events for more customizability
        }

        private string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
    }
}