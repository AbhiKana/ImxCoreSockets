using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Server_MessageContainer
{
    public int SrNo;
    public string IP;
    public string Message;
}

[System.Serializable]
public class MessagesList
{
    public List<Server_MessageContainer> messages;
}

namespace ImxCoreSockets
{
    public class MessageSpawnServer : MonoBehaviour
    {
        [SerializeField] ClientServerSelector canvasController;
        [SerializeField] ServerSendMessageController serverSendMessageController;

        [Header("Client Status_server")]
        [SerializeField] GameObject s_prefab;
        [SerializeField] GameObject s_targetPosition;

        [Header("Client Messages")]
        [SerializeField] GameObject serverScrollViewContent;
        [SerializeField] TextMeshProUGUI textMeshPro;

        GameObject serverController;
        ClientStatus_ServerSide serverSide;
        TCP_ServerController tcp_ServerController;

        [SerializeField] string tempMessage;
        [SerializeField] Text clientCount;

        public List<GameObject> noofClients = new List<GameObject>();
        public List<Server_MessageContainer> clientMessages;

        bool IsIPMessage = false;
        public void Initialize()
        {
            InitialzeServerType();
        }
        private void InitialzeServerType()
        {
            serverController = canvasController.GetServerController();
            if (serverController != null)
            {
                serverSide = serverController.GetComponent<ClientStatus_ServerSide>();
                tcp_ServerController = serverController.GetComponent<TCP_ServerController>();
            }
            else
            {
                Debug.LogError("ServerController is null.");
            }
        }
        private void Update()
        {
            if (serverSide != null)
            {
                //Do some stuff after receiving message
                ShowClientStatus(ref serverSide.tempConnectMsg, ref serverSide.tempDisconnectMsg);
                StoreTempMessage();
                ShowMessage();
            }
        }

        void StoreTempMessage()
        {
            if (!string.IsNullOrEmpty(tcp_ServerController.GetMessage()))
            {
                tempMessage = tcp_ServerController.GetMessage();
            }
        }

        private void ShowClientStatus(ref string tempConn, ref string tempDisConn)
        {
            if (!string.IsNullOrEmpty(tempConn))
            {
                Debug.Log("TEMP " + tempConn);
                AddClient(tempConn, s_prefab, s_targetPosition);
                tempConn = string.Empty;
            }

            if (!string.IsNullOrEmpty(tempDisConn))
            {
                Debug.Log("TEMP DIS " + tempDisConn);
                RemoveClient(tempDisConn);
                tempDisConn = string.Empty;
            }

            if (clientCount != null)
                clientCount.text = "No of Client: " + noofClients.Count;
        }

        private void ShowMessage()
        {
            if (!string.IsNullOrEmpty(tempMessage))
            {
                clientMessages.Clear();
                LoadSingleData(tempMessage);
                for (int i = 0; i < clientMessages.Count; i++)
                {
                    if (string.IsNullOrEmpty(clientMessages[i].IP))
                    {
                        Debug.Log("IP is Empty");
                        InstantiateMessageInScrollView(tcp_ServerController.messageReception + " " + clientMessages[i].Message);
                    }
                    else
                    {
                        Debug.Log("IP is not Empty");
                        IsIPMessage = true;
                    }
                }
                if (IsIPMessage)
                {
                    Debug.Log("IP is not Empty 1");
                    serverSendMessageController.SendMessageFromClientToClient();
                    InstantiateMessageInScrollView(tcp_ServerController.messageReception + " " + tempMessage);
                    IsIPMessage = false;
                }
                //Invoke("C2CMessage", 1f);
                //serverSendMessageController.SendMessageFromClientToClient();
                tempMessage = "";
            }
        }

        public void InstantiateMessageInScrollView(string Message)
        {
            string m = Message.ToString() + "\n";
            textMeshPro.text += m + "\n";
        }
        public void LoadSingleData(string jsonData)
        {
            Debug.Log("Data Loaded");
            MessagesList list = JsonUtility.FromJson<MessagesList>(jsonData);

            foreach (Server_MessageContainer message in list.messages)
            {
                clientMessages.Add(message);
                Debug.Log("SrNo: " + message.SrNo + ", IP: " + message.IP + ", Message: " + message.Message);
            }
        }
        public void AddClient(string message, GameObject prefab, GameObject taretPosition)
        {
            Debug.Log("Instantiate");
            GameObject g = Instantiate(prefab, taretPosition.transform);
            g.GetComponentInChildren<Text>().text = message + " Connected";
            noofClients.Add(g);

        }
        public void RemoveClient(string clientIP)
        {
            GameObject clientToRemove = noofClients.Find(obj => obj.GetComponentInChildren<Text>().text.StartsWith(clientIP));
            if (clientToRemove != null)
            {
                noofClients.Remove(clientToRemove);
                Destroy(clientToRemove);
            }
        }
    }
}