using UnityEngine;
using UnityEngine.UI;

namespace ImxCoreSockets
{
    public class Client_SendMessageController : MonoBehaviour
    {
        [SerializeField] ClientServerSelector clientServerSelector;
        /*[SerializeField] */
        MessageSpawnClient messagingClient;


        private void Start()
        {
            messagingClient = FindFirstObjectByType<MessageSpawnClient>();
        }

        string IP_address;

        public void SendMessageToSelectedOne()
        {
            IP_address = string.Empty;
            bool allToggleOff = true;
            MessageList selectedMessage = new MessageList();

            if (messagingClient != null)
            {
                var noofClient = messagingClient.noofClients;
                for (int i = 0; i < noofClient.Count; i++)
                {
                    var IsToggleStatus = noofClient[i].GetComponentInChildren<Toggle>();

                    if (IsToggleStatus.isOn)
                    {
                        allToggleOff = false;
                        var IPtext = noofClient[i].GetComponentInChildren<Text>();
                        IP_address = IPtext.text;
                        IP_address = IP_address.Replace(" Connected", "");
                        //Debug.Log("<color=red> IP:::: </color>" + IP_address);

                        SaveJsonData(i, selectedMessage);
                    }
                }
            }

            if (allToggleOff)
            {
                int i = 0;
                SaveJsonData(i, selectedMessage);
            }
            string jsonArray = JsonUtility.ToJson(selectedMessage);
            Debug.Log("Json Data: " + jsonArray);
            clientServerSelector.GetClientController().GetComponent<TCP_ClientController>().SendMessage(jsonArray);
        }
        private void SaveJsonData(int srNo, MessageList messageList)
        {
            Client_MessageContainer messageDestination = new Client_MessageContainer();
            messageDestination.SrNo = srNo;
            messageDestination.IP = IP_address;
            if (messagingClient.sendMsg() != string.Empty && messagingClient != null)
            {
                messageDestination.Message = messagingClient.sendMsg();
            }
            else
            {
                messageDestination.Message = "Empty message";
            }
            messageList.messages.Add(messageDestination);
        }
    }
}
