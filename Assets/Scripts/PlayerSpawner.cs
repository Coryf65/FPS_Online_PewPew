using System.Collections;
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

    [SerializeField]
    private const float respawnTime = 5f;
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
    public void Died(string damager)
    {       
        UIController.instance.deathText.text = $"You were killed by {damager}";
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        // run a timer to wait
        if (player != null)
        {
            StartCoroutine(DeathCoroutine());
        }
    }

    public IEnumerator DeathCoroutine()
    {
        // spawn effect
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        // Destroy the player
        PhotonNetwork.Destroy(player);
        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);

        SpawnPlayer();
    }
}
