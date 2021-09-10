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
        ChangeStats,
        NextRound,
        TimerSync
    }

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    [Tooltip("Main Menu Scene index on Build Settings.")]
    public int mainMenu = 0;
    [Header("Game State")]
    public GameState currentState = GameState.Waiting;
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    [Header("Move to another Class? or SO")]
    public int killsToWin = 3;
    public Transform mapCameraPoint;
    public float waitTimeAfterRound = 5f;
    public bool redoRound;
    public float roundLength = 300f;

    private float currentRoundTimer;
    private float sendTimer;
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
            currentState = GameState.Playing;
            UIController.instance.TogglePlayerUI(true);
            SetupTimer();
            
            if (!PhotonNetwork.IsMasterClient)
            {
                UIController.instance.roundCountdownText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentRoundTimer > 0 && currentState == GameState.Playing)
            {
                currentRoundTimer -= Time.deltaTime;

                if (currentRoundTimer <= 0)
                {
                    currentRoundTimer = 0;
                    currentState = GameState.Ending;

                    ListPlayersSend();
                    StateCheck();
                }

                UpdateRoundTimerDisplay();
                sendTimer -= Time.deltaTime;

                if (sendTimer <= 0)
                {
                    sendTimer += 1f;
                    TimerSend();
                }                
            }
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
                case EventCodes.NextRound:
                    NextRoundReceive();
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
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
        object[] package = new object[allPlayers.Count + 1];

        package[0] = currentState;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] peice = new object[4];
            peice[0] = allPlayers[i].Name;
            peice[1] = allPlayers[i].ActorID;
            peice[2] = allPlayers[i].Kills;
            peice[3] = allPlayers[i].Deaths;

            // weird ?
            package[i + 1] = peice;
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

        currentState = (GameState)data[0];

        for (int i = 1; i < data.Length; i++)
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
                index = i - 1;
            }
        }
        // each indivdual checks game state, master sends
        StateCheck();
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
        CheckForRoundWinCondition();
    }

    public void NextRoundSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextRound,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });
    }

    public void NextRoundReceive()
    {
        currentState = GameState.Playing;

        UIController.instance.roundOverScreen.SetActive(false);
        UIController.instance.ToggleDisplayLeaderboards("false");

        // Reset scores for all players
        foreach (PlayerInfo player in allPlayers)
        {
            player.Kills = 0;
            player.Deaths = 0;
        }

        // Update stats to others
        UpdateLeaderboard();
        PlayerSpawner.instance.SpawnPlayer();
        SetupTimer();
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

    /// <summary>
    /// Player quit game
    /// </summary>
    public override void OnLeftRoom()
    {
        // run the default functions
        base.OnLeftRoom();

        // then leave after
        SceneManager.LoadScene(mainMenu);
    }

    /// <summary>
    /// When players update their stats check for winners
    /// </summary>
    void CheckForRoundWinCondition()
    {
        bool isWinner = false;

        foreach (PlayerInfo player in allPlayers)
        {
            if (player.Kills > killsToWin && killsToWin > 0)
            {
                isWinner = true;
                // TODO: Save Winner for dispay ?
                break;
            }
        }

        if (isWinner && PhotonNetwork.IsMasterClient && currentState != GameState.Ending)
        {
            currentState = GameState.Ending;
            // update everyone on game state
            ListPlayersSend();
        }

    }

    void StateCheck()
    {
        if (currentState == GameState.Ending)
        {
            EndRound();
        }
    }

    void EndRound()
    {
        currentState = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            // Destroy all Network items
            PhotonNetwork.DestroyAll();
        }

        UIController.instance.roundOverScreen.SetActive(true);
        UIController.instance.ToggleDisplayLeaderboards("true");
        //UIController.instance.timerContainer.SetActive(false);
        UIController.instance.TogglePlayerUI(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // TODO: Cory try to switch to map camera
        //Camera.main.transform.position = mapCameraPoint.position;
        //Camera.main.transform.rotation = mapCameraPoint.rotation;

        StartCoroutine(WaitTimer(waitTimeAfterRound));
    }

    /// <summary>
    /// Wait for a given amount of seconds
    /// </summary>
    /// <param name="seconds">Seconds to wait for</param>
    /// <returns>Nothing</returns>
    private IEnumerator WaitTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!redoRound)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        } else
        {
            // redo round
            if (PhotonNetwork.IsMasterClient)
            {
                // stay on map
                if (!Launcher.instance.changeMapBetweenRounds)
                {
                    NextRoundSend();
                } else
                {
                    int newLevel = UnityEngine.Random.Range(0, Launcher.instance.allMaps.Length);

                    if (Launcher.instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        // load same map
                        NextRoundSend();
                    } else
                    {
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[newLevel]);
                    }
                }
            }
        }        
    }

    public void SetupTimer()
    {
        if (roundLength > 0)
        {
            UIController.instance.timerContainer.SetActive(true);            
            currentRoundTimer = roundLength;
            UpdateRoundTimerDisplay();
        }
    }

    public void UpdateRoundTimerDisplay()
    {
        TimeSpan timeDisplay = System.TimeSpan.FromSeconds(currentRoundTimer);

        UIController.instance.roundCountdownText.text = timeDisplay.ToString("mm\\:ss");
    }

    // Sending time out to all players
    void TimerSend()
    {
        object[] package = new object[] { (int)currentRoundTimer, currentState };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TimerSync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true });
    }

    void TimerReceive(object[] dataReceived)
    {
        currentRoundTimer = (int)dataReceived[0];
        currentState = (GameState)dataReceived[1];

        UpdateRoundTimerDisplay();
    }
}
