using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    public int totalAmmo;
    public int clipSize;
    public int damage;
    public float fireRate;
    public float range;
    public float reloadingTime;
    public float gunIndex;

    [HideInInspector]
    public int currentAmmo;
    
    void Awake()
    {
        currentAmmo = clipSize;
    }
}
