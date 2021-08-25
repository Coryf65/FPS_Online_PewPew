using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using System.Linq;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // singleton
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        ChangeStats
    }

    [Tooltip("Main Menu Scene index on Build Settings.")]
    public int mainMenu = 0;
    [Header("Game State")]
    public string butt = "";
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();

    private int index;
    private List<Leaderboard> playersLeaderboard = new List<Leaderboard>();

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(mainMenu);
        } else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        // Our Codes
        if (photonEvent.Code < 200)
        {
            EventCodes eventCode = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.ChangeStats:
                    UpdateStatsReceive(data);
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Send a New Player Event, creating the new player
    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];

        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0; // kills
        package[3] = 0; // deaths

        // send data to network and master client
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true });
    }

    // Receive a New Player Event
    public void NewPlayerReceive(object[] data)
    {
        PlayerInfo player = new PlayerInfo(data[0].ToString(), (int)data[1], (int)data[2], (int)data[3]);

        allPlayers.Add(player);

        ListPlayersSend();
    }

    private void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] peice = new object[4];
            peice[0] = allPlayers[i].Name;
            peice[1] = allPlayers[i].ActorID;
            peice[2] = allPlayers[i].Kills;
            peice[3] = allPlayers[i].Deaths;

            package[i] = peice;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });
    }

    private void ListPlayersReceive(object[] data)
    {
        allPlayers.Clear();

        for (int i = 0; i < data.Length; i++)
        {
            object[] peice = (object[])data[i];

            PlayerInfo player = new PlayerInfo(
                (string)peice[0],
                (int)peice[1],
                (int)peice[2],
                (int)peice[3]);

            allPlayers.Add(player);

            // extra check
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.ActorID)
            {
                // this is our player, store that
                index = i;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actorSending">Player whom Sent</param>
    /// <param name="statToUpdate">Kill or Death</param>
    /// <param name="amountToChange">How much to add</param>
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[]
        {
            actorSending,
            statToUpdate,
            amountToChange
        };

        // Raise the Event
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStats,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });

    }

    private void UpdateStatsReceive(object[] data)
    {
        int actor = (int)data[0];
        int statType = (int)data[1];
        int amount = (int)data[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].ActorID == actor)
            {
                // which type
                switch (statType)
                {
                    case 0:
                        // Kill
                        allPlayers[i].Kills += amount;
                        break;
                    case 1:
                        // death
                        allPlayers[i].Deaths += amount;
                        break;
                }

                if (i == index)
                {
                    UpdateScoreBoard();
                }

                if (UIController.instance.leaderboard.activeInHierarchy)
                {
                    UpdateLeaderboard();
                }

                break; // we found the player to update we are done
            }
        }
    }

    public void UpdateScoreBoard()
    {
        if (allPlayers.Count > index)
        {
            UIController.instance.killsText.text = $"Kills: {allPlayers[index].Kills}";
            UIController.instance.deathsText.text = $"Deaths: {allPlayers[index].Deaths}";
        } else
        {
            UIController.instance.killsText.text = $"Kills: 0";
            UIController.instance.deathsText.text = $"Deaths: 0";
        }        
    }

    public void UpdateLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);

        foreach (Leaderboard lp in playersLeaderboard)
        {
            Destroy(lp);
        }
        playersLeaderboard.Clear();

        UIController.instance.playerDisplay.gameObject.SetActive(false);
        List<PlayerInfo> sortedPlayers = SortPlayersByKills(allPlayers);

        // display all players
        foreach (PlayerInfo playerInfo in sortedPlayers)
        {
            // create new player
            Leaderboard newPlayerDisplay = Instantiate(UIController.instance.playerDisplay, UIController.instance.playerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(playerInfo.Name, playerInfo.Kills, playerInfo.Deaths);
            newPlayerDisplay.gameObject.SetActive(true);

            playersLeaderboard.Add(newPlayerDisplay);
        }
    }

    /// <summary>
    /// Sort a List by Kills
    /// </summary>
    /// <param name="playerList"></param>
    /// <returns></returns>
    private List<PlayerInfo> SortPlayersByKills(List<PlayerInfo> playerList)
    {
        List<PlayerInfo> sortedPlayers = new List<PlayerInfo>();

        sortedPlayers = playerList.OrderBy(p => p.Kills).ToList();

        return sortedPlayers;
    }
}
