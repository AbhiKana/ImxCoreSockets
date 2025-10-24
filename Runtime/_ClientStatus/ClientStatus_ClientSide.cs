using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ImxCoreSockets
{
    public class ClientStatus_ClientSide : MonoBehaviour
    {
        public List<string> messages = new List<string>();
        HashSet<string> uniqueMessage = new HashSet<string>();

        public string tempConnectMsg = "", tempDisconnectMsg = "";

        //Store connected client in a list
        public void ConnectedClientList(string message)
        {
            string[] multipleIPs = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ip in multipleIPs)
            {
                if (!uniqueMessage.Contains(ip) && IsIP_Address(ip))
                {
                    messages.Add(ip);
                    uniqueMessage.Add(ip);
                }
            }
        }
        bool IsIP_Address(string ip)
        {
            //reqular expression pattern to match IP
            string ipAddressPattern = @"\b(?:\d{1,3}\.){3}\d{1,3}:\d+\b";
            return Regex.IsMatch(ip, ipAddressPattern);
        }

        //Remove all clients
        public void RemoveAllClients()
        {
            messages.Clear();
            uniqueMessage.Clear();
        }

        //Store IP in a list
        public void AddStringToList(string newString)
        {
            messages.Add(newString);
            tempConnectMsg = newString;
        }
    }
}