using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Player UI")]
    public TMP_Text overheatMessage;
    public Slider overheatSlider;
    [Header("Death Screen")]
    public GameObject deathScreen;
    public TMP_Text deathText;


    // Start is called before the first frame update
    void Start()
    {
        overheatMessage.gameObject.SetActive(false);
    }
}
