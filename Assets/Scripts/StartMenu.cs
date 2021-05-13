using System;
using System.Globalization;
using System.Security.AccessControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;

public class StartMenu : MonoBehaviour
{
    private float nextRefreshTime = 0.0f;
    public float refreshPeriod = 1.5f;

    private Player player = null;

    public GameObject waitMenu;
    public GameObject NicksPanel;
    public GameObject NickInput;
    public GameObject StartButton;
    public GameObject Board;

    private void Update() 
    {
        if(player == null)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
            out var networkedClient))
            {
                var tmp = networkedClient.PlayerObject.GetComponent<Player>();
                if (tmp)
                {
                    player = tmp;  
                }
            }
        }

        if(waitMenu.activeSelf && Time.time > nextRefreshTime && player)
        {
            nextRefreshTime = Time.time + refreshPeriod;
            
            List<string> nicks = player.GetPlayersNicks();
            for(int i=0; i<nicks.Count; i++)
            {
                TextMeshProUGUI nickBlock = NicksPanel.transform.Find("Nick_" + i).GetComponent<TextMeshProUGUI>();
                nickBlock.text = nicks[i];
            }
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        StartButton.GetComponent<Button>().interactable = false;
    }

    public void StartGame()
    {
        foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
        {
            var player = networkedClient.Value.PlayerObject.GetComponent<Player>();
            player.StartGameClientRpc();
        }
        player.InitializeUsersServerRpc();
        player.InitializeGameServerRpc();
    }   

    public void DisableStartMenu()
    {
        GameObject.Find("StartMenu").SetActive(false);
    }

    public void EnableBoard()
    {
        Board.SetActive(true);
    }
}
