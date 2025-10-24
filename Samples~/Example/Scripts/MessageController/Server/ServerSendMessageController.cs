using UnityEngine;
using UnityEngine.UI;

namespace ImxCoreSockets
{
    public class ServerSendMessageController : MonoBehaviour
    {
        [SerializeField] ClientServerSelector clientServerSelector;
        MessageSpawnServer messageLoader;

        private void Start()
        {
            messageLoader = FindFirstObjectByType<MessageSpawnServer>();
        }

        //Call this funtion on button
        public void SendMessageToSelectedOne()
        {
            var noofClients = messageLoader.noofClients;
            var tcpServerController = clientServerSelector.GetServerController().GetComponent<TCP_ServerController>();
            for (int i = 0; i < noofClients.Count; i++)
            {
                var IsToggleStatus = noofClients[i].GetComponentInChildren<Toggle>();
                if (IsToggleStatus.isOn)
                {
                    ClientInfo info = tcpServerController._connectedClients[i];
                    tcpServerController.SendMessageFromServer(info.Client, "Hi, Server from message");
                }
            }
        }
        public void SendMessageFromClientToClient()
        {
            var allClients = clientServerSelector.GetServerController().GetComponent<ClientStatus_ServerSide>().allClients;
            var tcpServerController = clientServerSelector.GetServerController().GetComponent<TCP_ServerController>();

            for (int i = 0; i < allClients.Count; i++)
            {
                for (int j = 0; j < messageLoader.clientMessages.Count; j++)
                {
                    string ip = messageLoader.clientMessages[j].IP;
                    //ip = ip.Trim();
                    if (allClients[i] == ip)
                    {
                        Debug.Log("Message from client to client");
                        ClientInfo info = tcpServerController._connectedClients[i];
                        string msg = messageLoader.clientMessages[j].Message + " from " + messageLoader.clientMessages[j].SrNo;
                        tcpServerController.SendMessageFromServer(info.Client, msg);
                    }
                }
            }
        }

        public void SendJsonMessageToSelectedOne(string message)
        {
            var noofClients = messageLoader.noofClients;
            var tcpServerController = clientServerSelector.GetServerController().GetComponent<TCP_ServerController>();
            for (int i = 0; i < noofClients.Count; i++)
            {
                var IsToggleStatus = noofClients[i].GetComponentInChildren<Toggle>();
                if (IsToggleStatus.isOn)
                {
                    ClientInfo info = tcpServerController._connectedClients[i];
                    tcpServerController.SendMessageFromServer(info.Client, message);
                    Debug.Log("Message send to client: " + message);
                }
            }
        }
    }
}