using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImxCoreSockets
{
    public class ProcessMessageSync : MonoBehaviour
    {
        private void SyncVarToClients(string variableID)
        {
            // This would be on the server
            var field = this.GetType().GetField(variableID, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null) return;

            object value = field.GetValue(this);
            byte[] payload;
            if (value is int i) payload = BitConverter.GetBytes(i);
            else return; // Add other types

            var message = new ProcessMessage
            {
                Type = ProcessMessage.MessageType.ProcessVariable,
                ID = variableID,
                Payload = payload
            };

            // Find the server instance and tell it to send the message
            //FindObjectOfType<TCP_ServerController>()?.SendMessageToEveryOne(message.Serialize());
        }

        private void SendRpcToServer(string functionID, params object[] parameters)
        {
            // This would be on the client
            byte[] payload;
            if (parameters[0] is int i) payload = BitConverter.GetBytes(i);
            else return;

            var message = new ProcessMessage
            {
                Type = ProcessMessage.MessageType.ProcessMethod,
                ID = functionID,
                Payload = payload
            };

            // Find the client instance and tell it to send the message
            //FindObjectOfType<TCP_ClientController>()?.SendMessage(message.Serialize());
        }
    }
}
