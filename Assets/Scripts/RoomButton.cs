using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{

    public TMP_Text buttonText;
    public TMP_Text playerCountText;

    private RoomInfo roomInfo;

    public void SetButtonDetails(RoomInfo data)
    {
        roomInfo = data;

        buttonText.text = roomInfo.Name;
        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    public void OpenRoom()
    {
        Debug.Log($"Joining Room {roomInfo}");
        Launcher.instance.JoinRoom(roomInfo);
    }
}
