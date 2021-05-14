using System;
using System.Linq;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;
using TMPro;

public class Player : NetworkBehaviour
{

    private StartMenu StartMenuObject;
    public bool GameStarted = false;
    //private PlayerStruct PlayerStruct;


    public NetworkVariableString Nick = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkList<string> PlayersNicks = new NetworkList<string>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new List<string>());

    public NetworkDictionary<int, string> PlayersNicks2 = new NetworkDictionary<int, string>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new Dictionary<int, string>());

    public NetworkDictionary<int, ulong> PlayersNetworkId = new NetworkDictionary<int, ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new Dictionary<int, ulong>());

    public NetworkList<int> CardStack = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new List<int>());

     public NetworkList<int> PlayersCardsNumber = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new List<int>());

    public NetworkList<int> PlayerCardStack = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new List<int>());

    public NetworkList<int> PlayersInGame = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }, new List<int>());

    public NetworkVariableInt ActualPlayer = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableInt CardsToTake = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableInt LastUsedCard = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart()
    {
        if(IsLocalPlayer)
        {
            StartMenuObject = GameObject.Find("StartMenu").GetComponent<StartMenu>();

            SetNick(StartMenuObject.NickInput.GetComponent<TMP_InputField>().text);
        }
    }

    public void SetNick(string nick)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Nick.Value = nick;
        }
        else
        {
            SubmitNickServerRpc(nick);
        }
    }

    public List<string> GetPlayersNicks()
    {
        SyncNicksServerRpc();

        List<string> nicks = new List<string>();
        foreach(string nick in PlayersNicks)
        {
            nicks.Add(nick);
        }

        return nicks;
    }

    [ServerRpc]
    void SubmitNickServerRpc(string nick)
    {
        Nick.Value = nick;
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        GameStarted = true;
        if(IsLocalPlayer)
        {
            StartMenuObject.DisableStartMenu();
            StartMenuObject.EnableBoard();
        }
    }

    [ServerRpc]
    void SyncNicksServerRpc()
    {
        PlayersNicks.Clear();
        foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
        {
            var player = networkedClient.Value.PlayerObject.GetComponent<Player>();
            PlayersNicks.Add(player.Nick.Value);
        }
    }

    [ServerRpc]
    public void NextPlayerServerRpc(int cardsToTake = 1)
    {
        int tmp = ActualPlayer.Value;
        do{
            if(tmp == GetNumberOfPlayers() - 1)
            {
                tmp = 0;
            }
            else
            {
                tmp++;
            }
        }
        while(!PlayersInGame.Contains(tmp));

        foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
        {
            var player = networkedClient.Value.PlayerObject.GetComponent<Player>();
            player.ActualPlayer.Value = tmp;
            player.CardsToTake.Value = cardsToTake;
        }
    }

    [ServerRpc]
    public void DrawCardServerRpc(ulong playerId, int number = 1)
    {
        for(int i=0; i<number; i++)
        {
            var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>();

            if(CardStack[0] == (int)Card.Kitten)
            {
                Debug.Log("Wylosowano Kotka!");
                if(player.PlayerCardStack.Count(c => c == (int)Card.Defuse) >= 1)
                {
                    player.PlayerCardStack.RemoveAt(player.PlayerCardStack.IndexOf((int)Card.Defuse));

                    player.ShuffleCardStackServerRpc();
                    
                    SetLastUsedCardServerRpc((int)Card.Defuse);
                }
                else
                {
                    int playerNormalizedId = player.PlayersNetworkId.Single(r => r.Value == playerId).Key;
                    foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
                    {
                        var player1 = networkedClient.Value.PlayerObject.GetComponent<Player>();
                        player1.PlayersInGame.RemoveAt(player1.PlayersInGame.Single(r => r == playerNormalizedId));
                        player1.CardStack.RemoveAt(0);
                    }
                    SetLastUsedCardServerRpc((int)Card.Kitten);
                }
            }
            else
            {
                player.PlayerCardStack.Add(CardStack[0]);
            
                foreach (var networkedClient in NetworkManager.Singleton.ConnectedClients)
                {
                    var player1 = networkedClient.Value.PlayerObject.GetComponent<Player>();
                    player1.CardStack.RemoveAt(0);
                }
            }
        }
    }

    [ServerRpc]
    public void SyncCardNumberServerRpc()
    {
        foreach (var netClient in NetworkManager.Singleton.ConnectedClients)
        {
            var actualizedPlayer = netClient.Value.PlayerObject.GetComponent<Player>();
            actualizedPlayer.PlayersCardsNumber.Clear();
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var player = kvp.Value.PlayerObject.GetComponent<Player>();
                actualizedPlayer.PlayersCardsNumber.Add(player.PlayerCardStack.Count);
            }
        }
    }

    [ServerRpc]
    public void ShuffleCardStackServerRpc()
    {
        List<Card> tmpCardStack = new List<Card>();

        foreach(int card in CardStack)
        {
            tmpCardStack.Add((Card)card);
        }

        tmpCardStack = ShuffleCardStack(tmpCardStack);

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var player = kvp.Value.PlayerObject.GetComponent<Player>();
            player.CardStack.Clear();
        }

        foreach(Card card in tmpCardStack)
        {
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var player = kvp.Value.PlayerObject.GetComponent<Player>();
                player.CardStack.Add((int)card);
            }
        }
    }

    [ServerRpc]
    public void SetLastUsedCardServerRpc(int card)
    {
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var player = kvp.Value.PlayerObject.GetComponent<Player>();
            player.LastUsedCard.Value = card;
        }
    }

    [ServerRpc]
    public void RemoveCardFromUserServerRpc(ulong playerId, int card)
    {
        var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>();
        player.PlayerCardStack.RemoveAt(player.PlayerCardStack.IndexOf(card));
    }

    [ServerRpc]
    public void GetCardFromUserServerRpc(ulong playerId, ulong targetPlayerId, int card)
    {
        var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Player>();
        var targetPlayer = NetworkManager.Singleton.ConnectedClients[targetPlayerId].PlayerObject.GetComponent<Player>();

        if(targetPlayer.PlayerCardStack.Count(c => c == card) >= 1)
        {
            RemoveCardFromUserServerRpc(targetPlayerId, card);
            player.PlayerCardStack.Add(card);
        }
    }

    [ServerRpc]
    public void InitializeUsersServerRpc()
    {
        foreach (var netClient in NetworkManager.Singleton.ConnectedClients)
        {
            int i = 0;
            var playerToAdd = netClient.Value.PlayerObject.GetComponent<Player>();
            foreach (KeyValuePair<ulong, MLAPI.Connection.NetworkClient> kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var player = kvp.Value.PlayerObject.GetComponent<Player>();
                playerToAdd.PlayersNicks2.Add(i, player.Nick.Value);
                playerToAdd.PlayersNetworkId.Add(i, kvp.Key);
                playerToAdd.PlayersInGame.Add(i);
                player.CardsToTake.Value = 1;
                i++;
            }
        }
    }

    [ServerRpc]
    public void InitializeGameServerRpc()
    {
        List<Card> cardStack = new List<Card>();
        cardStack = AddCardToList(cardStack, Card.Attack, 4);
        cardStack = AddCardToList(cardStack, Card.Skip, 4);
        cardStack = AddCardToList(cardStack, Card.Favor, 4);
        cardStack = AddCardToList(cardStack, Card.Shuffle, 4);
        cardStack = AddCardToList(cardStack, Card.Future, 4);
        
        cardStack = ShuffleCardStack(cardStack);

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var player = kvp.Value.PlayerObject.GetComponent<Player>();
            for(int i=0; i<4; i++)
            {
                player.PlayerCardStack.Add((int)cardStack[0]);
                cardStack.RemoveAt(0);
            }
            player.PlayerCardStack.Add((int)Card.Defuse);
           
            player.ActualPlayer.Value = 0;
        }

        cardStack = AddCardToList(cardStack, Card.Kitten, GetNumberOfPlayers() - 1);
        if(GetNumberOfPlayers() == 2)
        {
            cardStack = AddCardToList(cardStack, Card.Defuse, 2);
        }
        else
        {
            cardStack = AddCardToList(cardStack, Card.Defuse, 6 - GetNumberOfPlayers());
        }
        
        cardStack = ShuffleCardStack(cardStack);

        foreach(Card card in cardStack)
        {
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                var player = kvp.Value.PlayerObject.GetComponent<Player>();
                player.CardStack.Add((int)card);
            }
        }
    }

    public int GetNumberOfPlayers()
    {
        return PlayersNicks2.Count;
    }

    private List<Card> ShuffleCardStack(List<Card> stack)
    {
        return stack.OrderBy(x => Guid.NewGuid()).ToList();
    }

    private List<Card> AddCardToList(List<Card> list, Card card, int num = 1)
    {
        for(int i=0; i<num; i++)
        {
            list.Add(card);
        }
        return list;
    }

}