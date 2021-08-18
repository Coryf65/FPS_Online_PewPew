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

    public GameObject menuButtons;
    public TMP_Text gameTitle;
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;
    public GameObject roomScreen;
    public TMP_Text roomNameText;
    public GameObject errorScreen;
    public TMP_Text errorText;
    public GameObject roomBrowserScreen;
    public RoomButton roomButton;
    
    [SerializeField]
    private const int MAX_PLAYERS_PER_ROOM = 8;
    [SerializeField]
    private List<RoomButton> allRooms = new List<RoomButton>();

    private void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Server";

        PhotonNetwork.ConnectUsingSettings();
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {       
        PhotonNetwork.JoinLobby();
        loadingText.text = "Attempting to Join a Open Lobby";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
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

    public override void OnJoinedRoom()
    {
        CloseMenus();

        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
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
        foreach (RoomButton button in allRooms)
        {
            Destroy(button.gameObject);
        }

        allRooms.Clear();

        roomButton.gameObject.SetActive(false);

        // get all rooms available
        for (int i = 0; i < roomList.Count; i++)
        {
            //// TODO: Cory try to implement, room size maxed out
            //if (roomList[i].PlayerCount == MAX_PLAYERS_PER_ROOM)
            //{
            //    // disabled room, show 8/8 players?
            //}

            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && roomList[i].RemovedFromList)
            {
                RoomButton newRoom = Instantiate(roomButton, roomButton.transform.parent);
                newRoom.SetButtonDetails(roomList[i]);
                newRoom.gameObject.SetActive(true);

                allRooms.Add(newRoom);
            }
        }
    }
}
