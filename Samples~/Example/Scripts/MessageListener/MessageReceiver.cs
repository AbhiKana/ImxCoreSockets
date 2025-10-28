using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImxCoreSockets
{

    //this is the ideal format for Sending and Receiving 
    //MessageKey can be any key tag and Message Value should be the value for it. ex. "Key1":"true"
    [Serializable]
    public class MessageFormat
    {
        public string MessageKey;
        public string MessageValue;
    }

    public class MessageReceiver : MonoBehaviour
    {

        [SerializeField] ClientServerSelector selector;
        public MessageFormat recvMsg;

        private void OnEnable()
        {
            if (selector.selectedType == MainManager.SelectType.Server) TCP_ServerController.OnMessageReceived += ProcessMessage;
            else TCP_ClientController.OnMessageReceived += ProcessMessage;
        }

        private void OnDisable()
        {
            if (selector.selectedType == MainManager.SelectType.Server) TCP_ServerController.OnMessageReceived -= ProcessMessage;
            else TCP_ClientController.OnMessageReceived -= ProcessMessage;
        }


        /// <summary>
        /// create switch case below and data between client and server with message key 
        ///    switch (recvMsg.MessageKey)
        ///    {
        ///        case "tag 1":
        ///            DoSomething1(); break;
        ///        case "tag 2":
        ///            DoSomething2(); break;
        ///            .
        ///            .
        ///            .
        ///    }
        /// </summary>
        /// <param name="revmsg"></param>
        public void ProcessMessage(string revmsg)
        {
            if (!IsValidJson<MessageFormat>(revmsg))
                return;

            recvMsg = JsonUtility.FromJson<MessageFormat>(revmsg);

            // add switch case here

        }

        public bool IsValidJson<T>(string jsonString)
        {
            try
            {
                JsonUtility.FromJson<T>(jsonString);
                return true;
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning($"Invalid JSON: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unexpected error: {e.Message}");
                return false;
            }
        }

    }
}
