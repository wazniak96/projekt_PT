using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class StartMenu : MonoBehaviour
{
    public void startHost()
    {
        NetworkManager.Singleton.StartHost();
        setNick();
    }

    public void startClient()
    {
        NetworkManager.Singleton.StartClient();
        setNick();
    }

    public void showNicks()
    {
        foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
        {
            var player = networkedClient.Value.PlayerObject.GetComponent<Player>();
            Debug.Log(player.Nick.Value);
        }
    }

    private void setNick()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
                    out var networkedClient))
            {
                var player = networkedClient.PlayerObject.GetComponent<Player>();
                var rand = new System.Random();
                if (player)
                {   
                    Debug.Log("Setting nick...");
                    player.setNick("test " + rand.Next());
                }
            }
    }

}
