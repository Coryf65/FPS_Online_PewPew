using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [Header("Destroys Attached Game Object over lifespan")]
    public float lifeSpan = 1.5f;

    private void Start()
    {
        Destroy(gameObject, lifeSpan);
    }
}
