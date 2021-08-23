using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    // singleton
    public static Launcher instance;
    private void Awake()
    {
        instance = this;
    }
    [Header("Main Menu Screen")]
    public TMP_Text gameTitle;
    public GameObject menuButtons;
    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    [Header("Room Creation Screen")]
    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;
    [Header("Joined Room Screen")]
    public GameObject roomScreen;
    public TMP_Text roomNameText;
    public TMP_Text playerNameText;
    public GameObject startGameButton;
    [Header("Error Screen")]
    public GameObject errorScreen;
    public TMP_Text errorText;
    [Header("Room Browser Screen")]
    public GameObject roomBrowserScreen;
    public RoomButton roomSelectButton;
    [Header("Player Creation Screen")]
    public GameObject playerCreationScreen;
    public TMP_InputField playerNameInput;
    [Header("Levels")]
    public string levelToLoad;
    [Header("TEST Tools")]
    public GameObject testGameBtn;


    [SerializeField]
    private const int MAX_PLAYERS_PER_ROOM = 8;
    [SerializeField]
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
    [SerializeField]
    private List<TMP_Text> playerNamesInRoom = new List<TMP_Text>();
    private bool isNicknameSet = false;

    private void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Server";

        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        // Only runs in editor
        testGameBtn.SetActive(true);
#endif
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        playerCreationScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {       
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true; // Tells us which scene to load up
        loadingText.text = "Attempting to Join a Open Lobby";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        if (!isNicknameSet)
        {
            CloseMenus();
            playerCreationScreen.SetActive(true);

            if (PlayerPrefs.HasKey("PlayerNickname"))
            {
                playerNameInput.text = PlayerPrefs.GetString("PlayerNickname");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerNickname");
        }
    }

    public void OpenRoomCreationScreen()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        // use name typed in as room, do a room name check
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            // limit the max players in a room
            RoomOptions options = new RoomOptions()
            {
                MaxPlayers = MAX_PLAYERS_PER_ROOM
            };

            PhotonNetwork.CreateRoom(roomNameInput.text);

            CloseMenus();
            loadingText.text = $"Creating Room ... <b>{roomNameInput.text}</b>";
            loadingScreen.SetActive(true);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenus();

        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayersInRoom();

        // Player host, allowed to start game
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        } else
        {
            // Could create a ready up system here
            startGameButton.SetActive(false);
        }
    }

    private void ListAllPlayersInRoom()
    {
        foreach (TMP_Text playerName in playerNamesInRoom)
        {
            Destroy(playerName.gameObject);
        }
        playerNamesInRoom.Clear();

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Player currentPlayer in players)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameText, playerNameText.transform.parent);
            newPlayerLabel.text = currentPlayer.NickName;
            newPlayerLabel.gameObject.SetActive(true);

            playerNamesInRoom.Add(newPlayerLabel);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameText, playerNameText.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        playerNamesInRoom.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayersInRoom();
    }

    /// <summary>
    ///  Room Creation Failed
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = $"Failed to Create Room,  Error:{message}";
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving room...";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        loadingText.text = "Going to rooom browser...";
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        loadingText.text = "Returning to Main Menu...";
        menuButtons.SetActive(true);
    }

    /// <summary>
    /// Anytime the room list changes
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton button in allRoomButtons)
        {
            Destroy(button.gameObject);
        }

        allRoomButtons.Clear();

        roomSelectButton.gameObject.SetActive(false);

        // get all rooms available
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers)
            {
                RoomButton newRoom = Instantiate(roomSelectButton, roomSelectButton.transform.parent);
                newRoom.SetButtonDetails(roomList[i]);
                newRoom.gameObject.SetActive(true);

                allRoomButtons.Add(newRoom);
            }
        }
    }

    /// <summary>
    /// Join a room
    /// </summary>
    /// <param name="info"></param>
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);

        CloseMenus();
        loadingText.text = $"Joining {info.Name} room";
        loadingScreen.SetActive(true);
    }

    public void SetPlayerNickname()
    {
        if (!string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text;

            PlayerPrefs.SetString("PlayerNickname", playerNameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);

            isNicknameSet = true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToLoad);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {

        // Host left Lobby, switch to new player
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }

    public void QuickStartGame()
    {
        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = MAX_PLAYERS_PER_ROOM
        };

        PhotonNetwork.CreateRoom("TEST_ROOM");
        CloseMenus();
        
        loadingText.text = "TEST MODE";
        loadingScreen.SetActive(true);
    }

    /// <summary>
    /// Quit Game to Desktop
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
