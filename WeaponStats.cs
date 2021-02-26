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
    public int gunIndex;
    public string gunName;
    public float verticalRecoil;
    public float horizontalRecoil;

    [HideInInspector] public int maxAmmo;
    [HideInInspector] public int currentAmmo;
    
    void Awake()
    {
        maxAmmo = totalAmmo;
        currentAmmo = clipSize;
    }

    public void ResetStats()
    {
        totalAmmo = maxAmmo;
        currentAmmo = clipSize;
    }
}
