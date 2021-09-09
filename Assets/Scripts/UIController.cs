using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Player UI")]
    [Space]
    public GameObject playerUIContainer;
    [Header("Overheating Weapon")]
    public GameObject overheatContainer;
    public TMP_Text overheatMessage;
    public Slider overheatSlider;
    [Header("Match Timer")]
    public GameObject timerContainer;
    public TMP_Text roundCountdownText;
    [Header("Crosshair")]
    public GameObject crosshair;
    [Header("Death Screen")]
    public GameObject deathScreen;
    public TMP_Text deathText;
    [Header("Health UI")]
    public GameObject healthDisplay;
    public Image heartImage;
    public TMP_Text healthAmountText;
    [Header("Scoreboard K/Ds")]
    public GameObject scoreBoard;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    [Header("Leaderboard")]
    public GameObject leaderboard;
    public Leaderboard playerDisplay;
    [Header("Round Over Screen")]
    public GameObject roundOverScreen;
    [Header("Options Screen")]
    public GameObject optionsScreen;
    public Button quitBtn;
    public Button resumeBtn;

    // Start is called before the first frame update
    void Start()
    {
        TogglePlayerUI(true);
        playerUIContainer.SetActive(true);
        healthDisplay.SetActive(true);
        overheatMessage.gameObject.SetActive(false);
        scoreBoard.SetActive(true);
        leaderboard.SetActive(false);
        roundOverScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();

        }

        if (optionsScreen.activeInHierarchy)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// Toggles the Game UI, On and Off
    /// </summary>
    public void ToggleUI()
    {
        if (playerUIContainer.activeSelf)
        {
            playerUIContainer.SetActive(false);
        } else
        {
            playerUIContainer.SetActive(true);
        }
    }

    public void ToggleDisplayLeaderboards(string overrideDisplay = null)
    {

        if (!string.IsNullOrEmpty(overrideDisplay))
        {
            leaderboard.SetActive(true);
            return;
        }

        if (leaderboard.activeInHierarchy)
        {
            leaderboard.SetActive(false);
        }
        else
        {
            leaderboard.SetActive(true);
            //MatchManager.instance.UpdateLeaderboard();
            MatchManager.instance.UpdateLeaderboard();
        }
    }

    public void TogglePlayerUI(bool toggle)
    {
        overheatContainer.SetActive(toggle);
        crosshair.SetActive(toggle);
        timerContainer.SetActive(toggle);
        healthDisplay.SetActive(toggle);
        scoreBoard.SetActive(toggle);
    }

    public void ShowHideOptions()
    {        
        if (!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
            TogglePlayerUI(false);
        } else
        {
            optionsScreen.SetActive(false);
            TogglePlayerUI(true);
        }
    }

    // Return to Main Menu
    public void QuitGame()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }
}
