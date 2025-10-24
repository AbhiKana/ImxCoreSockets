using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ImxCoreSockets
{
    public class MainManager : MonoBehaviour
    {
        public enum SelectType { Server, Client };
        public SelectType selectedType;

        protected GameObject ServerController;
        protected GameObject ClientController;

        [Space(10)]
        [SerializeField] bool IsAutoStart;

        public virtual void ProcessStart()
        {
            if (IsAutoStart)
                GetSelectType();
        }

        //If not auto start, call this method
        public void StartApp()
        {
            GetSelectType();
        }
        protected SelectType GetSelectType()
        {
            switch (selectedType)
            {
                case SelectType.Server:
                    Debug.Log("Server selected");
                    ServerSelected();
                    break;
                case SelectType.Client:
                    Debug.Log("Client selected");
                    ClientSelected();
                    break;
            }
            return selectedType;
        }

        public virtual void ServerSelected()
        {
            SetUpServer();
        }
        private void SetUpServer()
        {
            Debug.Log("Set up server");
            GameObject temp = GameObject.Find("ServerController");
            if (temp == null)
            {
                ServerController = new GameObject("ServerController");
                ServerController.transform.SetParent(transform);
                ServerController.AddComponent<TCP_ServerController>();
                ServerController.AddComponent<ClientStatus_ServerSide>();
                ServerController.GetComponent<TCP_ServerController>()._Initialize();
            }
            else
            {
                ServerController.GetComponent<TCP_ServerController>()._Initialize();
            }
        }

        public virtual void ClientSelected()
        {
            SetUpClient();
        }

        private void SetUpClient()
        {
            Debug.Log("Set up client");
            GameObject temp = GameObject.Find("ClientController");
            if (temp == null)
            {
                ClientController = new GameObject("ClientController");
                ClientController.transform.SetParent(transform);
                ClientController.AddComponent<TCP_ClientController>();
                ClientController.AddComponent<ClientStatus_ClientSide>();
                ClientController.GetComponent<TCP_ClientController>()._Initialze();
            }
            else
            {
                ClientController.GetComponent<TCP_ClientController>()._Initialze();
            }
        }

        public ClientStatus_ServerSide Get_ServerSide_ClientStatus()
        {
            return ServerController.GetComponent<ClientStatus_ServerSide>();
        }

        public ClientStatus_ClientSide Get_ClientSide_ClientStatus()
        {
            return ClientController.GetComponent<ClientStatus_ClientSide>();
        }

        public TCP_ServerController Get_ServerController()
        {
            return ServerController.GetComponent<TCP_ServerController>();
        }

        public TCP_ClientController Get_ClientController()
        {
            return ClientController.GetComponent<TCP_ClientController>();
        }
    }
}