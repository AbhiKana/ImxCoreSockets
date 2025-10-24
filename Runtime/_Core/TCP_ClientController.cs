using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace ImxCoreSockets
{
    public class TCP_ClientController : MonoBehaviour
    {
        public TcpClient tcpClient;
        private Thread clientThread;
        private NetworkStream stream;

        private StartAsClient startAs;
        private ConnectViaInput connectViaInput;
        private ClientStatus_ClientSide _status;

        private int pollInterval = 2;
        public bool currentPollState;
        public bool previousPollState;

        [SerializeField] private bool isRunning = false;
        [SerializeField] private bool isConnected = false;
        [SerializeField] private bool shouldReconnect = true;
        [SerializeField] private bool isQuitting = false;
        private bool checkMaxAttempts;
        public bool ByPassMaxAttempts
        {
            get { return checkMaxAttempts; }
            set { checkMaxAttempts = value; }
        }


        public bool _IsMessageReceived;

        [Header("Client Settings")]
        public string serverIP = ""; //Got from base url or input field
        private int serverPort = 8052;
        public string myIP;
        public float reconnectInterval = 5f;
        public float maxReconnectAttempts = 5;
        public float connectionTimeout = 3f;

        private Queue<string> messageQueue = new Queue<string>();
        private object queueLock = new object();

        public static UnityAction OnConnect;
        public static UnityAction OnServerDisconnected;

        public static event Action<string> OnMessageReceived;

        private void OnEnable()
        {
            OnMessageReceived += AttemptReconnection;
        }

        private void OnDisable()
        {
            OnMessageReceived -= AttemptReconnection;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessage("Check connection");
            }

            // Process any received messages on the main thread
            lock (queueLock)
            {
                while (messageQueue.Count > 0)
                {
                    string message = messageQueue.Dequeue();
                    ProcessMessage(message);
                }
            }
        }

        public void _Initialze()
        {
            Debug.Log("Client Initialize");
            _status = GetComponent<ClientStatus_ClientSide>();
            startAs = FindFirstObjectByType<StartAsClient>();
            connectViaInput = FindFirstObjectByType<ConnectViaInput>();
            checkMaxAttempts = connectViaInput != null ? connectViaInput.CheckMaxAttempts : false;
            serverIP = connectViaInput?.ipKey ?? startAs?.ipKey;
            StartClient();
            StartPolling();
        }

        private void AttemptReconnection(string msg)
        {
            if (msg.Contains("Server Disconnected"))
            {
                Debug.Log("Stop Client");
                StopClient();
                Invoke(nameof(StartClient), 1f);
            }
        }

        public void StartClient()
        {
            myIP = GetIPAddress();
            if (isRunning)
            {
                Debug.Log("Client is already running.");
                return;
            }

            isRunning = true;
            shouldReconnect = true;
            isQuitting = false;
            ConnectToServer();
            Debug.Log("Starting Client...");
        }

        //Connect to server
        private void ConnectToServer()
        {
            serverIP = connectViaInput?.ipKey ?? startAs?.ipKey;

            if (!string.IsNullOrEmpty(serverIP))
            {
                try
                {
                    clientThread = new Thread(new ThreadStart(AttemptConnection));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError("Client thread start exception: " + e);
                }
            }
            else
            {
                Debug.LogWarning("IP is null or connection failed.");
            }
        }

        //Start Polling
        private void StartPolling()
        {
            StartCoroutine(PollingLoopback());
        }

        IEnumerator PollingLoopback()
        {
            while (true)
            {
                if (isConnected)
                {
                    SendMessage("Poll");
                    if (currentPollState != previousPollState)
                    {
                        previousPollState = currentPollState;
                        Debug.Log(currentPollState ? "Network connected." : "Network disconnected.");

                        if (!currentPollState)
                        {
                            UnityMainThreadDispatcher.Enqueue(() =>
                            {
                                OnMessageReceived?.Invoke("Server Disconnected");
                                //UIManager.uiManagerInstance.CheckImageOrvideo(" ");
                            });
                        }
                    }
                }

                yield return new WaitForSeconds(pollInterval);
            }
        }

        //Send request to server to connect
        private void AttemptConnection()
        {
            int reconnectAttempts = 0;
            while (isRunning && !isQuitting)
            {
                if (!isConnected && shouldReconnect)
                {
                    if (checkMaxAttempts)
                        if (reconnectAttempts >= maxReconnectAttempts)
                        {
                            Debug.LogError("Max reconnection attempts reached. Stopping reconnection attempts.");
                            StopClient();
                            break;
                        }


                    Debug.Log("Attempting to connect to server... (" + (reconnectAttempts + 1) + ")");

                    try
                    {
                        // Try to connect with timeout
                        tcpClient = new TcpClient();
                        var result = tcpClient.BeginConnect(serverIP, serverPort, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(System.TimeSpan.FromSeconds(connectionTimeout));

                        if (!success)
                        {
                            throw new System.Exception("Connection timeout");
                        }

                        tcpClient.EndConnect(result);
                        stream = tcpClient.GetStream();
                        isConnected = true;
                        reconnectAttempts = 0;

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            Debug.Log("Connected to server");
                            OnConnect?.Invoke();
                        });


                        // Start receiving messages
                        ReceiveMessages();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Connection failed: " + e.Message);
                        isConnected = false;
                        reconnectAttempts++;

                        // Clean up
                        if (tcpClient != null)
                        {
                            tcpClient.Close();
                        }

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            OnServerDisconnected?.Invoke();
                        });
                        // Wait before trying to reconnect
                        Thread.Sleep((int)(reconnectInterval * 1000));
                    }
                }
                else if (isConnected)
                {
                    // Check if still connected
                    if (!IsConnected())
                    {
                        isConnected = false;
                        Debug.LogWarning("Disconnected from server");

                        // Clean up
                        if (stream != null)
                        {
                            stream.Close();
                        }

                        if (tcpClient != null)
                        {
                            tcpClient.Close();
                        }

                        // Wait before trying to reconnect
                        Thread.Sleep((int)(reconnectInterval * 1000));
                    }
                }
            }
        }

        private void ReceiveMessages()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            try
            {
                while (isConnected && isRunning && !isQuitting)
                {
                    bytesRead = 0;

                    try
                    {
                        if (stream.DataAvailable)
                        {
                            bytesRead = stream.Read(message, 0, message.Length);

                            if (bytesRead == 0)
                            {
                                // Server disconnected
                                isConnected = false;
                                break;
                            }

                            string serverMessage = System.Text.Encoding.ASCII.GetString(message, 0, bytesRead);

                            lock (queueLock)
                            {
                                messageQueue.Enqueue(serverMessage);
                            }
                        }
                        else
                        {
                            // No data available
                            Thread.Sleep(10);
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        isConnected = false;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving message: " + e.Message);
                isConnected = false;
            }
        }

        bool IsConnected()
        {
            try
            {
                if (tcpClient == null || !tcpClient.Connected)
                    return false;


                // Check if the socket is still connected
                if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Server disconnected
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ProcessMessage(string message)
        {
            // Process received messages on the main thread
            Debug.Log("Received: " + message);

            // Add your message processing logic here
            OnMessageReceived?.Invoke(message);
        }

        //Send message to server
        public new void SendMessage(string msg)
        {
            if (tcpClient == null)
            {
                return;
            }
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                if (networkStream.CanWrite)
                {
                    currentPollState = true;
                    //Debug.Log("CLIENT MSG " + clientMessage);
                    byte[] buffer = Encoding.ASCII.GetBytes(msg);
                    networkStream.Write(buffer, 0, buffer.Length);
                    Debug.Log("Client sent his message - should be received by Server: " + msg);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                currentPollState = false;
            }
        }

        //Get your device IP
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

        //Use Anywhere with reference
        public string GetMessage()
        {
            return "Dummy String";
            //return MsgFromServer;
        }

        //Close TCP connections
        public void StopClient()
        {
            if (!isRunning) return;

            isRunning = false;
            shouldReconnect = false;
            isQuitting = true;

            if (stream != null)
                stream.Close();

            if (tcpClient != null)
                tcpClient.Close();

            if (clientThread != null && clientThread.IsAlive)
                clientThread.Join(500);
            Debug.Log("Client socket connection closed");
        }

        private void OnDestroy()
        {
            //if (!isQuitting)
            //    SendMessage("Disconnect:" + startAs.ScreenName);
            StopClient();
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
            //SendMessage("Disconnect:" + startAs.ScreenName);
            StopClient();
        }
    }
}

