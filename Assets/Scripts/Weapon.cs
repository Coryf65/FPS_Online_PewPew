using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool isFullAuto;
    // Energy Weapons
    public float timeBetweenShots = .1f;
    public float heatPerShot = 1f;
    public GameObject muzzleFlash;
    public int weaponDamage;
    public float adsZoom;
    public AudioSource shotSound;
}
