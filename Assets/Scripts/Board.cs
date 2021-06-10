using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

    public GameObject FutureFirstCard;
    public GameObject FutureSecondCard;
    public GameObject FutureThirdCard;

    public GameObject LastUsedCard;

    public GameObject FavorFirstPlayer;
    public GameObject FavorSecondPlayer;
    public GameObject FavorThirdPlayer;

    public GameObject ActionPanel;

    private Player LocalPlayer = null;
    private List<GameObject> FavorPlayerButtons = new List<GameObject>();
    private List<int> ButtonToPlayer = new List<int>();
    private int FavorSelectedUser; 

    void Start()
    {
        FavorPlayerButtons.Add(FavorFirstPlayer);
        FavorPlayerButtons.Add(FavorSecondPlayer);
        FavorPlayerButtons.Add(FavorThirdPlayer);
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
            try
            {
                if(LocalPlayer.PlayersInGame.Count == 1)
                {
                    EndGamePanel.SetActive(true);
                    EndGamePanel.transform.Find("Winner").GetComponent<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[LocalPlayer.PlayersInGame[0]];
                }

                LocalPlayer.SyncCardNumberServerRpc();

                DefuseCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Defuse).ToString();
                AttackCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Attack).ToString();
                SkipCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Skip).ToString();
                FavorCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Favor).ToString();
                ShuffleCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Shuffle).ToString();
                FutureCount.GetComponent<TextMeshProUGUI>().text = CountCardTypeInStack(Card.Future).ToString();

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

                if(LocalPlayer.LastUsedCard.Value == 0)
                {
                    LastUsedCard.SetActive(false);
                }
                else
                {
                    LastUsedCard.SetActive(true);
                    LastUsedCard.GetComponent<RawImage>().texture = getCardSprite((Card)LocalPlayer.LastUsedCard.Value);
                }

            }
            catch(System.IndexOutOfRangeException ex) {}
            catch(System.ArgumentOutOfRangeException ex) {}
        }
    }

    public void TakeCard()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.DrawCardServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], LocalPlayer.CardsToTake.Value);
            LocalPlayer.NextPlayerServerRpc();
        }
    }

    private int CountCardTypeInStack(Card card)
    {
        return LocalPlayer.PlayerCardStack.Count(c => c == (int)card);
    }

    public void Attack_Card()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.PlayerCardStack.IndexOf((int)Card.Attack) != -1 && LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.SetLastUsedCardServerRpc((int)Card.Attack);
            LocalPlayer.NextPlayerServerRpc(2);
            LocalPlayer.RemoveCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], (int)Card.Attack);
        }
    }

    public void Skip_Card()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.PlayerCardStack.IndexOf((int)Card.Skip) != -1 && LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.SetLastUsedCardServerRpc((int)Card.Skip);
            LocalPlayer.NextPlayerServerRpc();
            LocalPlayer.RemoveCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], (int)Card.Skip);
        }
    }

    public void Shuffle_Card()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.PlayerCardStack.IndexOf((int)Card.Shuffle) != -1 && LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.SetLastUsedCardServerRpc((int)Card.Shuffle);
            LocalPlayer.ShuffleCardStackServerRpc();
            LocalPlayer.RemoveCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], (int)Card.Shuffle);
        }
    }

    public void Future_Card()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.PlayerCardStack.IndexOf((int)Card.Future) != -1 && LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.SetLastUsedCardServerRpc((int)Card.Future);
            FutureFirstCard.GetComponent<RawImage>().texture = getCardSprite((Card)LocalPlayer.CardStack[0]);
            FutureSecondCard.GetComponent<RawImage>().texture = getCardSprite((Card)LocalPlayer.CardStack[1]);
            FutureThirdCard.GetComponent<RawImage>().texture = getCardSprite((Card)LocalPlayer.CardStack[2]);

            LocalPlayer.RemoveCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], (int)Card.Future);
        }
    }

    public void Favor_Card()
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();
        if(LocalPlayer.PlayerCardStack.IndexOf((int)Card.Favor) != -1 && LocalPlayer.ActualPlayer.Value == requestorId)
        {
            LocalPlayer.SetLastUsedCardServerRpc((int)Card.Favor);
            int buttonNumber = 0;
            ButtonToPlayer.Clear();
            foreach (int playerId in LocalPlayer.PlayersInGame)
            {
                if(playerId != LocalPlayer.ActualPlayer.Value)
                {
                    FavorPlayerButtons[buttonNumber].SetActive(true);
                    FavorPlayerButtons[buttonNumber].GetComponentInChildren<TextMeshProUGUI>().text = LocalPlayer.PlayersNicks2[playerId];
                    ButtonToPlayer.Add(playerId);
                    buttonNumber++;
                }
            }

            LocalPlayer.RemoveCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], (int)Card.Favor);
        }
    }

    public void FavorSelectUser(int buttonNumber)
    {
        FavorSelectedUser = ButtonToPlayer[buttonNumber];
    }

    public void FavorSelectCard(int card)
    {
        LocalPlayer.GetCardFromUserServerRpc(LocalPlayer.PlayersNetworkId[LocalPlayer.ActualPlayer.Value], LocalPlayer.PlayersNetworkId[FavorSelectedUser], card);
    }

    public void ShowActionsPanel(int userId)
    {
        int requestorId = LocalPlayer.PlayersNicks2.Where(pair => pair.Value == LocalPlayer.Nick.Value)
                    .Select(pair => pair.Key)
                    .FirstOrDefault();

        if(requestorId == userId)
            ActionPanel.SetActive(true);

    }

    private Texture2D getCardSprite(Card card) {
        switch(card)
        {
            case Card.Kitten:
                return Resources.Load<Texture2D>("kitten-card");

            case Card.Defuse:
                return Resources.Load<Texture2D>("defuse-card");

            case Card.Attack:
                return Resources.Load<Texture2D>("attack-card");

            case Card.Skip:
                return Resources.Load<Texture2D>("skip-card");

            case Card.Favor:
                return Resources.Load<Texture2D>("favor-card");

            case Card.Shuffle:
                return Resources.Load<Texture2D>("shuffle-card");

            default:
                return Resources.Load<Texture2D>("future-card");
        }
    }
}
