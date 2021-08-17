using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // creating a singleton
    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Transform[] spawnPointpositions;

    // Start is called before the first frame update
    void Start()
    {
        // set all points to be hidden
        foreach (Transform point in spawnPointpositions)
        {
            point.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Return a valid spawn point position
    /// </summary>
    /// <returns>Transform</returns>
    public Transform GetRandomSpawnPoint()
    {
        return spawnPointpositions[Random.Range(0, spawnPointpositions.Length)];
    }
}
