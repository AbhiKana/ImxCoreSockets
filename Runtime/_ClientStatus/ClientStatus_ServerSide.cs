using System.Collections.Generic;
using UnityEngine;

public class ClientStatus_ServerSide : MonoBehaviour
{
    TCP_ServerController tcpServerController;

    public List<string> allClients = new List<string>();

    public string tempConnectMsg = "", tempDisconnectMsg = "";

    public void _Initialize()
    {
        tcpServerController = GetComponent<TCP_ServerController>();
    }

    //Store IP in a list
    public void AddStringToList(string newString)
    {
        allClients.Add(newString);
        tempConnectMsg = newString;
    }
    //Remove IP from a list
    public void RemoveStringFromList(string newString)
    {
        string ip = tcpServerController.IPs;
        allClients.Remove(newString);
        Debug.Log("ALL CLIENT: " + allClients.Count);
        if (ip.Contains(newString))
        {
            ip = ip.Replace(newString, "");
        }
        tcpServerController.IPs = ip;
        tempDisconnectMsg = newString;
    }
}
