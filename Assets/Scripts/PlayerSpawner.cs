﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    // Singleton
    public static PlayerSpawner instance;
    public GameObject deathEffect;

    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;

    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        // make sure we are connected to the network
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        // pick a spawn point
        Transform spawnPoint = SpawnManager.instance.GetRandomSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    ///  Player has died
    /// </summary>
    public void Died()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);
        
        SpawnPlayer();
    }
}
