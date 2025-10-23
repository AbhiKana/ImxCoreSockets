using UnityEngine;

namespace Server
{
    public class ClientServerSelector : MainManager
    {
        public override void ProcessStart()
        {
            base.ProcessStart();
        }

        public override void ClientSelected()
        {
            base.ClientSelected();
        }

        public override void ServerSelected()
        {
            base.ServerSelected();
        }

        public GameObject GetServerController()
        {
            return ServerController;
        }

        public GameObject GetClientController()
        {
            return ClientController;
        }

        /*private void OnOffObjects(GameObject type, GameObject panel)
        {
            Debug.Log("Object on off");
            if (panel != null && type != null)
            {
                panel.SetActive(false);
                type.SetActive(true);
            }
        }*/
    }
}