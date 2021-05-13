using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using MLAPI;

public class Board : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject Player_0_Nick;
    public GameObject Player_1_Nick;
    public GameObject Player_2_Nick;
    public GameObject Player_3_Nick;

    public GameObject Player_0_CardNumber;
    public GameObject Player_1_CardNumber;
    public GameObject Player_2_CardNumber;
    public GameObject Player_3_CardNumber;

    public GameObject DefuseCount;
    public GameObject AttackCount;
    public GameObject SkipCount;
    public GameObject FavorCount;
    public GameObject ShuffleCount;
    public GameObject FutureCount;

    public GameObject ActualPlayerNick;
    public GameObject EndGamePanel;

    private Player LocalPlayer = null;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(LocalPlayer == null)
        {   
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
            out var networkedClient))
            {
                var tmp = networkedClient.PlayerObject.GetComponent<Player>();
                if (tmp)
                {
                    LocalPlayer = tmp;  
                }
            }
        }
        else
        {
            if(LocalPlayer.PlayersInGame.Count == 1)
            {
                EndGamePanel.SetActive(true);
                EndGamePanel.transform.Find("Winner").GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[LocalPlayer.PlayersInGame[0]];
            }

            LocalPlayer.SyncCardNumberServerRpc();

            DefuseCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Defuse);
            AttackCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Attack);
            SkipCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Skip);
            FavorCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Favor);
            ShuffleCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Shuffle);
            FutureCount.GetComponent<TextMeshProUGUI>().text = "Ilość: " + CountCardTypeInStack(Card.Future);

            if(LocalPlayer.PlayersNicks2.Count >= 2)
            {
                Player_0_Nick.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[0];
                Player_0_CardNumber.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersCardsNumber[0].ToString();
                Player_1_Nick.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[1];
                Player_1_CardNumber.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersCardsNumber[1].ToString();
                ActualPlayerNick.GetComponent<TextMeshProUGUI>().text = "Kolejka gracza: " + LocalPlayer.PlayersNicks2[LocalPlayer.ActualPlayer.Value];
            }
            if(LocalPlayer.PlayersNicks2.Count >= 3)
            {
                Player_2_Nick.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[2];
                Player_2_CardNumber.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersCardsNumber[2].ToString();
            }
            if(LocalPlayer.PlayersNicks2.Count >= 4)
            {
                Player_3_Nick.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[3];
                Player_3_CardNumber.GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersCardsNumber[3].ToString();
            }
        }
    }

    public void TakeCard()
    {
        LocalPlayer.DrawCardServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value]);
        LocalPlayer.NextPlayerServerRpc();
    }

    private int CountCardTypeInStack(Card card)
    {
        return LocalPlayer.PlayerCardStack.Count(c => c == (int)card);
    }

    public void showCards()
    {
        foreach(int cardd in LocalPlayer.CardStack)
        {
            Debug.Log((Card)cardd);
        }
    }
}
