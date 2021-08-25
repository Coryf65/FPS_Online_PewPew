using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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
    public TMP_Text overheatMessage;
    public Slider overheatSlider;
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


    // Start is called before the first frame update
    void Start()
    {
        playerUIContainer.SetActive(true);
        healthDisplay.SetActive(true);
        overheatMessage.gameObject.SetActive(false);
        scoreBoard.SetActive(true);
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
}
