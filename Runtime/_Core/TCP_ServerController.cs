using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ClientInfo
{
    public TcpClient Client { get; set; }
    public NetworkStream Stream { get; set; }
}

namespace ImxCoreSockets
{
    public class TCP_ServerController : MonoBehaviour
    {
        private TcpListener _listener;
        private Thread thread;
        public List<ClientInfo> _connectedClients = new List<ClientInfo>();
        ClientStatus_ServerSide _status;

        public string IPs = "";
        string MsgFromClient;
        public string messageReception;
        bool _IsMessageReceived;
        bool isRunning = true;


        public static UnityAction<string> OnMessageReceived;
        public static UnityAction<string, ClientInfo> OnclientInfoAssignment;
        public static UnityAction<bool, int> OnClientConnected;  //used for both connect and disconnect
        public static UnityAction<bool> OnServerStopped;

        public void _Initialize()
        {
            isRunning = true;
            Debug.Log("Server Started");
            _status = GetComponent<ClientStatus_ServerSide>();
            _status._Initialize();
            ServerStart();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessageToEveryOne("Check connection msg");
            }
        }

        public string GetMessage()
        {
            return MsgFromClient;
        }

        //Get your device IP 
        public string GetIPAddress()
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
            Debug.Log("My IP: " + localIP);
            return localIP;
        }

        //Start Server
        private void ServerStart()
        {
            thread = new Thread(new ThreadStart(ListeningOfAttemptedRequest));
            thread.IsBackground = true;
            thread.Start();
        }

        private void ListeningOfAttemptedRequest()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse(GetIPAddress()), 8052);
                _listener.Start();

                ThreadPool.QueueUserWorkItem(ListenerWorker, null);
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }

        //Server is listening, waiting for the client's request or message
        private void ListenerWorker(object token)
        {
            Debug.Log("Waiting for the clients to connect");
            try
            {
                while (_listener != null && isRunning)
                {
                    var client = _listener.AcceptTcpClient();
                    if (client.Connected)
                    {
                        string clientIP = client.Client.RemoteEndPoint.ToString();

                        bool alreadyConnected = false;
                        lock (_connectedClients)
                        {
                            alreadyConnected = _connectedClients.Any(c => c.Client.Client.RemoteEndPoint.ToString() == clientIP);
                        }

                        if (alreadyConnected)
                        {
                            Debug.Log($"Duplicate connection attempt from {clientIP}. Closing existing connection.");
                            // Close existing connection
                            var existingClient = _connectedClients.FirstOrDefault(c =>
                                c.Client.Client.RemoteEndPoint.ToString() == clientIP);
                            if (existingClient != null)
                            {
                                existingClient.Stream.Close();
                                existingClient.Client.Close();
                                _connectedClients.Remove(existingClient);
                            }
                        }

                        var clientInfo = new ClientInfo
                        {
                            Client = client,
                            Stream = client.GetStream()
                        };

                        lock (_connectedClients)
                        {
                            _connectedClients.Add(clientInfo);
                        }

                        Debug.Log($"{_connectedClients.Count} Client Connected: {clientIP}");
                        ThreadPool.QueueUserWorkItem(this.HandleClientsWorker, clientInfo);
                        _status.AddStringToList(clientIP);

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            OnClientConnected?.Invoke(true, _connectedClients.Count);
                        });
                    }
                    else
                    {
                        Debug.Log("TcpClient is not connected. Skipping...");
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                Debug.Log("Listener stopped gracefully");
            }
            catch (Exception ex)
            {
                Debug.Log($"Listener error: {ex}");
            }
        }

        //Handling multiple clients
        private void HandleClientsWorker(object token)
        {
            byte[] buffer = new byte[4096];
            var clientInfo = token as ClientInfo;
            var client = clientInfo.Client;
            var stream = clientInfo.Stream;

            try
            {
                while (client.Connected)
                {
                    string clientIP = client.Client.RemoteEndPoint.ToString();
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        _connectedClients.Remove(clientInfo);
                        Debug.Log($"Client disconnected: {client.Client.RemoteEndPoint} - Client IP: {clientIP}");
                        _status.RemoveStringFromList(clientIP);

                        // Dispatch to main thread
                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            //_IsMessageReceived = true;
                            OnClientConnected?.Invoke(false, _connectedClients.Count);
                        });
                        break;
                    }

                    var incommingData = new byte[bytesRead];
                    Array.Copy(buffer, 0, incommingData, 0, bytesRead);
                    string clientMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    messageReception = clientIP + ">>>";
                    MsgFromClient = clientMessage;
                    // Dispatch to main thread               
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        //_IsMessageReceived = true;
                        OnMessageReceived?.Invoke(clientMessage); // Your event here
                        OnclientInfoAssignment?.Invoke(clientMessage, clientInfo);
                    });
                    Debug.Log("Received from " + clientIP + ": " + clientMessage);
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log(socketException.ToString());
            }
        }

        //Send message to all Clients (Use this for General Purpose and below for Specific Purpose)
        public void SendMessageToEveryOne(string msg)
        {
            if (_connectedClients != null)
            {
                List<ClientInfo> clients = _connectedClients;
                for (int i = 0; i < clients.Count; i++)
                {
                    SendMessageFromServer(clients[i].Client, msg);
                }
            }
        }

        //Send message to anyone
        public void SendMessageFromServer(TcpClient client, string msg)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    if (stream.CanWrite)
                    {
                        Debug.Log("msg to send: " + msg);
                        byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(msg);
                        // Write byte array to socketConnection stream.            
                        stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                        Debug.Log("Server sent his message - should be received by client");
                    }
                }
                catch (SocketException socketException)
                {
                    Debug.Log("Socket exception: " + socketException);
                }
            }
            else
            {
                Debug.Log("Problem connectedTCPClient null");
                List<ClientInfo> clients = _connectedClients;
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].Client == client)
                    {
                        _connectedClients.Remove(clients[i]);
                    }

                }
            }
        }

        private void OnDestroy()
        {
            StopThreading();
        }

        private void OnApplicationQuit()
        {
            StopThreading();
        }

        public void StopThreading()
        {
            if (!isRunning) return;
            isRunning = false;
            try
            {
                //SendMessageToEveryOne("Server Disconnected");
                // Give clients time to process the disconnect message
                Thread.Sleep(500); // 500ms delay
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending disconnect message: {ex.Message}");
            }

            OnServerStopped?.Invoke(true);

            lock (_connectedClients)
            {
                foreach (var clientInfo in _connectedClients)
                {
                    try
                    {
                        if (clientInfo.Stream != null)
                        {
                            clientInfo.Stream.Close();
                        }
                        if (clientInfo.Client != null)
                        {
                            clientInfo.Client.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Error closing client: {ex.Message}");
                    }
                }
                _connectedClients.Clear();
            }

            // Stop the listener
            try
            {
                _listener?.Stop();
            }
            catch (Exception ex)
            {
                Debug.Log($"Listener stop error: {ex.Message}");
            }
            _listener = null;


            // Wait for thread to finish
            if (thread != null && thread.IsAlive)
            {
                if (!thread.Join(2000)) // Wait up to 1 second
                {
                    Debug.LogWarning("Listener thread did not terminate gracefully");
                }
            }
            Debug.Log("Server Stopped");
        }
        /*private void StopThread()
        {
            if (!isRunning)
            {
                //SendDisconnectMessageToEveryOne();
                Thread.Sleep(100);
                if (_listener != null)
                {
                    _listener.Stop();
                    Debug.Log("Listener stopped");
                }
            }
        }*/
    }

}