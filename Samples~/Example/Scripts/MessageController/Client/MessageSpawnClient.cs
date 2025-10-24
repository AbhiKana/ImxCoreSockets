using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace ImxCoreSockets
{
    public class MessageSpawnClient : MonoBehaviour
    {
        [SerializeField] ClientServerSelector canvasController;
        //[SerializeField] LoadFileJsonData loadImageJsonData;

        [Header("Client Status_client")]
        [SerializeField] GameObject prefab;
        [SerializeField] GameObject targetPosition;

        [Header("Server Messages")]
        [SerializeField] GameObject serverScrollViewContent;
        [SerializeField] GameObject messageBox;
        [SerializeField] TMP_InputField inputField;

        GameObject clientController;
        ClientStatus_ClientSide clientSide;
        TCP_ClientController tcp_ClientController;

        //[SerializeField] GameObject DialogBox;
        public List<GameObject> noofClients = new List<GameObject>();
        HashSet<string> uniqueMessage = new HashSet<string>();
        public List<string> messages = new List<string>();

        [SerializeField] string tempMessage = "";
        public void Initialize()
        {
            InitialzeClientType();
            //DialogBox = GameObject.Find("Canvas").transform.Find("DialogBox").gameObject;
        }
        private void InitialzeClientType()
        {
            clientController = canvasController.GetClientController();
            if (clientController != null)
            {
                clientSide = clientController.GetComponent<ClientStatus_ClientSide>();
                tcp_ClientController = clientController.GetComponent<TCP_ClientController>();
            }
            else
            {
                Debug.LogError("ServerController is null.");
            }
        }
        private void Update()
        {
            if (clientSide != null)
            {
                /*if(!string.IsNullOrEmpty(tcp_ClientController.socketException))
                {
                    //if(DialogBox != null)
                    //    DialogBox.SetActive(true);
                    tcp_ClientController.socketException = string.Empty;
                }*/
                StoreTempMessage();
                ShowClientStatus();
            }
        }
        void StoreTempMessage()
        {
            //This condition won't show multiple clients in a same system. It only works on different systems for multiple clients and a server
            if (!string.IsNullOrEmpty(tcp_ClientController.GetMessage()))
            {
                tempMessage = tcp_ClientController.GetMessage();
                Debug.Log("TEMP Message " + tempMessage);
            }
        }
        void ShowClientStatus()
        {
            if (!string.IsNullOrEmpty(tempMessage))
            {
                if (IsIP_Address(tempMessage) && !tempMessage.Contains("http"))
                {
                    RemoveAllClients();
                    RemoveMyIP(ref tempMessage);
                    AddClient(tempMessage, prefab, targetPosition);
                }
                else
                {
                    if (IsFileContain(tempMessage))
                    {
                        //loadImageJsonData.SelectFileType(tempMessage);
                        //InstantiateServerMessage(tempMessage, messageBox, serverScrollViewContent);
                    }
                    else
                    {
                        InstantiateServerMessage(tempMessage, messageBox, serverScrollViewContent);
                    }
                }
                tempMessage = "";
            }
        }
        private void RemoveMyIP(ref string message)
        {
            string[] Ips = message.Split('\n');
            message = "";
            for (int i = 0; i <= Ips.Length - 1; i++)
            {
                string ipWithPort = Ips[i];
                string[] parts = ipWithPort.Split(':');
                if (parts.Length == 2 && parts[0] == tcp_ClientController.myIP)
                {
                    Ips[i] = Ips[i].Replace(Ips[i], "");
                }
            }

            for (int i = 0; i <= Ips.Length - 1; i++)
            {
                message += Ips[i];
            }
        }
        public void AddClient(string Message, GameObject prefab, GameObject targetPosition)
        {
            string[] multipleIPs = Message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string ip in multipleIPs)
            {
                //Debug.Log("<color=red>IP: </color>" + ip);
                if (!uniqueMessage.Contains(ip))
                {
                    Debug.Log("ConnectedClientList");
                    messages.Add(ip);
                    uniqueMessage.Add(ip);
                    Debug.Log("Add client");
                    InstantiateServerMessage(ip, prefab, targetPosition);
                }
            }

        }
        public void RemoveAllClients()
        {
            foreach (GameObject clientObject in noofClients)
            {
                Destroy(clientObject);
            }
            noofClients.Clear();
            messages.Clear();
            uniqueMessage.Clear();
        }
        void InstantiateServerMessage(string message, GameObject prefab, GameObject targetPosition)
        {
            GameObject g = Instantiate(prefab, targetPosition.transform);
            g.transform.GetComponentInChildren<Text>().text = message;
            //RemoveAllClients();
            if (IsIP_Address(message))
                noofClients.Add(g);
        }
        public string sendMsg()
        {
            return inputField.text;
        }
        bool IsIP_Address(string ip)
        {
            //reqular expression pattern to match IP 
            //  @"([0-9]{1,3})[.]([0-9]{1,3})[.]([0-9]{1,3})[.]([0-9]{1,3})[:]([0-9]{4})"
            string ipAddressPattern = @"\d{1,3}\.\d{1,3}\.\d{1,1}\.\d{1,3}:\d{1,5}";
            return Regex.IsMatch(ip, ipAddressPattern);
        }
        bool IsFileContain(string message)
        {
            string pattern = @"\.(jpg|pdf|mp4|pptx)\b";
            return Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase);
        }
    }
}
