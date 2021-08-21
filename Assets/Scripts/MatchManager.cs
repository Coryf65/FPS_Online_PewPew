using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    // singleton
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    [Tooltip("Main Menu Scene index on Build Settings.")]
    public int mainMenu = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(mainMenu);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
