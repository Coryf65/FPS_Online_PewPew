using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;

    public void SetDetails(string name, int kills, int deaths)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }
}
