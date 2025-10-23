using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Server
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

        public virtual void ClientSelected()
        {
            //SetUpClient();
        }

        private void SetUpServer()
        {
            Debug.Log("Set up server");
            ServerController = new GameObject("ServerController");
            ServerController.transform.SetParent(transform);
            ServerController.AddComponent<TCP_ServerController>();
            ServerController.AddComponent<ClientStatus_ServerSide>();
            ServerController.GetComponent<TCP_ServerController>()._Initialize();
        }

        public ClientStatus_ServerSide Get_ServerSide_ClientStatus()
        {
            return ServerController.GetComponent<ClientStatus_ServerSide>();
        }
    }
}