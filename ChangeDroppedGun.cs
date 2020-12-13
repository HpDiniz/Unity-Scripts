using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDroppedGun : MonoBehaviour
{
    public WeaponStats [] weapons;
    public int currentGunIndex = 1;
    public float nextTimeToFire = 5f;
    //public List<WeaponStats> weapons = new List<WeaponStats>();

    void Start()
    {   
        weapons = GetComponentsInChildren<WeaponStats>();
        //GameObject[] saas = GetComponentsInChildren<GameObject>();
        //Debug.Log("tamanho: " + weapons.Length);
        //Debug.Log("saas: " + saas.Length);
        //weapons[0].gameObject.SetActive(true);

        for (int i = 0; i < weapons.Length; i++)
        {
            if(weapons[i].gunIndex == currentGunIndex){
                weapons[i].gameObject.SetActive(true);
                this.gameObject.name = weapons[i].gunName;
            }else
                weapons[i].gameObject.SetActive(false);
        }
    }

    /*Update is called once per frame
    void Update()
    {
        if(Time.time >= nextTimeToFire){
            nextTimeToFire = Time.time + 5f;
            if(currentGunIndex < 4)
                ChangeWeapons(weapons[currentGunIndex + 1]);
            else
                ChangeWeapons(weapons[0]);
        }
    }*/

    public WeaponStats ChangeWeapons(WeaponStats currentWeapon)
    {   
        WeaponStats newWeapon = null;

        for (int i = 0; i < weapons.Length; i++)
        {
            if(weapons[i].gunIndex == currentGunIndex){
                newWeapon = weapons[i];
                weapons[i].gameObject.SetActive(false);
            }else if(weapons[i].gunIndex == currentWeapon.gunIndex){
                weapons[i].gameObject.SetActive(true);
                this.gameObject.name = weapons[i].gunName;
            }
        }

        return newWeapon;
    }
}
