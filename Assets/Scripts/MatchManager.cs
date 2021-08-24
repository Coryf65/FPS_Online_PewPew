using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // singleton
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    public enum Events : byte
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

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(mainMenu);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        //
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
